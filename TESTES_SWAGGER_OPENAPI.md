# ?? Testes de Swagger/OpenAPI

Testes automatizados para validar a documentação e especificação da API.

## ?? Tipos de Testes

### 1. SwaggerDocumentationTests
Valida os atributos e metadados dos controllers e endpoints.

**O que testa:**
- ? Controlador existe e herda de `ControllerBase`
- ? Atributos `[Route]` e `[ApiController]` presentes
- ? Métodos públicos async
- ? Retorno correto (`IActionResult`)
- ? Atributos HTTP (`[HttpPost]`)
- ? Resposta esperada (`[ProducesResponseType]`)
- ? Documentação XML (`/// <summary>`)
- ? Parâmetros com `[FromBody]`

**Exemplos:**
```csharp
[Fact]
public void ConvidadoController_DeveTermoAtributoRoute() { ... }

[Fact]
public void AdicionarConvidado_DeveTermoProducesResponseType201() { ... }
```

### 2. ConvidadoControllerIntegrationTests
Testa a API real com `WebApplicationFactory`.

**O que testa:**
- ? Endpoint retorna 201 Created com dados válidos
- ? Endpoint retorna 400 Bad Request com dados inválidos
- ? Content-Type correto (application/json)
- ? Headers adequados
- ? Métodos HTTP corretos
- ? Swagger está disponível

**Exemplos:**
```csharp
[Fact]
public async Task Post_AdicionarConvidado_DeveRetornar201_QuandoDadosValidos() { ... }

[Fact]
public async Task Api_DeveTerSwaggerDisponivel() { ... }
```

### 3. OpenApiSpecificationTests
Valida a especificação OpenAPI/Swagger gerada.

**O que testa:**
- ? JSON do Swagger é válido
- ? Informações da API presentes
- ? Paths documentados
- ? Endpoints aparecem no Swagger
- ? Métodos HTTP corretos
- ? Respostas documentadas (201, 400, 500)
- ? Request body definido
- ? Schemas presentes
- ? Swagger UI acessível

**Exemplos:**
```csharp
[Fact]
public async Task SwaggerJson_DeveEstarDisponivel() { ... }

[Fact]
public async Task SwaggerJson_EndpointAdicionarConvidado_DeveTermoRespostas() { ... }
```

---

## ?? Como Executar

### Testes de Swagger apenas
```bash
dotnet test --filter "Swagger"
```

### Testes de Integração apenas
```bash
dotnet test --filter "IntegrationTests"
```

### Todos os testes com coverage
```bash
dotnet test /p:CollectCoverage=true
```

### Modo watch
```bash
dotnet watch test
```

---

## ?? Resultado Esperado

```
? SwaggerDocumentationTests (14 testes)
? ConvidadoControllerIntegrationTests (13 testes)
? OpenApiSpecificationTests (10 testes)
? Total: 37 testes de Swagger/OpenAPI
```

---

## ?? Benefícios

### 1. **Documentação Sempre Atualizada**
- Testes falham se documentação ficar desatualizada
- Force sincronização entre código e documentação

### 2. **Contrato da API Validado**
- Garante que respostas 201, 400, 500 estão documentadas
- Valida estrutura do JSON

### 3. **Swagger Sempre Válido**
- Especificação OpenAPI sempre gerável
- Swagger UI sempre funcional

### 4. **Detecção de Regressão**
- Se alguém remover `[ProducesResponseType]`, teste falha
- Se alguém mudar a rota, teste falha

---

## ?? Exemplo de Teste em Detalhes

### SwaggerDocumentationTests

```csharp
[Fact]
public void AdicionarConvidado_DeveTermoProducesResponseType201()
{
    // Arrange & Act
    var method = typeof(ConvidadoController).GetMethod("AdicionarConvidado");
    var producesAttributes = method.GetCustomAttributes<ProducesResponseTypeAttribute>();

    // Assert
    Assert.NotEmpty(producesAttributes);
    Assert.Contains(producesAttributes, attr => attr.StatusCode == StatusCodes.Status201Created);
}
```

**O que valida:**
- Método `AdicionarConvidado` existe
- Tem atributo `[ProducesResponseType(201)]`
- Isso garante que Swagger documenta resposta 201

---

### ConvidadoControllerIntegrationTests

```csharp
[Fact]
public async Task Post_AdicionarConvidado_DeveRetornar201_QuandoDadosValidos()
{
    // Arrange
    var request = new AdicionarConvidadoRequest(...);
    var json = JsonSerializer.Serialize(request);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    // Act
    var response = await _client.PostAsync($"{BaseUrl}/adicionar", content);

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    var result = JsonSerializer.Deserialize<BaseResponse>(responseContent);
    Assert.Equal(201, result.CodigoStatus);
}
```

**O que valida:**
- API real retorna 201 Created
- Response é JSON válido
- Mensagem faz sentido

---

### OpenApiSpecificationTests

```csharp
[Fact]
public async Task SwaggerJson_EndpointAdicionarConvidado_DeveTermoRespostas()
{
    // Act
    var response = await _client.GetAsync("/swagger/v1/swagger.json");
    var document = JsonSerializer.Deserialize<JsonElement>(content);
    
    // Parse do JSON...
    
    // Assert
    Assert.True(responses.TryGetProperty("201", out _), "Resposta 201 não documentada");
    Assert.True(responses.TryGetProperty("400", out _), "Resposta 400 não documentada");
}
```

**O que valida:**
- Swagger JSON contém documentação de 201 e 400
- Especificação está correta

---

## ?? Adicionando Novos Testes de Swagger

### Para um novo endpoint

```csharp
// 1. SwaggerDocumentationTests
[Fact]
public void NovoMetodo_DeveTermoAtributoHttpGet()
{
    var method = typeof(ConvidadoController).GetMethod("NovoMetodo");
    var httpGetAttribute = method.GetCustomAttribute<HttpGetAttribute>();
    Assert.NotNull(httpGetAttribute);
}

// 2. ConvidadoControllerIntegrationTests
[Fact]
public async Task Get_NovoMetodo_DeveRetornar200()
{
    var response = await _client.GetAsync("api/convidado/novo");
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}

// 3. OpenApiSpecificationTests
[Fact]
public async Task SwaggerJson_DeveConterEndpointNovoMetodo()
{
    // Verificar se endpoint está no Swagger JSON
}
```

---

## ?? Troubleshooting

| Problema | Solução |
|----------|---------|
| "WebApplicationFactory" não encontrado | Adicionar `using Microsoft.AspNetCore.Mvc.Testing` |
| Testes de integração lentos | Normal! Testes de integração criam toda a app |
| Swagger JSON não encontrado | Verificar se Swagger está configurado em `Program.cs` |
| Port já em uso | WebApplicationFactory usa porta aleatória |

---

## ?? Integração com CI/CD

Os testes são executados automaticamente via GitHub Actions:

```yaml
- name: ?? Executar testes
  run: dotnet test --verbosity normal
```

Se algum teste falhar, o pipeline bloqueia o merge! ?

---

## ?? Recursos

- [Swagger/OpenAPI](https://swagger.io/specification/)
- [ASP.NET Core Swagger](https://docs.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger)
- [xUnit Testing](https://xunit.net/)
- [WebApplicationFactory](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.testing.webapplicationfactory-1)

---

**? Seus testes de Swagger estão 100% automatizados!**
