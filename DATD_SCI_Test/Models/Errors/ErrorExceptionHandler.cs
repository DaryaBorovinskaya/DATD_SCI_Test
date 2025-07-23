namespace DATD_SCI_Test.Models.Errors
{

    /// <summary>
    /// Операции для различных типов исключений
    /// </summary>
    public class ErrorExceptionHandler
    {

        /// <summary>
        /// Получение текстового описания ошибки (на русском языке)
        /// </summary>
        /// <param name="err"></param>
        /// <returns></returns>
        public static string GetErrorDescription(ExceptionEnum err)
        {
            string description = string.Empty;
            switch (err)
            {
                case ExceptionEnum.SysExc:
                    description = "Синтаксическая ошибка";
                    break; 
                
                case ExceptionEnum.IOExc:
                    description = "Ошибка ввода-вывода";
                    break;  
                
                case ExceptionEnum.ScktExc:
                    description = "Ошибка подключения";
                    break; 
                
                case ExceptionEnum.InvalOpExc:
                    description = "Некорректная операция";
                    break;
                
            }

            return description;
        }
        
    }
}
