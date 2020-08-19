using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using WBackup.Data;
using System.Threading;
using System.Windows.Threading;
using System.Threading.Tasks;
using WBackup.BackupWindow;

namespace WBackup
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackupCreator creator;
        private Thread thread;
        private bool active;

        public MainWindow()
        {
            InitializeComponent();
            this.creator = new BackupCreator();
            this.active = true;
            this.thread = new Thread(new ThreadStart(this.DiscListener));
            this.thread.Start();
        }

        private void MnAddFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.ShowDialog();
            try
            {
                this.creator.AddPath(dialog.FileName);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                dialog.Dispose();
            }
        }

        private async void Do_Backup(object sender, RoutedEventArgs e)
        {
            this.lblWait.Content = "Tworzenie kopii zapasowej. Proszę czekać...";
            this.BtnBackup.IsEnabled = false;
            this.BtnRestore.IsEnabled = false;
            this.MnFile.IsEnabled = false;
            this.ComboBoxRemDisc.IsEnabled = false;
            this.creator.BackupDisc = this.ComboBoxRemDisc.SelectedItem.ToString();
            this.creator.PrepareBackupDirPath();
            this.creator.CreateBackupDir();
            await Task.Run(() =>
            {
                if (this.creator.FilePathsExists()) this.creator.LoadPaths();
                try
                {
                    this.creator.DoBackup();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
            this.lblWait.Content = "";
            this.BtnBackup.IsEnabled = true;
            this.BtnRestore.IsEnabled = true;
            this.MnFile.IsEnabled = true;
            this.ComboBoxRemDisc.IsEnabled = true;
        }

        private void End_Click(object sender, RoutedEventArgs e)
        {
            this.active = false;
            this.Close();
        }

        private void MnAddDirs_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            try
            {
                this.creator.AddPath(dialog.SelectedPath);           
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                dialog.Dispose();
            }
        }

        private void DiscListener()
        {
            int counter = 0;
            int itemCount = 0;
            while (this.active)
            {
                DriveInfo[] info = DriveInfo.GetDrives();
                counter = 0;
                this.CountRemDiscs(ref info, ref counter);
                if (itemCount == 0 && counter > 0)
                {
                    this.ComboBoxRemDisc.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate ()
                    {
                        this.InitComboBoxRemDisc(ref info);
                        itemCount = counter;
                    });
                }
                else if(itemCount > 0 && counter > 0)
                {
                    if (itemCount != counter)
                    {
                        this.ComboBoxRemDisc.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate ()
                        {
                            if (this.ComboBoxRemDisc.HasItems) this.ComboBoxRemDisc.Items.Clear();
                            this.InitComboBoxRemDisc(ref info);
                            itemCount = counter;
                        });
                    }
                }
                else if(itemCount > 0 && counter == 0)
                {
                    this.ComboBoxRemDisc.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate ()
                    {
                        if (this.ComboBoxRemDisc.HasItems) this.ComboBoxRemDisc.Items.Clear();
                        itemCount = 0;
                    });
                }              
                info = null;
                Thread.Sleep(2000);
            }
        }

        private void CountRemDiscs(ref DriveInfo[] info, ref int counter)
        {
            for(int i = 0; i < info.Length; i++)
            {
                if (info[i].DriveType == DriveType.Removable || info[i].DriveType==DriveType.Fixed) counter++;
            }
        }

        private void InitComboBoxRemDisc(ref DriveInfo[] info)
        {
            for (int i = 0; i < info.Length; i++)
            {
                if (info[i].DriveType == DriveType.Removable || info[i].DriveType == DriveType.Fixed)
                {
                    this.ComboBoxRemDisc.Items.Add(info[i].Name);
                }
            }
            if (this.ComboBoxRemDisc.HasItems) this.ComboBoxRemDisc.SelectedIndex = 0;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.active = false;
        }

        private async void BtnRestore_Click(object sender, RoutedEventArgs e)
        {
            this.lblWait.Content = "Przywracanie danych. Proszę czekać...";
            this.BtnBackup.IsEnabled = false;
            this.BtnRestore.IsEnabled = false;
            this.MnFile.IsEnabled = false;
            this.ComboBoxRemDisc.IsEnabled = false;
            this.creator.BackupDisc = this.ComboBoxRemDisc.SelectedItem.ToString();
            this.creator.PrepareBackupDirPath();
            await Task.Run(() =>
            {
                try
                {
                    this.creator.Restore();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
            this.lblWait.Content = "";
            this.BtnBackup.IsEnabled = true;
            this.BtnRestore.IsEnabled = true;
            this.MnFile.IsEnabled = true;
            this.ComboBoxRemDisc.IsEnabled = true;
        }

        private void MnLookPaths_Click(object sender, RoutedEventArgs e)
        {
            ConfigFileEditor editor = new ConfigFileEditor();
            editor.Owner = this;
            editor.Show();
        }
    }
}