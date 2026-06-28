namespace GeekAsset
{
    partial class MainFrm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码
        private void InitializeComponent()
        {
            this.navMenu = new Sunny.UI.UINavMenu();
            this.pnlContent = new Sunny.UI.UIPanel();
            this.pnlStatus = new Sunny.UI.UIPanel();
            this.lblClock = new Sunny.UI.UILabel();
            this.lblUser = new Sunny.UI.UILabel();
            this.pnlStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // navMenu
            // 
            this.navMenu.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.navMenu.Dock = System.Windows.Forms.DockStyle.Left;
            this.navMenu.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawAll;
            this.navMenu.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.navMenu.FullRowSelect = true;
            this.navMenu.HotTracking = true;
            this.navMenu.ItemHeight = 42;
            this.navMenu.Location = new System.Drawing.Point(0, 35);
            this.navMenu.Name = "navMenu";
            this.navMenu.ShowLines = false;
            this.navMenu.ShowPlusMinus = false;
            this.navMenu.ShowRootLines = false;
            this.navMenu.Size = new System.Drawing.Size(210, 630);
            this.navMenu.TabIndex = 0;
            this.navMenu.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.navMenu.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.navMenu_AfterSelect);
            // 
            // pnlContent
            // 
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.FillColor = System.Drawing.Color.White;
            this.pnlContent.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.pnlContent.Location = new System.Drawing.Point(210, 35);
            this.pnlContent.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlContent.MinimumSize = new System.Drawing.Size(1, 1);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Size = new System.Drawing.Size(790, 602);
            this.pnlContent.TabIndex = 1;
            this.pnlContent.Text = null;
            this.pnlContent.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlStatus
            // 
            this.pnlStatus.Controls.Add(this.lblClock);
            this.pnlStatus.Controls.Add(this.lblUser);
            this.pnlStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlStatus.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(243)))), ((int)(((byte)(243)))));
            this.pnlStatus.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.pnlStatus.Location = new System.Drawing.Point(210, 637);
            this.pnlStatus.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlStatus.MinimumSize = new System.Drawing.Size(1, 1);
            this.pnlStatus.Name = "pnlStatus";
            this.pnlStatus.Size = new System.Drawing.Size(790, 28);
            this.pnlStatus.TabIndex = 2;
            this.pnlStatus.Text = null;
            this.pnlStatus.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblClock
            // 
            this.lblClock.BackColor = System.Drawing.Color.Transparent;
            this.lblClock.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblClock.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.lblClock.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.lblClock.Location = new System.Drawing.Point(570, 0);
            this.lblClock.Name = "lblClock";
            this.lblClock.Padding = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.lblClock.Size = new System.Drawing.Size(220, 28);
            this.lblClock.TabIndex = 0;
            this.lblClock.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblUser
            // 
            this.lblUser.BackColor = System.Drawing.Color.Transparent;
            this.lblUser.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblUser.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.lblUser.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.lblUser.Location = new System.Drawing.Point(0, 0);
            this.lblUser.Name = "lblUser";
            this.lblUser.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.lblUser.Size = new System.Drawing.Size(420, 28);
            this.lblUser.TabIndex = 1;
            this.lblUser.Text = "当前用户：";
            this.lblUser.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // MainFrm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1000, 665);
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlStatus);
            this.Controls.Add(this.navMenu);
            this.Name = "MainFrm";
            this.Text = "企业IT固定资产管理系统";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ZoomScaleRect = new System.Drawing.Rectangle(15, 15, 1000, 665);
            this.pnlStatus.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private Sunny.UI.UINavMenu navMenu;
        private Sunny.UI.UIPanel pnlContent;
        private Sunny.UI.UIPanel pnlStatus;
        private Sunny.UI.UILabel lblUser;
        private Sunny.UI.UILabel lblClock;
    }
}
