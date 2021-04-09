using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SandBurst
{
    [XmlRoot(ElementName = "histories")]
    public class Histories
    {
        [XmlElement(ElementName = "history")]
        public List<History> Items { get; set; }

        [XmlIgnore]
        public int Capacity { get; set; }

        public Histories()
        {
            Capacity = 10;
        }

        public History Find(string path, string title)
        {
            foreach (History history in Items)
            {
                if ((path == history.Path) && (title == history.Title))
                {
                    return history;
                }
            }

            return null;
        }

        public static Histories LoadFromFile(string fileName)
        {
            return XmlHelper.Deserialize<Histories>(fileName);
        }

        public void SaveToFile(string fileName)
        {
            XmlHelper.Serialize(fileName, this);
        }

        public History Intert(string path, string title, string settingName, int width, int height, string args)
        {
            History history = Find(path, title);

            if (history != null)
            {
                Items.Remove(history);
                history.SettingName = settingName;
                history.Width = width;
                history.Height = height;
                history.Args = args;
            }
            else
            {
                history = new History()
                {
                    Path = path,
                    Title = title,
                    SettingName = settingName,
                    Width = width,
                    Height = height,
                    Args = args
                };
            }

            if (Items.Count == Capacity)
            {
                Items.RemoveAt(Items.Count - 1);
            }

            Items.Insert(0, history);

            return history;
        }
    }

    public class History
    {
        [XmlElement(ElementName = "path")]
        public string Path { get; set; }

        [XmlElement(ElementName = "title")]
        public string Title { get; set; }

        [XmlElement(ElementName = "settingName")]
        public string SettingName { get; set; }

        [XmlElement(ElementName = "width")]
        public int Width { get; set; }

        [XmlElement(ElementName = "height")]
        public int Height { get; set; }

        [XmlElement(ElementName = "args")]
        public string Args { get; set; }
    }
}
