using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Linq;

namespace TextEditor
{
    class Memento
    {
        public string Text { get; set; }
    }

    public interface IOriginator
    {
        object GetMemento();
        void SetMemento(object memento);
    }

    public class Caretaker
    {
        private object memento;

        public void SaveState(IOriginator originator)
        {
            memento = originator.GetMemento();
        }

        public void RestoreState(IOriginator originator)
        {
            originator.SetMemento(memento);
        }
    }

    [Serializable]
    class TextFile : IOriginator
    {
        public string Text { get; set; }

        public TextFile(string text)
        {
            Text = text;
        }

        object IOriginator.GetMemento()
        {
            return new Memento
            {
                Text = this.Text
            };
        }

        void IOriginator.SetMemento(object memento)
        {
            if (memento is Memento)
            {
                var mem = memento as Memento;
                Text = mem.Text;
            }
        }

        public void SerializeBinary(FileStream fs)
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, this);
            fs.Flush();
            fs.Close();
        }

        public void DeserializeBinary(FileStream fs)
        {
            BinaryFormatter bf = new BinaryFormatter();
            TextFile deserialized = (TextFile)bf.Deserialize(fs);
            Text = deserialized.Text;
            fs.Close();
        }

        public void SerializeXml(FileStream fs)
        {
            XmlSerializer xs = new XmlSerializer(this.GetType());
            xs.Serialize(fs, this);
            fs.Flush();
            fs.Close();
        }

        public void DeserializeXml(FileStream fs)
        {
            XmlSerializer xs = new XmlSerializer(this.GetType());
            TextFile deserialized = (TextFile)xs.Deserialize(fs);
            Text = deserialized.Text;
            fs.Close();
        }

        public override string ToString()
        {
            return Text;
        }
    }

    class KeywordSearch
    {
        int count = 0;
        public string Filepath { get; set; }
        public string Keyword { get; set; }

        public KeywordSearch(string filepath, string keyword)
        {
            Filepath = filepath;
            Keyword = keyword;
        }

        public void SearchResult()
        {
            var files =
                from search
                in Directory.GetFiles(Filepath)
                where File.ReadAllLines(search).Contains(Keyword)
                select search;

            foreach (var file in files)
            {
                Console.WriteLine(file);
                ++count;
            }

            Console.WriteLine("indexing finished! {0} files found", count);
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            
            Console.WriteLine("enter the name of the file you want to create or open: ");
            string filename = Console.ReadLine();

            FileStream fs = new FileStream(filename + ".txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);

            Console.WriteLine(
                "\nhere you go!" +
                "\nnew line commands: save, undo, exit\n\n");

            string input = "";
            using (StreamReader sr = new StreamReader(filename + ".txt"))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                    input += line + "\n";
                }
            }

            fs = new FileStream(filename + ".txt", FileMode.Append, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            Caretaker caretaker = new Caretaker();
            TextFile tf = new TextFile(input);

            while (true)
            {
                input = Console.ReadLine();

                if (input == "save")
                {
                    caretaker.SaveState(tf);
                    continue;
                }

                else if (input == "undo")
                {
                    caretaker.RestoreState(tf);
                    fs = new FileStream(filename + ".txt", FileMode.Truncate, FileAccess.Write);
                    sw = new StreamWriter(fs);
                    sw.WriteLine(tf.Text);
                    continue;
                }

                else if (input == "exit")
                {
                    sw.Close();
                    break;
                }

                tf.Text += input + "\n";
                sw.WriteLine(input);
            }

            Console.Clear();
            Console.WriteLine("Enter a keyword to search: ");
            string keyword = Console.ReadLine();
            string filepath = "/Volumes/Mac HDD/visualstudio/TextEditor/TextEditor/bin/Debug/netcoreapp3.1/";
            KeywordSearch ks = new KeywordSearch(filepath, keyword);
            ks.SearchResult();
        }
    }
}