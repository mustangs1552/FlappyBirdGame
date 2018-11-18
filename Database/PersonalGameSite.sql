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
	gameType varchar(50) NULL,
	gameDiscription varchar(255) NULL,

	CONSTRAINT pk_Games_gameID PRIMARY KEY (gameID),
);

Create TABLE Reviews
(
	reviewID int Identity NOT NULL,
	gameID int NOT NULL,
	reviewUsername varchar(50) NOT NULL,
	reviewMsg varchar(50) NOT NULL,
	reviewScore int NOT NULL
	
	CONSTRAINT pk_Reviews_reviewID PRIMARY KEY (reviewID),
	CONSTRAINT pk_Reviews_gameID FOREIGN KEY (gameID) REFERENCES Games(gameID),
);

CREATE TABLE HighScores
(
	scoreID int Identity NOT NULL,
	gameID int NOT NULL,
	scoreUsername varchar(50) NOT NULL,
	score int NOT NULL
	
	CONSTRAINT pk_HighScores_scoreID PRIMARY KEY (scoreID),
	CONSTRAINT pk_HighScores_gameID FOREIGN KEY (gameID) REFERENCES Games(gameID),
);

CREATE TABLE Games_Reviews
(
	gameID int NOT NULL,
	reviewID int NOT NULL
	
	CONSTRAINT fk_Games_Reviews_gameID FOREIGN KEY (gameID) REFERENCES Games(gameID),
	CONSTRAINT fk_Games_Reviews_reviewID FOREIGN KEY (reviewID) REFERENCES Games(gameID),
);

CREATE TABLE Games_HighScores
(
	gameID int NOT NULL,
	highscoreID int NOT NULL
	
	CONSTRAINT fk_Games_HighScores_gameID FOREIGN KEY (gameID) REFERENCES Games(gameID),
	CONSTRAINT fk_Games_HighScores_highscoreID FOREIGN KEY (highscoreID) REFERENCES Games(gameID),
);

COMMIT TRANSACTION;