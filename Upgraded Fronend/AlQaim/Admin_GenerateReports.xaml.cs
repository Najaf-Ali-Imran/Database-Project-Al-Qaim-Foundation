using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;
using System.Windows.Media;
using MahApps.Metro.IconPacks;

namespace AlQaim
{
    /// <summary>
    /// Interaction logic for Admin_ReportGeneration.xaml
    /// </summary>
    public partial class Admin_GenerateReports : Page
    {
        // Simulated data sources - in a real application, these would come from your database
        private DataTable _reportData = new DataTable();
        private bool _isReportGenerated = false;

        public Admin_GenerateReports()
        {
            InitializeComponent();
            InitializePage();
        }

        /// <summary>
        /// Initialize the page with default values and settings
        /// </summary>
        private void InitializePage()
        {
            // Set default date range (current month)
            dpStartDate.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dpEndDate.SelectedDate = DateTime.Now;

            // Select the first report type by default
            if (cmbReportType.Items.Count > 0)
                cmbReportType.SelectedIndex = 0;

            // Initially disable the export button until a report is generated
            btnExportReport.IsEnabled = false;
        }

        #region Event Handlers

        /// <summary>
        /// Event handler for Generate Report button click
        /// </summary>
        private void BtnGenerateReport_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateFilters())
            {
                GenerateReport();
                _isReportGenerated = true;
                btnExportReport.IsEnabled = true;
                HideInfoPanel();
            }
            else
            {
                ShowInfoPanel("Please select a report type and valid date range to generate a report.");
            }
        }

        /// <summary>
        /// Event handler for Export Report button click
        /// </summary>
        private void BtnExportReport_Click(object sender, RoutedEventArgs e)
        {
            if (_isReportGenerated)
            {
                ExportReport();
            }
            else
            {
                ShowInfoPanel("Please generate a report first before exporting.");
            }
        }

        /// <summary>
        /// Event handler for Refresh button click
        /// </summary>
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ResetFilters();
            ClearReportData();
            HideInfoPanel();
        }

        /// <summary>
        /// Event handler for Report Type selection change
        /// </summary>
        private void CmbReportType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _isReportGenerated = false;
            btnExportReport.IsEnabled = false;
            ClearReportData();
            SetupDataGridColumns();
        }

        /// <summary>
        /// Event handler for DatePicker selection change
        /// </summary>
        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // Reset report generation state when dates change
            _isReportGenerated = false;
            btnExportReport.IsEnabled = false;
            ClearReportData();
        }

        #endregion

        #region Report Generation Methods

        /// <summary>
        /// Validates the filter selections
        /// </summary>
        /// <returns>True if filters are valid, otherwise false</returns>
        private bool ValidateFilters()
        {
            if (cmbReportType.SelectedItem == null)
                return false;

            if (!dpStartDate.SelectedDate.HasValue || !dpEndDate.SelectedDate.HasValue)
                return false;

            if (dpStartDate.SelectedDate > dpEndDate.SelectedDate)
            {
                ShowInfoPanel("Start date cannot be later than end date.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Generates the report based on selected filters
        /// </summary>
        private void GenerateReport()
        {
            try
            {
                // Get selected report type and date range
                string reportType = ((ComboBoxItem)cmbReportType.SelectedItem).Content.ToString();
                DateTime startDate = dpStartDate.SelectedDate.Value;
                DateTime endDate = dpEndDate.SelectedDate.Value;

                // Generate appropriate data based on report type
                switch (reportType)
                {
                    case "Inventory Usage":
                        GenerateInventoryUsageReport(startDate, endDate);
                        break;
                    case "Appointments":
                        GenerateAppointmentsReport(startDate, endDate);
                        break;
                    case "Test Results":
                        GenerateTestResultsReport(startDate, endDate);
                        break;
                    case "Patient Registrations":
                        GeneratePatientRegistrationsReport(startDate, endDate);
                        break;
                }

                // Update UI
                // Update UI
                txtReportTitle.Text = $"{reportType} Report";
                txtReportSummary.Text = $"Report for period: {startDate.ToShortDateString()} to {endDate.ToShortDateString()}";
                txtRecordCount.Text = $"{_reportData.Rows.Count} records";

                // Bind data to DataGrid
                dgReportData.ItemsSource = _reportData.DefaultView;
            }
            catch (Exception ex)
            {
                ShowInfoPanel($"Error generating report: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets up DataGrid columns based on the selected report type
        /// </summary>
        private void SetupDataGridColumns()
        {
            // Clear existing columns
            dgReportData.Columns.Clear();

            if (cmbReportType.SelectedItem == null)
                return;

            string reportType = ((ComboBoxItem)cmbReportType.SelectedItem).Content.ToString();

            // Configure columns based on report type
            switch (reportType)
            {
                case "Inventory Usage":
                    SetupInventoryUsageColumns();
                    break;
                case "Appointments":
                    SetupAppointmentsColumns();
                    break;
                case "Test Results":
                    SetupTestResultsColumns();
                    break;
                case "Patient Registrations":
                    SetupPatientRegistrationsColumns();
                    break;
            }
        }

        /// <summary>
        /// Exports the generated report to a CSV file
        /// </summary>
        private void ExportReport()
        {
            if (_reportData.Rows.Count == 0)
            {
                ShowInfoPanel("No data to export.");
                return;
            }

            try
            {
                // Configure save file dialog
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|Excel Files (*.xlsx)|*.xlsx",
                    DefaultExt = "csv",
                    FileName = $"{txtReportTitle.Text.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;
                    string fileExt = Path.GetExtension(filePath).ToLower();

                    if (fileExt == ".csv")
                    {
                        // Export to CSV
                        ExportToCSV(filePath);
                        ShowInfoPanel($"Report successfully exported to {filePath}", true);
                    }
                    else if (fileExt == ".xlsx")
                    {
                        // Export to Excel
                        ExportToExcel(filePath);
                        ShowInfoPanel($"Report successfully exported to {filePath}", true);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowInfoPanel($"Error exporting report: {ex.Message}");
            }
        }

        /// <summary>
        /// Exports data to CSV format
        /// </summary>
        private void ExportToCSV(string filePath)
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                // Write headers
                List<string> headers = new List<string>();
                foreach (DataColumn column in _reportData.Columns)
                {
                    headers.Add(column.ColumnName);
                }
                sw.WriteLine(string.Join(",", headers));

                // Write data rows
                foreach (DataRow row in _reportData.Rows)
                {
                    List<string> fields = new List<string>();
                    foreach (var item in row.ItemArray)
                    {
                        // Handle commas and quotes in the data
                        string field = item.ToString();
                        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
                        {
                            field = $"\"{field.Replace("\"", "\"\"")}\"";
                        }
                        fields.Add(field);
                    }
                    sw.WriteLine(string.Join(",", fields));
                }
            }
        }

        /// <summary>
        /// Exports data to Excel format (simplified version - in real app, use a library like EPPlus or ClosedXML)
        /// </summary>
        private void ExportToExcel(string filePath)
        {
            // This is a placeholder for Excel export functionality
            // In a real application, you would use a library like EPPlus or ClosedXML
            // For now, we'll just export as CSV and rename it
            ExportToCSV(filePath);

            // Show a message that this is a basic implementation
            MessageBox.Show("Excel export implemented as CSV. For proper Excel formatting, please implement using a library like EPPlus or ClosedXML.",
                "Export Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Report Type-Specific Methods

        /// <summary>
        /// Generates inventory usage report data
        /// </summary>
        private void GenerateInventoryUsageReport(DateTime startDate, DateTime endDate)
        {
            // Create a new DataTable
            _reportData = new DataTable();

            // Define columns for inventory usage report
            _reportData.Columns.Add("ItemID", typeof(int));
            _reportData.Columns.Add("ItemName", typeof(string));
            _reportData.Columns.Add("Category", typeof(string));
            _reportData.Columns.Add("UnitCost", typeof(decimal));
            _reportData.Columns.Add("QuantityUsed", typeof(int));
            _reportData.Columns.Add("TotalCost", typeof(decimal));
            _reportData.Columns.Add("UsageDate", typeof(DateTime));
            _reportData.Columns.Add("Department", typeof(string));

            // Add sample data (replace with database queries later)
            // Sample data generator
            Random random = new Random();
            string[] itemNames = { "Syringes", "Gloves", "Test Tubes", "Alcohol Swabs", "Microscope Slides", "Petri Dishes", "Reagents", "Masks" };
            string[] categories = { "Medical Supplies", "Lab Equipment", "PPE", "Chemicals", "Diagnostic" };
            string[] departments = { "Hematology", "Microbiology", "Chemistry", "Pathology", "General" };

            // Generate data for each day in the range
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                // Generate 1-3 entries per day
                int entriesPerDay = random.Next(1, 4);
                for (int i = 0; i < entriesPerDay; i++)
                {
                    int itemId = random.Next(1001, 1050);
                    string itemName = itemNames[random.Next(itemNames.Length)];
                    string category = categories[random.Next(categories.Length)];
                    decimal unitCost = Math.Round((decimal)(random.NextDouble() * 100), 2);
                    int quantity = random.Next(1, 50);
                    decimal totalCost = unitCost * quantity;
                    string department = departments[random.Next(departments.Length)];

                    _reportData.Rows.Add(itemId, itemName, category, unitCost, quantity, totalCost, date, department);
                }
            }
        }

        /// <summary>
        /// Sets up columns for inventory usage report
        /// </summary>
        private void SetupInventoryUsageColumns()
        {
            // Add columns to DataGrid
            DataGridTextColumn itemIdColumn = new DataGridTextColumn
            {
                Header = "Item ID",
                Binding = new Binding("ItemID")
            };

            DataGridTextColumn itemNameColumn = new DataGridTextColumn
            {
                Header = "Item Name",
                Binding = new Binding("ItemName")
            };

            DataGridTextColumn categoryColumn = new DataGridTextColumn
            {
                Header = "Category",
                Binding = new Binding("Category")
            };

            DataGridTextColumn unitCostColumn = new DataGridTextColumn
            {
                Header = "Unit Cost",
                Binding = new Binding("UnitCost") { StringFormat = "${0:N2}" }
            };

            DataGridTextColumn quantityColumn = new DataGridTextColumn
            {
                Header = "Quantity Used",
                Binding = new Binding("QuantityUsed")
            };

            DataGridTextColumn totalCostColumn = new DataGridTextColumn
            {
                Header = "Total Cost",
                Binding = new Binding("TotalCost") { StringFormat = "${0:N2}" }
            };

            DataGridTextColumn dateColumn = new DataGridTextColumn
            {
                Header = "Usage Date",
                Binding = new Binding("UsageDate") { StringFormat = "MM/dd/yyyy" }
            };

            DataGridTextColumn departmentColumn = new DataGridTextColumn
            {
                Header = "Department",
                Binding = new Binding("Department")
            };

            dgReportData.Columns.Add(itemIdColumn);
            dgReportData.Columns.Add(itemNameColumn);
            dgReportData.Columns.Add(categoryColumn);
            dgReportData.Columns.Add(unitCostColumn);
            dgReportData.Columns.Add(quantityColumn);
            dgReportData.Columns.Add(totalCostColumn);
            dgReportData.Columns.Add(dateColumn);
            dgReportData.Columns.Add(departmentColumn);
        }

        /// <summary>
        /// Generates appointments report data
        /// </summary>
        private void GenerateAppointmentsReport(DateTime startDate, DateTime endDate)
        {
            // Create a new DataTable
            _reportData = new DataTable();

            // Define columns for appointments report
            _reportData.Columns.Add("AppointmentID", typeof(int));
            _reportData.Columns.Add("PatientID", typeof(int));
            _reportData.Columns.Add("PatientName", typeof(string));
            _reportData.Columns.Add("AppointmentDate", typeof(DateTime));
            _reportData.Columns.Add("TimeSlot", typeof(string));
            _reportData.Columns.Add("Purpose", typeof(string));
            _reportData.Columns.Add("Status", typeof(string));
            _reportData.Columns.Add("DoctorName", typeof(string));

            // Add sample data (replace with database queries later)
            Random random = new Random();
            string[] patientNames = { "Ahmed Ali", "Fatima Khan", "Mohammed Hassan", "Sara Ahmed", "Omar Mahmoud", "Layla Ibrahim" };
            string[] purposes = { "Regular Check-up", "Blood Test", "Follow-up", "Consultation", "Lab Results Review" };
            string[] statuses = { "Completed", "Cancelled", "No-show", "Rescheduled" };
            string[] doctors = { "Dr. Kareem", "Dr. Nadia", "Dr. Hassan", "Dr. Aisha", "Dr. Ali" };
            string[] timeSlots = { "09:00 AM", "10:30 AM", "12:00 PM", "01:30 PM", "03:00 PM", "04:30 PM" };

            int appointmentId = 5001;

            // Generate data for each day in the range
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                // Skip weekends
                if (date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday)
                    continue;

                // Generate 5-10 appointments per day
                int appointmentsPerDay = random.Next(5, 11);
                for (int i = 0; i < appointmentsPerDay; i++)
                {
                    int patientId = random.Next(1001, 1500);
                    string patientName = patientNames[random.Next(patientNames.Length)];
                    string timeSlot = timeSlots[random.Next(timeSlots.Length)];
                    string purpose = purposes[random.Next(purposes.Length)];
                    string status = statuses[random.Next(statuses.Length)];
                    string doctor = doctors[random.Next(doctors.Length)];

                    _reportData.Rows.Add(appointmentId++, patientId, patientName, date, timeSlot, purpose, status, doctor);
                }
            }
        }

        /// <summary>
        /// Sets up columns for appointments report
        /// </summary>
        private void SetupAppointmentsColumns()
        {
            // Add similar column setup pattern as with inventory
            dgReportData.Columns.Add(new DataGridTextColumn { Header = "Appointment ID", Binding = new Binding("AppointmentID") });
            dgReportData.Columns.Add(new DataGridTextColumn { Header = "Patient ID", Binding = new Binding("PatientID") });
            dgReportData.Columns.Add(new DataGridTextColumn { Header = "Patient Name", Binding = new Binding("PatientName") });
            dgReportData.Columns.Add(new DataGridTextColumn { Header = "Date", Binding = new Binding("AppointmentDate") { StringFormat = "MM/dd/yyyy" } });
            dgReportData.Columns.Add(new DataGridTextColumn { Header = "Time", Binding = new Binding("TimeSlot") });
            dgReportData.Columns.Add(new DataGridTextColumn { Header = "Purpose", Binding = new Binding("Purpose") });
            dgReportData.Columns.Add(new DataGridTextColumn { Header = "Status", Binding = new Binding("Status") });
            dgReportData.Columns.Add(new DataGridTextColumn { Header = "Doctor", Binding = new Binding("DoctorName") });
        }

        /// <summary>
        /// Generates test results report data
        /// </summary>
        private void GenerateTestResultsReport(DateTime startDate, DateTime endDate)
        {
            // Create a new DataTable
            _reportData = new DataTable();

            // Define columns for test results report
            _reportData.Columns.Add("TestID", typeof(int));
            _reportData.Columns.Add("PatientID", typeof(int));
            _reportData.Columns.Add("PatientName", typeof(string));
            _reportData.Columns.Add("TestType", typeof(string));
            _reportData.Columns.Add("TestDate", typeof(DateTime));
            _reportData.Columns.Add("ResultDate", typeof(DateTime));
            _reportData.Columns.Add("Result", typeof(string));
            _reportData.Columns.Add("ReferenceRange", typeof(string));
            _reportData.Columns.Add("TechnicianName", typeof(string));

            // Add sample data (replace with database queries later)
            Random random = new Random();
            string[] patientNames = { "Ahmed Ali", "Fatima Khan", "Mohammed Hassan", "Sara Ahmed", "Omar Mahmoud", "Layla Ibrahim" };
            string[] testTypes = { "Complete Blood Count", "Lipid Panel", "Urinalysis", "Liver Function", "Kidney Function", "Glucose Test" };
            string[] results = { "Normal", "Abnormal - High", "Abnormal - Low", "Inconclusive", "Requires Follow-up" };
            string[] technicians = { "Mahmoud", "Amal", "Youssef", "Nour", "Tariq" };

            int testId = 8001;

            // Generate data for each day in the range
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                // Generate 4-8 test results per day
                int testsPerDay = random.Next(4, 9);
                for (int i = 0; i < testsPerDay; i++)
                {
                    int patientId = random.Next(1001, 1500);
                    string patientName = patientNames[random.Next(patientNames.Length)];
                    string testType = testTypes[random.Next(testTypes.Length)];
                    DateTime resultDate = date.AddDays(random.Next(1, 4)); // Results come 1-3 days after test
                    string result = results[random.Next(results.Length)];
                    string referenceRange = GetReferenceRange(testType);
                    string technician = technicians[random.Next(technicians.Length)];

                    _reportData.Rows.Add(testId++, patientId, patientName, testType, date, resultDate, result, referenceRange, technician);
                }
            }
        }

        /// <summary>
        /// Helper method to get reference range for a test type
        /// </summary>
        private string GetReferenceRange(string testType)
        {
            switch (testType)
            {
                case "Complete Blood Count": return "4.5-11.0 x10^9/L";
                case "Lipid Panel": return "LDL: <100 mg/dL, HDL: >60 mg/dL";
                case "Urinalysis": return "pH: 4.5-8.0";
                case "Liver Function": return "ALT: 7-56 U/L, AST: 8-48 U/L";
                case "Kidney Function": return "Creatinine: 0.7-1.3 mg/dL";
                case "Glucose Test": return "70-99 mg/dL (fasting)";
                default: return "N/A";
            }
        }

        /// <summary>
        /// Sets up columns for test results report
        /// </summary>
        private void SetupTestResultsColumns()
        {
            // Setup columns
            dgReportData.Columns.Add(new DataGridTextColumn { Header = "Test ID", Binding = new Binding("TestID") });
            dgReportData.Columns.Add(new DataGridTextColumn { Header = "Patient ID", Binding = new Binding("PatientID") });
            dgReportData.Columns.Add(new DataGridTextColumn { Header = "Patient Name", Binding = new Binding("PatientName") });
            dgReportData.Columns.Add(new DataGridTextColumn { Header = "Test Type", Binding = new Binding("TestType") });
            dgReportData.Columns.Add(new DataGridTextColumn { Header = "Test Date", Binding = new Binding("TestDate") { StringFormat = "MM/dd/yyyy" } });
            dgReportData.Columns.Add(new DataGridTextColumn { Header = "Result Date", Binding = new Binding("ResultDate") { StringFormat = "MM/dd/yyyy" } });
            dgReportData.Columns.Add(new DataGridTextColumn { Header = "Result", Binding = new Binding("Result") });
            dgReportData.Columns.Add(new DataGridTextColumn { Header = "Reference Range", Binding = new Binding("ReferenceRange") });
            dgReportData.Columns.Add(new DataGridTextColumn { Header = "Technician", Binding = new Binding("TechnicianName") });
        }

        /// <summary>
        /// Generates patient registrations report data
        /// </summary>
        private void GeneratePatientRegistrationsReport(DateTime startDate, DateTime endDate)
        {
            // Create a new DataTable
            _reportData = new DataTable();

            // Define columns for patient registrations report
            _reportData.Columns.Add("PatientID", typeof(int));
            _reportData.Columns.Add("FullName", typeof(string));
            _reportData.Columns.Add("Gender", typeof(string));
            _reportData.Columns.Add("DateOfBirth", typeof(DateTime));
            _reportData.Columns.Add("Phone", typeof(string));
            _reportData.Columns.Add("Email", typeof(string));
            _reportData.Columns.Add("Address", typeof(string));
            _reportData.Columns.Add("RegistrationDate", typeof(DateTime));

            // Add sample data (replace with database queries later)
            Random random = new Random();
            string[] maleNames = { "Ahmed Ali", "Mohammed Hassan", "Omar Mahmoud", "Tariq Ibrahim", "Youssef Khalid" };
            string[] femaleNames = { "Fatima Khan", "Sara Ahmed", "Layla Ibrahim", "Noor Qasim", "Aisha Abdullah" };
            string[] cities = { "Baghdad", "Basra", "Mosul", "Erbil", "Najaf", "Karbala" };

            int patientId = 1001;

            // Generate data for each day in the range
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                // Generate 2-5 registrations per day
                int registrationsPerDay = random.Next(2, 6);
                for (int i = 0; i < registrationsPerDay; i++)
                {
                    bool isMale = random.Next(2) == 0;
                    string fullName = isMale ? maleNames[random.Next(maleNames.Length)] : femaleNames[random.Next(femaleNames.Length)];
                    string gender = isMale ? "Male" : "Female";
                    DateTime dob = DateTime.Now.AddYears(-random.Next(1, 90)).AddDays(-random.Next(1, 365));
                    string phone = $"07{random.Next(10, 100):00}{random.Next(100000, 1000000):000000}";
                    string email = $"{fullName.Replace(" ", ".").ToLower()}@example.com";
                    string city = cities[random.Next(cities.Length)];
                    string address = $"{random.Next(1, 100)} {city} Street, {city}";

                    _reportData.Rows.Add(patientId++, fullName, gender, dob, phone, email, address, date);
                }
            }
        }

        /// <summary>
        /// Sets up columns for patient registrations report
        /// </summary>
        private void SetupPatientRegistrationsColumns()
        {
            // Setup columns
            dgReportData.Columns.Add(new DataGridTextColumn { Header = "Patient ID", Binding = new Binding("PatientID") });
            dgReportData.Columns.Add(new DataGridTextColumn { Header = "Full Name", Binding = new Binding("FullName") });
            dgReportData.Columns.Add(new DataGridTextColumn { Header = "Gender", Binding = new Binding("Gender") });
            dgReportData.Columns.Add(new DataGridTextColumn { Header = "Date of Birth", Binding = new Binding("DateOfBirth") { StringFormat = "MM/dd/yyyy" } });
            dgReportData.Columns.Add(new DataGridTextColumn { Header = "Phone", Binding = new Binding("Phone") });
            dgReportData.Columns.Add(new DataGridTextColumn { Header = "Email", Binding = new Binding("Email") });
            dgReportData.Columns.Add(new DataGridTextColumn { Header = "Address", Binding = new Binding("Address") });
            dgReportData.Columns.Add(new DataGridTextColumn { Header = "Registration Date", Binding = new Binding("RegistrationDate") { StringFormat = "MM/dd/yyyy" } });
        }

        #endregion

        #region UI Helper Methods

        /// <summary>
        /// Resets the filter controls to their default values
        /// </summary>
        private void ResetFilters()
        {
            dpStartDate.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dpEndDate.SelectedDate = DateTime.Now;

            if (cmbReportType.Items.Count > 0)
                cmbReportType.SelectedIndex = 0;

            _isReportGenerated = false;
            btnExportReport.IsEnabled = false;
        }

        /// <summary>
        /// Clears the report data and resets the UI elements
        /// </summary>
        private void ClearReportData()
        {
            _reportData = new DataTable();
            dgReportData.ItemsSource = null;

            txtReportTitle.Text = "Report Results";
            txtReportSummary.Text = "Select a report type and date range above, then click 'Generate Report'";
            txtRecordCount.Text = "0 records";
        }

        /// <summary>
        /// Shows an information panel with a message
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <param name="isSuccess">Whether this is a success message</param>
        private void ShowInfoPanel(string message, bool isSuccess = false)
        {
            txtInfoMessage.Text = message;

            // Change the style based on message type
            if (isSuccess)
            {
                infoPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E7F5E9"));
                infoPanel.BorderBrush = (SolidColorBrush)FindResource("SuccessColor");
                ((PackIconMaterial)infoPanel.FindName("infoIcon")).Kind = PackIconMaterialKind.CheckCircleOutline;
                ((PackIconMaterial)infoPanel.FindName("infoIcon")).Foreground = (SolidColorBrush)FindResource("SuccessColor");
                txtInfoMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1B5E20"));
            }
            else
            {
                infoPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8E1"));
                infoPanel.BorderBrush = (SolidColorBrush)FindResource("WarningColor");
                ((PackIconMaterial)infoPanel.FindName("infoIcon")).Kind = PackIconMaterialKind.InformationOutline;
                ((PackIconMaterial)infoPanel.FindName("infoIcon")).Foreground = (SolidColorBrush)FindResource("WarningColor");
                txtInfoMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7E5400"));
            }

            infoPanel.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hides the information panel
        /// </summary>
        private void HideInfoPanel()
        {
            infoPanel.Visibility = Visibility.Collapsed;
        }

        #endregion
    }
}