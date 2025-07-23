using DATD_SCI_Test.Models.Bytes;
using DATD_SCI_Test.Models.Services.IndicatorParams;
using DATD_SCI_Test.Models.Tables;
using DATD_SCI_Test.Models.TimerAndProgress;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;

namespace DATD_SCI_Test.Models.Services
{
    /// <summary>
    /// Вызов операций, связанных с взаимодействием УСПД и ИКЗ, класс-обёртка
    /// </summary>
    public class Indicator
    {
        
        private TimerWorker _timerWorker;
        private OperationIndicator _operationIndicator;

        private int _numbOfReadingParams;

        public bool IsConnect { get; set; }
        
        public IPAddress BaseUnitIP { get; set; }
        public PhaseEnum Phase { get; set; }
        public bool IsReadParams { get; set; }


        public Action OnStartBlocking;
        public Action OnStopBlocking;
        public Action<string, string> OnLog;
        public Action<double> OnReceiveProgress;

        public ObservableCollection<TelemetryData> TelemetryDatas { get; set; }
        
        
        public Indicator()
        {
            IsReadParams = false;
            TelemetryDatas = new();
            _numbOfReadingParams = 1;

            _timerWorker = new();
            _timerWorker.OnLog += LogHandler;
            _timerWorker.OnStopBlocking += StopBlockingHandler;
            _timerWorker.OnReceiveProgress += ReceiveProgressHandler;
            _timerWorker.OnStopTimer += ReadIndicatorStopTimerHandlerAsync;

            _operationIndicator = new();
            _operationIndicator.OnLog += LogHandler;
            _operationIndicator.OnReceiveProgress += ReceiveProgressHandler;
            _operationIndicator.OnStartBlocking += StartBlockingHandler;
            _operationIndicator.OnReadIndicatorStopTimer += ReadIndicatorStopTimerHandlerAsync;
            _operationIndicator.OnStopBlocking += StopBlockingHandler;
        }

        /// <summary>
        /// Обработчик события логирования
        /// </summary>
        /// <param name="log"></param>
        /// <param name="cause"></param>
        private void LogHandler(string log, string cause)
        {
            OnLog?.Invoke(log, cause);
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
        /// Обработчик события остановки таймера при чтении параметров индикатора
        /// </summary>
        private async void ReadIndicatorStopTimerHandlerAsync()
        {
            if (_numbOfReadingParams == 1)
            {
                #region Чтение параметров определения межфазного замыкания

                await Task.Delay(1000);
                IsReadParams = await _operationIndicator.ReadDataAsync(Phase, TelemetryDatas, IndicatorParamEnum.PhaseToPhase);
                
                _numbOfReadingParams++;
                #endregion
            }

            else if (_numbOfReadingParams == 2)
            {
                #region Чтение параметров определения замыкания на землю
                await Task.Delay(1000);
                IsReadParams = await _operationIndicator.ReadDataAsync(Phase, TelemetryDatas, IndicatorParamEnum.Ground);
                
                _numbOfReadingParams++;
                #endregion
            }


        }

        /// <summary>
        /// Обработчик события обновления прогресса выполнения операции
        /// </summary>
        /// <param name="value"></param>
        public void ReceiveProgressHandler(double value)
        {
            OnReceiveProgress?.Invoke(value);
        }



        /// <summary>
        ///  Чтение параметров индикатора 
        /// </summary>
        public async void ReadDataIndicatorAsync()
        {
            if (IsConnect == false)
            {
                OnLog?.Invoke("Соединение не установлено", "Статус подключения");
                return;
            }

            OnStartBlocking?.Invoke();

            #region Чтение общих параметров индикатора
            IsReadParams = await _operationIndicator.ReadDataAsync(Phase, TelemetryDatas, IndicatorParamEnum.General);
            #endregion

            OnStopBlocking?.Invoke();
        }


        /// <summary>
        /// Запись данных на индикатор
        /// </summary>
        public async Task WriteDataIndicatorAsync()
        {
            if (IsConnect == false )
            {
                OnLog?.Invoke("Соединенине не установлено", "Статус подключения");
                return;
            }
            OnStartBlocking?.Invoke();

            await _operationIndicator.WriteDataAsync(Phase, TelemetryDatas, IndicatorParamEnum.General);
            await _operationIndicator.WriteDataAsync(Phase, TelemetryDatas, IndicatorParamEnum.PhaseToPhase);
            await _operationIndicator.WriteDataAsync(Phase, TelemetryDatas, IndicatorParamEnum.Ground);

            OnStopBlocking?.Invoke();

        }

 
    }

}
