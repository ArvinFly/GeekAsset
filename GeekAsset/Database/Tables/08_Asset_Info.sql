/* =============================================================
   08_Asset_Info  资产主表（黄金卡片）
   资产生命周期 2.2 + 数据库 3.2 防调包设计。
   - 财务/维保字段：SupplierID / PurchaseDate / PurchasePrice / WarrantyExpireDate
   - 防调包：Original_Spec(只读) vs Current_Spec(经维修/升级窗体变更，写硬件日志)
   ============================================================= */
USE [GeekAsset];
GO
IF OBJECT_ID(N'dbo.Asset_Info', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Asset_Info
    (
        AssetID            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Asset_Info PRIMARY KEY,
        AssetNo            NVARCHAR(50)   NOT NULL,             -- 资产编号（唯一ID）
        AssetName          NVARCHAR(100)  NOT NULL,             -- 资产名称
        CategoryID         INT            NULL,                 -- 资产分类
        Model              NVARCHAR(100)  NULL,                 -- 规格型号
        Status             TINYINT        NOT NULL DEFAULT(1),  -- 1闲置 2在用 3报修 4借出 5报废
        LocationID         INT            NULL,                 -- 存放位置
        CurrentEmpID       INT            NULL,                 -- 当前领用人

        -- 财务与维保字段
        SupplierID         INT            NULL,                 -- 供应商
        PurchaseDate       DATE           NULL,                 -- 采购日期
        PurchasePrice      DECIMAL(18,2)  NOT NULL DEFAULT(0),  -- 含税原值（折旧基数）
        WarrantyExpireDate DATE           NULL,                 -- 维保到期日（30天预警）

        -- IT 专属字段
        SN                 NVARCHAR(100)  NULL,                 -- 出厂序列号
        MacAddress         NVARCHAR(50)   NULL,                 -- MAC 地址
        IPAddress          NVARCHAR(50)   NULL,                 -- IP
        IPType             TINYINT        NULL,                 -- 1=固定IP 2=DHCP
        Original_Spec      NVARCHAR(500)  NULL,                 -- 入库原始配置（只读防调包）
        Current_Spec       NVARCHAR(500)  NULL,                 -- 当前配置

        -- 折旧相关
        ResidualValue      DECIMAL(18,2)  NULL,                 -- 残值（原值×残值率）
        IsDepreciated      BIT            NOT NULL DEFAULT(0),  -- 是否已折完（净值达残值封顶）

        -- 报废
        ScrapDate          DATE           NULL,
        ScrapReason        NVARCHAR(200)  NULL,                 -- 损毁/老旧/无法维修

        Remark             NVARCHAR(500)  NULL,
        IsDeleted          BIT            NOT NULL DEFAULT(0),  -- 作废标记
        CreateBy           INT            NULL,                 -- 录入人 UserID
        CreateTime         DATETIME       NOT NULL DEFAULT(GETDATE()),
        UpdateTime         DATETIME       NULL,

        CONSTRAINT UQ_Asset_Info_AssetNo UNIQUE (AssetNo),
        CONSTRAINT FK_Asset_Info_Category FOREIGN KEY (CategoryID) REFERENCES dbo.Base_AssetCategory(CategoryID),
        CONSTRAINT FK_Asset_Info_Location FOREIGN KEY (LocationID) REFERENCES dbo.Base_Location(LocationID),
        CONSTRAINT FK_Asset_Info_Emp      FOREIGN KEY (CurrentEmpID) REFERENCES dbo.Sys_Employee(EmpID),
        CONSTRAINT FK_Asset_Info_Supplier FOREIGN KEY (SupplierID) REFERENCES dbo.Base_Supplier(SupplierID)
    );
    -- 维保到期看板高频筛选
    CREATE INDEX IX_Asset_Info_Warranty ON dbo.Asset_Info(WarrantyExpireDate) WHERE WarrantyExpireDate IS NOT NULL;
    CREATE INDEX IX_Asset_Info_Status   ON dbo.Asset_Info(Status);
    PRINT '表 Asset_Info 已创建。';
END
ELSE PRINT '表 Asset_Info 已存在，跳过。';
GO
