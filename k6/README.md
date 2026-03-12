# Testes de Performance com K6

## Pré-requisitos

Instale o K6: https://k6.io/docs/get-started/installation/

```bash
# Windows (via Chocolatey)
choco install k6

# Windows (via winget)
winget install k6
```

---

## Como executar

### Contra a API local

```bash
k6 run k6/performance.js
```

### Contra a API no Render

```bash
k6 run -e BASE_URL=https://sua-api.onrender.com k6/performance.js
```

---

## O que o teste faz

### Setup (antes do teste)
- Insere até 10 convidados fictícios com o prefixo `[K6]` no nome
- Esses nomes nunca colidem com convidados reais

### Durante o teste (60s no total)
| Etapa | Endpoint | Verificação |
|---|---|---|
| 1 | `POST /api/convidado/adicionar` | Status 201 ou 401 (limite) |
| 2 | `GET /api/convidado/verificar` | Status 200, campo `existe` presente |
| 3 | `GET /api/convidado/listar` | Status 200, retorna array |
| 4 | `GET /api/relatorio/excel` | Status 200, content-type correto |

### Teardown (após o teste)
- Remove automaticamente **todos** os convidados com prefixo `[K6]`
- A base fica exatamente como estava antes do teste

---

## Thresholds (critérios de aprovação)

| Métrica | Limite |
|---|---|
| `duracao_adicionar` p95 | < 2000ms |
| `duracao_verificar` p95 | < 500ms |
| `duracao_listar` p95 | < 1000ms |
| `duracao_relatorio` p95 | < 3000ms |
| `taxa_erro` | < 5% |
| `http_req_duration` p95 | < 2000ms |

---

## Gerando relatório HTML

```bash
k6 run k6/performance.js --out json=k6/resultado.json
```

> O arquivo `k6/resultado.json` pode ser visualizado em https://app.k6.io ou importado em ferramentas como Grafana.
