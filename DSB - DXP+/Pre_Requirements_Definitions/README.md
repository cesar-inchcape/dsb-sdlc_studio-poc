# Pre-Requirements Definitions

Documentación técnica completa del proyecto **Digital Service Booking (DXP+)**.

---

## 📁 Contenido

### `architecture/` — Blueprints Técnicos

**Backend:** `backend_blueprint.md`
- .NET 10 + ASP.NET Core Web API
- Entity Framework Core + SQL Server Stored Procedures
- Vertical Slice Architecture
- Microservices: Login.Api, Booking.Api, Management.Api

**Frontend:** `frontend_blueprint.md`
- React 18 + TypeScript + Vite
- Atomic Design Pattern
- Axios para HTTP

---

### `application_design/` — Módulos

**Login/**
- `api_login_backend.md` — API de autenticación
- `view-login_frontend.md` — Vista de login
- `figma_login/` — Diseños Figma

**Management/**
- `Advisors_module/` — Gestión de asesores (API + Vista + Figma)
- `Users_module/` — Gestión de usuarios (API + Vista + Figma)

**Otros:**
- `db_schema.txt` — Esquema de base de datos
- `testing_requirements.md` — Lineamientos de testing

---

### `acceptance_criteria/` — Tickets JIRA

**Users and Roles:**
- `DXPOWN-3812.md` — Super Administrator (control total)
- `DXPOWN-3813.md` — Distributor Administrator (scope restringido)
- `DXPOWN-3814.md` — Workshop User (solo lectura)
- `DXPOWN-3815.md` — Retail Group User (multi-workshop)

---

### `qa_guidelines/` — Testing

Estándares de calidad y requerimientos de testing (Unit Tests + API Tests obligatorios).

---

## 🔄 Uso con SDLC Studio

Estos documentos son el **input** para SDLC Studio:

```bash
/sdlc-studio prd generate    # Analiza esta carpeta → genera PRD
/sdlc-studio trd generate    # Extrae TRD desde blueprints
/sdlc-studio epic            # Descompone en Epics
/sdlc-studio story           # Crea User Stories
```

**Resultado:** `../sdlc-studio/` con PRD, TRD, Epics, Stories estructurados.

---

## ⚠️ Orden de Implementación

1. **Backend primero** — El Frontend depende de APIs activas
2. **Tests obligatorios** — No PR sin tests (ver `qa_guidelines/`)
3. **Nombres en inglés** — Coherencia con arquitectura
