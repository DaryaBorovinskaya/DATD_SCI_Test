namespace DATD_SCI_Test.Models.Protocol
{
    /// <summary>
    /// Экспериментальный класс по внедрению в проект готовой библиотеки lib60870 
    /// для операций, происходящих по протоколу МЭК-104.
    /// Ссылка на библиотеку: https://github.com/mz-automation/lib60870.NET
    /// </summary>
    public class IEC104Client
    {
        //private Connection _connection;
        //private bool _dataReceived = false;

        //public async Task ConnectAsync(string ip, int port)
        //{
        //    _connection = new Connection(ip, port);
        //    _connection.SetConnectionHandler(OnConnectionStateChanged, null);
            

        //    await Task.Run(() => _connection.Connect());

        //    // Ждём подключения
        //    await Task.Delay(1000);
        //}

        //private void OnConnectionStateChanged(object parameter, ConnectionEvent connectionEvent)
        //{
        //    switch (connectionEvent)
        //    {
        //        case ConnectionEvent.OPENED:
        //            MessageBox.Show("Connected");
        //            break;
        //        case ConnectionEvent.CLOSED:
        //            MessageBox.Show("Connection closed");
        //            break;
        //        case ConnectionEvent.STARTDT_CON_RECEIVED:
        //            MessageBox.Show("STARTDT CON received");
        //            break;
        //        case ConnectionEvent.STOPDT_CON_RECEIVED:
        //            MessageBox.Show("STOPDT CON received");
        //            break;
        //    }
        //}



        //private bool HandleNewASDU(ApplicationLayer ASDU, object parameter)
        //{
        //    if (ASDU.CauseOfTransmission == CauseOfTransmission.Interrogation &&
        //        ASDU.TypeId == TypeID.M_ME_NC_1) // Аналоговые значения без качества
        //    {
        //        foreach (var io in ASDU.InformationObjects)
        //        {
        //            foreach (var element in io.Elements)
        //            {
        //                Console.WriteLine($"IOA={io.ObjectAddress}, Value={element.GetValue().ToString()}");
        //            }
        //        }
        //    }
        //    else if (ASDU.CauseOfTransmission == CauseOfTransmission.Interrogation &&
        //             ASDU.TypeId == TypeID.M_SP_NA_1) // Дискретные сигналы
        //    {
        //        foreach (var io in ASDU.InformationObjects)
        //        {
        //            foreach (var element in io.Elements)
        //            {
        //                Console.WriteLine($"IOA={io.ObjectAddress}, Signal={element.GetValue().ToString()}");
        //            }
        //        }
        //    }

        //    return true; // продолжаем получать данные
        //}

        //public void StartDataReceiver()
        //{
        //    _connection.SetASDUReceivedHandler(HandleNewASDU, CauseOfTransmission.Interrogation, true);
        //    _connection.Start();
        //}


        //public async Task SendInterrogationCommand(byte commonAddress = 1)
        //{
        //    var command = new InterrogationCommand(commonAddress, 0x0D); // M_EH_1
        //    _connection.SendASDU(command);
        //    Console.WriteLine("Команда общего вызова отправлена");
        //}

        //public void Disconnect()
        //{
        //    //_connection.Stop();
        //    //_connection.Dispose();
        //}




        //public async Task<ASDU> ReadDataAsync(int timeoutSec = 30)
        //{
        //    TaskCompletionSource<ASDU> responseTcs = new();
            
        //    // Создаем CancellationTokenSource для таймаута
        //    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSec));

        //    try
        //    {
        //        _connection.SendInterrogationCommand(
        //            CauseOfTransmission.ACTIVATION,
        //            1,     // Common Address
        //            0xFF   // IOA
        //        );

        //        // Регистрируем отмену через токен
        //        cts.Token.Register(() => responseTcs.TrySetCanceled(cts.Token));

        //        // Ожидаем завершения задачи (с привязкой к токену)
        //        return await responseTcs.Task.WaitAsync(cts.Token);
        //    }
        //    catch (OperationCanceledException)
        //    {
        //        Console.WriteLine("Операция отменена по таймауту");
        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Ошибка: {ex.Message}");
        //        return null;
        //    }
        //    finally
        //    {
        //        _connection.SetConnectionHandler(null, null);

        //        if (_connection.IsRunning)
        //            _connection.Close();
        //    }
        //}
    }
}
