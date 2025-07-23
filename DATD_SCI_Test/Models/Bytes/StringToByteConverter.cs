using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATD_SCI_Test.Models.Bytes
{
    /// <summary>
    /// Операции преобразования строки в массив байт
    /// </summary>
    public class StringToByteConverter
    {
        /// <summary>
        /// Преобразование строки в массив байт
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] StringToByteArray(string hex)
        {
            string[] flex = hex.Split(' ');
            byte[] bytes = new byte[flex.Length];
            for (var i = 0; i < flex.Length; i++)
            {
                //flex = hex.Substring(i, 2);
                bytes[i] = Convert.ToByte(flex[i], 16);
            }
            return bytes;
        }
    }
}
