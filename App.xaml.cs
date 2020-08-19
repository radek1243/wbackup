using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using WBackup.Config;

namespace WBackup
{
    /// <summary>
    /// Logika interakcji dla klasy App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            try
            {
                if (!File.Exists(BackupConfig.CFG_FILE_NAME)) File.Create(BackupConfig.CFG_FILE_NAME);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd przy tworzeniu pliku konfiguracyjnego.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
