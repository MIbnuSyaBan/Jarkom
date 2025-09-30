using System;
using System.Linq;
using System.Windows;

namespace Jarkom
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Ganti theme aplikasi (Light / Dark).
        /// Pastikan file ada di folder /Themes dengan nama LightTheme.xaml / DarkTheme.xaml.
        /// </summary>
        public void ChangeTheme(string themeName)
        {
            try
            {
                // Load theme baru
                var dict = new ResourceDictionary
                {
                    Source = new Uri($"/Themes/{themeName}Theme.xaml", UriKind.Relative)
                };

                // Cari theme lama yang aktif
                var oldTheme = Resources.MergedDictionaries
                    .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Theme.xaml"));

                if (oldTheme != null)
                    Resources.MergedDictionaries.Remove(oldTheme);

                // Masukkan theme baru di paling depan
                Resources.MergedDictionaries.Insert(0, dict);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal ganti theme: {ex.Message}");
            }
        }
    }
}
