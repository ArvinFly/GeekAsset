using System;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using GeekAsset.Data;
using GeekAsset.Models;

namespace GeekAsset.Services
{
    /// <summary>登录鉴权。</summary>
    public static class AuthService
    {
        /// <summary>
        /// 计算密码哈希。必须与数据库 Seed 完全一致：
        /// SQL 端 = CONVERT(NVARCHAR, HASHBYTES('SHA2_256', N'明文'), 2)
        /// 即：对 nvarchar(UTF-16LE) 字节做 SHA-256，输出大写十六进制（无 0x 前缀）。
        /// </summary>
        public static string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.Unicode.GetBytes(password ?? string.Empty));
                var sb = new StringBuilder(bytes.Length * 2);
                foreach (byte b in bytes) sb.Append(b.ToString("X2"));
                return sb.ToString();
            }
        }

        /// <summary>登录校验。成功返回用户；用户名/密码错误返回 null；账号停用抛异常。</summary>
        public static SysUser Login(string loginName, string password)
        {
            using (var conn = Db.Open())
            {
                var user = conn.QueryFirstOrDefault<SysUser>(
                    @"SELECT UserID, LoginName, PasswordHash, RealName, Role, IsEnabled
                      FROM Sys_User WHERE LoginName = @n",
                    new { n = loginName });

                if (user == null) return null;
                if (!user.IsEnabled) throw new Exception("该账号已被停用，请联系管理员。");
                if (!string.Equals(user.PasswordHash, HashPassword(password), StringComparison.OrdinalIgnoreCase))
                    return null;

                conn.Execute("UPDATE Sys_User SET LastLoginTime = GETDATE() WHERE UserID = @id",
                    new { id = user.UserID });
                return user;
            }
        }

        /// <summary>修改密码：校验旧密码后更新。旧密码不符返回 false。</summary>
        public static bool ChangePassword(int userId, string oldPwd, string newPwd)
        {
            using (var conn = Db.Open())
            {
                var hash = conn.ExecuteScalar<string>("SELECT PasswordHash FROM Sys_User WHERE UserID=@id", new { id = userId });
                if (hash == null) return false;
                if (!string.Equals(hash, HashPassword(oldPwd), StringComparison.OrdinalIgnoreCase)) return false;
                conn.Execute("UPDATE Sys_User SET PasswordHash=@h WHERE UserID=@id",
                    new { h = HashPassword(newPwd), id = userId });
                return true;
            }
        }

        /// <summary>管理员重置密码（无需旧密码）。</summary>
        public static void SetPassword(int userId, string newPwd)
        {
            using (var conn = Db.Open())
                conn.Execute("UPDATE Sys_User SET PasswordHash=@h WHERE UserID=@id",
                    new { h = HashPassword(newPwd), id = userId });
        }
    }
}
