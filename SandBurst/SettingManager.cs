using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace SandBurst
{
    class SettingManager
    {
        private IniFile iniFile;
        private IniFile defaultIniFile;

        public SettingManager(string fileName, string defaultFileName)
        {
            iniFile = new IniFile(fileName);
            defaultIniFile = new IniFile(defaultFileName);
        }

        /// <summary>
        /// 全ての設定名を取得する
        /// </summary>
        /// <returns></returns>
        public List<string> GetSettingNames()
        {
            return iniFile.GetSectionNames();
        }

        /// <summary>
        /// Settingを読み込む
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public CorrectionSetting LoadSetting(string name)
        {
            return LoadSetting(name, iniFile);
        }

        /// <summary>
        /// Settingを保存する
        /// </summary>
        /// <param name="setting"></param>
        public void SaveSetting(CorrectionSetting setting)
        {
            PropertyInfo[] infoArray = setting.GetType().GetProperties();

            foreach (PropertyInfo info in infoArray)
            {
                if (info.Name == "Name")
                    continue;

                if (info.PropertyType == typeof(bool))
                {
                    bool value = (bool)info.GetValue(setting, null);
                    iniFile.WriteBool(setting.Name, info.Name, value);
                }
                else if (info.PropertyType == typeof(int) || info.PropertyType == typeof(D3DFilter))
                {
                    int value = (int)info.GetValue(setting, null);
                    iniFile.WriteInt(setting.Name, info.Name, value);
                }
            }
        }

        /// <summary>
        /// DefaultSettingをロードする
        /// </summary>
        /// <returns></returns>
        public CorrectionSetting LoadDefaultSetting()
        {
            List<string> sections = defaultIniFile.GetSectionNames();
            return LoadSetting(sections[0], defaultIniFile);
        }

        /// <summary>
        /// Settingを削除する
        /// </summary>
        /// <param name="name"></param>
        public void DeleteSetting(string name)
        {
            iniFile.DeleteSection(name);
        }

        /// <summary>
        /// 同名のSettingがあるか判定する
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Exists(string name)
        {
            int index = GetIndex(name);

            return index >= 0;
        }
        
        /// <summary>
        /// nameから順番を取得する
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetIndex(string name)
        {
            List<string> names = GetSettingNames();

            for (int i = 0; i < names.Count; i++)
            {
                if (names[i] == name)
                    return i;
            }

            return -1;
        }

        private CorrectionSetting LoadSetting(string name, IniFile iniFile)
        {
            CorrectionSetting setting = new CorrectionSetting();

            setting.Name = name;

            PropertyInfo[] infoArray = setting.GetType().GetProperties();

            foreach (PropertyInfo info in infoArray)
            {
                if (info.Name == "Name")
                    continue;

                if (info.PropertyType == typeof(bool))
                {
                    var value = iniFile.ReadBool(name, info.Name, true);
                    info.SetValue(setting, value);
                }
                else if (info.PropertyType == typeof(int) || info.PropertyType == typeof(D3DFilter))
                {
                    int defValue = 0;
                    if (info.Name == "Ratio")
                    {
                        defValue = 90;
                    }
                    var value = iniFile.ReadInt(name, info.Name, defValue);
                    info.SetValue(setting, value);
                }
            }

            return setting;
        }
    }

    enum ScaleMode
    {
        Width,
        Magnification,
        Ratio
    };

    [Serializable()]
    public class CorrectionSetting
    {

        /// <summary>
        /// Section名
        /// </summary>
        public string Name { get; set; }

        // 以下はsettings.iniのkeyと同じ名前
        public bool WindowSize { get; set; }
        public bool ChildWindowSize { get; set; }
        public bool Thumbnail { get; set; }
        public bool D3D { get; set; }
        public bool ScreenShot { get; set; }
        public bool WmSize { get; set; }
        public bool WmWindowPos { get; set; }
        public bool GetWindowRect { get; set; }
        public bool GetClientRect { get; set; }
        public bool SetWindowPlacement { get; set; }
        public bool SetWindowPos { get; set; }
        public bool MoveWindow { get; set; }
        public bool ClipCursor { get; set; }
        public bool GetCursorPos { get; set; }
        public bool SetCursorPos { get; set; }
        public bool MsgHook { get; set; }
        public bool DWMMode { get; set; }
        public int Scale { get; set; }
        public bool HookType { get; set; }
        public D3DFilter Filter { get; set; }
        public int MagnificationMode { get; set; }
        public int Width { get; set; }
        public bool CentralizesWindow { get; set; }
        public bool ExcludesTaskbar { get; set; }
        public bool LimitsTaskbar { get; set; }
        public int Ratio { get; set; }
        public int DisplayIndex { get; set; }
    }
}
