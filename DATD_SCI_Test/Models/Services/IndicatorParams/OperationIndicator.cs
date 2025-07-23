using DATD_SCI_Test.Models.Bytes;
using DATD_SCI_Test.Models.Errors;
using DATD_SCI_Test.Models.Tables;
using DATD_SCI_Test.Models.TimerAndProgress;
using System.Collections.ObjectModel;
using System.IO;

namespace DATD_SCI_Test.Models.Services.IndicatorParams
{
    /// <summary>
    /// Класс для всех типов параметров по индикаторам
    /// </summary>
    public class OperationIndicator
    {
        public Action OnStartBlocking;
        public Action<string, string> OnLog;
        public Action<double> OnReceiveProgress;
        public Action OnStopBlocking;
        public Action OnReadIndicatorStopTimer;
       

        public ErrorExceptionHandler ErrorExceptionHandler {  get; set; }
        public OperationProgress OperationProgress { get; set; }
        public AsyncOperation AsyncOperation { get; set; }

        public OperationIndicator()
        {
            AsyncOperation = new();
            AsyncOperation.OnLog += LogHandler;
            ErrorExceptionHandler = new();
            OperationProgress = new();

            OperationProgress.OnLog += LogHandler;
            OperationProgress.OnStopBlocking += StopBlockingHandler;
            OperationProgress.OnReceiveProgress += ReceiveProgressHandler;
            OperationProgress.OnReadIndicatorStopTimer += ReadIndicatorStopTimerHandler;
            
        }

        /// <summary>
        /// Обработчик события логирования
        /// </summary>
        /// <param name="log"></param>
        /// <param name="cause"></param>
        public void LogHandler(string log, string cause)
        {
            OnLog?.Invoke(log, cause);
        }

        /// <summary>
        /// Обработчик события прекращения блокировки
        /// </summary>
        public void StopBlockingHandler()
        {
            OnStopBlocking?.Invoke();
        }

