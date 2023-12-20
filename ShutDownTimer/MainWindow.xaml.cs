using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Threading;
using System.Windows;

namespace ShutdownTimer
{
    public partial class MainWindow : Window
    {
        private Timer countdownTimer;
        private int countdownValue;
        private CancellationTokenSource cancellationTokenSource;

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                // Добавьте обработчик события загрузки окна
                Loaded += MainWindow_Loaded;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации окна: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получить оставшееся время до выключения при загрузке окна
                int remainingTime = GetRemainingTime();

                // Обновить ползунок и текст с выбранным временем
                if (timeSlider != null)
                    timeSlider.Value = remainingTime;

                if (selectedTimeText != null)
                    selectedTimeText.Text = $"{remainingTime} минут";

                // Не запускать таймер выключения при загрузке окна
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запуске приложения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetRemainingTime()
        {
            try
            {
                using (PowerShell PowerShellInstance = PowerShell.Create())
                {
                    // Запросить оставшееся время до выключения через PowerShell
                    PowerShellInstance.AddScript("(Get-ScheduledTask 'Reboot' | Get-ScheduledTaskInfo).NextRunTime");
                    Collection<PSObject> PSOutput = PowerShellInstance.Invoke();

                    // Проверить, что есть результаты и они не пусты
                    if (PSOutput != null && PSOutput.Count > 0)
                    {
                        // Извлечь значение NextRunTime
                        object nextRunTimeObject = PSOutput[0]?.BaseObject;

                        // Проверить, что объект не равен null и является DateTime
                        if (nextRunTimeObject != null && nextRunTimeObject is DateTime nextRunTime)
                        {
                            // Вычислить оставшееся время в минутах
                            TimeSpan remainingTime = nextRunTime - DateTime.Now;
                            return (int)Math.Ceiling(remainingTime.TotalMinutes);
                        }
                    }

                    // Если не удалось получить время через PowerShell, вернуть значение по умолчанию
                    return 10;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении оставшегося времени: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return 10; // Возвращаем значение по умолчанию в случае ошибки
            }
        }

        private void StartCountdown(int countdownValue)
        {
            try
            {
                // Остановить предыдущий таймер, если он существует
                countdownTimer?.Dispose();

                // Запустить таймер выключения
                cancellationTokenSource = new CancellationTokenSource();
                countdownTimer = new Timer(state => UpdateCountdown(), null, 1000, 1000);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обратном отсчете: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateCountdown()
        {
            try
            {
                if (cancellationTokenSource != null && cancellationTokenSource.Token.IsCancellationRequested)
                {
                    // Отменить таймер выключения
                    countdownTimer.Dispose();

                    // Отобразить сообщение об отмене выключения
                    Dispatcher.Invoke(() => countdownText.Text = "Выключение отменено");
                    return;
                }
                else
                {
                    countdownValue--;

                    // Проверить, достигло ли время нуля
                    if (countdownValue <= 0)
                    {
                        // Остановить таймер
                        countdownTimer.Dispose();

                        // Завершить процесс и выключить компьютер
                        if (cancellationTokenSource != null && !cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            MessageBox.Show("Выключение компьютера было вызвано. Для демонстрации это действие было отключено.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                            //RunPowerShellCommand("Stop-Computer -Force");
                        }

                        // Возвращаемся из метода, чтобы избежать дополнительных обновлений
                        return;
                    }

                    // Обновить текст обратного отсчета в UI
                    Dispatcher.Invoke(() => countdownText.Text = $"{countdownValue} минут");

                    // Обновить ползунок в соответствии с оставшимся временем
                    Dispatcher.Invoke(() => timeSlider.Value = countdownValue);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении обратного отсчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelShutdownButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Отменить таймер выключения
                cancellationTokenSource?.Cancel();

                // Отправить команду отмены выключения в PowerShell
                RunPowerShellCommand("Stop-Computer -Abort");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отмене выключения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RunPowerShellCommand(string command)
        {
            try
            {
                using (PowerShell PowerShellInstance = PowerShell.Create())
                {
                    // Используйте скриптблок для выполнения команды PowerShell
                    PowerShellInstance.AddScript(command);

                    // Запустить скриптблок
                    Collection<PSObject> PSOutput = PowerShellInstance.Invoke();

                    // Вывести любые ошибки
                    foreach (ErrorRecord error in PowerShellInstance.Streams.Error)
                    {
                        MessageBox.Show($"Ошибка PowerShell: {error.Exception.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выполнении PowerShell-команды: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                // Обновить текст с выбранным временем
                if (selectedTimeText != null)
                    selectedTimeText.Text = $"{timeSlider.Value} минут";//
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении текста выбранного времени: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получить значение времени из ползунка
                int selectedTime = (int)timeSlider.Value;

                if (selectedTime <= 0)
                {
                    MessageBox.Show("Выбранное время должно быть больше нуля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Обновить текст с выбранным временем
                selectedTimeText.Text = $"{selectedTime} минут";//

                // Запустить таймер
                StartCountdown(selectedTime);

                // Если необходимо, добавьте вызов для выключения компьютера здесь
                // Закомментируйте следующую строку
                // RunPowerShellCommand("Stop-Computer -Force");

                // Добавьте предупреждающее сообщение
                MessageBox.Show("Выключение компьютера было вызвано. Для демонстрации это действие было отключено.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обработке события кнопки 'Применить': {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
