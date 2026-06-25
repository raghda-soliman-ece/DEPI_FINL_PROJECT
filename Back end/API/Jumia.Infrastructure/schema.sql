-- Jumia Database Initialization Schema Script
-- Targets: Microsoft SQL Server

-- 1. AspNetUsers
IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetUsers (
        Id NVARCHAR(450) NOT NULL,
        UserName NVARCHAR(256) NULL,
        NormalizedUserName NVARCHAR(256) NULL,
        Email NVARCHAR(256) NULL,
        NormalizedEmail NVARCHAR(256) NULL,
        EmailConfirmed BIT NOT NULL,
        PasswordHash NVARCHAR(MAX) NULL,
        PhoneNumber NVARCHAR(MAX) NULL,
        PhoneNumberConfirmed BIT NOT NULL,
        TwoFactorEnabled BIT NOT NULL,
        LockoutEnd DATETIMEOFFSET(7) NULL,
        LockoutEnabled BIT NOT NULL,
        AccessFailedCount INT NOT NULL,
        DisplayName NVARCHAR(100) NOT NULL,
        Address NVARCHAR(500) NULL,
        CONSTRAINT PK_AspNetUsers PRIMARY KEY (Id)
    );
END

-- 2. AspNetRoles
IF OBJECT_ID(N'dbo.AspNetRoles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetRoles (
        Id NVARCHAR(450) NOT NULL,
        Name NVARCHAR(256) NULL,
        NormalizedName NVARCHAR(256) NULL,
        ConcurrencyStamp NVARCHAR(MAX) NULL,
        CONSTRAINT PK_AspNetRoles PRIMARY KEY (Id)
    );
END

-- 3. AspNetUserRoles
IF OBJECT_ID(N'dbo.AspNetUserRoles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetUserRoles (
        UserId NVARCHAR(450) NOT NULL,
        RoleId NVARCHAR(450) NOT NULL,
        CONSTRAINT PK_AspNetUserRoles PRIMARY KEY (UserId, RoleId),
        CONSTRAINT FK_AspNetUserRoles_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id) ON DELETE CASCADE,
        CONSTRAINT FK_AspNetUserRoles_AspNetRoles_RoleId FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles (Id) ON DELETE CASCADE
    );
END

