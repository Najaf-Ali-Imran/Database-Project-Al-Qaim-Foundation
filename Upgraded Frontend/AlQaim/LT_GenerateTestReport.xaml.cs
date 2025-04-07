using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Win32;

namespace AlQaim
{
    public class SampleViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string SampleId { get; set; }
        public string PatientName { get; set; }
        public string PatientId { get; set; }
        public DateTime CollectionDate { get; set; }
        public string SampleType { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TestResult
    {
        public string TestName { get; set; }
        public string Result { get; set; }
        public string Units { get; set; }
        public string ReferenceRange { get; set; }
        public string Status { get; set; }
        public Brush StatusColor { get; set; }
    }

    public partial class LT_GenerateTestReport : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private DateTime _currentDate;
        public DateTime CurrentDate
        {
            get => _currentDate;
            set { if (_currentDate != value) { _currentDate = value; OnPropertyChanged(); } }
        }

        private ObservableCollection<SampleViewModel> _samples;
        public ObservableCollection<SampleViewModel> Samples // Master list
        {
            get => _samples;
            set { if (_samples != value) { _samples = value; OnPropertyChanged(); } }
        }

        private ObservableCollection<SampleViewModel> _filteredSamples;
        public ObservableCollection<SampleViewModel> FilteredSamples // Displayed list
        {
            get => _filteredSamples;
            set { _filteredSamples = value; OnPropertyChanged(); }
        }

        private bool _isReportGenerated = false;

        public LT_GenerateTestReport()
        {
            InitializeComponent();
            DataContext = this;
            CurrentDate = DateTime.Now;
            LoadSampleData(); // Load data BEFORE initializing placeholder triggers filter
            InitializeSearchBoxPlaceholder();
        }

        private void InitializeSearchBoxPlaceholder()
        {
            SearchBox.Foreground = FindResource("PlaceholderGrayColor") as Brush ?? Brushes.Gray;
            SearchBox.Text = SearchBox.Tag as string ?? "Search samples..."; // Use Tag for placeholder text
        }

        private void LoadSampleData()
        {
            // Sample data for demonstration
            Samples = new ObservableCollection<SampleViewModel>
            {
                new SampleViewModel { SampleId = "S-2025040101", PatientName = "John Smith", PatientId = "P-12345678", CollectionDate = DateTime.Now.AddDays(-3), SampleType = "Blood", IsSelected = false },
                new SampleViewModel { SampleId = "S-2025040102", PatientName = "Emily Johnson", PatientId = "P-87654321", CollectionDate = DateTime.Now.AddDays(-3), SampleType = "Urine", IsSelected = false },
                new SampleViewModel { SampleId = "S-2025040103", PatientName = "Michael Brown", PatientId = "P-23456789", CollectionDate = DateTime.Now.AddDays(-2), SampleType = "Blood", IsSelected = false },
                new SampleViewModel { SampleId = "S-2025040104", PatientName = "Sarah Williams", PatientId = "P-98765432", CollectionDate = DateTime.Now.AddDays(-2), SampleType = "Tissue", IsSelected = false },
                new SampleViewModel { SampleId = "S-2025040105", PatientName = "David Martinez", PatientId = "P-34567890", CollectionDate = DateTime.Now.AddDays(-1), SampleType = "Blood", IsSelected = false }
            };
            ApplySearchFilter(""); // Apply empty filter initially to populate FilteredSamples
            UpdateSelectionInfo(); // Update count initially
        }

        #region Event Handlers

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Brush placeholderBrush = FindResource("PlaceholderGrayColor") as Brush ?? Brushes.Gray;
            // Check the actual text, not just the foreground color
            if (SearchBox.Text != (SearchBox.Tag as string ?? "") || SearchBox.Foreground != placeholderBrush)
            {
                ApplySearchFilter(SearchBox.Text);
            }
            else if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                ApplySearchFilter("");
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Brush placeholderBrush = FindResource("PlaceholderGrayColor") as Brush ?? Brushes.Gray;
            if (SearchBox.Text == (SearchBox.Tag as string ?? "") && SearchBox.Foreground == placeholderBrush)
            {
                SearchBox.Text = "";
                SearchBox.Foreground = FindResource("TextColor") as Brush ?? SystemColors.WindowTextBrush;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                InitializeSearchBoxPlaceholder();
            }
        }

