# AegiFinance — Guía de Deploy en Producción

Plataforma: Ubuntu Server + Docker + Cloudflare Tunnel.

```
Internet ──HTTPS──► Cloudflare ──Tunnel──► [contenedor cloudflared]
                                                    │ red proxy
                                           [aegifinance-web:8080]
                                                    │ red aegifinance
                                           [aegifinance-sqlserver:1433]
                                           [aegifinance-worker]
```

---

## 1. Prerrequisitos del servidor

```bash
# Docker Engine y plugin Compose
curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER   # cerrar sesión y volver a entrar

# Verificar
docker version
docker compose version          # debe ser v2.x
```

---

## 2. Red Docker compartida con el túnel

El Cloudflare Tunnel connector corre como contenedor en la red `proxy`. Crea la red una sola vez en el servidor:

```bash
docker network create proxy
```

> Si ya tienes otro servicio que crea esta red (como `cloudflared` con su propio `docker-compose.yml`), omite este comando.

---

## 3. Clonar el repositorio

```bash
git clone <url-del-repo> /opt/aegifinance
cd /opt/aegifinance
```

---

## 4. Crear el archivo `.env`

```bash
cp .env.example .env
nano .env          # o vim, o el editor que prefieras
```

Valores obligatorios que **debes cambiar**:

| Variable | Cómo generar |
|---|---|
| `MSSQL_SA_PASSWORD` | Elige una contraseña fuerte (≥8 chars, mayúsc + núms + especiales) |
| `JWT_SECRET` | `openssl rand -base64 48` |
| `CORS_ALLOWED_ORIGINS` | URL exacta del frontend, ej. `https://app.aegifinance.com` |

El archivo `.env` nunca se sube al repositorio (está en `.gitignore`).

---

## 5. Primer build y arranque

```bash
cd /opt/aegifinance

# Construir imágenes (solo la primera vez o tras cambios en el código)
docker compose build

# Arrancar todos los servicios en segundo plano
docker compose up -d
```

Al arrancar por primera vez, `aegifinance-web` aplica las migraciones de EF Core automáticamente y ejecuta el seed de datos (usuario `Admin` / contraseña `Admin` — **cámbiala inmediatamente**).

Verifica que todo está saludable:

```bash
docker compose ps          # todos en "healthy" o "running"
docker compose logs web    # ver migraciones y arranque
curl http://localhost:8088/health   # debe responder {"status":"ok",...}
```

---

## 6. Cloudflare Tunnel

### Opción A — Tunnel como contenedor Docker (recomendado)

Crea el tunnel en el panel de Cloudflare Zero Trust → Networks → Tunnels, copia el token y crea un `docker-compose.yml` separado para cloudflared (o añádelo al tuyo):

```yaml
services:
  cloudflared:
    image: cloudflare/cloudflared:latest
    container_name: cloudflared
    restart: unless-stopped
    command: tunnel --no-autoupdate run --token ${CLOUDFLARE_TUNNEL_TOKEN}
    networks:
      - proxy

networks:
  proxy:
    external: true
```

En el panel de Cloudflare, configura el hostname público del tunnel apuntando a:

```
http://aegifinance-web:8080
```

(El contenedor `cloudflared` y `aegifinance-web` comparten la red `proxy`, así que se resuelven por nombre.)

### Opción B — Tunnel como servicio del sistema

Si prefieres instalar `cloudflared` como servicio systemd en el host:

```bash
curl -L https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64.deb -o cloudflared.deb
sudo dpkg -i cloudflared.deb
sudo cloudflared service install <TOKEN>
```

En el panel de Cloudflare, apunta el hostname a:

```
http://localhost:8088
```

(El puerto `8088` está enlazado solo a `127.0.0.1` en el host.)

---

## 7. Actualizar a una nueva versión

```bash
cd /opt/aegifinance

git pull

# Reconstruir solo las imágenes modificadas
docker compose build

# Reemplazar contenedores (zero-downtime: Compose levanta los nuevos antes de bajar los viejos)
docker compose up -d

# Las migraciones nuevas se aplican automáticamente al arrancar
```

Si necesitas forzar reconstrucción completa sin caché (por ejemplo, para actualizar la imagen base de .NET):

```bash
docker compose build --no-cache
docker compose up -d
```

---

## 8. Backup de la base de datos

El volumen `aegifinance-sql-data` contiene todos los archivos de SQL Server. Haz backup del volumen directamente:

```bash
# Detener el contenedor de SQL para un snapshot consistente
docker compose stop sqlserver

# Comprimir el volumen
sudo tar -czf /backups/aegifinance-sql-$(date +%Y%m%d).tar.gz \
  /var/lib/docker/volumes/aegifinance_aegifinance-sql-data/_data

# Volver a arrancar
docker compose start sqlserver
```

Alternativamente, haz backup en caliente con `sqlcmd` (sin detener el servicio):

```bash
docker exec aegifinance-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C \
  -Q "BACKUP DATABASE [AegiFinanceDb] TO DISK = N'/var/opt/mssql/backup/AegiFinanceDb_$(date +%Y%m%d).bak' WITH INIT"
```

---

## 9. Comandos útiles

```bash
# Ver estado de todos los contenedores
docker compose ps

# Logs en tiempo real
docker compose logs -f web
docker compose logs -f worker
docker compose logs -f sqlserver

# Reiniciar un servicio específico
docker compose restart web

# Entrar al contenedor web (debug)
docker exec -it aegifinance-web bash

# Entrar a SQL Server
docker exec -it aegifinance-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C

# Detener todo (los datos persisten en volúmenes)
docker compose down

# Eliminar todo incluyendo volúmenes (¡borra la BD!)
docker compose down -v
```

---

## 10. Variables de entorno — referencia completa

| Variable | Descripción | Requerida |
|---|---|---|
| `MSSQL_SA_PASSWORD` | Contraseña del usuario `sa` de SQL Server | **Sí** |
| `JWT_SECRET` | Clave para firmar tokens JWT (mín. 32 chars) | **Sí** |
| `CORS_ALLOWED_ORIGINS` | URL del frontend Next.js, sin barra final | **Sí** |
| `EXCHANGE_RATE_BASE_URL` | URL base del proveedor de tipo de cambio | No (default incluido) |
| `EXCHANGE_RATE_API_KEY` | API key del proveedor | No |
| `EXCHANGE_RATE_ENABLED` | `true` para activar consultas automáticas | No (default `false`) |
| `BILLING_DAILY_RUN_TIME` | Hora de ejecución del worker (formato `HH:mm`) | No (default `02:00`) |

---

## 11. Después del primer arranque

1. **Cambiar la contraseña del administrador**: entra con `Admin` / `Admin` y cámbiala desde el perfil.
2. **Configurar tipo de cambio**: si usas monedas extranjeras, activa `EXCHANGE_RATE_ENABLED=true` y provee la API key.
3. **Verificar el tunnel**: abre `https://api.aegifinance.com/health` desde internet — debe responder `{"status":"ok"}`.
4. **Configurar backups automáticos**: crea un cron job en el servidor para ejecutar el script de backup diariamente.
