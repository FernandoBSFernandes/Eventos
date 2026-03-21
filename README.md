# Eventos API

API REST para gerenciamento de convidados de eventos, desenvolvida em **.NET 8** seguindo os princípios de **Domain-Driven Design (DDD)**.

---

## Sobre o projeto

A API permite registrar convidados para um evento, informando se irão sozinhos ou acompanhados, a quantidade de acompanhantes e os respectivos nomes. Também é possível listar todos os convidados, verificar se um convidado já está cadastrado, remover registros duplicados, exportar relatórios de confirmados em PDF ou Excel, enviar os relatórios por e-mail e zerar os dados do evento.

---

## Arquitetura

O projeto segue a arquitetura em camadas com separação clara de responsabilidades:

```
Eventos/
├── EventosAPI/              # Camada de apresentação — Controllers, Reports e configuração da API
├── Eventos.Application/     # Camada de aplicação — Services, DTOs e interfaces
├── Eventos.Domain/          # Camada de domínio — Entidades e contratos de repositório
├── Eventos.Infrastructure/  # Camada de infraestrutura — EF Core, DbContext e repositórios
├── Eventos.Tests/           # Testes de unidade
├── Eventos.IntegrationTests/# Testes de integração
└── k6/                      # Scripts de teste de carga
```

---

## Endpoints

### Convidados — `api/convidado`

#### `POST /api/convidado/adicionar`

Registra um novo convidado no evento.

**Body:**
```json
{
  "nome": "João Silva",
  "presencaConfirmada": true,
  "participacao": "Sozinho",
  "quantidadeAcompanhantes": 0,
  "nomesAcompanhantes": []
}
```

| Campo | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `nome` | `string` | ✔ | Nome do convidado (3–50 caracteres) |
| `presencaConfirmada` | `boolean` | ✔ | Confirmação de presença |
| `participacao` | `string` | ✔ | `"Sozinho"` ou `"Acompanhado"` |
| `quantidadeAcompanhantes` | `integer` | ✔ | Quantidade de acompanhantes (0–5) |
| `nomesAcompanhantes` | `string[]` | ✔ | Nomes dos acompanhantes (deve bater com a quantidade) |

| Código | Descrição |
|---|---|
| `201` | Convidado registrado com sucesso |
| `400` | Dados inválidos ou limite de convidados excedido |
| `500` | Erro interno do servidor |

---

#### `GET /api/convidado/verificar?nome={nome}`

Verifica se um convidado já está cadastrado pelo nome (sem distinção de maiúsculas/minúsculas).

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

#### `GET /api/convidado/listar`

Lista todos os convidados cadastrados com seus respectivos acompanhantes.

**Resposta `200`:**
```json
[
  {
    "nome": "João Silva",
    "presencaConfirmada": true,
    "participacao": "Acompanhado",
    "quantidadeAcompanhantes": 1,
    "nomesAcompanhantes": ["Maria Silva"]
  }
]
```

| Código | Descrição |
|---|---|
| `200` | Lista retornada com sucesso |
| `500` | Erro interno do servidor |

---

#### `DELETE /api/convidado/remover?nome={nome}`

Remove um convidado pelo nome.

| Código | Descrição |
|---|---|
| `200` | Convidado removido com sucesso |
| `400` | Nome não informado ou múltiplos convidados encontrados |
| `404` | Convidado não encontrado |
| `500` | Erro interno do servidor |

---

#### `GET /api/convidado/vagas-restantes`

Retorna o número de vagas disponíveis com base no limite configurado.

| Código | Descrição |
|---|---|
| `200` | Consulta realizada com sucesso |
| `500` | Erro interno do servidor |

---

### Relatórios — `api/relatorio`

#### `GET /api/relatorio/excel`

Exporta o relatório de convidados confirmados em formato **Excel** (`.xlsx`).

| Código | Descrição |
|---|---|
| `200` | Arquivo gerado e retornado |
| `500` | Erro interno do servidor |

---

#### `GET /api/relatorio/pdf`

Exporta o relatório de convidados confirmados em formato **PDF**.

| Código | Descrição |
|---|---|
| `200` | Arquivo gerado e retornado |
| `500` | Erro interno do servidor |

---

#### `POST /api/relatorio/enviar-email`

Envia o relatório de convidados confirmados por e-mail com os arquivos PDF e Excel em anexo.

| Código | Descrição |
|---|---|
| `200` | E-mail enviado com sucesso |
| `500` | Erro interno do servidor |

---

### Administração — `api/administracao`

#### `DELETE /api/administracao/remover-duplicatas`

