/* =============================================================
   99_SeedData.sql  初始化基础数据（幂等）
   - 默认系统管理员账号
   - 顶层组织/分类占位（可在系统内修改）
   说明：密码哈希算法最终以登录模块为准；此处先用 SQL Server
         HASHBYTES('SHA2_256', ...) 生成，登录模块需采用同一算法校验。
   默认账号：admin / Admin@123  （首次登录后请立即修改）
   ============================================================= */
USE [GeekAsset];
GO

/* 默认管理员 */
IF NOT EXISTS (SELECT 1 FROM dbo.Sys_User WHERE LoginName = N'admin')
BEGIN
    INSERT INTO dbo.Sys_User (LoginName, PasswordHash, RealName, Role, IsEnabled)
    VALUES (N'admin',
            CONVERT(NVARCHAR(200), HASHBYTES('SHA2_256', N'Admin@123'), 2),  -- 十六进制字符串
            N'系统管理员', N'Admin', 1);
    PRINT '默认管理员 admin 已创建（密码 Admin@123，请尽快修改）。';
END
ELSE PRINT '默认管理员 admin 已存在，跳过。';
GO

/* 顶层组织占位 */
IF NOT EXISTS (SELECT 1 FROM dbo.Sys_Dept WHERE ParentID IS NULL)
BEGIN
    INSERT INTO dbo.Sys_Dept (ParentID, DeptName, DeptLevel, SortOrder)
    VALUES (NULL, N'总公司', 1, 0);
    PRINT '顶层组织“总公司”已创建。';
END
GO

/* 资产分类占位（硬件/软件两大根） */
IF NOT EXISTS (SELECT 1 FROM dbo.Base_AssetCategory WHERE ParentID IS NULL)
BEGIN
    INSERT INTO dbo.Base_AssetCategory (ParentID, CategoryName, AssetType, SortOrder)
    VALUES (NULL, N'硬件资产', 1, 0),
           (NULL, N'虚拟/软件资产', 2, 1);
    PRINT '资产分类根节点已创建。';
END
GO
