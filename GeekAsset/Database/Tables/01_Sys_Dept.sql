/* =============================================================
   01_Sys_Dept  组织架构表（公司-部门-小组 多级树）
   基础设置模块 2.1。自引用 ParentID 形成树状结构。
   ============================================================= */
USE [GeekAsset];
GO
IF OBJECT_ID(N'dbo.Sys_Dept', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Sys_Dept
    (
        DeptID      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Sys_Dept PRIMARY KEY,
        ParentID    INT             NULL,                       -- 上级部门ID，公司根节点为 NULL
        DeptName    NVARCHAR(100)   NOT NULL,                   -- 部门/小组名称
        DeptLevel   TINYINT         NOT NULL DEFAULT(2),        -- 1=公司 2=部门 3=小组
        SortOrder   INT             NOT NULL DEFAULT(0),        -- 同级排序
        IsDeleted   BIT             NOT NULL DEFAULT(0),        -- 作废标记（禁止物理删除）
        CreateTime  DATETIME        NOT NULL DEFAULT(GETDATE()),
        CONSTRAINT FK_Sys_Dept_Parent FOREIGN KEY (ParentID) REFERENCES dbo.Sys_Dept(DeptID)
    );
    PRINT '表 Sys_Dept 已创建。';
END
ELSE PRINT '表 Sys_Dept 已存在，跳过。';
GO
