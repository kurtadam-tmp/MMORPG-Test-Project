# ☁️ MMORPG Dedicated Server - Cloud VPS (Linux / AWS / DigitalOcean) Deployment Guide

This guide provides end-to-end instructions for deploying the entire MMORPG Dedicated Server cluster to any Linux Cloud VPS instance (AWS EC2, DigitalOcean Droplet, Hetzner, Linode) using Docker Compose.

---

## 1. 📋 System Prerequisites

- **Operating System:** Ubuntu 22.04 LTS or 24.04 LTS
- **Recommended Hardware:** 2+ vCPU, 4GB+ RAM, 40GB+ SSD
- **Installed Packages:** `git`, `docker.io`, `docker-compose-v2`

### Quick Docker Installation on Linux VPS:
```bash
sudo apt update && sudo apt upgrade -y
sudo apt install -y git docker.io docker-compose-v2
sudo systemctl enable --now docker
```

---

## 2. 🛡️ Firewall & Network Security Rules

Configure your Cloud Provider Firewall (AWS Security Group / DigitalOcean Firewall / UFW) to allow the following inbound ports:

| Port | Protocol | Usage / Service | Exposure Level |
| :--- | :--- | :--- | :--- |
| **`80 / 443`** | TCP | Nginx SSL Reverse Proxy | Public |
| **`5000`** | TCP | Gateway REST API & Web Dashboard | Public |
| **`7777`** | UDP | Zone Server #1 (Main World) | Public |
| **`7778`** | UDP | Zone Server #2 (Overflow Zone) | Public |
| **`7779`** | UDP | Zone Server #99 (Dungeon Instance) | Public |
| **`5432`** | TCP | PostgreSQL 16 Database | Private / Internal Network Only |
| **`6379`** | TCP | Redis 7 Cache & Pub/Sub | Private / Internal Network Only |

---

## 3. 🚀 Deployment Commands (Single-Command Launch)

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/YourOrganization/MMORPG-Test-Project.git
   cd MMORPG-Test-Project
   ```

2. **Launch All 7 Container Services:**
   ```bash
   docker compose up -d --build
   ```

3. **Verify Active Running Containers:**
   ```bash
   docker compose ps
   ```

   *Expected Output:*
   ```text
   NAME                    COMMAND                  SERVICE             STATUS
   mmorpg_postgres         "docker-entrypoint.s…"   postgres            running
   mmorpg_redis            "docker-entrypoint.s…"   redis               running
   mmorpg_gateway_api      "dotnet MMORPG.Gatew…"   gateway-api         running
   mmorpg_master_server    "dotnet MMORPG.Maste…"   master-server       running
   mmorpg_zone_server_1    "dotnet MMORPG.Serve…"   zone-server-1       running
   mmorpg_zone_server_2    "dotnet MMORPG.Serve…"   zone-server-2       running
   mmorpg_zone_dungeon_99  "dotnet MMORPG.Serve…"   zone-dungeon-99     running
   ```

---

## 4. 🌐 Production Nginx SSL Reverse Proxy Configuration

Create `/etc/nginx/sites-available/mmorpg` for HTTPS encryption:

```nginx
server {
    listen 80;
    server_name api.yourgame.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'keep-alive';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

Enable SSL via Certbot:
```bash
sudo apt install -y certbot python3-certbot-nginx
sudo certbot --nginx -d api.yourgame.com
```

---

## 5. 🔍 Monitoring & Log Inspection

- **View Live Container Logs:**
  ```bash
  docker compose logs -f gateway-api
  docker compose logs -f zone-server-1
  ```

- **Restart Dedicated Zone Servers:**
  ```bash
  docker compose restart zone-server-1 zone-server-2 zone-dungeon-99
  ```
