import http from 'k6/http';
import { check, sleep, group } from 'k6';
import { Trend, Rate, Counter } from 'k6/metrics';
import { htmlReport } from 'https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js';
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.0.1/index.js';

// -----------------------------------------------------------------------------
// Métricas customizadas por endpoint
// -----------------------------------------------------------------------------
const m = {
    adicionar:         new Trend('dur_adicionar',          true),
    verificar:         new Trend('dur_verificar',          true),
    listar:            new Trend('dur_listar',             true),
    vagas:             new Trend('dur_vagas_restantes',    true),
    remover:           new Trend('dur_remover',            true),
    relatorioExcel:    new Trend('dur_relatorio_excel',    true),
    relatorioPdf:      new Trend('dur_relatorio_pdf',      true),
    legadoPdf:         new Trend('dur_legado_pdf',         true),
    removerDuplicatas: new Trend('dur_remover_duplicatas', true),
    migrarDados:       new Trend('dur_migrar_dados',       true),
    taxaErro:          new Rate('taxa_erro'),
    totalReqs:         new Counter('total_requisicoes'),
};

// -----------------------------------------------------------------------------
// Cenários de carga
//
//  [0s–30s]    smoke        — 1 VU, verifica se a API responde corretamente
//  [40s–130s]  load         — sobe até 20 VUs, simula uso normal sustentado
//  [150s–210s] stress       — sobe até 50 VUs, encontra o ponto de pressão
//  [220s–250s] spike        — salta para 100 VUs instantaneamente
//  [260s–560s] soak         — 10 VUs por 5 min, detecta vazamentos de memória
//  [570s–600s] administracao — 1 VU, valida endpoints administrativos
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

        administracao: {
            executor:  'constant-vus',
            vus:       1,
            duration:  '30s',
            startTime: '570s',
            tags:      { cenario: 'administracao' },
            exec:      'fluxoAdministracao',
        },
    },

    thresholds: {
        // --- Gerais ---
        // 2% acomoda falhas de conexão TCP esperadas durante o spike de 100 VUs
        'http_req_failed':                        ['rate<0.02'],
        'http_req_failed{cenario:smoke}':         ['rate<0.01'],
        'http_req_failed{cenario:load}':          ['rate<0.01'],
        'http_req_failed{cenario:stress}':        ['rate<0.01'],
        'http_req_failed{cenario:soak}':          ['rate<0.01'],
        'http_req_failed{cenario:spike}':         ['rate<0.05'],
        'http_req_duration': ['p(95)<2000'],
        'taxa_erro':         ['rate<0.02'],

        // --- Por endpoint ---
        'dur_adicionar':          ['p(95)<800',  'p(99)<1500'],
        'dur_verificar':          ['p(95)<300',  'p(99)<600' ],
        'dur_listar':             ['p(95)<600',  'p(99)<1200'],
        'dur_vagas_restantes':    ['p(95)<300',  'p(99)<600' ],
        'dur_remover':            ['p(95)<500',  'p(99)<1000'],
        'dur_relatorio_excel':    ['p(95)<3000', 'p(99)<5000'],
        'dur_relatorio_pdf':      ['p(95)<3000', 'p(99)<5000'],
        'dur_legado_pdf':         ['p(95)<5000', 'p(99)<8000'],
        'dur_remover_duplicatas': ['p(95)<2000', 'p(99)<4000'],
        'dur_migrar_dados':       ['p(95)<5000', 'p(99)<8000'],

        // --- Por cenário ---
        'http_req_duration{cenario:smoke}':         ['p(95)<500' ],
        'http_req_duration{cenario:load}':          ['p(95)<1500'],
        'http_req_duration{cenario:stress}':        ['p(95)<2500'],
        'http_req_duration{cenario:spike}':         ['p(95)<3000'],
        'http_req_duration{cenario:soak}':          ['p(95)<2000'],
        'http_req_duration{cenario:administracao}': ['p(95)<5000'],
    },
};

