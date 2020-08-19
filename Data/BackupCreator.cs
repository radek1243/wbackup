using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using WBackup.Exceptions;
using WBackup.Config;

namespace WBackup.Data
{
    class BackupCreator
    {
        private StreamReader reader;
        private StreamWriter writer;
        private List<string> copyList;
        private string backupPath;
        private StringBuilder builder;
        private readonly string desktop;
        private readonly string myDocuments;
        public string BackupDisc { get; set; }
        //stworzenie pliku paths.cfg przy pierwszym uruchomieniu -> wymuszenie uruchomienia aplikacji jako administrator -> działa
        //mozliwość podglądu/usunięcia ścieżek z pliku paths.cfg -> śmiga
        //sprawdzanie daty modyfikacji, czy się rózni czy nie?
        //może jakieś ustawienie rozszerzeń?
        public BackupCreator()
        {
            this.reader = null;
            this.copyList = new List<string>();
            this.builder = new StringBuilder();
            this.desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            this.myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            this.BackupDisc = null;
        }

        /// <summary>
        /// Check that file with paths to backup exist
        /// </summary>
        /// <returns>TRUE - if file with paths exist, FALSE otherwise</returns>       
        public bool FilePathsExists()
        {
            return File.Exists(BackupConfig.CFG_FILE_NAME);
        }

        /// <summary>
        /// Load other paths added by user to backup
        /// </summary>
        /// <exception cref="System.Exception">Throws an Exception if error</exception>
        /// <returns>true if paths were read correct</returns>
        public bool LoadPaths()
        {
            try
            {
                this.reader = new StreamReader(BackupConfig.CFG_FILE_NAME);
                string line;
                while ((line = this.reader.ReadLine()) != null)
                {
                    this.copyList.Add(line);
                }
                return true;
            }
            catch (Exception e)
            {
                if (this.reader != null) this.reader.Close();
                throw;
            }
            finally
            {
                if (this.reader != null)
                {
                    this.reader.Close();
                    this.reader = null;
                }
            }
        }

        /// <summary>
        /// Add file or directory path to backup
        /// </summary>
        /// <param name="path">Full path to file or directory on disk</param>
        /// <exception cref="System.Exception">Throw if can't save path to configuration file</exception>
        public void AddPath(string path)
        {
            try
            {
                this.writer = new StreamWriter(BackupConfig.CFG_FILE_NAME, true);
                this.writer.WriteLine(path);                
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                if(this.writer != null) this.writer.Close();
                this.writer = null;
            }
        }

