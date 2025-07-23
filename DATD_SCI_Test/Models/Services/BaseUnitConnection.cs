using System.Net;
using System.Net.Sockets;

namespace DATD_SCI_Test.Models.Services
{
    /// <summary>
    /// Управление соединением с УСПД
    /// </summary>
    public class BaseUnitConnection
    {
        private static TcpClient _client;
        private static NetworkStream _baseUnitStream;

        /// <summary>
        /// Получение сокета для дальнейших чтения и записи данных
        /// </summary>
        /// <returns></returns>
        public static NetworkStream GetStream()
        {
            if (_client == null /*|| _client.Connected == false*/)
            {
                _client = new TcpClient();
            }

            if (_baseUnitStream == null || !_baseUnitStream.CanRead)
            {
                
                _baseUnitStream = _client.GetStream();
            }
            
            return _baseUnitStream;
        }

        /// <summary>
        /// Установление соединения по IP и порту
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static Task ConnectAsync(IPAddress address, int port)
        {
            _client = new TcpClient();
            return _client.ConnectAsync(address, port);
        }

        /// <summary>
        /// Закрытие сокета и завершение соединения TCP-клиента
        /// </summary>
        public static void CloseAll()
        {
            if (_client != null)
            {
                _client.Dispose();
                _client = null;
            }

            if (_baseUnitStream != null) 
            { 
                _baseUnitStream.Dispose(); 
            }
            
        }

        /// <summary>
        /// Очищение TCP-клиента
        /// </summary>
        public static void DisposeClient()
        {
            _client.Dispose();
            _client = null;
        }
    }
}
