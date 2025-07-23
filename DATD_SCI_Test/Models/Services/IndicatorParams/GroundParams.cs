using DATD_SCI_Test.Models.Bytes;
using DATD_SCI_Test.Models.Tables;
using System.Collections.ObjectModel;

namespace DATD_SCI_Test.Models.Services.IndicatorParams
{
    /// <summary>
    /// Параметры определения замыкания на землю
    /// </summary>
    public class GroundParams : IParamsIndicator
    {
        private readonly string _statusWrite = "Запись настроек замыкания на землю.";
        private readonly string _statusRead = "Получение настроек замыкания на землю.";

        public string StatusWrite => _statusWrite;
        public string StatusRead => _statusRead;

        public BytePackageEnum BytePackageEnumRead => BytePackageEnum.ReadGroundParamsIndicator;
        public BytePackageEnum BytePackageEnumWrite => BytePackageEnum.DefaultGroundPackageIndicator;
        public int MinBytePackageLength => 30;
        public byte DestinationAddress => 0x32;

        public byte MaxLength => 0x02;
        public short RowShift => 11;

    }
}
