-- MySQL dump 10.13  Distrib 8.0.34, for Win64 (x86_64)
--
-- Host: localhost    Database: risk
-- ------------------------------------------------------
-- Server version	8.0.34

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
-- Table structure for table `doc_taskuseracknowledment_status`
--

DROP TABLE IF EXISTS `doc_taskuseracknowledment_status`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `doc_taskuseracknowledment_status` (
  `doc_taskuseracknowledment_id` int NOT NULL AUTO_INCREMENT,
  `USR_ID` int DEFAULT NULL,
  `AddDoc_id` int DEFAULT NULL,
  `Document_Id` varchar(100) DEFAULT NULL,
  `ack_status` varchar(50) DEFAULT NULL,
  `status` varchar(50) DEFAULT NULL,
  `createddate` date DEFAULT NULL,
  `user_location_mapping_id` int DEFAULT NULL,
  `Favorite` tinyint DEFAULT NULL,
  `Doc_User_Access_mapping_id` int DEFAULT NULL,
  PRIMARY KEY (`doc_taskuseracknowledment_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `doc_taskuseracknowledment_status`
--

LOCK TABLES `doc_taskuseracknowledment_status` WRITE;
/*!40000 ALTER TABLE `doc_taskuseracknowledment_status` DISABLE KEYS */;
/*!40000 ALTER TABLE `doc_taskuseracknowledment_status` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `doc_user_access_mapping`
--

DROP TABLE IF EXISTS `doc_user_access_mapping`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `doc_user_access_mapping` (
  `Doc_User_Access_mapping_id` int NOT NULL AUTO_INCREMENT,
  `Doc_User_Access_mapping_Status` varchar(45) DEFAULT NULL,
  `Doc_User_Access_mapping_createdDate` date DEFAULT NULL,
  `Unit_location_Master_id` int DEFAULT NULL,
  `AddDoc_id` int DEFAULT NULL,
  `Entity_Master_id` int DEFAULT NULL,
  `ack_status` varchar(45) DEFAULT NULL,
  `duedate` date DEFAULT NULL,
  `timeline` varchar(45) DEFAULT NULL,
  `trakstatus` varchar(45) DEFAULT NULL,
  `optionalreminder` varchar(45) DEFAULT NULL,
  `noofdays` int DEFAULT NULL,
  `everyday` int DEFAULT NULL,
  `timeperiod` varchar(45) DEFAULT NULL,
  `reqtimeperiod` varchar(45) DEFAULT NULL,
  `Document_Id` varchar(45) DEFAULT NULL,
  `createdBy` int DEFAULT NULL,
  PRIMARY KEY (`Doc_User_Access_mapping_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `doc_user_access_mapping`
--

LOCK TABLES `doc_user_access_mapping` WRITE;
/*!40000 ALTER TABLE `doc_user_access_mapping` DISABLE KEYS */;
/*!40000 ALTER TABLE `doc_user_access_mapping` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `doc_user_permission_mapping`
--

DROP TABLE IF EXISTS `doc_user_permission_mapping`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `doc_user_permission_mapping` (
  `doc_user_permission_mapping_pkid` int NOT NULL AUTO_INCREMENT,
  `Doc_User_Access_mapping_id` int DEFAULT NULL,
  `Doc_perm_rights_id` int DEFAULT NULL,
  `USR_ID` int DEFAULT NULL,
  `permissioncreateddate` datetime DEFAULT NULL,
  `permissionstatus` varchar(45) DEFAULT NULL,
  `ack_status` varchar(45) DEFAULT NULL,
  `user_location_mapping_id` int DEFAULT NULL,
  `AddDoc_id` int DEFAULT NULL,
  PRIMARY KEY (`doc_user_permission_mapping_pkid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `doc_user_permission_mapping`
--

LOCK TABLES `doc_user_permission_mapping` WRITE;
/*!40000 ALTER TABLE `doc_user_permission_mapping` DISABLE KEYS */;
/*!40000 ALTER TABLE `doc_user_permission_mapping` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2024-06-03 15:53:25
