using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace SandBurst
{
    class Preference
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int MaxScale { get; set; }
        public int MinScale { get; set; }
        public int Scale1 { get; set; }
        public int Scale2 { get; set; }
        public int Scale3 { get; set; }
        public int Scale4 { get; set; }
        public int Scale5 { get; set; }
        public bool ScaleLimitation { get; set; }
        public bool AutoUpdate { get; set; }

        public Preference(string fileName)
        {
            IniFile iniFile = new IniFile(fileName);

            PropertyInfo[] infoArray = this.GetType().GetProperties();

            string name = "SandBurst";

            foreach (PropertyInfo info in infoArray)
            {

                if (info.PropertyType == typeof(bool))
                {
                    var value = iniFile.ReadBool(name, info.Name, true);
                    info.SetValue(this, value);
                }
                else if (info.PropertyType == typeof(int))
                {
                    var value = iniFile.ReadInt(name, info.Name, 0);
                    info.SetValue(this, value);
                }
            }
        }

        public void SaveToFile(string fileName)
        {
            IniFile iniFile = new IniFile(fileName);
            PropertyInfo[] infoArray = this.GetType().GetProperties();

            string name = "SandBurst";

            foreach (PropertyInfo info in infoArray)
            {
                if (info.PropertyType == typeof(bool))
                {
                    bool value = (bool)info.GetValue(this, null);
                    iniFile.WriteBool(name, info.Name, value);
                }
                else if (info.PropertyType == typeof(int))
                {
                    int value = (int)info.GetValue(this, null);
                    iniFile.WriteInt(name, info.Name, value);
                }
            }
        }
    }
    
}
