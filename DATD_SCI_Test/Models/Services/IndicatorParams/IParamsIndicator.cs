using DATD_SCI_Test.Models.Bytes;

namespace DATD_SCI_Test.Models.Services.IndicatorParams
{
    public interface IParamsIndicator
    {
        /// <summary>
        /// Начало записи данных (для логирования)
        /// </summary>
        public string StatusWrite { get; }
        /// <summary>
        /// Начало чтения данных (для логирования)
        /// </summary>
        public string StatusRead { get; }
        /// <summary>
        /// Пакет байтов для записи
        /// </summary>
        public BytePackageEnum BytePackageEnumWrite { get; }
        /// <summary>
        /// Пакет байтов для чтения
        /// </summary>
        public BytePackageEnum BytePackageEnumRead {  get; }

        /// <summary>
        /// Минимальная длина пришедшего от УСПД пакета данных
        /// </summary>
        public int MinBytePackageLength { get; }

        /// <summary>
        /// Адрес параметров, которые запишутся в пакет данных:
        /// 0х30 - данные по общим параметрам
        /// 0х31 - данные по межфазному замыканию
        /// 0х32 - данные по замыканию на землю 
        /// </summary>
        public byte DestinationAddress { get; }

        /// <summary>
        /// Количество подпараметров
        /// </summary>
        public byte MaxLength { get; }

        /// <summary>
        /// Смещение по таблице телеизмерений (относительно порядка расположения параметров)
        /// </summary>
        public short RowShift { get; }
    }
}
