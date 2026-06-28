namespace GeekAsset.Security
{
    /// <summary>登录后全局保存当前用户与角色（RBAC 三角色）。</summary>
    public static class CurrentUser
    {
        public static int UserID { get; set; }
        public static string LoginName { get; set; }
        public static string RealName { get; set; }
        public static string Role { get; set; }   // Admin / Operator / Finance

        public static bool IsAdmin => Role == "Admin";
        public static bool IsOperator => Role == "Operator";
        public static bool IsFinance => Role == "Finance";

        public static void Set(int userId, string loginName, string realName, string role)
        {
            UserID = userId;
            LoginName = loginName;
            RealName = realName;
            Role = role;
        }
    }
}