// -----------------------------------------------------------------------------
// Constantes
// -----------------------------------------------------------------------------
const BASE_URL   = __ENV.BASE_URL || 'http://localhost:5000';
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

function removerConvidado(nome) {
    const res = http.del(
        `${BASE_URL}/api/convidado/remover?nome=${encodeURIComponent(nome)}`,
        null, { headers: HEADERS, responseCallback: http.expectedStatuses(200, 400, 404) }
    );
    m.remover.add(res.timings.duration);
    m.totalReqs.add(1);
    return res;
}

function obterLegadoPdf() {
    const res = http.get(`${BASE_URL}/api/legado/relatorio/pdf`, { headers: HEADERS });
    m.legadoPdf.add(res.timings.duration);
    m.totalReqs.add(1);
    return res;
}

function removerDuplicatas() {
    const res = http.del(`${BASE_URL}/api/administracao/remover-duplicatas`, null, { headers: HEADERS });
    m.removerDuplicatas.add(res.timings.duration);
    m.totalReqs.add(1);
    return res;
}

function migrarDados() {
    const res = http.post(`${BASE_URL}/api/administracao/migrar-dados`, null, { headers: HEADERS });
    m.migrarDados.add(res.timings.duration);
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
            'listar: convidados � array': (r) => Array.isArray(r.json('convidados')),
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

        group('Relatório — Legado PDF', () => {
            const res = obterLegadoPdf();
            const ok = check(res, {
                'legado pdf: status 200':           (r) => r.status === 200,
                'legado pdf: content-type correto': (r) =>
                    (r.headers['Content-Type'] || '').includes('application/pdf'),
                'legado pdf: body não vazio':       (r) => r.body.length > 0,
            });
            m.taxaErro.add(!ok);
        });

        sleep(0.5);
    }

    // Remoção dinâmica — 1 a cada 3 iterações, evita esvaziar a base
    if (__ITER % 3 === 0) {
        const nomeDinamicoAnterior = `${PREFIXO_K6}VU-${__VU}-${__ITER - 3}`;
        group('Escrita — remover convidado', () => {
            const res = removerConvidado(nomeDinamicoAnterior);
            const ok = check(res, {
                'remover: 200, 404 ou 400': (r) => [200, 404, 400].includes(r.status),
            });
            m.taxaErro.add(!ok);
        });
        sleep(0.3);
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
            'spike vagas: status 200':          (r) => r.status === 200,
            'spike vagas: vagasRestantes >= 0': (r) => r.json('vagasRestantes') >= 0,
        });
        m.taxaErro.add(!ok);
    });

    sleep(0.5);
}

// -----------------------------------------------------------------------------
// Cenário: fluxoAdministracao
// Valida os endpoints administrativos: remover duplicatas e migrar dados.
// Roda com 1 VU após todos os outros cenários para não interferir nos dados.
// Usado por: administracao
// -----------------------------------------------------------------------------
export function fluxoAdministracao() {
    group('Administração — remover duplicatas', () => {
        const res = removerDuplicatas();
        const ok = check(res, {
            'remover-duplicatas: status 200': (r) => r.status === 200,
            'remover-duplicatas: mensagem presente': (r) => {
                try { return r.json('mensagem') !== undefined; } catch { return false; }
            },
        });
        m.taxaErro.add(!ok);
    });

    sleep(1);

    group('Administração — migrar dados', () => {
        const res = migrarDados();
        const ok = check(res, {
            'migrar-dados: status 200 ou 500': (r) => r.status === 200 || r.status === 500,
            'migrar-dados: mensagem presente': (r) => {
                try { return r.json('mensagem') !== undefined; } catch { return false; }
            },
        });
        m.taxaErro.add(!ok);
    });

    sleep(2);
}

// -----------------------------------------------------------------------------
// Teardown: remove todos os dados inseridos pelo k6
// -----------------------------------------------------------------------------

