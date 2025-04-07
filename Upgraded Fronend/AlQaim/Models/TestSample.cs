using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AlQaim.Models // Ensure this namespace matches the folder
{
    public class TestSample : INotifyPropertyChanged
    {
        private string _sampleId;
        private string _patientName;
        private string _testName;
        private DateTime _collectionDateTime;
        private string _status;

        public string SampleId
        {
            get => _sampleId;
            set { _sampleId = value; OnPropertyChanged(); }
        }
        public string PatientName
        {
            get => _patientName;
            set { _patientName = value; OnPropertyChanged(); }
        }
        public string TestName
        {
            get => _testName;
            set { _testName = value; OnPropertyChanged(); }
        }
        public DateTime CollectionDateTime
        {
            get => _collectionDateTime;
            set { _collectionDateTime = value; OnPropertyChanged(); }
        }
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}