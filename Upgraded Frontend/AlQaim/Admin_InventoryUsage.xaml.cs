using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Media;
using System.Windows.Threading;       
using MahApps.Metro.IconPacks;         
using System.Windows.Media.Animation;  


namespace AlQaim
{
    public partial class Admin_InventoryUsage : Page, INotifyPropertyChanged // Consider IDisposable
    {
        // Properties for chart binding
        private SeriesCollection _pieChartData;
        public SeriesCollection PieChartData
        {
            get { return _pieChartData; }
            set
            {
                _pieChartData = value;
                OnPropertyChanged(nameof(PieChartData)); // Use nameof() for safety
            }
        }

        // CollectionViewSource for filtering
        private CollectionViewSource UsageViewSource;

        // Event for property changed notification
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // --- Additions for Toast ---
        private DispatcherTimer _toastTimer;
        private InventoryUsageModel _usageToDelete; // To hold item for deletion confirmation
        // --- End Additions ---

        public Admin_InventoryUsage()
        {
            InitializeComponent();

            // Set DataContext for binding
            this.DataContext = this;

            // Initialize date pickers with default values
            dpStartDate.SelectedDate = DateTime.Now.AddMonths(-1);
            dpEndDate.SelectedDate = DateTime.Now;

            // Set up pie chart
            InitializeChartData();

            // --- Additions for Toast ---
            // Initialize the toast timer
            _toastTimer = new DispatcherTimer();
            _toastTimer.Interval = TimeSpan.FromSeconds(4); // Slightly longer duration?
            _toastTimer.Tick += ToastTimer_Tick;
            // --- End Additions ---

            // Load initial data
            LoadUsageData(); // Load data after timer initialization
        }

        // --- Additions for Toast ---
        private void ToastTimer_Tick(object sender, EventArgs e)
        {
            _toastTimer.Stop();
            HideToast();
        }
        // --- End Additions ---


        private void InitializeChartData()
        {
            // ... (your existing chart initialization)
            PieChartData = new SeriesCollection
            {
                new PieSeries { Title = "Reagents", Values = new ChartValues<double> { 43 }, Fill = FindResource("PrimaryAccentColor") as SolidColorBrush, DataLabels = true },
                new PieSeries { Title = "Consumables", Values = new ChartValues<double> { 24 }, Fill = FindResource("SecondaryAccentColor") as SolidColorBrush, DataLabels = true },
                new PieSeries { Title = "Equipment", Values = new ChartValues<double> { 18 }, Fill = FindResource("SuccessColor") as SolidColorBrush, DataLabels = true },
                new PieSeries { Title = "Others", Values = new ChartValues<double> { 15 }, Fill = FindResource("WarningColor") as SolidColorBrush, DataLabels = true }
            }; // Using resource colors might be better
        }

        private void LoadUsageData()
        {
            // ... (your existing data loading logic)
            var usageData = new ObservableCollection<InventoryUsageModel> { /* ... Sample Data ... */ };
            UsageViewSource = new CollectionViewSource { Source = usageData };
            dgUsageLogs.ItemsSource = UsageViewSource.View;
            UpdateMetrics(usageData);
        }

        private void UpdateMetrics(ObservableCollection<InventoryUsageModel> data)
        {
            // ... (your existing metric update logic)
            txtTotalUsage.Text = "2,457"; /* ... etc ... */
        }

        #region Event Handlers

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters(); // Centralize filtering logic
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters(); // Centralize filtering logic
        }

        // --- Add Filter Logic ---
        private void ApplyFilters()
        {
            if (UsageViewSource?.View == null) return; // Guard clause

            UsageViewSource.View.Filter = item =>
            {
                var usage = item as InventoryUsageModel;
                if (usage == null) return false;

                // Date Filter
                bool dateInRange = true; // Default to true if dates are not set
                if (dpStartDate.SelectedDate.HasValue && dpEndDate.SelectedDate.HasValue)
                {
                    // Ensure start date is not after end date
                    if (dpStartDate.SelectedDate.Value > dpEndDate.SelectedDate.Value)
                    {
                        // Optionally show a validation toast here, but filtering might be enough
                        // ShowToast("Start date cannot be after end date.", PackIconMaterialKind.CalendarAlert);
                        return false; // Or handle appropriately
                    }
                    DateTime startDate = dpStartDate.SelectedDate.Value.Date;
                    DateTime endDate = dpEndDate.SelectedDate.Value.Date.AddDays(1).AddTicks(-1); // End of the day
                    dateInRange = usage.UsageDate >= startDate && usage.UsageDate <= endDate;
                }
                else if (dpStartDate.SelectedDate.HasValue) // Only start date set
                {
                    dateInRange = usage.UsageDate.Date >= dpStartDate.SelectedDate.Value.Date;
                }
                else if (dpEndDate.SelectedDate.HasValue) // Only end date set
                {
                    dateInRange = usage.UsageDate.Date <= dpEndDate.SelectedDate.Value.Date;
                }


                // Text Search Filter
                string searchText = txtSearch.Text?.ToLower() ?? string.Empty; // Null check
                bool matchesSearch = string.IsNullOrEmpty(searchText) ||
                                   (usage.ItemName?.ToLower().Contains(searchText) ?? false) ||
                                   (usage.UsageID?.ToLower().Contains(searchText) ?? false) ||
                                   (usage.UsedBy?.ToLower().Contains(searchText) ?? false) ||
                                   (usage.TestName?.ToLower().Contains(searchText) ?? false);

                return dateInRange && matchesSearch;
            };
        }
        // --- End Filter Logic ---


        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            // Clear filters visually
            txtSearch.Clear();
            dpStartDate.SelectedDate = DateTime.Now.AddMonths(-1);
            dpEndDate.SelectedDate = DateTime.Now;

