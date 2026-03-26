import http from 'k6/http';
import { check, sleep, group } from 'k6';
import { Trend, Rate, Counter } from 'k6/metrics';
import { htmlReport } from 'https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js';
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.0.1/index.js';

// -----------------------------------------------------------------------------
// Métricas customizadas por endpoint
// -----------------------------------------------------------------------------
const m = {
    adicionar:      new Trend('dur_adicionar',       true),
    verificar:      new Trend('dur_verificar',       true),
    listar:         new Trend('dur_listar',          true),
    vagas:          new Trend('dur_vagas_restantes', true),
    relatorioExcel: new Trend('dur_relatorio_excel', true),
    relatorioPdf:   new Trend('dur_relatorio_pdf',   true),
    taxaErro:       new Rate('taxa_erro'),
    totalReqs:      new Counter('total_requisicoes'),
};

// -----------------------------------------------------------------------------
// Cenários de carga
//
//  [0s–30s]    smoke  — 1 VU, verifica se a API responde corretamente
//  [40s–130s]  load   — sobe até 20 VUs, simula uso normal sustentado
//  [150s–210s] stress — sobe até 50 VUs, encontra o ponto de pressão
//  [220s–250s] spike  — salta para 100 VUs instantaneamente
//  [260s–560s] soak   — 10 VUs por 5 min, detecta vazamentos de memória
// -----------------------------------------------------------------------------
export const options = {
    scenarios: {

        smoke: {
            executor:  'constant-vus',
            vus:       1,
            duration:  '30s',
            startTime: '0s',
            tags:      { cenario: 'smoke' },
            exec:      'fluxoPadrao',
        },

        load: {
            executor: 'ramping-vus',
            startVUs: 0,
            stages: [
                { duration: '20s', target: 20 },
                { duration: '50s', target: 20 },
                { duration: '20s', target: 0  },
            ],
            startTime:        '40s',
            gracefulRampDown: '5s',
            tags:             { cenario: 'load' },
            exec:             'fluxoPadrao',
        },

        stress: {
            executor: 'ramping-vus',
            startVUs: 0,
            stages: [
                { duration: '15s', target: 50 },
                { duration: '30s', target: 50 },
                { duration: '15s', target: 0  },
            ],
            startTime:        '150s',
            gracefulRampDown: '5s',
            tags:             { cenario: 'stress' },
            exec:             'fluxoPadrao',
        },

        spike: {
            executor: 'ramping-vus',
            startVUs: 0,
            stages: [
                { duration: '5s',  target: 100 },
                { duration: '20s', target: 100 },
                { duration: '5s',  target: 0   },
            ],
            startTime:        '220s',
            gracefulRampDown: '5s',
            tags:             { cenario: 'spike' },
            exec:             'fluxoLeitura',
        },

        soak: {
            executor:  'constant-vus',
            vus:       10,
            duration:  '5m',
            startTime: '260s',
            tags:      { cenario: 'soak' },
            exec:      'fluxoPadrao',
        },
    },

    thresholds: {
        // --- Gerais ---
        'http_req_failed':   ['rate<0.01'],
        'http_req_duration': ['p(95)<2000'],
        'taxa_erro':         ['rate<0.02'],

        // --- Por endpoint ---
        'dur_adicionar':       ['p(95)<800',  'p(99)<1500'],
        'dur_verificar':       ['p(95)<300',  'p(99)<600' ],
        'dur_listar':          ['p(95)<600',  'p(99)<1200'],
        'dur_vagas_restantes': ['p(95)<300',  'p(99)<600' ],
        'dur_relatorio_excel': ['p(95)<3000', 'p(99)<5000'],
        'dur_relatorio_pdf':   ['p(95)<3000', 'p(99)<5000'],

        // --- Por cenário ---
        'http_req_duration{cenario:smoke}':  ['p(95)<500' ],
        'http_req_duration{cenario:load}':   ['p(95)<1500'],
        'http_req_duration{cenario:stress}': ['p(95)<2500'],
        'http_req_duration{cenario:spike}':  ['p(95)<3000'],
        'http_req_duration{cenario:soak}':   ['p(95)<2000'],
    },
};

