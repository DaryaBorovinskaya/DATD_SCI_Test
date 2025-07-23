using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATD_SCI_Test.Models.TimerAndProgress
{
    /// <summary>
    /// Управление длительностью операций и изменение значений ProgressBar
    /// </summary>
    public class OperationProgress
    {
        private TimerWorker _timerWorker;

        public Action<double> OnReceiveProgress;
        public Action OnReadIndicatorStopTimer;
        public Action OnStopBlocking;
        public Action<string, string> OnLog;

        public OperationProgress()
        {
            _timerWorker = new ();

            _timerWorker.OnReceiveProgress += ReceiveProgressHandler;
            _timerWorker.OnLog += LogHandler;
            _timerWorker.OnStopBlocking += StopBlockingHandler;
            _timerWorker.OnStopTimer += ReadIndicatorStopTimerHandler;
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
        /// Обработчик события прекращения блокировки
        /// </summary>
        private void StopBlockingHandler()
        {
            OnStopBlocking?.Invoke();
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
        /// Обработчик события остановки таймера при чтении параметров индикатора
        /// </summary>
        private void ReadIndicatorStopTimerHandler()
        {
            OnReadIndicatorStopTimer?.Invoke();
        }

        


        /// <summary>
        /// Начало работы таймера
        /// </summary>
        /// <param name="timeout"></param>
        public void TimerStart(double timeout)
        {
            _timerWorker.TimerStart(timeout);
        }

        /// <summary>
        /// Прерывание работающего таймера
        /// </summary>
        public void TimerBreak()
        {
            _timerWorker.TimerBreak();
        }


    }
}
