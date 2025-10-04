# 🏠 Sistema Inmobiliario

**InnovaPropiedades** es una aplicación web desarrollada en **ASP.NET Core MVC con Entity Framework Core y MySQL/MariaDB**, diseñada para gestionar propietarios, inquilinos, inmuebles y contratos de alquiler. El sistema ofrece una experiencia fluida para la administración de la cartera inmobiliaria, adaptándose a escenarios reales.
---
## 🚀 Tecnologías utilizadas

- **ASP.NET Core 7 / C#**
- **Entity Framework Core (ORM)**
- **MySQL / MariaDB** 
- **Bootstrap 5** + **Font Awesome**
- **Razor (cshtml)** 
- **Patrón Repository (LINQ)** 
- **bcrypt** (encriptación de contraseñas)
- **Soft Delete en contratos** 

## 📑 Tabla de Contenidos

1. [Introducción](#-introducción)  
2. [Características](#-características)  
3. [Arquitectura](#-arquitectura)  
4. [Requisitos](#-requisitos)  
5. [Instalación](#-instalación)  
6. [Estructura del Proyecto](#-estructura-del-proyecto)  
7. [Uso](#-uso)  
8. [Credenciales de Acceso](#-credenciales-de-acceso)  
9. [Licencia](#-licencia)  

---

## 📌 Introducción

**Sistema InnovaPropiedades**  permite:

- **Registrar y gestionar propietarios e inquilinos.**  
- **Administrar inmuebles**, con disponibilidad y características. 
- **Gestionar contratos** con validaciones de fechas, rescisión y control de solapamientos. 
- **Controlar el ciclo de vida de contratos**, (vigentes, rescindidos, finalizados).

Construido bajo arquitectura MVC, utiliza Entity Framework Core para la persistencia y Bootstrap 5 para la interfaz moderna y responsiva.
---

## 🚀 Características

- ✅ **Gestión de Propietarios e Inquilinos**

- ✅ **Gestión de Inmuebles** con disponibilidad

- ✅ **Contratos con validación de fechas**

- ✅ **Prevención de contratos solapados en un mismo inmueble** 

- ✅ **Sistema de Usuarios con Roles** (administrador, empleado.)

- ✅ **Soft Delete en contratos** (con DeletedAt, DeletedBy)

- ✅ **Interfaz Web con Bootstrap**

- ✅ **Autenticación y Sesiones**: alta, baja, edición de usuarios.  
  Control de acceso según rol.

- ✅ **Base de Datos MySQL** usando Sequelize

- ✅ **Recuperación de contraseña** con envío de token por correo.

- ✅ **Paginación y búsqueda en listados** 

---

## 🏗️ Arquitectura

El proyecto está organizado en una estructura **MVC**:

- **Modelos (EF Core):** Propietario, Inquilino, Inmueble, Contrato

- **Controladores:** lógica de negocio desacoplada, conexión con modelos.

- **Vistas (Razor):** componentes y formularios renderizados desde el servidor.

- **Repositorio:** acceso a datos mediante Repository + LINQ.

- **DbContext:** configuración de EF Core y mapeos a MySQL/MariaDB.

---

## ⚙️ Requisitos

- .NET 7 SDK
- MySQL/MariaDB 
- Git  
- Editor de texto (recomendado: VS Code)  

---

## 📥 Instalación

### 1️⃣ Clonar este repositorio


git clone https://github.com/FacuMartinGarcia/Inmobiliaria10.git

cd Inmobiliaria10


### 2️⃣ Instalar dependencias


dotnet restore


### 3️⃣ Configurar entorno 

Editar appsettings.json con la cadena de conexión:

{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=inmobiliaria10;user=root;password=tu_password;"
  }
}


### 4️⃣ Inicializar base de datos

Ejecutar el archivo SQL:

```sql
BD/inmogenial.sql
```

Asegúrate de tener creado el schema `inmogenial`.

---

## 📂 Estructura del Proyecto

```
Inmobiliaria10/
│
├── Data/                         # Configuración de EF Core y repos
│   ├── AppDbContext.cs
│   └── Repositories/
│       ├── IContratoRepo.cs
│       └── ContratoRepo.cs
│
├── Models/                       # Entidades de dominio
│   ├── Propietario.cs
│   ├── Inquilino.cs
│   ├── Inmueble.cs
│   └── Contrato.cs
│
├── Controllers/                  # Lógica de negocio (MVC)
│   ├── PropietarioController.cs
│   ├── InquilinoController.cs
│   ├── InmuebleController.cs
│   └── ContratoController.cs
│
├── Views/                        # Vistas Razor (cshtml)
│   ├── Shared/                   # Layout, partials
│   ├── Propietario/
│   ├── Inquilino/
│   ├── Inmueble/
│   └── Contrato/
│
├── wwwroot/                      # JS, CSS, imágenes
│   └── css/site.css              # Estilos (form-card, form-title, etc.)
│
├── appsettings.json              # Configuración (conexión a BD)
└── Program.cs / Startup.cs       # Punto de entrada

```

---

## ▶️ Uso

```
dotnet run
```
Luego accede a: https://localhost:5265

---

## 🔐 Credenciales de Acceso

| Usuario  | Contraseña | Tipo          |
|----------|------------|---------------|
| admin    | admin      | Administrador |
| RamonC   | RamonC     | Empleado      |


---
## 👨‍💻 Autor

Romanela Ricchiardi - Backend/Frontend & DB-- Contacto: roma.ricchiardi@gmail.com
Facundo Garcia – Backend/Frontend & DB-- Contacto:

Carrera: Desarrollo de Software – Universidad de La Punta

Materia: Laboratorio de Programacion II (2025)

## 🖼️ Galería del Sistema

👥 Propietarios e Inquilinos


📑 Gestión de Contratos


## 📜 Licencia

Este proyecto está licenciado bajo la Licencia MIT. Puedes usarlo, modificarlo y distribuirlo libremente bajo los términos de esta licencia.
