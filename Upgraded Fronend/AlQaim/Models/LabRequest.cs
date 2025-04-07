// LabRequest.cs (Located in AlQaim.Models namespace)
using System;
using System.ComponentModel;

namespace AlQaim.Models // Ensure this is the correct namespace
{
    // The class definition starts HERE
    public class LabRequest : INotifyPropertyChanged
    {
        // Backing fields should be INSIDE the class
        private string _status;
        private bool _canApprove;
        private bool _canReject;
        private DateTime _submissionDate;
        private string _urgency;
        private string _adminNotes;

        // --- Properties needed by BOTH Admin and LT ---
        public string RequestId { get; set; }
        public string RequestType { get; set; }
        public string SubmittedBy { get; set; }
        public DateTime SubmissionDate
        {
            get => _submissionDate;
            set { _submissionDate = value; OnPropertyChanged(nameof(SubmissionDate)); }
        }
        public string Description { get; set; }
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(CanCancel)); // Notify CanCancel might change too
                }
            }
        }

        // --- Properties primarily needed by LT (Added) ---
        public DateTime RequestDate { get; set; } // The date selected in the form
        public string Urgency // Using INotifyPropertyChanged in case it's ever needed
        {
            get => _urgency;
            set { _urgency = value; OnPropertyChanged(nameof(Urgency)); }
        }
        public string AttachmentPath { get; set; } // Path to attached file
        public string AdminNotes // Using INotifyPropertyChanged
        {
            get => _adminNotes;
            set { _adminNotes = value; OnPropertyChanged(nameof(AdminNotes)); }
        }

        // --- Properties primarily needed by Admin ---
        public bool CanApprove
        {
            get => _canApprove;
            set
            {
                if (_canApprove != value) { _canApprove = value; OnPropertyChanged(nameof(CanApprove)); }
            }
        }
        public bool CanReject
        {
            get => _canReject;
            set
            {
                if (_canReject != value) { _canReject = value; OnPropertyChanged(nameof(CanReject)); }
            }
        }

        // Calculated property for LT cancel button visibility (retained)
        // This property should be INSIDE the class
        public bool CanCancel => Status == "Pending";

        // --- Detailed Properties (from Admin version, keep if needed for Admin view modal) ---
        // These should be INSIDE the class
        public string PatientId { get; set; }
        public string PatientName { get; set; }
        public string TestType { get; set; }
        public string DoctorNotes { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string CurrentStock { get; set; }
        public string RequestedQuantity { get; set; }
        public string Priority { get; set; }
        public string EquipmentId { get; set; }
        public string EquipmentName { get; set; }
        public string MaintenanceType { get; set; }
        public string LastMaintenance { get; set; }
        public string EquipmentPriority { get; set; }
        public string IssueDescription { get; set; }
        // --- End Detailed Properties ---


        // --- INotifyPropertyChanged Implementation ---
        // This event and method should be INSIDE the class
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    } // The class definition ENDS HERE
} // Namespace ends here