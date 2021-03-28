using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace TextEditor
{
    class Memento
    {
        public string Name { get; set; }
        public string Grade { get; set; }
        public string Age { get; set; }
        public string Interests { get; set; }
    }
    public interface IOriginator
    {
        object GetMemento();
        void SetMemento(object memento);
    }

    [Serializable]
    class StudentInfo : IOriginator
    {
        public string Name { get; set; }
        public string Grade { get; set; }
        public string Age { get; set; }
        public string Interests { get; set; }
        public StudentInfo(string name, string grade, string age, string interests)
        {
            Name = name; Grade = grade; Age = age; Interests = interests;
        }
        object IOriginator.GetMemento()
        {
            return new Memento
            {
                Name = this.Name,
                Grade = this.Grade,
                Age = this.Age,
                Interests = this.Interests
            };
        }
        void IOriginator.SetMemento(object memento)
        {
            if (memento is Memento)
            {
                var mem = memento as Memento;
                Name = mem.Name;
                Grade = mem.Grade;
                Age = mem.Age;
                Interests = mem.Interests;
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
            StudentInfo deserialized = (StudentInfo)bf.Deserialize(fs);
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
            StudentInfo deserialized = (StudentInfo)xs.Deserialize(fs);
            fs.Close();
        }
        public void Print()
        {
            Console.WriteLine("name={0} grade={1} age={2} interests={3}", Name, Grade, Age, Interests);
        }
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

    class Program
    {
        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            StudentInfo student = new StudentInfo("Will", "12", "18", "Music");
            Console.WriteLine("creating a new file!...\nwrite \"stop\" on the new line to stop writing into the file");

            Random random = new Random();
            string filename = Convert.ToString(random.Next());
            FileStream fs = new FileStream(filename + ".txt", FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            string input;
            while (true)
            {
                input = Console.ReadLine();
                if (input == "stop")
                {
                    sw.Close();
                    break;
                }
                sw.WriteLine(input);
            }

        }
    }
}