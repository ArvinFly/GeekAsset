namespace GeekAsset.Models
{
    /// <summary>系统登录用户（对应表 Sys_User）。</summary>
    public class SysUser
    {
        public int UserID { get; set; }
        public string LoginName { get; set; }
        public string PasswordHash { get; set; }
        public string RealName { get; set; }
        public string Role { get; set; }
        public bool IsEnabled { get; set; }
    }

    /// <summary>用户管理列表项（联表取关联人员）。</summary>
    public class UserListItem
    {
        public int UserID { get; set; }
        public string LoginName { get; set; }
        public string RealName { get; set; }
        public string Role { get; set; }
        public bool IsEnabled { get; set; }
        public string EmpName { get; set; }
        public System.DateTime? LastLoginTime { get; set; }

        public string StatusText => IsEnabled ? "启用" : "停用";
    }
}
