import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend, Rate, Counter } from 'k6/metrics';

// ─── Métricas customizadas ────────────────────────────────────────────────────
const duracao_adicionar      = new Trend('duracao_adicionar',      true);
const duracao_verificar      = new Trend('duracao_verificar',      true);
const duracao_listar         = new Trend('duracao_listar',         true);
const duracao_relatorio      = new Trend('duracao_relatorio',      true);
const duracao_vagas_restantes = new Trend('duracao_vagas_restantes', true);

const taxa_erro          = new Rate('taxa_erro');
const total_requisicoes  = new Counter('total_requisicoes');

// ─── Configuração do teste ────────────────────────────────────────────────────
export const options = {
    stages: [
        { duration: '15s', target: 5  },  // rampa de subida
        { duration: '30s', target: 10 },  // carga sustentada
        { duration: '15s', target: 0  },  // rampa de descida
    ],
    thresholds: {
        // 95% das requisições devem responder em menos de 2s
        'duracao_adicionar':       ['p(95)<2000'],
        'duracao_verificar':       ['p(95)<500'],
        'duracao_listar':          ['p(95)<1000'],
        'duracao_relatorio':       ['p(95)<3000'],
        'duracao_vagas_restantes': ['p(95)<500'],
        // Taxa de erro abaixo de 5%
        'taxa_erro':               ['rate<0.05'],
        // Tempo de resposta geral
        'http_req_duration':       ['p(95)<2000'],
    },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';
const HEADERS  = { 'Content-Type': 'application/json' };

// Prefixo que identifica dados de teste — nunca usados em produção real
const PREFIXO_TESTE = '[K6] ';

// Pool de nomes fictícios usados nos testes
const NOMES_TESTE = [
    'Alfredo Teste',
    'Beatriz Teste',
    'Caetano Teste',
    'Daniela Teste',
    'Eduardo Teste',
    'Fabiana Teste',
    'Gustavo Teste',
    'Helena Teste',
    'Igor Teste',
    'Juliana Teste',
];

// ─── Setup: insere convidados de teste antes do teste iniciar ─────────────────
export function setup() {
    const inseridos = [];

    for (const nome of NOMES_TESTE) {
        const nomeCompleto = `${PREFIXO_TESTE}${nome}`;
        const payload = JSON.stringify({
            nome:                  nomeCompleto,
            iraAoRodizio:          true,
            participacao:          'Sozinho',
            quantidadeAcompanhantes: 0,
            nomesAcompanhantes:    [],
        });

        const res = http.post(`${BASE_URL}/api/convidado/adicionar`, payload, { headers: HEADERS });

        // Aceita 201 (criado) ou 401 (limite atingido — encerra o setup)
        if (res.status === 201) {
            inseridos.push(nomeCompleto);
        } else if (res.status === 401) {
            console.warn(`[setup] Limite de 100 pessoas atingido após ${inseridos.length} inserções.`);
            break;
        } else {
            console.error(`[setup] Falha ao inserir "${nomeCompleto}": HTTP ${res.status} — ${res.body}`);
        }
    }

    console.log(`[setup] ${inseridos.length} convidado(s) de teste inserido(s).`);
    return { nomesInseridos: inseridos };
}

// ─── Cenário principal ────────────────────────────────────────────────────────
export default function (data) {
    const nomeAleatorio = data.nomesInseridos[Math.floor(Math.random() * data.nomesInseridos.length)];

    // 1. Adicionar convidado com acompanhante
    {
        const payload = JSON.stringify({
            nome:                    `${PREFIXO_TESTE}Convidado VU-${__VU}-${__ITER}`,
            iraAoRodizio:            true,
            participacao:            'Acompanhado',
            quantidadeAcompanhantes: 1,
            nomesAcompanhantes:      [`${PREFIXO_TESTE}Acomp VU-${__VU}-${__ITER}`],
        });

        const res = http.post(`${BASE_URL}/api/convidado/adicionar`, payload, { headers: HEADERS });
        duracao_adicionar.add(res.timings.duration);
        total_requisicoes.add(1);

        const ok = check(res, {
            'adicionar: status 201 ou 401 (limite)': (r) => r.status === 201 || r.status === 401,
        });
        taxa_erro.add(!ok);
    }

    sleep(0.5);

    // 2. Verificar convidado existente
    {
        const res = http.get(
            `${BASE_URL}/api/convidado/verificar?nome=${encodeURIComponent(nomeAleatorio)}`,
            { headers: HEADERS }
        );
        duracao_verificar.add(res.timings.duration);
        total_requisicoes.add(1);

        const ok = check(res, {
            'verificar: status 200': (r) => r.status === 200,
            'verificar: campo existe presente': (r) => JSON.parse(r.body).existe !== undefined,
        });
        taxa_erro.add(!ok);
    }

    sleep(0.3);

    // 3. Listar convidados
    {
        const res = http.get(`${BASE_URL}/api/convidado/listar`, { headers: HEADERS });
        duracao_listar.add(res.timings.duration);
        total_requisicoes.add(1);

        const ok = check(res, {
            'listar: status 200': (r) => r.status === 200,
            'listar: retorna array': (r) => Array.isArray(JSON.parse(r.body)),
        });
        taxa_erro.add(!ok);
    }

    sleep(0.3);

    // 4. Relatório Excel
    {
        const res = http.get(`${BASE_URL}/api/relatorio/excel`, { headers: HEADERS });
        duracao_relatorio.add(res.timings.duration);
        total_requisicoes.add(1);

        const ok = check(res, {
            'relatorio/excel: status 200': (r) => r.status === 200,
            'relatorio/excel: content-type xlsx': (r) =>
                r.headers['Content-Type'].includes('spreadsheetml'),
        });
        taxa_erro.add(!ok);
    }

    sleep(0.3);

    // 5. Vagas restantes
    {
        const res = http.get(`${BASE_URL}/api/convidado/vagas-restantes`, { headers: HEADERS });
        duracao_vagas_restantes.add(res.timings.duration);
        total_requisicoes.add(1);

        const ok = check(res, {
            'vagas-restantes: status 200': (r) => r.status === 200,
            'vagas-restantes: campo vagasRestantes presente': (r) => JSON.parse(r.body).vagasRestantes !== undefined,
            'vagas-restantes: vagasRestantes nao negativo': (r) => JSON.parse(r.body).vagasRestantes >= 0,
        });
        taxa_erro.add(!ok);
    }

    sleep(1);
}

// ─── Teardown: remove todos os convidados de teste inseridos ──────────────────
export function teardown(data) {
    let removidos = 0;

    for (const nome of data.nomesInseridos) {
        const res = http.del(
            `${BASE_URL}/api/convidado/remover?nome=${encodeURIComponent(nome)}`,
            null,
            { headers: HEADERS }
        );

        if (res.status === 200) {
            removidos++;
        } else {
            console.warn(`[teardown] Não foi possível remover "${nome}": HTTP ${res.status}`);
        }
    }

    // Remove também os convidados criados dinamicamente durante o teste
    const listaRes = http.get(`${BASE_URL}/api/convidado/listar`, { headers: HEADERS });
    if (listaRes.status === 200) {
        const todos = JSON.parse(listaRes.body);
        const deTeste = todos.filter((c) => c.nome.startsWith(PREFIXO_TESTE));

        for (const convidado of deTeste) {
            const res = http.del(
                `${BASE_URL}/api/convidado/remover?nome=${encodeURIComponent(convidado.nome)}`,
                null,
                { headers: HEADERS }
            );
            if (res.status === 200) removidos++;
        }
    }

    console.log(`[teardown] ${removidos} convidado(s) de teste removido(s). Base limpa.`);
}

