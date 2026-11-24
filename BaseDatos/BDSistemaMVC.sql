/* =============================================================
   Base de datos: BDSistemaMVC
   Casos: 1) Generar reserva de habitación  2) Generar requerimiento
   Curso: Diseño y Arquitectura de Software - UPN
   Ejecutar completo en SQL Server Management Studio (F5)
   ============================================================= */
USE master;
GO
IF DB_ID('BDSistemaMVC') IS NOT NULL
BEGIN
    ALTER DATABASE BDSistemaMVC SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE BDSistemaMVC;
END
GO
CREATE DATABASE BDSistemaMVC;
GO
USE BDSistemaMVC;
GO

/* ---------------- SEGURIDAD ---------------- */
CREATE TABLE Usuario (
    IdUsuario     INT IDENTITY(1,1) PRIMARY KEY,
    NombreUsuario VARCHAR(30)  NOT NULL UNIQUE,
    Clave         VARCHAR(50)  NOT NULL,
    NombreCompleto VARCHAR(100) NOT NULL,
    Rol           VARCHAR(20)  NOT NULL CHECK (Rol IN ('Recepcionista','Operario','Administrador')),
    CodOperario   CHAR(5)      NULL
);
GO

/* ---------------- CASO 1: RESERVA DE HABITACIÓN ---------------- */
CREATE TABLE Cliente (
    CodCliente CHAR(5)      PRIMARY KEY,          -- C0001
    Nombres    VARCHAR(60)  NOT NULL,
    Apellidos  VARCHAR(60)  NOT NULL,
    DNI        CHAR(8)      NULL,
    Telefono   VARCHAR(15)  NULL
);
GO

CREATE TABLE Habitacion (
    NumHabitacion VARCHAR(5)    PRIMARY KEY,      -- 101, 102...
    Tipo          VARCHAR(30)   NOT NULL,         -- Simple, Doble, Matrimonial, Suite
    CostoDia      DECIMAL(10,2) NOT NULL CHECK (CostoDia > 0),
    Estado        VARCHAR(15)   NOT NULL DEFAULT 'Disponible'
                  CHECK (Estado IN ('Disponible','Reservado','Ocupado'))
);
GO

CREATE TABLE Reserva (
    NumReserva    INT           PRIMARY KEY,      -- autogenerado: último + 1
    CodCliente    CHAR(5)       NOT NULL REFERENCES Cliente(CodCliente),
    FechaRegistro DATETIME      NOT NULL DEFAULT GETDATE(),
    FechaLlegada  DATE          NOT NULL,
    FechaSalida   DATE          NOT NULL,
    CantidadDias  INT           NOT NULL CHECK (CantidadDias > 0),
    SubTotal      DECIMAL(10,2) NOT NULL,
    IGV           DECIMAL(10,2) NOT NULL,
    MontoTotal    DECIMAL(10,2) NOT NULL,
    CONSTRAINT CK_Reserva_Fechas CHECK (FechaSalida > FechaLlegada)
);
GO

CREATE TABLE DetalleReserva (
    NumReserva    INT           NOT NULL REFERENCES Reserva(NumReserva),
    NumHabitacion VARCHAR(5)    NOT NULL REFERENCES Habitacion(NumHabitacion),
    NombreHuesped VARCHAR(100)  NOT NULL,
    CostoDia      DECIMAL(10,2) NOT NULL,
    Dias          INT           NOT NULL,
    MontoPagar    DECIMAL(10,2) NOT NULL,
    PRIMARY KEY (NumReserva, NumHabitacion)
);
GO

/* ---------------- CASO 2: GENERAR REQUERIMIENTO ---------------- */
CREATE TABLE Operario (
    CodOperario CHAR(5)     PRIMARY KEY,          -- OP001
    Nombres     VARCHAR(60) NOT NULL,
    Apellidos   VARCHAR(60) NOT NULL,
    Area        VARCHAR(40) NULL
);
GO

