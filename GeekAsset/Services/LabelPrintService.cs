using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using GeekAsset.Models;
using GeekAsset.Printing;

namespace GeekAsset.Services
{
    /// <summary>
    /// 资产标签打印（TSPL 指令直发热敏标签机）。标签内容：资产名称 + 编号 + 二维码(编号)。
    /// 中文字体名各品牌不同，如打印乱码可改 ChineseFont（TSC/Gprinter 多为 TST24.BF2）。
    /// 中文需按 GB2312 编码下发。默认标签 40mm×30mm（203dpi，8 点/mm）。
    /// </summary>
    public static class LabelPrintService
    {
        /// <summary>打印机内置简体中文字体名（按机型可调）。</summary>
        public static string ChineseFont = "TST24.BF2";

        public static List<string> GetPrinters()
        {
            var list = new List<string>();
            foreach (string p in PrinterSettings.InstalledPrinters) list.Add(p);
            return list;
        }

        public static string DefaultPrinter() => new PrinterSettings().PrinterName;

        /// <summary>生成单张标签的 TSPL 指令（copies 控制打印份数）。</summary>
        public static string BuildTspl(AssetListItem a, int copies)
        {
            if (copies < 1) copies = 1;
            string name = Clean(a.AssetName);
            string no = Clean(a.AssetNo);
            string sub = Clean(string.IsNullOrEmpty(a.Model) ? a.CategoryName : a.Model);

            var sb = new StringBuilder();
            sb.Append("SIZE 40 mm, 30 mm\r\n");
            sb.Append("GAP 2 mm, 0 mm\r\n");
            sb.Append("DIRECTION 1\r\n");
            sb.Append("REFERENCE 0,0\r\n");
            sb.Append("CLS\r\n");
            sb.Append($"TEXT 16,20,\"{ChineseFont}\",0,1,1,\"{name}\"\r\n");
            sb.Append($"TEXT 16,60,\"3\",0,1,1,\"NO:{no}\"\r\n");
            if (sub.Length > 0)
                sb.Append($"TEXT 16,92,\"{ChineseFont}\",0,1,1,\"{sub}\"\r\n");
            sb.Append($"QRCODE 210,120,L,4,A,0,\"{no}\"\r\n");
            sb.Append($"PRINT {copies},1\r\n");
            return sb.ToString();
        }

        /// <summary>生成二维码位图（内容=资产编号），供屏幕预览。</summary>
        public static Bitmap BuildQrBitmap(string content, int sizePx)
        {
            var writer = new ZXing.BarcodeWriter
            {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new ZXing.QrCode.QrCodeEncodingOptions
                {
                    Width = sizePx,
                    Height = sizePx,
                    Margin = 1,
                    ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.M
                }
            };
            return writer.Write(content ?? "");
        }

        /// <summary>是否为 GDI 虚拟打印机（PDF/XPS/OneNote/Fax）——这类无法解释 TSPL。</summary>
        public static bool IsVirtualPrinter(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            string n = name.ToLowerInvariant();
            return n.Contains("pdf") || n.Contains("xps") || n.Contains("onenote") || n.Contains("fax") || n.Contains("document writer");
        }

        /// <summary>直发到打印机（中文按 GB2312 编码）。</summary>
        public static void Print(string printerName, AssetListItem a, int copies)
        {
            string tspl = BuildTspl(a, copies);
            byte[] bytes = Encoding.GetEncoding("GB2312").GetBytes(tspl);
            RawPrinterHelper.SendBytes(printerName, bytes);
        }

        // 转义 TSPL 字符串中的引号/反斜杠，避免指令被截断
        private static string Clean(string s)
            => string.IsNullOrEmpty(s) ? "" : s.Replace("\\", "\\\\").Replace("\"", "'").Replace("\r", " ").Replace("\n", " ");
    }
}
