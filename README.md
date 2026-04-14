# ONLYOFFICE + .NET 8 + Vue 3 — Fully Offline

================================================================
 WHAT YOU GET
================================================================

- Upload PPTX (all versions), DOCX, XLSX, PDF
- Users view files in browser exactly like Microsoft Office
- 100% offline — no internet calls after setup
- Free (ONLYOFFICE Community Edition, AGPLv3)
- Arabic RTL support built-in

================================================================
 FOLDER STRUCTURE
================================================================

edu-platform/
├── docker-compose.yml            ← main entry point
├── .env.example                  ← copy to .env and fill in
├── storage/                      ← uploaded files live here (auto-created)
│
├── onlyoffice/
│   └── fonts/                    ← put .ttf Arabic fonts here
│
├── dotnet-api/                   ← .NET 8 ASP.NET Core API
│   ├── Dockerfile
│   ├── Program.cs
│   ├── appsettings.json
│   ├── EduPlatform.Api.csproj
│   ├── Controllers/
│   │   └── FilesController.cs
│   └── Services/
│       └── OnlyOfficeService.cs
│
└── vue-frontend/                 ← Vue 3 + Vite frontend
    ├── Dockerfile
    ├── nginx.conf
    ├── package.json
    ├── vite.config.js
    ├── index.html
    └── src/
        ├── main.js
        ├── App.vue
        └── components/
            ├── FileViewer.vue    ← ONLYOFFICE viewer wrapper
            └── FileUpload.vue    ← drag & drop uploader

================================================================
 STEP 1 — Prerequisites (install once)
================================================================

Install Docker Desktop:
  Windows: https://docs.docker.com/desktop/install/windows-install/
  Linux:   sudo apt install docker.io docker-compose

Verify:
  docker --version
  docker compose version

================================================================
 STEP 2 — First time setup
================================================================

# 1. Copy this project to your server
#    (USB drive, LAN copy, whatever — no internet needed after this)

# 2. Create your .env file
cp .env.example .env

# 3. Edit .env — change the JWT secret to any long random string
#    Example:
#    ONLYOFFICE_JWT_SECRET=MyEduPlatform_SuperSecretKey_2024_ChangeThis!

# 4. (Optional but recommended) Add Arabic fonts
#    Copy Cairo.ttf, Tajawal.ttf, etc. into:
#    ./onlyoffice/fonts/
#    ONLYOFFICE will pick them up automatically

# 5. Create the storage directory
mkdir -p storage

================================================================
 STEP 3 — Start everything
================================================================

docker compose up -d

# First run downloads images — takes 5-10 minutes
# Subsequent runs start in ~10 seconds

# Check all 3 containers are running:
docker compose ps

# You should see:
#   edu_onlyoffice   running   0.0.0.0:8080->80/tcp
#   edu_api          running   0.0.0.0:5000->80/tcp
#   edu_vue          running   0.0.0.0:3000->80/tcp

================================================================
 STEP 4 — Open the app
================================================================

Open your browser:
  http://localhost:3000

That's it. Upload a PPTX or DOCX and click preview.

For other machines on same network:
  http://YOUR_SERVER_IP:3000

================================================================
 USEFUL COMMANDS
================================================================

# View logs
docker compose logs -f

# View ONLYOFFICE logs only
docker compose logs -f onlyoffice

# Stop everything
docker compose down

# Restart one service
docker compose restart dotnet_api

# Rebuild after code changes
docker compose up -d --build vue_frontend
docker compose up -d --build dotnet_api

# Full reset (WARNING: deletes all uploaded files)
docker compose down -v

================================================================
 TROUBLESHOOTING
================================================================

Problem: ONLYOFFICE shows "Download failed"
Fix: Check that dotnet_api is reachable from the ONLYOFFICE container
     docker compose exec onlyoffice curl http://dotnet_api:80/api/files

Problem: JWT error in ONLYOFFICE
Fix: Make sure ONLYOFFICE_JWT_SECRET in .env matches what .NET uses
     docker compose down && docker compose up -d

Problem: Arabic fonts not rendering correctly
Fix: Put .ttf files in ./onlyoffice/fonts/ and restart:
     docker compose restart onlyoffice

Problem: "File too large" on upload
Fix: Already set to 200MB in nginx.conf. For bigger files increase:
     client_max_body_size 500M;  in nginx.conf then rebuild vue_frontend

Problem: Port 8080 or 3000 already in use
Fix: Change ports in docker-compose.yml:
     - "9090:80"   instead of "8080:80" for ONLYOFFICE
     - "4000:80"   instead of "3000:80" for Vue
     Also update VITE_ONLYOFFICE_PUBLIC_URL in vue-frontend .env

================================================================
 GOING TO PRODUCTION 
================================================================

1. Set server IP in docker-compose.yml environment:
   - ONLYOFFICE__PublicUrl=http://YOUR_SERVER_IP:8080
   - Change Cors AllowedOrigins in appsettings.json

2. Add a .env file on each client machine pointing to server:
   VITE_ONLYOFFICE_PUBLIC_URL=http://YOUR_SERVER_IP:8080

3. Users open: http://YOUR_SERVER_IP:3000

================================================================
