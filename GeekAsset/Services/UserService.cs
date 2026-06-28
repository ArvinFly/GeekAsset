using System.Collections.Generic;
using System.Linq;
using Dapper;
using GeekAsset.Data;
using GeekAsset.Models;

namespace GeekAsset.Services
{
    /// <summary>系统用户管理（RBAC 三角色）。停用代替删除，避免日志外键悬挂。</summary>
    public static class UserService
    {
        /// <summary>新用户默认密码。</summary>
        public const string DefaultPassword = "Geek@123";

        public static List<UserListItem> GetAll()
        {
            using (var c = Db.Open())
                return c.Query<UserListItem>(
                    @"SELECT u.UserID, u.LoginName, u.RealName, u.Role, u.IsEnabled,
                             e.EmpName, u.LastLoginTime
                      FROM Sys_User u
                      LEFT JOIN Sys_Employee e ON u.EmpID=e.EmpID
                      ORDER BY u.UserID").ToList();
        }

        /// <summary>新增用户（默认密码 DefaultPassword，需用户首次登录后自行修改）。返回 UserID。</summary>
        public static int Insert(string loginName, string realName, string role, int? empId)
        {
            using (var c = Db.Open())
                return c.ExecuteScalar<int>(
                    @"INSERT INTO Sys_User(LoginName,PasswordHash,RealName,Role,EmpID,IsEnabled,CreateTime)
                      VALUES(@ln,@h,@rn,@role,@emp,1,GETDATE());
                      SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    new { ln = loginName, h = AuthService.HashPassword(DefaultPassword), rn = realName, role, emp = empId });
        }

        public static void Update(int userId, string realName, string role, int? empId)
        {
            using (var c = Db.Open())
                c.Execute("UPDATE Sys_User SET RealName=@rn, Role=@role, EmpID=@emp WHERE UserID=@id",
                    new { rn = realName, role, emp = empId, id = userId });
        }

        public static void SetEnabled(int userId, bool enabled)
        {
            using (var c = Db.Open())
                c.Execute("UPDATE Sys_User SET IsEnabled=@en WHERE UserID=@id", new { en = enabled, id = userId });
        }

        public static bool LoginNameExists(string loginName)
        {
            using (var c = Db.Open())
                return c.ExecuteScalar<int>("SELECT COUNT(*) FROM Sys_User WHERE LoginName=@n", new { n = loginName }) > 0;
        }
    }
}
