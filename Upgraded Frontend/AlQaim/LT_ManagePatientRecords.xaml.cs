using MahApps.Metro.IconPacks;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace AlQaim
{
    public partial class LT_ManagePatientRecords : Page, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Fields & Properties
        private ObservableCollection<Patient> _patients;
        public ObservableCollection<Patient> Patients
        {
            get { return _patients; }
            set { _patients = value; OnPropertyChanged(nameof(Patients)); }
        }

        private ICollectionView _patientsView;
        public ICollectionView PatientsView // Property for DataGrid binding
        {
            get { return _patientsView; }
            set { _patientsView = value; OnPropertyChanged(nameof(PatientsView)); }
        }

        private Patient _selectedPatient; // Currently selected/viewed/edited patient
        private DispatcherTimer _toastTimer;
        private DispatcherTimer _filterPanelTimer; // For filter panel animation
        private string _originalPhone; // To track changes in edit modal
        private string _originalMedicalNotes; // To track changes in edit modal

        #endregion

        public LT_ManagePatientRecords()
        {
            InitializeComponent();
            this.DataContext = this;

            Patients = new ObservableCollection<Patient>();

            // Setup the collection view for filtering/sorting
            PatientsView = CollectionViewSource.GetDefaultView(Patients);
            PatientsView.Filter = null; // Initially show all

            // Initialize timers
            InitializeTimers();

            // Initialize UI elements
            InitializeUI();

            // Load data (replace with actual DB call)
            LoadSampleData();
        }

        #region Initialization
        private void InitializeTimers()
        {
            _toastTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _toastTimer.Tick += (s, e) => HideToast();

            _filterPanelTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10) }; // Short interval for smooth animation
            _filterPanelTimer.Tick += AnimateFilterPanel;
        }

        private void InitializeUI()
        {
            // Set default filter dates
            dpFilterFrom.SelectedDate = DateTime.Today.AddMonths(-1);
            dpFilterTo.SelectedDate = DateTime.Today;
            cmbFilterBloodGroup.SelectedIndex = 0; // "All"
        }
        #endregion

        #region Data Loading
        private void LoadSampleData()
        {
            // Clear existing data first
            Patients.Clear();

            // Add sample patients for the UI demonstration
            Patients.Add(new Patient
            {
                PatientId = "LT-001", // Using string ID to match Patient class potentially used elsewhere
                Name = "Ahmed Khan (LT)",
                FatherName = "Zafar Khan",
                Age = 35,
                BloodGroup = "A+",
                CNIC = "34101-1234567-8",
                Phone = "0300-1234567",
                RegistrationDate = DateTime.Now.AddDays(-30),
                MedicalNotes = "Patient presented with mild fever and cough. Prescribed antibiotics. Needs follow-up blood test."
            });

            Patients.Add(new Patient
            {
                PatientId = "LT-002",
                Name = "Sara Ahmed (LT)",
                FatherName = "Imran Ahmed",
                Age = 28,
                BloodGroup = "O-",
                CNIC = "34101-7654321-0",
                Phone = "0333-7654321",
                RegistrationDate = DateTime.Now.AddDays(-15),
                MedicalNotes = "Regular checkup. Blood tests show normal values. Lab results attached."
            });

            Patients.Add(new Patient
            {
                PatientId = "LT-003",
                Name = "Zain Ali (LT)",
                FatherName = "Asad Ali",
                Age = 42,
                BloodGroup = "B+",
                CNIC = "34101-9876543-2",
                Phone = "0321-9876543",
                RegistrationDate = DateTime.Now.AddDays(-7),
                MedicalNotes = "Patient complains of joint pain. Recommended physical therapy and pain medication. Further tests required."
            });

            // Add more sample data as needed
            for (int i = 4; i <= 10; i++)
            {
                Patients.Add(new Patient
                {
                    PatientId = $"LT-00{i}",
                    Name = $"Sample Patient {i}",
                    FatherName = $"Sample Father {i}",
                    Age = 30 + i,
                    BloodGroup = "AB+",
                    CNIC = $"34101-{i}654321-{i}",
                    Phone = $"0300-{i}654321",
                    RegistrationDate = DateTime.Now.AddDays(-i),
                    MedicalNotes = $"Sample medical notes for patient {i}. Lab test results pending."
                });
            }

            // Refresh the view after loading data
            PatientsView.Refresh();
        }

        // Placeholder for actual database loading
        private void LoadDataFromDatabase()
        {
            ShowToast("Loading data from database...", PackIconMaterialKind.DatabaseSyncOutline, isError: false, durationSeconds: 2);
            try
            {
                // Simulate DB Load Delay
                var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                timer.Tick += (sender, args) =>
                {
                    timer.Stop();
                    Patients.Clear(); // Clear existing data

                    // --- Replace with actual MySQL connection and query ---
                    // Example: LoadSampleData(); // For now, just reload sample
                    LoadSampleData();
                    // --- End DB Call Placeholder ---

                    PatientsView.Refresh();
                    ShowToast("Data loaded successfully", PackIconMaterialKind.DatabaseCheckOutline);
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                ShowToast($"Error loading data: {ex.Message}", PackIconMaterialKind.AlertCircleOutline, isError: true, durationSeconds: 5);
                // Consider logging the full exception details
            }
        }
        #endregion

        #region Event Handlers

        // --- Header & Search/Filter ---
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            ToggleFilterPanel();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            // Clear filters and search, then reload data
            txtSearch.Text = string.Empty;
            cmbFilterBloodGroup.SelectedIndex = 0;
            dpFilterFrom.SelectedDate = DateTime.Today.AddMonths(-1);
            dpFilterTo.SelectedDate = DateTime.Today;

            LoadDataFromDatabase(); // Or LoadSampleData() for testing
            // ApplyFilters(); // LoadData already refreshes view
            ShowToast("Data refreshed", PackIconMaterialKind.Refresh);
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for Export Functionality
            ShowToast("Export functionality not yet implemented.", PackIconMaterialKind.FileExportOutline, isError: false, durationSeconds: 4);
            // Future implementation: Export data in dgPatients.ItemsSource (filtered view) to CSV/Excel.
        }


        // --- Filter Panel ---
        private void BtnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
            ToggleFilterPanel(forceHide: true);
        }

        private void BtnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            cmbFilterBloodGroup.SelectedIndex = 0;
            dpFilterFrom.SelectedDate = DateTime.Today.AddMonths(-1);
            dpFilterTo.SelectedDate = DateTime.Today;
            ApplyFilters(); // Re-apply to reflect cleared filters
        }

        // --- DataGrid Actions ---
        private void DgPatients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedPatient = dgPatients.SelectedItem as Patient;
        }

        private void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Patient patient)
            {
                _selectedPatient = patient;
                DisplayPatientDetails();
            }
        }

        private void BtnEditLimited_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Patient patient)
            {
                _selectedPatient = patient;
                PopulateLimitedEditForm();
                ShowModal(editModalOverlay);
            }
        }

        // --- View Details Modal ---
        private void BtnCloseDetailsModal_Click(object sender, RoutedEventArgs e)
        {
            HideModal(detailsModalOverlay);
        }

        private void BtnCloseDetails_Click(object sender, RoutedEventArgs e)
        {
            HideModal(detailsModalOverlay);
        }

        // --- Limited Edit Modal ---
        private void BtnCloseEditModal_Click(object sender, RoutedEventArgs e)
        {
            HideModal(editModalOverlay);
            // Optional: Revert changes if needed, though typically cancel handles this
            if (_selectedPatient != null)
            {
                txtEditPhone.Text = _originalPhone ?? _selectedPatient.Phone;
                txtEditMedicalNotes.Text = _originalMedicalNotes ?? _selectedPatient.MedicalNotes;
            }
        }

        private void BtnCancelEdit_Click(object sender, RoutedEventArgs e)
        {
            // Revert UI fields to original values before hiding
            if (_selectedPatient != null)
            {
                txtEditPhone.Text = _originalPhone ?? _selectedPatient.Phone;
                txtEditMedicalNotes.Text = _originalMedicalNotes ?? _selectedPatient.MedicalNotes;
            }
            HideModal(editModalOverlay);
        }

        private void BtnSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPatient == null) return;

            // Simple validation (can be expanded)
            if (string.IsNullOrWhiteSpace(txtEditPhone.Text))
            {
                ShowToast("Phone number cannot be empty.", PackIconMaterialKind.AlertCircleOutline, isError: true);
                txtEditPhone.Focus();
                return;
            }
            // Add more specific validation for phone format if needed

            bool phoneChanged = _selectedPatient.Phone != txtEditPhone.Text;
            bool notesChanged = _selectedPatient.MedicalNotes != txtEditMedicalNotes.Text;

            if (phoneChanged || notesChanged)
            {
                // Update the selected patient object
                _selectedPatient.Phone = txtEditPhone.Text;
                _selectedPatient.MedicalNotes = txtEditMedicalNotes.Text;

                ShowToast("Saving changes...", PackIconMaterialKind.DatabaseSyncOutline, isError: false, durationSeconds: 2);

                // --- TODO: Save changes to the database ---
                // Placeholder for actual database update logic
                /*
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(YourConnectionString))
                    {
                        connection.Open();
                        string query = "UPDATE patients SET phone = @phone, medical_notes = @medicalNotes WHERE patient_id = @patientId";
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@phone", _selectedPatient.Phone);
                            command.Parameters.AddWithValue("@medicalNotes", _selectedPatient.MedicalNotes);
                            command.Parameters.AddWithValue("@patientId", _selectedPatient.PatientId); // Ensure PatientId type matches DB
                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                ShowToast("Changes saved successfully!", PackIconMaterialKind.CheckCircleOutline);
                                PatientsView.Refresh(); // Refresh grid to show updated data
                                HideModal(editModalOverlay);
                            }
                            else
                            {
                                throw new Exception("No records updated. Patient might not exist or data hasn't changed.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowToast($"Error saving changes: {ex.Message}", PackIconMaterialKind.AlertCircleOutline, isError: true, durationSeconds: 5);
                    // Consider logging ex details
                    // Optional: Revert UI changes if save fails?
                     txtEditPhone.Text = _originalPhone;
                     txtEditMedicalNotes.Text = _originalMedicalNotes;
                }
                */
                // --- End DB Call Placeholder ---

                // Simulate successful save for demo
                var saveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                saveTimer.Tick += (s, args) =>
                {
                    saveTimer.Stop();
                    ShowToast("Changes saved successfully!", PackIconMaterialKind.CheckCircleOutline);
                    PatientsView.Refresh(); // Refresh grid *after* successful save
                    HideModal(editModalOverlay);
                };
                saveTimer.Start();

            }
            else
            {
                ShowToast("No changes detected.", PackIconMaterialKind.InformationOutline);
                HideModal(editModalOverlay);
            }
        }


        // --- Toast Notification ---
        private void BtnCloseToast_Click(object sender, RoutedEventArgs e)
        {
            HideToast();
        }

        #endregion

        #region Helper Methods

        // --- Filtering ---
        private void ApplyFilters()
        {
            if (PatientsView == null) return;

            PatientsView.Filter = item =>
            {
                if (!(item is Patient patient)) return false;

                // Search Text Filter
                bool matchesSearch = true;
                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    string searchText = txtSearch.Text.Trim().ToLower();
                    matchesSearch = (patient.Name?.ToLower().Contains(searchText) ?? false) ||
                                    (patient.CNIC?.Replace("-", "").Contains(searchText) ?? false) ||
                                    (patient.Phone?.Replace("-", "").Replace("+", "").Contains(searchText) ?? false);
                }

                // Blood Group Filter
                bool matchesBloodGroup = true;
                var selectedBloodGroupItem = cmbFilterBloodGroup.SelectedItem as ComboBoxItem;
                string selectedBloodGroup = selectedBloodGroupItem?.Content.ToString();
                if (selectedBloodGroup != "All" && !string.IsNullOrEmpty(selectedBloodGroup))
                {
                    matchesBloodGroup = patient.BloodGroup == selectedBloodGroup;
                }

                // Date Range Filter
                bool matchesDateRange = true;
                DateTime? fromDate = dpFilterFrom.SelectedDate;
                DateTime? toDate = dpFilterTo.SelectedDate;

                // Ensure 'To' date includes the entire day
                if (toDate.HasValue)
                    toDate = toDate.Value.Date.AddDays(1).AddSeconds(-1);

                if (fromDate.HasValue && toDate.HasValue)
                {
                    matchesDateRange = patient.RegistrationDate >= fromDate.Value.Date && patient.RegistrationDate <= toDate.Value;
                }
                else if (fromDate.HasValue)
                {
                    matchesDateRange = patient.RegistrationDate >= fromDate.Value.Date;
                }
                else if (toDate.HasValue)
                {
                    matchesDateRange = patient.RegistrationDate <= toDate.Value;
                }


                return matchesSearch && matchesBloodGroup && matchesDateRange;
            };
        }

        // --- Modal Display ---
        private void DisplayPatientDetails()
        {
            if (_selectedPatient == null) return;

            // Populate the details view modal
            txtDetailsName.Text = _selectedPatient.Name ?? "N/A";
            txtDetailsId.Text = $"ID: {_selectedPatient.PatientId ?? "N/A"}";
            txtDetailsFatherName.Text = _selectedPatient.FatherName ?? "N/A";
            txtDetailsAge.Text = _selectedPatient.Age > 0 ? _selectedPatient.Age.ToString() : "N/A";
            txtDetailsBloodGroup.Text = _selectedPatient.BloodGroup ?? "N/A";
            txtDetailsCNIC.Text = _selectedPatient.CNIC ?? "N/A";
            txtDetailsPhone.Text = _selectedPatient.Phone ?? "N/A";
            txtDetailsRegistrationDate.Text = _selectedPatient.RegistrationDate != DateTime.MinValue
                                              ? _selectedPatient.RegistrationDate.ToString("dd-MM-yyyy") : "N/A";
            txtDetailsMedicalNotes.Text = string.IsNullOrWhiteSpace(_selectedPatient.MedicalNotes)
                                          ? "No medical notes available." : _selectedPatient.MedicalNotes;

            ShowModal(detailsModalOverlay);
        }

        private void PopulateLimitedEditForm()
        {
            if (_selectedPatient == null) return;

            // Store original values for cancellation/comparison
            _originalPhone = _selectedPatient.Phone;
            _originalMedicalNotes = _selectedPatient.MedicalNotes;

            // Populate the edit modal fields
            txtEditPatientId.Text = _selectedPatient.PatientId ?? "N/A";
            txtEditName.Text = _selectedPatient.Name ?? "N/A";
            txtEditFatherName.Text = _selectedPatient.FatherName ?? "N/A";
            txtEditAge.Text = _selectedPatient.Age > 0 ? _selectedPatient.Age.ToString() : "N/A";
            txtEditBloodGroup.Text = _selectedPatient.BloodGroup ?? "N/A";
            txtEditCNIC.Text = _selectedPatient.CNIC ?? "N/A";
            txtEditRegDate.Text = _selectedPatient.RegistrationDate != DateTime.MinValue
                                  ? _selectedPatient.RegistrationDate.ToString("dd-MM-yyyy") : "N/A";

            // Editable fields
            txtEditPhone.Text = _selectedPatient.Phone ?? "";
            txtEditMedicalNotes.Text = _selectedPatient.MedicalNotes ?? "";
        }

        private void ShowModal(Border modal)
        {
            modal.Visibility = Visibility.Visible;
            var fadeIn = FindResource("FadeInAnimation") as Storyboard;
            if (fadeIn != null)
            {
                Storyboard.SetTarget(fadeIn, modal);
                fadeIn.Begin();
            }
            else
            {
                modal.Opacity = 1; // Fallback if animation not found
            }
        }

        private void HideModal(Border modal)
        {
            var fadeOut = FindResource("FadeOutAnimation") as Storyboard;
            if (fadeOut != null)
            {
                Storyboard.SetTarget(fadeOut, modal);
                fadeOut.Completed += (s, e) => modal.Visibility = Visibility.Collapsed;
                fadeOut.Begin();
            }
            else
            {
                modal.Opacity = 0; // Fallback
                modal.Visibility = Visibility.Collapsed;
            }
        }

        // --- Filter Panel Animation ---
        private bool _isFilterPanelVisible = false;
        private double _filterPanelTargetOpacity = 0;

        private void ToggleFilterPanel(bool? forceHide = null)
        {
            bool shouldBeVisible = forceHide.HasValue ? !forceHide.Value : !_isFilterPanelVisible;

            if (shouldBeVisible)
            {
                filterPanel.Visibility = Visibility.Visible;
                _filterPanelTargetOpacity = 1;
            }
            else
            {
                _filterPanelTargetOpacity = 0;
                // Visibility will be set to Collapsed upon animation completion
            }

            _isFilterPanelVisible = shouldBeVisible;
            _filterPanelTimer.Start();
        }

        private void AnimateFilterPanel(object sender, EventArgs e)
        {
            double step = 0.1; // Animation speed factor
            bool changed = false;

            if (Math.Abs(filterPanel.Opacity - _filterPanelTargetOpacity) > 0.01)
            {
                if (filterPanel.Opacity < _filterPanelTargetOpacity)
                    filterPanel.Opacity = Math.Min(1, filterPanel.Opacity + step);
                else
                    filterPanel.Opacity = Math.Max(0, filterPanel.Opacity - step);
                changed = true;
            }

            if (!changed || Math.Abs(filterPanel.Opacity - _filterPanelTargetOpacity) < 0.01)
            {
                filterPanel.Opacity = _filterPanelTargetOpacity; // Snap to final value
                _filterPanelTimer.Stop();
                if (_filterPanelTargetOpacity == 0)
                {
                    filterPanel.Visibility = Visibility.Collapsed;
                }
            }
        }


        // --- Toast Notification ---
        private void ShowToast(string message, PackIconMaterialKind iconKind, bool isError = false, double durationSeconds = 3)
        {
            toastMessage.Text = message;
            toastIcon.Kind = iconKind;
            toastNotification.Background = isError ? (FindResource("DangerColor") as SolidColorBrush) : (FindResource("PrimaryAccentColor") as SolidColorBrush);

            // Restart timer with potentially new duration
            _toastTimer.Stop();
            _toastTimer.Interval = TimeSpan.FromSeconds(durationSeconds);

            toastNotification.Visibility = Visibility.Visible;
            var fadeIn = FindResource("FadeInAnimation") as Storyboard;
            if (fadeIn != null)
            {
                Storyboard.SetTarget(fadeIn, toastNotification);
                fadeIn.Begin();
            }
            else
            {
                toastNotification.Opacity = 1; // Fallback
            }

            _toastTimer.Start();
        }

        private void HideToast()
        {
            _toastTimer.Stop(); // Ensure timer is stopped if manually closed

            var fadeOut = FindResource("FadeOutAnimation") as Storyboard;
            if (fadeOut != null)
            {
                Storyboard.SetTarget(fadeOut, toastNotification);
                fadeOut.Completed += (s, e) => toastNotification.Visibility = Visibility.Collapsed;
                fadeOut.Begin();
            }
            else
            {
                toastNotification.Opacity = 0; // Fallback
                toastNotification.Visibility = Visibility.Collapsed;
            }
        }

        #endregion
    }

    // Data Model (Assuming same structure as Admin's Patient class, but using string ID maybe)
    // Make sure this matches the actual class definition used in your project.
    // If it's different from the Admin's Patient class, use that definition here.
    // For this example, I'll adapt the original LT's PatientRecord and make it INotifyPropertyChanged.

    public class Patient : INotifyPropertyChanged // Changed from PatientRecord to Patient
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _patientId;
        public string PatientId
        {
            get => _patientId;
            set { _patientId = value; OnPropertyChanged(nameof(PatientId)); }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        private string _fatherName;
        public string FatherName
        {
            get => _fatherName;
            set { _fatherName = value; OnPropertyChanged(nameof(FatherName)); }
        }

        private int _age;
        public int Age
        {
            get => _age;
            set { _age = value; OnPropertyChanged(nameof(Age)); }
        }

        private string _bloodGroup;
        public string BloodGroup
        {
            get => _bloodGroup;
            set { _bloodGroup = value; OnPropertyChanged(nameof(BloodGroup)); }
        }

        private string _cnic;
        public string CNIC
        {
            get => _cnic;
            set { _cnic = value; OnPropertyChanged(nameof(CNIC)); }
        }

        private string _phone;
        public string Phone
        {
            get => _phone;
            set { _phone = value; OnPropertyChanged(nameof(Phone)); }
        }

        private DateTime _registrationDate;
        public DateTime RegistrationDate
        {
            get => _registrationDate;
            set { _registrationDate = value; OnPropertyChanged(nameof(RegistrationDate)); }
        }

        private string _medicalNotes;
        public string MedicalNotes
        {
            get => _medicalNotes;
            set { _medicalNotes = value; OnPropertyChanged(nameof(MedicalNotes)); }
        }
    }
}