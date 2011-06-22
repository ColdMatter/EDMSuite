IF EXISTS(SELECT name FROM sys.databases WHERE name = 'EDMDatabase') DROP DATABASE [EDMDatabase]
GO

CREATE DATABASE EDMDatabase 
ON
PRIMARY ( NAME = edm,
    FILENAME = 'c:\edmdatabase\edm.mdf'),
FILEGROUP FileStreamGroup1 CONTAINS FILESTREAM( NAME = Arch3,
    FILENAME = 'c:\edmdatabase\filestream1')
LOG ON  ( NAME = edmlog,
    FILENAME = 'c:\edmdatabase\edmlog.ldf')
GO

BEGIN
CREATE TABLE EDMDatabase.dbo.DBBlocks (
	[blockID] [int] IDENTITY(1,1) NOT NULL,
	[cluster] [varchar](50) COLLATE Latin1_General_CI_AS NOT NULL,
	[clusterIndex] [int] NOT NULL,
	[include] [bit] NOT NULL,
	[eState] [bit] NOT NULL,
	[bState] [bit] NOT NULL,
	[rfState] [bit] NOT NULL,
	[ePlus] [float] NOT NULL,
	[eMinus] [float] NOT NULL,
	[blockTime] [datetime] NOT NULL,
	[configBytes] [varbinary](max) NOT NULL,
 CONSTRAINT [PK_DBBlocks] PRIMARY KEY CLUSTERED 
(
	[blockID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END

BEGIN
CREATE TABLE EDMDatabase.dbo.DBTOFChannelSets (
	[tcsID] [int] IDENTITY(1,1) NOT NULL,
	[blockID] [int] NOT NULL,
	[detector] [varchar](50) COLLATE Latin1_General_CI_AS NOT NULL,
	[FileID] [uniqueidentifier] ROWGUIDCOL NOT NULL UNIQUE,
	[tcsData] [varbinary](max) FILESTREAM NOT NULL,
 CONSTRAINT [PK_DBTOFChannelSets] PRIMARY KEY CLUSTERED 
(
	[tcsID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO

BEGIN
CREATE TABLE EDMDatabase.dbo.DBDBlocks (
	[dblockID] [int] IDENTITY(1,1) NOT NULL,
	[blockID] [int] NOT NULL,
	[aTag] [varchar](50) COLLATE Latin1_General_CI_AS NOT NULL,
	[FileID] [uniqueidentifier] ROWGUIDCOL NOT NULL UNIQUE,
	[dblockData] [varbinary](max) FILESTREAM NOT NULL,
 CONSTRAINT [PK_DBDBlocks] PRIMARY KEY CLUSTERED 
(
	[dblockID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO

ALTER TABLE EDMDatabase.dbo.DBTOFChannelSets  WITH CHECK ADD  CONSTRAINT [FK_DBTOFChannelSets_DBBlocks] FOREIGN KEY([blockID])
REFERENCES EDMDatabase.dbo.DBBlocks ([blockID])
GO

ALTER TABLE EDMDatabase.dbo.DBTOFChannelSets CHECK CONSTRAINT [FK_DBTOFChannelSets_DBBlocks]
GO

ALTER TABLE EDMDatabase.dbo.DBDBlocks  WITH CHECK ADD  CONSTRAINT [FK_DBDBlocks_DBBlocks] FOREIGN KEY([blockID])
REFERENCES EDMDatabase.dbo.DBBlocks ([blockID])
GO

ALTER TABLE EDMDatabase.dbo.DBDBlocks CHECK CONSTRAINT [FK_DBDBlocks_DBBlocks]
GO
