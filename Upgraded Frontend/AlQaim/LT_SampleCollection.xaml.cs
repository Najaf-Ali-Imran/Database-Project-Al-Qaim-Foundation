using System;
using System.Collections.Generic;
using System.Collections.ObjectModel; // Needed for ObservableCollection
using System.ComponentModel; // Needed for INotifyPropertyChanged (optional but good practice)
using System.Globalization; // Needed for DateTime parsing
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

// Namespace changed to match XAML's x:Class
namespace AlQaim
{
    // Basic class definition for DataGrid items (Ideally in its own file: SampleItem.cs)
    // Implementing INotifyPropertyChanged is good practice for UI updates if properties change later
    public class SampleItem : INotifyPropertyChanged
    {
        private string _sampleId;
        private string _sampleType;
        private DateTime _collectionDate;

        public string SampleId
        {
            get => _sampleId;
            set { _sampleId = value; OnPropertyChanged(nameof(SampleId)); }
        }

        public string SampleType
        {
            get => _sampleType;
            set { _sampleType = value; OnPropertyChanged(nameof(SampleType)); }
        }

        public DateTime CollectionDate
        {
            get => _collectionDate;
            set { _collectionDate = value; OnPropertyChanged(nameof(CollectionDate)); }
        }

        // Optional: Add properties for Patient ID/Name if needed in the grid
        // public int PatientId { get; set; }
        // public string PatientName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Constructor for easy creation
        public SampleItem(string sampleId, string sampleType, DateTime collectionDate)
        {
            SampleId = sampleId;
            SampleType = sampleType;
            CollectionDate = collectionDate;
        }
        public SampleItem() { } // Default constructor might be needed
    }


    /// <summary>
    /// Interaction logic for SampleCollection.xaml
    /// </summary>
    // Class name changed to match XAML's x:Class
    public partial class LT_SampleCollection : Page
    {
        // ObservableCollection automatically updates the UI when items are added/removed
        public ObservableCollection<SampleItem> RecentSamples { get; set; }

        public LT_SampleCollection()
        {
            InitializeComponent();

            // Initialize the collection for the DataGrid
            RecentSamples = new ObservableCollection<SampleItem>();

            // Set the DataContext for data binding (specifically for the RecentSamples collection)
            // This assumes the Page itself is the DataContext. A ViewModel is often preferred.
            this.DataContext = this;

            // Load initial data (e.g., recent samples, patient list) - Placeholder example
            LoadInitialData();
        }

        private void LoadInitialData()
        {
            // --- Populate Patient ComboBox (Example) ---
            // In a real app, load this from a database or service
            var patients = new List<object> {
                new { FullName = "John Doe (ID: P10045)", PatientId = 10045 },
                new { FullName = "Jane Smith (ID: P10046)", PatientId = 10046 },
                new { FullName = "Michael Johnson (ID: P10047)", PatientId = 10047 }
            };
            PatientComboBox.ItemsSource = patients;
            // PatientComboBox.DisplayMemberPath = "FullName"; // Set in XAML
            // PatientComboBox.SelectedValuePath = "PatientId"; // Set in XAML


            // --- Populate Recent Samples DataGrid (Example) ---
            // In a real app, load this from a database or service
            RecentSamples.Add(new SampleItem("SAM-20250401-0001", "Blood", DateTime.Parse("2025-04-01")));
            RecentSamples.Add(new SampleItem("SAM-20250401-0002", "Urine", DateTime.Parse("2025-04-01")));
            RecentSamples.Add(new SampleItem("SAM-20250331-0005", "Saliva", DateTime.Parse("2025-03-31")));
            RecentSamples.Add(new SampleItem("SAM-20250331-0004", "Swab", DateTime.Parse("2025-03-31")));
            RecentSamples.Add(new SampleItem("SAM-20250331-0003", "Blood", DateTime.Parse("2025-03-31")));

            // Set default values for Date/Time pickers
            CollectionDatePicker.SelectedDate = DateTime.Now;
            CollectionTimeTextBox.Text = DateTime.Now.ToString("HH:mm");
        }


