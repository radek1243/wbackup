using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using WBackup.Config;

namespace WBackup.BackupWindow
{
    /// <summary>
    /// Logika interakcji dla klasy ConfigFileEditor.xaml
    /// </summary>
    public partial class ConfigFileEditor : Window
    {

        public ConfigFileEditor()
        {
            InitializeComponent();
            try
            {
                StreamReader reader = new StreamReader(BackupConfig.CFG_FILE_NAME);
                this.TxtBoxContent.Text = reader.ReadToEnd();
                reader.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StreamWriter writer = new StreamWriter(BackupConfig.CFG_FILE_NAME);
                writer.Write(this.TxtBoxContent.Text);
                writer.Close();
                this.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}
