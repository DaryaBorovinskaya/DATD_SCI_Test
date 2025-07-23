using DATD_SCI_Test.Models.Bytes;
using DATD_SCI_Test.Models.Errors;
using DATD_SCI_Test.Models.TimerAndProgress;
using System.Net;
using System.Net.Sockets;
using System.Windows;

namespace DATD_SCI_Test.Models.Services
{
    /// <summary>
    /// Операции, связанные только с УСПД 
    /// </summary>
    public class BaseUnit
    {
        private AsyncOperation _asyncOperation;

        public ExceptionEnum ConnectionState { get; set; }
        public bool IsSuccessCompletionTCPconnection {  get; set; }
        public bool IsConnect {  get; set; }
        public IPAddress BaseUnitIP {  get; set; }
        
        public Action OnStartBlocking;
        public Action OnStopBlocking;
        public Action<Exception, ExceptionEnum> OnException;
        public Action<ExceptionEnum> OnResultConnection;
        public Action<string, string> OnLog;
        public Action OnCompletionTCPconnection;
        public Action<string> OnGetDataBaseUnit;
        public Action<ExceptionEnum> OnConnectionState;
        public Action<string, string, MessageBoxImage> OnMessage;
        public Action OnUpdateIsConnect;
        public Action OnUpdateBaseUnitClient;
        public Action OnUpdateBaseUnitStream;
        public Action<double> OnReceiveProgress;


        public BaseUnit()
        {
            IsConnect = false;
            IsSuccessCompletionTCPconnection = false;

            _asyncOperation = new();
            _asyncOperation.OnLog += LogHandler;
        }

        /// <summary>
        /// Обработчик события логирования
        /// </summary>
        /// <param name="log"></param>
        /// <param name="cause"></param>
        private void LogHandler(string log, string cause) =>  OnLog?.Invoke(log, cause);

        /// <summary>
        /// Обработчик события прекращения блокировки
        /// </summary>
        private void StopBlockingHandler() => OnStopBlocking?.Invoke();

        /// <summary>
        /// Обработчик события обновления прогресса выполнения операции
        /// </summary>
        /// <param name="value"></param>
        private void ReceiveProgressHandler(double value) => OnReceiveProgress?.Invoke(value);


        /// <summary>
        /// Установление TCP-соединения с УСПД
        /// </summary>
        /// <param name="dataIP"></param>
        /// <param name="dataPort"></param>
        public async void SetTCPconnectionAsync( string dataIP, string dataPort)
        {
            IsSuccessCompletionTCPconnection = false;

            // При повторной попытке нажать на кнопку подключения
            if (IsConnect == true)
            {
                OnLog?.Invoke("Соединение установлено", "Статус подключения");
                return;
            }

            BaseUnitIP = IPAddress.Parse(dataIP);

            double timeout = 5.0;

            TimerWorker timerWorker = new();
            timerWorker.OnLog += LogHandler;
            timerWorker.OnStopBlocking += StopBlockingHandler;
            timerWorker.OnReceiveProgress += ReceiveProgressHandler;

            try
            {
                OnStartBlocking?.Invoke();

                // Запускаем подключение асинхронно
               timerWorker.AsyncTask = BaseUnitConnection.ConnectAsync(BaseUnitIP, Convert.ToInt32(dataPort));
                await timerWorker.AsyncTask;

                // Создаем задачу таймаута (по сути - таймер, обратный отсчет)
                timerWorker.TimeoutTask = Task.Delay(TimeSpan.FromSeconds(timeout));

                // Ожидаем завершения любой из задач
                timerWorker.CompletedTask = await Task.WhenAny(timerWorker.AsyncTask, timerWorker.TimeoutTask);

                if (timerWorker.CompletedTask == timerWorker.TimeoutTask)
                {
                    // Таймаут сработал
                    BaseUnitConnection.DisposeClient();
                    OnLog?.Invoke($"Превышено время ожидания подключения ({timeout} сек)", "Ошибка");
                    return;
                }

                OnStopBlocking?.Invoke();

                // Успешное подключение
                OnResultConnection?.Invoke(ExceptionEnum.None);
                IsConnect = true;


                #region Проверка установленного соединения с помощью тестового запроса на УСПД
                byte[] dataResponse = new byte[256];
                byte[] initBaseUnitBytes = BytePackage.GetBytePackage(BytePackageEnum.InitBaseUnit);

                await _asyncOperation.ExecuteOperationWriteAsync(initBaseUnitBytes, 0,
                    initBaseUnitBytes.Length, timeout);

                int bytesRead = await _asyncOperation.ExecuteOperationReadAsync(dataResponse, 0, dataResponse.Length, timeout);

                if (bytesRead <= 0)
                {
                    OnLog?.Invoke(bytesRead == 0 ? "Нет данных" : "Ошибка чтения", "Ошибка");
                    return;
                }
                #endregion
            }
            catch (SocketException)
            {
                OnLog?.Invoke(ErrorExceptionHandler.GetErrorDescription(ExceptionEnum.ScktExc), "Ошибка");
            }
            catch (Exception)
            {
                OnLog?.Invoke("Подключение не установлено", "Ошибка");
            }
            finally
            {
                OnStopBlocking?.Invoke();
            }
        }


