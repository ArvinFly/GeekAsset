using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GeekAsset.Models;
using GeekAsset.Security;
using GeekAsset.Services;
using Sunny.UI;

namespace GeekAsset.Views
{
    /// <summary>资产黄金卡片：新增入库 / 编辑。</summary>
    public class AssetEditForm : UIForm
    {
        private readonly AssetInfo _model;
        private readonly bool _isNew;

        private UITextBox txtNo, txtName, txtModel, txtPrice, txtSN, txtMac, txtIp, txtOrig, txtCur, txtRemark;
        private UIComboBox cbCategory, cbStatus, cbLocation, cbEmp, cbSupplier, cbIpType;
        private UIDatePicker dpPurchase, dpWarranty;
        private UICheckBox chkWarranty;

        private const int LblX = 24, CtrlX = 120, CtrlW = 210, RLblX = 370, RCtrlX = 466;

        public AssetEditForm(AssetInfo model, bool isNew)
        {
            _model = model;
            _isNew = isNew;
            Text = isNew ? "资产入库" : "编辑资产";
            ShowTitle = true; ShowIcon = false;
            MaximizeBox = false; MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(710, 640);
            AutoScroll = true;

            Section("基本信息", 52);
            txtNo = Text2(LblX, 82, "资产编号", _model.AssetNo);
            txtName = Text2(RLblX, 82, "资产名称", _model.AssetName, RCtrlX);
            cbCategory = Combo2(LblX, 122, "资产分类", CategoryItems(), _model.CategoryID);
            txtModel = Text2(RLblX, 122, "规格型号", _model.Model, RCtrlX);
            cbStatus = Combo2(LblX, 162, "状态", StatusItems(), _model.Status);
            cbLocation = Combo2(RLblX, 162, "存放位置", LocationItems(), _model.LocationID, RCtrlX);
            cbEmp = Combo2(LblX, 202, "当前领用人", EmpItems(), _model.CurrentEmpID);
            cbSupplier = Combo2(RLblX, 202, "供应商", SupplierItems(), _model.SupplierID, RCtrlX);

            Section("财务与维保", 244);
            txtPrice = Text2(LblX, 274, "含税原值", _model.PurchasePrice.ToString("0.##"));
            dpPurchase = DatePick(RLblX, 274, "采购日期", _model.PurchaseDate ?? DateTime.Today, RCtrlX);
            // 维保到期（可选）
            new UILabel { Text = "维保到期", Location = new Point(LblX, 320), Size = new Size(86, 24), Font = F(10), TextAlign = ContentAlignment.MiddleLeft, Parent = this };
            chkWarranty = new UICheckBox { Text = "启用", Location = new Point(CtrlX, 320), Size = new Size(60, 24), Font = F(10), Parent = this, Checked = _model.WarrantyExpireDate.HasValue };
            dpWarranty = new UIDatePicker { Location = new Point(CtrlX + 66, 314), Size = new Size(150, 32), Parent = this, Value = _model.WarrantyExpireDate ?? DateTime.Today.AddYears(3) };
            chkWarranty.CheckedChanged += (s, e) => dpWarranty.Enabled = chkWarranty.Checked;
            dpWarranty.Enabled = chkWarranty.Checked;

            Section("IT 信息", 356);
            txtSN = Text2(LblX, 386, "SN序列号", _model.SN);
            txtMac = Text2(RLblX, 386, "MAC地址", _model.MacAddress, RCtrlX);
            txtIp = Text2(LblX, 426, "IP地址", _model.IPAddress);
            cbIpType = Combo2(RLblX, 426, "IP类型", IpTypeItems(), _model.IPType, RCtrlX);
            txtOrig = TextFull(466, "原始配置", _model.Original_Spec, _isNew); // 入库时可填，编辑时只读（防调包）
            txtCur = TextFull(506, "当前配置", _model.Current_Spec, true);
            txtRemark = TextFull(546, "备注", _model.Remark, true);

            var btnOk = new UIButton { Text = "确定", Location = new Point(CtrlX, 592), Size = new Size(140, 36), Parent = this };
            var btnCancel = new UIButton { Text = "取消", Location = new Point(CtrlX + 160, 592), Size = new Size(140, 36), Parent = this };
            btnOk.Click += BtnOk_Click;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            AcceptButton = btnOk;

            if (_isNew && string.IsNullOrEmpty(txtNo.Text))
                txtNo.Text = AssetService.NextAssetNo();
        }

        private static Font F(float s) => new Font("微软雅黑", s);

