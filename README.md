# TalentoPlus S.A.S. - Sistema de Gestión de Empleados

Sistema integral para la administración de recursos humanos desarrollado con ASP.NET Core 8.0 y PostgreSQL, implementando Clean Architecture.

**Repositorio:** https://github.com/BioHazard23/Prueba-.NET

**Link Drive Archivo Comprimido:** https://drive.google.com/drive/folders/1ymo4vxmDvVlg7AkL1imUrptDaqclGU1t?usp=drive_link

---

## Tabla de Contenidos

1. [Arquitectura del Proyecto](#arquitectura-del-proyecto)
2. [Tecnologías Utilizadas](#tecnologías-utilizadas)
3. [Requisitos Previos](#requisitos-previos)
4. [Instalación y Ejecución](#instalación-y-ejecución)
5. [Configuración de Variables de Entorno](#configuración-de-variables-de-entorno)
6. [Credenciales de Acceso](#credenciales-de-acceso)
7. [Documentación de la API](#documentación-de-la-api)
8. [Funcionalidades](#funcionalidades)

---

## Arquitectura del Proyecto

El proyecto está estructurado siguiendo los principios de Clean Architecture:

```
TalentoPlus/
├── src/
│   ├── TalentoPlus.Domain/          # Capa de dominio (Entidades, Enums, Interfaces)
│   ├── TalentoPlus.Application/     # Capa de aplicación (DTOs, Servicios)
│   ├── TalentoPlus.Infrastructure/  # Capa de infraestructura (EF Core, Repositorios)
│   ├── TalentoPlus.Web/             # Aplicación Web MVC (Administrador)
│   └── TalentoPlus.API/             # API REST (Empleados)
├── docker-compose.yml
└── TalentoPlus.sln
```

---

## Tecnologías Utilizadas

| Categoría | Tecnología |
|-----------|------------|
| Framework | ASP.NET Core 8.0 |
| Base de datos | PostgreSQL 16 |
| ORM | Entity Framework Core 8.0 |
| Autenticación Web | ASP.NET Core Identity |
| Autenticación API | JWT Bearer |
| Generación PDF | QuestPDF |
| Procesamiento Excel | ClosedXML |
| Envío de correos | MailKit (SMTP) |
| Inteligencia Artificial | Google Gemini API |
| Contenedores | Docker / Docker Compose |

---

## Requisitos Previos

- .NET 8.0 SDK
- Docker y Docker Compose (para ejecución containerizada)
- PostgreSQL 16 (si se ejecuta localmente sin Docker)

---

## Instalación y Ejecución

### Opción 1: Ejecución con Docker Compose (Recomendado)

Este método levanta toda la solución (base de datos, aplicación web y API) en contenedores.

```bash
# Clonar el repositorio
git clone https://github.com/BioHazard23/Prueba-.NET.git
cd Prueba-.NET

# Configurar variables de entorno
cp .env.example .env
# Editar el archivo .env con las credenciales correspondientes

# Construir y ejecutar los contenedores
docker compose up -d

# Verificar que los servicios estén corriendo
docker compose ps
```

**URLs de acceso:**
- Aplicación Web: http://localhost:5000
- API REST (Swagger): http://localhost:5001

### Opción 2: Ejecución en Desarrollo Local

```bash
# Clonar el repositorio
git clone https://github.com/BioHazard23/Prueba-.NET.git
cd Prueba-.NET

# Iniciar PostgreSQL (puede ser local o en Docker)
docker run -d \
  --name postgres \
  -e POSTGRES_USER=talentoadmin \
  -e POSTGRES_PASSWORD=TalentoPlus2024! \
  -e POSTGRES_DB=talentoplusdb \
  -p 5432:5432 \
  postgres:16-alpine

# Restaurar dependencias
dotnet restore

# Aplicar migraciones a la base de datos
cd src/TalentoPlus.Web
dotnet ef database update

# Ejecutar la aplicación Web (Terminal 1)
dotnet run --urls "http://localhost:5223"

# Ejecutar la API (Terminal 2)
cd ../TalentoPlus.API
dotnet run --urls "http://localhost:5221"
```

**URLs de acceso en desarrollo:**
- Aplicación Web: http://localhost:5223
- API REST (Swagger): http://localhost:5221

---

## Configuración de Variables de Entorno

Crear un archivo `.env` en la raíz del proyecto con las siguientes variables:

```env
# Base de datos
DB_USER=talentoadmin
DB_PASSWORD=TalentoPlus2024!
DB_NAME=talentoplusdb

# JWT (mínimo 32 caracteres)
JWT_SECRET=TalentoPlus2024SuperSecretKeyMinimoTreintaDosCaracteres!

# Configuración SMTP para envío de correos
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USER=correo@gmail.com
SMTP_PASSWORD=contraseña_de_aplicacion
SMTP_FROM=correo@gmail.com

# Configuración de IA
AI_PROVIDER=Gemini
AI_API_KEY=clave_api_de_gemini
```

### Obtención de Credenciales Externas

**Gmail SMTP:**
1. Acceder a la cuenta de Google
2. Activar verificación en dos pasos
3. Generar una "Contraseña de aplicación" en: https://myaccount.google.com/apppasswords

**Google Gemini API:**
1. Acceder a Google AI Studio: https://aistudio.google.com/app/apikey
2. Crear una nueva API Key

---

## Credenciales de Acceso

### Aplicación Web (Panel de Administración)

| Campo | Valor |
|-------|-------|
| URL (Docker) | http://localhost:5000 |
| URL (Local) | http://localhost:5223 |
| Email | admin@talentoplus.com |
| Contraseña | Admin123 |
| Rol | Administrador |

Nota: En caso de ser la primera ejecución, se debe registrar un usuario administrador desde la interfaz de login.

### API REST

| Campo | Valor |
|-------|-------|
| URL (Docker) | http://localhost:5001 |
| URL (Local) | http://localhost:5221 |
| Documentación Swagger | /swagger |

**Autenticación en la API:**

El login de empleados requiere documento y email:

```http
POST /api/Auth/login
Content-Type: application/json

{
  "documento": "43743727",
  "email": "luisa.castro40@correo.com"
}
```

La respuesta incluye un token JWT que debe enviarse en el header `Authorization: Bearer {token}` para acceder a los endpoints protegidos.

---

## Documentación de la API

### Endpoints Públicos

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | /api/Departamentos | Obtiene lista de departamentos |
| GET | /api/Departamentos/{id} | Obtiene departamento por ID |
| POST | /api/Auth/registro | Registro de nuevo empleado |
| POST | /api/Auth/login | Autenticación (retorna JWT) |

### Endpoints Protegidos (Requieren JWT)

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | /api/Empleados/me | Obtiene información del empleado autenticado |
| GET | /api/Empleados/me/hoja-vida | Descarga hoja de vida en PDF |

---

## Funcionalidades

### Aplicación Web (Administrador de RRHH)

- Autenticación mediante ASP.NET Core Identity
- Gestión completa de empleados (crear, editar, listar, eliminar)
- Importación masiva de empleados desde archivos Excel
- Generación dinámica de hojas de vida en formato PDF
- Dashboard con estadísticas del sistema
- Asistente de inteligencia artificial para consultas en lenguaje natural

### API REST (Empleados)

- Consulta de departamentos disponibles
- Autoregistro de empleados con envío de correo de bienvenida
- Autenticación mediante JWT
- Consulta de información personal
- Descarga de hoja de vida en PDF

---

## Contacto

Proyecto desarrollado como prueba técnica para TalentoPlus S.A.S.
