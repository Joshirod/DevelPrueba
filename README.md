# 📝 Plataforma de Encuestas en ASP.NET Core MVC

Aplicación web para crear, compartir y gestionar encuestas dinámicas. Los usuarios autenticados pueden definir formularios con campos personalizados, generar un enlace público para responder y consultar resultados almacenados en base de datos.

---

## 🚀 Características principales

- Autenticación con Identity (Login/Logout)
- CRUD de Encuestas
- Campos dinámicos con soporte a:
  - Texto
  - Número
  - Fecha
  - (Extensible a más tipos)
- Ordenar campos con **Drag & Drop**
- Duplicar encuestas con un clic
- Generación de link público para responder
- Llenado de encuestas sin login
- Validaciones por tipo y campo requerido
- Almacenamiento de respuestas por campo
- Exportar resultados a CSV
- Diseño básico con Bootstrap 5

---

## 🧱 Tecnologías utilizadas

- **ASP.NET Core MVC (.NET 8)**
- **Entity Framework Core + SQL Server**
- **ASP.NET Identity**
- Bootstrap 5
- LINQ / EF Migrations

---

## 📦 Requerimientos

- .NET 8 SDK
- SQL Server (local o remoto)
- Visual Studio / VS Code
- (Opcional) Azure App Service para despliegue

---

## 🛠️ Configuración local

1) Clonar el repo  
```bash
git clone https://github.com/Joshirod/DevelPrueba.git
cd DevelPrueba
```
2) Ajustar cadena de conexión en appsettings.json
```
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=EncuestasDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```
3) Crear base y aplicar migraciones
```
dotnet ef database update
```

4) Ejecutar
```
dotnet run
```
