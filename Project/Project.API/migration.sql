CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `Projects` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` int NOT NULL,
    `Avatar` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Company` longtext CHARACTER SET utf8mb4 NOT NULL,
    `OriginBPFile` longtext CHARACTER SET utf8mb4 NOT NULL,
    `FormatBPFile` longtext CHARACTER SET utf8mb4 NOT NULL,
    `ShowSecurityInfo` tinyint(1) NOT NULL,
    `ProvinceId` int NOT NULL,
    `Provice` longtext CHARACTER SET utf8mb4 NOT NULL,
    `CityId` int NOT NULL,
    `City` longtext CHARACTER SET utf8mb4 NOT NULL,
    `AreaId` int NOT NULL,
    `AreaName` longtext CHARACTER SET utf8mb4 NOT NULL,
    `RegisterTime` datetime(6) NOT NULL,
    `Introduction` longtext CHARACTER SET utf8mb4 NOT NULL,
    `FinPercentage` longtext CHARACTER SET utf8mb4 NOT NULL,
    `FinStage` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Income` int NOT NULL,
    `Revenue` int NOT NULL,
    `Valuation` int NOT NULL,
    `BrokerageOptions` int NOT NULL,
    `OnPlatform` tinyint(1) NOT NULL,
    `SourceId` int NOT NULL,
    `ReferenceId` int NOT NULL,
    `Tags` longtext CHARACTER SET utf8mb4 NOT NULL,
    `CreateTime` datetime(6) NOT NULL,
    `UpdateTime` datetime(6) NOT NULL,
    CONSTRAINT `PK_Projects` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `ProjectContributors` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ProjectId` int NOT NULL,
    `UserId` int NOT NULL,
    `UserName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Avatar` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedTime` datetime(6) NOT NULL,
    `IsCloser` tinyint(1) NOT NULL,
    `ContributorType` int NOT NULL,
    CONSTRAINT `PK_ProjectContributors` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ProjectContributors_Projects_ProjectId` FOREIGN KEY (`ProjectId`) REFERENCES `Projects` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `ProjectProperties` (
    `Key` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Value` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
    `ProjectId` int NOT NULL,
    `Text` longtext CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_ProjectProperties` PRIMARY KEY (`ProjectId`, `Key`, `Value`),
    CONSTRAINT `FK_ProjectProperties_Projects_ProjectId` FOREIGN KEY (`ProjectId`) REFERENCES `Projects` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `ProjectViewers` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ProjectId` int NOT NULL,
    `UserId` int NOT NULL,
    `UserName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Avatar` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedTime` datetime(6) NOT NULL,
    CONSTRAINT `PK_ProjectViewers` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ProjectViewers_Projects_ProjectId` FOREIGN KEY (`ProjectId`) REFERENCES `Projects` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `ProjectVisibleRules` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ProjectId` int NOT NULL,
    `Visible` tinyint(1) NOT NULL,
    `Tags` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_ProjectVisibleRules` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ProjectVisibleRules_Projects_ProjectId` FOREIGN KEY (`ProjectId`) REFERENCES `Projects` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_ProjectContributors_ProjectId` ON `ProjectContributors` (`ProjectId`);

CREATE INDEX `IX_ProjectViewers_ProjectId` ON `ProjectViewers` (`ProjectId`);

CREATE UNIQUE INDEX `IX_ProjectVisibleRules_ProjectId` ON `ProjectVisibleRules` (`ProjectId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20251109092230_initProjectDb', '8.0.0');

COMMIT;