        /// <summary>
        /// Copy file from disk
        /// </summary>
        /// <param name="sourceFile">FileInfo object that represent file on disk</param>
        /// <param name="destPath">Destination path of file to copy</param>
        /// <exception cref="System.Exception">Throw if can't copy file</exception>
        public void CopyFile(FileInfo sourceFile, string destPath)
        {
            try
            {   //jesli plik nie jest sytemowy i ukryty do ko skopiuj i nadpisz
                if (!sourceFile.Attributes.HasFlag(FileAttributes.System) && !sourceFile.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    File.Copy(sourceFile.FullName, destPath, true);
                }
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Prepares wbackup directory path on disk
        /// </summary>
        /// <exception cref="System.Exception">Throw if can't prepare path or backup disc is not choosed</exception>
        public void PrepareBackupDirPath()     //prepare wbackup directory path on disk
        {
            this.PrepareBackupDirPath(this.BackupDisc);
        }

        private void PrepareBackupDirPath(string remDisc)   //prepare wbackup directory path on disk
        {
            if (remDisc == null) throw new MissingDiscException();  //jeśli nie ustawiony dysk to wyjątek
            this.backupPath = Path.Combine(remDisc,"wbackup");  //przygotuj ścieżke wbackup do kopii
        }

        /// <summary>
        /// Creates wbackup directory on disk used to backup
        /// </summary>
        /// <param name="remDisc">Path to disk used to backup</param>
        /// <exception cref="System.Exception">Throw if can't create directory or backup path doesn't exist</exception>
        public void CreateBackupDir(string remDisc)
        {
            try
            {
                if (this.backupPath == null) throw new MissingPathException();  
                Directory.CreateDirectory(this.backupPath);
            }
            catch(Exception ex)
            {
                throw;
            }

        }

        /// <summary>
        /// Creates wbackup directory on disk used to backup
        /// </summary>
        /// <exception cref="System.Exception">Throw if can't create directory</exception>
        public void CreateBackupDir()
        {
            this.CreateBackupDir(this.BackupDisc);
        }

        private string PrepareBackupPath(string fullPath)   //prepare file/directory path to copy to wbackup directory
        {
            return Path.Combine(this.backupPath, fullPath.Replace(":", ""));
        }

        /// <summary>
        /// Creates direcory and all parent directories
        /// </summary>
        /// <param name="fullDirPath">Full directory path</param>
        /// <exception cref="System.Exception">Throw if can't create directory and parent directories</exception>
        private void CreateDir(string fullDirPath)
        {
            try
            {
                Directory.CreateDirectory(fullDirPath);
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        private string PrepareRestorePath(string fullBackupPath)
        {
            if (this.backupPath == null) throw new MissingPathException();  //nie ma ustawionej ścieżki d backupów
            this.builder.Clear();
            this.builder.Append(this.backupPath);   
            this.builder.Append("\\");  //dopisanie ukosnika do ssciezki od backupow
            if(fullBackupPath.Replace(this.builder.ToString(), "").Length == 1) //jesli jest jeden znak czyli katalog root to
            {
                return fullBackupPath.Replace(this.builder.ToString(), "").Insert(1, ":\\");    //dopisz dwukropek i ukosnik
            }
            else
            {
                return fullBackupPath.Replace(this.builder.ToString(), "").Insert(1, ":");      //dopisz tylko dwukropek
            }           
        }

        /// <summary>
        /// Copy directory and all his content if it is not system or hidden directory
        /// </summary>
        /// <param name="dir">DirectorInfo object represents directory to copy</param>
        /// <param name="destPath">Destination path of directory</param>
        /// <exception cref="System.Exception">Throw if can't copy directory and its contents</exception>
        public void CopyDirectory(DirectoryInfo dir, string destPath)
        {
            try
            {
                if (!dir.Attributes.HasFlag(FileAttributes.System) && !dir.Attributes.HasFlag(FileAttributes.Hidden))   //jesli katalog nie jest systemowy i ukryty
                {
                    Directory.CreateDirectory(destPath);    //stworz katalog w katalogu backupowym
                    FileInfo[] files = dir.GetFiles();  //lista plikow
                    DirectoryInfo[] dirs = dir.GetDirectories();    //lista katalogow
                    for (int i = 0; i < files.Length; i++)  //skopiuj pliki
                    {
                        this.CopyFile(files[i], this.PrepareBackupPath(files[i].FullName));
                    }
                    files = null;
                    for (int i = 0; i < dirs.Length; i++)   //skopiuj katalogi
                    {
                        this.CopyDirectory(dirs[i], this.PrepareBackupPath(dirs[i].FullName));
                    }
                    dirs = null;
                }
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Creates backup of specified files and directories
        /// </summary>
        /// <exception cref="WBackup.Exceptions.MissingPathException">Throw if ath to wbackup directory on disk aren't set</exception>
        /// <exception cref="System.Exception">Throw if can't create backup</exception>
        public void DoBackup()
        {
            try
            {
                if (this.backupPath == null) throw new MissingPathException();  //brak przygotowanej sciezki do backupow
                List<string> toDeleteList = new List<string>(); 
                if (this.copyList.Count > 0)
                {
                    for (int i = 0; i < this.copyList.Count; i++)   //kopiowanie plikow i katalogow z pliku paths.cfg jesli istnieja
                    {
                        if (File.Exists(this.copyList.ElementAt(i))) this.CopyFile(new FileInfo(this.copyList.ElementAt(i)), this.PrepareBackupPath(this.copyList.ElementAt(i)));
                        else if (Directory.Exists(this.copyList.ElementAt(i))) this.CopyDirectory(new DirectoryInfo(this.copyList.ElementAt(i)), this.PrepareBackupPath(this.copyList.ElementAt(i)));
                        else
                        {
                            toDeleteList.Add(this.copyList.ElementAt(i));   //jak nie ma pliku to dodaj na liste do usunięcia
                        }
                    }
                    for (int i = 0; i < toDeleteList.Count; i++)
                    {
                        this.copyList.Remove(toDeleteList.ElementAt(i));    //usuwamy z listy aktualnych
                    }
                }
                toDeleteList.Clear();
                this.CopyDirectory(new DirectoryInfo(this.desktop), this.PrepareBackupPath(this.desktop));
                this.CopyDirectory(new DirectoryInfo(this.myDocuments), this.PrepareBackupPath(this.myDocuments));
            }
            catch(Exception e)
            {
                throw;
            }
        }

        /// <summary>
        /// Copy file from backup disc to its original place
        /// </summary>
        /// <param name="fullPath">Full path on backup disc</param>
        /// <param name="destPath">Full path of original place</param>
        /// <exception cref="System.Exception">Throw when can't copy file to original path</exception>
        public void RestoreFile(string fullPath, string destPath)
        {
            try
            {
                if(!File.Exists(destPath)) File.Copy(fullPath, destPath);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private bool IsRoot(string path)    //sprawdzenie czy sciezka jest rootem
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            for(int i = 0; i < drives.Length; i++)
            {
                if (drives[i].Name.Equals(path)) return true;
            }
            drives = null;
            return false;
        }

        /// <summary>
        /// Restore Directory to its original place
        /// </summary>
        /// <param name="fullPath">Full path of directory on backup disc</param>
        /// <exception cref="System.Exception">Throw if can't copy directory to its original place</exception>
        public void RestoreDirectory(string fullPath)
        {
            try
            {
                string restorePath = this.PrepareRestorePath(fullPath);     //przygotuj sciezke oryginalna
                if (!this.IsRoot(restorePath)) {    //jesli nie jest rootem i nie istnieje to go stworz
                    if (!Directory.Exists(restorePath)) Directory.CreateDirectory(restorePath);
                }
                DirectoryInfo info = new DirectoryInfo(fullPath);
                DirectoryInfo[] dirs = info.GetDirectories();
                FileInfo[] files = info.GetFiles();
                for(int i = 0; i < files.Length; i++)   //przywroc pliki
                {
                    this.RestoreFile(files[i].FullName, this.PrepareRestorePath(files[i].FullName));
                }
                for(int i = 0; i < dirs.Length; i++)    //przywroc katalogi
                {
                    this.RestoreDirectory(dirs[i].FullName);
                }
                files = null;
                dirs = null;
                info = null;
                restorePath = null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Restore all files and directories from backup disc
        /// </summary>
        /// <exception cref="System.Exception">Throw if can't restore files and directories</exception>
        public void Restore()
        {
            try
            {
                if (this.backupPath == null) throw new MissingPathException();  //nie ustawiona sciezka do backupow
                if (!Directory.Exists(this.backupPath)) throw new MissingDirectoryException();  //brak folderu z backupami na dysku
                DirectoryInfo info = new DirectoryInfo(this.backupPath);
                DirectoryInfo[] dirs = info.GetDirectories();
                for(int i = 0; i < dirs.Length; i++)    //odzyskiwanie
                {
                    this.RestoreDirectory(dirs[i].FullName);
                }
                dirs = null;
                info = null;
            }
            catch(Exception ex)
            {
                throw;
            }
        }
    }
}
