# ?? Testes Automatizados - Eventos API

## ? Resultado Atual

```
Resumo do teste: total: 15; falhou: 0; bem-sucedido: 15; ignorado: 0
```

Todos os **15 testes passaram com sucesso** ?

---

## ?? Como Executar Testes

### Opção 1: CLI Rápida (Recomendado)

```bash
# Testes básicos
dotnet test

# Com saída detalhada
dotnet test --verbosity detailed

# Apenas um arquivo de teste
dotnet test --filter "FullyQualifiedName~EventoServiceTests"
```

### Opção 2: Scripts Locais

#### PowerShell (Windows)
```powershell
# Executar testes simples
.\testes.ps1

# Com cobertura de código
.\testes.ps1 -Coverage

# Modo verbose
.\testes.ps1 -Verbose

# Watch mode (contínuo)
.\testes.ps1 -Watch

# Combinar opções
.\testes.ps1 -Coverage -Verbose
```

#### Bash (Linux/Mac)
```bash
# Executar testes simples
bash testes.sh

# Com cobertura de código
bash testes.sh --coverage

# Modo verbose
bash testes.sh --verbose

# Watch mode (contínuo)
bash testes.sh --watch

# Combinar opções
bash testes.sh --coverage --verbose
```

### Opção 3: Watch Mode (Desenvolvimento Contínuo)

```bash
dotnet watch test
```

Testes rodarão **automaticamente** sempre que você salvar um arquivo!

### Opção 4: Com Cobertura de Código

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov /p:CoverletOutput=./coverage/
```

Gera relatório em `./coverage/coverage.info`

### Opção 5: Visual Studio

1. Abra **Test Explorer** (`Ctrl + E, T`)
2. Clique em **Run All Tests** ??
3. Visualize resultados em tempo real

---

## ?? Automação Contínua (GitHub Actions)

Testes rodam **automaticamente** em cada push ou pull request!

**Arquivo configurado:** `.github/workflows/testes.yml`

### O que acontece automaticamente:

- ? Testes rodam em Ubuntu Latest
- ? .NET 8 é instalado
- ? Dependências são restauradas
- ? Projeto é buildado
- ? Testes executados
- ? Cobertura de código é medida
- ? Resultados publicados no PR

**Visualizar resultados:** Na aba "Actions" do GitHub

---

## ?? Cobertura de Código

Após rodar testes com cobertura, você pode visualizar com ReportGenerator:

```bash
# Instalar ReportGenerator (se necessário)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Gerar relatório HTML
reportgenerator -reports:"./coverage/coverage.opencover.xml" -targetdir:"./coverage/report" -reporttypes:Html
```

Abra `./coverage/report/index.html` no navegador!

---

## ?? Testes Existentes

### EventoServiceTests (15 testes)

#### ? Sucesso
- `AdicionarConvidado_DeveRegistrarComSucesso_QuandoDadosValidos`
- `AdicionarConvidado_DeveRegistrarComAcompanhantes_QuandoAcompanhadoComNomesValidos`

#### ? Validação de Nome
- `AdicionarConvidado_DeveRetornarBadRequest_QuandoNomeNulo`
- `AdicionarConvidado_DeveRetornarBadRequest_QuandoNomeVazio`
- `AdicionarConvidado_DeveRetornarBadRequest_QuandoNomeMuitoCurto`
- `AdicionarConvidado_DeveRetornarBadRequest_QuandoNomeMuitoLongo`

#### ? Validação de Acompanhantes
- `AdicionarConvidado_DeveRetornarBadRequest_QuandoQuantidadeAcompanhantesMaiorQue5`
- `AdicionarConvidado_DeveRetornarBadRequest_QuandoQuantidadeAcompanhantesNegativa`
- `AdicionarConvidado_DeveRetornarBadRequest_QuandoSozinhoComAcompanhantes`
- `AdicionarConvidado_DeveRetornarBadRequest_QuandoQuantidadeNomesNaoCorresponde`

#### ? Validação de Nomes de Acompanhantes
- `AdicionarConvidado_DeveRetornarBadRequest_QuandoNomeAcompanhanteVazio`
- `AdicionarConvidado_DeveRetornarBadRequest_QuandoNomeAcompanhanteMuitoCurto`
- `AdicionarConvidado_DeveRetornarBadRequest_QuandoNomeAcompanhanteMuitoLongo`

#### ? Erro Genérico
- `AdicionarConvidado_DeveRetornarInternalServerError_QuandoErroGeometrico`

---

## ?? Dicas Importantes

### 1?? **Durante Desenvolvimento**
Use `dotnet watch test` para feedback contínuo

### 2?? **Antes de Commit**
```bash
dotnet test /p:CollectCoverage=true
```

### 3?? **CI/CD Automático**
Testes rodam em GitHub Actions a cada push

### 4?? **Velocidade**
```bash
# Teste apenas um projeto/categoria
dotnet test --filter "FullyQualifiedName~EventoService"
```

---

## ?? Adicionar Novos Testes

Crie um novo arquivo em `Eventos.Tests/Services/` com o padrão:

```csharp
using Xunit;
using NSubstitute;
// ... imports

public class MinhasTesteTests
{
    [Fact]
    public async Task MetodoDeveSerBem_QuandoCondicao()
    {
        // Arrange
        
        // Act
        
        // Assert
    }
}
```

Depois execute:
```bash
dotnet test
```

---

## ?? Troubleshooting

| Problema | Solução |
|----------|---------|
| Testes não encontrados | Execute `dotnet restore` |
| Erro de compilação | `dotnet clean` depois `dotnet build` |
| Database error | Verifique connection string em `appsettings.json` |
| Timeout | Use `dotnet test --logger "console;verbosity=detailed"` |

---

## ?? Recursos

- [xUnit Documentation](https://xunit.net/)
- [NSubstitute Mocking](https://nsubstitute.github.io/)
- [Coverlet Code Coverage](https://github.com/coverlet-coverage/coverlet)
- [GitHub Actions](https://docs.github.com/en/actions)

---

**? Pronto! Seus testes estão 100% automatizados!**