        /// <summary>
        /// Обработчик события остановки таймера при чтении параметров индикатора
        /// </summary>
        public void ReadIndicatorStopTimerHandler()
        {
            OnReadIndicatorStopTimer?.Invoke();
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
        /// Получение экземпляра типов параметров
        /// </summary>
        /// <param name="indicatorParam"></param>
        /// <returns></returns>
        private IParamsIndicator GetParamsIndicator(IndicatorParamEnum indicatorParam)
        {
            switch (indicatorParam) 
            {
                case IndicatorParamEnum.General:
                    return new GeneralParams();
                case IndicatorParamEnum.PhaseToPhase:
                    return new PhaseToPhaseParams();
                case IndicatorParamEnum.Ground:
                    return new GroundParams();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Получение буквенного обозначения фазы
        /// </summary>
        /// <param name="phase"></param>
        /// <returns></returns>
        private string GetPhaseName(PhaseEnum phase)
        {
            switch (phase)
            {
                case PhaseEnum.PhaseA:
                    return "A";
                case PhaseEnum.PhaseB:
                    return "B";
                case PhaseEnum.PhaseC:
                    return "C";
                default:
                    return string.Empty;
            }

        }


        /// <summary>
        /// Запись параметров в TelemetryDatas при чтении данных.
        /// </summary>
        /// <param name="apdu"></param>
        /// <param name="destinationAddress"></param>
        /// <param name="maxLength"></param>
        /// <param name="rowShift"></param>
        /// <param name="phase"></param>
        /// <param name="telemetryDatas"></param>
        /// <returns></returns>
        private bool WriteToTelemetryDatas(byte[] apdu, byte destinationAddress, byte maxLength, short rowShift,
            PhaseEnum phase, ObservableCollection<TelemetryData> telemetryDatas)
        {
            bool isAddData = false;

            try
            {
                for (int i = 19; i < apdu[18]; ++i)
                {
                    if (apdu[i] == destinationAddress && apdu[i - 1] < maxLength) //&& apdu[i + 2] > 0x00)
                                                                                  //TODO: На данный момент здесь возможен баг, если значение в памяти будет равно в первой половине 0x30/0x31/0x32
                    {
                        // Обработка однобайтовых значений
                        if (apdu[i + 2] == 0x01)
                        {
                            TelemetryData.SetValueByPhase(telemetryDatas[apdu[i - 1] + rowShift], phase, Convert.ToInt16(apdu[i + 3]).ToString());
                            isAddData = true;

                        }

                        // Обработка двухбайтовых значений
                        if (apdu[i + 2] == 0x02 || apdu[i + 2] == 0x04)
                        {
                            TelemetryData.SetValueByPhase(telemetryDatas[apdu[i - 1] + rowShift], phase, (apdu[i + 4] << 8 | apdu[i + 3]).ToString());
                            isAddData = true;
                            
                        }
                    }
                }
            }
            catch (Exception)
            {
                OnLog?.Invoke("Не удалось получить данные", "Ошибка");
            }

            if (isAddData)
                TelemetryData.ClearDataPhaseChecked(telemetryDatas, phase);
            else
                OnLog?.Invoke("УСПД вернул не те данные, попробуйте снова отправить запрос на чтение", "Ошибка");

            return isAddData;
        }


        /// <summary>
        /// Чтение параметров индикатора.
        /// На УСПД отправляется запрос в формате
        /// 68 11 1A 00 4C 00 7A 01 0D 00 00 00 00 00 00 2A 00 04 00,
        /// где: 
        ///     2A - тип параметров, который мы хотим прочитать,
        ///     04 - фаза, с которой мы хотим прочитать нужные нам параметры.
        ///     
        /// Первым читается RunParam - общие параметры индикатора (2A)
        /// Вторым читается CurrentParam - параметры определения межфазного замыкания (2B)
        /// Третьим читается GroundParam - параметры определения замыкания на землю (2C)
        /// </summary>
        /// <param name="phase"></param>
        /// <param name="telemetryDatas"></param>
        /// <param name="indicatorParamEnum"></param>
        /// <returns></returns>
        public async Task<bool> ReadDataAsync(PhaseEnum phase, 
            ObservableCollection<TelemetryData> telemetryDatas, IndicatorParamEnum indicatorParamEnum)
        {
            IParamsIndicator indicatorParams = GetParamsIndicator(indicatorParamEnum);

            OnLog?.Invoke(indicatorParams.StatusRead, $"Чтение данных фаза {GetPhaseName(phase)}");

            byte[] readParams = BytePackage.GetBytePackage(indicatorParams.BytePackageEnumRead);
            //Добавление фазы, с которой будут считываться данные 
            readParams[17] += (byte)(phase + 1);
            //Массив для хранения данных
            byte[] dataResponse = new byte[256];

            //Общий таймаут на операцию чтения
            double fullReadTimeout = 40.0;

            //Таймаут на операцию записи (отправки запроса на УСПД)
            double writeTimeout = 5.0;

            OperationProgress.TimerStart(fullReadTimeout);
            
            try
            {

                //В МЭК 104 клиент инициирует запрос(C_RD_NA_1, C_IC_NA_1 и т.д.).
                //Сервер(УСПД) отвечает только после запроса.
                //Если не отправить запрос — УСПД может ничего не прислать при дальнейшем чтении(или только служебные S / U - пакеты).
                await AsyncOperation.ExecuteOperationWriteAsync(readParams, 0, readParams.Length, writeTimeout);

                byte[] buffer = new byte[256];
                DateTime timeout = DateTime.Now.AddSeconds(fullReadTimeout);
                
                while (DateTime.Now < timeout)
                {
                    Array.Clear(dataResponse, 0, dataResponse.Length);  // Очищаем буфер

                    bool isWrite = await AsyncOperation.ExecuteOperationWriteAsync(readParams, 0, readParams.Length, 6);
                    OnLog?.Invoke(isWrite ? "Запрос на чтение успешно отправлен." : "Запрос на чтение не отправлен.", "Статус");

                    int bytesRead = await AsyncOperation.ExecuteOperationReadAsync(dataResponse, 0, dataResponse.Length, 6);
                    OnLog?.Invoke(bytesRead.ToString(), "Получено байт");

                    
                    if (bytesRead >= indicatorParams.MinBytePackageLength && dataResponse[1]/*buffer[1]*/ <= 0x66)
                    {
                        
                        OnLog?.Invoke(string.Join("-", dataResponse), "Данные");


                        //Конвертация полученных данных в битовый массив и разделение по ячейкам
                        string[] str = BitConverter.ToString(dataResponse, 0, dataResponse.Length).Split('-');

                        int sizeStr = Convert.ToInt16(str[1], 16);

                        Array.Resize(ref dataResponse, sizeStr + 2);

                        // Завершаем цикл заранее, если данные успешно получены
                        if (WriteToTelemetryDatas(dataResponse, indicatorParams.DestinationAddress,
                            indicatorParams.MaxLength, indicatorParams.RowShift, phase, telemetryDatas))
                        {
                            OnLog?.Invoke("Данные успешно прочитаны", $"Чтение данных фаза {GetPhaseName(phase)}");
                            OperationProgress.TimerBreak();
                            return true;
                        }

                    }
                    await Task.Delay(1000);
                }

                //dataResponse = responseStream.ToArray();
            }

            catch (IOException)
            {
                OnLog?.Invoke(ErrorExceptionHandler.GetErrorDescription(ExceptionEnum.IOExc), "Ошибка");
                return false;
            }

            ////Время истекло - обновляем ProgressBar до 100 %
            //OnReceiveProgress?.Invoke(TimerWorker.TargetValue);
            return false;
        }



        /// <summary>
        /// Запись данных из таблицы в массив для передачи на УСПД
        /// </summary>
        /// <param name="defaultPackage"></param>
        /// <param name="destinationAdress"></param>
        /// <param name="rowShift"></param>
        /// <param name="phase"></param>
        /// <param name="telemetryDatas"></param>
        private void WriteToDefaultPackage(ref byte[] defaultPackage, byte destinationAdress, int rowShift, PhaseEnum phase,
            ObservableCollection<TelemetryData> telemetryDatas)
        {
            short storage;
            string data;

            try
            {
                for (int i = 19; i < defaultPackage.Length; ++i)
                {
                    if (defaultPackage[i] == destinationAdress && defaultPackage[i - 1] < 0x08) //&& dataResponse[i + 2] > 0x00)
                                                                                                //TODO: На данный момент здесь возможен баг, если значение в памяти будет равно
                                                                                                //в первой половине 0x30/0x31/0x32
                    {
                        if (defaultPackage[i + 2] == 0x01)
                        {
                            data = TelemetryData.GetValueByPhase(telemetryDatas[defaultPackage[i - 1] + rowShift], phase);

                            if (data == null || data == string.Empty)
                            {
                                defaultPackage = null;
                            }
                            else
                                defaultPackage[i + 3] = Convert.ToByte(data);
                        }
                        if (defaultPackage[i + 2] == 0x02 || defaultPackage[i + 2] == 0x04)
                        {
                            data = TelemetryData.GetValueByPhase(telemetryDatas[defaultPackage[i - 1] + rowShift], phase);

                            if (data == null || data == string.Empty)
                            {
                                defaultPackage = null;
                            }
                            else
                            {
                                storage = Convert.ToInt16(data);
                                defaultPackage[i + 3] = (byte)(storage & 0x00FF);
                                defaultPackage[i + 4] = (byte)(storage >> 8);
                            }

                        }
                        //if (defaultPackage[i + 2] == 0x04)
                        //{
                        //    if (telemetryDatas[defaultPackage[i - 1] + index].Cells[phase] == null ||
                        //        telemetryDatas[defaultPackage[i - 1] + index].Cells[phase].Value.ToString() == "")
                        //    {
                        //        defaultPackage = null;
                        //    }
                        //    storage = Convert.ToInt16(telemetryDatas[defaultPackage[i - 1] + index].Cells[phase].Value);
                        //    defaultPackage[i + 3] = (byte)(storage & 0x00FF);
                        //    defaultPackage[i + 4] = (byte)(storage >> 8);
                        //    //defaultRunPackage[i + 5] = 0xFF; UNUSED
                        //    //defaultRunPackage[i + 6] = 0xFF; UNSUED
                        //    //TODO: В проге китайцев написано что диапазон у чисел в 4 байта с 0 до 65535, что является по факту диапазоном в 2 байта - 0xFFFF. Надо уточнять у китайцев
                        //}
                    }
                }
            }
            catch (Exception)
            {
                OnLog?.Invoke("Не удалось записать данные", "Ошибка");
            }
        }

        /// <summary>
        /// Запись параметров индикатора.
        /// На УСПД отправляется запрос в формате
        /// 68 XX 1C 00 4E 00 7D 01 0D 00 00 00 00 00 00 2A 00 04 XX ...
        /// где: 
        ///     2A - тип параметров, который мы хотим записать,
        ///     04 - фаза, на котороую мы хотим записать нужные нам параметры
        /// </summary>
        /// <param name="phase"></param>
        /// <param name="telemetryDatas"></param>
        /// <param name="indicatorParamEnum"></param>
        public async Task WriteDataAsync(PhaseEnum phase, 
            ObservableCollection<TelemetryData> telemetryDatas, IndicatorParamEnum indicatorParamEnum)
        {
            IParamsIndicator indicatorParams = GetParamsIndicator(indicatorParamEnum);

            OnLog?.Invoke(indicatorParams.StatusWrite, $"Запись данных фаза {GetPhaseName(phase)}");

            byte[] defaultGeneralPackage = BytePackage.GetBytePackage(indicatorParams.BytePackageEnumWrite);
            defaultGeneralPackage[17] += (byte)phase;

            WriteToDefaultPackage(ref defaultGeneralPackage, indicatorParams.DestinationAddress,
                indicatorParams.RowShift, phase, telemetryDatas);

            if (defaultGeneralPackage == null)
                return;

            bool isWrite = await AsyncOperation.ExecuteOperationWriteAsync(defaultGeneralPackage, 0, defaultGeneralPackage.Length, 5);

            if (isWrite)
                OnLog?.Invoke("Данные успешно записаны", "Статус");
        }

    }
}
