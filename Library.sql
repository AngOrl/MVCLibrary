
IF DB_ID(N'Library') IS NULL
BEGIN
    CREATE DATABASE Library;
END
GO

USE Library;
GO


IF OBJECT_ID('dbo.BookAutors', 'U') IS NOT NULL DROP TABLE dbo.BookAutors;
IF OBJECT_ID('dbo.Books', 'U') IS NOT NULL DROP TABLE dbo.Books;
IF OBJECT_ID('dbo.Autors', 'U') IS NOT NULL DROP TABLE dbo.Autors;
GO

CREATE TABLE dbo.Books(
    ID int IDENTITY (1, 1) PRIMARY KEY,
    Title VARCHAR(255) NOT NULL,
    Year int NOT NULL,
    Edition VARCHAR(255) NOT NULL,
    NumberPages int NOT NULL,
    Contents xml NULL
);
GO

CREATE TABLE dbo.Autors(
    ID int IDENTITY (1, 1) PRIMARY KEY,
    FirstName varchar(255) NOT NULL,
    Surname varchar(255) NOT NULL
);
GO

CREATE TABLE dbo.BookAutors
(
    BookId  INT NOT NULL,
    AutorId INT NOT NULL,

    CONSTRAINT PK_BookAutors PRIMARY KEY (BookId, AutorId),

    CONSTRAINT FK_BA_Books FOREIGN KEY (BookId)
        REFERENCES dbo.Books(ID) ON DELETE CASCADE,

    CONSTRAINT FK_BA_Autors FOREIGN KEY (AutorId)
        REFERENCES dbo.Autors(ID) ON DELETE CASCADE
);
GO

CREATE UNIQUE INDEX UX_Autors_Name
ON dbo.Autors(Surname, FirstName);
GO



