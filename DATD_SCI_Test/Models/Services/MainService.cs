using DATD_SCI_Test.Models.Errors;
using DATD_SCI_Test.Models.Tables;
using DATD_SCI_Test.Models.TextOperations;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace DATD_SCI_Test.Models.Services
{
    /// <summary>
    /// Главный класс блока Model, обепечивает обмен данных между ViewModel и Model
    /// </summary>
    public class MainService
    {
        private BaseUnit _baseUnit;
        private Indicator _indicator;

        public bool IsConnect { get; set; }
        public double ReceiveProgress {  get; set; }
        
        public IPAddress BaseUnitIP { get; set; }
        
        public bool IsPrintTime { get; set; }
        public PhaseEnum Phase { get; set; }
        public ExceptionEnum ConnectionState => _baseUnit.ConnectionState; 
        public bool IsSuccessCompletionTCPconnection => _baseUnit.IsSuccessCompletionTCPconnection;

        public Action OnStartBlocking;
        public Action OnStopBlocking;
        public Action<string, string, MessageBoxImage> OnMessage;
        public Action<string> OnLog;
        public Action<string> OnGetDataBaseUnit;
        public Action<ExceptionEnum> OnResultConnection;
        public Action OnCompletionTCPconnection;
        public Action<double> OnReceiveProgress;

        public ObservableCollection<TelemetryData> TelemetryDatas { get; set; }
        

        public MainService()
        {
            _baseUnit = new ();
            _indicator = new ();

            TelemetryDatas = new();
            
            _baseUnit.OnStartBlocking += StartBlockingHandler;
            _baseUnit.OnStopBlocking += StopBlockingHandler;
            _baseUnit.OnException += ExceptionHandler;
            _baseUnit.OnResultConnection += ResultConnectionHandler;
            _baseUnit.OnCompletionTCPconnection += CompletionTCPconnectionHandler;
            _baseUnit.OnLog += LogHandler;
            _baseUnit.OnGetDataBaseUnit += GetDataBaseUnitHandler;
            _baseUnit.OnMessage += MessageHandler;
            _baseUnit.OnUpdateIsConnect += UpdateIsConnectHandler;
            _baseUnit.OnReceiveProgress += ReceiveProgressHandler;

            _indicator.OnStartBlocking += StartBlockingHandler;
            _indicator.OnStopBlocking += StopBlockingHandler;
            _indicator.OnLog += LogHandler;
            _indicator.OnReceiveProgress += ReceiveProgressHandler;

            InitializeTelemetryDatas();
        }


        /// <summary>
        /// Обработчик события начала блокировки
        /// </summary>
        private void StartBlockingHandler()
        {
            OnStartBlocking?.Invoke();
        }

        /// <summary>
        /// Обработчик события прекращения блокировки
        /// </summary>
        private void StopBlockingHandler()
        {
            OnStopBlocking?.Invoke();
        }

        /// <summary>
        /// Обработчик события завершения TCP-соединения с УСПД
        /// </summary>
        private void CompletionTCPconnectionHandler()
        {
            OnCompletionTCPconnection?.Invoke();
        }

        /// <summary>
        /// Обработчик события логирования
        /// </summary>
        /// <param name="log"></param>
        /// <param name="cause"></param>
        private void LogHandler(string log, string cause)
        {
            TextConverter textConverter = new ();

            string newLog = textConverter.ConvertTextToPrint(log, cause, IsPrintTime);
            OnLog?.Invoke(newLog);
        }


        /// <summary>
        /// Обработчик события сообщения для пользователя
        /// </summary>
        /// <param name="message"></param>
        /// <param name="caption"></param>
        /// <param name="messageBoxImage"></param>
        private void MessageHandler(string message, string caption, MessageBoxImage messageBoxImage)
        {
            OnMessage?.Invoke(message, caption, messageBoxImage);
        }

        /// <summary>
        /// Обработчик события сработанного исключения
        /// </summary>
        /// <param name="e"></param>
        /// <param name="err"></param>
        private void ExceptionHandler(Exception e, ExceptionEnum err)
        {
            OnStopBlocking?.Invoke();

            OnResultConnection?.Invoke(err);

            TextConverter textConverter = new();

            string log = textConverter.ConvertTextToPrint(err.ToString(), "Ошибка", IsPrintTime);
            OnLog?.Invoke(log);
            //connection_log.AppendText(AdditionalFunctions.TextBoxPrint(AdditionalFunctions.ErrorExceptionHandler(err, e.ToString()).ToString(), "Код ошибки", _showTime));

        }

        /// <summary>
        /// Обработчик события изменения состояния соединения (различные варианты соединения)
        /// </summary>
        /// <param name="err"></param>
        private void ResultConnectionHandler(ExceptionEnum err)
        {
            if (err == ExceptionEnum.None)
            {
                TextConverter textConverter = new();

                string log = textConverter.ConvertTextToPrint("Успешно", "Статус подключения", IsPrintTime);
                OnLog?.Invoke(log);
            }
            OnResultConnection?.Invoke(err);
        }

        /// <summary>
        /// Обработчик события получения параметра с УСПД (Время отправки телеизмерений)
        /// </summary>
        /// <param name="data"></param>
        private void GetDataBaseUnitHandler(string data)
        {
            OnGetDataBaseUnit?.Invoke(data);
        }

        /// <summary>
        /// Обработчик события изменения состояния подключения (варианты подключено или не подключено)
        /// </summary>
        private void UpdateIsConnectHandler()
        {
            IsConnect = _baseUnit.IsConnect;
        }

        /// <summary>
        /// Обработчик события обновления прогресса выполнения операции
        /// </summary>
        /// <param name="value"></param>
        private void ReceiveProgressHandler(double value)
        {
            OnReceiveProgress?.Invoke(value);
        }

        
        /// <summary>
        /// Инициализация таблицы телеизмерений
        /// </summary>
        public void InitializeTelemetryDatas()
        {
            TelemetryData.Initialize(TelemetryDatas);
        }

        /// <summary>
        /// Установление TCP-соединения с УСПД
        /// </summary>
        /// <param name="dataIP"></param>
        /// <param name="dataPort"></param>
        public void SetTCPconnection(string dataIP, string dataPort)
        {
            DataValidation validation = new();
            validation.OnLog += LogHandler;
            validation.OnMessage += MessageHandler;

            if (validation.ValidationDataIPAndDataPort(dataIP, dataPort))
            {
                TextConverter textConverter = new();

                string log = textConverter.ConvertTextToPrint(dataIP, "IP Адрес", IsPrintTime);
                OnLog?.Invoke(log);
                log = textConverter.ConvertTextToPrint(dataPort, "Порт", IsPrintTime);
                OnLog?.Invoke(log);

                _baseUnit.SetTCPconnectionAsync(dataIP, dataPort);
            }

            BaseUnitIP = _baseUnit.BaseUnitIP;
            
            
        }

        /// <summary>
        /// Завершение TCP-соединения с УСПД
        /// </summary>
        public void CompletionTCPconnection()
        {
            _baseUnit.CompletionTCPconnection();
        }

        /// <summary>
        /// Чтение данных с УСПД (Время отправки телеизмерений)
        /// </summary>
        /// <returns></returns>
        public async Task ReadDataBaseUnitAsync()
        {
            await _baseUnit.ReadDataBaseUnitAsync();
        }

        /// <summary>
        /// Запись данных в УСПД (Время отправки телеизмерений)
        /// </summary>
        /// <param name="data"></param>
        public void WriteDataBaseUnit(string data)
        {
            DataValidation validation = new();
            validation.OnLog += LogHandler;
            validation.OnMessage += MessageHandler;

            if (validation.ValidationDataBaseUnit(data))
                _baseUnit.WriteDataBaseUnitAsync(data);
        }

        /// <summary>
        /// Отправка данных на УСПД через сервисную консоль
        /// </summary>
        /// <param name="data"></param>
        public void SendDataBaseUnit(string data)
        {
            DataValidation validation = new();
            validation.OnLog += LogHandler;
            validation.OnMessage += MessageHandler;

            if (validation.ValidationServiceConsole(data))
                _baseUnit.SendDataBaseUnitAsync(data);
        }

        /// <summary>
        /// Информация по отправке данных на УСПД
        /// </summary>
        public void SendQuestion()
        {
            _baseUnit.SendQuestion();
        }

        /// <summary>
        /// Обновление данных от BaseUnit к Indicator
        /// </summary>
        private void UpdateDataBaseUnitIndicator()
        {
            _indicator.IsConnect = _baseUnit.IsConnect;
            _indicator.BaseUnitIP = _baseUnit.BaseUnitIP;
            _indicator.TelemetryDatas = TelemetryDatas;
            _indicator.Phase = Phase;
        }

        /// <summary>
        /// Обновление данных от Indicator к BaseUnit
        /// </summary>
        public void UpdateDataIndicatorBaseUnit()
        {
            _baseUnit.IsConnect = _indicator.IsConnect;
            
            _baseUnit.BaseUnitIP = _indicator.BaseUnitIP;
            TelemetryDatas = _indicator.TelemetryDatas;
            Phase = _indicator.Phase;
        }

        /// <summary>
        /// Чтение параметров индикатора
        /// </summary>
        public void ReadDataIndicator()
        {
            UpdateDataBaseUnitIndicator();
            if (Phase == PhaseEnum.None)
            {
                OnMessage?.Invoke("Фаза не выбрана", "Ошибка", MessageBoxImage.Error);
                return;
            }

            _indicator.ReadDataIndicatorAsync();
            
            //_baseUnit.BaseUnitClient = _indicator.BaseUnitClient;
            UpdateDataIndicatorBaseUnit();
        }


        /// <summary>
        /// Запись данных на индикатор
        /// </summary>
        public void WriteDataIndicator()
        {
            UpdateDataBaseUnitIndicator();
            OnMessage?.Invoke("Данные, введённые для невыбранных фаз, будут удалены","Внимание",  MessageBoxImage.Warning);

            if (!_indicator.IsReadParams)
            {
                OnMessage?.Invoke("Перед записью данных необходимо прочитать данные с индикатора", "Ошибка", MessageBoxImage.Error);
                return;
            }

            if (Phase == PhaseEnum.None)
            {
                OnMessage?.Invoke("Фаза не выбрана", "Ошибка", MessageBoxImage.Error);
                return;
            }
            

            // Удаляем те данные, которые были введены в невыделенные столбцы
            TelemetryData.ClearDataPhaseChecked(TelemetryDatas, Phase);

            DataValidation validation = new();
            validation.OnLog += LogHandler;
            validation.OnMessage += MessageHandler;

            bool isValid = validation.ValidationDataIndicator(Phase, TelemetryDatas);

            if (isValid) {
                _indicator.WriteDataIndicatorAsync();
            }
        }

        /// <summary>
        /// Очищение таблицы телеизмерений
        /// </summary>
        public void ClearDataIndicator()
        {
            TelemetryData.ClearData(TelemetryDatas);
        }

        /// <summary>
        /// Закрытие соединения
        /// </summary>
        public void Close()
        {
            BaseUnitConnection.CloseAll();
        }
    }
}
