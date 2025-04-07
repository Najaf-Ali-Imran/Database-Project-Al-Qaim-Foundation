using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AlQaim.Models // Ensure this namespace matches the folder
{
    public class TestComponentResult : INotifyPropertyChanged
    {
        private string _componentName;
        private string _value;
        private string _unit;
        private string _referenceRange;
        private string _flag;
        private string _comment;

        public string ComponentName
        {
            get => _componentName;
            set { _componentName = value; OnPropertyChanged(); }
        }
        public string Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged(); }
        }
        public string Unit
        {
            get => _unit;
            set { _unit = value; OnPropertyChanged(); OnPropertyChanged(nameof(ReferenceRangeWithUnits)); }
        }
        public string ReferenceRange
        {
            get => _referenceRange;
            set { _referenceRange = value; OnPropertyChanged(); OnPropertyChanged(nameof(ReferenceRangeWithUnits)); }
        }
        public string Flag
        {
            get => _flag;
            set { _flag = value; OnPropertyChanged(); }
        }
        public string Comment
        {
            get => _comment;
            set { _comment = value; OnPropertyChanged(); }
        }

        public string ReferenceRangeWithUnits => $"{ReferenceRange} {Unit}".Trim();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}