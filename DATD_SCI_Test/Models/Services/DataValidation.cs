using DATD_SCI_Test.Models.Errors;
using DATD_SCI_Test.Models.Tables;
using DATD_SCI_Test.Models.TextOperations;
using System.Collections.ObjectModel;
using System.Windows;

namespace DATD_SCI_Test.Models.Services
{
    /// <summary>
    /// Операции валидации данных
    /// </summary>
    public class DataValidation
    {
        private TextValidation _textValidation;
        public Action<string, string, MessageBoxImage> OnMessage;
        public Action<string, string> OnLog;
        public DataValidation()
        {
            _textValidation = new();
        }

        /// <summary>
        /// Валидация IP-адреса и порта
        /// </summary>
        /// <param name="dataIP"></param>
        /// <param name="dataPort"></param>
        /// <returns></returns>
        public bool ValidationDataIPAndDataPort(string dataIP, string dataPort)
        {

            if (string.IsNullOrEmpty(dataIP) && string.IsNullOrEmpty(dataPort))
            {
                OnMessage?.Invoke("Поля IP Адрес и Порт пустые, их необходимо заполнить", "Ошибка", MessageBoxImage.Error);
                return false;
            }

            else if (string.IsNullOrEmpty(dataIP))
            {
                OnMessage?.Invoke("Поле IP Адрес пустое, его необходимо заполнить", "Ошибка", MessageBoxImage.Error);
                return false;
            }

            else if (string.IsNullOrEmpty(dataPort))
            {
                OnMessage?.Invoke("Поле Порт пустое, его необходимо заполнить", "Ошибка", MessageBoxImage.Error);
                return false;
            }

            else
            {
                try
                {
                    string[] strIp = dataIP.Split('.');
                    if (strIp.Length != 4)
                    {
                        OnMessage?.Invoke("IP Адрес написан не по стандарту\n\rФормат записи: XXX.XXX.XXX.XXX или XXX.XXX.X.X", "Ошибка", MessageBoxImage.Error);

                        return false;
                    }
                    for (var i = 0; i <= 3; ++i)
                    {
                        if (strIp[i] == "")
                        {
                            OnMessage?.Invoke("IP Адрес написан не по стандарту\n\rФормат записи: XXX.XXX.XXX.XXX или XXX.XXX.X.X", "Ошибка", MessageBoxImage.Error);
                            return false;
                        }

                        if (!_textValidation.IsOnlyDigits(strIp[i]))
                        {
                            OnMessage?.Invoke("Поле IP Адрес содержит символы, не являющиеся цифрами или точкой", "Ошибка", MessageBoxImage.Error);
                            return false;
                        }


                        if ((Convert.ToInt32(strIp[i]) > 255 || (Convert.ToInt32(strIp[i]) < 0)))
                        {
                            OnMessage?.Invoke("Все числа в IP Адресе должны быть в диапазоне от 0 до 255", "Ошибка", MessageBoxImage.Error);
                            return false;
                        }
                    }
                }
                catch (Exception )
                {
                    OnLog?.Invoke(ErrorExceptionHandler.GetErrorDescription(ExceptionEnum.SysExc), "Ошибка");
                    return false;
                    //connection_log.AppendText(AdditionalFunctions.TextBoxPrint(AdditionalFunctions.ErrorExceptionHandler(errorCodes.SysExc, e1.ToString()).ToString(), "Код ошибки", _showTime));
                }

                if (!_textValidation.IsOnlyDigits(dataPort))
                {
                    OnMessage?.Invoke("Поле Порт содержит символы, не являющиеся цифрами", "Ошибка", MessageBoxImage.Error);
                    return false;
                }

                try
                {
                    if (Convert.ToInt64(dataPort) > 65535)
                    {
                        OnMessage?.Invoke("Порт макcимально может быть 65535", "Ошибка", MessageBoxImage.Error);
                        return false;
                    }
                }
                catch (Exception )
                {
                    OnLog?.Invoke(ErrorExceptionHandler.GetErrorDescription(ExceptionEnum.SysExc), "Ошибка");
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Валидация параметра Время отправки телеизмерений
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool ValidationDataBaseUnit(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                OnMessage?.Invoke("Поле Время отправки телеизмерений пустое, его необходимо заполнить", "Ошибка", MessageBoxImage.Error);
                return false;
            }

            else
            {
                if (!_textValidation.IsOnlyDigits(data))
                {
                    OnMessage?.Invoke("Поле Время отправки телеизмерений содержит символы, не являющиеся цифрами", "Ошибка", MessageBoxImage.Error);
                    return false;
                }

                try
                {
                    if (Convert.ToInt32(data) > 32767)
                    {
                        OnMessage?.Invoke("Время отправки телеизмерений макcимально может быть 32767", "Ошибка", MessageBoxImage.Error);
                        return false;
                    }
                }
                catch (Exception)
                {
                    OnLog?.Invoke("Время отправки телеизмерений макcимально может быть 32767", "Ошибка");
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Валидация данных из сервисной консоли
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool ValidationServiceConsole(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                OnMessage?.Invoke("Поле Сервисная консоль пустое, его необходимо заполнить", "Ошибка", MessageBoxImage.Error);
                return false;
            }

            else
            {
                if (!_textValidation.IsHexValid(data))
                {
                    OnMessage?.Invoke("Поле Сервисная консоль содержит символы, не относящиеся к 16 системе счисления", "Ошибка", MessageBoxImage.Error);
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Валидация отдельного значения из таблицы телеизмерений
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool ValidateTelemetryData(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                if (!_textValidation.IsOnlyDigits(data))
                {
                    OnMessage?.Invoke("Поле из таблицы Телеизмерений индикатора содержит символы, не являющиеся цифрами", "Ошибка", MessageBoxImage.Error);
                    return false;
                }

                try
                {
                    if (Convert.ToInt32(data) > 32767)
                    {
                        OnMessage?.Invoke("Значение поля из таблицы Телеизмерений индикатора макcимально может быть 32767", "Ошибка", MessageBoxImage.Error);
                        return false;
                    }
                }
                catch (Exception)
                {
                    OnLog?.Invoke("Значение поля из таблицы Телеизмерений индикатора макcимально может быть 32767", "Ошибка");
                    return false;
                }
                return true;
            }
            return true;
        }

        /// <summary>
        /// Валидация значений из таблицы телеизмерений
        /// </summary>
        /// <param name="phase"></param>
        /// <param name="telemetryDatas"></param>
        /// <returns></returns>
        public bool ValidationDataIndicator(PhaseEnum phase, 
            ObservableCollection<TelemetryData> telemetryDatas)
        {
            string phaseData = string.Empty;
            foreach (TelemetryData data in telemetryDatas)
            {
                phaseData = string.Empty;
                switch (phase)
                {
                    case PhaseEnum.PhaseA:
                        phaseData = data.PhaseA;
                        break;

                    case PhaseEnum.PhaseB:
                        phaseData = data.PhaseB;
                        break;

                    case PhaseEnum.PhaseC:
                        phaseData = data.PhaseC;
                        break;
                }

                if (!ValidateTelemetryData(phaseData))
                    return false;
                

            }

            return true;
        }

    }
}
