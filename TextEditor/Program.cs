using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

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
    
    class Program
    {
        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.DarkGreen;

            Console.WriteLine("enter name of the file you want to create or open: ");
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
        
        }
    }
}