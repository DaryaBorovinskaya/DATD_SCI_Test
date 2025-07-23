using DATD_SCI_Test.Models.Bytes;
using DATD_SCI_Test.Models.Tables;
using System.Collections.ObjectModel;

namespace DATD_SCI_Test.Models.Services.IndicatorParams
{
    /// <summary>
    /// Параметры определения межфазного замыкания
    /// </summary>
    public class PhaseToPhaseParams : IParamsIndicator
    {
        private readonly string _statusWrite = "Запись настроек межфазного замыкания.";
        private readonly string _statusRead = "Получение настроек межфазного замыкания.";


        public string StatusWrite => _statusWrite;
        public string StatusRead => _statusRead;

        public BytePackageEnum BytePackageEnumRead => BytePackageEnum.ReadPhaseToPhaseParamsIndicator;
        public BytePackageEnum BytePackageEnumWrite => BytePackageEnum.DefaultPhaseToPhasePackageIndicator;

        public int MinBytePackageLength => 30;
        public byte DestinationAddress => 0x31;

        public byte MaxLength => 0x03;
        public short RowShift => 8;

    }
}
