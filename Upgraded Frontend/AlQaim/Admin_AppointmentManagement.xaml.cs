using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;        // For Time
using MahApps.Metro.IconPacks;        
using System.Windows.Media.Animation; 

namespace AlQaim
{
    public partial class Admin_AppointmentManagement : Page
    {
        // Properties
        private ObservableCollection<AppointmentViewModel> _appointments;
        private ICollectionView _appointmentsView;
        private int? _selectedAppointmentId; 
        private bool _isEditMode;
        private Action _confirmationAction; 

        
        private DispatcherTimer _toastTimer;
        

        // Constructor
        public Admin_AppointmentManagement()
        {
            InitializeComponent();

            
            _toastTimer = new DispatcherTimer();
            _toastTimer.Interval = TimeSpan.FromSeconds(3); // Adjust duration if needed
            _toastTimer.Tick += ToastTimer_Tick;
            

            // Asynchronously load data when the page loads
            LoadData();
        }

        
        private void ToastTimer_Tick(object sender, EventArgs e)
        {
            _toastTimer.Stop();
            HideToast();
        }
        

        // Load initial data asynchronously
        private async void LoadData()
        {
            ShowLoading(true); // Show loading indicator

            try
            {
                // Simulate fetching data in a background task
                await Task.Run(() =>
                {
                
                    Dispatcher.Invoke(() =>
                    {
                        _appointments = new ObservableCollection<AppointmentViewModel>();
                    });

                    // Load appointments (mock data for now)
                    LoadMockAppointments(); 
                   
                    Dispatcher.Invoke(() =>
                    {
                        LoadDoctorsAndPatients(); 
                    });
                });

                // Ensure _appointments is initialized before creating the view
                if (_appointments == null)
                {
                    _appointments = new ObservableCollection<AppointmentViewModel>();
                }

                // Set up the CollectionView for filtering on the UI thread
                _appointmentsView = CollectionViewSource.GetDefaultView(_appointments);
                _appointmentsView.Filter = AppointmentFilter; // Assign the filter method
                dgAppointments.ItemsSource = _appointmentsView; // Bind DataGrid

                // Optional: Show toast on successful load
                // ShowToast("Appointments loaded successfully.", PackIconMaterialKind.CheckCircleOutline, useInfoColor: true);

            }
            catch (Exception ex)
            {
                // Show error toast if loading fails
                ShowToast($"Error loading appointments: {ex.Message}", PackIconMaterialKind.AlertCircleOutline);
                // Ensure collections are initialized even on error to prevent null reference exceptions
                if (_appointments == null) _appointments = new ObservableCollection<AppointmentViewModel>();
                if (_appointmentsView == null) _appointmentsView = CollectionViewSource.GetDefaultView(_appointments);
                dgAppointments.ItemsSource = _appointmentsView; // Bind empty collection
            }
            finally
            {
                ShowLoading(false); // Hide loading indicator
            }
        }

        // Load mock appointment data (replace with database calls)
        private void LoadMockAppointments()
        {
            // Simulate adding items (Ensure this runs on UI thread if _appointments is bound)
            Dispatcher.Invoke(() =>
            {
                _appointments.Clear(); // Clear existing before loading new
                _appointments.Add(new AppointmentViewModel { AppointmentId = 1, PatientId = 1, PatientName = "Ahmed Hassan", DoctorId = 101, DoctorName = "Dr. Fatima Al-Zahrawi", AppointmentDate = DateTime.Now.Date.AddHours(9), Status = "Scheduled", Code = "APT-001", Notes = "Regular check-up", CanBeCanceled = true });
                _appointments.Add(new AppointmentViewModel { AppointmentId = 2, PatientId = 2, PatientName = "Sara Mohammed", DoctorId = 102, DoctorName = "Dr. Khalid Rahman", AppointmentDate = DateTime.Now.Date.AddDays(-1).AddHours(14).AddMinutes(30), Status = "Completed", Code = "APT-002", Notes = "Follow-up appointment", CanBeCanceled = false });
                _appointments.Add(new AppointmentViewModel { AppointmentId = 3, PatientId = 3, PatientName = "Mahmoud Ali", DoctorId = 101, DoctorName = "Dr. Fatima Al-Zahrawi", AppointmentDate = DateTime.Now.Date.AddDays(2).AddHours(11), Status = "Scheduled", Code = "APT-003", Notes = "Initial consultation", CanBeCanceled = true });
                _appointments.Add(new AppointmentViewModel { AppointmentId = 4, PatientId = 4, PatientName = "Layla Ibrahim", DoctorId = 103, DoctorName = "Dr. Hamza Jabir", AppointmentDate = DateTime.Now.Date.AddDays(-3).AddHours(10), Status = "Canceled", Code = "APT-004", Notes = "Patient requested cancellation", CanBeCanceled = false });
                _appointments.Add(new AppointmentViewModel { AppointmentId = 5, PatientId = 5, PatientName = "Omar Farooq", DoctorId = 102, DoctorName = "Dr. Khalid Rahman", AppointmentDate = DateTime.Now.Date.AddHours(13), Status = "Scheduled", Code = "APT-005", Notes = "Urgent consultation", CanBeCanceled = true });
            });
        }

