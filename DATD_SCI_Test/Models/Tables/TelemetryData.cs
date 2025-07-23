using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATD_SCI_Test.Models.Tables
{
    /// <summary>
    /// Управление таблицой телеизмерений.
    /// Данные приходят от УСПД, который опрашивает соответствующую фазу (A,B,C).
    /// </summary>
    public class TelemetryData
    {
        /// <summary>
        /// Тип значения
        /// </summary>
        public string Type {  get; set; }

        public string PhaseA { get; set; }
        public string PhaseB { get; set; }
        public string PhaseC { get; set; }

        public static string TypeColumnTitle => "Тип значения";
        public static string PhaseAColumnTitle => "Фаза А";
        public static string PhaseBColumnTitle => "Фаза B";
        public static string PhaseCColumnTitle => "Фаза C";

        public static string GeneralParametersRowTitle => "Общие параметры индикатора";
        public static string GeneralParameter1RowTitle => "Величина электрического поля (в В/м)";
        public static string GeneralParameter2RowTitle => "Минимальное значение тока в линии (в А)";
        public static string GeneralParameter3RowTitle => "Интервал отправки телеизмерений №1 (в с)";
        public static string GeneralParameter4RowTitle => "Интервал отправки телеизмерений №2 (в с)";
        public static string GeneralParameter5RowTitle => "Период отключения датчиков (в мин)";
        public static string GeneralParameter6RowTitle => "Порог значения тока для отправки телеизмерений (в А)";
        public static string GeneralParameter7RowTitle => "Относительное значение изменения тока (в %)";
        public static string GeneralParameter8RowTitle => "Абсолютное значение изменения тока (в А)";


        public static string PhaseToPhaseParametersRowTitle => "Параметры определения межфазного замыкания";
        public static string PhaseToPhaseParameter1RowTitle => "Время перезап. индик. после КЗ на линии (в с)";
        public static string PhaseToPhaseParameter2RowTitle => "Время перезап. индик. после КЗ на линии после перезап. (в с)";
        public static string PhaseToPhaseParameter3RowTitle => "Минимальное значение тока КЗ на линии (в А)";


        public static string GroundParametersRowTitle => "Параметры определения замыкания на землю";
        public static string GroundParameter1RowTitle => "Падение напряжения в линии (в %)"; 
        public static string GroundParameter2RowTitle => "Время засечки падения напряжения (в с)";

        /// <summary>
        /// Установление начальных значений (первая колонка) в таблицу телеизмерений
        /// </summary>
        /// <returns></returns>
        private static List<TelemetryData> CreateData()
        {
            TelemetryData data1_1 = new() { GroupName = "General", Type = GeneralParameter1RowTitle };
            TelemetryData data1_2 = new() { GroupName = "General", Type = GeneralParameter2RowTitle };
            TelemetryData data1_3 = new() { GroupName = "General", Type = GeneralParameter3RowTitle };
            TelemetryData data1_4 = new() { GroupName = "General", Type = GeneralParameter4RowTitle };
            TelemetryData data1_5 = new() { GroupName = "General", Type = GeneralParameter5RowTitle };
            TelemetryData data1_6 = new() { GroupName = "General", Type = GeneralParameter6RowTitle };
            TelemetryData data1_7 = new() { GroupName = "General", Type = GeneralParameter7RowTitle };
            TelemetryData data1_8 = new() { GroupName = "General", Type = GeneralParameter8RowTitle };

            TelemetryData data2_1 = new() { GroupName = "PhaseToPhase", Type = PhaseToPhaseParameter1RowTitle };
            TelemetryData data2_2 = new() { GroupName = "PhaseToPhase", Type = PhaseToPhaseParameter2RowTitle };
            TelemetryData data2_3 = new() { GroupName = "PhaseToPhase", Type = PhaseToPhaseParameter3RowTitle };

            TelemetryData data3_1 = new() { GroupName = "Ground", Type = GroundParameter1RowTitle };
            TelemetryData data3_2 = new() { GroupName = "Ground", Type = GroundParameter2RowTitle };

            return new() { data1_1, data1_2, data1_3, data1_4,
                data1_5, data1_6, data1_7, data1_8,
                data2_1, data2_2, data2_3,data3_1, data3_2};
        }


        /// <summary>
        /// Добавление данных из List<TelemetryData> в ObservableCollection<TelemetryData>
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="telemetryDatas"></param>
        private static void AddNewData(ObservableCollection<TelemetryData> datas, List<TelemetryData> telemetryDatas)
        {
            foreach (TelemetryData telemetryData in telemetryDatas)
                datas.Add(telemetryData);
        }
        
        /// <summary>
        /// Инициализация таблицы телеизмерений
        /// </summary>
        /// <param name="datas"></param>
        public static void Initialize(ObservableCollection<TelemetryData> datas)
        {
            List<TelemetryData> telemetryDatas = CreateData();

            AddNewData(datas, telemetryDatas);
        }

        /// <summary>
        /// Удаление данных, введённых пользователем, из таблицы телеизмерений
        /// </summary>
        /// <param name="datas"></param>
        public static void ClearData(ObservableCollection<TelemetryData> datas)
        {
            //foreach (TelemetryData data in datas)
            //{
            //    data.PhaseA = string.Empty;
            //    data.PhaseB = string.Empty;
            //    data.PhaseC = string.Empty;
            //}
            datas.Clear();
            Initialize(datas);
        }

        /// <summary>
        /// Удаление данных из всех столбцов с невыбранной фазой (чтобы остались данные только по одной фазе)
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="phase"></param>
        public static void ClearDataPhaseChecked(ObservableCollection<TelemetryData> datas, PhaseEnum phase)
        {
            string[] phaseDatas = new string[datas.Count];
            for (int i = 0; i< datas.Count;i++)
            {
                switch(phase)
                {
                    case PhaseEnum.PhaseA:
                        phaseDatas[i] = datas[i].PhaseA;
                        break;

                    case PhaseEnum.PhaseB:
                        phaseDatas[i] = datas[i].PhaseB;
                        break;

                    case PhaseEnum.PhaseC:
                        phaseDatas[i] = datas[i].PhaseC;
                        break;
                }
                 
            }

            datas.Clear();

            List<TelemetryData> newTelemetryDatas = CreateData();

            for (int i = 0;i< newTelemetryDatas.Count;i++)
            {
                switch (phase)
                {
                    case PhaseEnum.PhaseA:
                        newTelemetryDatas[i].PhaseA = phaseDatas[i];
                        break;

                    case PhaseEnum.PhaseB:
                        newTelemetryDatas[i].PhaseB = phaseDatas[i];
                        break;

                    case PhaseEnum.PhaseC:
                        newTelemetryDatas[i].PhaseC = phaseDatas[i];
                        break;
                }

                
            }

            AddNewData(datas, newTelemetryDatas);
        }


        #region TelemetryData Rows - для вёрстки таблицы
        public string GroupName { get; set; }

        public static Dictionary<string, string> GroupTitles = new Dictionary<string, string>
        {
            { "General", GeneralParametersRowTitle },
            { "PhaseToPhase", PhaseToPhaseParametersRowTitle },
            { "Ground", GroundParametersRowTitle }
        };
        #endregion


        /// <summary>
        /// Получение значения по выбранной пользователем фазе
        /// </summary>
        /// <param name="telemetryData"></param>
        /// <param name="phase"></param>
        /// <returns></returns>
        public static string GetValueByPhase(TelemetryData telemetryData, PhaseEnum phase)
        {
            switch (phase)
            {
                case PhaseEnum.PhaseA:
                    return telemetryData.PhaseA;

                case PhaseEnum.PhaseB:
                    return telemetryData.PhaseB;

                case PhaseEnum.PhaseC:
                    return telemetryData.PhaseC;
                default:
                    return string.Empty;
            }

        }

        /// <summary>
        /// Установление значения в соответствии с выбранной пользователем фазой
        /// </summary>
        /// <param name="telemetryData"></param>
        /// <param name="phase"></param>
        /// <param name="value"></param>
        public static void SetValueByPhase(TelemetryData telemetryData, PhaseEnum phase, string value)
        {
            switch (phase)
            {
                case PhaseEnum.PhaseA:
                    telemetryData.PhaseA = value;
                    break;

                case PhaseEnum.PhaseB:
                    telemetryData.PhaseB = value;
                    break;
                case PhaseEnum.PhaseC:
                    telemetryData.PhaseC = value;
                    break;
                default:
                    break;
            }

        }

    }
}
