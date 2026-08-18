# Deploy de AegiFinance en Ubuntu con Docker Compose

Esta instalación publica únicamente el frontend. Next.js envía internamente `/api/*` al contenedor .NET; SQL Server, la API y el worker permanecen en la red privada de Compose.

## Requisitos

- Ubuntu 22.04 o 24.04 de 64 bits.
- Docker Engine y el plugin Docker Compose actualizados.
- Un dominio HTTPS administrado por Nginx Proxy Manager, Cloudflare Tunnel u otro proxy inverso.
- Al menos 4 GB de RAM; 8 GB es preferible para compilar y ejecutar SQL Server en el mismo host.

## Primera instalación

```bash
git clone <repositorio> /opt/aegifinance
cd /opt/aegifinance
AEGI_BOOTSTRAP_PASSWORD='contraseña-temporal' ./scripts/bootstrap-env.sh https://finance.example.com
```

El script crea el `.env` ignorado por Git con permisos `600`, genera un JWT criptográfico nuevo y configura el origen público de CORS. No subas `.env` al repositorio. Si necesitás API de tipo de cambio, editá después `EXCHANGE_RATE_API_KEY` y activá `EXCHANGE_RATE_ENABLED=true`.

Antes de iniciar:

```bash
docker compose config --quiet
docker compose build --pull
docker compose up -d
docker compose ps
docker compose logs --tail=100 web worker frontend
```

La API espera a que SQL Server esté saludable, aplica las migraciones pendientes una sola vez y después crea los catálogos iniciales. El primer usuario es `Admin` y utiliza `ADMIN_SEED_PASSWORD`; la aplicación obliga a cambiar esa contraseña.

## Proxy y dominio

Si Nginx Proxy Manager o Cloudflare se ejecuta en Docker y ya utiliza una red llamada `proxy`, configurá:

```dotenv
PROXY_NETWORK_NAME=proxy
PROXY_NETWORK_EXTERNAL=true
```

El proxy debe apuntar a:

- host interno: `aegifinance-frontend`;
- puerto: `3000`;
- esquema: `http`;
- WebSocket: habilitado;
- dominio público: el mismo valor usado en `CORS_ALLOWED_ORIGINS`.

No publiques los puertos de SQL Server, API o worker. Si el proxy se ejecuta directamente en el host Ubuntu, apuntá a `http://127.0.0.1:3001`.

## Actualización segura

```bash
cd /opt/aegifinance
git pull --ff-only
docker compose config --quiet
docker compose build --pull
docker compose up -d --remove-orphans
docker compose ps
docker compose logs --tail=100 web worker frontend
```

No uses `docker compose down -v`: la opción `-v` elimina la base de datos, documentos y claves persistentes.

## Respaldo

Creá primero un backup nativo de SQL Server:

```bash
docker compose exec -T sqlserver mkdir -p /var/opt/mssql/backup
docker compose exec -T sqlserver sh -c '/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "BACKUP DATABASE [AegiFinanceDb] TO DISK = N'"'"'/var/opt/mssql/backup/AegiFinanceDb.bak'"'"' WITH INIT, CHECKSUM" -b'
docker compose cp sqlserver:/var/opt/mssql/backup/AegiFinanceDb.bak ./AegiFinanceDb.bak
```

Además respaldá los volúmenes:

- `aegifinance-client-documents`: archivos asociados a clientes;
- `aegifinance-dataprotection-keys`: claves que protegen números de cuenta y sesiones;
- `aegifinance-sql-data`: complemento del backup nativo, nunca su sustituto.

Perder `aegifinance-dataprotection-keys` puede hacer irrecuperables los campos protegidos, aunque la base de datos sobreviva.

## Diagnóstico

```bash
docker compose ps
docker compose logs -f web
docker compose logs -f worker
docker compose logs -f sqlserver
curl -I http://127.0.0.1:3001/login
```

La salud de `web` sólo se declara correcta cuando también puede conectarse a SQL Server. El frontend no debe apuntar a `localhost` desde su contenedor: su destino interno correcto es `http://web:8080`.

## Licencia de SQL Server

El valor predeterminado es `MSSQL_PID=Express`, gratuito pero limitado a 10 GB por base de datos y a los límites de recursos de esa edición. Para una operación que supere esos límites, configurá una edición de SQL Server con licencia válida. La edición `Developer` no está autorizada para cargas productivas.
