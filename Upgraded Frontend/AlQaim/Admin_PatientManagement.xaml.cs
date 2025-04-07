using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace AlQaim
{
    public partial class Admin_PatientManagement : Page, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Patient> _patients;
        public ObservableCollection<Patient> Patients
        {
            get { return _patients; }
            set
            {
                _patients = value;
                OnPropertyChanged(nameof(Patients));
            }
        }

        // View for filtering
        private ICollectionView _patientsView;

        // Selected patient for operations
        private Patient _selectedPatient;

        // Flag to track if we're editing or adding
        private bool _isEditMode = false;

        // For deletion confirmation
        private Patient _patientToDelete;

        // Timer for toast notification
        private DispatcherTimer _toastTimer;

        public Admin_PatientManagement()
        {
            InitializeComponent();

            // Initialize collections and load data
            Patients = new ObservableCollection<Patient>();

            // Set DataContext
            this.DataContext = this;

            // Initialize the toast timer
            _toastTimer = new DispatcherTimer();
            _toastTimer.Interval = TimeSpan.FromSeconds(3);
            _toastTimer.Tick += (s, e) =>
            {
                _toastTimer.Stop();
                HideToast();
            };

            // Load sample data for demonstration (replace with actual data loading)
            LoadSampleData();

            // Setup the collection view for filtering
            _patientsView = CollectionViewSource.GetDefaultView(Patients);
            dgPatients.ItemsSource = _patientsView;

            // Initialize date pickers to today's date
            dpRegistrationDate.SelectedDate = DateTime.Today;
            dpFilterFrom.SelectedDate = DateTime.Today.AddMonths(-1);
            dpFilterTo.SelectedDate = DateTime.Today;

            // Set default blood group selection
            cmbFilterBloodGroup.SelectedIndex = 0; // "All"
            if (cmbBloodGroup.Items.Count > 0)
                cmbBloodGroup.SelectedIndex = 0;
        }

        // Property change notification method
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Load sample data for demonstration
        private void LoadSampleData()
        {
            // Add sample patients for the UI demonstration
            Patients.Add(new Patient
            {
                PatientId = 1,
                Name = "Ahmed Khan",
                FatherName = "Muhammad Khan",
                Age = 35,
                BloodGroup = "O+",
                CNIC = "12345-1234567-1",
                Phone = "+92-300-1234567",
                RegistrationDate = DateTime.Today.AddDays(-15),
                MedicalNotes = "Patient has mild hypertension. Advised to reduce salt intake and exercise regularly."
            });

            Patients.Add(new Patient
            {
                PatientId = 2,
                Name = "Sara Ali",
                FatherName = "Ali Ahmed",
                Age = 28,
                BloodGroup = "A+",
                CNIC = "54321-7654321-2",
                Phone = "+92-333-9876543",
                RegistrationDate = DateTime.Today.AddDays(-30),
                MedicalNotes = "Regular checkup, no significant health issues."
            });

            Patients.Add(new Patient
            {
                PatientId = 3,
                Name = "Muhammad Usman",
                FatherName = "Usman Farooq",
                Age = 42,
                BloodGroup = "B-",
                CNIC = "98765-4321098-3",
                Phone = "+92-312-5678901",
                RegistrationDate = DateTime.Today.AddDays(-5),
                MedicalNotes = "Diabetic patient. Needs regular monitoring of blood sugar levels."
            });
        }

        // Event handlers for UI interactions

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            // Toggle filter panel visibility
            filterPanel.Visibility = filterPanel.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            // Reload data (in a real app, this would fetch fresh data from the database)
            // For demo, we'll just clear filters
            txtSearch.Text = string.Empty;
            cmbFilterBloodGroup.SelectedIndex = 0;
            dpFilterFrom.SelectedDate = DateTime.Today.AddMonths(-1);
            dpFilterTo.SelectedDate = DateTime.Today;

            ApplyFilters();
            ShowToast("Data refreshed", PackIconMaterialKind.Refresh);
        }

        private void BtnAddPatient_Click(object sender, RoutedEventArgs e)
        {
            // Prepare the modal for adding a new patient
            _isEditMode = false;
            ClearForm();
            modalTitle.Text = "Add New Patient";
            modalOverlay.Visibility = Visibility.Visible;
        }

        private void BtnView_Click(object sender, RoutedEventArgs e)
        {
            // Get the patient from the button's DataContext
            var button = sender as Button;
            var patient = button.DataContext as Patient;

            if (patient != null)
            {
                DisplayPatientDetails(patient);
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Get the patient from the button's DataContext
            var button = sender as Button;
            var patient = button.DataContext as Patient;

            if (patient != null)
            {
                _selectedPatient = patient;
                _isEditMode = true;
                PopulateForm(patient);
                modalTitle.Text = "Edit Patient";
                modalOverlay.Visibility = Visibility.Visible;
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Get the patient from the button's DataContext
            var button = sender as Button;
            var patient = button.DataContext as Patient;

            if (patient != null)
            {
                _patientToDelete = patient;
                txtConfirmationMessage.Text = $"Are you sure you want to delete the record for {patient.Name}? This action cannot be undone.";
                confirmationOverlay.Visibility = Visibility.Visible;
            }
        }

        private void BtnCloseModal_Click(object sender, RoutedEventArgs e)
        {
            modalOverlay.Visibility = Visibility.Collapsed;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            modalOverlay.Visibility = Visibility.Collapsed;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validate form
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                ShowToast("Name is required", PackIconMaterialKind.Alert);
                return;
            }

            if (_isEditMode && _selectedPatient != null)
            {
                // Update existing patient
                _selectedPatient.Name = txtName.Text;
                _selectedPatient.FatherName = txtFatherName.Text;

                if (int.TryParse(txtAge.Text, out int age))
                    _selectedPatient.Age = age;

                _selectedPatient.BloodGroup = (cmbBloodGroup.SelectedItem as ComboBoxItem)?.Content.ToString();
                _selectedPatient.CNIC = txtCNIC.Text;
                _selectedPatient.Phone = txtPhone.Text;
                _selectedPatient.RegistrationDate = dpRegistrationDate.SelectedDate ?? DateTime.Today;
                _selectedPatient.MedicalNotes = txtMedicalNotes.Text;

                ShowToast("Patient updated successfully", PackIconMaterialKind.Check);
            }
            else
            {
                // Add new patient
                var newPatient = new Patient
                {
                    PatientId = Patients.Count > 0 ? Patients.Max(p => p.PatientId) + 1 : 1,
                    Name = txtName.Text,
                    FatherName = txtFatherName.Text,
                    Age = int.TryParse(txtAge.Text, out int age) ? age : 0,
                    BloodGroup = (cmbBloodGroup.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    CNIC = txtCNIC.Text,
                    Phone = txtPhone.Text,
                    RegistrationDate = dpRegistrationDate.SelectedDate ?? DateTime.Today,
                    MedicalNotes = txtMedicalNotes.Text
                };

                Patients.Add(newPatient);
                ShowToast("Patient added successfully", PackIconMaterialKind.Check);
            }

            // Refresh the view
            _patientsView.Refresh();

            // Close the modal
            modalOverlay.Visibility = Visibility.Collapsed;
        }

        private void BtnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
            filterPanel.Visibility = Visibility.Collapsed;
        }

        private void BtnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            cmbFilterBloodGroup.SelectedIndex = 0;
            dpFilterFrom.SelectedDate = DateTime.Today.AddMonths(-1);
            dpFilterTo.SelectedDate = DateTime.Today;

            ApplyFilters();
        }

        private void BtnCloseDetailsModal_Click(object sender, RoutedEventArgs e)
        {
            detailsModalOverlay.Visibility = Visibility.Collapsed;
        }

        private void BtnCloseDetails_Click(object sender, RoutedEventArgs e)
        {
            detailsModalOverlay.Visibility = Visibility.Collapsed;
        }

        private void BtnEditFromDetails_Click(object sender, RoutedEventArgs e)
        {
            // Close details view and open edit form
            detailsModalOverlay.Visibility = Visibility.Collapsed;

            if (_selectedPatient != null)
            {
                _isEditMode = true;
                PopulateForm(_selectedPatient);
                modalTitle.Text = "Edit Patient";
                modalOverlay.Visibility = Visibility.Visible;
            }
        }

        private void BtnCancelConfirmation_Click(object sender, RoutedEventArgs e)
        {
            confirmationOverlay.Visibility = Visibility.Collapsed;
            _patientToDelete = null;
        }

        private void BtnConfirmAction_Click(object sender, RoutedEventArgs e)
        {
            if (_patientToDelete != null)
            {
                Patients.Remove(_patientToDelete);
                ShowToast("Patient deleted successfully", PackIconMaterialKind.Delete);
                _patientToDelete = null;
            }

            confirmationOverlay.Visibility = Visibility.Collapsed;
        }

        private void BtnCloseToast_Click(object sender, RoutedEventArgs e)
        {
            HideToast();
        }

        private void DgPatients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedPatient = dgPatients.SelectedItem as Patient;
        }

        // Helper methods

        private void ApplyFilters()
        {
            if (_patientsView != null)
            {
                _patientsView.Filter = item =>
                {
                    var patient = item as Patient;
                    if (patient == null) return false;

                    bool matchesSearch = true;
                    if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                    {
                        string searchText = txtSearch.Text.ToLower();
                        matchesSearch = patient.Name.ToLower().Contains(searchText) ||
                                        patient.CNIC.ToLower().Contains(searchText) ||
                                        patient.Phone.ToLower().Contains(searchText);
                    }

                    bool matchesBloodGroup = true;
                    var selectedBloodGroup = (cmbFilterBloodGroup.SelectedItem as ComboBoxItem)?.Content.ToString();
                    if (selectedBloodGroup != "All" && !string.IsNullOrEmpty(selectedBloodGroup))
                    {
                        matchesBloodGroup = patient.BloodGroup == selectedBloodGroup;
                    }

                    bool matchesDateRange = true;
                    if (dpFilterFrom.SelectedDate.HasValue && dpFilterTo.SelectedDate.HasValue)
                    {
                        DateTime fromDate = dpFilterFrom.SelectedDate.Value.Date;
                        DateTime toDate = dpFilterTo.SelectedDate.Value.Date.AddDays(1).AddSeconds(-1); // End of the selected day

                        matchesDateRange = patient.RegistrationDate >= fromDate && patient.RegistrationDate <= toDate;
                    }

                    return matchesSearch && matchesBloodGroup && matchesDateRange;
                };
            }
        }

        private void ClearForm()
        {
            txtName.Text = string.Empty;
            txtFatherName.Text = string.Empty;
            txtAge.Text = string.Empty;
            if (cmbBloodGroup.Items.Count > 0)
                cmbBloodGroup.SelectedIndex = 0;
            txtCNIC.Text = string.Empty;
            txtPhone.Text = string.Empty;
            dpRegistrationDate.SelectedDate = DateTime.Today;
            txtMedicalNotes.Text = string.Empty;
        }

        private void PopulateForm(Patient patient)
        {
            if (patient != null)
            {
                txtName.Text = patient.Name;
                txtFatherName.Text = patient.FatherName;
                txtAge.Text = patient.Age.ToString();

                // Find and select the appropriate blood group
                foreach (ComboBoxItem item in cmbBloodGroup.Items)
                {
                    if (item.Content.ToString() == patient.BloodGroup)
                    {
                        cmbBloodGroup.SelectedItem = item;
                        break;
                    }
                }

                txtCNIC.Text = patient.CNIC;
                txtPhone.Text = patient.Phone;
                dpRegistrationDate.SelectedDate = patient.RegistrationDate;
                txtMedicalNotes.Text = patient.MedicalNotes;
            }
        }

        private void DisplayPatientDetails(Patient patient)
        {
            if (patient != null)
            {
                _selectedPatient = patient;

                // Populate the details view
                txtDetailsName.Text = patient.Name;
                txtDetailsId.Text = $"ID: {patient.PatientId}";
                txtDetailsFatherName.Text = patient.FatherName;
                txtDetailsAge.Text = patient.Age.ToString();
                txtDetailsBloodGroup.Text = patient.BloodGroup;
                txtDetailsCNIC.Text = patient.CNIC;
                txtDetailsPhone.Text = patient.Phone;
                txtDetailsRegistrationDate.Text = patient.RegistrationDate.ToString("dd-MM-yyyy");
                txtDetailsMedicalNotes.Text = string.IsNullOrEmpty(patient.MedicalNotes)
                    ? "No medical notes available."
                    : patient.MedicalNotes;

                // Show the details modal
                detailsModalOverlay.Visibility = Visibility.Visible;
            }
        }

        private void ShowToast(string message, PackIconMaterialKind iconKind)
        {
            toastMessage.Text = message;
            toastIcon.Kind = iconKind;

            // Show the toast
            toastNotification.Visibility = Visibility.Visible;

            // Create a fade-in animation
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            toastNotification.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            // Start the timer to auto-hide
            _toastTimer.Start();
        }

        private void HideToast()
        {
            // Create a fade-out animation
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            fadeOut.Completed += (s, e) => toastNotification.Visibility = Visibility.Collapsed;
            toastNotification.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        // Export functionality
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            // This would be connected to export functionality (CSV/PDF)
            // For demonstration purposes, we'll just show a toast
            ShowToast("Export functionality will be implemented here", PackIconMaterialKind.FileExport);
        }

        // Method to validate CNIC format
        private bool IsValidCNIC(string cnic)
        {
            // Basic CNIC validation (Pakistan format: 12345-1234567-1)
            return System.Text.RegularExpressions.Regex.IsMatch(cnic, @"^\d{5}-\d{7}-\d{1}$");
        }

        // Method to validate phone number format
        private bool IsValidPhone(string phone)
        {
            // Basic phone validation for Pakistan format
            return System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\+92-\d{3}-\d{7}$");
        }

        // Method to perform full form validation
        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                ShowToast("Name is required", PackIconMaterialKind.Alert);
                txtName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtFatherName.Text))
            {
                ShowToast("Father's name is required", PackIconMaterialKind.Alert);
                txtFatherName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtAge.Text) || !int.TryParse(txtAge.Text, out _))
            {
                ShowToast("Valid age is required", PackIconMaterialKind.Alert);
                txtAge.Focus();
                return false;
            }

            if (cmbBloodGroup.SelectedIndex < 0)
            {
                ShowToast("Blood group is required", PackIconMaterialKind.Alert);
                cmbBloodGroup.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtCNIC.Text) || !IsValidCNIC(txtCNIC.Text))
            {
                ShowToast("Valid CNIC is required (format: 12345-1234567-1)", PackIconMaterialKind.Alert);
                txtCNIC.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPhone.Text) || !IsValidPhone(txtPhone.Text))
            {
                ShowToast("Valid phone is required (format: +92-300-1234567)", PackIconMaterialKind.Alert);
                txtPhone.Focus();
                return false;
            }

            if (!dpRegistrationDate.SelectedDate.HasValue)
            {
                ShowToast("Registration date is required", PackIconMaterialKind.Alert);
                dpRegistrationDate.Focus();
                return false;
            }

            return true;
        }

        // Method to format CNIC on input
        private void TxtCNIC_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                // Remove all non-digit characters
                string digitsOnly = new string(textBox.Text.Where(char.IsDigit).ToArray());

                // Format as 12345-1234567-1
                if (digitsOnly.Length > 0)
                {
                    string formatted = "";

                    // Add first section (5 digits)
                    if (digitsOnly.Length > 5)
                    {
                        formatted = digitsOnly.Substring(0, 5) + "-";

                        // Add second section (7 digits)
                        if (digitsOnly.Length > 12)
                        {
                            formatted += digitsOnly.Substring(5, 7) + "-" + digitsOnly.Substring(12, 1);
                        }
                        else if (digitsOnly.Length > 5)
                        {
                            formatted += digitsOnly.Substring(5);
                        }
                    }
                    else
                    {
                        formatted = digitsOnly;
                    }

                    // Update text without triggering event again
                    textBox.TextChanged -= TxtCNIC_TextChanged;
                    textBox.Text = formatted;
                    textBox.CaretIndex = formatted.Length;
                    textBox.TextChanged += TxtCNIC_TextChanged;
                }
            }
        }

        // Method to format phone number on input
        private void TxtPhone_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                // Remove all non-digit characters
                string digitsOnly = new string(textBox.Text.Where(char.IsDigit).ToArray());

                // Format as +92-300-1234567
                if (digitsOnly.Length > 0)
                {
                    string formatted = "+";

                    // Country code
                    if (digitsOnly.Length >= 2)
                    {
                        formatted += digitsOnly.Substring(0, 2);

                        // Add first hyphen and area code
                        if (digitsOnly.Length > 2)
                        {
                            formatted += "-";

                            if (digitsOnly.Length >= 5)
                            {
                                formatted += digitsOnly.Substring(2, 3);

                                // Add second hyphen and remaining digits
                                if (digitsOnly.Length > 5)
                                {
                                    formatted += "-" + digitsOnly.Substring(5);
                                }
                            }
                            else
                            {
                                formatted += digitsOnly.Substring(2);
                            }
                        }
                    }
                    else
                    {
                        formatted += digitsOnly;
                    }

                    // Update text without triggering event again
                    textBox.TextChanged -= TxtPhone_TextChanged;
                    textBox.Text = formatted;
                    textBox.CaretIndex = formatted.Length;
                    textBox.TextChanged += TxtPhone_TextChanged;
                }
            }
        }

        // Age validation to ensure only numbers are entered
        private void TxtAge_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Allow only digits
            e.Handled = !char.IsDigit(e.Text[0]);
        }

        // Ensure registration date is not in the future
        private void DpRegistrationDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpRegistrationDate.SelectedDate.HasValue && dpRegistrationDate.SelectedDate.Value > DateTime.Today)
            {
                ShowToast("Registration date cannot be in the future", PackIconMaterialKind.CalendarAlert);
                dpRegistrationDate.SelectedDate = DateTime.Today;
            }
        }

        // Print patient record
        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            // This would integrate with printing functionality
            if (_selectedPatient != null)
            {
                ShowToast("Print functionality will be implemented here", PackIconMaterialKind.Printer);
            }
            else
            {
                ShowToast("Please select a patient to print", PackIconMaterialKind.Alert);
            }
        }

        // Navigate to medical history
        private void BtnMedicalHistory_Click(object sender, RoutedEventArgs e)
        {
            // This would navigate to a detailed medical history page for the selected patient
            if (_selectedPatient != null)
            {
                ShowToast($"Navigating to medical history for {_selectedPatient.Name}", PackIconMaterialKind.History);
                // Actual navigation would be implemented here
            }
            else
            {
                ShowToast("Please select a patient to view medical history", PackIconMaterialKind.Alert);
            }
        }

        // Exit page
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            // This would navigate back to the main dashboard or previous page
            // For demo, we'll just show a toast
            ShowToast("Returning to dashboard", PackIconMaterialKind.ExitToApp);
        }

        // This method would be called when the page is being unloaded
        private void SaveChanges()
        {
            // In a real application, this would save any pending changes to the database
            // For the demo, we'll assume all changes are immediately saved
        }

        // Page unloaded event handler
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // Cleanup resources and save changes if needed
            SaveChanges();

            // Stop any active timers
            if (_toastTimer.IsEnabled)
            {
                _toastTimer.Stop();
            }
        }

        // Helper method to handle database operations (simulated)
        private void SavePatientToDatabase(Patient patient)
        {
            // In real implementation, this would save the patient to a database
            // For demo purposes, changes are made to the observable collection directly

            // Simulate a delay as if saving to database
            // In real implementation, this would be an async method
            System.Threading.Thread.Sleep(200);
        }

        // Helper method to load patients from database (simulated)
        private void LoadPatientsFromDatabase()
        {
            // In real implementation, this would fetch patients from a database
            // For demo purposes, we use the sample data

            // Simulate a delay as if loading from database
            System.Threading.Thread.Sleep(500);

            // Sample data is already loaded in LoadSampleData method
        }

        // Helper method for pagination (if implemented)
        private void SetupPagination(int totalRecords, int pageSize)
        {
            // Calculate total pages
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            // Setup pagination controls (not implemented in this demo)
            // This would typically involve creating page navigation buttons
        }

        // Method to dispose resources when page is no longer needed
        public void Dispose()
        {
            // Clean up resources
            if (_toastTimer != null)
            {
                _toastTimer.Stop();
                _toastTimer = null;
            }

            // Other cleanup as needed
        }
        public class Patient : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private int _patientId;
            public int PatientId
            {
                get { return _patientId; }
                set
                {
                    _patientId = value;
                    OnPropertyChanged(nameof(PatientId));
                }
            }

            private string _name;
            public string Name
            {
                get { return _name; }
                set
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }

            private string _fatherName;
            public string FatherName
            {
                get { return _fatherName; }
                set
                {
                    _fatherName = value;
                    OnPropertyChanged(nameof(FatherName));
                }
            }

            private int _age;
            public int Age
            {
                get { return _age; }
                set
                {
                    _age = value;
                    OnPropertyChanged(nameof(Age));
                }
            }

            private string _bloodGroup;
            public string BloodGroup
            {
                get { return _bloodGroup; }
                set
                {
                    _bloodGroup = value;
                    OnPropertyChanged(nameof(BloodGroup));
                }
            }

            private string _cnic;
            public string CNIC
            {
                get { return _cnic; }
                set
                {
                    _cnic = value;
                    OnPropertyChanged(nameof(CNIC));
                }
            }

            private string _phone;
            public string Phone
            {
                get { return _phone; }
                set
                {
                    _phone = value;
                    OnPropertyChanged(nameof(Phone));
                }
            }

            private DateTime _registrationDate;
            public DateTime RegistrationDate
            {
                get { return _registrationDate; }
                set
                {
                    _registrationDate = value;
                    OnPropertyChanged(nameof(RegistrationDate));
                }
            }

            private string _medicalNotes;
            public string MedicalNotes
            {
                get { return _medicalNotes; }
                set
                {
                    _medicalNotes = value;
                    OnPropertyChanged(nameof(MedicalNotes));
                }
            }

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}