        private void ApplySearchFilter(string searchText)
        {
            // Prevent error during initialization if Samples isn't ready
            if (Samples == null)
            {
                FilteredSamples = new ObservableCollection<SampleViewModel>();
                return;
            }

            ObservableCollection<SampleViewModel> filteredList;
            if (string.IsNullOrWhiteSpace(searchText) || searchText == (SearchBox.Tag as string ?? ""))
            {
                filteredList = new ObservableCollection<SampleViewModel>(Samples);
            }
            else
            {
                string lowerSearchText = searchText.ToLower();
                var query = Samples.Where(s =>
                    (s.SampleId?.ToLower().Contains(lowerSearchText) ?? false) ||
                    (s.PatientName?.ToLower().Contains(lowerSearchText) ?? false) ||
                    (s.PatientId?.ToLower().Contains(lowerSearchText) ?? false)
                );
                filteredList = new ObservableCollection<SampleViewModel>(query);
            }
            FilteredSamples = filteredList;
        }


        // Handles clicks on the checkbox itself to update selection count/button state
        private void SampleCheckBox_Click(object sender, RoutedEventArgs e)
        {
            // Use Dispatcher to ensure the IsSelected binding updates before we count.
            Dispatcher.BeginInvoke(new Action(() => {
                UpdateSelectionInfo();
            }), System.Windows.Threading.DispatcherPriority.ContextIdle);
        }

        // Updates the selection count text and Generate button state
        private void UpdateSelectionInfo()
        {
            if (Samples == null) return; // Safety check

            int selectedCount = Samples.Count(s => s.IsSelected);
            SelectedCountText.Text = $"{selectedCount} sample{(selectedCount != 1 ? "s" : "")} selected";
            GenerateButton.IsEnabled = selectedCount > 0;

            // Also reset action buttons if no samples are selected after generation occurred
            if (selectedCount == 0 && _isReportGenerated)
            {
                SetActionButtonsEnabled(false);
            }
        }

        private void ClearSelection_Click(object sender, RoutedEventArgs e)
        {
            if (Samples == null) return;
            foreach (var sample in Samples)
            {
                sample.IsSelected = false;
            }
            UpdateSelectionInfo();
        }


        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            List<SampleViewModel> selectedSamples = Samples?.Where(s => s.IsSelected).ToList() ?? new List<SampleViewModel>();
            if (!selectedSamples.Any())
            {
                MessageBox.Show("Please select at least one sample.", "No Samples Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            UpdateStatus("Generating report...", "\uE895");

            var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                DisplayReport(selectedSamples);
            };
            timer.Start();
        }

        private void DisplayReport(List<SampleViewModel> selectedSamples)
        {
            if (selectedSamples == null || !selectedSamples.Any()) return;

            NoReportPlaceholder.Visibility = Visibility.Collapsed;
            ReportContent.Visibility = Visibility.Visible;

            ReportDateText.Text = $"Date: {DateTime.Now:MM/dd/yy}";
            ReportIdText.Text = $"ID: LAB-{DateTime.Now:yyMMdd}{new Random().Next(10, 99)}";

            // Populate header with first selected sample's info (simplification)
            var firstSample = selectedSamples.First();
            PatientNameText.Text = firstSample.PatientName ?? "[N/A]";
            PatientIdText.Text = firstSample.PatientId ?? "[N/A]";
            PatientDobText.Text = "[Fetch DOB]"; // TODO: Fetch real data
            PatientGenderText.Text = "[Fetch Gender]"; // TODO: Fetch real data

            SampleIdText.Text = firstSample.SampleId ?? "[N/A]";
            SampleTypeText.Text = firstSample.SampleType ?? "[N/A]";
            CollectionDateText.Text = firstSample.CollectionDate.ToString("MM/dd/yy HH:mm");
            ReceivedDateText.Text = firstSample.CollectionDate.AddHours(new Random().Next(1, 4)).ToString("MM/dd/yy HH:mm"); // Example

            // TODO: Populate TestResultsItemsControl.ItemsSource with actual fetched data
            // Example: TestResultsItemsControl.ItemsSource = GetTestResultsForSamples(selectedSamples);

            CommentsText.Text = "[Generate comments based on results...]"; // TODO: Generate comments
            LabDirectorText.Text = "[Director Name]"; // TODO: Fetch real data
            TechnicianText.Text = "[Technician Name]"; // TODO: Fetch real data

            // TODO: Apply configuration CheckBox states (e.g., hide/show logo, signatures)

            if (TryFindResource("FadeInStoryboard") is Storyboard fadeIn)
            {
                Storyboard.SetTarget(fadeIn, ReportContent);
                fadeIn.Begin();
            }

            UpdateStatus("Report generated successfully.", "\uE734");
            SetActionButtonsEnabled(true);
            _isReportGenerated = true;
        }


