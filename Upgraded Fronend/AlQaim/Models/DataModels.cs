// DataModels.cs
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media; // For Brush
using FontAwesome.Sharp;    // For IconChar

namespace AlQaim.Models
{
    /// <summary>
    /// Represents a task that can be assigned or viewed. Used by both LT and Admin dashboards.
    /// Implements INotifyPropertyChanged for dynamic UI updates when properties change.
    /// </summary>
    public class AdminTask : INotifyPropertyChanged
    {
        private int _taskId;
        public int TaskId
        {
            get => _taskId;
            set { _taskId = value; OnPropertyChanged(); }
        }

        private string _title;
        public string Title // Using "Title" consistently now
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        private DateTime _dueDate;
        public DateTime DueDate
        {
            get => _dueDate;
            set { _dueDate = value; OnPropertyChanged(); }
        }

        private string _priority; // e.g., "High", "Medium", "Low", "Urgent", "Normal"
        public string Priority
        {
            get => _priority;
            set { _priority = value; OnPropertyChanged(); }
        }

        private string _status; // e.g., "Pending", "In Progress", "Completed"
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        private string _assigneeName; // Relevant for Admin view primarily
        public string AssigneeName
        {
            get => _assigneeName;
            set { _assigneeName = value; OnPropertyChanged(); }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Represents an item in the Activity Feed.
    /// </summary>
    public class ActivityItem
    {
        public IconChar Icon { get; set; }
        public Brush IconForeground { get; set; }
        public Brush IconBackground { get; set; } // Background for the icon container
        public string Description { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }
    }
}