-- MySQL dump 10.13  Distrib 8.0.34, for Win64 (x86_64)
--
-- Host: localhost    Database: risk
-- ------------------------------------------------------
-- Server version	8.0.35

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
-- Table structure for table `schedule_assessment`
--

DROP TABLE IF EXISTS `schedule_assessment`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `schedule_assessment` (
  `Schedule_Assessment_id` int NOT NULL AUTO_INCREMENT,
  `Date_Of_Request` datetime(6) DEFAULT NULL,
  `value_Frequency` int DEFAULT NULL,
  `frequency_period` varchar(45) DEFAULT NULL,
  `Duration_of_Assessment` varchar(45) DEFAULT NULL,
  `repeatEndDate` datetime(6) DEFAULT NULL,
  `userid` int DEFAULT NULL,
  `DocTypeID` int DEFAULT NULL,
  `Doc_CategoryID` int DEFAULT NULL,
  `Doc_SubCategoryID` int DEFAULT NULL,
  `Entity_Master_id` int DEFAULT NULL,
  `Unit_location_Master_id` int DEFAULT NULL,
  `Department_Master_id` int DEFAULT NULL,
  `created_date` datetime(6) DEFAULT NULL,
  `status` varchar(45) DEFAULT NULL,
  `Shuffle_Questions` int DEFAULT NULL,
  `Shuffle_Answers` int DEFAULT NULL,
  `startDate` datetime(5) DEFAULT NULL,
  `endDate` datetime(5) DEFAULT NULL,
  `objective` varchar(45) DEFAULT NULL,
  `message` varchar(45) DEFAULT NULL,
  `ass_template_id` int DEFAULT NULL,
  `AssessmentStatus` varchar(150) DEFAULT NULL,
  `mapped_user` int DEFAULT NULL,
  `tpauserid` int DEFAULT NULL,
  `login_userid` int DEFAULT NULL,
  `uq_ass_schid` varchar(50) DEFAULT NULL,
  `pagetype` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`Schedule_Assessment_id`)
) ENGINE=InnoDB AUTO_INCREMENT=151 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2024-06-03 12:31:47
