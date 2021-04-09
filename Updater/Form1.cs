using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO.Compression;
using System.IO;
#if DEBUG
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
#endif

namespace Updater
{
    /// <summary>
    /// SandBurstの自動更新プログラム
    ///
    /// コマンドライン
    /// arg1: SandBurstのルートディレクトリ
    /// arg2: 更新ファイルのURL
    /// </summary>
    public partial class Form1 : Form
    {
#if DEBUG
        [DllImport("kernel32.dll",
            EntryPoint = "GetStdHandle",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll",
            EntryPoint = "AllocConsole",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern int AllocConsole();

        [DllImport("kernel32.dll", EntryPoint = "OutputDebugStringW")]
        public static extern void OutputDebugString(string message);

        [DllImport("kernel32.dll", EntryPoint = "DebugBreak")]
        public static extern void DebugBreak();

        private const int STD_OUTPUT_HANDLE = -11;
        private const int MY_CODE_PAGE = 437;
        private static TextWriter originalOutput;
        private static StreamWriter standardOutput;
#endif
        private WebClient client;
        private string zipPath;
        private string tempPath = System.IO.Path.GetTempPath();
        private string extractPath = System.IO.Path.GetTempPath() + "SandBurst";
        private string backupPath = System.IO.Path.GetTempPath() + "SandBurst_backup";
        private string exePath;
        private string rootPath;

        private const string UserPath = "Data\\User";

        public Form1()
        {
            InitializeComponent();

#if DEBUG
            AllocConsole();
            IntPtr stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            SafeFileHandle safeFileHandle = new SafeFileHandle(stdHandle, true);
            FileStream fileStream = new FileStream(safeFileHandle, FileAccess.Write);
            Encoding encoding = System.Text.Encoding.GetEncoding(MY_CODE_PAGE);
            standardOutput = new StreamWriter(fileStream, encoding);
            standardOutput.AutoFlush = true;
            originalOutput = Console.Out;
#endif
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            client = new WebClient();
            string[] args = Environment.GetCommandLineArgs();


            if (args.Length < 3)
            {
                Application.Exit();
                return;
            }

            if (!Directory.Exists(args[1]))
            {
                Application.Exit();
                return;
            }

            if (!args[2].StartsWith("http"))
            {
                Application.Exit();
                return;
            }

            string url = args[2];
            string tempPath = System.IO.Path.GetTempPath();
            rootPath = args[1];
            zipPath = tempPath + "SandBurst.zip";
            exePath = args[0];

#if NO_DOWNLOAD
            UpdateFiles();
#else
            Task.Run(() =>
            {
                // SandBurst本体が終了する時間を与える
                System.Threading.Thread.Sleep(1000);

                client.DownloadProgressChanged += DownloadProgressChanged;
                client.DownloadFileCompleted += DownloadComplete;
                client.DownloadFileAsync(new Uri(url), zipPath);
            });
#endif
        }


        public static void DebugPrint(object message)
        {
#if DEBUG
            Console.SetOut(standardOutput);
            Console.WriteLine(message);
            Console.SetOut(originalOutput);
            OutputDebugString(message.ToString());
#endif
        }


        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Invoke((MethodInvoker)delegate ()
            {
                label1.Text = "ダウンロード中";
                label2.Text = $"{e.ProgressPercentage}%  {e.BytesReceived}/{e.TotalBytesToReceive}byte";

                progressBar1.Value = e.ProgressPercentage;
            });
        }

        private void DownloadComplete(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Invoke((MethodInvoker)delegate ()
            {
                
                if (e.Error != null)
                {
                    MessageBox.Show($"エラーが発生しました\n\n{e.Error.Message}", "エラー");
                    return;
                }

                UpdateFiles();
            });
        }

        private void UpdateFiles()
        {
            try
            {
                label1.Text = "ファイルをコピーしています";
                label2.Text = "";
                progressBar1.Value = progressBar1.Maximum;

                // Zipファイルを解凍する
                Delete(extractPath);
                ZipFile.ExtractToDirectory(zipPath, extractPath);


                // 全データを本体にコピー
                string exRoot = extractPath + "\\SandBurst\\";
                IEnumerable<string> files = Directory.EnumerateFiles(exRoot, "*", SearchOption.AllDirectories);
                IEnumerable<string> folders = Directory.EnumerateDirectories(exRoot, "*", SearchOption.AllDirectories);

                // フォルダを作成
                DebugPrint("Create Directories");
                foreach (string folder in folders)
                {
                    string node = folder.Replace(exRoot, "");
                    string destPath = rootPath + "\\" + node;

                    DebugPrint(destPath);
                    SafeCreateDirectory(destPath);
                }

                // ファイルコピー
                DebugPrint("Copy Files");
                foreach (string name in files)
                {
                    string node = name.Replace(exRoot, "");
                    string destPath = rootPath + "\\" + node;

                    //Userフォルダ以外をコピー
                    if (!node.StartsWith(UserPath))
                    {
                        DebugPrint(node);
                        File.Copy(name, destPath, true);
                    }
                }

#if NO_DOWNLOAD
                //Tempデータの削除
                Delete(extractPath);
#else
                //Tempデータの削除
                File.Delete(zipPath);
                Delete(extractPath);
#endif
                MessageBox.Show("アップデートが完了しました", "SandBurst");

                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = rootPath + "\\SandBurst.exe";
                p.StartInfo.WorkingDirectory = rootPath;
                p.Start();

                Close();
            }
            catch(Exception e)
            {
                MessageBox.Show($"エラーが発生しました\n\n{e.Message}", "エラー");
                return;
            }
            
        }

        public static DirectoryInfo SafeCreateDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                return null;
            }
            return Directory.CreateDirectory(path);
        }

        public static void Delete(string targetDirectoryPath)
        {
            if (!Directory.Exists(targetDirectoryPath))
            {
                return;
            }

            //ディレクトリ以外の全ファイルを削除
            string[] filePaths = Directory.GetFiles(targetDirectoryPath);
            foreach (string filePath in filePaths)
            {
                File.SetAttributes(filePath, FileAttributes.Normal);
                File.Delete(filePath);
            }

            //ディレクトリの中のディレクトリも再帰的に削除
            string[] directoryPaths = Directory.GetDirectories(targetDirectoryPath);
            foreach (string directoryPath in directoryPaths)
            {
                Delete(directoryPath);
            }

            //中が空になったらディレクトリ自身も削除
            Directory.Delete(targetDirectoryPath, false);
        }


    }
}