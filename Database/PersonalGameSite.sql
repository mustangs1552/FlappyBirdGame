USE master
GO

IF EXISTS(SELECT * FROM sys.databases WHERE name='PersonalGameSite')
DROP DATABASE PersonalGameSite
GO

CREATE DATABASE PersonalGameSite
GO

USE PersonalGameSite
GO

BEGIN TRANSACTION;

CREATE TABLE Games
(
	gameID int Identity NOT NULL,
	gameName varchar(50) NOT NULL,
	gameTypeID int NULL,
	gameDiscription varchar(255) NULL,
	gameSrc varchar(255) NOT NULL,

	CONSTRAINT pk_Games_gameID PRIMARY KEY (gameID),
);

CREATE TABLE GameTypes
(
	typeID int Identity NOT NULL,
	name varchar(50) NOT NULL,

	CONSTRAINT pk_GameTypes_typeID PRIMARY KEY(typeID),
);

Create TABLE Reviews
(
	reviewID int Identity NOT NULL,
	gameID int NOT NULL,
	reviewUsername varchar(50) NOT NULL,
	reviewMsg varchar(50) NOT NULL,
	reviewScore int NOT NULL
	
	CONSTRAINT pk_Reviews_reviewID PRIMARY KEY (reviewID),
	CONSTRAINT fk_Reviews_gameID FOREIGN KEY (gameID) REFERENCES Games(gameID),
);

CREATE TABLE HighScores
(
	scoreID int Identity NOT NULL,
	gameID int NOT NULL,
	scoreUsername varchar(50) NOT NULL,
	score int NOT NULL
	
	CONSTRAINT pk_HighScores_scoreID PRIMARY KEY (scoreID),
	CONSTRAINT fk_HighScores_gameID FOREIGN KEY (gameID) REFERENCES Games(gameID),
);

CREATE TABLE Games_GameTypes
(
	gameID int NOT NULL,
	typeID int NOT NULL,
	
	CONSTRAINT fk_Games_GameTypes_gameID FOREIGN KEY (gameID) REFERENCES Games(gameID),
	CONSTRAINT fk_Games_GameTypes_typeID FOREIGN KEY (typeID) REFERENCES GameTypes(typeID),
);

COMMIT TRANSACTION;