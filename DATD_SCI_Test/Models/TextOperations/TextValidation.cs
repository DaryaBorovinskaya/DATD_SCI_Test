using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATD_SCI_Test.Models.TextOperations
{
    /// <summary>
    /// Операции валидации текста
    /// </summary>
    public class TextValidation
    {
        /// <summary>
        /// Проверка текста на наличие только цифр
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public bool IsOnlyDigits(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (!char.IsDigit(text[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Проверка текста на наличие только цифр и букв, характерных для 16 системы счисления (A-F)
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public bool IsHexValid(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (!char.IsDigit(text[i]) && text[i] != 'A' && text[i] != 'B' && text[i] != 'C'
                    && text[i] != 'D' && text[i] != 'E' && text[i] != 'F' && text[i] != ' ')
                {
                    return false;
                }
            }
            return true;
        }
    }
}
