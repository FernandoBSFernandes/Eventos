import http from 'k6/http';
import { check, sleep } from 'k6';
import { htmlReport } from 'https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js';
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.0.1/index.js';

// ---------------------------------------------------------------------------
// Cenários de carga para o endpoint POST /api/relatorio/enviar
// ---------------------------------------------------------------------------
export const options = {
  scenarios: {
    // Smoke: verificação básica do endpoint
    smoke: {
      executor: 'constant-vus',
      vus: 1,
      duration: '10s',
      tags: { cenario: 'smoke' },
    },

    // Load: carga moderada
    load: {
      executor: 'constant-vus',
      startTime: '15s',
      vus: 5,
      duration: '30s',
      tags: { cenario: 'load' },
    },

    // Stress: pico de carga
    stress: {
      executor: 'constant-vus',
      startTime: '50s',
      vus: 10,
      duration: '20s',
      tags: { cenario: 'stress' },
    },
  },

  // Thresholds específicos para o endpoint de envio de relatório
  thresholds: {
    'http_req_duration{endpoint:enviar-relatorio}': ['p(95)<3000'], // 95% das requests < 3s
    'http_req_failed{endpoint:enviar-relatorio}':   ['rate<0.1'],   // menos de 10% de falhas
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------
const headers = { 'Content-Type': 'application/json' };

function enviarRelatorio() {
  return http.post(`${BASE_URL}/api/relatorio/enviar`, null, {
    headers,
    tags: { endpoint: 'enviar-relatorio' },
  });
}

// ---------------------------------------------------------------------------
// Fluxo principal executado por cada VU em cada iteração
// ---------------------------------------------------------------------------
export default function () {
  const res = enviarRelatorio();

  check(res, {
    '[enviar-relatorio] status 200': (r) => r.status === 200,
    '[enviar-relatorio] body contém mensagem de sucesso': (r) => {
      try {
        const body = r.json();
        return body && body.mensagem !== undefined;
      } catch {
        return false;
      }
    },
  });

  sleep(1);
}

// ---------------------------------------------------------------------------
// Relatórios gerados ao final da execução
// ---------------------------------------------------------------------------
export function handleSummary(data) {
  return {
    'k6/relatorio-email-report.html': htmlReport(data),
    stdout: textSummary(data, { indent: '  ', enableColors: true }),
  };
}
