-- MySQL dump 10.15  Distrib 10.0.21-MariaDB, for Linux (x86_64)
--
-- Host: localhost    Database: moodle_db
-- ------------------------------------------------------
-- Server version	10.0.21-MariaDB-log

--
-- Table structure for table `search_allparcels`
--

CREATE TABLE `search_allparcels` (
  `regionuuid`   varchar(36)  NOT NULL DEFAULT '',
  `parcelname`   varchar(255) NOT NULL DEFAULT '',
  `owneruuid`    varchar(36)  NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
  `groupuuid`    varchar(36)  NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
  `landingpoint` varchar(255) NOT NULL DEFAULT '',
  `parceluuid`   varchar(36)  NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
  `infouuid`     varchar(36)  NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
  `parcelarea`   bigint(11)   NOT NULL,
  PRIMARY KEY (`parceluuid`),
  KEY `regionuuid` (`regionuuid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Table structure for table `search_events`
--

CREATE TABLE `search_events` (
  `owneruuid`   varchar(36)  NOT NULL DEFAULT '',
  `name`        varchar(255) NOT NULL DEFAULT '',
  `eventid`     bigint(11)   NOT NULL NOT NULL AUTO_INCREMENT,
  `creatoruuid` varchar(36)  NOT NULL DEFAULT '',
  `category`    tinyint(2)   NOT NULL,
  `description` longtext     NOT NULL,
  `dateutc`     bigint(10)   NOT NULL,
  `duration`    bigint(10)   NOT NULL,
  `covercharge` bigint(10)   NOT NULL,
  `coveramount` bigint(10)   NOT NULL,
  `simname`     varchar(255) NOT NULL DEFAULT '',
  `globalpos`   varchar(255) NOT NULL DEFAULT '',
  `eventflags`  tinyint(1)   NOT NULL,
  PRIMARY KEY (`eventid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Table structure for table `search_hostsregister`
--

CREATE TABLE `search_hostsregister` (
  `host`        varchar(255) NOT NULL DEFAULT '',
  `port`        mediumint(5) NOT NULL,
  `register`    bigint(10)   NOT NULL,
  `nextcheck`   bigint(10)   NOT NULL,
  `checked`     tinyint(1)   NOT NULL,
  `failcounter` bigint(10)   NOT NULL,
  PRIMARY KEY (`host`,`port`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Table structure for table `search_objects`
--

CREATE TABLE `search_objects` (
  `objectuuid`  varchar(36)  NOT NULL DEFAULT '',
  `parceluuid`  varchar(36)  NOT NULL DEFAULT '',
  `location`    varchar(255) NOT NULL DEFAULT '',
  `name`        varchar(255) NOT NULL DEFAULT '',
  `description` varchar(255) NOT NULL DEFAULT '',
  `regionuuid`  varchar(36)  NOT NULL DEFAULT '',
  PRIMARY KEY (`objectuuid`,`parceluuid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Table structure for table `search_parcels`
--

CREATE TABLE `search_parcels` (
  `regionuuid`     varchar(36)          NOT NULL DEFAULT '',
  `parcelname`     varchar(255)         NOT NULL DEFAULT '',
  `parceluuid`     varchar(36)          NOT NULL DEFAULT '',
  `landingpoint`   varchar(255)         NOT NULL DEFAULT '',
  `description`    varchar(255)         NOT NULL DEFAULT '',
  `searchcategory` varchar(50)          NOT NULL DEFAULT '',
  `build`          enum('true','false') NOT NULL DEFAULT 'false',
  `script`         enum('true','false') NOT NULL DEFAULT 'false',
  `public`         enum('true','false') NOT NULL DEFAULT 'false',
  `dwell`          decimal(20,8)        NOT NULL,
  `infouuid`       varchar(36)          NOT NULL DEFAULT '',
  `mature`         varchar(10)          NOT NULL DEFAULT 'PG',
  PRIMARY KEY (`regionuuid`,`parceluuid`),
  KEY `name` (`parcelname`),
  KEY `description` (`description`),
  KEY `searchcategory` (`searchcategory`),
  KEY `dwell` (`dwell`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Table structure for table `search_parcelsales`
--

CREATE TABLE `search_parcelsales` (
  `regionuuid`   varchar(36)  NOT NULL DEFAULT '',
  `parcelname`   varchar(255) NOT NULL DEFAULT '',
  `parceluuid`   varchar(36)  NOT NULL DEFAULT '',
  `area`         mediumint(6) NOT NULL,
  `saleprice`    bigint(11)   NOT NULL,
  `landingpoint` varchar(255) NOT NULL DEFAULT '',
  `infouuid`     varchar(36)  NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
  `dwell`        bigint(11)   NOT NULL,
  `parentestate` bigint(11)   NOT NULL DEFAULT '1',
  `mature`       varchar(10)  NOT NULL DEFAULT 'PG',
  PRIMARY KEY (`regionuuid`,`parceluuid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Table structure for table `search_popularplaces`
--

CREATE TABLE `search_popularplaces` (
  `id`          bigint(10)    NOT NULL AUTO_INCREMENT,
  `regionuuid`  varchar(36)   NOT NULL DEFAULT '',
  `name`        varchar(255)  NOT NULL DEFAULT '',
  `dwell`       decimal(20,8) NOT NULL,
  `infouuid`    varchar(32)   NOT NULL DEFAULT '',
  `has_picture` smallint(4)   NOT NULL,
  `mature`      smallint(4)   NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Table structure for table `search_regions`
--

CREATE TABLE `search_regions` (
  `regionname`   varchar(255) NOT NULL DEFAULT '',
  `regionuuid`   varchar(36)  NOT NULL DEFAULT '',
  `regionhandle` varchar(255) NOT NULL DEFAULT '',
  `url`          varchar(255) NOT NULL DEFAULT '',
  `owner`        varchar(255) NOT NULL DEFAULT '',
  `owneruuid`    varchar(36)  NOT NULL DEFAULT '',
  PRIMARY KEY (`regionuuid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Dump completed on 2016-07-17 17:08:27