Remove registros duplicados de convidados e acompanhantes. O critério de duplicidade é o nome idêntico após normalização (sem distinção de maiúsculas/minúsculas). O **primeiro** registro de cada grupo é preservado.

**Resposta `200`:**
```json
{
  "codigoStatus": 200,
  "mensagem": "Duplicatas removidas com sucesso. Convidados removidos: 2. Acompanhantes removidos: 4."
}
```

| Código | Descrição |
|---|---|
| `200` | Duplicatas removidas com sucesso |
| `500` | Erro interno do servidor |

---

#### `DELETE /api/administracao/zerar-tabelas`

Zera todos os registros de convidados e acompanhantes do banco de dados.

| Código | Descrição |
|---|---|
| `200` | Dados removidos com sucesso |
| `500` | Erro interno do servidor |

---

## Regras de negócio

- O nome do convidado deve ter entre **3 e 50 caracteres**
- Convidados com participação `"Sozinho"` **não podem** ter acompanhantes
- A `quantidadeAcompanhantes` deve ser **igual** ao número de nomes em `nomesAcompanhantes`
- Cada nome de acompanhante deve ter entre **3 e 50 caracteres**
- A quantidade de acompanhantes não pode ser **negativa** nem **superior a 5**
- O total de pessoas no evento (convidados + acompanhantes) não pode ultrapassar o **limite configurado** (padrão: 100)

---

## Tecnologias

| Tecnologia | Uso |
|---|---|
| .NET 8 | Framework principal |
| ASP.NET Core | Web API |
| Entity Framework Core | ORM |
| PostgreSQL (Npgsql) | Banco de dados |
| ClosedXML | Geração de Excel |
| QuestPDF | Geração de PDF |
| xUnit | Testes de unidade |
| NSubstitute | Mocking nos testes |
| Swagger / OpenAPI | Documentação interativa |
| k6 | Testes de carga |
| Docker | Containerização |

---

## Configuração

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/)
- [Docker](https://www.docker.com/) *(opcional)*
- [k6](https://k6.io/docs/get-started/installation/) *(opcional, para testes de carga)*

### String de conexão

Configure a connection string no `appsettings.json` do projeto `EventosAPI`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=SEU_HOST;Database=EventosDb;Username=SEU_USUARIO;Password=SUA_SENHA"
  },
  "Evento": {
    "LimiteMaximoPessoas": 100
  },
  "Email": {
    "SmtpHost": "smtp.office365.com",
    "SmtpPort": 587,
    "Remetente": "seu@email.com",
    "Senha": "<senha>",
    "Destinatario": "destino@email.com"
  },
  "Swagger": {
    "Enabled": true
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
http://localhost:8080/swagger
```

---

## Testes

### Unitários

Os testes de unidade estão no projeto `Eventos.Tests` e utilizam mocks para isolar completamente as dependências externas:

```
Eventos.Tests/
└── Services/
    ├── AdicionarConvidadoTests.cs
    ├── VerificarConvidadoTests.cs
    ├── ListarConvidadosTests.cs
    ├── RemoverConvidadoPorNomeTests.cs
    ├── ObterVagasRestantesTests.cs
    ├── RemoverDuplicatasTests.cs
    ├── ZerarTabelasTests.cs
    └── ObterRelatorioTests.cs
```

```bash
dotnet test Eventos.Tests
```

### Integração

Os testes de integração estão no projeto `Eventos.IntegrationTests` e utilizam `WebApplicationFactory` para testar os controllers de ponta a ponta:

```
Eventos.IntegrationTests/
└── Controllers/
    ├── ConvidadoControllerTests.cs
    ├── AdministracaoControllerTests.cs
    └── RelatorioEmailIntegrationTests.cs
```

```bash
dotnet test Eventos.IntegrationTests
```

### Executar todos os testes

```bash
dotnet test
```

---

## Testes de Carga (k6)

Os scripts estão na pasta `k6/`:

```bash
# Teste de carga com cenários de aquecimento e pico
k6 run k6/load-test.js -e BASE_URL=http://localhost:8080

# Teste de performance individual
k6 run k6/performance.js -e BASE_URL=http://localhost:8080
```

**Thresholds configurados:**

| Métrica | Limite |
|---|---|
| Taxa de erros | < 1% |
| Latência p(95) geral | < 500 ms |
| Latência p(95) — listar | < 400 ms |
| Latência p(95) — verificar | < 300 ms |
| Latência p(95) — adicionar | < 600 ms |

---

## Docker

```bash
# Build da imagem
docker build -f EventosAPI/Dockerfile -t eventos-api .

# Executar o container
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=SEU_HOST;Database=EventosDb;Username=SEU_USUARIO;Password=SUA_SENHA" \
  eventos-api
