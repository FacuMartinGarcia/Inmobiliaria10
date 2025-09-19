-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 20-09-2025 a las 01:14:25
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
(11, 'Mes Abril'),
(15, 'Mes Agosto'),
(1, 'Mes Alquiler'),
(19, 'Mes Diciembre'),
(8, 'Mes Enero'),
(9, 'Mes Febrero'),
(14, 'Mes Julio'),
(13, 'Mes Junio'),
(10, 'Mes Marzo'),
(12, 'Mes Mayo'),
(18, 'Mes Noviembre'),
(17, 'Mes Octubre'),
(16, 'Mes Septiembre'),
(2, 'Multa'),
(7, 'Reajuste 4 meses'),
(5, 'Reajuste de 6 meses');

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
  `created_at` datetime NOT NULL DEFAULT current_timestamp(),
  `deleted_at` datetime DEFAULT NULL,
  `deleted_by` int(10) UNSIGNED DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_spanish_ci;

--
-- Volcado de datos para la tabla `contratos`
--

INSERT INTO `contratos` (`id_contrato`, `fecha_firma`, `id_inmueble`, `id_inquilino`, `fecha_inicio`, `fecha_fin`, `monto_mensual`, `rescision`, `monto_multa`, `created_by`, `created_at`, `deleted_at`, `deleted_by`) VALUES
(1, NULL, 1, 14, '2025-09-05', '2026-09-05', 120000.00, NULL, NULL, 1, '2025-09-05 18:30:25', NULL, NULL);

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
  `activo` tinyint(1) NOT NULL DEFAULT 1,
  `created_at` datetime NOT NULL DEFAULT current_timestamp(),
  `updated_at` datetime NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_spanish_ci;

--
-- Volcado de datos para la tabla `inmuebles`
--

INSERT INTO `inmuebles` (`id_inmueble`, `id_propietario`, `id_uso`, `id_tipo`, `direccion`, `piso`, `depto`, `lat`, `lon`, `ambientes`, `precio`, `activo`, `created_at`, `updated_at`) VALUES
(1, 1, 2, 4, 'AV SIEMPREVIVA', '1', '1', NULL, NULL, 1, 120000.00, 1, '2025-09-05 15:29:44', '2025-09-05 15:29:44');

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
-- Estructura de tabla para la tabla `pagos`
--

CREATE TABLE `pagos` (
  `id_pago` int(10) UNSIGNED NOT NULL,
  `id_contrato` int(10) UNSIGNED NOT NULL,
  `fecha_pago` date NOT NULL,
  `detalle` varchar(50) DEFAULT NULL,
  `id_concepto` int(10) UNSIGNED NOT NULL,
  `importe` decimal(15,2) NOT NULL,
  `motivo_anulacion` varchar(255) DEFAULT NULL,
  `created_by` int(10) UNSIGNED NOT NULL,
  `created_at` datetime NOT NULL DEFAULT current_timestamp(),
  `deleted_at` datetime DEFAULT NULL,
  `deleted_by` int(10) UNSIGNED DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_spanish_ci;

--
-- Volcado de datos para la tabla `pagos`
--

INSERT INTO `pagos` (`id_pago`, `id_contrato`, `fecha_pago`, `detalle`, `id_concepto`, `importe`, `motivo_anulacion`, `created_by`, `created_at`, `deleted_at`, `deleted_by`) VALUES
(1, 1, '2025-09-05', 'mes comicion', 1, 12000000.00, NULL, 1, '2025-09-05 21:56:39', NULL, NULL),
(2, 1, '2025-09-06', 'mes septiembre', 3, 6000000.00, NULL, 1, '2025-09-07 00:41:49', '2025-09-07 00:53:46', 1);

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
(1, '24299754', 'QUIROGA VERONICA', 'LAFINUR 1238', NULL, NULL),
(6, '35842052', 'Ricchiardi Roma', 'AV 1234', '2664750247', 'roma.ricchiardi@gmail.com');

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
(1, 'Admin', 'admin', 'temp', 'admin@inmo.test', 1, '2025-08-15 17:52:18', '2025-08-15 17:52:18', NULL, NULL),
(2, 'RICCHIARDI, ROMANELA', 'ROMITHA!!', '$2a$11$C4WZ3li.xwlLuzSfaUJ79ux7DQFZBlrFpZ2txOsLqtn5YzFijrtGC', 'roma.ricchiardi@gmail.com', 1, '2025-09-17 17:17:34', '2025-09-19 16:33:58', NULL, NULL),
(3, 'PEREZ, JUAN', 'JUANCHO', '$2a$11$rcmwzKHXf1iY.g5yz6JU/u6uqQQxqSSdy5qgTcXhG2HjHpt259F8.', 'juanp@gmail.com', 2, '2025-09-19 14:40:21', '2025-09-19 16:34:06', NULL, NULL),
(4, 'GARCIA, FACUNDO', 'FACU', '$2a$11$dWXsCYHuzxGm1Q9ThFNy6e1nIa6Llb4AIPa/DQ7bP7TOgAFH5dFtO', 'elfaculee@gmail.com', 1, '2025-09-19 18:33:15', '2025-09-19 18:33:15', NULL, NULL);

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
-- Indices de la tabla `pagos`
--
ALTER TABLE `pagos`
  ADD PRIMARY KEY (`id_pago`),
  ADD KEY `fk_pagos_contrato` (`id_contrato`),
  ADD KEY `fk_pagos_concepto` (`id_concepto`),
  ADD KEY `fk_pagos_cb` (`created_by`),
  ADD KEY `fk_pagos_db` (`deleted_by`);

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
  MODIFY `id_contrato` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- AUTO_INCREMENT de la tabla `contratos_audit`
--
ALTER TABLE `contratos_audit`
  MODIFY `id_audit` bigint(20) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `inmuebles`
--
ALTER TABLE `inmuebles`
  MODIFY `id_inmueble` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

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
-- AUTO_INCREMENT de la tabla `pagos`
--
ALTER TABLE `pagos`
  MODIFY `id_pago` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT de la tabla `pagos_audit`
--
ALTER TABLE `pagos_audit`
  MODIFY `id_audit` bigint(20) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `propietarios`
--
ALTER TABLE `propietarios`
  MODIFY `id_propietario` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT de la tabla `roles`
--
ALTER TABLE `roles`
  MODIFY `id_rol` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT de la tabla `usuarios`
--
ALTER TABLE `usuarios`
  MODIFY `id_usuario` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

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
  ADD CONSTRAINT `fk_pagos_db` FOREIGN KEY (`deleted_by`) REFERENCES `usuarios` (`id_usuario`) ON DELETE SET NULL;

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
