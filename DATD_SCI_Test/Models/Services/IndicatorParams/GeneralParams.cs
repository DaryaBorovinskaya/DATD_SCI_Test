using DATD_SCI_Test.Models.Bytes;
using DATD_SCI_Test.Models.Tables;
using System.Collections.ObjectModel;

namespace DATD_SCI_Test.Models.Services.IndicatorParams
{
    /// <summary>
    /// Общие параметры индикатора
    /// </summary>
    public class GeneralParams : IParamsIndicator
    {
        private readonly string _statusWrite = "Запись общих параметров индикатора.";
        private readonly string _statusRead = "Получение общих параметров индикатора.";
        
        public string StatusWrite => _statusWrite;
        public string StatusRead => _statusRead;

        public BytePackageEnum BytePackageEnumRead => BytePackageEnum.ReadGeneralParamsIndicator;
        public BytePackageEnum BytePackageEnumWrite => BytePackageEnum.DefaultGeneralPackageIndicator;
        public int MinBytePackageLength => 20;
        public byte DestinationAddress => 0x30;

        public byte MaxLength => 0x08;
        public short RowShift => 0;
        
    }
}