        // --- Event Handlers (Implement actual logic here) ---

        private void GenerateSampleId_Click(object sender, RoutedEventArgs e)
        {
            // Implement logic to generate a unique Sample ID
            // Example: Combine date and a sequential number
            string datePart = DateTime.Now.ToString("yyyyMMdd");
            // This sequential part needs proper handling (e.g., query DB for last ID)
            string sequencePart = (RecentSamples.Count + 1).ToString("D4");
            SampleIdTextBox.Text = $"SAM-{datePart}-{sequencePart}";
            HideValidationError(SampleIdErrorMessage); // Hide error if previously shown
            ShowFeedback("Sample ID Generated.", isError: false, temporary: true);
        }

        private void ClearForm_Click(object sender, RoutedEventArgs e)
        {
            // Reset all form fields to default state
            PatientComboBox.SelectedIndex = -1; // Clear selection
            PatientComboBox.Text = string.Empty; // Clear text if editable
            SampleIdTextBox.Text = "SAM-YYYYMMDD-NNNN"; // Reset placeholder
            SampleTypeComboBox.SelectedIndex = -1; // Clear selection
            CollectionDatePicker.SelectedDate = DateTime.Now;
            CollectionTimeTextBox.Text = DateTime.Now.ToString("HH:mm");
            NotesTextBox.Text = string.Empty;

            // Hide any validation messages
            HideValidationError(PatientErrorMessage);
            HideValidationError(SampleIdErrorMessage);
            HideValidationError(SampleTypeErrorMessage);
            HideValidationError(DateErrorMessage);
            HideValidationError(TimeErrorMessage);
            HideFeedback(); // Hide general feedback message
        }

        private void SubmitSample_Click(object sender, RoutedEventArgs e)
        {
            HideFeedback(); // Clear previous feedback

            // 1. Validate Form Inputs
            if (!ValidateForm())
            {
                ShowFeedback("Please correct the errors before submitting.", isError: true);
                return; // Stop submission if validation fails
            }

            // 2. Create Sample Item from Form Data
            try
            {
                DateTime collectionDate = CollectionDatePicker.SelectedDate.Value;
                TimeSpan collectionTime;

                // Use TryParseExact for robust time parsing
                if (!TimeSpan.TryParseExact(CollectionTimeTextBox.Text, "hh\\:mm", CultureInfo.InvariantCulture, out collectionTime) &&
                    !TimeSpan.TryParseExact(CollectionTimeTextBox.Text, "HH\\:mm", CultureInfo.InvariantCulture, out collectionTime))
                {
                    // This should ideally be caught by ValidateForm, but double-check
                    ShowValidationError(TimeErrorMessage, "Invalid time format. Use HH:mm.");
                    ShowFeedback("Invalid time format.", isError: true);
                    return;
                }


                DateTime collectionDateTime = collectionDate.Date + collectionTime;

                var newSample = new SampleItem(
                    SampleIdTextBox.Text,
                    (SampleTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? SampleTypeComboBox.Text, // Handle editable combo box
                    collectionDateTime
                // You might want to add PatientId/Name here too
                );
                // Add Notes if necessary to SampleItem or handle separately
                // newSample.Notes = NotesTextBox.Text;

                // 3. TODO: Save the Sample Item (e.g., to database)
                // Database.SaveSample(newSample);

                // 4. Add to Recent Samples List (Simulates saving and refreshing)
                // Add to the top of the list for visibility
                RecentSamples.Insert(0, newSample);

                // 5. Show Success Feedback & Clear Form
                ShowFeedback("Sample successfully submitted!", isError: false);
                ClearForm_Click(sender, e); // Reuse clear logic
            }
            catch (Exception ex)
            {
                // Log the exception details (ex.ToString())
                ShowFeedback($"An error occurred: {ex.Message}", isError: true);
                // Consider more specific error handling
            }
        }

        private void ViewSample_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is SampleItem sample)
            {
                // Implement logic to view details of the selected sample
                // This could open a new window/dialog or navigate to a details page.
                MessageBox.Show($"Viewing details for Sample ID: {sample.SampleId}\n" +
                                $"Type: {sample.SampleType}\n" +
                                $"Collected: {sample.CollectionDate:yyyy-MM-dd HH:mm}",
                                "Sample Details");
                // Example navigation (if using a Frame):
                // NavigationService?.Navigate(new SampleDetailsPage(sample.SampleId));
            }
        }

