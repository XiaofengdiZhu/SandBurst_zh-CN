using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;

namespace SandBurst
{
    public partial class MainForm : Form
    {
        [DllImport("WinHelper.dll", EntryPoint = "GetD3DDevice9PresentRVA")]
        public extern static uint GetD3DDevice9PresentRVA();

        public const string DataDirectory = "Data";
        public const string UserDirectory = DataDirectory + "\\User";
        private const string DefaultSettingFile = DataDirectory + "\\DefaultSettings.ini";
        private const string SettingFile = UserDirectory + "\\Settings.ini";
        private const string PreferenceFile = UserDirectory + "\\Preferences.ini";
        private const string HistoryFile = UserDirectory + "\\Histories.xml";
        private const string IgnoreFile = UserDirectory + "\\ignores.txt";

        /// <summary>
        /// 選択されたウィンドウ
        /// </summary>
        private WindowInformation selectedWindow;

        /// <summary>
        /// 現在のSetting
        /// </summary>
        private CorrectionSetting currentSetting;

        /// <summary>
        /// settings.iniの操作ラッパー
        /// </summary>
        private SettingManager settingManager;

        /// <summary>
        /// SandBurst自体の設定
        /// </summary>
        private Preference preference;

        /// <summary>
        /// 描画エンジン
        /// </summary>
        private Renderer renderer;

        /// <summary>
        /// CoreDll操作インスタンス
        /// </summary>
        private CoreServer coreServer;

        /// <summary>
        /// 描画用シャドウウィンドウ
        /// </summary>
        private ShadowWindow shadowWindow;

        /// <summary>
        /// 拡大しているウィンドウ
        /// </summary>
        private IntPtr targetWindow;

        /// <summary>
        /// 拡大履歴
        /// </summary>
        private Histories histories;
                
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Formを初期化する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                settingManager = new SettingManager(SettingFile, DefaultSettingFile);
                preference = new Preference(PreferenceFile);
                coreServer = new CoreServer();
                renderer = new Renderer();
                shadowWindow = new ShadowWindow();
                LoadHistory();

                groupBox1.Paint += OnPaint;
                groupBox2.Paint += OnPaint;

                Text = "SandBurst " + VersionMamager.CurrentVersion;

                EnablePrivilege("SeDebugPrivilege", true);

                // Settingをロード
                List<string> names = settingManager.GetSettingNames();

                if (names.Count > 0)
                {
                    foreach (string name in names)
                    {
                        settingListBox.Items.Add(name);
                    }

                    currentSetting = settingManager.LoadSetting(names[0]);
                }
                else
                {
                    // 何も無い場合はDefault設定をロード
                    currentSetting = settingManager.LoadDefaultSetting();
                    settingListBox.Items.Add(currentSetting.Name);
                }

                settingListBox.SelectedIndex = 0;
                UpdateUI(currentSetting);

                // 拡大率ボタンを設定する
                scaleButton1.Text = $"{preference.Scale1}%";
                scaleButton2.Text = $"{preference.Scale2}%";
                scaleButton3.Text = $"{preference.Scale3}%";
                scaleButton4.Text = $"{preference.Scale4}%";
                scaleButton5.Text = $"{preference.Scale5}%";

                // 表示位置を設定する
                Point pos = new Point
                {
                    X = preference.X,
                    Y = preference.Y
                };

                Location = pos;