        // Load mock doctor and patient data for ComboBoxes (replace with database calls)
        private void LoadDoctorsAndPatients()
        {
            // Mock Doctors
            List<UserViewModel> doctors = new List<UserViewModel>
            {
                new UserViewModel { UserId = 101, FullName = "Dr. Fatima Al-Zahrawi" },
                new UserViewModel { UserId = 102, FullName = "Dr. Khalid Rahman" },
                new UserViewModel { UserId = 103, FullName = "Dr. Hamza Jabir" },
                new UserViewModel { UserId = 104, FullName = "Dr. Aisha Musa" }
            };
            cmbDoctor.ItemsSource = doctors;
            cmbDoctor.DisplayMemberPath = "FullName"; // Ensure this is set
            cmbDoctor.SelectedValuePath = "UserId";

            // Mock Patients
            List<PatientViewModel> patients = new List<PatientViewModel>
            {
                new PatientViewModel { PatientId = 1, FullName = "Ahmed Hassan" },
                new PatientViewModel { PatientId = 2, FullName = "Sara Mohammed" },
                new PatientViewModel { PatientId = 3, FullName = "Mahmoud Ali" },
                new PatientViewModel { PatientId = 4, FullName = "Layla Ibrahim" },
                new PatientViewModel { PatientId = 5, FullName = "Omar Farooq" },
                new PatientViewModel { PatientId = 6, FullName = "Nour Al-Ahmad" },
                new PatientViewModel { PatientId = 7, FullName = "Yousef Kareem" }
            };
            cmbPatient.ItemsSource = patients;
            cmbPatient.DisplayMemberPath = "FullName"; // Ensure this is set - THIS SHOULD FIX THE DISPLAY ISSUE
            cmbPatient.SelectedValuePath = "PatientId";

            // **ComboBox Display Fix Explanation:** The issue "alqaim patient view modal" likely means the ComboBox was displaying the result of the PatientViewModel's default ToString() method instead of the FullName. Explicitly setting `DisplayMemberPath="FullName"` (as was already in your XAML and confirmed here) is the correct way to tell the ComboBox which property to display for each item, both in the dropdown and usually for the selected item box. If this still fails, a more robust fix involves setting an explicit `ItemTemplate` or `SelectionBoxItemTemplate` in the ComboBox style, but `DisplayMemberPath` should suffice.
        }


        // Filter logic for the appointment view
        private bool AppointmentFilter(object item)
        {
            if (!(item is AppointmentViewModel appointment))
                return false;

            bool matchesSearch = true;
            bool matchesStatus = true;
            bool matchesDate = true;

            // Search filter (case-insensitive)
            string searchText = txtSearch.Text?.ToLower() ?? string.Empty;
            if (!string.IsNullOrEmpty(searchText))
            {
                matchesSearch = (appointment.PatientName?.ToLower().Contains(searchText) ?? false) ||
                                (appointment.DoctorName?.ToLower().Contains(searchText) ?? false) ||
                                (appointment.Code?.ToLower().Contains(searchText) ?? false) ||
                                (appointment.AppointmentId.ToString().Contains(searchText)); // Search by ID too
            }

            // Status filter
            if (cmbStatusFilter.SelectedItem is ComboBoxItem statusItem && cmbStatusFilter.SelectedIndex > 0) // Check if selected item is valid
            {
                string selectedStatus = statusItem.Content.ToString();
                matchesStatus = appointment.Status == selectedStatus;
            }

            // Date filter
            if (dpDateFilter.SelectedDate.HasValue)
            {
                matchesDate = appointment.AppointmentDate.Date == dpDateFilter.SelectedDate.Value.Date;
            }

            return matchesSearch && matchesStatus && matchesDate;
        }

