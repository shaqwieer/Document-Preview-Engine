# ONLYOFFICE + .NET 8 + Vue 3 — Fully Offline

Self-hosted document viewing for PPTX, DOCX, XLSX, and PDF using ONLYOFFICE Community Edition, ASP.NET Core 8, and Vue 3.

---

## 🚀 What You Get

* Upload and preview **PPTX**, **DOCX**, **XLSX**, and **PDF**
* View documents directly in the browser with Microsoft Office–compatible rendering
* Fully offline after initial setup
* Free and open-source (**ONLYOFFICE Community Edition - AGPLv3**)
* Built-in Arabic RTL support
* Custom Arabic fonts support

---

## 📁 Folder Structure

```text
edu-platform/
├── docker-compose.yml            # Main entry point
├── .env.example                  # Copy to .env and fill values
├── storage/                      # Uploaded files (auto-created)
│
├── onlyoffice/
│   └── fonts/                    # Optional Arabic fonts (.ttf)
│
├── dotnet-api/
│   ├── Dockerfile
│   ├── Program.cs
│   ├── appsettings.json
│   ├── EduPlatform.Api.csproj
│   ├── Controllers/
│   │   └── FilesController.cs
│   └── Services/
│       └── OnlyOfficeService.cs
│
└── vue-frontend/
    ├── Dockerfile
    ├── nginx.conf
    ├── package.json
    ├── vite.config.js
    ├── index.html
    └── src/
        ├── main.js
        ├── App.vue
        └── components/
            ├── FileViewer.vue
            └── FileUpload.vue
```

---

## 📦 Prerequisites

### Install Docker

#### Windows

Install Docker Desktop.

#### Ubuntu / Debian

```bash
sudo apt install docker.io docker-compose
```

### Verify Installation

```bash
docker --version
docker compose version
```

---

## ⚙️ First-Time Setup

### 1. Copy the Project

Transfer the project to your target server.

---

### 2. Create Environment File

```bash
cp .env.example .env
```

Edit `.env` and set a strong JWT secret:

```env
ONLYOFFICE_JWT_SECRET=MyEduPlatform_SuperSecretKey_2024_ChangeThis!
```

---

### 3. Add Arabic Fonts (Optional)

Copy fonts such as:

* Cairo.ttf
* Tajawal.ttf
* NotoSansArabic.ttf

into:

```text
./onlyoffice/fonts/
```

Restart ONLYOFFICE after adding fonts.

---

### 4. Create Storage Directory

```bash
mkdir -p storage
```

---

## ▶️ Start Everything

```bash
docker compose up -d
```

### First Run

The first startup downloads Docker images and may take **5–10 minutes**.

Subsequent starts usually take only a few seconds.

---

### Verify Containers

```bash
docker compose ps
```

Expected output:

| Container      | Purpose                    | Port |
| -------------- | -------------------------- | ---- |
| edu_onlyoffice | ONLYOFFICE Document Server | 8080 |
| edu_api        | ASP.NET Core API           | 5000 |
| edu_vue        | Vue 3 Frontend             | 3000 |

---

## 🌐 Open the Application

### Local Machine

```text
http://localhost:3000
```

### Other Devices on the Same Network

```text
http://YOUR_SERVER_IP:3000
```

Upload a document and click **Preview**.

---

## 🛠 Useful Commands

### View All Logs

```bash
docker compose logs -f
```

### View ONLYOFFICE Logs

```bash
docker compose logs -f onlyoffice
```

### Stop Everything

```bash
docker compose down
```

### Restart API

```bash
docker compose restart dotnet_api
```

### Rebuild Frontend

```bash
docker compose up -d --build vue_frontend
```

### Rebuild API

```bash
docker compose up -d --build dotnet_api
```

### Full Reset (Deletes Uploaded Files)

```bash
docker compose down -v
```

> ⚠️ Warning: This removes all uploaded documents.

---

## 🔍 Troubleshooting

### ONLYOFFICE Shows "Download Failed"

Verify the API is reachable from the ONLYOFFICE container:

```bash
docker compose exec onlyoffice curl http://dotnet_api:80/api/files
```

---

### JWT Errors

Ensure the JWT secret matches between ONLYOFFICE and .NET:

```bash
docker compose down
docker compose up -d
```

---

### Arabic Fonts Not Rendering

Place `.ttf` files in:

```text
./onlyoffice/fonts/
```

Then restart ONLYOFFICE:

```bash
docker compose restart onlyoffice
```

---

### File Too Large

Increase upload limit inside `nginx.conf`:

```nginx
client_max_body_size 500M;
```

Then rebuild the frontend container.

---

### Port Already in Use

Change port mappings in `docker-compose.yml`:

```yaml
ports:
  - "9090:80"
```

instead of:

```yaml
ports:
  - "8080:80"
```

For Vue:

```yaml
ports:
  - "4000:80"
```

instead of:

```yaml
ports:
  - "3000:80"
```

Also update:

```env
VITE_ONLYOFFICE_PUBLIC_URL=http://SERVER_IP:9090
```

---

## 🚀 Production Deployment

### Update ONLYOFFICE Public URL

```yaml
environment:
  - ONLYOFFICE__PublicUrl=http://YOUR_SERVER_IP:8080
```

---

### Configure CORS

Update `appsettings.json`:

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://YOUR_SERVER_IP:3000"
    ]
  }
}
```

---

### Frontend Environment

Create a frontend `.env`:

```env
VITE_ONLYOFFICE_PUBLIC_URL=http://YOUR_SERVER_IP:8080
```

---

### User Access

```text
http://YOUR_SERVER_IP:3000
```

---

## 🔒 Recommended Production Enhancements

### Enable HTTPS

Use one of:

* Nginx
* Traefik
* Caddy

---

### Restrict Internal Services

Expose ONLY:

* Frontend
* Reverse Proxy

Keep API and ONLYOFFICE internal when possible.

---

### Backup Storage

Regularly backup:

```text
/storage
```

---

### Pin Docker Image Versions

Avoid using `latest` tags in production.

Example:

```yaml
image: onlyoffice/documentserver:8.2.0
```

---

## ✨ Features Summary

| Feature            | Supported |
| ------------------ | --------- |
| PPTX Viewer        | ✅         |
| DOCX Viewer        | ✅         |
| XLSX Viewer        | ✅         |
| PDF Viewer         | ✅         |
| Arabic RTL         | ✅         |
| Offline Operation  | ✅         |
| Docker Deployment  | ✅         |
| Custom Fonts       | ✅         |
| .NET 8 Backend     | ✅         |
| Vue 3 Frontend     | ✅         |
| Free & Open Source | ✅         |

---

Built with **ONLYOFFICE Community Edition**, **ASP.NET Core 8**, **Vue 3**, and **Docker Compose**.