        private void Section(string text, int y)
        {
            new UILabel
            {
                Text = "▎" + text,
                Location = new Point(LblX - 6, y),
                Size = new Size(300, 24),
                Font = new Font("微软雅黑", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(48, 48, 48),
                Parent = this
            };
        }

        private UITextBox Text2(int labelX, int y, string label, string val, int ctrlX = CtrlX, int w = CtrlW)
        {
            new UILabel { Text = label, Location = new Point(labelX, y + 4), Size = new Size(86, 24), Font = F(10), TextAlign = ContentAlignment.MiddleLeft, Parent = this };
            return new UITextBox { Location = new Point(ctrlX, y), Size = new Size(w, 32), Text = val ?? "", Parent = this };
        }

        private UITextBox TextFull(int y, string label, string val, bool enabled)
        {
            new UILabel { Text = label, Location = new Point(LblX, y + 4), Size = new Size(86, 24), Font = F(10), TextAlign = ContentAlignment.MiddleLeft, Parent = this };
            return new UITextBox { Location = new Point(CtrlX, y), Size = new Size(556, 32), Text = val ?? "", Enabled = enabled, Parent = this };
        }

        private UIComboBox Combo2(int labelX, int y, string label, List<KeyValuePair<string, object>> items, object val, int ctrlX = CtrlX, int w = CtrlW)
        {
            new UILabel { Text = label, Location = new Point(labelX, y + 4), Size = new Size(86, 24), Font = F(10), TextAlign = ContentAlignment.MiddleLeft, Parent = this };
            var cb = new UIComboBox { Location = new Point(ctrlX, y), Size = new Size(w, 32), Parent = this, Tag = items };
            int sel = -1;
            for (int i = 0; i < items.Count; i++)
            {
                cb.Items.Add(items[i].Key);
                if (Equals(items[i].Value, val)) sel = i;
            }
            cb.SelectedIndex = sel;
            return cb;
        }

        private UIDatePicker DatePick(int labelX, int y, string label, DateTime val, int ctrlX = CtrlX, int w = CtrlW)
        {
            new UILabel { Text = label, Location = new Point(labelX, y + 4), Size = new Size(86, 24), Font = F(10), TextAlign = ContentAlignment.MiddleLeft, Parent = this };
            return new UIDatePicker { Location = new Point(ctrlX, y), Size = new Size(w, 32), Parent = this, Value = val };
        }

        private static object ComboVal(UIComboBox cb)
        {
            var items = (List<KeyValuePair<string, object>>)cb.Tag;
            return cb.SelectedIndex >= 0 ? items[cb.SelectedIndex].Value : null;
        }

        // ---- 下拉数据源 ----
        private static List<KeyValuePair<string, object>> StatusItems() => new List<KeyValuePair<string, object>>
        {
            new KeyValuePair<string, object>("闲置",(byte)1), new KeyValuePair<string, object>("在用",(byte)2),
            new KeyValuePair<string, object>("报修",(byte)3), new KeyValuePair<string, object>("借出",(byte)4),
            new KeyValuePair<string, object>("报废",(byte)5),
        };
        private static List<KeyValuePair<string, object>> IpTypeItems() => new List<KeyValuePair<string, object>>
        {
            new KeyValuePair<string, object>("(无)", null),
            new KeyValuePair<string, object>("固定IP",(byte)1), new KeyValuePair<string, object>("DHCP",(byte)2),
        };
        private static List<KeyValuePair<string, object>> CategoryItems()
        {
            var list = new List<KeyValuePair<string, object>>();
            foreach (var c in CategoryService.GetAll()) list.Add(new KeyValuePair<string, object>(c.CategoryName, c.CategoryID));
            return list;
        }
        private static List<KeyValuePair<string, object>> LocationItems()
        {
            var list = new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>("(无)", null) };
            foreach (var c in LocationService.GetAll()) list.Add(new KeyValuePair<string, object>(c.LocationName, c.LocationID));
            return list;
        }
        private static List<KeyValuePair<string, object>> EmpItems()
        {
            var list = new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>("(无)", null) };
            foreach (var c in EmployeeService.GetAll()) list.Add(new KeyValuePair<string, object>(c.EmpName, c.EmpID));
            return list;
        }
        private static List<KeyValuePair<string, object>> SupplierItems()
        {
            var list = new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>("(无)", null) };
            foreach (var c in SupplierService.GetAll()) list.Add(new KeyValuePair<string, object>(c.SupplierName, c.SupplierID));
            return list;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            string no = txtNo.Text.Trim(), name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(no)) { UIMessageTip.ShowWarning("请填写资产编号"); return; }
            if (string.IsNullOrEmpty(name)) { UIMessageTip.ShowWarning("请填写资产名称"); return; }
            if (cbCategory.SelectedIndex < 0) { UIMessageTip.ShowWarning("请选择资产分类"); return; }
            if (!decimal.TryParse(txtPrice.Text.Trim(), out decimal price)) { UIMessageTip.ShowWarning("含税原值必须是数字"); return; }
            if (AssetService.AssetNoExists(no, _model.AssetID)) { UIMessageTip.ShowWarning("资产编号已存在"); return; }

            _model.AssetNo = no;
            _model.AssetName = name;
            _model.CategoryID = (int?)ComboVal(cbCategory);
            _model.Model = txtModel.Text.Trim();
            _model.Status = Convert.ToByte(ComboVal(cbStatus));
            _model.LocationID = (int?)ComboVal(cbLocation);
            _model.CurrentEmpID = (int?)ComboVal(cbEmp);
            _model.SupplierID = (int?)ComboVal(cbSupplier);
            _model.PurchasePrice = price;
            _model.PurchaseDate = dpPurchase.Value;
            _model.WarrantyExpireDate = chkWarranty.Checked ? (DateTime?)dpWarranty.Value : null;
            _model.SN = txtSN.Text.Trim();
            _model.MacAddress = txtMac.Text.Trim();
            _model.IPAddress = txtIp.Text.Trim();
            _model.IPType = (byte?)ComboVal(cbIpType);
            if (_isNew) _model.Original_Spec = txtOrig.Text.Trim();   // 防调包：原始配置仅入库时写入
            _model.Current_Spec = txtCur.Text.Trim();
            _model.Remark = txtRemark.Text.Trim();
            if (_isNew) _model.CreateBy = CurrentUser.UserID;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
