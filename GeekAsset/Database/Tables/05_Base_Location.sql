/* =============================================================
   05_Base_Location  存放位置库
   基础设置模块 2.1。如：机房A、3楼办公区、IT备件库房。
   ============================================================= */
USE [GeekAsset];
GO
IF OBJECT_ID(N'dbo.Base_Location', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Base_Location
    (
        LocationID    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Base_Location PRIMARY KEY,
        LocationName  NVARCHAR(100) NOT NULL,                   -- 位置名称
        Remark        NVARCHAR(200) NULL,
        IsDeleted     BIT           NOT NULL DEFAULT(0),
        CreateTime    DATETIME      NOT NULL DEFAULT(GETDATE())
    );
    PRINT '表 Base_Location 已创建。';
END
ELSE PRINT '表 Base_Location 已存在，跳过。';
GO
