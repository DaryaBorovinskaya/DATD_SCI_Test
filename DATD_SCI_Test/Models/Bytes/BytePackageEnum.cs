using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATD_SCI_Test.Models.Bytes
{
    /// <summary>
    /// Именное обращение к определённому пакету байтов
    /// </summary>
    public enum BytePackageEnum
    {
        InitBaseUnit,
        ReadParamsBaseUnit,
        DefaultWriteBaseUnit,
        ReadGeneralParamsIndicator,
        ReadPhaseToPhaseParamsIndicator,
        ReadGroundParamsIndicator,
        DefaultGeneralPackageIndicator,
        DefaultPhaseToPhasePackageIndicator,
        DefaultGroundPackageIndicator
    }
}
