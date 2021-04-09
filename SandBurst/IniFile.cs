using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace SandBurst
{
    /// <summary>
    /// IniFileの操作ラッパークラス
    /// </summary>
    class IniFile
    {
        public string FileName { get; private set; }

        public IniFile(string fileName)
        {
            if (fileName.Contains(":\\"))
            {
                // フルパス

                this.FileName = fileName;
            }
            else
            {
                // 相対パス

                // フルパスへ変換
                string dir = System.AppDomain.CurrentDomain.BaseDirectory;
                this.FileName = dir + fileName;
            }

            if (!File.Exists(this.FileName))
                throw new FileNotFoundException($"ファイルが見つかりません\n{this.FileName}");
            
        }

        /// <summary>
        /// IniFileに含まれる全てのセクションを取得する
        /// </summary>
        /// <returns></returns>
        public List<string> GetSectionNames()
        {
            byte[] buffer = new byte[1024];

            Win32.API.GetPrivateProfileSectionNames(buffer, (uint)buffer.Length, FileName);

            string allSections = System.Text.Encoding.Default.GetString(buffer);
            string[] sectionNames = allSections.Split('\0');

            List<string> result = new List<string>();

            foreach (string sectionName in sectionNames)
            {
                if (sectionName != string.Empty)
                    result.Add(sectionName);
            }

            return result;
        }

        /// <summary>
        /// IniFileからbool値を読み込む
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public bool ReadBool(string section, string key, bool defaultValue)
        {
            System.Text.StringBuilder valueText = new System.Text.StringBuilder(256);
            string defaultValueText = defaultValue ? "1" : "0";

            Win32.API.GetPrivateProfileString(section, key, defaultValueText, valueText, 256, FileName);

            int result;

            if (int.TryParse(valueText.ToString(), out result))
                return result != 0;
            else
                return defaultValue;
        }

        /// <summary>
        /// IniFileからInt値を読み込む
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public int ReadInt(string section, string key, int defaultValue)
        {
            System.Text.StringBuilder valueText = new System.Text.StringBuilder(256);
            string defaultValueText = defaultValue.ToString();

            Win32.API.GetPrivateProfileString(section, key, defaultValueText, valueText, 256, FileName);

            int result;

            if (int.TryParse(valueText.ToString(), out result))
                return result;
            else
                return defaultValue;
        }

        /// <summary>
        /// IniFileにbool値を書きこむ
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void WriteBool(string section, string key, bool value)
        {
            string valueText = value ? "1" : "0";

            Win32.API.WritePrivateProfileString(section, key, valueText, FileName);
        }

        /// <summary>
        /// IniFileにInt値を書き込む
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void WriteInt(string section, string key, int value)
        {
            Win32.API.WritePrivateProfileString(section, key, value.ToString(), FileName);
        }

        /// <summary>
        /// Sectionを削除する
        /// </summary>
        /// <param name="section"></param>
        public void DeleteSection(string section)
        {
            Win32.API.WritePrivateProfileSection(section, null, FileName);
        }
    }
}
