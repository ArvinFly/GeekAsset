/* =============================================================
   04_Base_AssetCategory  资产分类树
   基础设置模块 2.1。自引用树；AssetType 区分硬件/软件虚拟资产。
   ============================================================= */
USE [GeekAsset];
GO
IF OBJECT_ID(N'dbo.Base_AssetCategory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Base_AssetCategory
    (
        CategoryID    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Base_AssetCategory PRIMARY KEY,
        ParentID      INT           NULL,                       -- 上级分类，根节点 NULL
        CategoryName  NVARCHAR(100) NOT NULL,                   -- 分类名称
        AssetType     TINYINT       NOT NULL DEFAULT(1),        -- 1=硬件资产 2=虚拟/软件资产
        SortOrder     INT           NOT NULL DEFAULT(0),
        IsDeleted     BIT           NOT NULL DEFAULT(0),
        CreateTime    DATETIME      NOT NULL DEFAULT(GETDATE()),
        CONSTRAINT FK_Base_AssetCategory_Parent FOREIGN KEY (ParentID) REFERENCES dbo.Base_AssetCategory(CategoryID)
    );
    PRINT '表 Base_AssetCategory 已创建。';
END
ELSE PRINT '表 Base_AssetCategory 已存在，跳过。';
GO