        /// <summary>
        /// Завершение TCP-соединения с УСПД
        /// </summary>
        public void CompletionTCPconnection()
        {
            // Если подключение не установлено
            if (IsConnect == false)
            {
                OnLog?.Invoke("Соединение не установлено", "Статус подключения");

                IsConnect = false;
                return;
            }

            try
            {
                BaseUnitConnection.GetStream().Close();

                IsSuccessCompletionTCPconnection = true;
                OnCompletionTCPconnection?.Invoke();
                OnLog?.Invoke("Отключено", "Статус подключения");
                IsConnect = false;
            }
            catch (System.InvalidOperationException)
            {
                OnResultConnection?.Invoke(ExceptionEnum.InvalOpExc);
                OnLog?.Invoke(ErrorExceptionHandler.GetErrorDescription(ExceptionEnum.InvalOpExc), "Ошибка");
            }
        }



        /// <summary>
        /// Обработка прочитанных с УСПД данных
        /// </summary>
        /// <param name="data"></param>
        private async Task ProcessResponseAsync(byte[] data)
        {
            // Пропускаем S-форматы (6 байт)
            if (data.Length == 6 && data[0] == 0x68 && data[1] == 0x04)
                return;

            for (int i = 19; i < data[18]; ++i)
            {
                if (data[i] == 0x00 && data[i - 1] == 0x15)
                {
                    string value = (data[i + 4] << 8 | data[i + 3]).ToString();

                    OnGetDataBaseUnit?.Invoke(value);
                }
            }

        }

        /// <summary>
        ///  Проверка на валидность APDU (единица данных в протоколе IEC 60870-5-104 (МЭК 104), 
        /// которая передаётся между клиентом (программой) и сервером (УСПД)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool IsValidAPDU(byte[] data)
        {
            if (data.Length < 6) return false;
            if (data[0] != 0x68) return false; // Стартовый байт МЭК 104
            int apduLength = data[1];
            return data.Length >= apduLength + 2; // +2 байта (старт и длина)
        }

