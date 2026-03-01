# ?? Eventos API

API REST para gerenciamento de convidados de eventos, desenvolvida em **.NET 8** seguindo os princípios de **Domain-Driven Design (DDD)**.

---

## ?? Sobre o projeto

A API permite registrar convidados para um evento, informando se irão sozinhos ou acompanhados, a quantidade de acompanhantes e os respectivos nomes. Também é possível verificar se um convidado já está cadastrado na base pelo nome.

---

## ??? Arquitetura

O projeto segue a arquitetura em camadas com separação clara de responsabilidades:

```
Eventos/
??? EventosAPI/              # Camada de apresentação — Controllers e configuração da API
??? Eventos.Application/     # Camada de aplicação — Services, DTOs e interfaces
??? Eventos.Domain/          # Camada de domínio — Entidades e contratos de repositório
??? Eventos.Infrastructure/  # Camada de infraestrutura — EF Core, DbContext e repositórios
??? Eventos.Tests/           # Testes de unidade
```

---

## ?? Endpoints

### `POST /api/convidado/adicionar`

Registra um novo convidado no evento.

**Body:**
```json
{
  "nome": "João Silva",
  "iraAoRodizio": true,
  "participacao": "Sozinho",
  "quantidadeAcompanhantes": 0,
  "nomesAcompanhantes": []
}
```

| Campo | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `nome` | `string` | ? | Nome do convidado (3–50 caracteres) |
| `iraAoRodizio` | `boolean` | ? | Confirmação de presença |
| `participacao` | `string` | ? | `"Sozinho"` ou `"Acompanhado"` |
| `quantidadeAcompanhantes` | `integer` | ? | Quantidade de acompanhantes (0–5) |
| `nomesAcompanhantes` | `string[]` | ? | Nomes dos acompanhantes (deve bater com a quantidade) |

**Respostas:**

| Código | Descrição |
|---|---|
| `201` | Convidado registrado com sucesso |
| `400` | Dados inválidos |
| `500` | Erro interno do servidor |

---

### `GET /api/convidado/verificar?nome={nome}`

Verifica se um convidado já está cadastrado pelo nome (sem distinção de maiúsculas/minúsculas).

**Exemplo:**
```
GET /api/convidado/verificar?nome=João Silva
```

**Resposta `200`:**
```json
{
  "codigoStatus": 200,
  "mensagem": "Consulta realizada com sucesso.",
  "existe": true
}
```

| Código | Descrição |
|---|---|
| `200` | Consulta realizada — `existe` indica se o convidado está cadastrado |
| `400` | Nome não informado |
| `500` | Erro interno do servidor |

---

## ? Regras de negócio

- O nome do convidado deve ter entre **3 e 50 caracteres**
- Convidados com participação `"Sozinho"` **não podem** ter acompanhantes
- A `quantidadeAcompanhantes` deve ser **igual** ao número de nomes em `nomesAcompanhantes`
- Cada nome de acompanhante deve ter entre **3 e 50 caracteres**
- A quantidade de acompanhantes não pode ser **negativa** nem **superior a 5**

---

## ??? Tecnologias

| Tecnologia | Uso |
|---|---|
| .NET 8 | Framework principal |
| ASP.NET Core | Web API |
| Entity Framework Core | ORM |
| SQL Server | Banco de dados |
| xUnit | Testes de unidade |
| NSubstitute | Mocking nos testes |
| Swagger / OpenAPI | Documentação interativa |

---

## ?? Configuração

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server

### String de conexão

Configure a connection string no `appsettings.json` do projeto `EventosAPI`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SEU_SERVIDOR;Database=EventosDb;Trusted_Connection=True;"
  }
}
```

### Aplicar migrations

```bash
dotnet ef database update --project Eventos.Infrastructure --startup-project EventosAPI
```

### Executar a API

```bash
dotnet run --project EventosAPI
```

A documentação interativa estará disponível em:
```
http://localhost:{porta}/swagger
```

---

## ?? Testes

Os testes de unidade estão no projeto `Eventos.Tests` e utilizam mocks para isolar completamente as dependências externas.

```bash
dotnet test Eventos.Tests
```