// -----------------------------------------------------------------------------
// Constantes
// -----------------------------------------------------------------------------
const BASE_URL   = __ENV.BASE_URL || 'http://localhost:8080';
const HEADERS    = { 'Content-Type': 'application/json' };
const PREFIXO_K6 = '[K6] ';

const NOMES_FIXOS = [
    'Alfredo Teste',  'Beatriz Teste',  'Caetano Teste',
    'Daniela Teste',  'Eduardo Teste',  'Fabiana Teste',
    'Gustavo Teste',  'Helena Teste',   'Igor Teste',
    'Juliana Teste',  'Kleber Teste',   'Larissa Teste',
    'Marcelo Teste',  'Natalia Teste',  'Osvaldo Teste',
];

// -----------------------------------------------------------------------------
// Helpers
// -----------------------------------------------------------------------------
function nomeAleatorio(lista) {
    if (!lista || lista.length === 0) return null;
    return lista[Math.floor(Math.random() * lista.length)];
}

function adicionarConvidado(nome, acompanhantes = []) {
    const payload = JSON.stringify({
        nome,
        presencaConfirmada:      true,
        participacao:            acompanhantes.length > 0 ? 'Acompanhado' : 'Sozinho',
        quantidadeAcompanhantes: acompanhantes.length,
        nomesAcompanhantes:      acompanhantes,
    });
    const res = http.post(`${BASE_URL}/api/convidado/adicionar`, payload, {
        headers:          HEADERS,
        responseCallback: http.expectedStatuses(201, 401),
    });
    m.adicionar.add(res.timings.duration);
    m.totalReqs.add(1);
    return res;
}

function verificarConvidado(nome) {
    const res = http.get(
        `${BASE_URL}/api/convidado/verificar?nome=${encodeURIComponent(nome)}`,
        { headers: HEADERS }
    );
    m.verificar.add(res.timings.duration);
    m.totalReqs.add(1);
    return res;
}

function listarConvidados() {
    const res = http.get(`${BASE_URL}/api/convidado/listar`, { headers: HEADERS });
    m.listar.add(res.timings.duration);
    m.totalReqs.add(1);
    return res;
}

function obterVagasRestantes() {
    const res = http.get(`${BASE_URL}/api/convidado/vagas-restantes`, { headers: HEADERS });
    m.vagas.add(res.timings.duration);
    m.totalReqs.add(1);
    return res;
}

function obterRelatorioExcel() {
    const res = http.get(`${BASE_URL}/api/relatorio/excel`, { headers: HEADERS });
    m.relatorioExcel.add(res.timings.duration);
    m.totalReqs.add(1);
    return res;
}

function obterRelatorioPdf() {
    const res = http.get(`${BASE_URL}/api/relatorio/pdf`, { headers: HEADERS });
    m.relatorioPdf.add(res.timings.duration);
    m.totalReqs.add(1);
    return res;
}

// -----------------------------------------------------------------------------
// Setup: insere convidados fixos antes de qualquer cenário rodar
// -----------------------------------------------------------------------------
export function setup() {
    const inseridos = [];

    for (const nome of NOMES_FIXOS) {
        const nomeCompleto = `${PREFIXO_K6}${nome}`;
        const res = adicionarConvidado(nomeCompleto);

        if (res.status === 201) {
            inseridos.push(nomeCompleto);
        } else if (res.status === 401) {
            console.warn(`[setup] Limite atingido após ${inseridos.length} inserções.`);
            break;
        }
    }

    console.log(`[setup] ${inseridos.length} convidado(s) inserido(s).`);
    return { nomesFixos: inseridos };
}

