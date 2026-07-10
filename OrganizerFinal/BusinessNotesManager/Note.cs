using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessNotes
{
    /// <summary>
    /// Заметка.
    /// </summary>
    public class Note
    {
        #region Поля и свойства
        /// <summary>
        /// Id заметки.
        /// </summary>
        public int Id { get; set; }  
        
        /// <summary>
        /// Строка для генерирования следующего Id.
        /// </summary>
        public static int nextId;   
        
        /// <summary>
        /// Дата показа заметки.
        /// </summary>
        public DateTime DisplayDate { get; set; }

        /// <summary>
        /// Id пользователя чата.
        /// </summary>
        public long UserId;

        /// <summary>
        /// Текст заметки.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Количество полей класса.
        /// </summary>
        public static int CountField = 4;
        #endregion;

        #region Конструктор
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="displayDate"></param>
        public Note(string description,DateTime displayDate)
        {
            Id = Interlocked.Increment(ref nextId);
            this.Description = description;  
            this.DisplayDate = displayDate;
        }
        #endregion
    }
}
