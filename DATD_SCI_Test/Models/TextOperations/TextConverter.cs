using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATD_SCI_Test.Models.TextOperations
{
    /// <summary>
    /// Операции преобразования текста
    /// </summary>
    class TextConverter
    {
        /// <summary>
        /// Преобразование входных данных в единую строку</summary>
        /// <param name="text"></param>
        /// <param name="cause"></param>
        /// <param name="showTimeFlag"></param>
        /// <returns></returns>
        public string ConvertTextToPrint(string text, string cause, bool showTimeFlag)
        {
            string print = "";
            if (showTimeFlag)
            {
                print += $"[{DateTime.Now}] {cause}: {text}\r\n";
                return print;
            }
            print += $"{cause}: {text}\r\n";
            return print;
        }
    }
}
