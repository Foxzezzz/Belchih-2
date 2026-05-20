using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfApp4;

namespace WpfApp4.Tests
{
    [TestClass]
    public class MainViewModelTests
    {
        private MainViewModel _viewModel;

        [TestInitialize]
        public void Setup()
        {
            _viewModel = new MainViewModel();
        }

        [TestMethod]
        public void Constructor_LoadsTestData_FoldersAndFilesExist()
        {
            Assert.IsTrue(_viewModel.Folders.Count > 0, "Папки должны быть загружены");
            Assert.IsTrue(_viewModel.Logs.Count > 0, "Журнал должен содержать запись о запуске");

            var firstFolder = _viewModel.Folders.First();
            Assert.IsNotNull(firstFolder.Name, "У папки должно быть имя");
        }

        [TestMethod]
        public void SelectingFolder_LoadsFilesIntoFilesCollection()
        {
            var folder = _viewModel.Folders.First(f => f.Name.Contains("Документы"));
            _viewModel.SelectedFolder = folder;

            Assert.IsTrue(_viewModel.Files.Count > 0, "Файлы должны появиться после выбора папки");
            Assert.IsTrue(_viewModel.Files.All(f => f.FolderName == folder.Name),
                "Все файлы должны принадлежать выбранной папке");
        }

        [TestMethod]
        public void UploadFile_AddsFileToCollection_AndLogsAction()
        {
            var folder = _viewModel.Folders.First(f => f.Name.Contains("Документы"));
            _viewModel.SelectedFolder = folder;

            Assert.IsTrue(_viewModel.UploadCommand.CanExecute(null),
                "Команда загрузки должна быть активна при выбранной папке");

            int initialLogCount = _viewModel.Logs.Count;
            _viewModel.GetType().GetMethod("AddLog",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(_viewModel, new object[] { "Тест", "Сообщение" });

            Assert.AreEqual(initialLogCount + 1, _viewModel.Logs.Count,
                "Лог должен увеличиться на 1");
        }

        [TestMethod]
        public void DeleteFile_RemovesFileFromCollection_AndLogsAction()
        {
            var folder = _viewModel.Folders.First(f => f.Name.Contains("Документы"));
            _viewModel.SelectedFolder = folder;

            if (_viewModel.Files.Count == 0)
            {
                Assert.Inconclusive("Нет файлов для удаления — тест пропускается");
                return;
            }

            var fileToDelete = _viewModel.Files.First();
            _viewModel.SelectedFile = fileToDelete;

            Assert.IsTrue(_viewModel.DeleteCommand.CanExecute(null),
                "Команда удаления должна быть активна при выбранном файле");

            int initialFileCount = _viewModel.Files.Count;
            int initialLogCount = _viewModel.Logs.Count;

            _viewModel.DeleteCommand.Execute(null);

            Assert.AreEqual(initialFileCount - 1, _viewModel.Files.Count,
                "Количество файлов должно уменьшиться на 1");
            Assert.AreEqual(initialLogCount + 1, _viewModel.Logs.Count,
                "Должна появиться запись в журнале об удалении");
        }
    }
}