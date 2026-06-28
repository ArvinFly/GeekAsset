using System;
using System.Drawing;
using System.Windows.Forms;
using GeekAsset.Models;
using GeekAsset.Services;
using Sunny.UI;

namespace GeekAsset.Views
{
    /// <summary>资产标签打印对话框：选打印机、份数，可预览 TSPL 指令。</summary>
    public class LabelPrintDialog : UIForm
    {
        private readonly AssetListItem _asset;
        private readonly UIComboBox _printer;
        private readonly UITextBox _copies;

        public LabelPrintDialog(AssetListItem asset)
        {
            _asset = asset;
            Text = "打印资产标签 - " + asset.AssetName;
            ShowIcon = false;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(440, 230);
            Padding = new Padding(0, 35, 0, 0);

            Controls.Add(new UILabel { Text = "资产编号：" + asset.AssetNo, Location = new Point(24, 50), AutoSize = true });
            Controls.Add(new UILabel { Text = "打印机", Location = new Point(24, 90), AutoSize = true });
            _printer = new UIComboBox { Location = new Point(110, 84), Width = 300, DropDownStyle = UIDropDownStyle.DropDownList };
            foreach (var p in LabelPrintService.GetPrinters()) _printer.Items.Add(p);
            string def = LabelPrintService.DefaultPrinter();
            if (_printer.Items.Contains(def)) _printer.SelectedItem = def;
            else if (_printer.Items.Count > 0) _printer.SelectedIndex = 0;
            Controls.Add(_printer);

            Controls.Add(new UILabel { Text = "打印份数", Location = new Point(24, 132), AutoSize = true });
            _copies = new UITextBox { Location = new Point(110, 126), Width = 80, Text = "1" };
            Controls.Add(_copies);

            var btnPrint = new UIButton { Text = "打印", Location = new Point(110, 178), Size = new Size(90, 34) };
            btnPrint.Click += (s, e) => DoPrint();
            var btnPreview = new UIButton { Text = "预览标签", Location = new Point(210, 178), Size = new Size(90, 34) };
            btnPreview.Click += (s, e) => new LabelPreviewDialog(_asset).ShowDialog();
            var btnCancel = new UIButton { Text = "取消", Location = new Point(310, 178), Size = new Size(90, 34) };
            btnCancel.Click += (s, e) => Close();
            Controls.AddRange(new Control[] { btnPrint, btnPreview, btnCancel });
        }

        private int Copies()
        {
            int n;
            return int.TryParse(_copies.Text, out n) && n > 0 ? n : 1;
        }

        private void DoPrint()
        {
            string printer = _printer.SelectedItem as string;
            if (string.IsNullOrEmpty(printer)) { UIMessageTip.ShowWarning("未检测到可用打印机"); return; }
            if (LabelPrintService.IsVirtualPrinter(printer) &&
                !UIMessageBox.ShowAsk($"「{printer}」看起来是 PDF/XPS 等虚拟打印机，无法解释 TSPL 指令，直发会生成损坏文件。\r\n\r\n如只想看效果，请用「预览标签」。仍要继续打印吗？"))
                return;
            try
            {
                LabelPrintService.Print(printer, _asset, Copies());
                UIMessageBox.ShowSuccess("标签已发送到打印机：" + printer);
                Close();
            }
            catch (Exception ex)
            {
                UIMessageBox.ShowError("打印失败：" + ex.Message);
            }
        }
    }
}
