using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using WpfApp4.ViewModel;

namespace WpfApp4.ViewModels
{
    public class FolderItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public override string ToString() => Name;
    }

    public class FileItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public DateTime UploadDate { get; set; }
        public string FolderName { get; set; }
    }

    public class LogEntry
    {
        public string ActionType { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<FolderItem> Folders { get; }
        public ObservableCollection<FileItem> Files { get; }
        public ObservableCollection<LogEntry> Logs { get; }

        private FolderItem _selectedFolder;
        public FolderItem SelectedFolder
        {
            get => _selectedFolder;
            set
            {
                if (_selectedFolder != value)
                {
                    _selectedFolder = value;
                    OnPropertyChanged();
                    LoadFilesForFolder();
                }
            }
        }

        private FileItem _selectedFile;
        public FileItem SelectedFile
        {
            get => _selectedFile;
            set { _selectedFile = value; OnPropertyChanged(); }
        }

        public ICommand UploadCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ShareCommand { get; }
        public ICommand RefreshLogsCommand { get; }
        public ICommand CreateFolderCommand { get; }

        private int _nextFolderId = 4;
        private int _nextFileId = 100;

        public MainViewModel()
        {
            Folders = new ObservableCollection<FolderItem>();
            Files = new ObservableCollection<FileItem>();
            Logs = new ObservableCollection<LogEntry>();

            UploadCommand = new RelayCommand(_ => UploadFile(), _ => SelectedFolder != null);
            DeleteCommand = new RelayCommand(_ => DeleteFile(), _ => SelectedFile != null);
            ShareCommand = new RelayCommand(_ => ShareFile(), _ => SelectedFile != null);
            RefreshLogsCommand = new RelayCommand(_ => RefreshLogs());
            CreateFolderCommand = new RelayCommand(_ => CreateFolder());

            LoadTestData();
            AddLog("Система", "Приложение готово к работе");
        }

        private void LoadTestData()
        {
            Folders.Add(new FolderItem { Id = 1, Name = "📁 Документы" });
            Folders.Add(new FolderItem { Id = 2, Name = "🖼️ Фотографии" });
            Folders.Add(new FolderItem { Id = 3, Name = "💼 Проекты" });

            Files.Add(new FileItem
            {
                Id = 1,
                Name = "Отчет_2024.docx",
                Size = 245760,
                UploadDate = DateTime.Now.AddDays(-2),
                FolderName = "Документы"
            });
            Files.Add(new FileItem
            {
                Id = 2,
                Name = "План_развития.pdf",
                Size = 1048576,
                UploadDate = DateTime.Now.AddDays(-5),
                FolderName = "Документы"
            });

            Logs.Add(new LogEntry
            {
                ActionType = "Система",
                Details = "Загружены тестовые данные",
                Timestamp = DateTime.Now
            });
        }

        private void LoadFilesForFolder()
        {
            Files.Clear();
            if (SelectedFolder == null) return;

            if (SelectedFolder.Name.Contains("Документы"))
            {
                Files.Add(new FileItem { Id = _nextFileId++, Name = "Договор_001.docx", Size = 125000, UploadDate = DateTime.Now, FolderName = SelectedFolder.Name });
                Files.Add(new FileItem { Id = _nextFileId++, Name = "Смета_2024.xlsx", Size = 89000, UploadDate = DateTime.Now.AddDays(-1), FolderName = SelectedFolder.Name });
            }
            else if (SelectedFolder.Name.Contains("Фото"))
            {
                Files.Add(new FileItem { Id = _nextFileId++, Name = "IMG_20240501.jpg", Size = 3145728, UploadDate = DateTime.Now, FolderName = SelectedFolder.Name });
                Files.Add(new FileItem { Id = _nextFileId++, Name = "Vacation.png", Size = 2097152, UploadDate = DateTime.Now.AddDays(-3), FolderName = SelectedFolder.Name });
            }
            else if (SelectedFolder.Name.Contains("Проекты"))
            {
                Files.Add(new FileItem { Id = _nextFileId++, Name = "ProjectAlpha.zip", Size = 15728640, UploadDate = DateTime.Now, FolderName = SelectedFolder.Name });
            }

            AddLog("Навигация", $"Открыта папка: {SelectedFolder.Name}");
        }

        private void UploadFile()
        {
            if (SelectedFolder == null)
            {
                MessageBox.Show("⚠️ Сначала выберите папку!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new OpenFileDialog
            {
                Filter = "Все файлы|*.*",
                Title = "Выберите файл для загрузки"
            };

            if (dialog.ShowDialog() == true)
            {
                var info = new System.IO.FileInfo(dialog.FileName);
                var newFile = new FileItem
                {
                    Id = _nextFileId++,
                    Name = info.Name,
                    Size = info.Length,
                    UploadDate = DateTime.Now,
                    FolderName = SelectedFolder.Name
                };

                Files.Add(newFile);
                AddLog("✅ Загрузка", $"Файл '{newFile.Name}' ({FormatSize(newFile.Size)}) добавлен в '{SelectedFolder.Name}'");
                MessageBox.Show($"Файл загружен!\n{newFile.Name}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteFile()
        {
            if (SelectedFile == null) return;

            var result = MessageBox.Show(
                $"Удалить файл \"{SelectedFile.Name}\"?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var name = SelectedFile.Name;
                Files.Remove(SelectedFile);
                SelectedFile = null;
                AddLog("🗑️ Удаление", $"Файл '{name}' удалён");
            }
        }

        private void ShareFile()
        {
            if (SelectedFile == null) return;

            var link = $"https://mycloud.local/share/{Guid.NewGuid():N}";
            Clipboard.SetText(link);
            AddLog("🔗 Поделиться", $"Ссылка на '{SelectedFile.Name}' скопирована");
            MessageBox.Show($"Ссылка скопирована в буфер:\n{link}", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CreateFolder()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Новая папка",
                Filter = "Папка|*.folder",
                FileName = "Новая папка"
            };

            var inputWindow = new FolderNameWindow();
            if (inputWindow.ShowDialog() == true && !string.IsNullOrWhiteSpace(inputWindow.FolderName))
            {
                var newFolder = new FolderItem
                {
                    Id = _nextFolderId++,
                    Name = "📂 " + inputWindow.FolderName
                };
                Folders.Add(newFolder);
                AddLog("📁 Создано", $"Папка '{newFolder.Name}' добавлена");
            }
        }

        private void RefreshLogs()
        {
            AddLog("🔄 Обновление", "Журнал обновлён");
        }

        private void AddLog(string action, string details)
        {
            Logs.Insert(0, new LogEntry
            {
                ActionType = action,
                Details = details,
                Timestamp = DateTime.Now
            });
            if (Logs.Count > 50) Logs.RemoveAt(Logs.Count - 1);
        }

        private string FormatSize(long bytes)
        {
            string[] sizes = { "Б", "КБ", "МБ", "ГБ" };
            int order = 0;
            double len = bytes;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}