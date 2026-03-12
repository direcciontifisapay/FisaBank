# FisaBank API v1 — Entorno de Laboratorio

> ⚠️ **API deliberadamente vulnerable. NO usar en producción. NO usar datos reales.**

Desarrollada por **Fisapay** para el programa *"Desarrollo Seguro de APIs"*  
dictado a **Toyota Financial Services Colombia (TFSCO)** — 2026.

---

## Requisitos

| Herramienta | Versión |
|---|---|
| .NET SDK | **8.0** → https://dotnet.microsoft.com/download/dotnet/8 |
| Visual Studio 2022 | 17.8+ (opcional) |
| VS Code + C# Dev Kit | (opcional) |
| Postman | Cualquier versión |

---

## Arrancar en 3 pasos

```bash
# 1. Clonar
git clone https://github.com/<org>/FisaBank.git
cd FisaBank

# 2. Compilar
dotnet build

# 3. Arrancar
cd src/FisaBank.Api
dotnet run
```

La primera vez crea `fisabank.db` y siembra los datos automáticamente.

| URL | |
|---|---|
| `https://localhost:5001/swagger` | Swagger UI |
| `http://localhost:5000/swagger`  | HTTP (redirige) |

---

## Usuarios de prueba

| Rol | Email | Password |
|---|---|---|
| Usuario | `carlos@tfsco.com` | `Abc123!` |
| Usuario | `laura@tfsco.com` | `Xyz789!` |
| Admin | `admin@fisabank.com` | `Admin@2026!` |

---

## Vulnerabilidades (resumen)

| ID | Nombre | Dificultad |
|---|---|---|
| VULN-01 | BOLA — Object Level Authorization | Fácil |
| VULN-02 | BFLA — Function Level Authorization | Fácil |
| VULN-03 | Mass Assignment | Fácil |
| VULN-04 | JWT alg:none | Media |
| VULN-05 | Datos sensibles en JWT y respuestas | Fácil |
| VULN-06 | Errores verbosos con StackTrace | Fácil |
| VULN-07 | Sin rate limiting | Fácil |
| VULN-08 | CORS abierto | Media |
| VULN-09 | Timing Attack en login | **Difícil** |
| VULN-10 | Actualización silenciosa de campos + comparación hash incorrecta | **Difícil** |

Detalles completos en [`docs/VULNERABILITIES.md`](docs/VULNERABILITIES.md) — **solo instructor**.

---

## Importar colección Postman

Importa `postman/FisaBank_v1.postman_collection.json`.  
Incluye requests normales y carpeta **Lab Attacks** con exploits preconfigurados.

---

## Abrir en Visual Studio

Doble clic en `FisaBank.sln` → Ejecutar con perfil `FisaBank.Api (https)`.

## Abrir en VS Code

```bash
code FisaBank/
# Instalar extensión C# Dev Kit si no la tienes
# F5 para depurar
```

---

© 2026 Fisapay S.A.S. — Material educativo propietario. Uso exclusivo TFSCO.
