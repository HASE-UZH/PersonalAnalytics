// Created by Jan Pilzer
// Created: 2019-07-04
//
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows;
using Shared.Data;

namespace WindowRecommender.UploadWindow
{
    public partial class UploadWizard
    {
        private string _generatedFilePath;
        private string _generatedFolderPath;

        public UploadWizard()
        {
            InitializeComponent();
        }

        private void RawClicked(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", Shared.Settings.ExportFilePath);
        }

        private void GenerateClicked(object sender, RoutedEventArgs e)
        {
            var dataContext = (UploadWindowDataContext)DataContext;
            var hasWindowTitles = dataContext.IncludeWindowTitles;
            var hasProcessNames = dataContext.IncludeProcessNames;
            dataContext.StartGeneration();

            var timestamp = DateTime.Now;
            var fileNameBase = $"pa_{timestamp.ToFileTime()}";
            var tempDbDirectoryPath = Path.Combine(Path.GetTempPath(), fileNameBase);
            var databaseCopyPath = Path.Combine(tempDbDirectoryPath, $"{fileNameBase}.dat");
            var databasePath = Database.GetLocalDatabaseSavePath();
            _generatedFolderPath = Path.Combine(Path.GetTempPath(), $"{fileNameBase}-zip");
            _generatedFilePath = Path.Combine(_generatedFolderPath, $"{fileNameBase}.zip");

            Directory.CreateDirectory(tempDbDirectoryPath);
            File.Copy(databasePath, databaseCopyPath, true);

            if (!(hasWindowTitles && hasProcessNames))
            {
                var anonDb = new DatabaseImplementation(databaseCopyPath);
                anonDb.Connect();
                var query = @"DROP TABLE IF EXISTS windows_activity;";
                anonDb.ExecuteDefaultQuery(query);
                if (!hasWindowTitles)
                {
                    query = $@"UPDATE {Settings.WindowEventTable} SET windowTitle = '';";
                    anonDb.ExecuteDefaultQuery(query);
                }
                if (!hasProcessNames)
                {
                    query = $@"UPDATE {Settings.WindowEventTable} SET processName = '';";
                    anonDb.ExecuteDefaultQuery(query);
                }
                anonDb.Disconnect();
            }

            Directory.CreateDirectory(_generatedFolderPath);
            ZipFile.CreateFromDirectory(tempDbDirectoryPath, _generatedFilePath);

            dataContext.EndGeneration(hasWindowTitles, hasProcessNames, _generatedFilePath, timestamp);
        }

        private void GeneratedClicked(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", _generatedFilePath);
        }

        private void SubmitClicked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Please add the .zip archive to the email as an attachment.", "Results Email", MessageBoxButton.OK, MessageBoxImage.Information);
            Process.Start("explorer.exe", _generatedFolderPath);
            Process.Start($"mailto:pilzer@cs.ubc.ca?subject=WindowDimmer Results&body=See attachment&attachment={_generatedFilePath}&Attach={_generatedFilePath}");
        }

        private void DeleteClicked(object sender, RoutedEventArgs e)
        {
            var dataContext = (UploadWindowDataContext)DataContext;
            dataContext.DeleteGenerated();
        }
    }
}