-- 4. RefreshTokens
IF OBJECT_ID(N'dbo.RefreshTokens', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RefreshTokens (
        Id INT IDENTITY(1,1) NOT NULL,
        Token NVARCHAR(450) NOT NULL,
        Expires DATETIME2(7) NOT NULL,
        Created DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        Revoked DATETIME2(7) NULL,
        UserId NVARCHAR(450) NOT NULL,
        CONSTRAINT PK_RefreshTokens PRIMARY KEY (Id),
        CONSTRAINT FK_RefreshTokens_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX IX_RefreshTokens_Token ON dbo.RefreshTokens (Token);
END

-- 5. Categories
IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories (
        Id INT IDENTITY(1,1) NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        PictureUrl NVARCHAR(500) NULL,
        ParentCategoryId INT NULL,
        CONSTRAINT PK_Categories PRIMARY KEY (Id),
        CONSTRAINT FK_Categories_Categories_ParentCategoryId FOREIGN KEY (ParentCategoryId) REFERENCES dbo.Categories (Id) ON DELETE NO ACTION
    );
END

-- 6. Brands
IF OBJECT_ID(N'dbo.Brands', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Brands (
        Id INT IDENTITY(1,1) NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        PictureUrl NVARCHAR(500) NULL,
        CONSTRAINT PK_Brands PRIMARY KEY (Id)
    );
END

-- 7. Products
IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products (
        Id INT IDENTITY(1,1) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(2000) NULL,
        Price DECIMAL(18,2) NOT NULL,
        OldPrice DECIMAL(18,2) NULL,
        PictureUrl NVARCHAR(500) NULL,
        Stock INT NOT NULL DEFAULT 0,
        CategoryId INT NOT NULL,
        BrandId INT NOT NULL,
        CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        IsActive BIT NOT NULL DEFAULT 1,
        CONSTRAINT PK_Products PRIMARY KEY (Id),
        CONSTRAINT FK_Products_Categories_CategoryId FOREIGN KEY (CategoryId) REFERENCES dbo.Categories (Id) ON DELETE NO ACTION,
        CONSTRAINT FK_Products_Brands_BrandId FOREIGN KEY (BrandId) REFERENCES dbo.Brands (Id) ON DELETE NO ACTION
    );
    CREATE INDEX IX_Products_CategoryId ON dbo.Products (CategoryId);
    CREATE INDEX IX_Products_BrandId ON dbo.Products (BrandId);
END

-- 8. ProductImages
IF OBJECT_ID(N'dbo.ProductImages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductImages (
        Id INT IDENTITY(1,1) NOT NULL,
        ImageUrl NVARCHAR(500) NOT NULL,
        IsMain BIT NOT NULL DEFAULT 0,
        ProductId INT NOT NULL,
        CONSTRAINT PK_ProductImages PRIMARY KEY (Id),
        CONSTRAINT FK_ProductImages_Products_ProductId FOREIGN KEY (ProductId) REFERENCES dbo.Products (Id) ON DELETE CASCADE
    );
END

-- 9. Reviews
IF OBJECT_ID(N'dbo.Reviews', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Reviews (
        Id INT IDENTITY(1,1) NOT NULL,
        Comment NVARCHAR(1000) NULL,
        Rating INT NOT NULL,
        CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        UserId NVARCHAR(450) NOT NULL,
        ProductId INT NOT NULL,
        CONSTRAINT PK_Reviews PRIMARY KEY (Id),
        CONSTRAINT CK_Review_Rating CHECK (Rating >= 1 AND Rating <= 5),
        CONSTRAINT FK_Reviews_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id) ON DELETE NO ACTION,
        CONSTRAINT FK_Reviews_Products_ProductId FOREIGN KEY (ProductId) REFERENCES dbo.Products (Id) ON DELETE CASCADE
    );
END

-- 10. Baskets
IF OBJECT_ID(N'dbo.Baskets', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Baskets (
        Id INT IDENTITY(1,1) NOT NULL,
        UserId NVARCHAR(450) NOT NULL,
        CONSTRAINT PK_Baskets PRIMARY KEY (Id),
        CONSTRAINT FK_Baskets_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX IX_Baskets_UserId ON dbo.Baskets (UserId);
END

-- 11. BasketItems
IF OBJECT_ID(N'dbo.BasketItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BasketItems (
        Id INT IDENTITY(1,1) NOT NULL,
        Quantity INT NOT NULL,
        BasketId INT NOT NULL,
        ProductId INT NOT NULL,
        CONSTRAINT PK_BasketItems PRIMARY KEY (Id),
        CONSTRAINT FK_BasketItems_Baskets_BasketId FOREIGN KEY (BasketId) REFERENCES dbo.Baskets (Id) ON DELETE CASCADE,
        CONSTRAINT FK_BasketItems_Products_ProductId FOREIGN KEY (ProductId) REFERENCES dbo.Products (Id) ON DELETE NO ACTION
    );
END

-- 12. DeliveryMethods
IF OBJECT_ID(N'dbo.DeliveryMethods', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DeliveryMethods (
        Id INT IDENTITY(1,1) NOT NULL,
        ShortName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        DeliveryTime NVARCHAR(100) NOT NULL,
        Price DECIMAL(18,2) NOT NULL,
        CONSTRAINT PK_DeliveryMethods PRIMARY KEY (Id)
    );
END

-- 13. Orders
IF OBJECT_ID(N'dbo.Orders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders (
        Id INT IDENTITY(1,1) NOT NULL,
        OrderDate DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        SubTotal DECIMAL(18,2) NOT NULL,
        DeliveryPrice DECIMAL(18,2) NOT NULL,
        UserId NVARCHAR(450) NOT NULL,
        DeliveryMethodId INT NOT NULL,
        CONSTRAINT PK_Orders PRIMARY KEY (Id),
        CONSTRAINT FK_Orders_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id) ON DELETE NO ACTION,
        CONSTRAINT FK_Orders_DeliveryMethods_DeliveryMethodId FOREIGN KEY (DeliveryMethodId) REFERENCES dbo.DeliveryMethods (Id) ON DELETE NO ACTION
    );
END

-- 14. ShippingAddresses
IF OBJECT_ID(N'dbo.ShippingAddresses', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ShippingAddresses (
        Id INT IDENTITY(1,1) NOT NULL,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        Street NVARCHAR(200) NOT NULL,
        City NVARCHAR(100) NOT NULL,
        State NVARCHAR(100) NOT NULL,
        ZipCode NVARCHAR(20) NOT NULL,
        OrderId INT NOT NULL,
        CONSTRAINT PK_ShippingAddresses PRIMARY KEY (Id),
        CONSTRAINT FK_ShippingAddresses_Orders_OrderId FOREIGN KEY (OrderId) REFERENCES dbo.Orders (Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX IX_ShippingAddresses_OrderId ON dbo.ShippingAddresses (OrderId);
END

-- 15. OrderItems
IF OBJECT_ID(N'dbo.OrderItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems (
        Id INT IDENTITY(1,1) NOT NULL,
        ProductName NVARCHAR(200) NOT NULL,
        PictureUrl NVARCHAR(500) NULL,
        Price DECIMAL(18,2) NOT NULL,
        Quantity INT NOT NULL,
        ProductId INT NULL,
        OrderId INT NOT NULL,
        CONSTRAINT PK_OrderItems PRIMARY KEY (Id),
        CONSTRAINT FK_OrderItems_Products_ProductId FOREIGN KEY (ProductId) REFERENCES dbo.Products (Id) ON DELETE SET NULL,
        CONSTRAINT FK_OrderItems_Orders_OrderId FOREIGN KEY (OrderId) REFERENCES dbo.Orders (Id) ON DELETE CASCADE
    );
END

-- 16. WishlistItems
IF OBJECT_ID(N'dbo.WishlistItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WishlistItems (
        Id INT IDENTITY(1,1) NOT NULL,
        UserId NVARCHAR(450) NOT NULL,
        ProductId INT NOT NULL,
        AddedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT PK_WishlistItems PRIMARY KEY (Id),
        CONSTRAINT UQ_WishlistItems_UserId_ProductId UNIQUE (UserId, ProductId),
        CONSTRAINT FK_WishlistItems_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id) ON DELETE CASCADE,
        CONSTRAINT FK_WishlistItems_Products_ProductId FOREIGN KEY (ProductId) REFERENCES dbo.Products (Id) ON DELETE CASCADE
    );
END

-----------------------------------------------------------------------
-- SEED DATA SECTION
-----------------------------------------------------------------------

-- A. Seed Roles (1 Admin, 1 Customer)
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name = 'Admin')
BEGIN
    INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES ('a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d', 'Admin', 'ADMIN', 'b1c2d3e4-f5a6-7b8c-9d0e-1f2a3b4c5d6e');
END

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name = 'Customer')
BEGIN
    INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES ('f5e4d3c2-b1a0-9f8e-7d6c-5b4a3f2e1d0c', 'Customer', 'CUSTOMER', 'd3e4f5a6-b7c8-9d0e-1f2a-3b4c5d6e7f8a');
END

-- B. Seed 3 Delivery Methods (Standard, Express, Same-day)
IF NOT EXISTS (SELECT 1 FROM dbo.DeliveryMethods WHERE ShortName = 'Standard')
BEGIN
    INSERT INTO dbo.DeliveryMethods (ShortName, Description, DeliveryTime, Price)
    VALUES ('Standard', 'Deliver in 3-5 business days to your doorstep', '3-5 Days', 10.00);
END

IF NOT EXISTS (SELECT 1 FROM dbo.DeliveryMethods WHERE ShortName = 'Express')
BEGIN
    INSERT INTO dbo.DeliveryMethods (ShortName, Description, DeliveryTime, Price)
    VALUES ('Express', 'Deliver in 1-2 business days with priority shipping', '1-2 Days', 25.00);
END

IF NOT EXISTS (SELECT 1 FROM dbo.DeliveryMethods WHERE ShortName = 'Same-day')
BEGIN
    INSERT INTO dbo.DeliveryMethods (ShortName, Description, DeliveryTime, Price)
    VALUES ('Same-day', 'Super fast delivery within the same calendar day', 'Same Day', 50.00);
END

-- C. Seed Categories (Electronics > Phones, Computers; Fashion > Men, Women; Home & Kitchen)
-- 1. Parent Category: Electronics
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Name = 'Electronics')
BEGIN
    INSERT INTO dbo.Categories (Name, Description, PictureUrl, ParentCategoryId)
    VALUES ('Electronics', 'Smart gadgets, appliances, and computer equipment', NULL, NULL);
END

-- 2. Parent Category: Fashion
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Name = 'Fashion')
BEGIN
    INSERT INTO dbo.Categories (Name, Description, PictureUrl, ParentCategoryId)
    VALUES ('Fashion', 'Trendiest apparel, shoes, and clothing accessories', NULL, NULL);
END

-- 3. Parent Category: Home & Kitchen
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Name = 'Home & Kitchen')
BEGIN
    INSERT INTO dbo.Categories (Name, Description, PictureUrl, ParentCategoryId)
    VALUES ('Home & Kitchen', 'Furniture, cookware, kitchen tools, and decors', NULL, NULL);
END

-- 4. Sub-categories under Electronics
DECLARE @ElectronicsId INT;
SELECT @ElectronicsId = Id FROM dbo.Categories WHERE Name = 'Electronics';

IF @ElectronicsId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Name = 'Phones' AND ParentCategoryId = @ElectronicsId)
    BEGIN
        INSERT INTO dbo.Categories (Name, Description, PictureUrl, ParentCategoryId)
        VALUES ('Phones', 'Smartphones, mobile accessories, and tablets', NULL, @ElectronicsId);
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Name = 'Computers' AND ParentCategoryId = @ElectronicsId)
    BEGIN
        INSERT INTO dbo.Categories (Name, Description, PictureUrl, ParentCategoryId)
        VALUES ('Computers', 'Desktop PCs, laptops, computer peripherals, and software', NULL, @ElectronicsId);
    END
END

-- 5. Sub-categories under Fashion
DECLARE @FashionId INT;
SELECT @FashionId = Id FROM dbo.Categories WHERE Name = 'Fashion';

IF @FashionId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Name = 'Men' AND ParentCategoryId = @FashionId)
    BEGIN
        INSERT INTO dbo.Categories (Name, Description, PictureUrl, ParentCategoryId)
        VALUES ('Men', 'Mens clothing collection and accessories', NULL, @FashionId);
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Name = 'Women' AND ParentCategoryId = @FashionId)
    BEGIN
        INSERT INTO dbo.Categories (Name, Description, PictureUrl, ParentCategoryId)
        VALUES ('Women', 'Womens clothing collection and accessories', NULL, @FashionId);
    END
END
