using System;
using System.Drawing;
using System.Windows.Forms;
using Sunny.UI;

namespace GeekAsset.Views
{
    /// <summary>轻量网格页：表格 + 全自定义工具栏（无默认增删改），供流转类页面使用。</summary>
    public abstract class SimpleGridPage : UserControl
    {
        protected UIDataGridView Grid;
        protected UIPanel Toolbar;
        private int _nextX = 12;

        protected SimpleGridPage()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.White;

            Toolbar = new UIPanel { Dock = DockStyle.Top, Height = 50, FillColor = Color.White, Text = "" };
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
            Controls.Add(Grid);
            Controls.Add(Toolbar);
            Load += (s, e) => { BuildColumns(); SafeRun(ReloadData); };
        }

        protected UIButton AddToolButton(string text, Action onClick, int width = 96)
        {
            var btn = new UIButton { Text = text, Location = new Point(_nextX, 8), Size = new Size(width, 34), Font = new Font("微软雅黑", 10F) };
            btn.Click += (s, e) => SafeRun(onClick);
            Toolbar.Controls.Add(btn);
            _nextX += width + 6;
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

        protected T Selected<T>() where T : class => Grid.CurrentRow?.DataBoundItem as T;
        protected void Bind(object dataSource) { Grid.DataSource = null; Grid.DataSource = dataSource; }

        protected void SafeRun(Action action)
        {
            try { action(); }
            catch (Exception ex) { UIMessageBox.ShowError("操作失败：" + ex.Message); }
        }

        protected abstract void BuildColumns();
        protected abstract void ReloadData();
    }
}