        /// <summary>
        /// Чтение данных с УСПД (Время отправки телеизмерений)
        /// </summary>
        /// <returns></returns>
        public async Task ReadDataBaseUnitAsync()
        {
            if (IsConnect == false)
            {
                OnLog?.Invoke("Соединение не установлено", "Статус подключения");
                OnGetDataBaseUnit?.Invoke(string.Empty);
                return;
            }

            string[] str;
            int sizeStr;
            string data = string.Empty;
            bool isSuccessWrite = false;
            byte[] dataResponse = new byte[256];
            double timeout = 5.0;
            try
            {
                OnStartBlocking?.Invoke();

                //В МЭК 104 клиент инициирует запрос(C_RD_NA_1, C_IC_NA_1 и т.д.).
                //Сервер(УСПД) отвечает только после запроса.
                //Если не отправить запрос — УСПД может ничего не прислать при дальнейшем чтении(или только служебные S / U - пакеты).
                byte[] readParamsBytes = BytePackage.GetBytePackage(BytePackageEnum.ReadParamsBaseUnit);
                await _asyncOperation.ExecuteOperationWriteAsync(readParamsBytes, 0, readParamsBytes.Length, timeout);

                await _asyncOperation.ExecuteOperationReadAsync(dataResponse, 0, dataResponse.Length, timeout);

                #region Опрос УСПД несколькими запросами подряд (раньше всё ок было, сейчас хз почему не работает)
                //byte[] buffer = new byte[256];
                //MemoryStream responseStream = new();
                //bool dataReceived = false;
                //DateTime timeout = DateTime.Now.AddSeconds(5); // Максимальное время ожидания
                //int bytesRead = 0;
                //const int maxAttempts = 3;
                //int attempts = 0;

                //while (DateTime.Now < timeout && !dataReceived && attempts < maxAttempts)
                //{
                //    attempts++;
                //    bytesRead = await _asyncOperation.ExecuteOperationRead(buffer, 0, buffer.Length, 1);
                //    if (bytesRead > 0)
                //    {
                //        await _asyncOperation.ExecuteOperationWrite(buffer, 0, bytesRead, 1);

                //        byte[] fullResponse = responseStream.ToArray();

                //        // Проверяем, это данные (I-формат) или служебный пакет
                //        if (IsValidAPDU(fullResponse) && fullResponse.Length >= 20)
                //        {
                //            // Это нужные нам данные
                //            await ProcessResponse(fullResponse);
                //            dataReceived = true;
                //        }
                //        else if (IsValidAPDU(fullResponse))
                //        {
                //            // Это служебный пакет - пропускаем
                //            responseStream.SetLength(0); // Очищаем для следующего пакета
                //            continue;
                //        }
                //    }
                //    await Task.Delay(50); // Короткая пауза между попытками
                //}

                //if (!dataReceived)
                //{
                //    OnLog?.Invoke("Не удалось получить данные", "Ошибка");
                //    OnStopBlocking?.Invoke();
                //    return;
                //}
                //dataResponse = responseStream.ToArray();

                ////Преобразование байт типа 0х68(пример) в вид 68 - для удобства работы
                //str = BitConverter.ToString(dataResponse, 0, bytesRead).Split('-');

                //// Копирование последние 9 байт из tempDataResponse в confirmArray 
                //Array.ConstrainedCopy(dataResponse, dataResponse.GetUpperBound(0) - 8, confirmArray, 0, 9);
                //sizeStr = bytesRead;
                //Array.Resize(ref dataResponse, sizeStr + 2);
                #endregion

                //Преобразование байт типа 0х68(пример) в вид 68 - для удобства работы
                str = BitConverter.ToString(dataResponse, 0, dataResponse.Length).Split('-');

                //Количество полученных байт (написано)
                sizeStr = Convert.ToInt16(str[1], 16);
                Array.Resize(ref dataResponse, sizeStr + 2);

                if (sizeStr < 6) // Минимальный APDU
                    OnLog?.Invoke("Короткий пакет", "Предупреждение");

                OnStopBlocking?.Invoke();

                // Обработка данных (асинхронно, чтобы не блокировать UI)
                await Task.Run(() => ProcessResponseAsync(dataResponse));
            }
            catch (System.IO.IOException)
            {
                OnStopBlocking?.Invoke();
                OnConnectionState?.Invoke(ExceptionEnum.IOExc);
                OnLog?.Invoke(ErrorExceptionHandler.GetErrorDescription(ExceptionEnum.IOExc), "Ошибка");
                return;
            }
            OnLog?.Invoke($"Время отправки телеизмерений прочитано", "Чтение данных с УСПД");

            OnStopBlocking?.Invoke();
            await BaseUnitConnection.GetStream().FlushAsync();
        }

