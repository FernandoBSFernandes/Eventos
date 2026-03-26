# ---------------------------------------------------------------------------
# testar-carga.ps1
# Executa os testes de carga localmente, espelhando o workflow carga.yml.
#
# Uso:
#   .\testar-carga.ps1                         # roda performance.js (padrão)
#   .\testar-carga.ps1 load-test.js            # roda load-test.js
#   .\testar-carga.ps1 relatorio-email-test.js
# ---------------------------------------------------------------------------
param(
    [string]$Script = "performance.js"
)

$ComposeFile  = "docker-compose.carga.yml"
$RelatoriosDir = "k6"

function Write-Header($msg) {
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host "  $msg" -ForegroundColor Cyan
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host ""
}

function Cleanup {
    Write-Host ""
    Write-Host "[cleanup] Derrubando containers..." -ForegroundColor Yellow
    docker compose -f $ComposeFile down --remove-orphans --volumes 2>$null
}

Write-Header "Teste de carga local — $Script"

# ---------------------------------------------------------------------------
# Verificação do Docker Desktop
# ---------------------------------------------------------------------------
Write-Host "[0/4] Verificando Docker..." -ForegroundColor Cyan

$dockerOk = $false
try {
    $null = docker info 2>&1
    if ($LASTEXITCODE -eq 0) { $dockerOk = $true }
} catch {}

if (-not $dockerOk) {
    Write-Host "      Docker nao esta em execucao. Tentando abrir o Docker Desktop..." -ForegroundColor Yellow

    $dockerExe = Get-ItemProperty `
        "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*", `
        "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*" `
        -ErrorAction SilentlyContinue |
        Where-Object { $_.DisplayName -like "*Docker Desktop*" } |
        Select-Object -First 1 -ExpandProperty InstallLocation

    if ($dockerExe) {
        $dockerExe = Join-Path $dockerExe "Docker Desktop.exe"
    } else {
        # fallback para o caminho padrão de instalação
        $dockerExe = "C:\Program Files\Docker\Docker\Docker Desktop.exe"
    }

    if (Test-Path $dockerExe) {
        Start-Process $dockerExe
        Write-Host "      Docker Desktop aberto. Aguardando inicializar (ate 60s)..." -ForegroundColor Yellow

        $iniciou = $false
        for ($i = 1; $i -le 30; $i++) {
            Start-Sleep -Seconds 2
            try {
                $null = docker info 2>&1
                if ($LASTEXITCODE -eq 0) { $iniciou = $true; break }
            } catch {}
        }

        if ($iniciou) {
            Write-Host "      Docker disponivel apos $($i * 2)s" -ForegroundColor Green
        } else {
            Write-Host "      ERRO: Docker nao inicializou em 60s. Abra manualmente e tente novamente." -ForegroundColor Red
            Write-Host "Pressione Enter para fechar..." -ForegroundColor DarkGray
            Read-Host
            exit 1
        }
    } else {
        Write-Host "      ERRO: Docker Desktop nao encontrado em '$dockerExe'." -ForegroundColor Red
        Write-Host "      Instale em https://www.docker.com/products/docker-desktop" -ForegroundColor DarkGray
        Write-Host "Pressione Enter para fechar..." -ForegroundColor DarkGray
        Read-Host
        exit 1
    }
} else {
    Write-Host "      Docker em execucao." -ForegroundColor Green
}

try {
    # [1/4] Build e infraestrutura
    Write-Host "[1/4] Buildando imagem da API e subindo infraestrutura..." -ForegroundColor Cyan
    docker compose -f $ComposeFile up --build --wait --no-log-prefix postgres api
    if ($LASTEXITCODE -ne 0) { throw "Falha ao subir containers." }

    # [2/4] Aguarda API
    Write-Host ""
    Write-Host "[2/4] Aguardando API responder em http://localhost:5000..." -ForegroundColor Cyan
    $pronta = $false
    for ($i = 1; $i -le 30; $i++) {
        try {
            $null = Invoke-WebRequest -Uri "http://localhost:5000/api/convidado/listar" -UseBasicParsing -ErrorAction Stop
            Write-Host "      API disponível após $($i * 2)s" -ForegroundColor Green
            $pronta = $true
            break
        } catch {
            Start-Sleep -Seconds 2
        }
    }
    if (-not $pronta) {
        Write-Host "      ERRO: API nao respondeu em 60s." -ForegroundColor Red
        docker compose -f $ComposeFile logs api
        throw "API indisponível."
    }

    # [3/4] Executa k6
    Write-Host ""
    Write-Host "[3/4] Executando k6/$Script..." -ForegroundColor Cyan
    Write-Host ""

    $k6Disponivel = $null -ne (Get-Command k6 -ErrorAction SilentlyContinue)

    if ($k6Disponivel) {
        k6 run "k6/$Script" -e BASE_URL=http://localhost:5000
    } else {
        Write-Host "      (k6 nao encontrado localmente - usando imagem Docker)" -ForegroundColor Yellow
        docker compose -f $ComposeFile `
            --profile carga `
            run --rm `
            -e BASE_URL=http://api:8080 `
            k6 run "/scripts/$Script"
    }

    # [4/4] Relatórios
    Write-Host ""
    Write-Host "[4/4] Relatorios gerados em $RelatoriosDir/:" -ForegroundColor Cyan
    $relatorios = Get-ChildItem -Path $RelatoriosDir -Filter "relatorio*" -ErrorAction SilentlyContinue
    if ($relatorios) {
        $relatorios | ForEach-Object {
            Write-Host ("      {0,-40} {1,8} KB" -f $_.Name, [math]::Round($_.Length / 1KB, 1)) -ForegroundColor White
        }
    } else {
        Write-Host "      (nenhum relatorio encontrado)" -ForegroundColor Yellow
    }

    Write-Header "Concluido com sucesso!"

} catch {
    Write-Host ""
    Write-Host "ERRO: $_" -ForegroundColor Red
    Write-Header "Execucao encerrada com falha."
} finally {
    Cleanup
}

Write-Host "Pressione Enter para fechar..." -ForegroundColor DarkGray
Read-Host
