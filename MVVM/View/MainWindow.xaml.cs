using Jarkom.MVVM.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls; // biar Button kebaca

namespace Jarkom
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isDark = true; // default theme Dark

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void ButtonMaximize_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState =
                Application.Current.MainWindow.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void ButtonClose_CLick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // 🔥 Toggle Theme Button
        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;

            if (isDark)
            {
                ((App)Application.Current).ChangeTheme("Light");
                btn.Content = "🌙"; // ubah jadi moon
                isDark = false;
            }
            else
            {
                ((App)Application.Current).ChangeTheme("Dark");
                btn.Content = "☀"; // ubah jadi sun
                isDark = true;
            }
        }
    }
}