            // Reload data
            LoadUsageData();
            // --- Add Toast ---
            ShowToast("Inventory usage data refreshed", PackIconMaterialKind.Refresh, useInfoColor: true);
            // --- End Add Toast ---
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv",
                DefaultExt = ".xlsx", // Changed default ext
                FileName = $"InventoryUsage_{DateTime.Now:yyyyMMdd}.xlsx", // Default file name
                Title = "Export Usage Data"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // *** Add actual export logic here (e.g., using EPPlus or CsvHelper) ***
                    // Example placeholder:
                    bool exportSuccessful = true; // Replace with result of actual export

                    if (exportSuccessful)
                    {
                        // --- Modify MessageBox ---
                        // MessageBox.Show($"Data exported successfully to {saveFileDialog.FileName}", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                        ShowToast($"Data exported to {System.IO.Path.GetFileName(saveFileDialog.FileName)}", PackIconMaterialKind.FileExportOutline, useInfoColor: true);
                        // --- End Modify ---
                    }
                    else
                    {
                        // --- Add Error Toast ---
                        ShowToast("Export failed. Could not write file.", PackIconMaterialKind.AlertCircleOutline);
                        // --- End Error Toast ---
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception details (ex.ToString()) for debugging
                    // --- Modify MessageBox ---
                    // MessageBox.Show($"Error exporting data: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    ShowToast($"Error exporting data: {ex.Message}", PackIconMaterialKind.AlertCircleOutline);
                    // --- End Modify ---
                }
            }
        }

        private void BtnView_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var usage = button?.DataContext as InventoryUsageModel; // Null check

            if (usage != null)
            {
                DisplayUsageDetails(usage);
                modalOverlay.Visibility = Visibility.Visible;
            }
            else
            {
                ShowToast("Could not load usage details.", PackIconMaterialKind.AlertCircleOutline);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var usage = button?.DataContext as InventoryUsageModel; // Null check

            if (usage != null)
            {
                _usageToDelete = usage; // Store for confirmation

                // --- Keep MessageBox for Confirmation (as requested implicitly) ---
                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to delete usage record '{usage.UsageID}' for '{usage.ItemName}'?", // More informative message
                    "Confirm Deletion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // In a real app, this would delete from the database
                    var source = UsageViewSource?.Source as ObservableCollection<InventoryUsageModel>;
                    if (source != null && source.Contains(_usageToDelete)) // Check if it exists
                    {
                        source.Remove(_usageToDelete);
                        // --- Add Success Toast ---
                        ShowToast($"Usage record '{_usageToDelete.UsageID}' deleted", PackIconMaterialKind.Delete);
                        // --- End Success Toast ---
                    }
                    else
                    {
                        ShowToast($"Could not find usage record '{_usageToDelete?.UsageID}' to delete.", PackIconMaterialKind.AlertCircleOutline);
                    }
                    _usageToDelete = null; // Clear after operation
                }
                else
                {
                    _usageToDelete = null; // Clear if cancelled
                }
                // --- End MessageBox ---
            }
            else
            {
                ShowToast("Could not identify record to delete.", PackIconMaterialKind.AlertCircleOutline);
            }
        }

        private void DgUsageLogs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle selection if needed
        }

        private void BtnCloseModal_Click(object sender, RoutedEventArgs e)
        {
            modalOverlay.Visibility = Visibility.Collapsed;
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            // --- Replace MessageBox ---
            // MessageBox.Show("Printing functionality would be implemented here.", "Print", MessageBoxButton.OK, MessageBoxImage.Information);
            ShowToast("Print action triggered (implementation needed)", PackIconMaterialKind.Printer, useInfoColor: true);
            // --- End Replace ---
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // --- Replace MessageBox ---
            // MessageBox.Show("Edit functionality would be implemented here.", "Edit", MessageBoxButton.OK, MessageBoxImage.Information);
            ShowToast("Edit action triggered (implementation needed)", PackIconMaterialKind.Pencil, useInfoColor: true);
            // --- End Replace ---
            // Potentially close the current modal and open an edit modal
            // modalOverlay.Visibility = Visibility.Collapsed;
            // // Open edit modal here...
        }

        private void BtnDeleteConfirm_Click(object sender, RoutedEventArgs e)
        {
            // This button is inside the modal, acting on the currently viewed item
            string usageId = txtUsageId.Text; // Assumes txtUsageId is populated correctly in DisplayUsageDetails

            // Find the item based on the ID shown in the modal
            var source = UsageViewSource?.Source as ObservableCollection<InventoryUsageModel>;
            _usageToDelete = source?.FirstOrDefault(u => u.UsageID == usageId);


            if (_usageToDelete != null)
            {
                // --- Keep MessageBox for Confirmation ---
                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to delete usage record '{_usageToDelete.UsageID}' for '{_usageToDelete.ItemName}'?", // More informative message
                    "Confirm Deletion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // Actual deletion
                    if (source != null && source.Contains(_usageToDelete))
                    {
                        source.Remove(_usageToDelete);
                        modalOverlay.Visibility = Visibility.Collapsed; // Close modal on success
                                                                        // --- Add Success Toast ---
                        ShowToast($"Usage record '{_usageToDelete.UsageID}' deleted", PackIconMaterialKind.Delete);
                        // --- End Success Toast ---
                    }
                    else
                    {
                        ShowToast($"Could not find usage record '{_usageToDelete.UsageID}' to delete.", PackIconMaterialKind.AlertCircleOutline);
                    }
                    _usageToDelete = null; // Clear after operation
                }
                else
                {
                    _usageToDelete = null; // Clear if cancelled
                }
                // --- End MessageBox ---
            }
            else
            {
                ShowToast($"Could not identify record '{usageId}' to delete.", PackIconMaterialKind.AlertCircleOutline);
            }
        }


        // --- Additions for Toast ---
        private void ShowToast(string message, PackIconMaterialKind iconKind, bool useInfoColor = false)
        {
            toastMessage.Text = message;
            toastIcon.Kind = iconKind;

            // Set background color based on type
            if (iconKind == PackIconMaterialKind.AlertCircleOutline || iconKind == PackIconMaterialKind.Alert || iconKind == PackIconMaterialKind.CalendarAlert)
            {
                toastNotification.Background = FindResource("DangerColor") as SolidColorBrush;
            }
            else if (iconKind == PackIconMaterialKind.Delete || iconKind == PackIconMaterialKind.Alert) // Warning/Deletion Confirmation
            {
                toastNotification.Background = FindResource("WarningColor") as SolidColorBrush;
            }
            else if (useInfoColor) // For Refresh, Export Info etc.
            {
                toastNotification.Background = FindResource("InfoColor") as SolidColorBrush;
            }
            else // Default to Success
            {
                toastNotification.Background = FindResource("PrimaryAccentColor") as SolidColorBrush; // Success color
            }

            toastNotification.Visibility = Visibility.Visible;

            var fadeIn = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(300) };
            toastNotification.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            _toastTimer.Stop(); // Stop previous timer if any
            _toastTimer.Start();
        }

        private void HideToast()
        {
            var fadeOut = new DoubleAnimation { From = toastNotification.Opacity, To = 0, Duration = TimeSpan.FromMilliseconds(300) }; // Fade from current opacity
            fadeOut.Completed += (s, e) => {
                // Check visibility again in case another toast was shown quickly
                if (toastNotification.Opacity < 0.1) // Only hide if it's still faded out
                {
                    toastNotification.Visibility = Visibility.Collapsed;
                }
            };
            toastNotification.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        private void BtnCloseToast_Click(object sender, RoutedEventArgs e)
        {
            _toastTimer.Stop(); // Stop timer if manually closed
            HideToast();
        }

        // Optional: Add cleanup if needed when the page is unloaded
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_toastTimer != null && _toastTimer.IsEnabled)
            {
                _toastTimer.Stop();
            }
            // Dispose other resources if necessary
        }

        // --- End Additions ---


        #endregion

        private void DisplayUsageDetails(InventoryUsageModel usage)
        {
            // ... (Your existing DisplayUsageDetails logic) ...
            txtUsageId.Text = usage.UsageID; /* ... etc ... */
            txtNotes.Text = GetNotes(usage.UsageID);
        }

        #region Helper Methods (Simulating database lookups)
        // ... (Your existing Helper Methods) ...
        private string GetItemCode(string itemName) { /*...*/ return "N/A"; }
        private string GetCurrentStock(string itemName) { /*...*/ return "N/A"; }
        private string GetCategory(string itemName) { /*...*/ return "Other"; }
        private string GetLocation(string itemName) { /*...*/ return "Unknown"; }
        private string GetDepartment(string userName) { /*...*/ return "Unknown"; }
        private string GetNotes(string usageId) { /*...*/ return "No notes available."; }
        #endregion
    }

    // Model class for Inventory Usage data
    public class InventoryUsageModel
    {
        public string UsageID { get; set; }
        public string ItemName { get; set; }
        public string UsedQuantity { get; set; }
        public string Unit { get; set; }
        public DateTime UsageDate { get; set; }
        public string UsedBy { get; set; }
        public string TestName { get; set; }

        // Consider adding INotifyPropertyChanged if properties can change while displayed
    }
}