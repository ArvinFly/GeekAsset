/* =============================================================
   02_Sys_Employee  人员档案中心
   基础设置模块 2.1。Status 离职联动“未交接资产”预警。
   ============================================================= */
USE [GeekAsset];
GO
IF OBJECT_ID(N'dbo.Sys_Employee', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Sys_Employee
    (
        EmpID       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Sys_Employee PRIMARY KEY,
        EmpNo       NVARCHAR(50)    NOT NULL,                   -- 工号（唯一）
        EmpName     NVARCHAR(50)    NOT NULL,                   -- 姓名
        DeptID      INT             NULL,                       -- 所属部门
        Mobile      NVARCHAR(20)    NULL,                       -- 手机
        Status      TINYINT         NOT NULL DEFAULT(1),        -- 1=在职 0=离职
        LeaveDate   DATE            NULL,                       -- 离职日期
        IsDeleted   BIT             NOT NULL DEFAULT(0),
        CreateTime  DATETIME        NOT NULL DEFAULT(GETDATE()),
        CONSTRAINT UQ_Sys_Employee_EmpNo UNIQUE (EmpNo),
        CONSTRAINT FK_Sys_Employee_Dept FOREIGN KEY (DeptID) REFERENCES dbo.Sys_Dept(DeptID)
    );
    PRINT '表 Sys_Employee 已创建。';
END
ELSE PRINT '表 Sys_Employee 已存在，跳过。';
GO
