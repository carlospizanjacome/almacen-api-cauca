# Almacen API - Sistema Inventario Educativo

API REST para la aplicación móvil del Sistema de Inventario y Almacén Educativo.

Cliente: Alcaldía de Popayán / Gobernación del Cauca

## Deploy

Desplegado automáticamente en Railway desde la rama `main`.

## Endpoints principales

- POST /api/auth/login
- GET /api/bienes
- POST /api/sync/kardex
- GET /health

## Variables de entorno requeridas

- `ConnectionStrings__MobileDb` - Connection string a Neon PostgreSQL
- `Jwt__Key` - Clave secreta para firmar JWT (mín 32 caracteres)
- `Jwt__Issuer` - Emisor del JWT
- `Jwt__Audience` - Audiencia del JWT
- `Jwt__AccessTokenMinutos` - Duración del token en minutos (default 60)