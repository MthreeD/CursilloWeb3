CREATE TABLE [ApplicationSettings] (
    [Id] uniqueidentifier NOT NULL,
    [ShowDebuggingPages] bit NOT NULL,
    [ShowHomePage] bit NOT NULL,
    [ShowArticleDetailsPage] bit NOT NULL,
    [ShowCounterPage] bit NOT NULL,
    [ShowWeatherPage] bit NOT NULL,
    [ShowTestPage] bit NOT NULL,
    [ShowTest2Page] bit NOT NULL,
    [ShowDebugPage] bit NOT NULL,
    [ShowAdminCleanupTestPage] bit NOT NULL,
    [ShowFileUploadPage] bit NOT NULL,
    [ShowWebmasterSettingsPage] bit NOT NULL,
    [ShowDashboardPage] bit NOT NULL,
    [ShowManageArticlePage] bit NOT NULL,
    [ShowManageContentPage] bit NOT NULL,
    [ShowManageFooterPage] bit NOT NULL,
    [ShowNewFooterEditPage] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [LastUpdated] datetime2 NOT NULL,
    CONSTRAINT [PK_ApplicationSettings] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Articles] (
    [Id] uniqueidentifier NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [ImageUrl] nvarchar(max) NULL,
    [IsVisible] bit NOT NULL,
    [Order] int NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_Articles] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [ContentBlocks] (
    [Id] uniqueidentifier NOT NULL,
    [Section] nvarchar(max) NOT NULL,
    [HtmlContent] nvarchar(max) NULL,
    [RTFContent] nvarchar(max) NULL,
    [LastUpdated] datetime2 NOT NULL,
    CONSTRAINT [PK_ContentBlocks] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [NewFooterContents] (
    [Id] uniqueidentifier NOT NULL,
    [RTFContent] varbinary(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [LastUpdated] datetime2 NOT NULL,
    CONSTRAINT [PK_NewFooterContents] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Tests] (
    [Id] uniqueidentifier NOT NULL,
    [BackgroundColor] nvarchar(max) NULL,
    [FontName] nvarchar(max) NULL,
    [FontSize] nvarchar(max) NULL,
    [DocumentContent] varbinary(max) NULL,
    [RTFContent] varbinary(max) NULL,
    [HTMLContent] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [LastUpdated] datetime2 NOT NULL,
    CONSTRAINT [PK_Tests] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [WebmasterSettings] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(50) NOT NULL,
    [FullPath] nvarchar(255) NOT NULL,
    [FileExtensions] nvarchar(max) NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    CONSTRAINT [PK_WebmasterSettings] PRIMARY KEY ([Id])
);
GO


