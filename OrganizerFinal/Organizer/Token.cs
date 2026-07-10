using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Organizer
{
    internal class Token
    {
        #region Поля и свойства
            /// <summary>
            /// Поле для сохранения пути к файлу с токеном.
            /// </summary>
            private readonly string Path;
            #endregion

            #region Методы
            /// <summary>
            /// Метод по получению токена из файла.
            /// </summary>
            public string GetToken()
            {
                string Path = "token.txt";
                FileInfo fileInfo = new FileInfo(Path);
                string readerToken = "";

                try
                {
                    if (File.Exists(Path))
                    {
                        using (StreamReader reader = fileInfo.OpenText())
                        {
                            readerToken = reader.ReadLine();
                        }
                    }
                    else
                    {
                        Console.WriteLine("Файл не найден.");                        
                        
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при чтении файла: " + ex.Message);
                }

                return readerToken;
            }

            /// <summary>
            /// Метод по удалению файла с токеном.
            /// </summary>
            public void DeletFile()
            {
                File.Delete(Path);
            }
            #endregion
        }
    }

