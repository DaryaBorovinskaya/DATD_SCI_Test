using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATD_SCI_Test.Models.Services.IndicatorParams
{
    /// <summary>
    /// Именное обращение к типам параметров индикатора
    /// </summary>
    public enum IndicatorParamEnum
    {
        /// <summary>
        /// Общие параметры индикатора
        /// </summary>
        General,

        /// <summary>
        /// Параметры определения межфазного замыкания
        /// </summary>
        PhaseToPhase,

        /// <summary>
        /// Параметры определения замыкания на землю
        /// </summary>
        Ground
    }
}