// -----------------------------------------------------------------------------
// Cenário: fluxoPadrao
// Representa o comportamento completo de um usuário real.
// Usado por: smoke, load, stress, soak
// -----------------------------------------------------------------------------
export function fluxoPadrao(data) {
const nomeFixo     = nomeAleatorio(data.nomesFixos);
const nomeDinamico = `${PREFIXO_K6}VU-${__VU}-${__ITER}`;
if (!nomeFixo) { sleep(1); return; }

    group('Escrita — adicionar convidado', () => {
        const res = adicionarConvidado(nomeDinamico);
        const ok = check(res, {
            'adicionar: 201 ou 401': (r) => r.status === 201 || r.status === 401,
        });
        m.taxaErro.add(!ok);
    });

    sleep(0.3);

    group('Leitura — verificar convidado', () => {
        const res = verificarConvidado(nomeFixo);
        const ok = check(res, {
            'verificar: status 200':                          (r) => r.status === 200,
            'verificar: campo existeComoConvidado presente':  (r) => r.json('existeComoConvidado') !== undefined,
            'verificar: campo existeComoAcompanhante presente': (r) => r.json('existeComoAcompanhante') !== undefined,
        });
        m.taxaErro.add(!ok);
    });

    sleep(0.3);

    group('Leitura — listar convidados', () => {
        const res = listarConvidados();
        const ok = check(res, {
            'listar: status 200':    (r) => r.status === 200,
            'listar: retorna array': (r) => Array.isArray(r.json()),
        });
        m.taxaErro.add(!ok);
    });

    sleep(0.3);

    group('Leitura — vagas restantes', () => {
        const res = obterVagasRestantes();
        const ok = check(res, {
            'vagas: status 200':              (r) => r.status === 200,
            'vagas: vagasRestantes >= 0':     (r) => r.json('vagasRestantes') >= 0,
        });
        m.taxaErro.add(!ok);
    });

    sleep(0.5);

    // Relatórios são pesados — executados 1 a cada 5 iterações
    if (__ITER % 5 === 0) {
        group('Relatório — Excel', () => {
            const res = obterRelatorioExcel();
            const ok = check(res, {
                'excel: status 200':           (r) => r.status === 200,
                'excel: content-type correto': (r) =>
                    (r.headers['Content-Type'] || '').includes('spreadsheetml'),
                'excel: body não vazio':       (r) => r.body.length > 0,
            });
            m.taxaErro.add(!ok);
        });

        sleep(0.5);

        group('Relatório — PDF', () => {
            const res = obterRelatorioPdf();
            const ok = check(res, {
                'pdf: status 200':           (r) => r.status === 200,
                'pdf: content-type correto': (r) =>
                    (r.headers['Content-Type'] || '').includes('application/pdf'),
                'pdf: body não vazio':       (r) => r.body.length > 0,
            });
            m.taxaErro.add(!ok);
        });

        sleep(0.5);
    }

    sleep(1);
}

// -----------------------------------------------------------------------------
// Cenário: fluxoLeitura
// Somente endpoints de leitura — usado no spike para não comprometer
// a integridade dos dados durante o pico súbito de usuários.
// Usado por: spike
// -----------------------------------------------------------------------------
export function fluxoLeitura(data) {
const nomeFixo = nomeAleatorio(data.nomesFixos);
if (!nomeFixo) { sleep(1); return; }

    group('Spike — verificar', () => {
        const res = verificarConvidado(nomeFixo);
        const ok = check(res, {
            'spike verificar: status 200': (r) => r.status === 200,
        });
        m.taxaErro.add(!ok);
    });

    sleep(0.2);

    group('Spike — listar', () => {
        const res = listarConvidados();
        const ok = check(res, {
            'spike listar: status 200': (r) => r.status === 200,
        });
        m.taxaErro.add(!ok);
    });

    sleep(0.2);

    group('Spike — vagas', () => {
        const res = obterVagasRestantes();
        const ok = check(res, {
            'spike vagas: status 200':              (r) => r.status === 200,
            'spike vagas: vagasRestantes >= 0':     (r) => r.json('vagasRestantes') >= 0,
        });
        m.taxaErro.add(!ok);
    });

    sleep(0.5);
}