                // 更新チェック
                if (preference.AutoUpdate)
                {
                    CheckUpdate();
                }
            }
            catch (Exception error)
            {
                ErrorHelper.ShowErrorMessage(error.ToString());
                this.Close();
            }
        }

        /// <summary>
        /// Formを閉じる
        /// currentSetting, preferenceを保存する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (currentSetting != null)
                settingManager.SaveSetting(currentSetting);

            if (WindowState == FormWindowState.Normal)
            {
                preference.X = Location.X;
                preference.Y = Location.Y;
            }
            else
            {
                preference.X = RestoreBounds.Left;
                preference.Y = RestoreBounds.Top;
            }
            
            preference.SaveToFile(PreferenceFile);
        }

        /// <summary>
        /// トップレベルウィンドウ一覧を取得してポップアップに表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowSelectButton_Click(object sender, EventArgs e)
        {
            windowMenu.Items.Clear();
            WindowSelector selector = new WindowSelector(IgnoreFile, new IntPtr[]{Handle, shadowWindow.Window});

            List<WindowInformation> windowList = selector.GetWindows();

            foreach (WindowInformation info in windowList)
            {
                ToolStripItem item = windowMenu.Items.Add(info.Title, null, WindowMenuItem_Click);
                item.Tag = info;
            }

            Point Pos = System.Windows.Forms.Cursor.Position;

            this.windowMenu.Show(Pos);
        }

        /// <summary>
        /// ポップアップで選択されたウィンドウをselectedWindowに設定する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripItem item = sender as ToolStripItem;

            if (item != null)
            {
                selectedWindow = item.Tag as WindowInformation;
                windowTextBox.Text = selectedWindow.Title;
            }
        }

        /// <summary>
        /// 拡大率を変更する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScaleBar_Scroll(object sender, EventArgs e)
        {
            scaleLabel.Text = $"{scaleBar.Value}%";

            if (currentSetting != null)
                currentSetting.Scale = scaleBar.Value;
        }

        /// <summary>
        /// 拡大方法を比率、倍率、横幅指定で切り替える
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScaleRadioButton_Click(object sender, EventArgs e)
        {
            scalePanel1.Visible = scaleRadioButton.Checked;
            scalePanel2.Visible = widthRadioButton.Checked;
            scalePanel3.Visible = ratioRadioButton.Checked;

            if (currentSetting != null)
            {
                if (scaleRadioButton.Checked)
                {
                    currentSetting.MagnificationMode = (int)ScaleMode.Magnification;
                }
                if (widthRadioButton.Checked)
                {
                    currentSetting.MagnificationMode = (int)ScaleMode.Width;
                }
                if (ratioRadioButton.Checked)
                {
                    currentSetting.MagnificationMode = (int)ScaleMode.Ratio;
                }
            }
                
        }
                
        /// <summary>
        /// セッティングを選択し、ロードする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingListBox_Click(object sender, EventArgs e)
        {
            if (currentSetting != null)
                settingManager.SaveSetting(currentSetting);

            ListBox box = sender as ListBox;

            string name = box.SelectedItem as string;

            if (name == null)
                return;

            currentSetting = settingManager.LoadSetting(name);

            UpdateUI(currentSetting);
        }

        /// <summary>
        /// settingに応じてUIを更新する
        /// </summary>
        /// <param name="setting"></param>
        private void UpdateUI(CorrectionSetting setting)
        {
            scaleBar.Value = setting.Scale;
            scaleLabel.Text = $"{setting.Scale}%";
            widthTextBox.Text = setting.Width.ToString();
            ratioBar.Value = setting.Ratio;
            ratioLabel.Text = $"{setting.Ratio}%";

            scaleRadioButton.Checked = setting.MagnificationMode == (int)ScaleMode.Magnification;
            widthRadioButton.Checked = setting.MagnificationMode == (int)ScaleMode.Width;
            ratioRadioButton.Checked = setting.MagnificationMode == (int)ScaleMode.Ratio;
        }

        /// <summary>
        /// Settingを編集する
        /// </summary>
        /// <param name="setting"></param>
        /// <returns>編集に成功したらtrue、キャンセルしたらfalse</returns>
        private bool EditSetting(CorrectionSetting setting)
        {
            SettingForm form = new SettingForm(setting);

            while (true)
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    // 現在の設定名と新しい設定名が同じかチェック
                    if (setting.Name != form.Setting.Name)
                    {
                        // 違う場合は新しい名前の重複チェック
                        if (settingManager.Exists(form.Setting.Name))
                        {
                            ErrorHelper.ShowErrorMessage("既に同じ名前の設定があります。\n違う名前にしてください。");
                            continue;
                        }

                        // 古い設定を削除
                        settingManager.DeleteSetting(setting.Name);
                    }

                    // 新しい設定を保存
                    currentSetting = DeepCopyHelper.DeepCopy<CorrectionSetting>(form.Setting);
                    settingManager.SaveSetting(currentSetting);                    
                    settingListBox.Items[settingListBox.SelectedIndex] = currentSetting.Name;

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private void SettingListBox_DoubleClick(object sender, EventArgs e)
        {
            EditSetting(currentSetting);
        }

        /// <summary>
        /// 新しいSettingを作成する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemAdd_Click(object sender, EventArgs e)
        {
            int index = settingListBox.Items.Add("新しい設定");
            settingListBox.SelectedIndex = index;

            CorrectionSetting newSetting = settingManager.LoadDefaultSetting();
            newSetting.Name = "新しい設定";

            if (!EditSetting(newSetting))
            {
                settingListBox.Items.RemoveAt(index);
            }
        }

        /// <summary>
        /// Settingを削除する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemDelete_Click(object sender, EventArgs e)
        {
            settingManager.DeleteSetting(currentSetting.Name);
            settingListBox.Items.Remove(settingListBox.SelectedItem);
            currentSetting = null;
        }

        private void ExecuteButton_Click(object sender, EventArgs e)
        {
            if (currentSetting == null)
                return;

            settingManager.SaveSetting(currentSetting);

            if (selectedWindow == null)
                return;
            
            if ((Win32.API.GetWindowLong(selectedWindow.Window, Win32.Constants.GWL_STYLE) & Win32.Constants.WS_MINIMIZE) != 0)
            {
                ErrorHelper.ShowErrorMessage("最小化されたウィンドウは拡大できません");
                return;
            }

            if (StartScale())
            {
                AddHistory(selectedWindow);
            }
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            if ((Win32.API.GetWindowLong(selectedWindow.Window, Win32.Constants.GWL_STYLE) & Win32.Constants.WS_MINIMIZE) != 0)
            {
                ErrorHelper.ShowErrorMessage("最小化されたウィンドウは停止できません");
                return;
            }

            StopScale();
        }

        /// <summary>
        /// 特権を有効化する
        /// </summary>
        /// <param name="privilege"></param>
        /// <param name="enable"></param>
        private void EnablePrivilege(string privilege, bool enable)
        {
            IntPtr token = IntPtr.Zero;
            Win32.LUID luid;
            Win32.TOKEN_PRIVILEGES tp;

            try
            {
                if (Win32.API.OpenProcessToken((IntPtr)(-1), Win32.Constants.TOKEN_QUERY | Win32.Constants.TOKEN_ADJUST_PRIVILEGES, out token) == 0)
                    return;

                if (Win32.API.LookupPrivilegeValue(null, privilege, out luid) == 0)
                    return;

                tp.PrivilegeCount = 1;
                tp.Privileges.Luid = luid;

                tp.Privileges.Attributes = enable ? Win32.Constants.SE_PRIVILEGE_ENABLED : 0;

                uint len;
                Win32.TOKEN_PRIVILEGES prev;
                Win32.API.AdjustTokenPrivileges(token, 0, ref tp, (uint)Marshal.SizeOf(tp), out prev, out len);
            }
            finally
            {
                if (token != IntPtr.Zero)
                    Win32.API.CloseHandle(token);
            }
        }

        /// <summary>
        /// 表示するスクリーンを取得
        /// </summary>
        /// <returns></returns>
        private Screen GetTargetScreen()
        {
            int di = currentSetting.DisplayIndex;
            if (di < Screen.AllScreens.Length)
            {
                return Screen.AllScreens[di];
            }

            return Screen.AllScreens[0];
        }

        /// <summary>
        /// 拡大率をクリップして取得する
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        private int GetScale(IntPtr window)
        {
            switch((ScaleMode)currentSetting.MagnificationMode)
            {
                case ScaleMode.Magnification:
                    return WindowHelper.ClipScale(window, currentSetting.Scale, preference.ScaleLimitation, currentSetting.LimitsTaskbar);

                case ScaleMode.Width:
                    int scale = WindowHelper.GetScaleFromWidth(window, currentSetting.Width);
                    return WindowHelper.ClipScale(window, scale, preference.ScaleLimitation, currentSetting.LimitsTaskbar);

                case ScaleMode.Ratio:
                    scale = WindowHelper.GetScaleFromRatio(window, currentSetting.Ratio, GetTargetScreen());
                    return WindowHelper.ClipScale(window, scale, preference.ScaleLimitation, currentSetting.LimitsTaskbar);

                default:
                    return 0;
            }
        }
           

        /// <summary>
        /// 拡大を開始
        /// </summary>
        unsafe private bool StartScale()
        {
            Renderer.RenderingMode mode = currentSetting.DWMMode ? Renderer.RenderingMode.Dwm : Renderer.RenderingMode.D3D;
            
            Win32.DwmAPI.DwmIsCompositionEnabled(out int dwmEnabled);


            // DWMの有無チェック
            if ((mode == Renderer.RenderingMode.Dwm) && (dwmEnabled == 0))
            {
                ErrorHelper.ShowErrorMessage("Windows Aeroが無効になっています\nDWMで拡大するにはWindows Aeroを有効化してください");
                return false;
            }

            targetWindow = selectedWindow.Window;
            int scale = GetScale(targetWindow);

            // プロセスが既に終了していたらscaleがマイナス
            if (scale < 0)
            {
                ErrorHelper.ShowErrorMessage("ウィンドウの取得に失敗しました");
                return false;
            }
                

            if (scale < 100)
            {
                ErrorHelper.ShowErrorMessage("元のウィンドウより小さくすることは出来ません");
                return false;
            }

            Win32.RECT scaledWidnowRect, scaledClientRect;

            WindowHelper.GetScaledWindowRect(targetWindow, scale, out scaledWidnowRect, out scaledClientRect);


            Win32.POINT windowSize, clientSize;
            WindowHelper.GetWindowSize(targetWindow, out windowSize, out clientSize);

            uint presetnRVA = GetD3DDevice9PresentRVA();
            //uint viewportRVA = GetD3D11SetViewportRVA();
            uint viewportRVA = 0;

            // Shadowウィンドウの設定
            shadowWindow.SetSize(scaledClientRect.right, scaledClientRect.bottom);

            int menuHeight = WindowHelper.GetMenuHeight(targetWindow);
            Win32.RECT sourceRect = new Win32.RECT
            {
                left = 0,
                top = menuHeight,
                right = clientSize.x,
                bottom = clientSize.y + menuHeight
            };

            // Windowsのバージョンを取得
            Win32.OSVERSIONINFOEX version = new Win32.OSVERSIONINFOEX();
            version.dwOSVersionInfoSize = (uint)sizeof(Win32.OSVERSIONINFOEX);
            Win32.API.RtlGetVersion(ref version);

            // Windows 7, 8, 8.1はメニューの高さ分、DWMの位置をずらす
            if ((version.dwMajorVersion == 6) && (mode == Renderer.RenderingMode.Dwm))
                scaledClientRect.bottom += menuHeight;

            // 線形補完の有無
            D3DFilter filter = currentSetting.Filter;

            // レンダリングの開始
            if (!renderer.Start(mode, targetWindow, shadowWindow.Window, sourceRect, scaledClientRect, filter, menuHeight))
            {
                return false;
            }

            Win32.RECT display = GetDisplay();

            // CoreDllのインストール
            if (!coreServer.InstallHook(targetWindow, this.Handle, shadowWindow.Window, scale, windowSize, clientSize, currentSetting, presetnRVA, viewportRVA, display))
            {
                renderer.Stop();
                return false;
            }

            shadowWindow.StartFollow(targetWindow, OnLostWindow);
            Win32.API.SetForegroundWindow(targetWindow);

            // Shadowウィンドウの表示
            shadowWindow.Visible = currentSetting.Thumbnail;
            shadowWindow.Topmost = currentSetting.Thumbnail;

            executeButton.Enabled = false;
            stopButton.Enabled = true;
            historyButton.Enabled = false;

            return true;
        }

        /// <summary>
        /// 拡大を停止
        /// </summary>
        private void StopScale()
        {
            executeButton.Enabled = true;
            stopButton.Enabled = false;

            coreServer.UninstallHook(targetWindow);
            renderer.Stop();
            shadowWindow.StopFollow();
            shadowWindow.Visible = false;
            historyButton.Enabled = true;

            targetWindow = IntPtr.Zero;
        }

        /// <summary>
        /// 横幅入力イベント 数値以外の入力を無効化する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WidthTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((char.IsControl(e.KeyChar)) || (char.IsDigit(e.KeyChar)))
            {
                e.Handled = false;
                return;
            }
            

            e.Handled = true;
        }

        /// <summary>
        /// 横幅をcurrentSettingに反映する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WidthTextBox_Leave(object sender, EventArgs e)
        {
            if (currentSetting == null)
                return;

            currentSetting.Width = int.Parse(widthTextBox.Text);
        }

        /// <summary>
        /// 拡大率ボタンをクリック
        /// 拡大率を変更する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScaleButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            string name = button.Name.Replace("scaleButton", "Scale");

            var property = typeof(Preference).GetProperty(name);

            int scale = (int)property.GetValue(preference);

            if (currentSetting != null)
                currentSetting.Scale = scale;

            scaleBar.Value = scale;
            scaleLabel.Text = $"{scale}%";
        }

        private void HistoryButton_Click(object sender, EventArgs e)
        {
            LoadHistory();

            // 履歴メニューを作成
            historyMenu.Items.Clear();

            foreach (History history in histories.Items)
            {
                ToolStripItem item = historyMenu.Items.Add(history.Title, null, History_Click);
                item.Tag = history;
            }

            if (historyMenu.Items.Count > 0)
            {
                historyMenu.Items.Add("■ 履歴を削除する ■", null, HistoryClear_Click);
            }

            Point Pos = System.Windows.Forms.Cursor.Position;

            this.historyMenu.Show(Pos);
        }

        private void History_Click(object sender, EventArgs e)
        {
            if (targetWindow != IntPtr.Zero)
                return;

            ToolStripItem item = sender as ToolStripItem;
            
            History history = item.Tag as History;

            ExcuteHistory(history);
        }

        private void HistoryClear_Click(object sender, EventArgs e)
        {
            histories.Items.Clear();
            histories.SaveToFile(HistoryFile);
        }

        private void LoadHistory()
        {
            histories = Histories.LoadFromFile(HistoryFile);
        }

        private void AddHistory(WindowInformation info)
        {
            string args = coreServer.GetCommandLine(info.Window);
            histories.Intert(info.Path, info.Title,  currentSetting.Name, info.WindowSize.x, info.WindowSize.y, args);
            histories.SaveToFile(HistoryFile);
        }

        private void OnLostWindow()
        {
            Invoke(
                (MethodInvoker)delegate ()
                {
                    StopScale();
                    selectedWindow = null;
                    windowTextBox.Text = "";
                }
            );
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            Color textColor = Color.Black;
            Color borderColor = Color.DarkGray; ;
            GroupBox box = (GroupBox)sender;
            Graphics g = e.Graphics;

            Brush textBrush = new SolidBrush(textColor);
            Brush borderBrush = new SolidBrush(borderColor);
            Pen borderPen = new Pen(borderBrush);
            SizeF strSize = g.MeasureString(box.Text, box.Font);
            Rectangle rect = new Rectangle(box.ClientRectangle.X,
                                           box.ClientRectangle.Y + (int)(strSize.Height / 2),
                                           box.ClientRectangle.Width - 1,
                                           box.ClientRectangle.Height - (int)(strSize.Height / 2) - 1);

            // Clear text and border
            g.Clear(this.BackColor);

            // Draw text
            g.DrawString(box.Text, box.Font, textBrush, box.Padding.Left, 0);

            // Drawing Border
            //Left
            g.DrawLine(borderPen, rect.Location, new Point(rect.X, rect.Y + rect.Height));
            //Right
            g.DrawLine(borderPen, new Point(rect.X + rect.Width, rect.Y), new Point(rect.X + rect.Width, rect.Y + rect.Height));
            //Bottom
            g.DrawLine(borderPen, new Point(rect.X, rect.Y + rect.Height), new Point(rect.X + rect.Width, rect.Y + rect.Height));
            //Top1
            g.DrawLine(borderPen, new Point(rect.X, rect.Y), new Point(rect.X + box.Padding.Left, rect.Y));
            //Top2
            g.DrawLine(borderPen, new Point(rect.X + box.Padding.Left + (int)(strSize.Width), rect.Y), new Point(rect.X + rect.Width, rect.Y));
        }

        /// <summary>
        /// Coreから送られるウィンドウメッセージを受け取るプロシージャ
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            uint wparam = (uint)m.WParam;

            switch (m.Msg)
            {
                // アクティブ、非アクティブが切り替わった
                case Win32.Constants.WM_USER:
                    wparam &= 0xFFFF;
                    if (wparam == Win32.Constants.WA_INACTIVE)
                    {
                        shadowWindow.Topmost = false;

                        if ((wparam & 0xFFFF0000) != 0)
                            shadowWindow.Visible = false;
                    }
                    else if ((wparam == Win32.Constants.WA_ACTIVE) || (wparam == Win32.Constants.WA_CLICKACTIVE))
                    {
                        shadowWindow.Topmost = true;
                        
                        if (currentSetting.Thumbnail)
                            shadowWindow.Visible = true;
                    }

                    break;

                // プロセスが終了した
                case Win32.Constants.WM_USER_EXIT:
                    if (m.WParam == targetWindow)
                    {
                        StopScale();
                        selectedWindow = null;
                        windowTextBox.Text = "";
                    }

                    break;
            }
        }
        
        /// <summary>
        /// 更新をチェックする
        /// </summary>
        private void CheckUpdate()
        {
            // 更新すべきデータが既にあるか
            if (VersionMamager.IsUpdatable(out VersionInformation info))
            {
                string message = $"最新バージョンに更新しますか？"
                + $"\n\n[v{VersionMamager.CurrentVersion}] → [v{info.version}]"
                + "\n\n-更新内容-"
                + $"\n{info.comment}";

                string caption = "SandBurst更新";

                MessageBoxButtons buttons = MessageBoxButtons.YesNo;

                DialogResult result = MessageBox.Show(this, message, caption, buttons, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    VersionMamager.ExcuteUpdate();
                    Close();
                    return;
                }
            }

            // 前回のチェックから時間が経ってない場合はチェックしない
            if ((info != null) && (info.checkedTime.Day == DateTime.Now.Day))
                return;

            
            // 最新バージョンをサーバーに問い合わせる
            // 実際に更新を行うのは次回起動時
            VersionMamager.CheckUpdate();
        }

        private void MenuItemEdit_Click(object sender, EventArgs e)
        {
            EditSetting(currentSetting);
        }

        /// <summary>
        /// 画面比率を変更する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RatioBar_Scroll(object sender, EventArgs e)
        {
            ratioLabel.Text = $"{ratioBar.Value}%";

            if (currentSetting != null)
                currentSetting.Ratio = ratioBar.Value;
        }

        /// <summary>
        /// プロセスが管理者モードかチェックする
        /// </summary>
        /// <returns></returns>
        private bool IsAdministrator()
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            // 引数の中に実行ファイルパスが含まれていたら即起動
            string[] args = Environment.GetCommandLineArgs();
            
            if (args.Length < 2)
            {
                return;
            }

            string path = args[1];
            
            foreach(History history in histories.Items)
            {
                if (history.Path.Equals(path))
                {
                    ExcuteHistory(history);
                    return;
                }
            }
        }

        /// <summary>
        /// 履歴からプロセスを実行して補完する
        /// </summary>
        /// <param name="history"></param>
        private void ExcuteHistory(History history)
        {
            if (!System.IO.File.Exists(history.Path))
            {
                ErrorHelper.ShowErrorMessage($"実行ファイルが見つかりません\n{history.Path}");
                return;
            }

            if (!settingManager.Exists(history.SettingName))
            {
                ErrorHelper.ShowErrorMessage($"動作モードが見つかりません\n{history.SettingName}");
                return;
            }

            if (currentSetting != null)
                settingManager.SaveSetting(currentSetting);

            currentSetting = settingManager.LoadSetting(history.SettingName);
            UpdateUI(currentSetting);

            // プロセスを起動する
            System.Diagnostics.ProcessStartInfo processInfo = new System.Diagnostics.ProcessStartInfo();
            processInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(history.Path);
            processInfo.FileName = history.Path;
            processInfo.Arguments = history.Args;
            System.Diagnostics.Process process = System.Diagnostics.Process.Start(processInfo);

            if (process == null)
            {
                ErrorHelper.ShowErrorMessage($"プロセスの起動に失敗しました\n{history.Path}");
                return;
            }

            historyButton.Enabled = false;
            windowTextBox.Text = "起動中";

            WindowSelector selector = new WindowSelector(IgnoreFile, new IntPtr[] { Handle, shadowWindow.Window });

            Task task = Task.Run(() =>
            {
                Win32.POINT windowSize;
                windowSize.x = history.Width;
                windowSize.y = history.Height;
                Win32.POINT defDisplaySize = WindowHelper.GetDisplaySize();

                bool fullScreenCanceled = false;

                for (int i = 0; i < 40; i++)
                {
                    Invoke(
                       (MethodInvoker)delegate ()
                       {
                           windowTextBox.Text += ".";
                       }
                    );

                    WindowInformation info = selector.Find(history.Path, history.Title);

                    // タイトルとウィンドウクラスが同じ名前のウィンドウが見つかった?
                    if (info != null)
                    {

                        Win32.POINT wSize, cSize;
                        WindowHelper.GetWindowSize(info.Window, out wSize, out cSize);

                        // 履歴のウィンドウサイズと異なる?
                        if ((wSize.x != windowSize.x) || (wSize.y != windowSize.y))
                        {
                            Win32.POINT displaySize = WindowHelper.GetDisplaySize();

                            // ディスプレイサイズとウィンドウサイズが同じ かつ
                            // 初期ディスプレイサイズと現在のディスプレイサイズが異なる かつ
                            // まだフルスクリーンを解除していない場合
                            if (
                                (wSize.x == displaySize.x) && (wSize.y == displaySize.y)
                                && ((defDisplaySize.x != displaySize.x) || (defDisplaySize.y != displaySize.y))
                                && (fullScreenCanceled == false)
                            )
                            {

                                // フルスクリーンになっているので Alt Enter をOSに送ってフルスクリーンを解除する
                                System.Threading.Thread.Sleep(1000);

                                // Alt
                                Win32.API.keybd_event(0x12, (byte)Win32.API.MapVirtualKey(0x12, 0), 0, UIntPtr.Zero);

                                // Enter
                                Win32.API.keybd_event(0x0D, (byte)Win32.API.MapVirtualKey(0x0D, 0), 0, UIntPtr.Zero);

                                // Alt (KEYEVENTF_KEYUP)
                                Win32.API.keybd_event(0x12, (byte)Win32.API.MapVirtualKey(0x12, 0), 2, UIntPtr.Zero);

                                // Enter (KEYEVENTF_KEYUP)
                                Win32.API.keybd_event(0x0D, (byte)Win32.API.MapVirtualKey(0x0D, 0), 2, UIntPtr.Zero);

                                fullScreenCanceled = true;


                            }

                            System.Threading.Thread.Sleep(300);

                            continue;
                        }

                        Invoke(
                            (MethodInvoker)delegate ()
                            {
                                targetWindow = info.Window;
                                selectedWindow = info;
                                settingListBox.SelectedIndex = settingManager.GetIndex(history.SettingName);
                                windowTextBox.Text = history.Title;
                                historyButton.Enabled = true;

                                if (StartScale())
                                {
                                    AddHistory(selectedWindow);
                                }
                            }
                        );

                        return;
                    }

                    System.Threading.Thread.Sleep(300);
                }

                Invoke(
                    (MethodInvoker)delegate ()
                    {
                        windowTextBox.Text = "";
                        ErrorHelper.ShowErrorMessage("プロセスの捕獲に失敗しました");
                        historyButton.Enabled = true;
                    }
                );
            });
        }

        private Win32.RECT GetDisplay()
        {
            Screen screen = GetTargetScreen();
            Win32.RECT ret;
            ret.left = screen.Bounds.Left;
            ret.right = screen.Bounds.Width;
            ret.top = screen.Bounds.Top;
            ret.bottom = screen.Bounds.Height;

            return ret;
        }
    }
}
