/* =============================================================
   00_CreateDatabase.sql
   企业IT固定资产管理系统 - 创建数据库
   目标实例：localhost (SQL Server 2019, 默认实例)
   说明：幂等，可重复执行。
   ============================================================= */
IF DB_ID(N'GeekAsset') IS NULL
BEGIN
    CREATE DATABASE [GeekAsset];
    PRINT '数据库 GeekAsset 已创建。';
END
ELSE
BEGIN
    PRINT '数据库 GeekAsset 已存在，跳过创建。';
END
GO
