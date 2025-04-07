using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading; 
using MahApps.Metro.IconPacks;

using AlQaim.Models;
using AlQaim.Converters;

namespace AlQaim
{
    public partial class LT_RecordTestResult : Page, INotifyPropertyChanged
    {
        private readonly ObservableCollection<TestSample> _allTestSamples;
        private readonly CollectionViewSource _pendingTestsViewSource;
        private TestSample _selectedTest;
        private ObservableCollection<TestComponentResult> _currentComponentResults;
        private TestComponentResult _selectedComponentResult;
        private bool _isFullPanelView = false;
        private bool _isDataLoading = false;

        private DispatcherTimer _toastTimer;
        // -----------------------------------------

        public event PropertyChangedEventHandler PropertyChanged;

        // --- Constructor (Original + InitializeToastTimer call) ---
        public LT_RecordTestResult()
        {
            InitializeComponent();
            _isDataLoading = true;
            _allTestSamples = new ObservableCollection<TestSample>();
            _pendingTestsViewSource = new CollectionViewSource { Source = _allTestSamples };
            _currentComponentResults = new ObservableCollection<TestComponentResult>();
            PendingTestsDataGrid.ItemsSource = _pendingTestsViewSource.View;
            FullPanelItemsControl.ItemsSource = _currentComponentResults;

            InitializeToastTimer(); 

            _isDataLoading = false;
        }

        // --- Event Handlers (Original) ---

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // --- Original Page_Loaded Logic ---
            if (!_allTestSamples.Any())
            {
                _isDataLoading = true;
                LoadSampleData();
                ApplyDefaultSort();
                ApplyFiltersAndSorts();
                SearchTextBox_LostKeyboardFocus(SearchTextBox, null);
                ShowEmptyState();
                _isDataLoading = false;
            }
            else
            {
                ApplyFiltersAndSorts();
                if (PendingTestsDataGrid.SelectedItem == null) ShowEmptyState();
                else if (PendingTestsDataGrid.SelectedItem is TestSample current && current.Status != "Completed") ShowResultEntryPanel();
                else { ShowEmptyState("Selected test completed."); PendingTestsDataGrid.SelectedItem = null; }
                SearchTextBox_LostKeyboardFocus(SearchTextBox, null);
            }
            // --- End Original Page_Loaded Logic ---
        }