        // Show or hide the loading overlay
        private void ShowLoading(bool show)
        {
            // Ensure UI updates happen on the UI thread
            Dispatcher.Invoke(() => {
                loadingOverlay.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        // Show or hide the Add/Edit Appointment modal
        private void ShowAppointmentModal(bool show, bool isEdit = false)
        {
            // Ensure UI updates happen on the UI thread
            Dispatcher.Invoke(() => {
                modalOverlay.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
                _isEditMode = isEdit;
                modalTitle.Text = isEdit ? "Edit Appointment" : "Add New Appointment";

                if (show && !isEdit) // If showing for Add
                {
                    // Default values for new appointment
                    ClearAppointmentForm(); // Clear previous entries
                    dpAppointmentDate.SelectedDate = DateTime.Now.Date; // Default to today
                                                                        // Select a default time if applicable, e.g., first available slot
                    cmbAppointmentTime.SelectedIndex = 0; // Example: 08:00 AM
                    cmbStatus.SelectedItem = cmbStatus.Items.OfType<ComboBoxItem>().FirstOrDefault(cbi => cbi.Content.ToString() == "Scheduled"); // Default status
                    cmbPatient.Focus(); // Focus the first field
                }
                else if (show && isEdit) // If showing for Edit
                {
                    // Data is loaded in LoadAppointmentForEdit, just set focus
                    cmbPatient.Focus();
                }
            });
        }


        // Clear the fields in the Add/Edit Appointment modal
        private void ClearAppointmentForm()
        {
            cmbPatient.SelectedIndex = -1;
            cmbDoctor.SelectedIndex = -1;
            dpAppointmentDate.SelectedDate = null;
            cmbAppointmentTime.SelectedIndex = -1;
            cmbStatus.SelectedIndex = -1;
            txtCode.Text = string.Empty;
            txtNotes.Text = string.Empty;
            _selectedAppointmentId = null; // Clear tracked ID
        }

        // Show the confirmation dialog
        private void ShowConfirmationDialog(string message, Action confirmAction)
        {
            // Ensure UI updates happen on the UI thread
            Dispatcher.Invoke(() => {
                txtConfirmationMessage.Text = message; // Set the message
                _confirmationAction = confirmAction; // Store the action to run on confirm
                confirmationDialog.Visibility = Visibility.Visible; // Show the dialog
            });
        }


        // Populate the modal form with data from an existing appointment for editing
        private void LoadAppointmentForEdit(AppointmentViewModel appointment)
        {
            _selectedAppointmentId = appointment.AppointmentId; // Store the ID of the item being edited

            // Set form values based on the appointment data
            cmbPatient.SelectedValue = appointment.PatientId;
            cmbDoctor.SelectedValue = appointment.DoctorId;
            dpAppointmentDate.SelectedDate = appointment.AppointmentDate.Date;

            // Find and select the matching time string in the ComboBox
            string timeString = appointment.AppointmentDate.ToString("hh:mm tt");
            cmbAppointmentTime.SelectedItem = cmbAppointmentTime.Items.OfType<ComboBoxItem>()
                                               .FirstOrDefault(item => item.Content.ToString() == timeString);

            // Find and select the matching status string in the ComboBox
            cmbStatus.SelectedItem = cmbStatus.Items.OfType<ComboBoxItem>()
                                        .FirstOrDefault(item => item.Content.ToString() == appointment.Status);

            txtCode.Text = appointment.Code;
            txtNotes.Text = appointment.Notes;
        }

        // Validate the appointment form fields
        private bool ValidateAppointmentForm()
        {
            if (cmbPatient.SelectedIndex == -1)
            {
                ShowToast("Please select a Patient.", PackIconMaterialKind.AccountAlertOutline);
                cmbPatient.Focus();
                return false;
            }
            if (cmbDoctor.SelectedIndex == -1)
            {
                ShowToast("Please select an assigned Doctor.", PackIconMaterialKind.Doctor);
                cmbDoctor.Focus();
                return false;
            }
            if (!dpAppointmentDate.SelectedDate.HasValue)
            {
                ShowToast("Please select an Appointment Date.", PackIconMaterialKind.CalendarAlert);
                dpAppointmentDate.Focus();
                return false;
            }
            if (dpAppointmentDate.SelectedDate.Value.Date < DateTime.Today && !_isEditMode) // Allow past dates when editing (e.g., setting status to Completed) but not for new appointments
            {
                ShowToast("Cannot schedule a new appointment for a past date.", PackIconMaterialKind.CalendarAlert);
                dpAppointmentDate.Focus();
                return false;
            }
            if (cmbAppointmentTime.SelectedIndex == -1)
            {
                ShowToast("Please select an Appointment Time.", PackIconMaterialKind.ClockAlertOutline);
                cmbAppointmentTime.Focus();
                return false;
            }
            if (cmbStatus.SelectedIndex == -1)
            {
                ShowToast("Please select a Status.", PackIconMaterialKind.ListStatus);
                cmbStatus.Focus();
                return false;
            }
            // Add more specific validation if needed (e.g., check for time conflicts)

            return true; // All basic validations passed
        }

        // Save (Add or Update) appointment data
        private async void SaveAppointment()
        {
            // Validate form first
            if (!ValidateAppointmentForm())
            {
                return; // Stop if validation fails (toast shown in validation method)
            }

            ShowLoading(true); // Show loading indicator

            try
            {
                // Get form values
                int patientId = (int)cmbPatient.SelectedValue;
                // Get patient name directly from selected item to avoid issues if binding fails
                string patientName = (cmbPatient.SelectedItem as PatientViewModel)?.FullName ?? "Unknown Patient";
                int doctorId = (int)cmbDoctor.SelectedValue;
                string doctorName = (cmbDoctor.SelectedItem as UserViewModel)?.FullName ?? "Unknown Doctor";
                DateTime appointmentDate = dpAppointmentDate.SelectedDate.Value;
                string selectedTime = ((ComboBoxItem)cmbAppointmentTime.SelectedItem).Content.ToString();
                // Use invariant culture for parsing standard time formats
                DateTime appointmentTime = DateTime.ParseExact(selectedTime, "hh:mm tt", CultureInfo.InvariantCulture);
                // Combine date and time
                DateTime fullAppointmentDate = appointmentDate.Date.Add(appointmentTime.TimeOfDay);
                string status = ((ComboBoxItem)cmbStatus.SelectedItem).Content.ToString();
                string code = txtCode.Text.Trim();
                string notes = txtNotes.Text.Trim();

                // Simulate database operation
                await Task.Delay(500);

                if (_isEditMode && _selectedAppointmentId.HasValue)
                {
                    // --- UPDATE existing appointment ---
                    var appointment = _appointments.FirstOrDefault(a => a.AppointmentId == _selectedAppointmentId.Value);
                    if (appointment != null)
                    {
                        // Update properties
                        appointment.PatientId = patientId;
                        appointment.PatientName = patientName;
                        appointment.DoctorId = doctorId;
                        appointment.DoctorName = doctorName;
                        appointment.AppointmentDate = fullAppointmentDate;
                        appointment.Status = status;
                        appointment.Code = code;
                        appointment.Notes = notes;
                        appointment.CanBeCanceled = status == "Scheduled"; // Update cancel state based on new status

                        // TODO: Add actual database update call here
                        // UpdateAppointmentInDatabase(appointment);

                        _appointmentsView.Refresh(); // Refresh the view to show changes
                        ShowToast($"Appointment ID {appointment.AppointmentId} updated successfully.", PackIconMaterialKind.CalendarEdit);
                        ShowAppointmentModal(false); // Close modal on success
                    }
                    else
                    {
                        ShowToast($"Error: Appointment ID {_selectedAppointmentId.Value} not found for update.", PackIconMaterialKind.AlertCircleOutline);
                    }
                }
                else
                {
                    // --- ADD new appointment ---
                    // Determine the next ID (in real app, DB generates this)
                    int newId = _appointments.Any() ? _appointments.Max(a => a.AppointmentId) + 1 : 1;
                    var newAppointment = new AppointmentViewModel
                    {
                        AppointmentId = newId,
                        PatientId = patientId,
                        PatientName = patientName,
                        DoctorId = doctorId,
                        DoctorName = doctorName,
                        AppointmentDate = fullAppointmentDate,
                        Status = status,
                        // Generate code if empty, otherwise use provided code
                        Code = string.IsNullOrEmpty(code) ? $"APT-{newId:D3}" : code,
                        Notes = notes,
                        CanBeCanceled = status == "Scheduled" // Set initial cancel state
                    };

                    // TODO: Add actual database insert call here
                    // AddAppointmentToDatabase(newAppointment);

                    _appointments.Add(newAppointment); // Add to observable collection (UI updates)
                    // No explicit Refresh needed for ObservableCollection adds if binding is correct
                    ShowToast($"Appointment ID {newAppointment.AppointmentId} added successfully.", PackIconMaterialKind.CalendarPlus);
                    ShowAppointmentModal(false); // Close modal on success
                }
            }
            catch (FormatException ex) // Catch specific errors like date/time parsing
            {
                ShowToast($"Error processing form data: {ex.Message}", PackIconMaterialKind.AlertCircleOutline);
            }
            catch (Exception ex) // Catch general errors
            {
                // Use ShowToast instead of MessageBox
                ShowToast($"Error saving appointment: {ex.Message}", PackIconMaterialKind.AlertCircleOutline);
            }
            finally
            {
                ShowLoading(false); // Hide loading indicator
            }
        }


        // Cancel the specified appointment
        private async void CancelAppointment(AppointmentViewModel appointment)
        {
            if (appointment == null) return;

            ShowLoading(true); // Show loading indicator

            try
            {
                // Simulate database operation
                await Task.Delay(500);

                // TODO: Add actual database update call here to set status to "Canceled"
                // UpdateAppointmentStatusInDatabase(appointment.AppointmentId, "Canceled");

                // Update the ViewModel properties
                appointment.Status = "Canceled";
                appointment.CanBeCanceled = false; // Cannot cancel again
                _appointmentsView.Refresh(); // Refresh the DataGrid to show the status change

                // Show success toast
                ShowToast($"Appointment ID {appointment.AppointmentId} canceled successfully.", PackIconMaterialKind.CalendarRemove);
            }
            catch (Exception ex)
            {
                // Use ShowToast instead of MessageBox
                ShowToast($"Error canceling appointment: {ex.Message}", PackIconMaterialKind.AlertCircleOutline);
            }
            finally
            {
                ShowLoading(false); // Hide loading indicator
            }
        }

        #region Event Handlers

        // Search TextBox TextChanged Event
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_appointmentsView != null)
            {
                _appointmentsView.Refresh(); // Re-apply filter when search text changes
            }
        }

        // Status Filter ComboBox Selection Changed Event
        private void CmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_appointmentsView != null)
            {
                _appointmentsView.Refresh(); // Re-apply filter when status selection changes
            }
        }

