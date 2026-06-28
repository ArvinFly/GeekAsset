using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Sunny.UI;

namespace GeekAsset.Views
{
    public enum EditFieldType { Text, Multiline, Int, Decimal, Combo, Date }

    /// <summary>编辑字段定义。</summary>
    public class EditField
    {
        public string Key;
        public string Label;
        public EditFieldType Type = EditFieldType.Text;
        public object Value;
        public bool Required;
        /// <summary>下拉项：显示文本 + 实际值。</summary>
        public List<KeyValuePair<string, object>> ComboItems;

        internal Control Ctrl;

        public static EditField Text(string key, string label, object value, bool required = false)
            => new EditField { Key = key, Label = label, Type = EditFieldType.Text, Value = value, Required = required };
        public static EditField Multiline(string key, string label, object value)
            => new EditField { Key = key, Label = label, Type = EditFieldType.Multiline, Value = value };
        public static EditField Int(string key, string label, object value, bool required = false)
            => new EditField { Key = key, Label = label, Type = EditFieldType.Int, Value = value, Required = required };
        public static EditField Decimal(string key, string label, object value, bool required = false)
            => new EditField { Key = key, Label = label, Type = EditFieldType.Decimal, Value = value, Required = required };
        public static EditField Combo(string key, string label, List<KeyValuePair<string, object>> items, object value, bool required = false)
            => new EditField { Key = key, Label = label, Type = EditFieldType.Combo, ComboItems = items, Value = value, Required = required };
        public static EditField Date(string key, string label, DateTime value)
            => new EditField { Key = key, Label = label, Type = EditFieldType.Date, Value = value };
    }

    /// <summary>通用多字段编辑对话框，供各基础数据模块复用。</summary>
    public class EditDialog : UIForm
    {
        private readonly List<EditField> _fields;
        public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();

        public EditDialog(string title, List<EditField> fields)
        {
            _fields = fields;
            Text = title;
            ShowTitle = true;
            ShowIcon = false;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            const int labelX = 24, ctrlX = 120, ctrlW = 300;
            int y = 55;

            foreach (var f in _fields)
            {
                var lbl = new UILabel
                {
                    Text = f.Label + (f.Required ? " *" : ""),
                    Location = new Point(labelX, y + 6),
                    Size = new Size(90, 24),
                    Font = new Font("微软雅黑", 10F),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                Controls.Add(lbl);

                Control ctrl;
                switch (f.Type)
                {
                    case EditFieldType.Combo:
                        var cb = new UIComboBox { Location = new Point(ctrlX, y), Size = new Size(ctrlW, 32) };
                        int sel = -1;
                        for (int i = 0; i < f.ComboItems.Count; i++)
                        {
                            cb.Items.Add(f.ComboItems[i].Key);
                            if (Equals(f.ComboItems[i].Value, f.Value)) sel = i;
                        }
                        cb.SelectedIndex = sel;
                        ctrl = cb;
                        break;

                    case EditFieldType.Date:
                        ctrl = new UIDatePicker
                        {
                            Location = new Point(ctrlX, y),
                            Size = new Size(ctrlW, 32),
                            Value = f.Value is DateTime dt ? dt : DateTime.Today
                        };
                        break;

                    case EditFieldType.Multiline:
                        ctrl = new UITextBox
                        {
                            Location = new Point(ctrlX, y),
                            Size = new Size(ctrlW, 70),
                            Multiline = true,
                            Text = f.Value?.ToString() ?? ""
                        };
                        y += 40;
                        break;

                    default:
                        ctrl = new UITextBox
                        {
                            Location = new Point(ctrlX, y),
                            Size = new Size(ctrlW, 32),
                            Text = f.Value?.ToString() ?? ""
                        };
                        break;
                }
                f.Ctrl = ctrl;
                Controls.Add(ctrl);
                y += 44;
            }

            var btnOk = new UIButton { Text = "确定", Location = new Point(ctrlX, y + 8), Size = new Size(140, 36) };
            var btnCancel = new UIButton { Text = "取消", Location = new Point(ctrlX + 160, y + 8), Size = new Size(140, 36) };
            btnOk.Click += BtnOk_Click;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
            AcceptButton = btnOk;

            ClientSize = new Size(ctrlX + ctrlW + 30, y + 60);
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            var result = new Dictionary<string, object>();
            foreach (var f in _fields)
            {
                object val;
                if (f.Type == EditFieldType.Combo)
                {
                    var cb = (UIComboBox)f.Ctrl;
                    if (cb.SelectedIndex < 0)
                    {
                        if (f.Required) { UIMessageTip.ShowWarning("请选择「" + f.Label + "」"); return; }
                        val = null;
                    }
                    else val = f.ComboItems[cb.SelectedIndex].Value;
                }
                else if (f.Type == EditFieldType.Date)
                {
                    val = ((UIDatePicker)f.Ctrl).Value;
                }
                else
                {
                    string text = ((UITextBox)f.Ctrl).Text.Trim();
                    if (f.Required && string.IsNullOrEmpty(text)) { UIMessageTip.ShowWarning("请填写「" + f.Label + "」"); return; }

                    if (f.Type == EditFieldType.Int)
                    {
                        if (string.IsNullOrEmpty(text)) val = null;
                        else if (int.TryParse(text, out int iv)) val = iv;
                        else { UIMessageTip.ShowWarning("「" + f.Label + "」必须是整数"); return; }
                    }
                    else if (f.Type == EditFieldType.Decimal)
                    {
                        if (string.IsNullOrEmpty(text)) val = null;
                        else if (decimal.TryParse(text, out decimal dv)) val = dv;
                        else { UIMessageTip.ShowWarning("「" + f.Label + "」必须是数字"); return; }
                    }
                    else val = text;
                }
                result[f.Key] = val;
            }

            foreach (var kv in result) Values[kv.Key] = kv.Value;
            DialogResult = DialogResult.OK;
            Close();
        }

        // 取值辅助
        public string GetString(string key) => Values.TryGetValue(key, out var v) ? v?.ToString() : null;
        public int GetInt(string key) => Values.TryGetValue(key, out var v) && v != null ? Convert.ToInt32(v) : 0;
        public int? GetNullableInt(string key) => Values.TryGetValue(key, out var v) && v != null ? (int?)Convert.ToInt32(v) : null;
        public decimal GetDecimal(string key) => Values.TryGetValue(key, out var v) && v != null ? Convert.ToDecimal(v) : 0m;
        public DateTime GetDate(string key) => Values.TryGetValue(key, out var v) && v is DateTime dt ? dt : DateTime.Today;
    }
}
