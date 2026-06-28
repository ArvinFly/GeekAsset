using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GeekAsset.Models;
using GeekAsset.Services;
using Sunny.UI;

namespace GeekAsset.Views
{
    /// <summary>资产标签可视化预览：按 40×30mm 比例在屏幕上画出名称/编号/副行 + 真二维码。</summary>
    public class LabelPreviewDialog : UIForm
    {
        private const int Scale = 10;       // 10 像素/mm
        private const int LabelW = 40 * Scale;  // 400
        private const int LabelH = 30 * Scale;  // 300

        private readonly AssetListItem _asset;
        private readonly Bitmap _qr;

        public LabelPreviewDialog(AssetListItem asset)
        {
            _asset = asset;
            _qr = LabelPrintService.BuildQrBitmap(asset.AssetNo, 130);

            Text = "标签预览（40×30mm 实际比例）";
            ShowIcon = false;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(LabelW + 80, LabelH + 130);
            Padding = new Padding(0, 35, 0, 0);

            var canvas = new Panel
            {
                Location = new Point(40, 50),
                Size = new Size(LabelW, LabelH),
                BackColor = Color.White
            };
            canvas.Paint += Canvas_Paint;
            Controls.Add(canvas);

            var tip = new UILabel
            {
                Text = "提示：此为屏幕预览。实际打印须用热敏标签机（TSPL），PDF 等虚拟打印机会生成损坏文件。",
                Location = new Point(40, LabelH + 60), Size = new Size(LabelW, 40), ForeColor = Color.Gray
            };
            Controls.Add(tip);

            var btnTspl = new UIButton { Text = "查看 TSPL 指令", Location = new Point(40, LabelH + 96), Size = new Size(130, 32) };
            btnTspl.Click += (s, e) => UIMessageBox.ShowInfo("TSPL 指令：\r\n\r\n" + LabelPrintService.BuildTspl(_asset, 1));
            var btnClose = new UIButton { Text = "关闭", Location = new Point(LabelW - 50, LabelH + 96), Size = new Size(90, 32) };
            btnClose.Click += (s, e) => Close();
            Controls.AddRange(new Control[] { btnTspl, btnClose });
        }

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // 标签边框
            using (var pen = new Pen(Color.FromArgb(180, 180, 180)))
                g.DrawRectangle(pen, 0, 0, LabelW - 1, LabelH - 1);

            string name = _asset.AssetName ?? "";
            string sub = string.IsNullOrEmpty(_asset.Model) ? _asset.CategoryName : _asset.Model;

            using (var fName = new Font("微软雅黑", 15F, FontStyle.Bold))
            using (var fNo = new Font("Consolas", 13F, FontStyle.Bold))
            using (var fSub = new Font("微软雅黑", 11F))
            using (var brush = new SolidBrush(Color.Black))
            {
                // 左侧文字区（给右侧二维码留位）
                var textRect = new RectangleF(16, 18, LabelW - _qr.Width - 40, 40);
                g.DrawString(name, fName, brush, textRect);
                g.DrawString("NO:" + _asset.AssetNo, fNo, brush, 16, 70);
                if (!string.IsNullOrEmpty(sub))
                    g.DrawString(sub, fSub, brush, 16, 100);
            }

            // 右侧二维码
            g.DrawImage(_qr, LabelW - _qr.Width - 16, (LabelH - _qr.Height) / 2, _qr.Width, _qr.Height);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _qr?.Dispose();
            base.Dispose(disposing);
        }
    }
}
