using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using static BusinessNotes.UserException;
using System.Xml;
using static System.Net.WebRequestMethods;
using static System.Net.Mime.MediaTypeNames;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BusinessNotes
{
    /// <summary>
    /// Менеджер заметок.
    /// </summary>
    public class BusinessNotesManager
    {
        #region Поля и свойства
        /// <summary>
        /// Путь к базе данных.
        /// </summary>
        private const string Path = "notes.txt";  
        
        /// <summary>
        /// Список заметок.
        /// </summary>
        public static List<Note> Notes = new List<Note>();        
        #endregion

        #region Методы
        /// <summary>
        /// Удаляет заметку, если она раньше текущей даты.
        /// </summary>
        /// <param name="note">Заметка.</param>
        /// <returns>true в случае усеха, иначе false.</returns>
        public static bool DispleyDateСheck(Note note)
        {
            int result = DateTime.Compare(DateTime.Today, note.DisplayDate);
            if (result == 0)
            {
                
                return true;
            }
            else 
            {
                return false;
            }            
        }

        /// <summary>
        /// Создает список. 
        /// </summary>
        /// <param name="all">Определяет нужны ли все заметки или только на текущую дату.</param>
        /// <returns>Список заметок.</returns>
        public List<Note> ListCreate(bool all=false)
        {
            List<Note> myNotes = new List<Note>();            
            foreach (Note note in Notes)
            {
                if (!all) 
                {
                    if (DispleyDateСheck(note))
                    {
                        myNotes.Add(note);
                    }
                }
                else
                {
                    myNotes.Add(note);
                }  
            }            
            return myNotes;
        }
             
        /// <summary>
        /// Добавляет заметку в коллекцию.
        /// </summary>
        /// <param name="note">Заметка.</param>
        /// <returns>true при успешном добавлении, false в случае неудачи.</returns>
        public bool Add(Note note)
        {
            int count = Notes.Count();
            Notes.Add(note);
            if ((count+1==Notes.Count()) && (WriteDown()))
            {
                return true;
            }
            else
            {
                return false;
            }                       
        }

        /// <summary>
        /// Поиск заметки по Id.
        /// </summary>
        /// <param name="id">Id заметки.</param>
        /// <returns>Заметку с заданным Id.</returns>
        /// <exception cref="EmployeeNotFound">Возникает, когда заметка с заданным Id не найдена.</exception>
        public Note Search(int id)
        {
            Note searchTask = Notes.Find(item => item.Id == id);
            if (searchTask == null)
            {
                throw new EmployeeNotFound("Заметка с таким Id не найдена");
            }
            return searchTask;
        }

        /// <summary>
        /// Редактирует заметку.
        /// </summary>
        /// <param name="note"></param>
        /// <param name="parametrs"></param>
        public void Update(Note note,Dictionary<string,string> parametrs)
        {
            note.Description = (parametrs["Description"]!=string.Empty)? parametrs["Description"]: note.Description;            
            note.DisplayDate = (parametrs["DisplayDate"] != string.Empty) ? Convert.ToDateTime(parametrs["DisplayDate"]) : note.DisplayDate;                  
            WriteDown();            
        }

        /// <summary>
        /// Удаляет заметку из коллекции.
        /// </summary>
        /// <param name="note">Заметка.</param>
        /// <returns>true при успешном добавлении, false в случае неудачи.</returns>
        public bool Delete(Note note)
        {
            int count = Notes.Count();
            Notes.Remove(note);
            if ((count - 1 == Notes.Count()) && (WriteDown()))
            {
                return true;
            }
            else
            {
                return false;
            }            
            
        }      
        
        /// <summary>
        /// Записывает данные.
        /// </summary>
        /// <returns>true в случае усешной записи, иначе false</returns>
        public bool WriteDown()
        {
            using StreamWriter sw = System.IO.File.CreateText(Path);
            int count=0;
            foreach (var item in Notes)
            {                
                var displayDate = item.DisplayDate.ToString("d MM yyyy");
                var description = item.Description.Replace("\n", "&~&");
                sw.WriteLine($"Id:{item.Id},DisplayDate:{displayDate},UserId:{item.UserId},Description:{description}");
                count++;
            }
            if (count == Notes.Count())
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }

        /// <summary>
        /// Считывает данные.
        /// </summary>
        /// <returns>Список заметок.</returns>
        public static bool Read()
        {
            if (!System.IO.File.Exists(Path)) return false;
            string []lines = System.IO.File.ReadAllLines(Path);
            
            Dictionary<string,string> myDictionary = new Dictionary<string,string>();
            foreach (string line in lines)
            {                
                string str = line.Trim();
                for (int i = 0; i < Note.CountField-1; i++)
                {                    
                    int indexOfChar = str.IndexOf(',');                    
                    string data = str.Substring(0, indexOfChar);                    
                    string[] item = data.Split(":");                   
                    str=str.Remove(0, data.Length+1);                    
                    myDictionary.Add(item[0].Trim(), item[1].Trim());
                }
                int index = str.IndexOf(':');
                string str1 = str.Substring(0, index);
                string str2 = str.Remove(0, str1.Length + 1);
                str2 = str2.Replace("&~&","\n");                
                myDictionary.Add(str1, str2);
                Note note = new Note(myDictionary["Description"], DateTime.Parse(myDictionary["DisplayDate"]));
                note.Id = Int32.Parse(myDictionary["Id"]);
                note.UserId = Convert.ToInt64(myDictionary["UserId"]);
                Notes.Add(note);
                myDictionary.Clear();

            } 
            return true;
        }
        #endregion

        #region Конструктор
        public BusinessNotesManager()
        {
            Read();
        }
        #endregion
    }

}