// -----------------------------------------------------------------------------
// Teardown: remove todos os dados inseridos pelo k6
// -----------------------------------------------------------------------------
export function teardown(data) {
    let removidos = 0;

    // Sort longest-first so that shorter names (which can be substrings of longer
    // ones) are only deleted after every longer-named guest is already gone.
    // This avoids HTTP 400 "múltiplos convidados" from the API's ILIKE %name% search.
    const deleteOpts = { headers: HEADERS, responseCallback: http.expectedStatuses(200, 400, 404) };

    const nomesFixosOrdenados = [...data.nomesFixos].sort((a, b) => b.length - a.length);
    for (const nome of nomesFixosOrdenados) {
        const res = http.del(
            `${BASE_URL}/api/convidado/remover?nome=${encodeURIComponent(nome)}`,
            null, deleteOpts
        );
        if (res.status === 200) removidos++;
    }

    const lista = http.get(`${BASE_URL}/api/convidado/listar`, { headers: HEADERS });
    if (lista.status === 200) {
        const k6Guests = lista.json()
            .filter(c => c.nome && c.nome.startsWith(PREFIXO_K6))
            .sort((a, b) => b.nome.length - a.nome.length);
        for (const c of k6Guests) {
            const res = http.del(
                `${BASE_URL}/api/convidado/remover?nome=${encodeURIComponent(c.nome)}`,
                null, deleteOpts
            );
            if (res.status === 200) removidos++;
        }
    }

    console.log(`[teardown] ${removidos} convidado(s) removido(s). Base limpa.`);
}

// -----------------------------------------------------------------------------
// Relatórios gerados ao final da execução
//
//  relatorio.html  — visual, gráficos de latência, throughput e erros
//  relatorio.json  — dados brutos, útil para comparação entre execuções
//  relatorio-junit.xml — formato JUnit lido pelo GitHub Actions como checks
// -----------------------------------------------------------------------------
export function handleSummary(data) {
    return {
        'k6/relatorio.html':       htmlReport(data),
        'k6/relatorio.json':       JSON.stringify(data, null, 2),
        'k6/relatorio-junit.xml':  buildJUnit(data),
        stdout: textSummary(data, { indent: '  ', enableColors: true }),
    };
}

// -----------------------------------------------------------------------------
// Gerador de JUnit XML
// Converte os thresholds do k6 em test cases no formato JUnit,
// permitindo que o GitHub Actions exiba cada threshold como um check
// com ✅ passou / ❌ falhou diretamente na aba Summary do workflow.
// -----------------------------------------------------------------------------
function escapeXml(str) {
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&apos;');
}

function buildJUnit(data) {
    const thresholds = data.metrics
        ? Object.entries(data.metrics).filter(([, v]) => v.thresholds)
        : [];

    let totalTests  = 0;
    let totalFailed = 0;
    let testCases   = '';

    for (const [metricName, metricData] of thresholds) {
        for (const [condition, result] of Object.entries(metricData.thresholds)) {
            totalTests++;
            const passed = result.ok;
            if (!passed) totalFailed++;

            const value = metricData.values
                ? (metricData.values['p(95)'] ?? metricData.values['rate'] ?? metricData.values['value'] ?? 0).toFixed(2)
                : '0';

            testCases += passed
                ? `    <testcase classname="${escapeXml(metricName)}" name="${escapeXml(condition)}" />\n`
                : `    <testcase classname="${escapeXml(metricName)}" name="${escapeXml(condition)}">\n` +
                  `      <failure message="Threshold falhou: ${escapeXml(metricName)} ${escapeXml(condition)} (valor: ${escapeXml(value)})">\n` +
                  `        Métrica: ${escapeXml(metricName)}\n        Condição: ${escapeXml(condition)}\n        Valor medido: ${escapeXml(value)}\n` +
                  `      </failure>\n    </testcase>\n`;
        }
    }

    return (
        `<?xml version="1.0" encoding="UTF-8"?>\n` +
        `<testsuites>\n` +
        `  <testsuite name="k6 Thresholds" tests="${totalTests}" failures="${totalFailed}">\n` +
        testCases +
        `  </testsuite>\n` +
        `</testsuites>\n`
    );
}