        private void RefreshPreview_Click(object sender, RoutedEventArgs e)
        {
            if (!_isReportGenerated)
            {
                UpdateStatus("Generate a report first to refresh.", "\uE8F1");
                return;
            }

            List<SampleViewModel> selectedSamples = Samples?.Where(s => s.IsSelected).ToList() ?? new List<SampleViewModel>();
            if (!selectedSamples.Any())
            {
                ResetReportView();
                return;
            }

            UpdateStatus("Refreshing report preview...", "\uE72C");

            var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(0.5) };
            timer.Tick += (s, args) => {
                timer.Stop();
                DisplayReport(selectedSamples); // Re-display with potentially updated data/options
                UpdateStatus("Report preview refreshed.", "\uE734");
            };
            timer.Start();
        }

        private void FullScreen_Click(object sender, RoutedEventArgs e)
        {
            if (!_isReportGenerated) return;
            MessageBox.Show("Full screen preview not implemented.", "Full Screen", MessageBoxButton.OK, MessageBoxImage.Information);
            // Actual implementation would likely open a new Window.
        }


        private void PrintReport_Click(object sender, RoutedEventArgs e)
        {
            if (!_isReportGenerated) return;
            UpdateStatus("Preparing print...", "\uE749");

            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                try
                {
                    // Ensure the visual is ready for printing
                    ReportContent.Measure(new Size(printDialog.PrintableAreaWidth, double.PositiveInfinity));
                    ReportContent.Arrange(new Rect(new Point(0, 0), ReportContent.DesiredSize));
                    ReportContent.UpdateLayout();

                    UpdateStatus("Printing report...", "\uE749");
                    printDialog.PrintVisual(ReportContent, $"Test Report - {DateTime.Now:yyyy-MM-dd}");
                    UpdateStatus("Report sent to printer.", "\uE73E");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error printing report: {ex.Message}", "Print Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    UpdateStatus("Printing failed.", "\uE783");
                }
            }
            else
            {
                UpdateStatus("Printing cancelled.", "\uE734");
            }
        }

        private void ExportReport_Click(object sender, RoutedEventArgs e)
        {
            if (!_isReportGenerated) return;

            string format = (ReportFormatComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "PDF Document";
            string defaultExt = ".pdf";
            string filter = "PDF Documents (*.pdf)|*.pdf";

            switch (format)
            {
                case "CSV Spreadsheet": defaultExt = ".csv"; filter = "CSV Files (*.csv)|*.csv"; break;
                case "Excel Workbook": defaultExt = ".xlsx"; filter = "Excel Files (*.xlsx)|*.xlsx"; break;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = filter,
                DefaultExt = defaultExt,
                FileName = $"TestReport_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                UpdateStatus($"Exporting as {format}...", "\uE73E");

                bool success = false;
                try
                {
                    // TODO: Replace with actual export implementations
                    switch (format)
                    {
                        case "PDF Document": success = ExportToPdf(filePath, ReportContent); break;
                        case "CSV Spreadsheet": success = ExportToCsv(filePath, Samples.Where(s => s.IsSelected).ToList()); break;
                        case "Excel Workbook": success = ExportToExcel(filePath, Samples.Where(s => s.IsSelected).ToList()); break;
                    }

                    if (success) UpdateStatus($"Report exported to {Path.GetFileName(filePath)}", "\uE73E");
                    else throw new NotImplementedException($"Export function for {format} is not implemented or failed.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting report: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    UpdateStatus($"Export failed.", "\uE783");
                }
            }
            else
            {
                UpdateStatus("Export cancelled.", "\uE734");
            }
        }

        private void EmailReport_Click(object sender, RoutedEventArgs e)
        {
            if (!_isReportGenerated) return;
            UpdateStatus("Preparing email...", "\uE715");

            // TODO: Implement actual email logic (export temp file, use MAPI/SMTP)
            string tempPdfPath = Path.Combine(Path.GetTempPath(), $"Report_{Guid.NewGuid()}.pdf");
            try
            {
                if (ExportToPdf(tempPdfPath, ReportContent)) // Use placeholder export for demo
                {
                    MessageBox.Show($"Report exported temporarily to:\n{tempPdfPath}\n\n(Email client integration not implemented.)",
                                     "Email Report (Demo)", MessageBoxButton.OK, MessageBoxImage.Information);
                    UpdateStatus("Email prepared (demo).", "\uE715");
                }
                else
                {
                    throw new Exception("Failed to create temporary report file.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error preparing email: {ex.Message}", "Email Error", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateStatus("Email preparation failed.", "\uE783");
            }
            // Note: Temp file cleanup would be needed in a real implementation
        }

        #endregion

        #region Helper Methods

        private void UpdateStatus(string message, string iconGlyph)
        {
            ReportStatusText.Text = message;
            ReportStatusIcon.Text = iconGlyph;
        }

        private void SetActionButtonsEnabled(bool isEnabled)
        {
            PrintButton.IsEnabled = isEnabled;
            ExportButton.IsEnabled = isEnabled;
            EmailButton.IsEnabled = isEnabled;
        }

        private void ResetReportView()
        {
            ReportContent.Visibility = Visibility.Collapsed;
            NoReportPlaceholder.Visibility = Visibility.Visible;
            UpdateStatus("Select samples and generate report.", "\uE734");
            SetActionButtonsEnabled(false);
            _isReportGenerated = false;
        }

        // --- Placeholder Export Methods ---
        // Replace these with calls to actual export libraries (QuestPDF, EPPlus, CsvHelper, etc.)
        private bool ExportToPdf(string filePath, FrameworkElement visualToExport)
        {
            // TODO: Implement PDF export using a library like QuestPDF or PdfSharp
            Console.WriteLine($"Placeholder: Exporting PDF to {filePath}");
            try { File.WriteAllText(filePath, "Placeholder PDF export content."); return true; } // Dummy success
            catch (Exception ex) { Console.WriteLine($"PDF Export Error: {ex.Message}"); return false; }
        }

        private bool ExportToCsv(string filePath, List<SampleViewModel> selectedSamples)
        {
            // TODO: Implement CSV export, including actual test results for selected samples
            Console.WriteLine($"Placeholder: Exporting CSV to {filePath}");
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("SampleId,PatientName,PatientId,CollectionDate,SampleType"); // Header
                    foreach (var sample in selectedSamples)
                    {
                        writer.WriteLine($"\"{sample.SampleId}\",\"{sample.PatientName}\",\"{sample.PatientId}\",\"{sample.CollectionDate:O}\",\"{sample.SampleType}\"");
                        // Fetch and write actual test results for this sample...
                    }
                }
                return true;
            }
            catch (Exception ex) { Console.WriteLine($"CSV Export Error: {ex.Message}"); return false; }
        }

        private bool ExportToExcel(string filePath, List<SampleViewModel> selectedSamples)
        {
            // TODO: Implement Excel export using a library like EPPlus or ClosedXML
            Console.WriteLine($"Placeholder: Exporting Excel to {filePath}");
            try { File.WriteAllText(filePath, "SampleId\tPatientName\nValue1\tValue2"); return true; } // Dummy success
            catch (Exception ex) { Console.WriteLine($"Excel Export Error: {ex.Message}"); return false; }
        }

        #endregion
    }
}