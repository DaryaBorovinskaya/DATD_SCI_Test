using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DATD_SCI_Test.Models.TimerAndProgress
{
    public class TimerWorker
    {
        private DispatcherTimer _timer;

        /// <summary>
        /// Конечное значение 
        /// </summary>
        private double _targetValue = 100; 
        
        /// <summary>
        /// Текущее значение 
        /// </summary>
        private double _currentValue = 0;

        /// <summary>
        /// Начальное значение
        /// </summary>
        private double _startValue = 0;

        /// <summary>
        /// Длительность действия в секундах
        /// </summary>
        private double _actionDuration = 5.0;

        /// <summary>
        /// Время начала
        /// </summary>
        private DateTime _startTime;

        /// <summary>
        /// Асинхронная операция
        /// </summary>
        private Task _asyncTask;

        /// <summary>
        /// Асинхронная операция таймера
        /// </summary>
        private Task? _timeoutTask;

        /// <summary>
        /// Асинхронная операция (первая завершившаяся)
        /// </summary>
        private Task? _completedTask;


        public Task AsyncTask
        {
            get { return _asyncTask; }
            set { _asyncTask = value; }
        }
        public Task? TimeoutTask
        {
            get { return _timeoutTask; }
            set { _timeoutTask = value; }
        }

        public Task? CompletedTask
        {
            get { return _completedTask; }
            set { _completedTask = value; }
        }

        public Action<double> OnReceiveProgress;
        public Action OnStopBlocking;
        public Action<string, string> OnLog;
        public Action OnStopTimer;

        /// <summary>
        /// Обработчик таймера
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            double elapsed = (DateTime.Now - _startTime).TotalSeconds;
            double progress = Math.Min(elapsed / _actionDuration, 1.0);

            // Используем квадратичную функцию для плавности
            double smoothProgress = progress * progress;

            _currentValue = smoothProgress * _targetValue;
            OnReceiveProgress?.Invoke(_currentValue);

            // Если задача уже завершилась (успешно), сразу доходим до 100%
            if (_asyncTask?.IsCompleted == true)
            {
                TimerBreak();
                return;
            }

            if (progress >= 1.0)
            {
                TimerBreak();
                
                // Принудительно прерываем ожидание подключения по таймауту
                if (_asyncTask != null && !_asyncTask.IsCompleted)
                {
                    OnLog?.Invoke($"Превышено время ожидания ({_actionDuration} сек)", "Ошибка");
                    OnStopBlocking?.Invoke();

                }
            }
        }

        public TimerWorker()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
            _timer.Tick += Timer_Tick;
        }

        /// <summary>
        /// Начало работы таймера
        /// </summary>
        /// <param name="timeout"></param>
        public void TimerStart(double timeout)
        {
            _currentValue = 0;
            OnReceiveProgress?.Invoke(_startValue);
            _targetValue = 100;
            _startTime = DateTime.Now;
            _actionDuration = timeout;

            // Останавливаем таймер, если уже запущен
            if (_timer.IsEnabled)
                _timer.Stop();

            
            _timer.Start();
        }

        /// <summary>
        /// Прерывание работающего таймера
        /// </summary>
        public void TimerBreak()
        {
            // Останавливаем таймер, если уже запущен
            if (_timer.IsEnabled)
            {
                _timer.Stop();
                OnReceiveProgress?.Invoke(_targetValue);
                OnStopTimer?.Invoke();
            }
        }
    }
}
