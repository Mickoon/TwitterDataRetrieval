USE [TwitterDataCollectionForRyan.TwitterContext]
GO
/****** Object:  Table [dbo].[Coordinates]    Script Date: 17/06/2014 11:23:44 a.m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Coordinates](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Latitude] [float] NOT NULL,
	[Longitude] [float] NOT NULL,
	[TwitterBoundingBox_id] [int] NULL,
	[TwitterGeo_id] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[EdmMetadata]    Script Date: 17/06/2014 11:23:44 a.m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EdmMetadata](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ModelHash] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[FollowerFriendship]    Script Date: 17/06/2014 11:23:44 a.m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FollowerFriendship](
	[UserId] [decimal](38, 0) NOT NULL,
	[IsFollowersAdded] [bit] NOT NULL,
	[IsFriendsAdded] [bit] NOT NULL,
	[IsTweetsAdded] [bit] NOT NULL,
	[IsTweetsInProgress] [bit] NOT NULL,
 CONSTRAINT [PK_FollowerFriendship] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Friendship]    Script Date: 17/06/2014 11:23:44 a.m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Friendship](
	[UserId] [decimal](38, 0) NOT NULL,
	[FollowerUserId] [decimal](38, 0) NOT NULL,
	[RecordedAt] [datetime] NOT NULL,
	[IsProcessed] [bit] NOT NULL,
	[IsTweetsAdded] [bit] NOT NULL,
	[IsTweetsInProgress] [bit] NOT NULL,
 CONSTRAINT [PK_Friendship] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[FollowerUserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[TwitterBoundingBoxes]    Script Date: 17/06/2014 11:23:44 a.m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TwitterBoundingBoxes](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Type] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[TwitterEntities]    Script Date: 17/06/2014 11:23:44 a.m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TwitterEntities](
	[TwitterEntityID] [int] IDENTITY(1,1) NOT NULL,
	[StartIndex] [int] NOT NULL,
	[EndIndex] [int] NOT NULL,
	[Text] [nvarchar](max) NULL,
	[ScreenName] [nvarchar](max) NULL,
	[Name] [nvarchar](max) NULL,
	[UserId] [decimal](38, 0) NULL,
	[Url] [nvarchar](max) NULL,
	[DisplayUrl] [nvarchar](max) NULL,
	[ExpandedUrl] [nvarchar](max) NULL,
	[Id] [decimal](38, 0) NULL,
	[IdString] [nvarchar](max) NULL,
	[MediaUrl] [nvarchar](max) NULL,
	[MediaUrlSecure] [nvarchar](max) NULL,
	[Discriminator] [nvarchar](128) NOT NULL,
	[TwitterStatus_Id] [char](20) NULL,
 CONSTRAINT [PK__TwitterE__D93B1DCF0AD2A005] PRIMARY KEY CLUSTERED 
(
	[TwitterEntityID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[TwitterGeos]    Script Date: 17/06/2014 11:23:44 a.m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TwitterGeos](
	[id] [int] IDENTITY(1,1) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[TwitterPlaces]    Script Date: 17/06/2014 11:23:44 a.m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TwitterPlaces](
	[Id] [nvarchar](128) NOT NULL,
	[CountryCode] [nvarchar](max) NULL,
	[PlaceType] [nvarchar](max) NULL,
	[DataAddress] [nvarchar](max) NULL,
	[Country] [nvarchar](max) NULL,
	[StreetAddress] [nvarchar](max) NULL,
	[FullName] [nvarchar](max) NULL,
	[Name] [nvarchar](max) NULL,
	[BoundingBox_id] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[TwitterStatus]    Script Date: 17/06/2014 11:23:44 a.m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TwitterStatus](
	[Id] [char](20) NOT NULL,
	[StringId] [nvarchar](20) NULL,
	[IsTruncated] [bit] NULL,
	[CreatedDate] [datetime] NOT NULL,
	[Source] [nvarchar](max) NULL,
	[InReplyToScreenName] [nvarchar](200) NULL,
	[InReplyToUserId] [decimal](38, 0) NULL,
	[InReplyToStatusId] [char](20) NULL,
	[IsFavorited] [bit] NULL,
	[Text] [nvarchar](max) NULL,
	[RetweetCountString] [nvarchar](max) NULL,
	[Retweeted] [bit] NOT NULL,
	[RetweetedStatus_Id] [char](20) NULL,
	[Place_Id] [nvarchar](128) NULL,
	[Geo_id] [int] NULL,
	[User_Id] [decimal](38, 0) NULL,
 CONSTRAINT [PK__TwitterS__3214EC07164452B1] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[TwitterUsers]    Script Date: 17/06/2014 11:23:44 a.m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TwitterUsers](
	[Id] [decimal](38, 0) NOT NULL,
	[StringId] [nvarchar](20) NULL,
	[Name] [nvarchar](max) NULL,
	[Location] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[CreatedDate] [datetime] NULL,
	[TimeZone] [nvarchar](max) NULL,
	[NumberOfFollowers] [bigint] NULL,
	[NumberOfStatuses] [bigint] NOT NULL,
	[NumberOfFriends] [bigint] NOT NULL,
	[IsContributorsEnabled] [bit] NOT NULL,
	[Language] [nvarchar](max) NULL,
	[DoesReceiveNotifications] [bit] NULL,
	[ScreenName] [nvarchar](200) NULL,
	[IsFollowing] [bit] NULL,
	[IsFollowedBy] [bit] NULL,
	[NumberOfFavorites] [bigint] NOT NULL,
	[IsProtected] [bit] NOT NULL,
	[IsGeoEnabled] [bit] NULL,
	[TimeZoneOffset] [float] NULL,
	[Website] [nvarchar](max) NULL,
	[ListedCount] [int] NOT NULL,
	[FollowRequestSent] [bit] NULL,
	[Verified] [bit] NULL,
	[ProfileBackgroundColorString] [nvarchar](max) NULL,
	[IsProfileBackgroundTiled] [bit] NULL,
	[ProfileLinkColorString] [nvarchar](max) NULL,
	[ProfileBackgroundImageLocation] [nvarchar](max) NULL,
	[ProfileTextColorString] [nvarchar](max) NULL,
	[ProfileImageLocation] [nvarchar](max) NULL,
	[ProfileImageSecureLocation] [nvarchar](max) NULL,
	[ProfileSidebarBorderColorString] [nvarchar](max) NULL,
 CONSTRAINT [PK__TwitterU__3214EC071A14E395] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
ALTER TABLE [dbo].[FollowerFriendship] ADD  CONSTRAINT [DF_FollowerFriendship_IsFollowersAdded]  DEFAULT ((0)) FOR [IsFollowersAdded]
GO
ALTER TABLE [dbo].[FollowerFriendship] ADD  CONSTRAINT [DF_FollowerFriendship_IsFriendsAdded]  DEFAULT ((0)) FOR [IsFriendsAdded]
GO
ALTER TABLE [dbo].[FollowerFriendship] ADD  CONSTRAINT [DF_FollowerFriendship_IsTweetsAdded]  DEFAULT ((0)) FOR [IsTweetsAdded]
GO
ALTER TABLE [dbo].[FollowerFriendship] ADD  CONSTRAINT [DF_FollowerFriendship_IsTweetsInProgress]  DEFAULT ((0)) FOR [IsTweetsInProgress]
GO
ALTER TABLE [dbo].[Friendship] ADD  CONSTRAINT [DF_Friendship_RecordedAt]  DEFAULT (((9999)-(12))-(31)) FOR [RecordedAt]
GO
ALTER TABLE [dbo].[Friendship] ADD  CONSTRAINT [DF_Friendship_IsProcessed]  DEFAULT ((0)) FOR [IsProcessed]
GO
ALTER TABLE [dbo].[Friendship] ADD  CONSTRAINT [DF_Friendship_IsTweetsAdded]  DEFAULT ((0)) FOR [IsTweetsAdded]
GO
ALTER TABLE [dbo].[Friendship] ADD  CONSTRAINT [DF_Friendship_IsTweetsInProgress]  DEFAULT ((0)) FOR [IsTweetsInProgress]
GO
ALTER TABLE [dbo].[Coordinates]  WITH CHECK ADD  CONSTRAINT [TwitterBoundingBox_Coordinates] FOREIGN KEY([TwitterBoundingBox_id])
REFERENCES [dbo].[TwitterBoundingBoxes] ([id])
GO
ALTER TABLE [dbo].[Coordinates] CHECK CONSTRAINT [TwitterBoundingBox_Coordinates]
GO
ALTER TABLE [dbo].[Coordinates]  WITH CHECK ADD  CONSTRAINT [TwitterGeo_Coordinates] FOREIGN KEY([TwitterGeo_id])
REFERENCES [dbo].[TwitterGeos] ([id])
GO
ALTER TABLE [dbo].[Coordinates] CHECK CONSTRAINT [TwitterGeo_Coordinates]
GO
ALTER TABLE [dbo].[TwitterEntities]  WITH CHECK ADD  CONSTRAINT [TwitterStatus_Entities] FOREIGN KEY([TwitterStatus_Id])
REFERENCES [dbo].[TwitterStatus] ([Id])
GO
ALTER TABLE [dbo].[TwitterEntities] CHECK CONSTRAINT [TwitterStatus_Entities]
GO
ALTER TABLE [dbo].[TwitterPlaces]  WITH CHECK ADD  CONSTRAINT [TwitterPlace_BoundingBox] FOREIGN KEY([BoundingBox_id])
REFERENCES [dbo].[TwitterBoundingBoxes] ([id])
GO
ALTER TABLE [dbo].[TwitterPlaces] CHECK CONSTRAINT [TwitterPlace_BoundingBox]
GO
ALTER TABLE [dbo].[TwitterStatus]  WITH CHECK ADD  CONSTRAINT [TwitterStatus_Geo] FOREIGN KEY([Geo_id])
REFERENCES [dbo].[TwitterGeos] ([id])
GO
ALTER TABLE [dbo].[TwitterStatus] CHECK CONSTRAINT [TwitterStatus_Geo]
GO
ALTER TABLE [dbo].[TwitterStatus]  WITH CHECK ADD  CONSTRAINT [TwitterStatus_Place] FOREIGN KEY([Place_Id])
REFERENCES [dbo].[TwitterPlaces] ([Id])
GO
ALTER TABLE [dbo].[TwitterStatus] CHECK CONSTRAINT [TwitterStatus_Place]
GO
ALTER TABLE [dbo].[TwitterStatus]  WITH CHECK ADD  CONSTRAINT [TwitterStatus_RetweetedStatus] FOREIGN KEY([RetweetedStatus_Id])
REFERENCES [dbo].[TwitterStatus] ([Id])
GO
ALTER TABLE [dbo].[TwitterStatus] CHECK CONSTRAINT [TwitterStatus_RetweetedStatus]
GO
ALTER TABLE [dbo].[TwitterStatus]  WITH CHECK ADD  CONSTRAINT [TwitterUser_Status] FOREIGN KEY([User_Id])
REFERENCES [dbo].[TwitterUsers] ([Id])
GO
ALTER TABLE [dbo].[TwitterStatus] CHECK CONSTRAINT [TwitterUser_Status]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'this column is not used. use IdString instead.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TwitterEntities', @level2type=N'COLUMN',@level2name=N'Id'
GO
