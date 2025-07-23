using DATD_SCI_Test.Models.Tables;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DATD_SCI_WPF
{
    /// <summary>
    /// Группировка строк в таблице телеизмерений (для подзаголовков параметров)
    /// </summary>
    public class GroupTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string groupName && TelemetryData.GroupTitles.TryGetValue(groupName, out var title))
                return title;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
