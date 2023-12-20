using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ShutdownTimer
{
    public partial class MainWindow : Window
    {
        private CancellationTokenSource cancellationTokenSource;

        public MainWindow()
        {
            InitializeComponent();
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

                if (cancellationTokenSource != null)
                {
                    MessageBox.Show("Таймер уже запущен", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                cancellationTokenSource = new CancellationTokenSource();
                UpdateCountdownText(selectedTime); // Добавляем обновление текста сразу
                StartCountdown(selectedTime);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обработке события кнопки 'Применить': {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void StartCountdown(int countdownMinutes)
        {
            try
            {
                for (int i = countdownMinutes; i >= 0; i--)
                {
                    UpdateCountdownText(i);
                    await Task.Delay(TimeSpan.FromMinutes(1), cancellationTokenSource.Token);
                }

                MessageBox.Show("Выключение компьютера было вызвано. Для демонстрации это действие было отключено.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);

                ResetInterface();
            }
            catch (TaskCanceledException)
            {
                // Таймер отменен пользователем
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обработке таймера: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelShutdownButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cancellationTokenSource != null)
                {
                    cancellationTokenSource.Cancel();
                    cancellationTokenSource = null;
                    MessageBox.Show("Таймер выключения отменен", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Таймер не запущен", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                ResetInterface();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отмене выключения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetInterface()
        {
            cancellationTokenSource = null;
            timeSlider.Value = 1;
            countdownText.Text = "Через сколько выключить?";
        }
        

        private void UpdateCountdownText(int remainingMinutes)
        {
            if (countdownText != null)
            {
                string minutesText;

                if (remainingMinutes % 10 == 1 && remainingMinutes % 100 != 11)
                {
                    minutesText = "минута";
                }
                else if (remainingMinutes % 10 >= 2 && remainingMinutes % 10 <= 4 && (remainingMinutes % 100 < 10 || remainingMinutes % 100 >= 20))
                {
                    minutesText = "минуты";
                }
                else
                {
                    minutesText = "минут";
                }

                countdownText.Text = $"До выключения {remainingMinutes} {minutesText}";
            }
        }

        private void TimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int selectedTime = (int)timeSlider.Value;
            UpdateCountdownText(selectedTime);
        }

    }
}