        // --- Validation Logic ---

        private bool ValidateForm()
        {
            bool isValid = true;

            // Validate Patient
            if (PatientComboBox.SelectedValue == null && string.IsNullOrWhiteSpace(PatientComboBox.Text))
            {
                ShowValidationError(PatientErrorMessage, "Please select or enter a patient.");
                isValid = false;
            }
            else
            {
                HideValidationError(PatientErrorMessage);
            }

            // Validate Sample ID
            if (string.IsNullOrWhiteSpace(SampleIdTextBox.Text) || SampleIdTextBox.Text == "SAM-YYYYMMDD-NNNN")
            {
                ShowValidationError(SampleIdErrorMessage, "Please generate a Sample ID.");
                isValid = false;
            }
            else
            {
                // Optional: Add check for uniqueness if needed here (might be slow)
                HideValidationError(SampleIdErrorMessage);
            }

            // Validate Sample Type
            if (SampleTypeComboBox.SelectedItem == null && string.IsNullOrWhiteSpace(SampleTypeComboBox.Text))
            {
                ShowValidationError(SampleTypeErrorMessage, "Please select a sample type.");
                isValid = false;
            }
            else
            {
                HideValidationError(SampleTypeErrorMessage);
            }

            // Validate Collection Date
            if (CollectionDatePicker.SelectedDate == null)
            {
                ShowValidationError(DateErrorMessage, "Please select a valid collection date.");
                isValid = false;
            }
            // Optional: Add check if date is in the future?
            // else if (CollectionDatePicker.SelectedDate.Value.Date > DateTime.Now.Date) { ... }
            else
            {
                HideValidationError(DateErrorMessage);
            }

            // Validate Collection Time
            if (string.IsNullOrWhiteSpace(CollectionTimeTextBox.Text))
            {
                ShowValidationError(TimeErrorMessage, "Please enter a collection time.");
                isValid = false;
            }
            else if (!TimeSpan.TryParseExact(CollectionTimeTextBox.Text, "hh\\:mm", CultureInfo.InvariantCulture, out _) &&
                     !TimeSpan.TryParseExact(CollectionTimeTextBox.Text, "HH\\:mm", CultureInfo.InvariantCulture, out _))
            {
                ShowValidationError(TimeErrorMessage, "Invalid time format. Use HH:mm (e.g., 14:30).");
                isValid = false;
            }
            else
            {
                HideValidationError(TimeErrorMessage);
            }

            // Notes field is optional, so no validation needed unless required

            return isValid;
        }

        private void ShowValidationError(TextBlock errorTextBlock, string message)
        {
            errorTextBlock.Text = message;
            errorTextBlock.Visibility = Visibility.Visible;
        }

        private void HideValidationError(TextBlock errorTextBlock)
        {
            errorTextBlock.Visibility = Visibility.Collapsed;
            errorTextBlock.Text = ""; // Clear text
        }

        // --- Feedback Message Logic ---

        private async void ShowFeedback(string message, bool isError = false, bool temporary = false)
        {
            FeedbackMessage.Text = message;
            FeedbackMessage.Style = (Style)(isError ? FindResource("ErrorMessage") : FindResource("SuccessMessage"));
            FeedbackMessage.Visibility = Visibility.Visible;

            if (temporary)
            {
                // Hide the message after a short delay
                await Task.Delay(3000); // Wait 3 seconds
                                        // Check if the message is still the same one we showed, avoids race conditions
                if (FeedbackMessage.Text == message)
                {
                    HideFeedback();
                }
            }
        }

        private void HideFeedback()
        {
            FeedbackMessage.Visibility = Visibility.Collapsed;
            FeedbackMessage.Text = "";
        }

    }
}