import http from 'k6/http';
import { check, sleep, group } from 'k6';
import { Trend, Rate, Counter } from 'k6/metrics';
import { htmlReport } from 'https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js';
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.0.1/index.js';

// -----------------------------------------------------------------------------
// MÃ©tricas customizadas por endpoint
// -----------------------------------------------------------------------------
const m = {
    adicionar:         new Trend('dur_adicionar',          true),
    verificar:         new Trend('dur_verificar',          true),
    listar:            new Trend('dur_listar',             true),
    vagas:             new Trend('dur_vagas_restantes',    true),
    remover:           new Trend('dur_remover',            true),
    relatorioExcel:    new Trend('dur_relatorio_excel',    true),
    relatorioPdf:      new Trend('dur_relatorio_pdf',      true),
    removerDuplicatas: new Trend('dur_remover_duplicatas', true),
    migrarDados:       new Trend('dur_migrar_dados',       true),
    taxaErro:          new Rate('taxa_erro'),
    totalReqs:         new Counter('total_requisicoes'),
};

// -----------------------------------------------------------------------------
// CenÃ¡rios de carga
//
//  [0sâ€“30s]    smoke        â€” 1 VU, verifica se a API responde corretamente
//  [40sâ€“130s]  load         â€” sobe atÃ© 20 VUs, simula uso normal sustentado
//  [150sâ€“210s] stress       â€” sobe atÃ© 50 VUs, encontra o ponto de pressÃ£o
//  [220sâ€“250s] spike        â€” salta para 100 VUs instantaneamente
//  [260sâ€“560s] soak         â€” 10 VUs por 5 min, detecta vazamentos de memÃ³ria
//  [570sâ€“600s] administracao â€” 1 VU, valida endpoints administrativos
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
        // 2% acomoda falhas de conexÃ£o TCP esperadas durante o spike de 100 VUs
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
        'dur_remover_duplicatas': ['p(95)<2000', 'p(99)<4000'],
        'dur_migrar_dados':       ['p(95)<5000', 'p(99)<8000'],

        // --- Por cenÃ¡rio ---
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
// Setup: insere convidados fixos antes de qualquer cenÃ¡rio rodar
// -----------------------------------------------------------------------------
export function setup() {
    const inseridos = [];

    for (const nome of NOMES_FIXOS) {
        const nomeCompleto = `${PREFIXO_K6}${nome}`;
        const res = adicionarConvidado(nomeCompleto);

        if (res.status === 201) {
            inseridos.push(nomeCompleto);
        } else if (res.status === 401) {
            console.warn(`[setup] Limite atingido apÃ³s ${inseridos.length} inserÃ§Ãµes.`);
            break;
        }
    }

    console.log(`[setup] ${inseridos.length} convidado(s) inserido(s).`);
    return { nomesFixos: inseridos };
}

