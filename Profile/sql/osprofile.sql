-- MySQL dump 10.15  Distrib 10.0.21-MariaDB, for Linux (x86_64)
--
-- Host: localhost    Database: moodle_db
-- ------------------------------------------------------
-- Server version	10.0.21-MariaDB-log

--
-- Table structure for table `profile_classifieds`
--

CREATE TABLE `profile_classifieds` (
  `classifieduuid`  varchar(36)  NOT NULL DEFAULT '',
  `creatoruuid`     varchar(36)  NOT NULL DEFAULT '',
  `creationdate`    bigint(20)   NOT NULL,
  `expirationdate`  bigint(20)   NOT NULL,
  `category`        varchar(20)  NOT NULL DEFAULT '',
  `name`            varchar(255) NOT NULL DEFAULT '',
  `description`     longtext     NOT NULL,
  `parceluuid`      varchar(36)  NOT NULL DEFAULT '',
  `parentestate`    bigint(11)   NOT NULL,
  `snapshotuuid`    varchar(36)  NOT NULL DEFAULT '',
  `simname`         varchar(255) NOT NULL DEFAULT '',
  `posglobal`       varchar(255) NOT NULL DEFAULT '',
  `parcelname`      varchar(255) NOT NULL DEFAULT '',
  `classifiedflags` int(8)       NOT NULL,
  `priceforlisting` mediumint(5) NOT NULL,
  PRIMARY KEY (`classifieduuid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Table structure for table `profile_usernotes`
--

CREATE TABLE `profile_usernotes` (
  `useruuid`   varchar(36) NOT NULL DEFAULT '',
  `targetuuid` varchar(36) NOT NULL DEFAULT '',
  `notes`      longtext NOT NULL,
  PRIMARY KEY (`useruuid`,`targetuuid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Table structure for table `profile_userpicks`
--

CREATE TABLE `profile_userpicks` (
  `pickuuid`     varchar(36)  NOT NULL DEFAULT '',
  `creatoruuid`  varchar(36)  NOT NULL DEFAULT '',
  `toppick`      varchar(6)   NOT NULL DEFAULT 'false',
  `parceluuid`   varchar(36)  NOT NULL DEFAULT '',
  `name`         varchar(255) NOT NULL DEFAULT '',
  `description`  longtext     NOT NULL,
  `snapshotuuid` varchar(36)  NOT NULL DEFAULT '',
  `user`         varchar(255) NOT NULL DEFAULT '',
  `originalname` varchar(255) NOT NULL DEFAULT '',
  `simname`      varchar(255) NOT NULL DEFAULT '',
  `posglobal`    varchar(255) NOT NULL DEFAULT '',
  `sortorder`    tinyint(2)   NOT NULL,
  `enabled`      varchar(6)   NOT NULL DEFAULT 'false',
  `gatekeeper`   varchar(255) NOT NULL DEFAULT '',
  PRIMARY KEY (`pickuuid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Table structure for table `profile_userprofile`
--

CREATE TABLE `profile_userprofile` (
  `useruuid`             varchar(36)  NOT NULL DEFAULT '',
  `profilepartner`       varchar(36)  NOT NULL DEFAULT '',
  `profileimage`         varchar(36)  NOT NULL DEFAULT '',
  `profileabouttext`     longtext     NOT NULL,
  `profileallowpublish`  binary(1)    NOT NULL,
  `profilematurepublish` binary(1)    NOT NULL,
  `profileurl`           varchar(255) NOT NULL DEFAULT '',
  `profilewanttomask`    smallint(3)  NOT NULL,
  `profilewanttotext`    longtext     NOT NULL,
  `profileskillsmask`    smallint(3)  NOT NULL,
  `profileskillstext`    longtext     NOT NULL,
  `profilelanguagestext` longtext     NOT NULL,
  `profilefirstimage`    varchar(36)  NOT NULL DEFAULT '',
  `profilefirsttext`     longtext     NOT NULL,
  PRIMARY KEY (`useruuid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Table structure for table `profile_usersettings`
--

CREATE TABLE `profile_usersettings` (
  `useruuid`   varchar(36)          NOT NULL DEFAULT '',
  `imviaemail` enum('true','false') NOT NULL DEFAULT 'false',
  `visible`    enum('true','false') NOT NULL DEFAULT 'true',
  `email`      varchar(254)         NOT NULL DEFAULT '',
  PRIMARY KEY (`useruuid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Dump completed on 2016-07-17 17:08:27
