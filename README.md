# Sistema MVC – Reserva de habitación y Generar requerimiento

Prototipo web del curso **Diseño y Arquitectura de Software** (UPN).
Estudiante: **Luis Obregón Hidalgo** – Docente: Robinson Manuel Yáñez Romero.

Implementa dos casos de uso con el patrón **MVC** en **ASP.NET Core 8 (C#)** y **SQL Server**:

| Caso de uso | Actor | Vista | Controlador |
|---|---|---|---|
| Generar reserva de habitación | Recepcionista | `Views/Reserva/Index.cshtml` | `ReservaController` |
| Generar requerimiento | Operario | `Views/Requerimiento/Index.cshtml` | `RequerimientoController` |

## Estructura

```
BaseDatos/BDSistemaMVC.sql      Script de la base de datos con datos de prueba
Documentacion/                  Word con diagramas de secuencia y comunicación + imágenes
SistemaMVC/
  Models/                       Entidades: Cliente, Habitacion, Reserva, DetalleReserva,
                                Material, Requerimiento, DetalleRequerimiento, Usuario
  Data/                         Acceso a datos (ADO.NET): *DAO.cs
  ViewModels/                   Estado de cada interfaz
  Controllers/                  Reserva, Requerimiento, Cliente, Account (login), Home (menú)
  Views/                        Vistas Razor (Bootstrap 5)
```

## Cómo ejecutarlo

1. Abrir **SQL Server Management Studio** y ejecutar `BaseDatos/BDSistemaMVC.sql` (crea la BD `BDSistemaMVC`).
2. Revisar la cadena de conexión en `SistemaMVC/appsettings.json`.
   Por defecto usa `Server=localhost` con autenticación de Windows. Si usas SQL Express cambia a `Server=localhost\\SQLEXPRESS`.
3. Abrir `SistemaMVC.sln` en **Visual Studio 2022** (con .NET 8) y presionar **F5**.
4. Iniciar sesión (clave de todos: `123`):
   - `recepcion` → Recepcionista (Generar Reserva)
   - `operario` → Operario (Generar Requerimiento, código OP001 precargado)
   - `admin` → ambas opciones

## Flujo implementado

**Generar reserva:** buscar cliente (por código o desde la lista) → si no existe, “Cliente no existe” y opción de registrarlo →
ingresar fechas y calcular días → buscar habitación disponible → nombre del huésped → agregar (calcula pago, subtotal, IGV 18 % y total) →
repetir → eliminar de la cuadrícula si hace falta → **Grabar**: autogenera el número (último + 1), graba reserva y detalle y cambia las habitaciones a **Reservado** → “Reserva generada con el Nro. X”.

**Generar requerimiento:** código de operario precargado → consultar materiales → seleccionar materiales y cantidades →
eliminar / limpiar → **Generar Requerimiento**: autogenera el número y graba → “Requerimiento generado con el Nro. X”.
