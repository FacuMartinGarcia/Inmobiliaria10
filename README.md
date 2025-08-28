📘 Proyecto Inmobiliaria10

Sistema web para la gestión de propiedades en alquiler desarrollado en ASP.NET Core MVC con base de datos MySQL/MariaDB.
Permite administrar Propietarios, Inquilinos, Inmuebles y Contratos de alquiler, incluyendo ABM completo, validaciones y reglas de negocio (como evitar solapamiento de contratos).

🚀 Tecnologías utilizadas

Backend: ASP.NET Core 7 / C#

Frontend: Razor Pages (cshtml), Bootstrap 5, Font Awesome

ORM: Entity Framework Core

Base de Datos: MySQL / MariaDB

IDE recomendado: Visual Studio 2022 o VS Code con extensión C#

📂 Estructura del proyecto
Inmobiliaria10/
│
├── Controllers/        # Controladores MVC
│   ├── PropietarioController.cs
│   ├── InquilinoController.cs
│   ├── ContratoController.cs
│   └── InmuebleController.cs
│
├── Models/             # Entidades de dominio
│   ├── Propietario.cs
│   ├── Inquilino.cs
│   ├── Inmueble.cs
│   └── Contrato.cs
│
├── Data/               
│   ├── AppDbContext.cs # Configuración EF Core
│   └── Repositories/   # Patrón repositorio
│       ├── IContratoRepo.cs
│       └── ContratoRepo.cs
│
├── Views/              # Vistas Razor
│   ├── Shared/         # Layout y partials
│   └── Propietario/    # CRUD Propietario
│
├── wwwroot/            # Archivos estáticos (CSS, JS, imágenes)
│   └── css/site.css    # Estilos personalizados (form-card, form-title, etc.)
│
└── README.md

⚙️ Configuración inicial
1. Clonar el repo
git clone https://github.com/tuusuario/Inmobiliaria10.git
cd Inmobiliaria10

2. Configurar la base de datos

Crear una base de datos MySQL:

CREATE DATABASE inmobiliaria10 CHARACTER SET utf8mb4 COLLATE utf8mb4_spanish_ci;


Configurar la conexión en appsettings.json:

"ConnectionStrings": {
  "DefaultConnection": "server=localhost;port=3306;database=inmobiliaria10;user=root;password=tu_password;"
}

3. Aplicar migraciones
dotnet ef database update

4. Ejecutar la aplicación
dotnet watch run


La app estará disponible en https://localhost:5265.

📋 Funcionalidades principales

✅ Gestión de Propietarios (alta, baja, modificación, listado).
✅ Gestión de Inquilinos.
✅ Gestión de Inmuebles.
✅ Gestión de Contratos:

Validación de fechas de inicio/fin.

Control de rescisión.

Evita contratos solapados en el mismo inmueble.

Soft delete (con DeletedAt y DeletedBy).

✅ Formularios unificados con estilos modernos (form-card, form-title, btn-save, btn-back).
✅ Paginación y búsqueda en listados.

🎨 Estilos personalizados

Se incluye un theme CSS en wwwroot/css/site.css con:

.form-card: fondo sólido, bordes redondeados, sombra.

.form-title: título centrado y destacado.

.btn-save, .btn-back: botones de acción consistentes en todos los formularios.

👥 Autores

Ricchiardi, Maria Romanela y Garcia Facundo – Desarrollo backend/frontend y diseño de base de datos.

