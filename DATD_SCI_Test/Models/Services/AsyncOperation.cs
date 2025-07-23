namespace DATD_SCI_Test.Models.Services
{
    /// <summary>
    /// Асинхронные операции чтения и записи (используется класс BaseUnitConnection)
    /// </summary>
    public class AsyncOperation
    {
        public Action<string, string> OnLog;

        /// <summary>
        /// Чтение данных из сокета 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task<bool> ExecuteOperationWriteAsync(byte[] buffer, int offset, int count, double timeout)
        {
            TimeSpan timeoutTimeSpan = TimeSpan.FromSeconds(timeout);
            using var cts = new CancellationTokenSource(timeoutTimeSpan);
            try
            {
                cts.CancelAfter(timeoutTimeSpan);

                await BaseUnitConnection.GetStream().WriteAsync(buffer, offset, count, cts.Token);
                return true;
            }
            catch (OperationCanceledException)
            {
                OnLog?.Invoke($"Превышено время ожидания операции записи данных ({timeoutTimeSpan.TotalSeconds} сек)", "Внимание");
                return false;
            }
            catch (Exception)
            {
                OnLog?.Invoke($"Ошибка при выполнении операции записи данных", "Ошибка");
                return false;
            }
        }

        /// <summary>
        /// Запись данных в сокет
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task<int> ExecuteOperationReadAsync(byte[] buffer, int offset, int count, double timeout)
        {
            TimeSpan timeoutTimeSpan = TimeSpan.FromSeconds(timeout);
            using var cts = new CancellationTokenSource(timeoutTimeSpan);
            int bytesCount = 0;
            try
            {
                cts.CancelAfter(timeoutTimeSpan);

                bytesCount = await BaseUnitConnection.GetStream().ReadAsync(buffer, offset, count, cts.Token);
                return bytesCount;
            }
            catch (OperationCanceledException)
            {
                OnLog?.Invoke($"Превышено время ожидания операции чтения данных ({timeoutTimeSpan.TotalSeconds} сек)", "Внимание");
                return bytesCount;
            }
            catch (Exception)
            {
                OnLog?.Invoke($"Ошибка при выполнении операции чтения данных", "Ошибка");
                return -1;
            }
        }
    }
}