ALTER TABLE Usuario ADD CONSTRAINT FK_Usuario_Operario
    FOREIGN KEY (CodOperario) REFERENCES Operario(CodOperario);
GO

CREATE TABLE Material (
    CodMaterial  CHAR(6)     PRIMARY KEY,         -- MAT001
    Nombre       VARCHAR(80) NOT NULL,
    Marca        VARCHAR(40) NOT NULL,
    UnidadMedida VARCHAR(15) NOT NULL,
    Stock        INT         NOT NULL DEFAULT 0
);
GO

CREATE TABLE Requerimiento (
    NumRequerimiento INT      PRIMARY KEY,        -- autogenerado: último + 1
    CodOperario      CHAR(5)  NOT NULL REFERENCES Operario(CodOperario),
    Fecha            DATETIME NOT NULL DEFAULT GETDATE(),
    Estado           VARCHAR(15) NOT NULL DEFAULT 'Pendiente'
);
GO

CREATE TABLE DetalleRequerimiento (
    NumRequerimiento INT     NOT NULL REFERENCES Requerimiento(NumRequerimiento),
    CodMaterial      CHAR(6) NOT NULL REFERENCES Material(CodMaterial),
    Cantidad         INT     NOT NULL CHECK (Cantidad > 0),
    PRIMARY KEY (NumRequerimiento, CodMaterial)
);
GO

/* ---------------- DATOS DE PRUEBA ---------------- */
INSERT INTO Operario VALUES
('OP001','Carlos','Ramírez Soto','Carpintería'),
('OP002','María','Torres Vega','Acabados');

INSERT INTO Usuario (NombreUsuario, Clave, NombreCompleto, Rol, CodOperario) VALUES
('recepcion','123','Ana Lucía Paredes','Recepcionista',NULL),
('operario','123','Carlos Ramírez Soto','Operario','OP001'),
('admin','123','Administrador del sistema','Administrador','OP002');

INSERT INTO Cliente VALUES
('C0001','Juan','Pérez García','45781236','987654321'),
('C0002','Rosa','Mendoza Ríos','70123458','912345678'),
('C0003','Pedro','Quispe Huamán','41236589','955112233'),
('C0004','Lucía','Fernández Castro','72589631','944556677');

INSERT INTO Habitacion VALUES
('101','Simple',     80.00,'Disponible'),
('102','Simple',     80.00,'Disponible'),
('103','Doble',     120.00,'Disponible'),
('201','Doble',     120.00,'Ocupado'),
('202','Matrimonial',150.00,'Disponible'),
('203','Matrimonial',150.00,'Reservado'),
('301','Suite',     250.00,'Disponible'),
('302','Suite',     250.00,'Disponible');

INSERT INTO Material VALUES
('MAT001','Plancha de melamina 18 mm','Pelikano','Plancha',40),
('MAT002','Tornillo autorroscante 1 1/2"','Fixser','Caja',120),
('MAT003','Bisagra cangrejo 35 mm','Hettich','Unidad',300),
('MAT004','Corredera telescópica 45 cm','Hafele','Par',80),
('MAT005','Tapacanto PVC 22 mm','Rehau','Rollo',25),
('MAT006','Cola sintética 1 kg','Pegafan','Frasco',60),
('MAT007','Tirador de aluminio 128 mm','Hafele','Unidad',150);

/* Reserva y requerimiento de ejemplo (para que el siguiente número sea el 2) */
INSERT INTO Reserva VALUES (1,'C0002',GETDATE(),'2026-11-20','2026-11-22',2,300.00,54.00,354.00);
INSERT INTO DetalleReserva VALUES (1,'203','Rosa Mendoza Ríos',150.00,2,300.00);
INSERT INTO Requerimiento VALUES (1,'OP001',GETDATE(),'Pendiente');
INSERT INTO DetalleRequerimiento VALUES (1,'MAT001',5),(1,'MAT003',20);
GO

SELECT 'Base de datos BDSistemaMVC creada correctamente' AS Mensaje;
GO
