/****** Object:  Database [assignmentdb]    Script Date: 09-10-2023 14:56:20 ******/
CREATE DATABASE [assignmentdb]  (EDITION = 'GeneralPurpose', SERVICE_OBJECTIVE = 'GP_S_Gen5_1', MAXSIZE = 32 GB) WITH CATALOG_COLLATION = SQL_Latin1_General_CP1_CI_AS, LEDGER = OFF;
GO
ALTER DATABASE [assignmentdb] SET COMPATIBILITY_LEVEL = 150
GO
ALTER DATABASE [assignmentdb] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [assignmentdb] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [assignmentdb] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [assignmentdb] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [assignmentdb] SET ARITHABORT OFF 
GO
ALTER DATABASE [assignmentdb] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [assignmentdb] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [assignmentdb] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [assignmentdb] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [assignmentdb] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [assignmentdb] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [assignmentdb] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [assignmentdb] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [assignmentdb] SET ALLOW_SNAPSHOT_ISOLATION ON 
GO
ALTER DATABASE [assignmentdb] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [assignmentdb] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [assignmentdb] SET  MULTI_USER 
GO
ALTER DATABASE [assignmentdb] SET ENCRYPTION ON
GO
ALTER DATABASE [assignmentdb] SET QUERY_STORE = ON
GO
ALTER DATABASE [assignmentdb] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 100, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
/*** The scripts of database scoped configurations in Azure should be executed inside the target database connection. ***/
GO
-- ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = 8;
GO
/****** Object:  Table [dbo].[ExecutionLog]    Script Date: 09-10-2023 14:56:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExecutionLog](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[LastExecutionDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserNotification]    Script Date: 09-10-2023 14:56:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserNotification](
	[RecordId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[UserName] [varchar](250) NOT NULL,
	[UserEmail] [varchar](250) NOT NULL,
	[DataValue] [varchar](250) NOT NULL,
	[NotificationFlag] [bit] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedDate] [datetime] NULL,
	[IsModified] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[RecordId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[UserNotification] ADD  DEFAULT ((0)) FOR [NotificationFlag]
GO
ALTER TABLE [dbo].[UserNotification] ADD  DEFAULT ((0)) FOR [IsModified]
GO
/****** Object:  StoredProcedure [dbo].[GetModifiedRecords]    Script Date: 09-10-2023 14:56:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetModifiedRecords]
    @LastExecutionDate DATETIME
AS
BEGIN
    SELECT *
    FROM UserNotification
    WHERE CreatedDate > @LastExecutionDate OR (ModifiedDate > @LastExecutionDate AND IsModified = 1);
END;
GO
ALTER DATABASE [assignmentdb] SET  READ_WRITE 
GO