        private void SearchTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            // --- Original SearchTextBox_GotKeyboardFocus Logic ---
            if (sender is TextBox tb && tb.Foreground == FindResource("GrayTextColor") as Brush)
            {
                tb.Text = "";
                tb.Foreground = FindResource("TextColor") as Brush;
            }
            // --- End Original Logic ---
        }

        private void SearchTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            // --- Original SearchTextBox_LostKeyboardFocus Logic ---
            if (sender is TextBox tb && string.IsNullOrEmpty(tb.Text))
            {
                tb.Foreground = FindResource("GrayTextColor") as Brush;
                tb.Text = tb.Tag as string ?? ""; // Use Tag for watermark text
            }
            // --- End Original Logic ---
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // --- Original SearchTextBox_TextChanged Logic ---
            if (!IsLoaded || _isDataLoading) return;

            if (sender is TextBox tb && tb.Foreground != FindResource("GrayTextColor") as Brush)
            {
                ApplyFiltersAndSorts();
            }
            else if (sender is TextBox tb2 && string.IsNullOrEmpty(tb2.Text) && !tb2.IsKeyboardFocused)
            {
                ApplyFiltersAndSorts();
            }
            // --- End Original Logic ---
        }

        private void PendingTestsDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            // --- Original PendingTestsDataGrid_SelectedCellsChanged Logic ---
            if (_isDataLoading) return;

            TestSample previouslySelected = _selectedTest;
            bool selectionChangingFromValid = previouslySelected != null;

            // Save progressive result BEFORE changing selection if applicable
            if (selectionChangingFromValid && !_isFullPanelView)
            {
                // Ensure we only save if the context is still the previously selected item
                // AND check if selection actually changed before saving
                if (previouslySelected == _selectedTest && PendingTestsDataGrid.SelectedItem != previouslySelected)
                    SaveCurrentProgressiveResult(previouslySelected);
            }

            if (PendingTestsDataGrid.SelectedItem is TestSample selectedSample)
            {
                // If clicking the same item again, ensure panel is visible if not completed
                if (selectedSample == previouslySelected)
                {
                    if (ResultEntryPanel.Visibility == Visibility.Collapsed && selectedSample.Status != "Completed")
                    {
                        ShowResultEntryPanel();
                    }
                    return; // No further action needed if selection didn't actually change
                }

                // --- Selection has changed ---
                _selectedTest = selectedSample;

                if (selectedSample.Status != "Completed")
                {
                    GenerateComponentResultEntries(selectedSample);
                    // Load the first component by default in progressive view
                    LoadProgressiveResult(_currentComponentResults.FirstOrDefault());
                    ShowResultEntryPanel();
                    ExpandToggleButton.Visibility = Visibility.Visible;
                }
                else
                {
                    // Test is completed, show empty state with message
                    _currentComponentResults.Clear();
                    ClearProgressiveResultFields();
                    ShowEmptyState($"Test '{selectedSample.SampleId}' is already completed.");
                    ExpandToggleButton.Visibility = Visibility.Collapsed;
                    ExpandToggleButton.IsChecked = false; // Reset toggle state
                    _isFullPanelView = false; // Reset view mode
                }
            }
            else
            {
                // --- No item selected (or selection cleared) ---
                _selectedTest = null;
                _currentComponentResults.Clear();
                ClearProgressiveResultFields();
                ShowEmptyState(); // Show default empty state
                ExpandToggleButton.Visibility = Visibility.Collapsed;
                ExpandToggleButton.IsChecked = false; // Reset toggle state
                _isFullPanelView = false; // Reset view mode
            }
            // --- End Original Logic ---
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            // --- Original RecordButton_Click Logic ---
            if (sender is Button button && button.CommandParameter is TestSample sample)
            {
                if (sample.Status == "Completed") return;

                if (PendingTestsDataGrid.SelectedItem != sample)
                {
                    PendingTestsDataGrid.SelectedItem = sample;
                }
                else if (_selectedTest == sample)
                {
                    ShowResultEntryPanel();
                }
            }
            // --- End Original Logic ---
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // --- Original RefreshButton_Click Logic + Toast ---
            _isDataLoading = true;
            LoadSampleData();
            ApplyFiltersAndSorts();
            SearchTextBox.Text = ""; // Clear search
            SearchTextBox_LostKeyboardFocus(SearchTextBox, null);
            PendingTestsDataGrid.SelectedItem = null;
            _isDataLoading = false;
            ShowToast("Worklist refreshed.", PackIconMaterialKind.Refresh); // <-- ADDED Toast
            // --- End Original Logic ---
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // --- Original StatusFilter_SelectionChanged Logic ---
            if (!IsLoaded || _isDataLoading) return;
            ApplyFiltersAndSorts();
            // --- End Original Logic ---
        }

        private void SortOption_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // --- Original SortOption_SelectionChanged Logic ---
            if (!IsLoaded || _isDataLoading) return;
            ApplyFiltersAndSorts();
            // --- End Original Logic ---
        }

        private void TestComponentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // --- Original TestComponentComboBox_SelectionChanged Logic ---
            if (!IsLoaded || _isDataLoading) return;

            if (e.RemovedItems.Count > 0 && e.RemovedItems[0] is TestComponentResult previousComponent)
            {
                // Check if we are in progressive view before saving
                if (!_isFullPanelView && _selectedComponentResult == previousComponent)
                {
                    SaveCurrentProgressiveResult(_selectedTest);
                }
            }

            if (e.AddedItems.Count > 0 && e.AddedItems[0] is TestComponentResult newComponent)
            {
                LoadProgressiveResult(newComponent);
            }
            // Handle case where selection might be programmatically set or re-selected
            else if (TestComponentComboBox.SelectedItem is TestComponentResult currentItem)
            {
                LoadProgressiveResult(currentItem);
            }
            // --- End Original Logic ---
        }

        private void ResultValueTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // --- Original ResultValueTextBox_LostFocus Logic ---
            if (_selectedComponentResult != null && !_isFullPanelView)
            {
                SaveCurrentProgressiveResult(_selectedTest);
            }
            // --- End Original Logic ---
        }

        private void CommentsTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // --- Original CommentsTextBox_LostFocus Logic ---
            if (_selectedComponentResult != null && !_isFullPanelView)
            {
                SaveCurrentProgressiveResult(_selectedTest);
            }
            // --- End Original Logic ---
        }

        private void FlagComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // --- Original FlagComboBox_SelectionChanged Logic ---
            if (!IsLoaded || _isDataLoading || _selectedComponentResult == null || _isFullPanelView || FlagComboBox.SelectedItem == null) return;

            // Check if user interaction caused the change (simple check)
            if (!FlagComboBox.IsKeyboardFocusWithin && !(ProgressiveEntryPanel.IsKeyboardFocusWithin && !_isFullPanelView)) return;

            if (FlagComboBox.SelectedItem is ComboBoxItem selectedFlag)
            {
                _selectedComponentResult.Flag = selectedFlag.Content.ToString();
            }
            // --- End Original Logic ---
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // --- Original SubmitButton_Click Logic with MessageBox replaced ---
            if (_selectedTest == null || !_currentComponentResults.Any())
            {
                //MessageBox.Show("No test selected or components list is empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                ShowToast("No test selected or components list is empty.", PackIconMaterialKind.Cross); // <-- REPLACED
                return;
            }
            if (_selectedTest.Status == "Completed")
            {
                //MessageBox.Show("This test result has already been completed.", "Already Completed", MessageBoxButton.OK, MessageBoxImage.Information);
                ShowToast("This test result has already been completed.", PackIconMaterialKind.InformationOutline); // <-- REPLACED
                return;
            }

            if (_isFullPanelView)
            {
                UIElement focusedElement = Keyboard.FocusedElement as UIElement;
                if (focusedElement != null)
                {
                    BindingExpression binding = BindingOperations.GetBindingExpression(focusedElement, TextBox.TextProperty);
                    binding?.UpdateSource();
                    if (binding == null)
                    {
                        binding = BindingOperations.GetBindingExpression(focusedElement, ComboBox.SelectedValueProperty);
                        binding?.UpdateSource();
                    }
                }
            }
            else
            {
                SaveCurrentProgressiveResult(_selectedTest);
            }

            var incomplete = _currentComponentResults.FirstOrDefault(r => string.IsNullOrWhiteSpace(r.Value));
            if (incomplete != null)
            {
                //MessageBox.Show($"Please enter a result value for '{incomplete.ComponentName}'.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                ShowToast($"Please enter a result value for '{incomplete.ComponentName}'.", PackIconMaterialKind.Alert); // <-- REPLACED
                if (!_isFullPanelView && incomplete != _selectedComponentResult)
                {
                    TestComponentComboBox.SelectedItem = incomplete;
                }
                if (!_isFullPanelView)
                {
                    Dispatcher.BeginInvoke(new Action(() => ResultValueTextBox.Focus()), System.Windows.Threading.DispatcherPriority.Input);
                }
                return;
            }

            Console.WriteLine($"--- Submitting Results for Sample: {_selectedTest.SampleId} ---");
            foreach (var result in _currentComponentResults)
            {
                Console.WriteLine($"  Component: {result.ComponentName}, Value: {result.Value}, Flag: {result.Flag}, Comment: {result.Comment}");
            }
            Console.WriteLine("--------------------------------------------------");

            string savedSampleId = _selectedTest.SampleId;
            var sampleToUpdate = _allTestSamples.FirstOrDefault(s => s.SampleId == _selectedTest.SampleId);
            if (sampleToUpdate != null)
            {
                sampleToUpdate.Status = "Completed";
            }

            _pendingTestsViewSource.View?.Refresh();
            PendingTestsDataGrid.SelectedItem = null; // Will trigger empty state via event

            //MessageBox.Show($"Test results for {savedSampleId} submitted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            ShowToast($"Test results for {savedSampleId} submitted successfully.", PackIconMaterialKind.CheckCircle); // <-- REPLACED
            // --- End Original Logic ---
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // --- Original CancelButton_Click Logic ---
            PendingTestsDataGrid.SelectedItem = null; // Clears selection, triggers empty state
            // --- End Original Logic ---
        }

        private void ExpandToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            // --- Original ExpandToggleButton_Checked Logic ---
            SaveCurrentProgressiveResult(_selectedTest);
            _isFullPanelView = true;
            ProgressiveEntryPanel.Visibility = Visibility.Collapsed;
            FullPanelItemsControl.Visibility = Visibility.Visible;
            // --- End Original Logic ---
        }

        private void ExpandToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            // --- Original ExpandToggleButton_Unchecked Logic ---
            _isFullPanelView = false;
            ProgressiveEntryPanel.Visibility = Visibility.Visible;
            FullPanelItemsControl.Visibility = Visibility.Collapsed;

            if (TestComponentComboBox.SelectedItem is TestComponentResult currentProgComponent)
            {
                LoadProgressiveResult(currentProgComponent);
            }
            else if (_currentComponentResults.Any())
            {
                TestComponentComboBox.SelectedIndex = 0; // Load first item
            }
            else
            {
                ClearProgressiveResultFields();
            }
            // --- End Original Logic ---
        }

        // --- Helper Methods (Original - KEEP ALL AS IS) ---
        private void LoadSampleData()
        {
            _allTestSamples.Clear();
            _allTestSamples.Add(new TestSample { SampleId = "S001-23", PatientName = "John Smith", TestName = "Complete Blood Count (CBC)", CollectionDateTime = DateTime.Now.AddHours(-12), Status = "Pending" });
            _allTestSamples.Add(new TestSample { SampleId = "S002-23", PatientName = "Sarah Johnson", TestName = "Basic Metabolic Panel", CollectionDateTime = DateTime.Now.AddHours(-10), Status = "Pending" });
            _allTestSamples.Add(new TestSample { SampleId = "S003-23", PatientName = "Michael Brown", TestName = "Lipid Panel", CollectionDateTime = DateTime.Now.AddHours(-8), Status = "In Progress" });
            _allTestSamples.Add(new TestSample { SampleId = "S004-23", PatientName = "Emily Davis", TestName = "Hemoglobin A1C", CollectionDateTime = DateTime.Now.AddHours(-6), Status = "Pending" });
            _allTestSamples.Add(new TestSample { SampleId = "S005-23", PatientName = "Robert Wilson", TestName = "Liver Function Test", CollectionDateTime = DateTime.Now.AddHours(-4), Status = "Completed" });
            _allTestSamples.Add(new TestSample { SampleId = "S006-23", PatientName = "Jessica Martinez", TestName = "Thyroid Panel", CollectionDateTime = DateTime.Now.AddHours(-3), Status = "Pending" });
            _allTestSamples.Add(new TestSample { SampleId = "S007-23", PatientName = "David Anderson", TestName = "Urinalysis", CollectionDateTime = DateTime.Now.AddHours(-2), Status = "In Progress" });
            _pendingTestsViewSource.View?.Refresh();
        }

        private void ApplyFiltersAndSorts()
        {
            if (_isDataLoading || !IsLoaded || _pendingTestsViewSource?.View == null) return;

            ICollectionView view = _pendingTestsViewSource.View;
            string searchText = SearchTextBox?.Text ?? string.Empty;
            string watermarkText = SearchTextBox?.Tag as string ?? string.Empty;
            Brush grayBrush = FindResource("GrayTextColor") as Brush;

            if (searchText == watermarkText && SearchTextBox?.Foreground == grayBrush)
            {
                searchText = string.Empty;
            }
            searchText = searchText.ToLowerInvariant(); // Use InvariantCulture

            string selectedStatus = (StatusFilterComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All Statuses";

            view.Filter = item =>
            {
                if (!(item is TestSample sample)) return false;
                bool statusMatch = selectedStatus == "All Statuses" ||
                                   string.Equals(sample.Status, selectedStatus, StringComparison.OrdinalIgnoreCase); // Case-insensitive status match

                bool searchMatch = string.IsNullOrEmpty(searchText) ||
                                   (sample.SampleId?.ToLowerInvariant().Contains(searchText) ?? false) ||
                                   (sample.PatientName?.ToLowerInvariant().Contains(searchText) ?? false) ||
                                   (sample.TestName?.ToLowerInvariant().Contains(searchText) ?? false);
                return statusMatch && searchMatch;
            };

            string sortOption = (SortOptionComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Collection Date (Newest)";
            view.SortDescriptions.Clear();
            switch (sortOption)
            {
                case "Collection Date (Newest)": view.SortDescriptions.Add(new SortDescription(nameof(TestSample.CollectionDateTime), ListSortDirection.Descending)); break;
                case "Collection Date (Oldest)": view.SortDescriptions.Add(new SortDescription(nameof(TestSample.CollectionDateTime), ListSortDirection.Ascending)); break;
                case "Sample ID": view.SortDescriptions.Add(new SortDescription(nameof(TestSample.SampleId), ListSortDirection.Ascending)); break;
                case "Patient Name": view.SortDescriptions.Add(new SortDescription(nameof(TestSample.PatientName), ListSortDirection.Ascending)); break;
                default: view.SortDescriptions.Add(new SortDescription(nameof(TestSample.CollectionDateTime), ListSortDirection.Descending)); break;
            }
            view.Refresh(); // Refresh after changing filter/sort
        }


        private void ApplyDefaultSort()
        {
            if (_pendingTestsViewSource?.View == null) return;
            ICollectionView view = _pendingTestsViewSource.View;
            if (!view.SortDescriptions.Any()) // Add default only if no sorts exist
            {
                view.SortDescriptions.Add(new SortDescription(nameof(TestSample.CollectionDateTime), ListSortDirection.Descending));
            }
        }

        private void GenerateComponentResultEntries(TestSample sample)
        {
            _currentComponentResults.Clear();
            if (sample == null) return;
            List<string> componentNames = GetComponentNamesForPanel(sample.TestName);
            foreach (var compName in componentNames)
            {
                (string unit, string range) = GetUnitAndReferenceRange(compName);
                _currentComponentResults.Add(new TestComponentResult
                {
                    ComponentName = compName,
                    Unit = unit,
                    ReferenceRange = range,
                    Value = "",
                    Flag = "Normal",
                    Comment = ""
                });
            }
            TestComponentComboBox.ItemsSource = null; // Detach
            TestComponentComboBox.ItemsSource = _currentComponentResults; // Re-attach
            TestComponentComboBox.SelectedIndex = _currentComponentResults.Any() ? 0 : -1;
        }

        private void LoadProgressiveResult(TestComponentResult componentResult)
        {
            _selectedComponentResult = componentResult;
            bool handlersAttached = true; // Assume attached

            try
            {
                // Detach handlers
                TestComponentComboBox.SelectionChanged -= TestComponentComboBox_SelectionChanged;
                ResultValueTextBox.LostFocus -= ResultValueTextBox_LostFocus;
                CommentsTextBox.LostFocus -= CommentsTextBox_LostFocus;
                FlagComboBox.SelectionChanged -= FlagComboBox_SelectionChanged;
                handlersAttached = false; // Mark as detached

                if (componentResult != null)
                {
                    // Update UI fields
                    ResultValueTextBox.Text = componentResult.Value;
                    UnitOfMeasureText.Text = componentResult.Unit;
                    ReferenceRangeText.Text = componentResult.ReferenceRange;
                    CommentsTextBox.Text = componentResult.Comment;

                    // Set Flag ComboBox
                    bool flagFound = false;
                    foreach (ComboBoxItem item in FlagComboBox.Items)
                    {
                        if (item.Content?.ToString() == componentResult.Flag)
                        {
                            FlagComboBox.SelectedItem = item;
                            flagFound = true;
                            break;
                        }
                    }
                    if (!flagFound) FlagComboBox.SelectedIndex = 0; // Default to Normal

                    // Ensure Component ComboBox shows correct selection
                    if (TestComponentComboBox.SelectedItem != componentResult)
                    {
                        TestComponentComboBox.SelectedItem = componentResult;
                    }
                }
                else
                {
                    ClearProgressiveResultFields(); // Clear if no component
                }
            }
            finally
            {
                // Re-attach handlers if they were detached
                if (!handlersAttached)
                {
                    TestComponentComboBox.SelectionChanged += TestComponentComboBox_SelectionChanged;
                    ResultValueTextBox.LostFocus += ResultValueTextBox_LostFocus;
                    CommentsTextBox.LostFocus += CommentsTextBox_LostFocus;
                    FlagComboBox.SelectionChanged += FlagComboBox_SelectionChanged;
                }
            }
        }


        private void SaveCurrentProgressiveResult(TestSample contextSample)
        {
            // Ensure context is valid and component exists
            if (_selectedComponentResult != null && contextSample != null && contextSample == _selectedTest && _currentComponentResults.Contains(_selectedComponentResult))

            {
                _selectedComponentResult.Value = ResultValueTextBox.Text;
                _selectedComponentResult.Comment = CommentsTextBox.Text;
                _selectedComponentResult.Flag = (FlagComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Normal";
            }
        }

        private void ShowEmptyState(string message = null)
        {
            Action showEmpty = () => {
                ResultEntryPanel.Visibility = Visibility.Collapsed;
                EmptyStatePanel.Visibility = Visibility.Visible;
                ExpandToggleButton.Visibility = Visibility.Collapsed;
                ExpandToggleButton.IsChecked = false;
                _isFullPanelView = false;
                EmptyStateMessageTextBlock.Text = message ?? "Select a pending test from the worklist on the left to record its results.";
            };

            // Use fade-out animation if available and panel is currently visible
            if (ResultEntryPanel.Opacity > 0 && ResultEntryPanel.Visibility == Visibility.Visible && TryFindResource("FadeOutStoryboard") is Storyboard fadeOut)
            {
                // Clone storyboard to avoid issues with multiple simultaneous calls
                var localFadeOut = fadeOut.Clone();
                localFadeOut.Completed += (s, eArgs) => showEmpty();
                Storyboard.SetTarget(localFadeOut, ResultEntryPanel);
                localFadeOut.Begin();
            }
            else
            {
                ResultEntryPanel.Opacity = 0; // Ensure opacity is 0 if no animation
                showEmpty();
            }
        }

        private void ShowResultEntryPanel()
        {
            EmptyStatePanel.Visibility = Visibility.Collapsed;

            if (ResultEntryPanel.Visibility != Visibility.Visible)
            {
                // Reset state if panel was hidden
                ExpandToggleButton.IsChecked = false;
                _isFullPanelView = false;
                ProgressiveEntryPanel.Visibility = Visibility.Visible;
                FullPanelItemsControl.Visibility = Visibility.Collapsed;
                ResultEntryPanel.Opacity = 0; // Prepare for fade-in
            }

            ResultEntryPanel.Visibility = Visibility.Visible;

            // Use fade-in animation if available and panel is not fully opaque
            if (ResultEntryPanel.Opacity < 1 && TryFindResource("FadeInStoryboard") is Storyboard fadeIn)
            {
                Storyboard.SetTarget(fadeIn, ResultEntryPanel);
                fadeIn.Begin();
            }
            else ResultEntryPanel.Opacity = 1; // Ensure full opacity if no animation
                                               // Ensure toggle button is visible
            ExpandToggleButton.Visibility = Visibility.Visible;
        }

        private void ClearProgressiveResultFields()
        {
            _selectedComponentResult = null;
            TestComponentComboBox.SelectedIndex = -1;
            ResultValueTextBox.Text = string.Empty;
            CommentsTextBox.Text = string.Empty;
            UnitOfMeasureText.Text = string.Empty;
            ReferenceRangeText.Text = string.Empty;
            FlagComboBox.SelectedIndex = 0; // Default to Normal
        }

        private void ClearResultEntryForm()
        {
            ClearProgressiveResultFields();
            _currentComponentResults.Clear();
            _selectedTest = null;
        }

        private List<string> GetComponentNamesForPanel(string panelName)
        {
            List<string> components = new List<string>();
            switch (panelName?.Trim()) // Use null-conditional operator and Trim
            {
                case "Complete Blood Count (CBC)": components.AddRange(new[] { "WBC Count", "RBC Count", "Hemoglobin", "Hematocrit", "Platelet Count", "MCV", "MCH", "MCHC", "RDW" }); break;
                case "Basic Metabolic Panel": components.AddRange(new[] { "Sodium", "Potassium", "Chloride", "Carbon Dioxide", "BUN", "Creatinine", "Glucose", "Calcium" }); break;
                case "Lipid Panel": components.AddRange(new[] { "Total Cholesterol", "HDL Cholesterol", "LDL Cholesterol", "Triglycerides" }); break;
                case "Hemoglobin A1C": components.Add("HbA1c"); break;
                case "Liver Function Test": components.AddRange(new[] { "ALT", "AST", "ALP", "Total Bilirubin", "Direct Bilirubin", "Albumin", "Total Protein" }); break;
                case "Thyroid Panel": components.AddRange(new[] { "TSH", "Free T4", "Free T3" }); break;
                case "Urinalysis": components.AddRange(new[] { "Color", "Clarity", "pH", "Specific Gravity", "Glucose UA", "Ketones", "Protein UA", "Blood UA", "Leukocyte Esterase", "Nitrite" }); break;
                // Add panel name itself if it's not null/empty and not recognized
                default: if (!string.IsNullOrEmpty(panelName)) components.Add(panelName); break;
            }
            return components;
        }
        private (string Unit, string ReferenceRange) GetUnitAndReferenceRange(string testType)
        {
            string unit = ""; string range = "N/A"; // Initialize with defaults
            switch (testType?.Trim()) // Use null-conditional operator and Trim
            {
                // CBC
                case "WBC Count": unit = "x10^9/L"; range = "4.0 - 11.0"; break;
                case "RBC Count": unit = "x10^12/L"; range = "4.2 - 5.9"; break;
                case "Hemoglobin": unit = "g/dL"; range = "13.0 - 17.5"; break;
                case "Hematocrit": unit = "%"; range = "38 - 50"; break;
                case "Platelet Count": unit = "x10^9/L"; range = "150 - 450"; break;
                case "MCV": unit = "fL"; range = "80 - 100"; break;
                case "MCH": unit = "pg"; range = "27 - 33"; break;
                case "MCHC": unit = "g/dL"; range = "32 - 36"; break;
                case "RDW": unit = "%"; range = "11.5 - 14.5"; break;
                // BMP
                case "Sodium": unit = "mmol/L"; range = "135 - 145"; break;
                case "Potassium": unit = "mmol/L"; range = "3.5 - 5.1"; break;
                case "Chloride": unit = "mmol/L"; range = "98 - 107"; break;
                case "Carbon Dioxide": unit = "mmol/L"; range = "22 - 29"; break;
                case "BUN": unit = "mg/dL"; range = "7 - 20"; break;
                case "Creatinine": unit = "mg/dL"; range = "0.6 - 1.3"; break;
                case "Glucose": unit = "mg/dL"; range = "70 - 99"; break;
                case "Calcium": unit = "mg/dL"; range = "8.6 - 10.3"; break;
                // Lipid Panel
                case "Total Cholesterol": unit = "mg/dL"; range = "< 200"; break;
                case "HDL Cholesterol": unit = "mg/dL"; range = "> 40"; break;
                case "LDL Cholesterol": unit = "mg/dL"; range = "< 100"; break;
                case "Triglycerides": unit = "mg/dL"; range = "< 150"; break;
                // Other common tests
                case "HbA1c": unit = "%"; range = "4.0 - 5.6"; break;
                // LFT
                case "ALT": unit = "U/L"; range = "7 - 55"; break;
                case "AST": unit = "U/L"; range = "8 - 48"; break;
                case "ALP": unit = "U/L"; range = "40 - 129"; break;
                case "Total Bilirubin": unit = "mg/dL"; range = "0.2 - 1.2"; break;
                case "Direct Bilirubin": unit = "mg/dL"; range = "0.0 - 0.3"; break;
                case "Albumin": unit = "g/dL"; range = "3.5 - 5.0"; break;
                case "Total Protein": unit = "g/dL"; range = "6.0 - 8.3"; break;
                // Thyroid
                case "TSH": unit = "mIU/L"; range = "0.4 - 4.5"; break;
                case "Free T4": unit = "ng/dL"; range = "0.8 - 1.8"; break;
                case "Free T3": unit = "pg/mL"; range = "2.3 - 4.2"; break;
                // Urinalysis (Qualitative)
                case "Color": unit = ""; range = "Yellow"; break;
                case "Clarity": unit = ""; range = "Clear"; break;
                case "pH": unit = ""; range = "4.5 - 8.0"; break;
                case "Specific Gravity": unit = ""; range = "1.005 - 1.030"; break;
                case "Glucose UA": unit = ""; range = "Negative"; break;
                case "Ketones": unit = ""; range = "Negative"; break;
                case "Protein UA": unit = ""; range = "Negative"; break;
                case "Blood UA": unit = ""; range = "Negative"; break;
                case "Leukocyte Esterase": unit = ""; range = "Negative"; break;
                case "Nitrite": unit = ""; range = "Negative"; break;
                default: break; // Keep default N/A for unknown types
            }
            return (unit, range);
        }

        // --- INotifyPropertyChanged (Original) ---
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // --- Toast Notification Methods (ADDED) ---

        private void InitializeToastTimer()
        {
            _toastTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3) // 3-second interval like patient page
            };
            _toastTimer.Tick += (s, e) =>
            {
                _toastTimer.Stop();
                HideToast();
            };
        }

        private void ShowToast(string message, PackIconMaterialKind iconKind)
        {
            // Ensure toast elements exist
            if (toastNotification == null || toastMessage == null || toastIcon == null)
            {
                Console.WriteLine($"Toast Error: Control not found. Message: {message}"); // Log if controls missing
                return;
            }

            toastMessage.Text = message;
            toastIcon.Kind = iconKind;

            // --- Consistent Background Coloring (Optional but good) ---
            Brush backgroundBrush = (SolidColorBrush)FindResource("PrimaryAccentColor"); // Default
            try // Use Try/Catch for resource lookup as it might fail if not defined
            {
                if (iconKind.ToString().Contains("Alert") || iconKind.ToString().Contains("Warning") || iconKind == PackIconMaterialKind.Cancel || iconKind == PackIconMaterialKind.CloseCircle)
                {
                    if (TryFindResource("WarningColor") is SolidColorBrush warnBrush) backgroundBrush = warnBrush;
                    if ((iconKind == PackIconMaterialKind.Cancel || iconKind == PackIconMaterialKind.CloseCircle) && TryFindResource("DangerColor") is SolidColorBrush dangerBrush) backgroundBrush = dangerBrush;
                }
                else if (iconKind.ToString().Contains("Check"))
                {
                    if (TryFindResource("SuccessColor") is SolidColorBrush successBrush) backgroundBrush = successBrush;
                }
                else if (iconKind.ToString().Contains("Information"))
                {
                    if (TryFindResource("InfoColor") is SolidColorBrush infoBrush) backgroundBrush = infoBrush;
                }
            }
            catch (Exception ex) { Console.WriteLine($"Toast Color Error: {ex.Message}"); } // Log if resource lookup fails

            toastNotification.Background = backgroundBrush;
            // --- End Consistent Background Coloring ---

            toastNotification.Visibility = Visibility.Visible;

            // Simple Fade-in like Patient Page
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            // Ensure animation runs on the correct element
            Storyboard.SetTarget(fadeIn, toastNotification);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath(UIElement.OpacityProperty));
            var sb = new Storyboard();
            sb.Children.Add(fadeIn);
            sb.Begin();


            // Start timer for auto-hide
            if (_toastTimer == null) InitializeToastTimer(); // Defensive check
            _toastTimer.Start();
        }

        private void HideToast()
        {
            if (toastNotification == null || toastNotification.Visibility == Visibility.Collapsed) return;

            // Stop timer if hide is called manually or before completion
            if (_toastTimer != null && _toastTimer.IsEnabled) _toastTimer.Stop();

            // Simple Fade-out like Patient Page
            var fadeOut = new DoubleAnimation
            {
                From = toastNotification.Opacity, // Fade from current opacity
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            // Ensure animation runs on the correct element
            Storyboard.SetTarget(fadeOut, toastNotification);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath(UIElement.OpacityProperty));

            EventHandler fadeOutCompletedHandler = null;
            fadeOutCompletedHandler = (s, e) =>
            {
                // Check if the clock associated with the animation has completed
                var clock = s as AnimationClock;
                if (clock != null && clock.CurrentState == ClockState.Filling)
                {
                    toastNotification.Visibility = Visibility.Collapsed;
                    // Clean up animation resources
                    toastNotification.BeginAnimation(UIElement.OpacityProperty, null);
                    // Detach the handler
                    clock.Completed -= fadeOutCompletedHandler;
                }
                else if (clock == null) // If sender is not Clock, just hide (fallback)
                {
                    toastNotification.Visibility = Visibility.Collapsed;
                    toastNotification.BeginAnimation(UIElement.OpacityProperty, null);
                }
            };
            fadeOut.Completed += fadeOutCompletedHandler;

            var sb = new Storyboard();
            sb.Children.Add(fadeOut);
            sb.Begin(toastNotification, HandoffBehavior.SnapshotAndReplace); // Use HandoffBehavior
        }


        // Event handler for the toast's close button (needs to be added)
        private void BtnCloseToast_Click(object sender, RoutedEventArgs e)
        {
            HideToast(); 
        }

    } 

   
} 