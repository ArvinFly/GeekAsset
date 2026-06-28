using System;
using System.Drawing;
using System.Windows.Forms;
using Sunny.UI;

namespace GeekAsset.Views
{
    /// <summary>
    /// 列表页基类：顶部工具栏(新增/编辑/删除/刷新) + 数据表格。各基础数据模块继承实现抽象方法。
    /// </summary>
    public abstract class BaseListPage : UserControl
    {
        protected UIDataGridView Grid;
        protected UIPanel Toolbar;
        private int _nextToolX = 376;   // 标准 4 按钮之后的起始位置

        protected BaseListPage()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.White;

            Toolbar = new UIPanel { Dock = DockStyle.Top, Height = 50, FillColor = Color.White, Text = "" };
            var btnAdd = MakeButton("新增", 12);
            var btnEdit = MakeButton("编辑", 102);
            var btnDel = MakeButton("删除", 192);
            var btnRefresh = MakeButton("刷新", 282);
            btnAdd.Click += (s, e) => SafeRun(DoAdd);
            btnEdit.Click += (s, e) => SafeRun(DoEdit);
            btnDel.Click += (s, e) => SafeRun(DoDelete);
            btnRefresh.Click += (s, e) => SafeRun(ReloadData);
            Toolbar.Controls.Add(btnAdd);
            Toolbar.Controls.Add(btnEdit);
            Toolbar.Controls.Add(btnDel);
            Toolbar.Controls.Add(btnRefresh);

            Grid = new UIDataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            Grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) SafeRun(DoEdit); };

            Controls.Add(Grid);
            Controls.Add(Toolbar);

            Load += (s, e) => { BuildColumns(); SafeRun(ReloadData); };
        }

        private UIButton MakeButton(string text, int x)
        {
            return new UIButton
            {
                Text = text,
                Location = new Point(x, 8),
                Size = new Size(84, 34),
                Font = new Font("微软雅黑", 10F)
            };
        }

        /// <summary>在标准按钮之后追加自定义工具栏按钮（子类在构造时调用）。</summary>
        protected UIButton AddToolButton(string text, Action onClick, int width = 96)
        {
            var btn = new UIButton
            {
                Text = text,
                Location = new Point(_nextToolX, 8),
                Size = new Size(width, 34),
                Font = new Font("微软雅黑", 10F)
            };
            btn.Click += (s, e) =>
            {
                try { onClick(); }
                catch (Exception ex) { UIMessageBox.ShowError("操作失败：" + ex.Message); }
            };
            Toolbar.Controls.Add(btn);
            _nextToolX += width + 6;
            return btn;
        }

        protected void AddColumn(string dataProperty, string header, int fillWeight = 100)
        {
            Grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = dataProperty,
                HeaderText = header,
                FillWeight = fillWeight,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
        }

        /// <summary>获取当前选中行绑定的实体；无选中返回 default。</summary>
        protected T Selected<T>() where T : class
        {
            if (Grid.CurrentRow == null) return null;
            return Grid.CurrentRow.DataBoundItem as T;
        }

        protected void Bind(object dataSource)
        {
            Grid.DataSource = null;
            Grid.DataSource = dataSource;
        }

        private void SafeRun(Action action)
        {
            try { action(); }
            catch (Exception ex) { UIMessageBox.ShowError("操作失败：" + ex.Message); }
        }

        protected abstract void BuildColumns();
        protected abstract void ReloadData();
        protected abstract void DoAdd();
        protected abstract void DoEdit();
        protected abstract void DoDelete();
    }
}