        // Date Filter DatePicker Selection Changed Event
        private void DpDateFilter_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_appointmentsView != null)
            {
                _appointmentsView.Refresh(); // Re-apply filter when date selection changes
            }
        }

        // Refresh Button Click Event
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            // Reset filters
            dpDateFilter.SelectedDate = null;
            cmbStatusFilter.SelectedIndex = 0; // Select "All"
            txtSearch.Text = string.Empty;
            // No need to call Refresh explicitly here if filters are reset,
            // but reloading data might be intended. Let's reload for consistency.
            LoadData(); // Reload data completely
                        // Show refresh toast *after* data reload attempt (LoadData handles its own errors)
                        // ShowToast("Appointment data refreshed.", PackIconMaterialKind.Refresh, useInfoColor: true);
        }


        // Add Appointment Button Click Event
        private void BtnAddAppointment_Click(object sender, RoutedEventArgs e)
        {
            ShowAppointmentModal(true, false); // Show modal for adding a new item
        }

        // Edit Button Click Event (in DataGrid row)
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Get the appointment associated with the clicked button's row
            if (sender is Button button && button.DataContext is AppointmentViewModel appointment)
            {
                LoadAppointmentForEdit(appointment); // Populate the form
                ShowAppointmentModal(true, true); // Show modal in edit mode
            }
        }

        // Cancel Appointment Button Click Event (in DataGrid row)
        private void BtnCancelAppointment_Click(object sender, RoutedEventArgs e)
        {
            // Get the appointment associated with the clicked button's row
            if (sender is Button button && button.DataContext is AppointmentViewModel appointment)
            {
                // Show confirmation dialog before canceling
                ShowConfirmationDialog(
                   $"Are you sure you want to cancel the appointment for {appointment.PatientName} on {appointment.AppointmentDate:MMM dd, yyyy 'at' hh:mm tt}?",
                   () => CancelAppointment(appointment) // Pass the cancel action
                );
            }
        }

        // Close Modal Button (X) Click Event
        private void BtnCloseModal_Click(object sender, RoutedEventArgs e)
        {
            ShowAppointmentModal(false); // Hide the modal
        }

        // Cancel Button Click Event (inside the modal)
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            ShowAppointmentModal(false); // Hide the modal
        }

        // Save Appointment Button Click Event (inside the modal)
        private void BtnSaveAppointment_Click(object sender, RoutedEventArgs e)
        {
            SaveAppointment(); // Attempt to save the appointment
        }

        // Cancel Button Click Event (on confirmation dialog)
        private void BtnCancelConfirmation_Click(object sender, RoutedEventArgs e)
        {
            confirmationDialog.Visibility = Visibility.Collapsed; // Hide confirmation
            _confirmationAction = null; // Clear stored action
        }

        // Confirm Action Button Click Event (on confirmation dialog)
        private void BtnConfirmAction_Click(object sender, RoutedEventArgs e)
        {
            confirmationDialog.Visibility = Visibility.Collapsed; // Hide confirmation
            _confirmationAction?.Invoke(); // Execute the stored action (e.g., CancelAppointment)
            _confirmationAction = null; // Clear action after execution
        }

        // DataGrid Selection Changed Event (Optional Use)
        private void DgAppointments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Can be used for master-detail views or enabling/disabling buttons
            // Example: var selectedAppt = dgAppointments.SelectedItem as AppointmentViewModel;
        }

        #endregion

        #region Toast Notification Methods

        // Shows a toast notification message
        private void ShowToast(string message, PackIconMaterialKind iconKind, bool useInfoColor = false)
        {
            // Ensure execution on the UI thread
            Dispatcher.Invoke(() => {
                toastMessage.Text = message;
                toastIcon.Kind = iconKind;

                // Determine background color based on message type/icon
                SolidColorBrush backgroundBrush;
                if (iconKind == PackIconMaterialKind.AlertCircleOutline || iconKind == PackIconMaterialKind.Alert || iconKind == PackIconMaterialKind.CalendarAlert || iconKind == PackIconMaterialKind.ClockAlertOutline || iconKind == PackIconMaterialKind.AccountAlertOutline || iconKind == PackIconMaterialKind.Doctor || iconKind == PackIconMaterialKind.ListStatus)
                {
                    // Error/Warning
                    backgroundBrush = FindResource("DangerColor") as SolidColorBrush ?? new SolidColorBrush(Colors.Red);
                }
                else if (useInfoColor || iconKind == PackIconMaterialKind.Refresh || iconKind == PackIconMaterialKind.Information || iconKind == PackIconMaterialKind.CheckCircleOutline)
                {
                    // Informational
                    backgroundBrush = FindResource("InfoColor") as SolidColorBrush ?? new SolidColorBrush(Colors.DodgerBlue); // Assuming InfoColor exists
                }
                else if (iconKind == PackIconMaterialKind.CalendarRemove) // Cancellation specific
                {
                    backgroundBrush = FindResource("WarningColor") as SolidColorBrush ?? new SolidColorBrush(Colors.Orange); // Use Warning color for cancel confirmation
                }
                else // Default to Success (PrimaryAccentColor)
                {
                    backgroundBrush = FindResource("PrimaryAccentColor") as SolidColorBrush ?? new SolidColorBrush(Colors.Green);
                }
                toastNotification.Background = backgroundBrush;

                // Make toast visible and animate fade-in
                toastNotification.Visibility = Visibility.Visible;
                var fadeIn = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(300) };
                toastNotification.BeginAnimation(UIElement.OpacityProperty, fadeIn);

                // Restart the timer for auto-hide
                _toastTimer.Stop();
                _toastTimer.Start();
            });
        }

        // Hides the toast notification with a fade-out animation
        private void HideToast()
        {
            // Ensure execution on the UI thread
            Dispatcher.Invoke(() => {
                // Avoid starting animation if already collapsed
                if (toastNotification.Visibility != Visibility.Visible) return;

                var fadeOut = new DoubleAnimation { From = toastNotification.Opacity, To = 0, Duration = TimeSpan.FromMilliseconds(300) };
                // When animation completes, set visibility to Collapsed
                fadeOut.Completed += (s, e) => {
                    // Check opacity again; another toast might have interrupted the fade-out
                    if (toastNotification.Opacity < 0.1)
                    {
                        toastNotification.Visibility = Visibility.Collapsed;
                    }
                };
                // Apply the animation to the Opacity property
                toastNotification.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            });
        }


        // Handles the click on the toast's close button
        private void BtnCloseToast_Click(object sender, RoutedEventArgs e)
        {
            _toastTimer.Stop(); // Stop the auto-hide timer
            HideToast();      // Manually hide the toast
        }

        #endregion

    } 


    #region View Models (Ensure these are accessible)

    // Appointment View Model
    public class AppointmentViewModel : INotifyPropertyChanged // Implement INotifyPropertyChanged if properties change dynamically after initial load
    {
        private string _status;
        private bool _canBeCanceled;

        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Code { get; set; }
        public string Notes { get; set; }

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }

        public bool CanBeCanceled
        {
            get => _canBeCanceled;
            set { _canBeCanceled = value; OnPropertyChanged(nameof(CanBeCanceled)); }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Patient View Model
    public class PatientViewModel
    {
        public int PatientId { get; set; }
        public string FullName { get; set; }
        // Add other patient details if needed for display or logic

        // Override ToString only if needed for debugging, DisplayMemberPath handles UI display
        // public override string ToString() => FullName ?? base.ToString();
    }

    // User View Model (for doctors)
    public class UserViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        // Add other user details if needed

        // Override ToString only if needed for debugging
        // public override string ToString() => FullName ?? base.ToString();
    }

    #endregion

    #region Converters (Ensure these are accessible)

    // Appointment Status to Color Converter
    public class AppointmentStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value as string;
            SolidColorBrush defaultBrush = new SolidColorBrush(Colors.Gray); // Default color

            if (string.IsNullOrEmpty(status)) return defaultBrush;

            // Use Application.Current.TryFindResource for better resource management if defined in App.xaml
            // Otherwise, create colors directly.
            switch (status)
            {
                case "Scheduled":
                    return Application.Current.TryFindResource("PrimaryAccentColor") as SolidColorBrush ?? new SolidColorBrush(Color.FromRgb(67, 97, 238));
                case "Completed":
                    return Application.Current.TryFindResource("SuccessColor") as SolidColorBrush ?? new SolidColorBrush(Color.FromRgb(46, 204, 113));
                case "Canceled":
                    return Application.Current.TryFindResource("DangerColor") as SolidColorBrush ?? new SolidColorBrush(Color.FromRgb(231, 76, 60));
                case "No Show":
                    return Application.Current.TryFindResource("WarningColor") as SolidColorBrush ?? new SolidColorBrush(Color.FromRgb(243, 156, 18));
                default:
                    return defaultBrush;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            throw new NotImplementedException();
        }
    }

    #endregion
}