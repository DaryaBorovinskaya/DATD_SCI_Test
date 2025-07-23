using DATD_SCI_Test.Models;
using DATD_SCI_Test.Models.Errors;
using DATD_SCI_Test.Models.Services;
using DATD_SCI_Test.Models.Tables;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace DATD_SCI_Test.ViewModels
{
    /// <summary>
    /// Обработка и получение данных из окна MainWindow
    /// </summary>
    public class MainWindowVM : BasicVM
    {
        #region TelemetryData Rows
        public ICollectionView TelemetryDataView { get; }
        #endregion

        private ObservableCollection<TelemetryData> _telemetryDataTable;
        private bool _isPrintTime = false;
        private MainService _mainService;
        private SolidColorBrush _connectionIndicatorColor = new (Colors.White);
        private string _connectionLogTextBox;
        private string _timeSendTelemetryTextBox;
        private string _serviceConsoleTextBox;
        private string _ipAddressTextBox;
        private string _portTextBox;
        private double _receiveProgressBar;

        public Action OnStartBlocking;
        public Action OnStopBlocking;
        public Action<string, string, MessageBoxImage> OnMessage;
        public Action<ExceptionEnum> OnResultConnection;
        public Action OnCompletionTCPconnection;

        private PhaseEnum _phase = PhaseEnum.None;

        public PhaseEnum Phase
        {
            get { return _phase; }
            set
            {
                Set(ref  _phase, value);
                _mainService.Phase = _phase;
            }
        }

        

        public ObservableCollection<TelemetryData> TelemetryDataTable
        {
            get { return _telemetryDataTable; }
            set
            {
                if (_telemetryDataTable == null)
                    _telemetryDataTable = value;

                else
                    Set(ref _telemetryDataTable, value);
            }
        }

        public string TelemetryDataColumnType => TelemetryData.TypeColumnTitle;
        public string TelemetryDataColumnPhaseA => TelemetryData.PhaseAColumnTitle;
        public string TelemetryDataColumnPhaseB => TelemetryData.PhaseBColumnTitle;
        public string TelemetryDataColumnPhaseC => TelemetryData.PhaseCColumnTitle;


        public bool IsPrintTime
        {
            get { return _isPrintTime; }
            set
            {
                Set(ref _isPrintTime, value);

                _mainService.IsPrintTime = _isPrintTime;
            }
        }

        public double ReceiveProgressBar
        {
            get { return _receiveProgressBar; }
            set
            {
                Set(ref _receiveProgressBar, value);

                _mainService.ReceiveProgress = _receiveProgressBar;
            }
        }

        private SolidColorBrush GetColorByError(ExceptionEnum err)
        {
            if (_mainService.IsSuccessCompletionTCPconnection)
                return new SolidColorBrush(Colors.White);

            else
            {
                switch (err)
                {
                    case ExceptionEnum.InvalOpExc:
                        return new SolidColorBrush(Colors.Yellow);
                    case ExceptionEnum.IOExc or ExceptionEnum.ScktExc:
                        return new SolidColorBrush(Colors.Red);
                    case ExceptionEnum.None:
                        return new SolidColorBrush(Colors.Lime);
                    default:
                        return new SolidColorBrush(Colors.White);
                }
            }
        }


        public SolidColorBrush ConnectionIndicatorColor
        {
            get { return _connectionIndicatorColor; }
            set
            {
                Set(ref _connectionIndicatorColor, value);
                
            }
        }
        public string ConnectionLogTextBox
        {
            get { return _connectionLogTextBox; }
            set
            {
                Set(ref _connectionLogTextBox, value);
            }
        }

        public string TimeSendTelemetryTextBox
        {
            get { return _timeSendTelemetryTextBox; }
            set
            {
                if (_timeSendTelemetryTextBox == null)
                    _timeSendTelemetryTextBox = value;
                else
                {
                    Set(ref _timeSendTelemetryTextBox, value);
                }
                    
            }
        }

        public string IPaddressTextBox
        {
            get { return _ipAddressTextBox; }
            set
            {
                if (_ipAddressTextBox == null)
                    _ipAddressTextBox = value;
                else
                {
                    Set(ref _ipAddressTextBox, value);
                }

            }
        }

        public string PortTextBox
        {
            get { return _portTextBox; }
            set
            {
                if (_portTextBox == null)
                    _portTextBox = value;
                else
                {
                    Set(ref _portTextBox, value);
                }

            }
        }

        public string ServiceConsoleTextBox
        {
            get { return _serviceConsoleTextBox; }
            set
            {
                if (_serviceConsoleTextBox == null)
                    _serviceConsoleTextBox = value;
                else
                {
                    Set(ref _serviceConsoleTextBox, value);
                }

            }
        }


        public MainWindowVM()
        {
            _mainService = new MainService();
            TelemetryDataTable = _mainService.TelemetryDatas;

            #region TelemetryData Rows
            TelemetryDataView = CollectionViewSource.GetDefaultView(_telemetryDataTable);
            TelemetryDataView.GroupDescriptions.Add(new PropertyGroupDescription("GroupName"));
            #endregion

            _timeSendTelemetryTextBox = string.Empty;
            _isPrintTime = true;
            _mainService.IsPrintTime = _isPrintTime;

            _mainService.OnStartBlocking += StartBlockingHandler;
            _mainService.OnStopBlocking += StopBlockingHandler;
            _mainService.OnMessage += MessageHandler;
            _mainService.OnLog += LogHandler;
            _mainService.OnResultConnection += ResultConnectionHandler;
            _mainService.OnCompletionTCPconnection += CompletionTCPconnectionHandler;
            _mainService.OnGetDataBaseUnit += GetDataBaseUnitHandler; 
            _mainService.OnReceiveProgress += ReceiveProgressHandler;
        }

        public void StartBlockingHandler()
        {
            OnStartBlocking?.Invoke();
        }

        public void StopBlockingHandler()
        {
            OnStopBlocking?.Invoke();
        }
        public void CompletionTCPconnectionHandler()
        {
            ConnectionIndicatorColor = GetColorByError(ExceptionEnum.None);
            OnCompletionTCPconnection?.Invoke();
        }

        private void ReceiveProgressHandler(double value)
        {
            ReceiveProgressBar = value;
        }

        public void MessageHandler(string message, string caption, MessageBoxImage messageBoxImage)
        {
            OnMessage?.Invoke( message,  caption,  messageBoxImage);
        }


        private void LogHandler(string log)
        {
            ConnectionLogTextBox += log;
        }

        private void ResultConnectionHandler(ExceptionEnum err)
        {
            ConnectionIndicatorColor = GetColorByError(err);

            OnResultConnection?.Invoke(err);
        }

        private void GetDataBaseUnitHandler(string data)
        {
            TimeSendTelemetryTextBox = data;
        }

        /// <summary>
        /// Закрытие приложения
        /// </summary>
        public void Close()
        {
            _mainService.Close();
        }

        public ICommand SetTCPconnectionClick
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {
                    _mainService.SetTCPconnection(IPaddressTextBox, PortTextBox);
                });
            }
        }

        public ICommand CompletionTCPconnectionClick
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {
                    _mainService.CompletionTCPconnection();
                });
            }
        }

        public ICommand ReadDataIndicatorClick
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {
                    _mainService.ReadDataIndicator();
                });
            }
        }

        public ICommand WriteDataIndicatorClick
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {
                    _mainService.WriteDataIndicator();
                });
            }
        }

        public ICommand ClearDataIndicatorClick
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {
                    _mainService.ClearDataIndicator();
                });
            }
        }

        public ICommand ClearLogClick
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {
                    ConnectionLogTextBox = string.Empty;
                });
            }
        }

        
        public ICommand ReadDataBaseUnitClick
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {
                    _mainService.ReadDataBaseUnitAsync();
                });
            }
        }

        public ICommand WriteDataBaseUnitClick
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {
                    _mainService.WriteDataBaseUnit(TimeSendTelemetryTextBox);
                });
            }
        }

        public ICommand SendClick
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {
                    _mainService.SendDataBaseUnit(ServiceConsoleTextBox);
                });
            }
        }

        public ICommand SendQuestionClick
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {
                    _mainService.SendQuestion();
                });
            }
        }
    }
}
