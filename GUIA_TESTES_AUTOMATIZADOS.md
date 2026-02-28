# ?? Guia de Testes Automatizados - Eventos API

## Opção 1: Executar Testes via CLI (Recomendado)

### 1.1 Executar todos os testes
```bash
dotnet test
```

### 1.2 Executar testes com saída verbosa
```bash
dotnet test --verbosity detailed
```

### 1.3 Executar testes com cobertura de código
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### 1.4 Executar testes específicos
```bash
dotnet test --filter "FullyQualifiedName~EventoServiceTests"
```

---

## Opção 2: Integração com GitHub Actions (CI/CD Automatizado)

Crie o arquivo `.github/workflows/testes.yml`:

```yaml
name: Testes Automatizados

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  test:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET 8
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restaurar dependências
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Executar testes
      run: dotnet test --no-build --verbosity normal --logger "trx;LogFileName=test-results.trx"
    
    - name: Publicar resultados de teste
      uses: EnricoMi/publish-unit-test-result-action@v2
      if: always()
      with:
        files: '**/test-results.trx'
```

---

## Opção 3: Automação Local com Scripts

### 3.1 Script PowerShell (testes.ps1)
```powershell
# Script para executar testes com relatório
Write-Host "?? Iniciando testes..." -ForegroundColor Cyan

# Executar testes com cobertura
dotnet test `
  /p:CollectCoverage=true `
  /p:CoverletOutputFormat=lcov `
  /p:CoverletOutput=./coverage/ `
  --logger "console;verbosity=detailed" `
  --logger "trx;LogFileName=TestResults.trx"

if ($LASTEXITCODE -eq 0) {
    Write-Host "? Testes executados com sucesso!" -ForegroundColor Green
} else {
    Write-Host "? Testes falharam!" -ForegroundColor Red
    exit 1
}
```

### 3.2 Script Bash (testes.sh)
```bash
#!/bin/bash

echo "?? Iniciando testes..."

dotnet test \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=lcov \
  /p:CoverletOutput=./coverage/ \
  --logger "console;verbosity=detailed" \
  --logger "trx;LogFileName=TestResults.trx"

if [ $? -eq 0 ]; then
    echo "? Testes executados com sucesso!"
else
    echo "? Testes falharam!"
    exit 1
fi
```

---

## Opção 4: Watch Mode (Testes Contínuos)

Executa testes automaticamente ao salvar um arquivo:

```bash
dotnet watch test
```

---

## Opção 5: Configurar no Visual Studio

1. Abra **Test Explorer** (`Ctrl + E, T`)
2. Clique em **Run All Tests** para executar todos os testes
3. Configure para rodar automaticamente ao compilar:
   - Tools > Options > Test Adapter for xUnit > Check "Run tests after build"

---

## Opção 6: Integração com Azure Pipelines

Crie o arquivo `azure-pipelines.yml`:

```yaml
trigger:
  - main
  - develop

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'

steps:
- task: UseDotNet@2
  inputs:
    version: '8.0.x'

- task: DotNetCoreCLI@2
  displayName: 'Restaurar dependências'
  inputs:
    command: 'restore'

- task: DotNetCoreCLI@2
  displayName: 'Build'
  inputs:
    command: 'build'
    arguments: '--configuration $(buildConfiguration)'

- task: DotNetCoreCLI@2
  displayName: 'Executar testes'
  inputs:
    command: 'test'
    arguments: '--configuration $(buildConfiguration) --logger trx --collect:"XPlat Code Coverage"'

- task: PublishTestResults@2
  displayName: 'Publicar resultados de teste'
  inputs:
    testResultsFormat: 'VSTest'
    testResultsFiles: '**/*.trx'
```

---

## Resumo Rápido

| Método | Comando | Quando usar |
|--------|---------|------------|
| **CLI Básico** | `dotnet test` | Verificação rápida |
| **Com Cobertura** | `dotnet test /p:CollectCoverage=true` | Análise de qualidade |
| **Watch Mode** | `dotnet watch test` | Desenvolvimento contínuo |
| **GitHub Actions** | Commit + Push | Automação em produção |
| **Visual Studio** | Test Explorer | Desenvolvimento no IDE |

---

## ?? Recomendação para seu projeto

Para automação completa **sem interação**, implemente:

1. **GitHub Actions** para CI/CD (recomendado)
2. **Watch Mode** durante desenvolvimento
3. **Cobertura de código** regularmente

Isso garante que testes rodem automaticamente em cada push!
