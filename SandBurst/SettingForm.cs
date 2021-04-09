using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SandBurst
{
    public partial class SettingForm : Form
    {
        public CorrectionSetting Setting { get; }

        public SettingForm(CorrectionSetting setting)
        {
            InitializeComponent();

            Setting = DeepCopyHelper.DeepCopy<CorrectionSetting>(setting);


            // 基本設定
            nameTextBox.Text = Setting.Name;
            thumbnailCheckbox.Checked = Setting.Thumbnail;
            windowCenterCheckBox.Checked = Setting.CentralizesWindow;
            windowSizeCheckBox.Checked = Setting.WindowSize;
            excludeTaskbarCheckBox.Checked = Setting.ExcludesTaskbar;
            limitTaskbarCheckBox.Checked = Setting.LimitsTaskbar;
            childWindowSizeCheckBox.Checked = Setting.ChildWindowSize;

            // マウス補正
            hookMessageCheckBox.Checked = Setting.MsgHook;
            hookSetCursorPosCheckBox.Checked = Setting.SetCursorPos;
            hookGetCursorPosCheckBox.Checked = Setting.GetCursorPos;
            hookClipCursorCheckBox.Checked = Setting.ClipCursor;

            // ウィンドウ補正
            hookMoveWindowCheckBox.Checked = Setting.MoveWindow;
            hookSetWindowPosCheckBox.Checked = Setting.SetWindowPos;
            hookSetWindowPlacementCheckBox.Checked = Setting.SetWindowPlacement;
            hookGetWindowRectCheckBox.Checked = Setting.GetWindowRect;
            hookGetClientRectCheckBox.Checked = Setting.GetClientRect;
            hookWmSizeCheckBox.Checked = Setting.WmSize;
            hookWmWndowposCheckBox.Checked = Setting.WmWindowPos;

            // その他の補正
            hookScreenshotCheckBox.Checked = Setting.ScreenShot;
            hookDirectXcheckBox.Checked = Setting.D3D;
            filterComboBox.SelectedIndex = (int)Setting.Filter;
 
             // 描画モード
            dwmRadioButton.Checked = Setting.DWMMode;
            dirextXRadioButton.Checked = !Setting.DWMMode;
 
            // フック箇所
            wow64RadioButton.Checked = Setting.HookType;
            winAPIRadioButton.Checked = !Setting.HookType;

            // ディスプレイ
            displayComboBox.Items.Clear();
            for (int i = 0; i < Screen.AllScreens.Length; i++)
            {
                displayComboBox.Items.Add($"显示器{i+1}");
            }
            if (Setting.DisplayIndex < Screen.AllScreens.Length)
            {
                displayComboBox.SelectedIndex = Setting.DisplayIndex;
            }
            else
            {
                displayComboBox.SelectedIndex = 0;
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            // 基本設定
            Setting.Name = nameTextBox.Text;
            Setting.Thumbnail = thumbnailCheckbox.Checked;
            Setting.CentralizesWindow = windowCenterCheckBox.Checked;
            Setting.WindowSize = windowSizeCheckBox.Checked;
            Setting.ExcludesTaskbar = excludeTaskbarCheckBox.Checked;
            Setting.LimitsTaskbar = limitTaskbarCheckBox.Checked;
            Setting.ChildWindowSize = childWindowSizeCheckBox.Checked;

            // マウス補正
            Setting.MsgHook = hookMessageCheckBox.Checked;
            Setting.SetCursorPos = hookSetCursorPosCheckBox.Checked;
            Setting.GetCursorPos = hookGetCursorPosCheckBox.Checked;
            Setting.ClipCursor = hookClipCursorCheckBox.Checked;

            // ウィンドウ補正
            Setting.MoveWindow = hookMoveWindowCheckBox.Checked;
            Setting.SetWindowPos = hookSetWindowPosCheckBox.Checked;
            Setting.SetWindowPlacement = hookSetWindowPlacementCheckBox.Checked;
            Setting.GetWindowRect = hookGetWindowRectCheckBox.Checked;
            Setting.GetClientRect = hookGetClientRectCheckBox.Checked;
            Setting.WmSize = hookWmSizeCheckBox.Checked;
            Setting.WmWindowPos = hookWmWndowposCheckBox.Checked;

            // その他の補正
            Setting.ScreenShot = hookScreenshotCheckBox.Checked;
            Setting.D3D = hookDirectXcheckBox.Checked;
            Setting.Filter = (D3DFilter)filterComboBox.SelectedIndex;

            // 描画モード
            Setting.DWMMode = dwmRadioButton.Checked;

            // フック箇所
            Setting.HookType = wow64RadioButton.Checked;

            // ディスプレイ
            Setting.DisplayIndex = displayComboBox.SelectedIndex;
        }
    }
}
