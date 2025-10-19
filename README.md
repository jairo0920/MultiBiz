# MultiBiz (.NET 8 + Blazor WASM + ASP.NET API + EF Core + SQL Server 2019)

**Multi-tenant por subdominio** con QueryFilter global, tema Bootstrap responsive amigable para touch, módulos Restaurante (KDS + ticket 80mm) y Barbería (citas), CRUD básico de Tenants/Usuarios, Swagger listo.

## Requisitos
- Windows Server 2025 + Visual Studio 2022
- .NET 8 SDK
- SQL Server 2019

## Ejecutar en desarrollo
1. Configure `MultiBiz.Server/appsettings.json` (cadena de conexión, JWT, SMTP, WhatsApp).
2. En la carpeta raíz:
   ```bash
   dotnet restore
   dotnet build
   dotnet ef migrations add Init --project MultiBiz.Server --startup-project MultiBiz.Server
   dotnet ef database update --project MultiBiz.Server --startup-project MultiBiz.Server
   dotnet run --project MultiBiz.Server
   ```
3. Abra Swagger en `/swagger`.

> **Nota**: Para multitenancy por subdominio, cree registros DNS como `barberia01.su-dominio.com` apuntando al host. El middleware resuelve `Subdomain` y fija el `TenantId` automáticamente.

## Publicar en DigitalOcean
- Con Docker + Compose:
  ```bash
  docker compose up -d --build
  ```
- Configure un A/AAAA y (opcional) Cloudflare Tunnel.
- Cambie `ConnectionStrings__DefaultConnection` y secretos.

## Pagos y mensajería
- SINPE/transferencia/efectivo/tarjeta: integre gateways específicos en `Server/Controllers/RestaurantController.cs` (stubs).
- WhatsApp Meta Cloud API: configure token/phone id y use `IWhatsAppSender`.

## Pendientes sugeridos
- Roles detallados por módulo y permisos por ruta.
- Recuperación de contraseña real (token + enlace).
- Calendario visual (librería JS) en Barbería.
- Impresora térmica 80mm vía WebUSB/WebSerial/EscPos.
- KDS en tiempo real con SignalR.
