using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GeekAsset.Security;
using GeekAsset.Views;
using Sunny.UI;

namespace GeekAsset
{
    public partial class MainFrm : UIForm
    {
        // 已加载页面缓存：key -> 控件
        private readonly Dictionary<string, UserControl> _pageCache = new Dictionary<string, UserControl>();
        private readonly Timer _clock = new Timer { Interval = 1000 };

        private TreeNode _firstLeaf;

        public MainFrm()
        {
            InitializeComponent();
            BuildMenu();
            ShowCurrentUser();

            _clock.Tick += (s, e) => lblClock.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss  dddd");
            _clock.Start();
            lblClock.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss  dddd");
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // 句柄已建立后再展开并选中首项，避免 OwnerDraw 在句柄未就绪时绘制
            navMenu.ExpandAll();
            if (_firstLeaf != null) navMenu.SelectedNode = _firstLeaf;
        }

        /// <summary>菜单项定义：(分组, 标题, 键, 允许角色)。分组为空表示一级叶子。</summary>
        private static readonly (string Group, string Title, string Key, string[] Roles)[] MenuItems =
        {
            (null,     "工作台",      "dashboard",   new[]{"Admin","Operator","Finance"}),

            ("基础设置","组织架构",    "dept",        new[]{"Admin","Operator"}),
            ("基础设置","人员档案",    "employee",    new[]{"Admin","Operator"}),
            ("基础设置","资产分类",    "category",    new[]{"Admin","Operator"}),
            ("基础设置","存放位置",    "location",    new[]{"Admin","Operator"}),
            ("基础设置","往来单位",    "supplier",    new[]{"Admin","Operator"}),
            ("基础设置","折旧规则",    "deprecRule",  new[]{"Admin","Finance"}),

            ("资产管理","资产台账",    "assetList",   new[]{"Admin","Operator","Finance"}),
            ("资产管理","资产入库",    "assetIn",     new[]{"Admin","Operator"}),
            ("资产管理","领用 / 退还", "assetUse",    new[]{"Admin","Operator"}),
            ("资产管理","借用 / 归还", "assetBorrow", new[]{"Admin","Operator"}),
            ("资产管理","资产调拨",    "assetMove",   new[]{"Admin","Operator"}),
            ("资产管理","报废处置",    "assetScrap",  new[]{"Admin","Operator"}),

            (null,     "扫码盘点",    "audit",       new[]{"Admin","Operator"}),
            (null,     "维修维保",    "maintenance", new[]{"Admin","Operator"}),
            (null,     "财务折旧",    "depreciation",new[]{"Admin","Finance"}),
            (null,     "低值易耗品",  "consumables", new[]{"Admin","Operator"}),
            (null,     "报表看板",    "reports",     new[]{"Admin","Operator","Finance"}),

            ("系统设置","用户管理",    "users",       new[]{"Admin"}),
            ("系统设置","修改密码",    "changePwd",   new[]{"Admin","Operator","Finance"}),
        };

        private void BuildMenu()
        {
            navMenu.Nodes.Clear();
            string role = CurrentUser.Role;
            var groupNodes = new Dictionary<string, TreeNode>();
            _firstLeaf = null;
            int pageIndex = 0;   // 不绑定 MultiPage，仅占位

            foreach (var item in MenuItems)
            {
                if (!item.Roles.Contains(role)) continue;

                TreeNode leaf;
                if (string.IsNullOrEmpty(item.Group))
                {
                    // 用 SunnyUI 正规 API 创建节点（会登记内部结构供 OwnerDraw 使用）
                    leaf = navMenu.CreateNode(item.Title, pageIndex++);
                }
                else
                {
                    if (!groupNodes.TryGetValue(item.Group, out var groupNode))
                    {
                        groupNode = navMenu.CreateNode(item.Group, pageIndex++);
                        groupNodes[item.Group] = groupNode;
                    }
                    leaf = navMenu.CreateChildNode(groupNode, item.Title, pageIndex++);
                }

                leaf.Tag = item.Key;
                if (_firstLeaf == null) _firstLeaf = leaf;
            }
        }

        private void ShowCurrentUser()
        {
            string roleName;
            switch (CurrentUser.Role)
            {
                case "Admin": roleName = "系统管理员"; break;
                case "Operator": roleName = "IT运维/库管"; break;
                case "Finance": roleName = "财务人员"; break;
                default: roleName = CurrentUser.Role; break;
            }
            string name = string.IsNullOrEmpty(CurrentUser.RealName) ? CurrentUser.LoginName : CurrentUser.RealName;
            lblUser.Text = $"当前用户：{name}（{roleName}）";
        }

        private void navMenu_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // 仅叶子节点（无子节点）才加载页面；点分组只做展开/收起
            if (e.Node == null || e.Node.Nodes.Count > 0) return;
            if (e.Node.Tag is string key)
                LoadPage(key, e.Node.Text);
        }

        private void LoadPage(string key, string title)
        {
            if (!_pageCache.TryGetValue(key, out var page))
            {
                page = CreatePage(key, title);
                _pageCache[key] = page;
            }

            pnlContent.SuspendLayout();
            pnlContent.Controls.Clear();
            page.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(page);
            pnlContent.ResumeLayout();
        }

        /// <summary>按菜单 key 创建真实页面；未实现的模块回退占位页。</summary>
        private UserControl CreatePage(string key, string title)
        {
            switch (key)
            {
                case "dashboard": return new DashboardPage();
                case "assetList":
                case "assetIn": return new AssetListPage();
                case "assetUse": return new AssetUsePage();
                case "assetBorrow": return new AssetBorrowPage();
                case "assetMove": return new AssetMovePage();
                case "assetScrap": return new AssetScrapPage();
                case "audit": return new AuditTaskPage();
                case "maintenance": return new MaintenancePage();
                case "depreciation": return new DepreciationPage();
                case "users": return new UserManagePage();
                case "changePwd": return new ChangePasswordPage();
                case "consumables": return new ConsumablesPage();
                case "reports": return new ReportsPage();
                case "dept": return new DeptPage();
                case "employee": return new EmployeePage();
                case "category": return new CategoryPage();
                case "location": return new LocationPage();
                case "supplier": return new SupplierPage();
                case "deprecRule": return new DeprecRulePage();
                default: return new PlaceholderControl(title);
            }
        }
    }
}
