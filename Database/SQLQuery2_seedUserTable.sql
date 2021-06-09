USE [cs3750]
GO

/****** Object:  Table [dbo].[Users]    Script Date: 6/9/2021 12:04:29 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Users](
	[userID] [int] IDENTITY(1,1) NOT NULL,
	[email] [varchar](100) NOT NULL,
	[firstName] [varchar](100) NOT NULL,
	[lastName] [varchar](100) NOT NULL,
	[birthday] [date] NOT NULL,
	[password] [varchar](256) NOT NULL,
	[accountType] [smallint] NOT NULL,
	[address1] [varchar](100) NULL,
	[address2] [varchar](100) NULL,
	[city] [varchar](100) NULL,
	[state] [int] NULL,
	[zip] [varchar](30) NULL,
	[phone] [varchar](30) NULL,
	[image] [varchar](256) NULL,
	[LinkedIn] [varchar](256) NULL,
	[Github] [varchar](256) NULL,
	[Twitter] [varchar](256) NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[userID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

