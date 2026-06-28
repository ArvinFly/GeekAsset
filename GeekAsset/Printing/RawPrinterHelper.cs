using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace GeekAsset.Printing
{
    /// <summary>
    /// 通过 Windows 打印后台(winspool)向打印机队列发送 RAW 原始字节。
    /// 用于直发 TSPL/CPCL 指令到热敏标签机，绕过 GDI 绘图。
    /// </summary>
    public static class RawPrinterHelper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)] public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)] public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)] public string pDataType;
        }

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true)]
        private static extern bool OpenPrinter(string src, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true)]
        private static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true)]
        private static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true)]
        private static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true)]
        private static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true)]
        private static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true)]
        private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        /// <summary>把字节原样发送到指定打印机队列。失败抛 Win32Exception。</summary>
        public static void SendBytes(string printerName, byte[] bytes)
        {
            if (string.IsNullOrEmpty(printerName)) throw new ArgumentException("未指定打印机。");
            if (bytes == null || bytes.Length == 0) return;

            IntPtr hPrinter;
            if (!OpenPrinter(printerName, out hPrinter, IntPtr.Zero))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "无法打开打印机：" + printerName);

            IntPtr pBytes = Marshal.AllocCoTaskMem(bytes.Length);
            try
            {
                Marshal.Copy(bytes, 0, pBytes, bytes.Length);
                var di = new DOCINFOA { pDocName = "GeekAsset 资产标签", pDataType = "RAW" };

                if (!StartDocPrinter(hPrinter, 1, di))
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "StartDocPrinter 失败。");
                try
                {
                    if (!StartPagePrinter(hPrinter))
                        throw new Win32Exception(Marshal.GetLastWin32Error(), "StartPagePrinter 失败。");
                    int written;
                    if (!WritePrinter(hPrinter, pBytes, bytes.Length, out written) || written != bytes.Length)
                        throw new Win32Exception(Marshal.GetLastWin32Error(), "WritePrinter 写入不完整。");
                    EndPagePrinter(hPrinter);
                }
                finally { EndDocPrinter(hPrinter); }
            }
            finally
            {
                Marshal.FreeCoTaskMem(pBytes);
                ClosePrinter(hPrinter);
            }
        }
    }
}
