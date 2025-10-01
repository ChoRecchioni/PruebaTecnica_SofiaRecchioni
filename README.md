Prueba Técnica - API de Productos y Órdenes

Este proyecto es una API REST desarrollada con .NET 7.0 + Entity Framework Core que permite gestionar productos y órdenes de compra con descuentos automáticos.

Características:
-CRUD de Productos
-CRUD de Órdenes, con:
  • Relación con productos (OrdenProductos)
  • Cálculo automático del total aplicando reglas de descuento:
    • 10% si el subtotal supera los 500
    • 5% adicional si hay más de 5 productos distintos
  • Persistencia en base de datos SQL Server usando EF Core
  • Documentación interactiva con Swagger

Requisitos:
- .NET 8 SDK
- SQL Server Express o superior
- Git 

Instalación y Configuración:
- Clonar el repositorio
git clone https://github.com/ChoRecchioni/PruebaTecnica_SofiaRecchioni.git
cd PruebaTecnica_SofiaRecchioni

-Configurar la conexión a la base de datos
Editar appsettings.json con tus credenciales de SQL Server:

"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=OrdenesDb;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;"
}

-Crear la base de datos y aplicar migraciones
dotnet ef migrations add InitialCreate
dotnet ef database update

-Ejecutar el proyecto
dotnet run

-Acceder a la documentación Swagger en:
https://localhost/swagger

Endpoints:
Productos
GET /api/productos → Listar todos los productos
GET /api/productos/{id} → Obtener un producto por ID
POST /api/productos → Crear un producto
PUT /api/productos/{id} → Actualizar un producto
DELETE /api/productos/{id} → Eliminar un producto

Órdenes
GET /api/ordenes → Listar todas las órdenes
GET /api/ordenes/{id} → Obtener una orden con detalle de productos
POST /api/ordenes → Crear una orden 
PUT /api/ordenes/{id} → Actualizar una orden
DELETE /api/ordenes/{id} → Eliminar una orden
