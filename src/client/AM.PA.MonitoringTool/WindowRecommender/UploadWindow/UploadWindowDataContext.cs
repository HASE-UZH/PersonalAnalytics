using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace WindowRecommender.UploadWindow
{
    class UploadWindowDataContext : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IncludeWindowTitles { get; set; }
        public bool IncludeProcessNames { get; set; }

        public bool Ready => !(Generated || Generating);
        public bool Generating { get; private set; }
        public bool Generated { get; private set; }

        public bool HasWindowTitles { get; private set; }
        public bool HasProcessNames { get; private set; }
        public string GeneratedFilePath { get; set; }
        public string GeneratedTimestamp { get; set; }

        public UploadWindowDataContext()
        {
            Generating = false;
            Generated = false;
            IncludeWindowTitles = true;
            IncludeProcessNames = true;
            GeneratedTimestamp = "N/A";
        }

        public void StartGeneration()
        {
            Generating = true;
            OnPropertyChanged(nameof(Generating));
            OnPropertyChanged(nameof(Ready));
        }

        public void EndGeneration(bool hasWindowTitles, bool hasProcessNames, string filePath, DateTime timestamp)
        {
            Generating = false;
            OnPropertyChanged(nameof(Generating));
            Generated = true;
            OnPropertyChanged(nameof(Generated));
            OnPropertyChanged(nameof(Ready));
            HasWindowTitles = hasWindowTitles;
            OnPropertyChanged(nameof(HasWindowTitles));
            HasProcessNames = hasProcessNames;
            OnPropertyChanged(nameof(HasProcessNames));
            GeneratedFilePath = filePath;
            OnPropertyChanged(nameof(GeneratedFilePath));
            GeneratedTimestamp = timestamp.ToString(CultureInfo.InvariantCulture);
            OnPropertyChanged(nameof(GeneratedTimestamp));
        }

        public void DeleteGenerated()
        {
            Generated = false;
            OnPropertyChanged(nameof(Generated));
            OnPropertyChanged(nameof(Ready));
            HasWindowTitles = false;
            OnPropertyChanged(nameof(HasWindowTitles));
            HasProcessNames = false;
            OnPropertyChanged(nameof(HasProcessNames));
            GeneratedFilePath = "";
            OnPropertyChanged(nameof(GeneratedFilePath));
            GeneratedTimestamp = "N/A";
            OnPropertyChanged(nameof(GeneratedTimestamp));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