-- InsertBooks
CREATE OR ALTER PROCEDURE dbo.InsertBooks
    @Year INT,
    @Title VARCHAR(255),
    @Edition VARCHAR(255),
    @NumberPages INT,
    @Contents XML = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Title IS NULL OR LTRIM(RTRIM(@Title)) = '' OR
       @Year IS NULL OR
       @Edition IS NULL OR LTRIM(RTRIM(@Edition)) = '' OR
       @NumberPages IS NULL
    BEGIN
        RAISERROR(N'Ошибка: обязательные поля не заполнены', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.Books b
               WHERE b.Title = @Title AND b.Year = @Year
                 AND b.Edition = @Edition AND b.NumberPages = @NumberPages)
    BEGIN
        RAISERROR(N'Ошибка: Такая книга уже существует', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.Books (Title, Year, Edition, NumberPages, Contents)
        VALUES (@Title, @Year, @Edition, @NumberPages, @Contents);

        SELECT SCOPE_IDENTITY() AS NewBookId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO


-- InsertAutors
CREATE OR ALTER PROCEDURE dbo.InsertAutors
    @FirstName VARCHAR(255),
    @Surname   VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    IF @FirstName IS NULL OR LTRIM(RTRIM(@FirstName)) = '' OR
       @Surname IS NULL OR LTRIM(RTRIM(@Surname)) = ''
    BEGIN
        RAISERROR(N'Ошибка: обязательные поля не заполнены', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.Autors WHERE FirstName=@FirstName AND Surname=@Surname)
    BEGIN
        RAISERROR(N'Ошибка: Такой автор уже существует', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.Autors(FirstName, Surname)
        VALUES (@FirstName, @Surname);

        SELECT SCOPE_IDENTITY() AS NewAutorId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO


-- SelectBooks (книга + авторы)
CREATE OR ALTER PROCEDURE dbo.SelectBooks
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        b.ID,
        b.Title,
        b.Year,
        b.Edition,
        b.NumberPages,
        b.Contents,

        a.ID AS AutorId,
        a.FirstName,
        a.Surname
    FROM dbo.Books b
    LEFT JOIN dbo.BookAutors ba ON ba.BookId = b.ID
    LEFT JOIN dbo.Autors a ON a.ID = ba.AutorId
END;
GO


-- SelectAllAutors
CREATE OR ALTER PROCEDURE dbo.SelectAllAutors
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ID, FirstName, Surname
    FROM dbo.Autors
    ORDER BY Surname, FirstName;
END;
GO


-- DeleteAutors
CREATE OR ALTER PROCEDURE dbo.DeleteAutors
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @ID IS NULL
    BEGIN
        RAISERROR(N'Ошибка: не выбран автор', 16, 1);
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Autors WHERE ID = @ID)
    BEGIN
        RAISERROR(N'Ошибка: такого автора не существует', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.BookAutors WHERE AutorId = @ID)
    BEGIN
        RAISERROR(N'Ошибка: нельзя удалить автора, т.к. у него есть книги', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE FROM dbo.Autors WHERE ID=@ID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO


-- DeleteBooks
CREATE OR ALTER PROCEDURE dbo.DeleteBooks
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @ID IS NULL
    BEGIN
        RAISERROR(N'Ошибка: не выбрана книга', 16, 1);
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Books WHERE ID = @ID)
    BEGIN
        RAISERROR(N'Ошибка: такой книги не существует', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE FROM dbo.Books WHERE ID=@ID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO


-- UpdateBooks
CREATE OR ALTER PROCEDURE dbo.UpdateBooks
    @ID INT,
    @Title VARCHAR(255),
    @Year INT,
    @Edition VARCHAR(255),
    @NumberPages INT,
    @Contents XML
AS
BEGIN
    SET NOCOUNT ON;

    IF @ID IS NULL
    BEGIN
        RAISERROR(N'Ошибка: не выбрана книга', 16, 1);
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Books WHERE ID = @ID)
    BEGIN
        RAISERROR(N'Ошибка: такой книги не существует', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.Books
        SET
            Title = COALESCE(@Title, Title),
            Year = COALESCE(@Year, Year),
            Edition = COALESCE(@Edition, Edition),
            NumberPages = COALESCE(@NumberPages, NumberPages),
            Contents = COALESCE(@Contents, Contents)
        WHERE ID=@ID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO


-- UpdateAutors
CREATE OR ALTER PROCEDURE dbo.UpdateAutors
    @ID INT,
    @FirstName VARCHAR(255),
    @Surname VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    IF @ID IS NULL
    BEGIN
        RAISERROR(N'Ошибка: не выбран автор', 16, 1);
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Autors WHERE ID = @ID)
    BEGIN
        RAISERROR(N'Ошибка: такого автора не существует', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.Autors
        SET
            FirstName = COALESCE(@FirstName, FirstName),
            Surname   = COALESCE(@Surname, Surname)
        WHERE ID=@ID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO


-- AddAutorToBook
CREATE OR ALTER PROCEDURE dbo.AddAutorToBook
    @BookId INT,
    @AutorId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Books WHERE ID=@BookId)
    BEGIN
        RAISERROR(N'Ошибка: книга не найдена', 16, 1);
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Autors WHERE ID=@AutorId)
    BEGIN
        RAISERROR(N'Ошибка: автор не найден', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.BookAutors WHERE BookId=@BookId AND AutorId=@AutorId)
        BEGIN
            INSERT INTO dbo.BookAutors(BookId, AutorId)
            VALUES (@BookId, @AutorId);
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO


-- RemoveAutorFromBook
CREATE OR ALTER PROCEDURE dbo.RemoveAutorFromBook
    @BookId INT,
    @AutorId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @BookId IS NULL OR @AutorId IS NULL
    BEGIN
        RAISERROR(N'Ошибка: не выбрана книга или автор', 16, 1);
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.BookAutors WHERE BookId = @BookId AND AutorId = @AutorId)
    BEGIN
        RAISERROR(N'Ошибка: такой связи книга–автор не существует', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE FROM dbo.BookAutors
        WHERE BookId = @BookId AND AutorId = @AutorId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO


-- SelectAutorsByBook
CREATE OR ALTER PROCEDURE dbo.SelectAutorsByBook
    @BookId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @BookId IS NULL
    BEGIN
        RAISERROR(N'Ошибка: не выбрана книга', 16, 1);
        RETURN;
    END

    SELECT AutorId
    FROM dbo.BookAutors
    WHERE BookId = @BookId;
END
GO



INSERT INTO dbo.Autors(FirstName, Surname)
VALUES (N'Евгений', N'Соколов');

INSERT INTO dbo.Books(Title, Year, Edition, NumberPages, Contents)
VALUES (N'Тестовая книга', 2026, N'Сова', 15, NULL);

INSERT INTO dbo.BookAutors(BookId, AutorId)
VALUES (1, 1);
GO