        /// <summary>
        /// Запись данных в УСПД (Время отправки телеизмерений)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async void WriteDataBaseUnitAsync(string data)
        {
            if (IsConnect == false)
            {
                OnLog?.Invoke("Соединенине не установлено", "Статус подключения");
                return;
            }

            OnStartBlocking?.Invoke();

            //Стандартный пакет для отправки на УСПД
            byte[] defaultWriteBytes = BytePackage.GetBytePackage(BytePackageEnum.DefaultWriteBaseUnit);

            //Добавление в пакет введённых данных
            defaultWriteBytes[72] = (byte)(Convert.ToInt16(data) >> 8);
            defaultWriteBytes[71] = (byte)(Convert.ToInt16(data) & 0xFF);

            byte[] dataResponse = new byte[256];
            double timeout = 5.0;

            await _asyncOperation.ExecuteOperationWriteAsync(defaultWriteBytes, 0, defaultWriteBytes.Length, timeout);

            int bytesRead = await _asyncOperation.ExecuteOperationReadAsync(dataResponse, 0, dataResponse.Length, timeout);

            if (bytesRead <= 0)
            {
                OnLog?.Invoke(bytesRead == 0 ? "Нет данных" : "Ошибка чтения", "Ошибка");
                return;
            }

            
            OnLog?.Invoke("Время отправки телеизмерений изменено", "Запись данных в УСПД");
            

            OnStopBlocking?.Invoke();
            await BaseUnitConnection.GetStream().FlushAsync();
            await Task.Delay(100); // Пауза после записи перед чтением
        }

        /// <summary>
        /// Отправка данных на УСПД.
        /// Данные отправляются по TCP на роутер и происходит проброс по RS-232.
        /// Данные отправляются только в HEX и с пробелами,
        /// пример: 68 04 03 00 00 00.
        /// При любом другом формате отправки возникнет ошибка.
        /// </summary>
        /// <param name="data"></param>
        public async void SendDataBaseUnitAsync(string data)
        {
            if (IsConnect == false)
            {
                OnLog?.Invoke("Соединенине не установлено", "Статус подключения");
                return;
            }
            byte[] dataResponse = new byte[256];
            double timeout = 5.0;
            try
            {
                OnStartBlocking?.Invoke();

                byte[] dataSend = StringToByteConverter.StringToByteArray(data);

                await _asyncOperation.ExecuteOperationWriteAsync(dataSend, 0, dataSend.Length, timeout);


                #region Чтение данных (опционально)
                //int bytesRead = await _asyncOperation.ExecuteOperationRead(dataResponse, 0, dataResponse.Length, timeout);

                //if (bytesRead <= 0)
                //{
                //    OnLog?.Invoke(bytesRead == 0 ? "Нет данных" : "Ошибка чтения", "Ошибка");
                //    OnStopBlocking.Invoke();
                //    return;
                //}
                #endregion
            }
            catch (System.IO.IOException)
            {
                OnConnectionState?.Invoke(ExceptionEnum.IOExc);
                OnLog?.Invoke(ErrorExceptionHandler.GetErrorDescription(ExceptionEnum.IOExc), "Ошибка");
                OnStopBlocking?.Invoke();
                return;
            }

            string[] str = BitConverter.ToString(dataResponse, 0, dataResponse.Length).Split('-');
            int sizeStr = Convert.ToInt16(str[1], 16);
            Array.Resize(ref dataResponse, sizeStr + 2);

            OnLog?.Invoke("Данные отправлены.", "Статус");
            OnStopBlocking?.Invoke();
        }


        /// <summary>
        /// Информация по отправке данных на УСПД
        /// </summary>
        public void SendQuestion()
        {
            OnMessage?.Invoke("Данная сервисная консоль предназначена для вывода информации в raw-виде и для отладки напрямую. Не рекомендуется использовать, если вы не знаете, как работает протокол МЭК-104", "Справка о кнопке Отправить", MessageBoxImage.Information);
        }
    }

}
