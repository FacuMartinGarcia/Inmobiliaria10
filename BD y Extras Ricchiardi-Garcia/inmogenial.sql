-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 05-10-2025 a las 15:39:07
-- Versión del servidor: 10.4.32-MariaDB
-- Versión de PHP: 8.0.30

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de datos: `inmogenial`
--
CREATE DATABASE IF NOT EXISTS `inmogenial` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE `inmogenial`;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `conceptos`
--

CREATE TABLE `conceptos` (
  `id_concepto` int(10) UNSIGNED NOT NULL,
  `denominacion_concepto` varchar(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_spanish_ci;

--
-- Volcado de datos para la tabla `conceptos`
--

INSERT INTO `conceptos` (`id_concepto`, `denominacion_concepto`) VALUES
(3, 'Expensas'),
(1, 'Mes Alquiler'),
(2, 'Multa');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `contratos`
--

CREATE TABLE `contratos` (
  `id_contrato` int(10) UNSIGNED NOT NULL,
  `fecha_firma` date DEFAULT NULL,
  `id_inmueble` int(10) UNSIGNED NOT NULL,
  `id_inquilino` int(10) UNSIGNED NOT NULL,
  `fecha_inicio` date NOT NULL,
  `fecha_fin` date NOT NULL,
  `monto_mensual` decimal(15,2) NOT NULL DEFAULT 1.00 CHECK (`monto_mensual` > 0),
  `rescision` date DEFAULT NULL,
  `monto_multa` decimal(15,2) DEFAULT NULL,
  `created_by` int(10) UNSIGNED NOT NULL,
  `updated_by` int(11) DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT current_timestamp(),
  `deleted_at` datetime DEFAULT NULL,
  `deleted_by` int(10) UNSIGNED DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_spanish_ci;

--
-- Volcado de datos para la tabla `contratos`
--

INSERT INTO `contratos` (`id_contrato`, `fecha_firma`, `id_inmueble`, `id_inquilino`, `fecha_inicio`, `fecha_fin`, `monto_mensual`, `rescision`, `monto_multa`, `created_by`, `updated_by`, `created_at`, `deleted_at`, `deleted_by`) VALUES
(1, NULL, 1, 11, '2025-09-05', '2026-09-05', 12000000.00, '2025-09-17', 240000.00, 1, NULL, '2025-09-05 18:30:25', NULL, NULL),
(2, NULL, 1, 19, '2025-09-25', '2026-09-25', 120000.00, NULL, NULL, 2, NULL, '2025-09-25 19:22:09', NULL, NULL),
(3, '2025-09-08', 2, 14, '2025-09-25', '2026-09-25', 1700000.00, NULL, NULL, 2, NULL, '2025-09-25 19:23:53', NULL, NULL),
(4, NULL, 3, 10, '2025-02-27', '2026-09-27', 3000000.00, NULL, NULL, 2, NULL, '2025-09-27 14:51:40', NULL, NULL),
(5, NULL, 4, 4, '2025-09-27', '2026-09-27', 345666434.00, NULL, NULL, 2, NULL, '2025-09-27 20:52:42', NULL, NULL),
(6, NULL, 5, 8, '2025-09-27', '2026-09-27', 2432342.00, NULL, NULL, 2, NULL, '2025-09-27 20:58:32', NULL, NULL),
(7, '2029-11-30', 4, 14, '2029-11-30', '2031-11-30', 345666434.00, NULL, NULL, 5, NULL, '2025-10-01 00:38:33', NULL, NULL),
(8, '2025-09-30', 5, 19, '2029-12-29', '2031-07-31', 2432342.00, NULL, NULL, 5, NULL, '2025-10-01 01:28:37', NULL, NULL),
(9, '2025-10-05', 6, 8, '2025-10-05', '2026-10-05', 850000.00, NULL, NULL, 1, NULL, '2025-10-05 13:21:28', NULL, NULL);

--
-- Disparadores `contratos`
--
DELIMITER $$
CREATE TRIGGER `trg_contratos_delete` AFTER UPDATE ON `contratos` FOR EACH ROW BEGIN
    -- Solo auditar cuando se hace un soft delete
    IF NEW.deleted_at IS NOT NULL AND OLD.deleted_at IS NULL THEN
        INSERT INTO contratos_audit (id_contrato, accion, accion_at, accion_by, old_data)
        VALUES (
            NEW.id_contrato,
            'DELETE',
            NOW(),
            NEW.deleted_by,
            JSON_OBJECT(
                'id_inquilino', OLD.id_inquilino,
                'id_inmueble', OLD.id_inmueble,
                'fecha_inicio', OLD.fecha_inicio,
                'fecha_fin', OLD.fecha_fin,
                'monto_mensual', OLD.monto_mensual
            )
        );
    END IF;
END
$$
DELIMITER ;
DELIMITER $$
CREATE TRIGGER `trg_contratos_insert` AFTER INSERT ON `contratos` FOR EACH ROW BEGIN
    INSERT INTO contratos_audit (id_contrato, accion, accion_at, accion_by, new_data)
    VALUES (
        NEW.id_contrato,
        'INSERT',
        NOW(),
        NEW.created_by,
        JSON_OBJECT(
            'id_inquilino', NEW.id_inquilino,
            'id_inmueble', NEW.id_inmueble,
            'fecha_inicio', NEW.fecha_inicio,
            'fecha_fin', NEW.fecha_fin,
            'monto_mensual', NEW.monto_mensual
        )
    );
END
$$
DELIMITER ;
DELIMITER $$
CREATE TRIGGER `trg_contratos_update` AFTER UPDATE ON `contratos` FOR EACH ROW BEGIN
    -- Solo auditar si no es un borrado lógico
    IF NEW.deleted_at IS NULL THEN
        INSERT INTO contratos_audit (id_contrato, accion, accion_at, accion_by, old_data, new_data)
        VALUES (
            NEW.id_contrato,
            'UPDATE',
            NOW(),
            NEW.updated_by,
            JSON_OBJECT(
                'id_inquilino', OLD.id_inquilino,
                'id_inmueble', OLD.id_inmueble,
                'fecha_inicio', OLD.fecha_inicio,
                'fecha_fin', OLD.fecha_fin,
                'monto_mensual', OLD.monto_mensual
            ),
            JSON_OBJECT(
                'id_inquilino', NEW.id_inquilino,
                'id_inmueble', NEW.id_inmueble,
                'fecha_inicio', NEW.fecha_inicio,
                'fecha_fin', NEW.fecha_fin,
                'monto_mensual', NEW.monto_mensual
            )
        );
    END IF;
END
$$
DELIMITER ;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `contratos_audit`
--

CREATE TABLE `contratos_audit` (
  `id_audit` bigint(20) UNSIGNED NOT NULL,
  `id_contrato` int(10) UNSIGNED NOT NULL,
  `accion` varchar(12) NOT NULL,
  `accion_at` datetime NOT NULL DEFAULT current_timestamp(),
  `accion_by` int(10) UNSIGNED DEFAULT NULL,
  `old_data` longtext DEFAULT NULL,
  `new_data` longtext DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_spanish_ci;

--
-- Volcado de datos para la tabla `contratos_audit`
--

INSERT INTO `contratos_audit` (`id_audit`, `id_contrato`, `accion`, `accion_at`, `accion_by`, `old_data`, `new_data`) VALUES
(1, 6, 'INSERT', '2025-09-27 17:58:32', 2, NULL, '{\"id_inquilino\": 8, \"id_inmueble\": 5, \"fecha_inicio\": \"2025-09-27\", \"fecha_fin\": \"2026-09-27\", \"monto_mensual\": 2432342.00}'),
(2, 7, 'INSERT', '2025-09-30 21:38:33', 5, NULL, '{\"id_inquilino\": 14, \"id_inmueble\": 4, \"fecha_inicio\": \"2029-11-30\", \"fecha_fin\": \"2031-11-30\", \"monto_mensual\": 345666434.00}'),
(3, 3, 'UPDATE', '2025-09-30 21:39:49', 2, '{\"id_inquilino\": 14, \"id_inmueble\": 2, \"fecha_inicio\": \"2025-09-25\", \"fecha_fin\": \"2026-09-25\", \"monto_mensual\": 17000000.00}', '{\"id_inquilino\": 14, \"id_inmueble\": 2, \"fecha_inicio\": \"2025-09-25\", \"fecha_fin\": \"2026-09-25\", \"monto_mensual\": 1700000.00}'),
(4, 8, 'INSERT', '2025-09-30 22:28:37', 5, NULL, '{\"id_inquilino\": 19, \"id_inmueble\": 5, \"fecha_inicio\": \"2029-12-29\", \"fecha_fin\": \"2031-07-31\", \"monto_mensual\": 2432342.00}'),
(5, 1, 'UPDATE', '2025-09-30 22:28:53', NULL, '{\"id_inquilino\": 11, \"id_inmueble\": 1, \"fecha_inicio\": \"2025-09-05\", \"fecha_fin\": \"2026-09-05\", \"monto_mensual\": 120000.00}', '{\"id_inquilino\": 11, \"id_inmueble\": 1, \"fecha_inicio\": \"2025-09-05\", \"fecha_fin\": \"2026-09-05\", \"monto_mensual\": 12000000.00}'),
(6, 9, 'INSERT', '2025-10-05 10:21:28', 1, NULL, '{\"id_inquilino\": 8, \"id_inmueble\": 6, \"fecha_inicio\": \"2025-10-05\", \"fecha_fin\": \"2026-10-05\", \"monto_mensual\": 850000.00}');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `imagenes`
--

CREATE TABLE `imagenes` (
  `id_imagen` int(10) UNSIGNED NOT NULL,
  `id_inmueble` int(10) UNSIGNED NOT NULL,
  `ruta` varchar(255) NOT NULL,
  `created_at` datetime NOT NULL DEFAULT current_timestamp(),
  `updated_at` datetime NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `imagenes`
--

INSERT INTO `imagenes` (`id_imagen`, `id_inmueble`, `ruta`, `created_at`, `updated_at`) VALUES
(1, 4, '/Uploads/Inmuebles/4/22190f83-c5b1-4b4d-b0f9-4627c2285b92.jpg', '2025-10-05 09:49:08', '2025-10-05 09:49:08'),
(2, 4, '/Uploads/Inmuebles/4/df7b3328-2dd3-4ee8-afc8-b7298d97f25b.jpg', '2025-10-05 09:49:08', '2025-10-05 09:49:08'),
(3, 4, '/Uploads/Inmuebles/4/1dc65317-2acc-4ff1-b24c-7e33c27bca9b.jpg', '2025-10-05 09:49:08', '2025-10-05 09:49:08'),
(4, 5, '/Uploads/Inmuebles/5/a8665e7e-7497-47eb-bbb3-a64db5597a7e.jpeg', '2025-10-05 09:49:57', '2025-10-05 09:49:57'),
(5, 5, '/Uploads/Inmuebles/5/b7644b6c-a17e-441d-8c9e-b2e36b2d0f33.jpg', '2025-10-05 09:49:57', '2025-10-05 09:49:57'),
(6, 5, '/Uploads/Inmuebles/5/64e2a23b-a598-449f-9a87-a331aea5e08f.jpg', '2025-10-05 09:49:57', '2025-10-05 09:49:57'),
(7, 1, '/Uploads/Inmuebles/1/652e4acb-7c9f-421b-9b81-5ae7fbd370d8.jpeg', '2025-10-05 09:50:33', '2025-10-05 09:50:33'),
(8, 1, '/Uploads/Inmuebles/1/ec0dbb3b-9090-4681-b6d5-6feb504d0630.jpg', '2025-10-05 09:50:33', '2025-10-05 09:50:33'),
(9, 1, '/Uploads/Inmuebles/1/86ec4cfa-06b9-4e96-b6c9-e3bd7bb8a138.jpg', '2025-10-05 09:50:33', '2025-10-05 09:50:33'),
(10, 6, '/Uploads/Inmuebles/6/3b26aeb2-fd5b-4bb5-ac87-28685549415a.jpg', '2025-10-05 09:52:47', '2025-10-05 09:52:47'),
(11, 6, '/Uploads/Inmuebles/6/1d55e53b-2837-40ef-bf1d-35e0c71c1267.jpg', '2025-10-05 09:52:47', '2025-10-05 09:52:47');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `inmuebles`
--

CREATE TABLE `inmuebles` (
  `id_inmueble` int(10) UNSIGNED NOT NULL,
  `id_propietario` int(10) UNSIGNED NOT NULL,
  `id_uso` int(10) UNSIGNED NOT NULL,
  `id_tipo` int(10) UNSIGNED NOT NULL,
  `direccion` varchar(255) NOT NULL,
  `piso` varchar(20) DEFAULT NULL,
  `depto` varchar(20) DEFAULT NULL,
  `lat` decimal(9,6) DEFAULT NULL,
  `lon` decimal(9,6) DEFAULT NULL,
  `ambientes` int(10) UNSIGNED DEFAULT NULL,
  `precio` decimal(15,2) DEFAULT NULL,
  `portada` varchar(255) DEFAULT NULL,
  `activo` tinyint(1) NOT NULL DEFAULT 1,
  `created_at` datetime NOT NULL DEFAULT current_timestamp(),
  `updated_at` datetime NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_spanish_ci;

--
-- Volcado de datos para la tabla `inmuebles`
--

INSERT INTO `inmuebles` (`id_inmueble`, `id_propietario`, `id_uso`, `id_tipo`, `direccion`, `piso`, `depto`, `lat`, `lon`, `ambientes`, `precio`, `portada`, `activo`, `created_at`, `updated_at`) VALUES
(1, 1, 2, 4, 'AV SIEMPREVIVA', '1', '1', NULL, NULL, 1, 120000.00, '/Uploads/inmuebles/portada_1.jpeg', 1, '2025-09-05 15:29:44', '2025-10-05 09:50:44'),
(2, 6, 2, 4, 'LOS OLMOS 365', '1', '5', NULL, NULL, 4, 17000000.00, NULL, 1, '2025-09-25 16:23:39', '2025-09-25 16:23:39'),
(3, 1, 2, 4, 'PASO DE LOS LIBRES 345', '1', '10', NULL, NULL, 1, 3000000.00, NULL, 1, '2025-09-27 11:50:06', '2025-09-27 11:50:06'),
(4, 6, 2, 3, 'ALEM 786', NULL, NULL, NULL, NULL, 5, 345666434.00, '/Uploads/inmuebles/portada_4.jpg', 1, '2025-09-27 17:52:15', '2025-10-05 09:48:44'),
(5, 7, 1, 2, 'AV OESTE', NULL, NULL, NULL, NULL, 1, 2432342.00, '/Uploads/inmuebles/portada_5.jpg', 1, '2025-09-27 17:58:12', '2025-10-05 09:49:28'),
(6, 9, 2, 4, 'HIPOLITO IRIGOYEN 450', '1', '12', NULL, NULL, 3, 850000.00, '/Uploads/inmuebles/portada_6.jpeg', 1, '2025-10-05 09:52:07', '2025-10-05 09:52:21'),
(7, 13, 1, 1, 'GALERIA CARACOL LOCAL 5', '3', '0', NULL, NULL, 2, 365000.00, NULL, 1, '2025-10-05 10:12:18', '2025-10-05 10:12:18'),
(8, 8, 1, 1, 'GALERIA CARACOL LOCAL 8', '1', '2', NULL, NULL, 2, 390000.00, NULL, 1, '2025-10-05 10:12:54', '2025-10-05 10:12:54'),
(9, 12, 2, 4, 'TOMAS JOFRE 1115', '3', '14', NULL, NULL, 3, 650000.00, NULL, 1, '2025-10-05 10:13:21', '2025-10-05 10:13:21');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `inmuebles_tipos`
--

CREATE TABLE `inmuebles_tipos` (
  `id_tipo` int(10) UNSIGNED NOT NULL,
  `denominacion_tipo` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_spanish_ci;

--
-- Volcado de datos para la tabla `inmuebles_tipos`
--

INSERT INTO `inmuebles_tipos` (`id_tipo`, `denominacion_tipo`) VALUES
(3, 'casa'),
(4, 'departamento'),
(2, 'depósito'),
(1, 'local');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `inmuebles_usos`
--

CREATE TABLE `inmuebles_usos` (
  `id_uso` int(10) UNSIGNED NOT NULL,
  `denominacion_uso` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_spanish_ci;

--
-- Volcado de datos para la tabla `inmuebles_usos`
--

INSERT INTO `inmuebles_usos` (`id_uso`, `denominacion_uso`) VALUES
(1, 'comercial'),
(2, 'residencial');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `inquilinos`
--

CREATE TABLE `inquilinos` (
  `id_inquilino` int(10) UNSIGNED NOT NULL,
  `documento` varchar(20) NOT NULL,
  `apellido_nombres` varchar(100) NOT NULL,
  `domicilio` varchar(255) DEFAULT NULL,
  `telefono` varchar(50) DEFAULT NULL,
  `email` varchar(100) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_spanish_ci;

--
-- Volcado de datos para la tabla `inquilinos`
--

INSERT INTO `inquilinos` (`id_inquilino`, `documento`, `apellido_nombres`, `domicilio`, `telefono`, `email`) VALUES
(2, '30111223', 'PEREZ MARIA JOSE', 'CALLE PRINGLES 456', '02664022345', 'maria.perez2@mail.com'),
(3, '30111224', 'LOPEZ MIGUEL ANGEL', 'BARRIO JARDIN MZ 3 CASA 5', '02664033456', 'miguel.lopez3@mail.com'),
(4, '30111225', 'FERNANDEZ ANA LAURA', 'SAN MARTIN 789', '02664044567', 'ana.fernandez4@mail.com'),
(5, '30111226', 'RODRIGUEZ CARLOS ALBERTO', 'ITALIA 234', '02664055678', 'carlos.rodriguez5@mail.com'),
(6, '30111227', 'MARTINEZ CLAUDIA PATRICIA', 'AVENIDA ESPAÑA 345', '02664066789', 'claudia.martinez6@mail.com'),
(7, '30111228', 'SANCHEZ LUIS ENRIQUE', 'BARRIO CERRO DE LA CRUZ CASA 12', '02664077890', 'luis.sanchez7@mail.com'),
(8, '30111229', 'TORRES VERONICA ESTELA', 'MITRE 678', '02664088901', 'veronica.torres8@mail.com'),
(9, '30111230', 'GUTIERREZ JOSEFINA DEL VALLE', 'AVENIDA SUCRE 910', '02664099012', 'josefina.gutierrez9@mail.com'),
(10, '30111231', 'RAMIREZ MARTIN ALEJANDRO', 'CALLE BELGRANO 150', '02664111223', 'martin.ramirez10@mail.com'),
(11, '30111232', 'DOMINGUEZ LAURA BEATRIZ', 'AVENIDA PRESIDENTE PERON 321', '02664122334', 'laura.dominguez11@mail.com'),
(12, '30111233', 'AGUILAR PEDRO NICOLAS', 'BARRIO PUEYRREDON CASA 20', '02664133445', 'pedro.aguilar12@mail.com'),
(13, '30111234', 'HERRERA SUSANA VICTORIA', 'AVENIDA LAFINUR 654', '02664144556', 'susana.herrera13@mail.com'),
(14, '30111235', 'CASTRO RICARDO EMILIO', 'JUAN GILBERTO FUNES 432', '02664155667', 'ricardo.castro14@mail.com'),
(15, '30111236', 'MOLINA GRACIELA CECILIA', 'COLON 876', '02664166778', 'graciela.molina15@mail.com'),
(16, '30111237', 'RIVERA JORGE DANIEL', 'BARRIO RAWSON CASA 14', '02664177889', 'jorge.rivera16@mail.com'),
(17, '30111238', 'ORTEGA PATRICIA ALEJANDRA', 'AVENIDA EVA PERON 222', '02664188990', 'patricia.ortega17@mail.com'),
(18, '30111239', 'VARGAS EDUARDO FELIPE', 'CALLE JUNIN 900', '02664199001', 'eduardo.vargas18@mail.com'),
(19, '30111240', 'CARRIZO MONICA GABRIELA', 'BARRIO 500 VIVIENDAS MZ 4 CASA 8', '02664211212', 'monica.carrizo19@mail.com'),
(35, '28399283', 'GARCIA', 'ADSAS', NULL, NULL);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `meses`
--

CREATE TABLE `meses` (
  `id_mes` int(11) NOT NULL,
  `nombre` varchar(20) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `meses`
--

INSERT INTO `meses` (`id_mes`, `nombre`) VALUES
(4, 'Abril'),
(8, 'Agosto'),
(12, 'Diciembre'),
(1, 'Enero'),
(2, 'Febrero'),
(7, 'Julio'),
(6, 'Junio'),
(3, 'Marzo'),
(5, 'Mayo'),
(11, 'Noviembre'),
(10, 'Octubre'),
(9, 'Septiembre');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `pagos`
--

CREATE TABLE `pagos` (
  `id_pago` int(10) UNSIGNED NOT NULL,
  `id_contrato` int(10) UNSIGNED NOT NULL,
  `numero_pago` int(11) NOT NULL DEFAULT 0,
  `fecha_pago` date NOT NULL,
  `id_mes` int(11) DEFAULT NULL,
  `anio` int(11) NOT NULL,
  `detalle` varchar(50) DEFAULT NULL,
  `id_concepto` int(10) UNSIGNED NOT NULL,
  `importe` decimal(15,2) NOT NULL,
  `motivo_anulacion` varchar(255) DEFAULT NULL,
  `created_by` int(10) UNSIGNED NOT NULL,
  `updated_by` int(11) DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT current_timestamp(),
  `deleted_at` datetime DEFAULT NULL,
  `deleted_by` int(10) UNSIGNED DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_spanish_ci;

--
-- Volcado de datos para la tabla `pagos`
--

INSERT INTO `pagos` (`id_pago`, `id_contrato`, `numero_pago`, `fecha_pago`, `id_mes`, `anio`, `detalle`, `id_concepto`, `importe`, `motivo_anulacion`, `created_by`, `updated_by`, `created_at`, `deleted_at`, `deleted_by`) VALUES
(8, 3, 1, '2025-09-25', 9, 2025, '54646', 3, 684640.00, NULL, 2, NULL, '2025-09-25 21:57:09', NULL, NULL),
(9, 2, 1, '2025-09-26', 9, 2025, '', 2, 5465465.00, NULL, 2, NULL, '2025-09-26 15:47:38', NULL, NULL),
(10, 2, 2, '2025-09-26', 9, 2025, 'uhiu4114', 3, 474217.00, 'porque si', 2, NULL, '2025-09-26 17:16:26', '2025-09-26 20:40:15', 2),
(11, 2, 3, '2025-09-26', 9, 2025, '', 1, 5645654.00, NULL, 2, NULL, '2025-09-27 00:07:18', NULL, NULL),
(12, 3, 2, '2025-09-26', 8, 2025, '456456', 1, 46464.00, NULL, 2, NULL, '2025-09-27 00:07:45', NULL, NULL),
(13, 5, 1, '2025-09-30', 9, 2025, 'lkmlñm', 1, 722225.00, NULL, 5, NULL, '2025-10-01 00:37:37', NULL, NULL),
(14, 1, 1, '2025-09-30', 9, 2025, 'Multa por rescisión del contrato 1', 2, 240000.00, NULL, 5, NULL, '2025-10-01 01:28:53', NULL, NULL);

--
-- Disparadores `pagos`
--
DELIMITER $$
CREATE TRIGGER `trg_pagos_delete` AFTER UPDATE ON `pagos` FOR EACH ROW BEGIN
    IF NEW.deleted_at IS NOT NULL AND OLD.deleted_at IS NULL THEN
        INSERT INTO pagos_audit (id_pago, accion, accion_at, accion_by, old_data, new_data)
        VALUES (
            NEW.id_pago,
            'DELETE',
            NOW(),
            NEW.deleted_by,
            JSON_OBJECT(
                'id_contrato', OLD.id_contrato,
                'numero_pago', OLD.numero_pago,
                'fecha_pago', OLD.fecha_pago,
                'id_mes', OLD.id_mes,
                'anio', OLD.anio,
                'detalle', OLD.detalle,
                'id_concepto', OLD.id_concepto,
                'importe', OLD.importe
            ),
            NULL
        );
    END IF;
END
$$
DELIMITER ;
DELIMITER $$
CREATE TRIGGER `trg_pagos_insert` AFTER INSERT ON `pagos` FOR EACH ROW BEGIN
    INSERT INTO pagos_audit (id_pago, accion, accion_at, accion_by, old_data, new_data)
    VALUES (
        NEW.id_pago,
        'INSERT',
        NOW(),    
        NEW.created_by,
        NULL,
        JSON_OBJECT(
            'id_contrato', NEW.id_contrato,
            'numero_pago', NEW.numero_pago,
            'fecha_pago', NEW.fecha_pago,
            'id_mes', NEW.id_mes,
            'anio', NEW.anio,
            'detalle', NEW.detalle,
            'id_concepto', NEW.id_concepto,
            'importe', NEW.importe
        )
    );
END
$$
DELIMITER ;
DELIMITER $$
CREATE TRIGGER `trg_pagos_update` AFTER UPDATE ON `pagos` FOR EACH ROW BEGIN
    -- Solo registrar cambios si no es un soft delete
    IF NEW.deleted_at IS NULL THEN
        INSERT INTO pagos_audit (id_pago, accion, accion_at, accion_by, old_data, new_data)
        VALUES (
            NEW.id_pago,
            'UPDATE',
            NOW(),
            NEW.updated_by,
            JSON_OBJECT(
                'detalle', OLD.detalle,
                'id_concepto', OLD.id_concepto,
                'importe', OLD.importe,
                'motivo_anulacion', OLD.motivo_anulacion
            ),
            JSON_OBJECT(
                'detalle', NEW.detalle,
                'id_concepto', NEW.id_concepto,
                'importe', NEW.importe,
                'motivo_anulacion', NEW.motivo_anulacion
            )
        );
    END IF;
END
$$
DELIMITER ;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `pagos_audit`
--

CREATE TABLE `pagos_audit` (
  `id_audit` bigint(20) UNSIGNED NOT NULL,
  `id_pago` int(10) UNSIGNED NOT NULL,
  `accion` varchar(12) NOT NULL,
  `accion_at` datetime NOT NULL DEFAULT current_timestamp(),
  `accion_by` int(10) UNSIGNED DEFAULT NULL,
  `old_data` longtext DEFAULT NULL,
  `new_data` longtext DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_spanish_ci;

--
-- Volcado de datos para la tabla `pagos_audit`
--

INSERT INTO `pagos_audit` (`id_audit`, `id_pago`, `accion`, `accion_at`, `accion_by`, `old_data`, `new_data`) VALUES
(1, 10, 'UPDATE', '2025-09-26 21:06:49', 2, '{\"detalle\": \"uhiu4114\", \"id_concepto\": 1, \"importe\": 474217.00, \"motivo_anulacion\": \"porque si\"}', '{\"detalle\": \"uhiu4114\", \"id_concepto\": 3, \"importe\": 474217.00, \"motivo_anulacion\": \"porque si\"}'),
(2, 11, 'INSERT', '2025-09-26 21:07:18', 2, NULL, '{\"id_contrato\": 2, \"numero_pago\": 3, \"fecha_pago\": \"2025-09-26\", \"id_mes\": 9, \"anio\": 2025, \"detalle\": \"\", \"id_concepto\": 2, \"importe\": 5645654.00}'),
(3, 12, 'INSERT', '2025-09-26 21:07:45', 2, NULL, '{\"id_contrato\": 3, \"numero_pago\": 2, \"fecha_pago\": \"2025-09-26\", \"id_mes\": 8, \"anio\": 2025, \"detalle\": \"456456\", \"id_concepto\": 1, \"importe\": 46464.00}'),
(4, 13, 'INSERT', '2025-09-30 21:37:37', 5, NULL, '{\"id_contrato\": 5, \"numero_pago\": 1, \"fecha_pago\": \"2025-09-30\", \"id_mes\": 9, \"anio\": 2025, \"detalle\": \"lkmlñm\", \"id_concepto\": 1, \"importe\": 722225.00}'),
(5, 11, 'UPDATE', '2025-09-30 21:39:59', NULL, '{\"detalle\": \"\", \"id_concepto\": 2, \"importe\": 5645654.00, \"motivo_anulacion\": null}', '{\"detalle\": \"\", \"id_concepto\": 1, \"importe\": 5645654.00, \"motivo_anulacion\": null}'),
(6, 14, 'INSERT', '2025-09-30 22:28:53', 5, NULL, '{\"id_contrato\": 1, \"numero_pago\": 1, \"fecha_pago\": \"2025-09-30\", \"id_mes\": null, \"anio\": 0, \"detalle\": \"Multa por rescisión del contrato 1\", \"id_concepto\": 2, \"importe\": 240000.00}'),
(7, 8, 'UPDATE', '2025-09-30 22:29:08', NULL, '{\"detalle\": \"54646\", \"id_concepto\": 1, \"importe\": 684640.00, \"motivo_anulacion\": null}', '{\"detalle\": \"54646\", \"id_concepto\": 3, \"importe\": 684640.00, \"motivo_anulacion\": null}'),
(8, 14, 'UPDATE', '2025-10-05 10:32:30', NULL, '{\"detalle\": \"Multa por rescisión del contrato 1\", \"id_concepto\": 2, \"importe\": 240000.00, \"motivo_anulacion\": null}', '{\"detalle\": \"Multa por rescisión del contrato 1\", \"id_concepto\": 2, \"importe\": 240000.00, \"motivo_anulacion\": null}'),
(9, 14, 'UPDATE', '2025-10-05 10:32:35', NULL, '{\"detalle\": \"Multa por rescisión del contrato 1\", \"id_concepto\": 2, \"importe\": 240000.00, \"motivo_anulacion\": null}', '{\"detalle\": \"Multa por rescisión del contrato 1\", \"id_concepto\": 2, \"importe\": 240000.00, \"motivo_anulacion\": null}');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `propietarios`
--

CREATE TABLE `propietarios` (
  `id_propietario` int(10) UNSIGNED NOT NULL,
  `documento` varchar(20) NOT NULL,
  `apellido_nombres` varchar(100) NOT NULL,
  `domicilio` varchar(255) DEFAULT NULL,
  `telefono` varchar(50) DEFAULT NULL,
  `email` varchar(100) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_spanish_ci;

--
-- Volcado de datos para la tabla `propietarios`
--

INSERT INTO `propietarios` (`id_propietario`, `documento`, `apellido_nombres`, `domicilio`, `telefono`, `email`) VALUES
(1, '24299754', 'QUIROGA VERONICA', 'LAFINUR 1238', '2554854574', 'vero@hotmail.com'),
(6, '35842052', 'RICCHIARDI ROMA', 'AV 1234', '2664750247', 'roma.ricchiardi@gmail.com'),
(7, '65465465', 'PUENTES, RAUL', 'CALLE FALSA 987', '2664758546', 'decimeraul@gmail.com'),
(8, '20123456', 'PEREZ JUAN', 'AV. BELGRANO 123', '2914567890', 'juan.perez@example.com'),
(9, '22345678', 'GONZALEZ MARIA', 'CALLE ALSINA 456', '2915678901', 'maria.gonzalez@example.com'),
(10, '23456789', 'LOPEZ CARLOS', 'RIVADAVIA 789', '2916789012', 'carlos.lopez@example.com'),
(11, '24567890', 'FERNANDEZ LAURA', 'AV. ALEM 1020', '2917890123', 'laura.fernandez@example.com'),
(12, '25678901', 'MARTINEZ RICARDO', 'SAAVEDRA 333', '2918901234', 'ricardo.martinez@example.com'),
(13, '26789012', 'ROMERO SOFIA', 'DONADO 876', '2919012345', 'sofia.romero@example.com'),
(14, '27890123', 'TORRES LUCIANO', 'BROWN 554', '2919123456', 'luciano.torres@example.com'),
(15, '28901234', 'RUIZ CAMILA', 'ZAPIOLA 221', '2919234567', 'camila.ruiz@example.com');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `roles`
--

CREATE TABLE `roles` (
  `id_rol` int(10) UNSIGNED NOT NULL,
  `denominacion_rol` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_spanish_ci;

--
-- Volcado de datos para la tabla `roles`
--

INSERT INTO `roles` (`id_rol`, `denominacion_rol`) VALUES
(1, 'administrador'),
(2, 'empleado');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `usuarios`
--

CREATE TABLE `usuarios` (
  `id_usuario` int(10) UNSIGNED NOT NULL,
  `apellido_nombres` varchar(100) NOT NULL,
  `alias` varchar(50) NOT NULL,
  `password` varchar(255) NOT NULL,
  `email` varchar(100) NOT NULL,
  `id_rol` int(10) UNSIGNED NOT NULL,
  `created_at` datetime NOT NULL DEFAULT current_timestamp(),
  `updated_at` datetime NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `reset_token` varchar(255) DEFAULT NULL,
  `reset_token_expira` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_spanish_ci;

--
-- Volcado de datos para la tabla `usuarios`
--

INSERT INTO `usuarios` (`id_usuario`, `apellido_nombres`, `alias`, `password`, `email`, `id_rol`, `created_at`, `updated_at`, `reset_token`, `reset_token_expira`) VALUES
(1, 'ADMIN', 'ADMIN', '$2a$11$7oUM5dBlgxZgi5vTn3Sac.SyfpaL.c9JLW721ahnsex5l8ouoAVFW', 'admin@gmail.com', 1, '2025-08-15 17:52:18', '2025-10-05 09:43:58', NULL, NULL),
(2, 'RICCHIARDI, ROMA', 'ROMITHA!!', '$2a$11$oRmmk370yDEIJCbN.T9wtO3LtKHyFG5MChCDtgSDJttYSYhhU6STS', 'roma.ricchiardi@gmail.com', 1, '2025-09-17 17:17:34', '2025-09-30 14:12:18', '4963648471b7438c8f5ad005cf1fcfbc', '2025-09-30 18:12:18'),
(3, 'PEREZ, JUAN', 'JUANCHO', '$2a$11$rcmwzKHXf1iY.g5yz6JU/u6uqQQxqSSdy5qgTcXhG2HjHpt259F8.', 'juanp@gmail.com', 2, '2025-09-19 14:40:21', '2025-09-19 16:34:06', NULL, NULL),
(4, 'GARCIA, FACUNDO', 'FACUNDO', '$2a$11$dWXsCYHuzxGm1Q9ThFNy6e1nIa6Llb4AIPa/DQ7bP7TOgAFH5dFtO', 'elfaculee@gmail.com', 1, '2025-09-19 18:33:15', '2025-09-19 23:22:30', NULL, NULL),
(5, 'GOMEZ, MARTIN', 'TINCHO', '$2a$11$Sp0AqvcX23iKdqfJihTzROAffVk.ESlBzxXR.XpMZpigDUtou3waa', 'tincho@gmail.com', 2, '2025-09-28 22:41:01', '2025-09-28 22:41:01', NULL, NULL),
(6, 'MACHADO RAMON', 'RAMON', '$2a$11$U18VJRiD68tQ9t/FcgWC5uWyw1IFGCZ9n3nVEe9v1WZIw5MW9ulKe', 'empleado@gmail.com', 2, '2025-10-05 09:25:05', '2025-10-05 09:44:27', NULL, NULL);

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `conceptos`
--
ALTER TABLE `conceptos`
  ADD PRIMARY KEY (`id_concepto`),
  ADD UNIQUE KEY `uq_conceptos_den` (`denominacion_concepto`);

--
-- Indices de la tabla `contratos`
--
ALTER TABLE `contratos`
  ADD PRIMARY KEY (`id_contrato`),
  ADD KEY `fk_contratos_inmueble` (`id_inmueble`),
  ADD KEY `fk_contratos_inquilino` (`id_inquilino`),
  ADD KEY `fk_contratos_cb` (`created_by`),
  ADD KEY `fk_contratos_db` (`deleted_by`);

--
-- Indices de la tabla `contratos_audit`
--
ALTER TABLE `contratos_audit`
  ADD PRIMARY KEY (`id_audit`),
  ADD KEY `fk_contratos_audit_user` (`accion_by`),
  ADD KEY `fk_contratos_audit_contrato` (`id_contrato`);

--
-- Indices de la tabla `imagenes`
--
ALTER TABLE `imagenes`
  ADD PRIMARY KEY (`id_imagen`),
  ADD KEY `fk_imagenes_inmuebles` (`id_inmueble`);

--
-- Indices de la tabla `inmuebles`
--
ALTER TABLE `inmuebles`
  ADD PRIMARY KEY (`id_inmueble`),
  ADD KEY `fk_inmuebles_propietarios` (`id_propietario`),
  ADD KEY `fk_inmuebles_tipos` (`id_tipo`),
  ADD KEY `fk_inmuebles_usos` (`id_uso`);

--
-- Indices de la tabla `inmuebles_tipos`
--
ALTER TABLE `inmuebles_tipos`
  ADD PRIMARY KEY (`id_tipo`),
  ADD UNIQUE KEY `uq_tipos_den` (`denominacion_tipo`);

--
-- Indices de la tabla `inmuebles_usos`
--
ALTER TABLE `inmuebles_usos`
  ADD PRIMARY KEY (`id_uso`),
  ADD UNIQUE KEY `uq_usos_den` (`denominacion_uso`);

--
-- Indices de la tabla `inquilinos`
--
ALTER TABLE `inquilinos`
  ADD PRIMARY KEY (`id_inquilino`),
  ADD UNIQUE KEY `uq_inq_documento` (`documento`);

--
-- Indices de la tabla `meses`
--
ALTER TABLE `meses`
  ADD PRIMARY KEY (`id_mes`),
  ADD UNIQUE KEY `nombre` (`nombre`);

--
-- Indices de la tabla `pagos`
--
ALTER TABLE `pagos`
  ADD PRIMARY KEY (`id_pago`),
  ADD KEY `fk_pagos_contrato` (`id_contrato`),
  ADD KEY `fk_pagos_concepto` (`id_concepto`),
  ADD KEY `fk_pagos_cb` (`created_by`),
  ADD KEY `fk_pagos_db` (`deleted_by`),
  ADD KEY `fk_pagos_meses` (`id_mes`);

--
-- Indices de la tabla `pagos_audit`
--
ALTER TABLE `pagos_audit`
  ADD PRIMARY KEY (`id_audit`),
  ADD KEY `fk_pagos_audit_user` (`accion_by`),
  ADD KEY `fk_pagos_audit_pago` (`id_pago`);

--
-- Indices de la tabla `propietarios`
--
ALTER TABLE `propietarios`
  ADD PRIMARY KEY (`id_propietario`),
  ADD UNIQUE KEY `uq_prop_documento` (`documento`);

--
-- Indices de la tabla `roles`
--
ALTER TABLE `roles`
  ADD PRIMARY KEY (`id_rol`),
  ADD UNIQUE KEY `uq_roles_den` (`denominacion_rol`);

--
-- Indices de la tabla `usuarios`
--
ALTER TABLE `usuarios`
  ADD PRIMARY KEY (`id_usuario`),
  ADD UNIQUE KEY `uq_usuarios_alias` (`alias`),
  ADD UNIQUE KEY `uq_usuarios_email` (`email`),
  ADD KEY `fk_usuarios_roles` (`id_rol`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `conceptos`
--
ALTER TABLE `conceptos`
  MODIFY `id_concepto` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=20;

--
-- AUTO_INCREMENT de la tabla `contratos`
--
ALTER TABLE `contratos`
  MODIFY `id_contrato` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=10;

--
-- AUTO_INCREMENT de la tabla `contratos_audit`
--
ALTER TABLE `contratos_audit`
  MODIFY `id_audit` bigint(20) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT de la tabla `imagenes`
--
ALTER TABLE `imagenes`
  MODIFY `id_imagen` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=12;

--
-- AUTO_INCREMENT de la tabla `inmuebles`
--
ALTER TABLE `inmuebles`
  MODIFY `id_inmueble` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=10;

--
-- AUTO_INCREMENT de la tabla `inmuebles_tipos`
--
ALTER TABLE `inmuebles_tipos`
  MODIFY `id_tipo` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT de la tabla `inmuebles_usos`
--
ALTER TABLE `inmuebles_usos`
  MODIFY `id_uso` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT de la tabla `inquilinos`
--
ALTER TABLE `inquilinos`
  MODIFY `id_inquilino` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=37;

--
-- AUTO_INCREMENT de la tabla `meses`
--
ALTER TABLE `meses`
  MODIFY `id_mes` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=13;

--
-- AUTO_INCREMENT de la tabla `pagos`
--
ALTER TABLE `pagos`
  MODIFY `id_pago` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=15;

--
-- AUTO_INCREMENT de la tabla `pagos_audit`
--
ALTER TABLE `pagos_audit`
  MODIFY `id_audit` bigint(20) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=10;

--
-- AUTO_INCREMENT de la tabla `propietarios`
--
ALTER TABLE `propietarios`
  MODIFY `id_propietario` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=16;

--
-- AUTO_INCREMENT de la tabla `roles`
--
ALTER TABLE `roles`
  MODIFY `id_rol` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT de la tabla `usuarios`
--
ALTER TABLE `usuarios`
  MODIFY `id_usuario` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- Restricciones para tablas volcadas
--

--
-- Filtros para la tabla `contratos`
--
ALTER TABLE `contratos`
  ADD CONSTRAINT `fk_contratos_cb` FOREIGN KEY (`created_by`) REFERENCES `usuarios` (`id_usuario`),
  ADD CONSTRAINT `fk_contratos_db` FOREIGN KEY (`deleted_by`) REFERENCES `usuarios` (`id_usuario`) ON DELETE SET NULL,
  ADD CONSTRAINT `fk_contratos_inmueble` FOREIGN KEY (`id_inmueble`) REFERENCES `inmuebles` (`id_inmueble`),
  ADD CONSTRAINT `fk_contratos_inquilino` FOREIGN KEY (`id_inquilino`) REFERENCES `inquilinos` (`id_inquilino`);

--
-- Filtros para la tabla `contratos_audit`
--
ALTER TABLE `contratos_audit`
  ADD CONSTRAINT `fk_contratos_audit_contrato` FOREIGN KEY (`id_contrato`) REFERENCES `contratos` (`id_contrato`) ON DELETE CASCADE,
  ADD CONSTRAINT `fk_contratos_audit_user` FOREIGN KEY (`accion_by`) REFERENCES `usuarios` (`id_usuario`) ON DELETE SET NULL;

--
-- Filtros para la tabla `imagenes`
--
ALTER TABLE `imagenes`
  ADD CONSTRAINT `fk_imagenes_inmuebles` FOREIGN KEY (`id_inmueble`) REFERENCES `inmuebles` (`id_inmueble`);

--
-- Filtros para la tabla `inmuebles`
--
ALTER TABLE `inmuebles`
  ADD CONSTRAINT `fk_inmuebles_propietarios` FOREIGN KEY (`id_propietario`) REFERENCES `propietarios` (`id_propietario`),
  ADD CONSTRAINT `fk_inmuebles_tipos` FOREIGN KEY (`id_tipo`) REFERENCES `inmuebles_tipos` (`id_tipo`),
  ADD CONSTRAINT `fk_inmuebles_usos` FOREIGN KEY (`id_uso`) REFERENCES `inmuebles_usos` (`id_uso`);

--
-- Filtros para la tabla `pagos`
--
ALTER TABLE `pagos`
  ADD CONSTRAINT `fk_pagos_cb` FOREIGN KEY (`created_by`) REFERENCES `usuarios` (`id_usuario`),
  ADD CONSTRAINT `fk_pagos_concepto` FOREIGN KEY (`id_concepto`) REFERENCES `conceptos` (`id_concepto`),
  ADD CONSTRAINT `fk_pagos_contrato` FOREIGN KEY (`id_contrato`) REFERENCES `contratos` (`id_contrato`),
  ADD CONSTRAINT `fk_pagos_db` FOREIGN KEY (`deleted_by`) REFERENCES `usuarios` (`id_usuario`) ON DELETE SET NULL,
  ADD CONSTRAINT `fk_pagos_meses` FOREIGN KEY (`id_mes`) REFERENCES `meses` (`id_mes`);

--
-- Filtros para la tabla `pagos_audit`
--
ALTER TABLE `pagos_audit`
  ADD CONSTRAINT `fk_pagos_audit_pago` FOREIGN KEY (`id_pago`) REFERENCES `pagos` (`id_pago`),
  ADD CONSTRAINT `fk_pagos_audit_user` FOREIGN KEY (`accion_by`) REFERENCES `usuarios` (`id_usuario`) ON DELETE SET NULL;

--
-- Filtros para la tabla `usuarios`
--
ALTER TABLE `usuarios`
  ADD CONSTRAINT `fk_usuarios_roles` FOREIGN KEY (`id_rol`) REFERENCES `roles` (`id_rol`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
