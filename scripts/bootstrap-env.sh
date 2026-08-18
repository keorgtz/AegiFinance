#!/usr/bin/env bash
set -euo pipefail

project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
env_file="${project_root}/.env"
public_origin="${1:-http://localhost:3001}"
bootstrap_password="${AEGI_BOOTSTRAP_PASSWORD:-}"

if [[ -z "${bootstrap_password}" ]]; then
  read -r -s -p "Temporary SQL/Admin password: " bootstrap_password
  printf '\n'
fi

if [[ ${#bootstrap_password} -lt 8 ]]; then
  echo "Password must contain at least 8 characters." >&2
  exit 1
fi

if ! command -v openssl >/dev/null 2>&1; then
  echo "OpenSSL is required to generate JWT_SECRET." >&2
  exit 1
fi

umask 077
jwt_secret="$(openssl rand -base64 64 | tr -d '\n')"

cat > "${env_file}" <<EOF
MSSQL_SA_PASSWORD=${bootstrap_password}
MSSQL_PID=Express
JWT_SECRET=${jwt_secret}
ADMIN_SEED_PASSWORD=${bootstrap_password}
TZ=America/Mexico_City
SECURITY_MAX_FAILED_LOGIN_ATTEMPTS=5
SECURITY_LOCKOUT_MINUTES=15
SECURITY_SESSION_LIFETIME_DAYS=7
PROXY_NETWORK_NAME=aegifinance-proxy
PROXY_NETWORK_EXTERNAL=false
CORS_ALLOWED_ORIGINS=${public_origin}
EXCHANGE_RATE_BASE_URL=https://api.exchangerate-api.com/v4/latest/
EXCHANGE_RATE_API_KEY=
EXCHANGE_RATE_ENABLED=false
BILLING_DAILY_RUN_TIME=02:00
EOF

chmod 600 "${env_file}"
echo "Created ${env_file} with mode 600 and a new random JWT secret."
echo "Validate with: docker compose config --quiet"
