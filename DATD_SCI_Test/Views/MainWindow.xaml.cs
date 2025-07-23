using DATD_SCI_Test.Models;
using DATD_SCI_Test.Models.Errors;
using DATD_SCI_Test.ViewModels;
using System.Windows;

namespace DATD_SCI_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Не трогать, оставить так, иначе будет грустно
        private MainWindowVM mainWindowVM = new MainWindowVM();

        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = mainWindowVM;
            mainWindowVM.OnStartBlocking += DisableButtons;
            mainWindowVM.OnStopBlocking += EnableButtons;
            mainWindowVM.OnResultConnection += ResultConnectionHandler;
            mainWindowVM.OnMessage += MessageHandler;
            mainWindowVM.OnCompletionTCPconnection += CompletionTCPconnectionHandler;

            
        }

        

        

        /// <summary>
        ///  Настройка таблицы с телеизмерениями.
        /// </summary>
        private void TelemetryDataGrid_Setup()
        {
            telemetryDataGrid.Columns[0].Header = mainWindowVM.TelemetryDataColumnType;
            telemetryDataGrid.Columns[0].IsReadOnly = true;
            telemetryDataGrid.Columns[1].Header = mainWindowVM.TelemetryDataColumnPhaseA;
            telemetryDataGrid.Columns[2].Header = mainWindowVM.TelemetryDataColumnPhaseB;
            telemetryDataGrid.Columns[3].Header = mainWindowVM.TelemetryDataColumnPhaseC;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TelemetryDataGrid_Setup();
        }


        /// <summary>
        /// Отключение кнопок.
        /// Выключает кнопки при передаче данных, 
        /// чтобы пользователь не смог повредить информацию во время передачи/приёма
        /// </summary>
        public void DisableButtons()
        {
            setTCPconnectionButton.IsEnabled = false;
            completionTCPconnectionButton.IsEnabled = false;
            readDataBaseUnit.IsEnabled = false;
            writeDataBaseUnit.IsEnabled = false;
            sendButton.IsEnabled = false;
            readDataIndicator.IsEnabled = false;
            writeDataIndicator.IsEnabled = false;
        }

        /// <summary>
        /// Включение кнопок
        /// </summary>
        private void EnableButtons()
        {
            setTCPconnectionButton.IsEnabled = true;
            completionTCPconnectionButton.IsEnabled = true;
            readDataBaseUnit.IsEnabled = true;
            writeDataBaseUnit.IsEnabled = true;
            sendButton.IsEnabled = true;
            readDataIndicator.IsEnabled = true;
            writeDataIndicator.IsEnabled = true;
        }

        private void ResultConnectionHandler(ExceptionEnum err)
        {
            if (err == ExceptionEnum.None) 
                setTCPconnectionButton.IsEnabled = false;
        }

        private void CompletionTCPconnectionHandler()
        {
            setTCPconnectionButton.IsEnabled = true;
        }

        private void MessageHandler(string message, string caption, MessageBoxImage messageBoxImage)
        { 
             MessageBox.Show(message, caption, MessageBoxButton.OK, messageBoxImage);
        }

        /// <summary>
        /// Обработчик события выбора фазы А
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void phaseAcheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (phaseAcheckBox.IsChecked!.Value == true) 
            { 
                phaseBcheckBox.IsChecked = false;
                phaseCcheckBox.IsChecked = false;
            }
            mainWindowVM.Phase = PhaseEnum.PhaseA;
        }

        /// <summary>
        /// Обработчик события выбора фазы В
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void phaseBcheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (phaseBcheckBox.IsChecked!.Value == true)
            {
                phaseAcheckBox.IsChecked = false;
                phaseCcheckBox.IsChecked = false;
            }
            mainWindowVM.Phase = PhaseEnum.PhaseB;
        }

        /// <summary>
        /// Обработчик события выбора фазы С
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void phaseCcheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (phaseCcheckBox.IsChecked!.Value == true)
            {
                phaseAcheckBox.IsChecked = false;
                phaseBcheckBox.IsChecked = false;
            }
            mainWindowVM.Phase = PhaseEnum.PhaseC;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mainWindowVM.Close();
        }

    }
}