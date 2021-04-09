using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SandBurst
{
    static class ErrorHelper
    {
        public static void ShowErrorMessage(string message, int win32ErrorCode)
        {
            MessageBox.Show(message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }

        public static void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }
    }
}
