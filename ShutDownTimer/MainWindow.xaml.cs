using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ShutdownTimer
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer countdownTimer;
        private int countdownValue;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                Loaded += MainWindow_Loaded;

                countdownTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                countdownTimer.Tick += CountdownTimer_Tick;
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при инициализации окна", ex);
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                int remainingTime = GetRemainingTime();

                UpdateUI(remainingTime);

                // Не запускать таймер выключения при загрузке окна
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при запуске приложения", ex);
            }
        }

        private int GetRemainingTime()
        {
            try
            {
                return 10; // Замените этот код на ваш запрос к PowerShell
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при получении оставшегося времени", ex);
                return 10; // Возвращаем значение по умолчанию в случае ошибки
            }
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                countdownValue--;

                if (countdownValue <= 0)
                {
                    countdownTimer.Stop();
                    ShowShutdownWarning();
                    ResetInterface();
                }

                UpdateUI(countdownValue);
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при обновлении обратного отсчета", ex);
            }
        }

        private void ResetInterface()
        {
            countdownValue = 0;
            UpdateUI(countdownValue);
        }

        private void ShowShutdownWarning()
        {
            MessageBox.Show("Выключение компьютера было вызвано. Для демонстрации это действие было отключено.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void CancelShutdownButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                countdownTimer.Stop();

                RunPowerShellCommand("Stop-Computer -Abort");

                ResetInterface();
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при отмене выключения", ex);
            }
        }

        private void RunPowerShellCommand(string command)
        {
            // Реализация выполнения команды PowerShell
        }

        private void UpdateUI(int value)
        {
            countdownText.Text = $"{value} минут";
            timeSlider.Value = value;
        }

        private void TimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (selectedTimeText != null)
                    selectedTimeText.Text = $"{timeSlider.Value} минут";
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при обновлении текста выбранного времени", ex);
            }
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int selectedTime = (int)timeSlider.Value;

                if (selectedTime <= 0)
                {
                    MessageBox.Show("Выбранное время должно быть больше нуля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                StartCountdown(selectedTime);

                MessageBox.Show("Таймер запущен. По истечении времени будет выведено предупреждение.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при обработке события кнопки 'Применить'", ex);
            }
        }

        private void StartCountdown(int countdownMinutes)
        {
            countdownValue = countdownMinutes;
            countdownTimer.Start();
        }

        private void HandleError(string message, Exception ex)
        {
            MessageBox.Show($"{message}: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
