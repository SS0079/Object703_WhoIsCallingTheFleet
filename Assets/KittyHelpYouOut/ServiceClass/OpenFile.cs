using System;
using System.Runtime.InteropServices;

namespace KittyHelpYouOut.ServiceClass
{
    
    public static class OpenFile
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class FileDlg
        {
            public int structSize = 0;
            public IntPtr dlgOwner = IntPtr.Zero;
            public IntPtr instance = IntPtr.Zero;
            public String filter = null;
            public String customFilter = null;
            public int maxCustFilter = 0;
            public int filterIndex = 0;
            public String file = null;
            public int maxFile = 0;
            public String fileTitle = null;
            public int maxFileTitle = 0;
            public String initialDir = null;
            public String title = null;
            public int flags = 0;
            public short fileOffset = 0;
            public short fileExtension = 0;
            public String defExt = null;
            public IntPtr custData = IntPtr.Zero;
            public IntPtr hook = IntPtr.Zero;
            public String templateName = null;
            public IntPtr reservedPtr = IntPtr.Zero;
            public int reservedInt = 0;
            public int flagsEx = 0;
        }
        [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        private static extern bool GetOpenFileName([In, Out] FileDlg ofd);
        
        /// <summary>
        /// 打开一个资源管理器窗口并拣选文件
        /// </summary>
        /// <param name="path">默认路径</param>
        /// <returns>被拣选文件的路径</returns>
        public static string Open(string path)
        {
            string result=String.Empty;
            FileDlg pth = new FileDlg();
            pth.structSize = System.Runtime.InteropServices.Marshal.SizeOf(pth);
            pth.filter = "txt (*.txt)";
            pth.file = new string(new char[256]);
            pth.maxFile = pth.file.Length;
            pth.fileTitle = new string(new char[64]);
            pth.maxFileTitle = pth.fileTitle.Length;
            pth.initialDir = path;  // default path  
            pth.title = "打开文件";
            pth.defExt = "txt";
            pth.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
            //0x00080000   是否使用新版文件选择窗口
            //0x00000200   是否可以多选文件
            if (GetOpenFileName(pth))
            {
                result = pth.file;//选择的文件路径;  
            }
            return result;
        }

    }
}