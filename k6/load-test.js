import http from 'k6/http';
import { check, sleep } from 'k6';
import { htmlReport } from 'https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js';
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.0.1/index.js';

// ---------------------------------------------------------------------------
// Cenários de carga
// ---------------------------------------------------------------------------
export const options = {
  scenarios: {
    // Aquecimento: sobe gradualmente até 10 usuários simultâneos
    aquecimento: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: '15s', target: 10 },
        { duration: '30s', target: 10 },
        { duration: '15s', target: 0 },
      ],
      gracefulRampDown: '5s',
      tags: { cenario: 'aquecimento' },
    },

    // Pico: simula um momento de alta demanda
    pico: {
      executor: 'ramping-vus',
      startTime: '65s',
      startVUs: 0,
      stages: [
        { duration: '10s', target: 50 },
        { duration: '20s', target: 50 },
        { duration: '10s', target: 0 },
      ],
      gracefulRampDown: '5s',
      tags: { cenario: 'pico' },
    },
  },

  // Thresholds: o job falha se esses limites forem ultrapassados
  thresholds: {
    http_req_failed:          ['rate<0.01'],        // menos de 1% de erros
    http_req_duration:        ['p(95)<500'],        // 95% das requisições abaixo de 500ms
    'http_req_duration{endpoint:listar}':     ['p(95)<400'],
    'http_req_duration{endpoint:verificar}':  ['p(95)<300'],
    'http_req_duration{endpoint:adicionar}':  ['p(95)<600'],
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------
const headers = { 'Content-Type': 'application/json' };

function adicionarConvidado(nome) {
  const payload = JSON.stringify({
    nome,
    presencaConfirmada: true,
    participacao: 'Sozinho',
    quantidadeAcompanhantes: 0,
    nomesAcompanhantes: [],
  });

  return http.post(`${BASE_URL}/api/convidado/adicionar`, payload, {
    headers,
    tags: { endpoint: 'adicionar' },
  });
}

function listarConvidados() {
  return http.get(`${BASE_URL}/api/convidado/listar`, {
    tags: { endpoint: 'listar' },
  });
}

function verificarConvidado(nome) {
  return http.get(`${BASE_URL}/api/convidado/verificar?nome=${encodeURIComponent(nome)}`, {
    tags: { endpoint: 'verificar' },
  });
}

function obterVagasRestantes() {
  return http.get(`${BASE_URL}/api/convidado/vagas-restantes`, {
    tags: { endpoint: 'vagas-restantes' },
  });
}

// ---------------------------------------------------------------------------
// Fluxo principal executado por cada VU em cada iteração
// ---------------------------------------------------------------------------
export default function () {
  const nome = `Convidado K6 ${__VU}-${__ITER}`;

  // 1. Lista convidados existentes
  const resListar = listarConvidados();
  check(resListar, {
    '[listar] status 200': (r) => r.status === 200,
    '[listar] body é array': (r) => Array.isArray(r.json()),
  });

  sleep(0.3);

  // 2. Verifica se o convidado já existe
  const resVerificar = verificarConvidado(nome);
  check(resVerificar, {
    '[verificar] status 200': (r) => r.status === 200,
    '[verificar] campo existe presente': (r) => r.json('existe') !== undefined,
  });

  sleep(0.3);

  // 3. Adiciona o convidado (pode retornar 401 quando o limite for atingido)
  const resAdicionar = adicionarConvidado(nome);
  check(resAdicionar, {
    '[adicionar] cadastrado ou limite atingido': (r) => r.status === 201 || r.status === 401,
  });

  sleep(0.3);

  // 4. Consulta vagas restantes
  const resVagas = obterVagasRestantes();
  check(resVagas, {
    '[vagas] status 200': (r) => r.status === 200,
    '[vagas] vagasRestantes >= 0': (r) => r.json('vagasRestantes') >= 0,
  });

  sleep(0.5);
}

// ---------------------------------------------------------------------------
// Teardown: remove todos os dados inseridos pelo k6
// ---------------------------------------------------------------------------
export function teardown() {
  const lista = http.get(`${BASE_URL}/api/convidado/listar`);
  if (lista.status !== 200) return;

  let removidos = 0;
  for (const c of lista.json()) {
    if (!c.nome.startsWith('Convidado K6')) continue;
    const res = http.del(
      `${BASE_URL}/api/convidado/remover?nome=${encodeURIComponent(c.nome)}`,
      null, { headers }
    );
    if (res.status === 200) removidos++;
  }

  console.log(`[teardown] ${removidos} convidado(s) removido(s). Base limpa.`);
}

// ---------------------------------------------------------------------------
// Relatórios gerados ao final da execução
// ---------------------------------------------------------------------------
export function handleSummary(data) {
  return {
    'k6/relatorio.html': htmlReport(data),
    stdout: textSummary(data, { indent: '  ', enableColors: true }),
  };
}
