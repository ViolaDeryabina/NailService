-- MySqlBackup.NET 2.7.1.0
-- Dump Time: 2026-06-15 19:34:58
-- --------------------------------------
-- Server version 9.4.0 MySQL Community Server - GPL


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;


-- 
-- Definition of category
-- 

DROP TABLE IF EXISTS `category`;
CREATE TABLE IF NOT EXISTS `category` (
  `IDCategory` int NOT NULL AUTO_INCREMENT,
  `CategoryName` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`IDCategory`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 
-- Dumping data for table category
-- 

/*!40000 ALTER TABLE `category` DISABLE KEYS */;
INSERT INTO `category`(`IDCategory`,`CategoryName`,`IsActive`) VALUES(1,'Маникюр',1),(2,'Педикюр',1),(3,'Наращивание',1),(4,'Уход',1);
/*!40000 ALTER TABLE `category` ENABLE KEYS */;

-- 
-- Definition of role
-- 

DROP TABLE IF EXISTS `role`;
CREATE TABLE IF NOT EXISTS `role` (
  `IDRole` int NOT NULL AUTO_INCREMENT,
  `RoleName` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`IDRole`),
  UNIQUE KEY `RoleName` (`RoleName`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 
-- Dumping data for table role
-- 

/*!40000 ALTER TABLE `role` DISABLE KEYS */;
INSERT INTO `role`(`IDRole`,`RoleName`) VALUES(2,'Админ'),(3,'Мастер'),(4,'Менеджер');
/*!40000 ALTER TABLE `role` ENABLE KEYS */;

-- 
-- Definition of services
-- 

DROP TABLE IF EXISTS `services`;
CREATE TABLE IF NOT EXISTS `services` (
  `IDServices` int NOT NULL AUTO_INCREMENT,
  `ServiceName` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `Description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `Price` decimal(10,2) NOT NULL,
  `Photo` longblob,
  `Category` int NOT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`IDServices`),
  KEY `Category` (`Category`),
  CONSTRAINT `services_ibfk_1` FOREIGN KEY (`Category`) REFERENCES `category` (`IDCategory`)
) ENGINE=InnoDB AUTO_INCREMENT=21 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 
-- Dumping data for table services
-- 

/*!40000 ALTER TABLE `services` DISABLE KEYS */;
/*!40000 ALTER TABLE `services` ENABLE KEYS */;

-- 
-- Definition of status
-- 

DROP TABLE IF EXISTS `status`;
CREATE TABLE IF NOT EXISTS `status` (
  `IDStatus` int NOT NULL AUTO_INCREMENT,
  `StatusName` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`IDStatus`),
  UNIQUE KEY `StatusName` (`StatusName`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 
-- Dumping data for table status
-- 

/*!40000 ALTER TABLE `status` DISABLE KEYS */;
/*!40000 ALTER TABLE `status` ENABLE KEYS */;

-- 
-- Definition of users
-- 

DROP TABLE IF EXISTS `users`;
CREATE TABLE IF NOT EXISTS `users` (
  `IDUser` int NOT NULL AUTO_INCREMENT,
  `LastName` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `FirstName` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `MiddleName` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Login` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `Password` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `Role` int NOT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`IDUser`),
  UNIQUE KEY `Login` (`Login`),
  KEY `Role` (`Role`),
  CONSTRAINT `users_ibfk_1` FOREIGN KEY (`Role`) REFERENCES `role` (`IDRole`)
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 
-- Dumping data for table users
-- 

/*!40000 ALTER TABLE `users` DISABLE KEYS */;
/*!40000 ALTER TABLE `users` ENABLE KEYS */;

-- 
-- Definition of masters
-- 

DROP TABLE IF EXISTS `masters`;
CREATE TABLE IF NOT EXISTS `masters` (
  `IDMasters` int NOT NULL AUTO_INCREMENT,
  `User` int NOT NULL,
  `Description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `Phone` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`IDMasters`),
  KEY `User` (`User`),
  CONSTRAINT `masters_ibfk_1` FOREIGN KEY (`User`) REFERENCES `users` (`IDUser`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 
-- Dumping data for table masters
-- 

/*!40000 ALTER TABLE `masters` DISABLE KEYS */;
/*!40000 ALTER TABLE `masters` ENABLE KEYS */;

-- 
-- Definition of record
-- 

DROP TABLE IF EXISTS `record`;
CREATE TABLE IF NOT EXISTS `record` (
  `IDRecord` int NOT NULL AUTO_INCREMENT,
  `Master` int NOT NULL,
  `ClientName` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `ClientPhone` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Date` datetime NOT NULL,
  `Status` int NOT NULL,
  `Service` int NOT NULL,
  `User` int NOT NULL,
  `discount` tinyint(1) DEFAULT '0',
  PRIMARY KEY (`IDRecord`),
  KEY `Master` (`Master`),
  KEY `Status` (`Status`),
  KEY `Service` (`Service`),
  KEY `User` (`User`),
  CONSTRAINT `record_ibfk_1` FOREIGN KEY (`Master`) REFERENCES `masters` (`IDMasters`),
  CONSTRAINT `record_ibfk_3` FOREIGN KEY (`Status`) REFERENCES `status` (`IDStatus`),
  CONSTRAINT `record_ibfk_4` FOREIGN KEY (`Service`) REFERENCES `services` (`IDServices`),
  CONSTRAINT `record_ibfk_5` FOREIGN KEY (`User`) REFERENCES `users` (`IDUser`)
) ENGINE=InnoDB AUTO_INCREMENT=91 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 
-- Dumping data for table record
-- 

/*!40000 ALTER TABLE `record` DISABLE KEYS */;
/*!40000 ALTER TABLE `record` ENABLE KEYS */;


/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;
/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;


-- Dump completed on 2026-06-15 19:34:59
-- Total time: 0:0:0:0:134 (d:h:m:s:ms)
