using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Diagnostics;

namespace SandBurst
{
    delegate void SampleDelegate(VersionResponse version);

    class VersionMamager
    {
        private const string FilePath = MainForm.DataDirectory + "\\version.json";
        private const string BinPath = MainForm.DataDirectory + "\\Bin";
        private const string UpdaterPath = BinPath + "\\Updater.exe";

        public const string VersionUrl = "https://grevival.net/api/sandburst/version.php";

        public const string CurrentVersion = "2.7.0";

        private static bool IsUpdatable(string gotVersion)
        {
            string[] currentVers = CurrentVersion.Split('.');
            string[] gotVers = gotVersion.Split('.');
            
            string current = "";
            string got = "";

            for (int  i = 0; i < currentVers.Length; i++)
            {
                current += currentVers[i].PadLeft(2, '0');
                got += gotVers[i].PadLeft(2, '0');
            }

            int currentNo = int.Parse(current);
            int gotNo = int.Parse(got);

            return gotNo > currentNo;
        }

        public static bool IsUpdatable(out VersionInformation info)
        {
            info = VersionInformation.LoadFromFile(FilePath);
            if (info == null)
            {
                return false;
            }

            return IsUpdatable(info.version);
        }

        public static async void CheckUpdate()
        {
            string url = VersionUrl;
            var request = WebRequest.Create(url);

            VersionResponse ver;
            try
            {
                using (WebResponse response = await request.GetResponseAsync())
                {
                    using (var resStream = response.GetResponseStream())
                    {
                        var serializer = new DataContractJsonSerializer(typeof(VersionResponse));
                        ver = (VersionResponse)serializer.ReadObject(resStream);

                        VersionInformation info = new VersionInformation(ver);
                        info.SaveToFile(FilePath);
                    }
                }
            }
            catch (WebException)
            {
                // ネットワークエラーなら何もしなくていい
            }
        }

        /// <summary>
        /// アップデートプログラムを実行する
        /// このメソッドを実行したらアプリケーションを直ちに終了すること
        /// </summary>
        public static void ExcuteUpdate()
        {
            VersionInformation ver = VersionInformation.LoadFromFile(FilePath);
            
            string tempFile = Path.GetTempPath() + "SandBurstUpdater.exe";
            File.Copy(UpdaterPath, tempFile, true);


            string dir = System.AppDomain.CurrentDomain.BaseDirectory;
            dir = dir.Substring(0, dir.LastIndexOf('\\'));
            string args = $"\"{dir}\" \"{ver.url}\"";

            ProcessStartInfo psi = new ProcessStartInfo();           
            psi.FileName = Path.GetFileName(tempFile);
            psi.Arguments = args;
            psi.WorkingDirectory = Path.GetTempPath();
            Process.Start(psi);
        }
    }

    [DataContract]
    public class VersionResponse
    {
        [DataMember]
        public string version;

        [DataMember]
        public string comment;

        [DataMember]
        public string url;
    }

    [DataContract]
    public class VersionInformation : VersionResponse
    {
        [DataMember]
        public DateTime checkedTime;
        
        public VersionInformation(VersionResponse v)
        {
            this.version = v.version;
            this.comment = v.comment;
            this.url = v.url;
            this.checkedTime = new DateTime(DateTime.Now.Ticks);
        }

        public static VersionInformation LoadFromFile(string path)
        {
            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    var serializer = new DataContractJsonSerializer(typeof(VersionInformation));
                    VersionInformation version = (VersionInformation)serializer.ReadObject(stream);

                    return version;
                }
            }
            catch
            {
                return null;
            }
        }

        public void SaveToFile(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                var serializer = new DataContractJsonSerializer(typeof(VersionInformation));
                serializer.WriteObject(stream, this);
            }
        }
    }
}
