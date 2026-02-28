# ?? Testes Automatizados - Eventos API

## ? Resultado Atual

```
Resumo do teste: total: 15; falhou: 0; bem-sucedido: 15; ignorado: 0
```

Todos os **15 testes passaram com sucesso** ?

---

## ?? Como Executar Testes

### Op��o 1: CLI R�pida (Recomendado)

```bash
# Testes b�sicos
dotnet test

# Com sa�da detalhada
dotnet test --verbosity detailed

# Apenas um arquivo de teste
dotnet test --filter "FullyQualifiedName~EventoServiceTests"
```

### Op��o 2: Scripts Locais

#### PowerShell (Windows)
```powershell
# Executar testes simples
.\testes.ps1

# Com cobertura de c�digo
.\testes.ps1 -Coverage

# Modo verbose
.\testes.ps1 -Verbose

# Watch mode (cont�nuo)
.\testes.ps1 -Watch

# Combinar op��es
.\testes.ps1 -Coverage -Verbose
```

#### Bash (Linux/Mac)
```bash
# Executar testes simples
bash testes.sh

# Com cobertura de c�digo
bash testes.sh --coverage

# Modo verbose
bash testes.sh --verbose

# Watch mode (cont�nuo)
bash testes.sh --watch

# Combinar op��es
bash testes.sh --coverage --verbose
```

### Op��o 3: Watch Mode (Desenvolvimento Cont�nuo)

```bash
dotnet watch test
```

Testes rodar�o **automaticamente** sempre que voc� salvar um arquivo!

### Op��o 4: Com Cobertura de C�digo

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov /p:CoverletOutput=./coverage/
```

Gera relat�rio em `./coverage/coverage.info`

### Op��o 5: Visual Studio

1. Abra **Test Explorer** (`Ctrl + E, T`)
2. Clique em **Run All Tests** ??
3. Visualize resultados em tempo real

---

## ?? Automa��o Cont�nua (GitHub Actions)

Testes rodam **automaticamente** em cada push ou pull request!

**Arquivo configurado:** `.github/workflows/testes.yml`

### O que acontece automaticamente:

- ? Testes rodam em Ubuntu Latest
- ? .NET 8 � instalado
- ? Depend�ncias s�o restauradas
- ? Projeto � buildado
- ? Testes executados
- ? Cobertura de c�digo � medida
- ? Resultados publicados no PR

**Visualizar resultados:** Na aba "Actions" do GitHub

---

## ?? Cobertura de C�digo

Ap�s rodar testes com cobertura, voc� pode visualizar com ReportGenerator:

```bash
# Instalar ReportGenerator (se necess�rio)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Gerar relat�rio HTML
reportgenerator -reports:"./coverage/coverage.opencover.xml" -targetdir:"./coverage/report" -reporttypes:Html
```

Abra `./coverage/report/index.html` no navegador!

---

## ?? Testes Existentes

### EventoServiceTests (15 testes)

#### ? Sucesso
- `AdicionarConvidado_DeveRegistrarComSucesso_QuandoDadosValidos`
- `AdicionarConvidado_DeveRegistrarComAcompanhantes_QuandoAcompanhadoComNomesValidos`

#### ? Valida��o de Nome
- `AdicionarConvidado_DeveRetornarBadRequest_QuandoNomeNulo`
- `AdicionarConvidado_DeveRetornarBadRequest_QuandoNomeVazio`
- `AdicionarConvidado_DeveRetornarBadRequest_QuandoNomeMuitoCurto`
- `AdicionarConvidado_DeveRetornarBadRequest_QuandoNomeMuitoLongo`

#### ? Valida��o de Acompanhantes
- `AdicionarConvidado_DeveRetornarBadRequest_QuandoQuantidadeAcompanhantesMaiorQue5`
- `AdicionarConvidado_DeveRetornarBadRequest_QuandoQuantidadeAcompanhantesNegativa`
- `AdicionarConvidado_DeveRetornarBadRequest_QuandoSozinhoComAcompanhantes`
- `AdicionarConvidado_DeveRetornarBadRequest_QuandoQuantidadeNomesNaoCorresponde`

#### ? Valida��o de Nomes de Acompanhantes
- `AdicionarConvidado_DeveRetornarBadRequest_QuandoNomeAcompanhanteVazio`
- `AdicionarConvidado_DeveRetornarBadRequest_QuandoNomeAcompanhanteMuitoCurto`
- `AdicionarConvidado_DeveRetornarBadRequest_QuandoNomeAcompanhanteMuitoLongo`

#### ? Erro Gen�rico
- `AdicionarConvidado_DeveRetornarInternalServerError_QuandoErroGeometrico`

---

## ?? Dicas Importantes

### 1?? **Durante Desenvolvimento**
Use `dotnet watch test` para feedback cont�nuo

### 2?? **Antes de Commit**
```bash
dotnet test /p:CollectCoverage=true
```

### 3?? **CI/CD Autom�tico**
Testes rodam em GitHub Actions a cada push

### 4?? **Velocidade**
```bash
# Teste apenas um projeto/categoria
dotnet test --filter "FullyQualifiedName~EventoService"
```

---

## ?? Adicionar Novos Testes

Crie um novo arquivo em `Eventos.Tests/Services/` com o padr�o:

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

| Problema | Solu��o |
|----------|---------|
| Testes n�o encontrados | Execute `dotnet restore` |
| Erro de compila��o | `dotnet clean` depois `dotnet build` |
| Database error | Verifique connection string em `appsettings.json` |
| Timeout | Use `dotnet test --logger "console;verbosity=detailed"` |

---

## ?? Recursos

- [xUnit Documentation](https://xunit.net/)
- [NSubstitute Mocking](https://nsubstitute.github.io/)
- [Coverlet Code Coverage](https://github.com/coverlet-coverage/coverlet)
- [GitHub Actions](https://docs.github.com/en/actions)

---

**? Pronto! Seus testes est�o 100% automatizados!**