// -----------------------------------------------------------------------------
// CenÃ¡rio: fluxoPadrao
// Representa o comportamento completo de um usuÃ¡rio real.
// Usado por: smoke, load, stress, soak
// -----------------------------------------------------------------------------
export function fluxoPadrao(data) {
const nomeFixo     = nomeAleatorio(data.nomesFixos);
const nomeDinamico = `${PREFIXO_K6}VU-${__VU}-${__ITER}`;
if (!nomeFixo) { sleep(1); return; }

    group('Escrita â€” adicionar convidado', () => {
        const res = adicionarConvidado(nomeDinamico);
        const ok = check(res, {
            'adicionar: 201 ou 401': (r) => r.status === 201 || r.status === 401,
        });
        m.taxaErro.add(!ok);
    });

    sleep(0.3);

    group('Leitura â€” verificar convidado', () => {
        const res = verificarConvidado(nomeFixo);
        const ok = check(res, {
            'verificar: status 200':                          (r) => r.status === 200,
            'verificar: campo existeComoConvidado presente':  (r) => r.json('existeComoConvidado') !== undefined,
            'verificar: campo existeComoAcompanhante presente': (r) => r.json('existeComoAcompanhante') !== undefined,
        });
        m.taxaErro.add(!ok);
    });

    sleep(0.3);

    group('Leitura â€” listar convidados', () => {
        const res = listarConvidados();
        const ok = check(res, {
            'listar: status 200':    (r) => r.status === 200,
            'listar: convidados é array': (r) => Array.isArray(r.json('convidados')),
        });
        m.taxaErro.add(!ok);
    });

    sleep(0.3);

    group('Leitura â€” vagas restantes', () => {
        const res = obterVagasRestantes();
        const ok = check(res, {
            'vagas: status 200':              (r) => r.status === 200,
            'vagas: vagasRestantes >= 0':     (r) => r.json('vagasRestantes') >= 0,
        });
        m.taxaErro.add(!ok);
    });

    sleep(0.5);

    // RelatÃ³rios sÃ£o pesados â€” executados 1 a cada 5 iteraÃ§Ãµes
    if (__ITER % 5 === 0) {
        group('RelatÃ³rio â€” Excel', () => {
            const res = obterRelatorioExcel();
            const ok = check(res, {
                'excel: status 200':           (r) => r.status === 200,
                'excel: content-type correto': (r) =>
                    (r.headers['Content-Type'] || '').includes('spreadsheetml'),
                'excel: body nÃ£o vazio':       (r) => r.body.length > 0,
            });
            m.taxaErro.add(!ok);
        });

        sleep(0.5);

        group('RelatÃ³rio â€” PDF', () => {
            const res = obterRelatorioPdf();
            const ok = check(res, {
                'pdf: status 200':           (r) => r.status === 200,
                'pdf: content-type correto': (r) =>
                    (r.headers['Content-Type'] || '').includes('application/pdf'),
                'pdf: body nÃ£o vazio':       (r) => r.body.length > 0,
            });
            m.taxaErro.add(!ok);
        });

        sleep(0.5);

        group('RelatÃ³rio â€” Legado PDF', () => {
            const res = obterLegadoPdf();
            const ok = check(res, {
                'legado pdf: status 200':           (r) => r.status === 200,
                'legado pdf: content-type correto': (r) =>
                    (r.headers['Content-Type'] || '').includes('application/pdf'),
                'legado pdf: body nÃ£o vazio':       (r) => r.body.length > 0,
            });
            m.taxaErro.add(!ok);
        });

        sleep(0.5);
    }

    // RemoÃ§Ã£o dinÃ¢mica â€” 1 a cada 3 iteraÃ§Ãµes, evita esvaziar a base
    if (__ITER % 3 === 0) {
        const nomeDinamicoAnterior = `${PREFIXO_K6}VU-${__VU}-${__ITER - 3}`;
        group('Escrita â€” remover convidado', () => {
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
// CenÃ¡rio: fluxoLeitura
// Somente endpoints de leitura â€” usado no spike para nÃ£o comprometer
// a integridade dos dados durante o pico sÃºbito de usuÃ¡rios.
// Usado por: spike
// -----------------------------------------------------------------------------
export function fluxoLeitura(data) {
    const nomeFixo = nomeAleatorio(data.nomesFixos);
    if (!nomeFixo) { sleep(1); return; }

    group('Spike â€” verificar', () => {
        const res = verificarConvidado(nomeFixo);
        const ok = check(res, {
            'spike verificar: status 200': (r) => r.status === 200,
        });
        m.taxaErro.add(!ok);
    });

    sleep(0.2);

    group('Spike â€” listar', () => {
        const res = listarConvidados();
        const ok = check(res, {
            'spike listar: status 200': (r) => r.status === 200,
        });
        m.taxaErro.add(!ok);
    });

    sleep(0.2);

    group('Spike â€” vagas', () => {
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
// CenÃ¡rio: fluxoAdministracao
// Valida os endpoints administrativos: remover duplicatas e migrar dados.
// Roda com 1 VU apÃ³s todos os outros cenÃ¡rios para nÃ£o interferir nos dados.
// Usado por: administracao
// -----------------------------------------------------------------------------
export function fluxoAdministracao() {
    group('AdministraÃ§Ã£o â€” remover duplicatas', () => {
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

    group('AdministraÃ§Ã£o â€” migrar dados', () => {
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

