-- MySQL dump 10.15  Distrib 10.0.21-MariaDB, for Linux (x86_64)
--
-- Host: localhost    Database: moodle_db
-- ------------------------------------------------------
-- Server version	10.0.21-MariaDB-log

--
-- Table structure for table `mute_list`
--

CREATE TABLE `mute_list` (
  `agentid`   varchar(36)  NOT NULL DEFAULT '',
  `muteid`    varchar(36)  NOT NULL DEFAULT '',
  `mutename`  varchar(255) NOT NULL DEFAULT '',
  `mutetype`  bigint(10)   NOT NULL DEFAULT '0',
  `muteflags` bigint(10)   NOT NULL DEFAULT '0',
  `timestamp` bigint(11)   NOT NULL DEFAULT '0',
  PRIMARY KEY (`agentid`,`muteid`,`mutename`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Dump completed on 2016-07-17 17:08:27
