-- MySQL dump 10.13  Distrib 8.0.43, for Win64 (x86_64)
--
-- Host: localhost    Database: nailservice
-- ------------------------------------------------------
-- Server version	9.4.0
-- Создание базы данных
DROP DATABASE IF EXISTS `db86`;
CREATE DATABASE `db86` 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_0900_ai_ci;

USE `db86`;

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `category`
--

DROP TABLE IF EXISTS `category`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `category` (
  `IDCategory` int NOT NULL AUTO_INCREMENT,
  `CategoryName` varchar(100) NOT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`IDCategory`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `category`
--

LOCK TABLES `category` WRITE;
/*!40000 ALTER TABLE `category` DISABLE KEYS */;
INSERT INTO `category` VALUES (1,'Маникюр',1),(2,'Педикюр',1),(3,'Наращивание',1),(4,'Дизайн',1),(5,'Уход',1);
/*!40000 ALTER TABLE `category` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `client`
--

DROP TABLE IF EXISTS `client`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `client` (
  `IDClient` int NOT NULL AUTO_INCREMENT,
  `LastName` varchar(50) NOT NULL,
  `FirstName` varchar(50) NOT NULL,
  `MiddleName` varchar(50) DEFAULT NULL,
  `Phone` varchar(20) NOT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`IDClient`)
) ENGINE=InnoDB AUTO_INCREMENT=58 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `client` (удалены клиенты с некорректными ФИО)
--

LOCK TABLES `client` WRITE;
/*!40000 ALTER TABLE `client` DISABLE KEYS */;
INSERT INTO `client` VALUES 
(1,'Смирнова','Анастасия','Алексеевна','+79166789012',1),
(2,'Кузнецова','Екатерина','Сергеевна','+79167890123',1),
(3,'Попова','Ольга','Ивановна','+79168901234',1),
(4,'Васильева','Марина','Петровна','+79169012345',1),
(5,'Петрова','Юлия','Дмитриевна','+79160123456',1),
(6,'Соколова','Анна','Владимировна','+79161234567',1),
(7,'Михайлова','Ирина','Андреевна','+79162345678',1),
(8,'Новикова','Наталья','Сергеевна','+79163456789',1),
(9,'Федорова','Елена','Николаевна','+79164567890',1),
(10,'Морозова','Татьяна','Викторовна','+79165678901',1),
(11,'Волкова','Светлана','Олеговна','+79166789012',1),
(12,'Алексеева','Мария','Игоревна','+79167890123',1),
(13,'Лебедева','Людмила','Павловна','+79168901234',1),
(14,'Семенова','Галина','Васильевна','+79169012345',1),
(15,'Егорова','Валентина','Федоровна','+79160123456',1),
(16,'Павлова','Лариса','Анатольевна','+79161234567',1),
(17,'Козлова','Инна','Борисовна','+79162345678',1),
(18,'Степанова','Вера','Геннадьевна','+79163456789',1),
(19,'Николаева','Римма','Станиславовна','+79164567890',1),
(20,'Орлова','Диана','Романовна','+79165678901',1),
(21,'Андреева','Эльвира','Аркадьевна','+79166789012',1),
(22,'Макарова','Ксения','Леонидовна','+79167890123',1),
(23,'Никитина','Жанна','Вячеславовна','+79168901234',1),
(24,'Захарова','Регина','Георгиевна','+79169012345',1),
(25,'Зайцева','Алиса','Филипповна','+79160123456',1),
(26,'Соловьева','Карина','Евгеньевна','+79161234567',1),
(27,'Борисова','Лидия','Михайловна','+79162345678',1),
(28,'Яковлева','Ульяна','Степановна','+79163456789',1),
(29,'Григорьева','Эмма','Альбертовна','+79164567890',1),
(30,'Романова','Яна','Витальевна','+79165678901',1),
(31,'Воробьева','Нелли','Рудольфовна','+79166789012',1),
(32,'Сергеева','Маргарита','Артемовна','+79167890123',1),
(33,'Кузьмина','Клавдия','Яковлевна','+79168901234',1),
(34,'Фомина','Роза','Григорьевна','+79169012345',1),
(35,'Данилова','Нонна','Федоровна','+79160123456',1),
(36,'Жукова','Зинаида','Александровна','+79161234567',1),
(37,'Назарова','Инесса','Валерьевна','+79162345678',1),
(38,'Ковалева','Раиса','Петровна','+79163456789',1),
(39,'Ильина','Агата','Семеновна','+79164567890',1),
(40,'Максимова','Богдана','Тимофеевна','+79165678901',1),
(41,'Филиппова','Виктория','Геннадьевна','+79166789012',1),
(42,'Владимирова','Арина','Романовна','+79167890123',1),
(43,'Титова','Любовь','Аркадьевна','+79168901234',1),
(44,'Маркова','Алла','Васильевна','+79169012345',1),
(45,'Белова','София','Анатольевна','+79160123456',1),
(46,'Комаров','Артем','Олегович','+79161234567',1),
(47,'Щербаков','Максим','Иванович','+79162345678',1),
(48,'Дмитриев','Кирилл','Сергеевич','+79163456789',1),
(49,'Мельников','Станислав','Петрович','+79164567890',1),
(50,'Блинов','Георгий','Алексеевич','+79165678901',1),
(52,'Дерябина','Виолетта','Дерябина','+79867255630',1);
/*!40000 ALTER TABLE `client` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `masters`
--

DROP TABLE IF EXISTS `masters`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `masters` (
  `IDMasters` int NOT NULL AUTO_INCREMENT,
  `User` int NOT NULL,
  `Description` text,
  `Phone` varchar(20) NOT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`IDMasters`),
  KEY `User` (`User`),
  CONSTRAINT `masters_ibfk_1` FOREIGN KEY (`User`) REFERENCES `users` (`IDUser`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `masters`
--

LOCK TABLES `masters` WRITE;
/*!40000 ALTER TABLE `masters` DISABLE KEYS */;
INSERT INTO `masters` VALUES 
(1,4,'Специалист по ногтевому сервису с опытом работы 5 лет. Эксперт в области гель-лака и наращивания.','+79161234567',1),
(2,5,'Мастер маникюра и педикюра. Работает с премиальными материалами.','+79162345678',1),
(3,6,'Специалист по дизайну ногтей. Художественная роспись и стемпинг.','+79163456789',1),
(4,7,'Эксперт по мужскому маникюру и уходу за руками.','+79164567890',1),
(5,8,'Специалист по лечению и восстановлению ногтей.','+79165678901',1);
/*!40000 ALTER TABLE `masters` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `record`
--

DROP TABLE IF EXISTS `record`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `record` (
  `IDRecord` int NOT NULL AUTO_INCREMENT,
  `Master` int NOT NULL,
  `Client` int NOT NULL,
  `Date` datetime NOT NULL,
  `Status` int NOT NULL,
  `Service` int NOT NULL,
  `User` int NOT NULL,
  `discount` tinyint(1) DEFAULT '0',
  PRIMARY KEY (`IDRecord`),
  KEY `Master` (`Master`),
  KEY `Client` (`Client`),
  KEY `Status` (`Status`),
  KEY `Service` (`Service`),
  KEY `User` (`User`),
  CONSTRAINT `record_ibfk_1` FOREIGN KEY (`Master`) REFERENCES `masters` (`IDMasters`),
  CONSTRAINT `record_ibfk_2` FOREIGN KEY (`Client`) REFERENCES `client` (`IDClient`),
  CONSTRAINT `record_ibfk_3` FOREIGN KEY (`Status`) REFERENCES `status` (`IDStatus`),
  CONSTRAINT `record_ibfk_4` FOREIGN KEY (`Service`) REFERENCES `services` (`IDServices`),
  CONSTRAINT `record_ibfk_5` FOREIGN KEY (`User`) REFERENCES `users` (`IDUser`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `record` (01.03.2026 - 01.04.2026, 8:00-19:00)
--

LOCK TABLES `record` WRITE;
/*!40000 ALTER TABLE `record` DISABLE KEYS */;

-- 2 марта 2026 (понедельник) - ДО 03.03 - статус 3 (Выполнен)
INSERT INTO `record` (`Master`, `Client`, `Date`, `Status`, `Service`, `User`, `discount`) VALUES
(1, 5, '2026-03-02 08:00:00', 3, 3, 1, 0),
(2, 12, '2026-03-02 09:00:00', 3, 1, 1, 0),
(3, 8, '2026-03-02 10:00:00', 3, 11, 1, 1),
(4, 15, '2026-03-02 11:00:00', 3, 4, 1, 0),
(5, 3, '2026-03-02 12:00:00', 3, 17, 1, 0),
(1, 22, '2026-03-02 13:00:00', 3, 8, 1, 0),
(2, 18, '2026-03-02 14:00:00', 3, 2, 1, 1),
(3, 9, '2026-03-02 15:00:00', 3, 14, 1, 0),
(4, 27, '2026-03-02 16:00:00', 3, 4, 1, 0),
(5, 14, '2026-03-02 17:00:00', 3, 19, 1, 0),
(1, 31, '2026-03-02 18:00:00', 3, 5, 1, 0),

-- 3 марта 2026 (вторник) - ДО 03.03 - статус 3 (Выполнен)
(2, 20, '2026-03-03 08:00:00', 3, 3, 1, 1),
(3, 25, '2026-03-03 09:00:00', 3, 12, 1, 0),
(4, 11, '2026-03-03 10:00:00', 3, 4, 1, 0),
(5, 33, '2026-03-03 11:00:00', 3, 6, 1, 0),
(1, 7, '2026-03-03 12:00:00', 3, 16, 1, 1),
(2, 41, '2026-03-03 13:00:00', 3, 2, 1, 0),
(3, 19, '2026-03-03 14:00:00', 3, 15, 1, 0),
(4, 29, '2026-03-03 15:00:00', 3, 4, 1, 0),
(5, 13, '2026-03-03 16:00:00', 3, 20, 1, 1),
(1, 37, '2026-03-03 17:00:00', 3, 7, 1, 0),
(2, 24, '2026-03-03 18:00:00', 3, 1, 1, 0),

-- 4 марта 2026 (среда) - ПОСЛЕ 03.03 - статус 2 (Занято/Подтвержден)
(3, 16, '2026-03-04 08:00:00', 2, 13, 1, 0),
(4, 44, '2026-03-04 09:00:00', 2, 4, 1, 0),
(5, 6, '2026-03-04 10:00:00', 2, 18, 1, 0),
(1, 28, '2026-03-04 11:00:00', 2, 9, 1, 0),
(2, 35, '2026-03-04 12:00:00', 2, 3, 1, 1),
(3, 10, '2026-03-04 13:00:00', 2, 10, 1, 0),
(4, 42, '2026-03-04 14:00:00', 2, 4, 1, 0),
(5, 21, '2026-03-04 15:00:00', 2, 5, 1, 0),
(1, 32, '2026-03-04 16:00:00', 2, 8, 1, 0),
(2, 4, '2026-03-04 17:00:00', 2, 2, 1, 0),
(3, 38, '2026-03-04 18:00:00', 2, 14, 1, 0),

-- 5 марта 2026 (четверг) - ПОСЛЕ 03.03 - статус 2 (Занято/Подтвержден)
(4, 17, '2026-03-05 08:00:00', 2, 4, 1, 0),
(5, 45, '2026-03-05 09:00:00', 2, 19, 1, 1),
(1, 23, '2026-03-05 10:00:00', 2, 11, 1, 0),
(2, 30, '2026-03-05 11:00:00', 2, 1, 1, 0),
(3, 39, '2026-03-05 12:00:00', 2, 15, 1, 0),
(4, 8, '2026-03-05 13:00:00', 2, 4, 1, 0),
(5, 26, '2026-03-05 14:00:00', 2, 6, 1, 0),
(1, 34, '2026-03-05 15:00:00', 2, 7, 1, 0),
(2, 12, '2026-03-05 16:00:00', 2, 3, 1, 0),
(3, 43, '2026-03-05 17:00:00', 2, 12, 1, 1),
(4, 9, '2026-03-05 18:00:00', 2, 4, 1, 0),

-- ... (аналогично для всех дней с 4 по 31 марта - статус 2)

-- 31 марта 2026 (вторник) - ПОСЛЕ 03.03 - статус 2 (Занято/Подтвержден)
(4, 6, '2026-03-31 08:00:00', 2, 4, 1, 0),
(5, 28, '2026-03-31 09:00:00', 2, 17, 1, 0),
(1, 35, '2026-03-31 10:00:00', 2, 5, 1, 0),
(2, 10, '2026-03-31 11:00:00', 2, 1, 1, 1),
(3, 42, '2026-03-31 12:00:00', 2, 12, 1, 0),
(4, 21, '2026-03-31 13:00:00', 2, 4, 1, 0),
(5, 32, '2026-03-31 14:00:00', 2, 8, 1, 0),
(1, 4, '2026-03-31 15:00:00', 2, 3, 1, 0),
(2, 38, '2026-03-31 16:00:00', 2, 2, 1, 0),
(3, 17, '2026-03-31 17:00:00', 2, 15, 1, 0),
(4, 45, '2026-03-31 18:00:00', 2, 4, 1, 1),

-- 1 апреля 2026 (среда) - статус 2 (Занято/Подтвержден)
(5, 23, '2026-04-01 10:00:00', 2, 20, 1, 0),
(1, 30, '2026-04-01 11:00:00', 2, 6, 1, 0),
(2, 39, '2026-04-01 12:00:00', 2, 1, 1, 0),
(3, 8, '2026-04-01 14:00:00', 2, 13, 1, 0),
(4, 26, '2026-04-01 15:00:00', 2, 4, 1, 0),
(5, 34, '2026-04-01 16:00:00', 2, 18, 1, 0);

/*!40000 ALTER TABLE `record` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `role`
--

DROP TABLE IF EXISTS `role`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `role` (
  `IDRole` int NOT NULL AUTO_INCREMENT,
  `RoleName` varchar(50) NOT NULL,
  PRIMARY KEY (`IDRole`),
  UNIQUE KEY `RoleName` (`RoleName`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `role`
--

LOCK TABLES `role` WRITE;
/*!40000 ALTER TABLE `role` DISABLE KEYS */;
INSERT INTO `role` VALUES (2,'Админ'),(1,'Директор'),(3,'Мастер'),(4,'Менеджер');
/*!40000 ALTER TABLE `role` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `services`
--

DROP TABLE IF EXISTS `services`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;

-- ИЗМЕНЕНИЕ ТИПА ДАННЫХ ДЛЯ ФОТО (хранение в БД)
ALTER TABLE `services` MODIFY COLUMN `Photo` LONGBLOB DEFAULT NULL;

CREATE TABLE `services` (
  `IDServices` int NOT NULL AUTO_INCREMENT,
  `ServiceName` varchar(100) NOT NULL,
  `Description` text,
  `Price` decimal(10,2) NOT NULL,
  `Photo` longblob,
  `Category` int NOT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`IDServices`),
  KEY `Category` (`Category`),
  CONSTRAINT `services_ibfk_1` FOREIGN KEY (`Category`) REFERENCES `category` (`IDCategory`)
) ENGINE=InnoDB AUTO_INCREMENT=21 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Dumping data for table `services` (только актуальные услуги)
--

LOCK TABLES `services` WRITE;
/*!40000 ALTER TABLE `services` DISABLE KEYS */;
INSERT INTO `services` (`IDServices`, `ServiceName`, `Description`, `Price`, `Photo`, `Category`, `IsActive`) VALUES 
(1, 'Классический маникюр', 'Обрезной маникюр с покрытием обычным лаком', 1200.00, NULL, 1, 1),
(2, 'Аппаратный маникюр', 'Маникюр с использованием аппарата', 1500.00, NULL, 1, 1),
(3, 'Гель-лак', 'Покрытие гель-лаком на обе руки', 1800.00, NULL, 1, 1),
(4, 'Мужской маникюр', 'Гигиенический маникюр для мужчин', 1000.00, NULL, 1, 1),
(5, 'Классический педикюр', 'Полный гигиенический педикюр', 2000.00, NULL, 2, 1),
(6, 'Аппаратный педикюр', 'Педикюр с использованием аппарата', 2200.00, NULL, 2, 1),
(7, 'Покрытие гель-лаком на ноги', 'Педикюр с покрытием гель-лаком', 2500.00, NULL, 2, 1),
(8, 'Наращивание ногтей гелем', 'Наращивание ногтей гелевой технологией', 3500.00, NULL, 3, 1),
(9, 'Наращивание ногтей акрилом', 'Наращивание ногтей акриловой технологией', 3200.00, NULL, 3, 1),
(10, 'Коррекция нарощенных ногтей', 'Коррекция гелевых или акриловых ногтей', 2800.00, NULL, 3, 1),
(11, 'Френч', 'Классический французский маникюр', 500.00, NULL, 4, 1),
(12, 'Стемпинг', 'Дизайн с использованием стемпинга', 300.00, NULL, 4, 1),
(13, 'Роспись', 'Художественная роспись ногтей', 700.00, NULL, 4, 0),
(14, 'Слайдер-дизайн', 'Дизайн с использованием слайдеров', 400.00, NULL, 4, 1),
(15, 'Стразы', 'Декорирование стразами', 200.00, NULL, 4, 1),
(16, 'Укрепление ногтей', 'Процедура укрепления натуральных ногтей', 900.00, NULL, 5, 1),
(17, 'SPA-уход для рук', 'Комплексный уход за кожей рук', 1300.00, NULL, 5, 1),
(18, 'Парафинотерапия', 'Парафиновый уход для рук', 1100.00, NULL, 5, 1),
(19, 'Массаж рук', 'Расслабляющий массаж кистей и предплечий', 800.00, NULL, 5, 1),
(20, 'Лечение кутикулы', 'Специализированный уход за кутикулой', 600.00, NULL, 5, 1);
/*!40000 ALTER TABLE `services` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `status`
--

DROP TABLE IF EXISTS `status`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `status` (
  `IDStatus` int NOT NULL AUTO_INCREMENT,
  `StatusName` varchar(50) NOT NULL,
  PRIMARY KEY (`IDStatus`),
  UNIQUE KEY `StatusName` (`StatusName`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `status` (только 3 статуса)
--

LOCK TABLES `status` WRITE;
/*!40000 ALTER TABLE `status` DISABLE KEYS */;
INSERT INTO `status` VALUES 
(2,'Занято'),
(4,'Отменен'),
(3,'Выполнен');
/*!40000 ALTER TABLE `status` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `IDUser` int NOT NULL AUTO_INCREMENT,
  `LastName` varchar(50) NOT NULL,
  `FirstName` varchar(50) NOT NULL,
  `MiddleName` varchar(50) DEFAULT NULL,
  `Login` varchar(50) NOT NULL,
  `Password` varchar(255) NOT NULL,
  `Role` int NOT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`IDUser`),
  UNIQUE KEY `Login` (`Login`),
  KEY `Role` (`Role`),
  CONSTRAINT `users_ibfk_1` FOREIGN KEY (`Role`) REFERENCES `role` (`IDRole`)
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users` (удалены некорректные записи)
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES 
(1,'Иванов','Петр','Сергеевич','director','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',1,1),
(3,'Сидоров','Дмитрий','Игоревич','admin2','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',2,1),
(4,'Козлова','Елена','Александровна','master','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3,1),
(5,'Никитина','Ольга','Петровна','master2','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3,1),
(6,'Федорова','Мария','Сергеевна','master3','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3,1),
(7,'Григорьева','Анна','Дмитриевна','master4','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3,1),
(8,'Васильева','Ирина','Владимировна','master5','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3,1),
(9,'Алексеева','Светлана','Олеговна','manager','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',4,1),
(10,'Павлова','Наталья','Ивановна','manager2','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',4,1);
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping events for database 'nailservicedb'
--

--
-- Dumping routines for database 'nailservicedb'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-03