/* =============================================================
   迁移：Asset_Flow_Log 的 ActionType 增加允许值 N'报废'
   背景：报废纳入核心流转业务，改由 FlowService.Scrap 写流转日志，
         需放开 CK_Asset_Flow_Log_Action 约束。幂等，可重复执行。
   ============================================================= */
USE [GeekAsset];
GO
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Asset_Flow_Log_Action')
    ALTER TABLE dbo.Asset_Flow_Log DROP CONSTRAINT CK_Asset_Flow_Log_Action;
GO
ALTER TABLE dbo.Asset_Flow_Log ADD CONSTRAINT CK_Asset_Flow_Log_Action
    CHECK (ActionType IN (N'领用', N'退还', N'借用', N'归还', N'调拨', N'报废'));
GO
PRINT '约束 CK_Asset_Flow_Log_Action 已更新（含 N''报废''）。';
GO
