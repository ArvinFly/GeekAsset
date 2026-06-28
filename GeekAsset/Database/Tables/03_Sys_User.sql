/* =============================================================
   03_Sys_User  系统登录用户（RBAC 三角色）
   边界设计 三点五.A：Role = Admin / Operator / Finance。
   密码以哈希存储（哈希算法由登录模块统一约定，禁止明文）。
   ============================================================= */
USE [GeekAsset];
GO
IF OBJECT_ID(N'dbo.Sys_User', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Sys_User
    (
        UserID        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Sys_User PRIMARY KEY,
        LoginName     NVARCHAR(50)  NOT NULL,                   -- 登录名（唯一）
        PasswordHash  NVARCHAR(200) NOT NULL,                   -- 密码哈希
        RealName      NVARCHAR(50)  NULL,                       -- 姓名
        Role          NVARCHAR(20)  NOT NULL,                   -- Admin / Operator / Finance
        EmpID         INT           NULL,                       -- 关联人员档案（可选）
        IsEnabled     BIT           NOT NULL DEFAULT(1),        -- 是否启用
        LastLoginTime DATETIME      NULL,
        CreateTime    DATETIME      NOT NULL DEFAULT(GETDATE()),
        CONSTRAINT UQ_Sys_User_LoginName UNIQUE (LoginName),
        CONSTRAINT CK_Sys_User_Role CHECK (Role IN (N'Admin', N'Operator', N'Finance')),
        CONSTRAINT FK_Sys_User_Emp FOREIGN KEY (EmpID) REFERENCES dbo.Sys_Employee(EmpID)
    );
    PRINT '表 Sys_User 已创建。';
END
ELSE PRINT '表 Sys_User 已存在，跳过。';
GO
