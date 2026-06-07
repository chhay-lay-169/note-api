IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'NotesDb')
BEGIN
    CREATE DATABASE NotesDb;
END
GO

USE NotesDb;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Users] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [Email] NVARCHAR(255) NOT NULL UNIQUE,
        [PasswordHash] NVARCHAR(MAX) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserRefreshTokens]') AND type in (N'U'))
BEGIN
    CREATE TABLE UserRefreshTokens (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Token NVARCHAR(1000) NOT NULL UNIQUE,
        IsUsed BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    -- Index for instant lookups during refresh operations
    CREATE NONCLUSTERED INDEX IX_UserRefreshTokens_Token ON UserRefreshTokens(Token);
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Notes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Notes] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [UserId] INT NOT NULL,
        [Title] NVARCHAR(255) NOT NULL,
        [Content] NVARCHAR(MAX),
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_Notes_Users FOREIGN KEY (UserId) REFERENCES [dbo].[Users](Id)
    );
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Notes_Users')
    BEGIN
        ALTER TABLE [dbo].[Notes] ADD CONSTRAINT FK_Notes_Users FOREIGN KEY (UserId) REFERENCES [dbo].[Users](Id);
    END
END
GO
