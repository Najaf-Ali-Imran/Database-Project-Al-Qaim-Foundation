using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using LiveCharts;
using LiveCharts.Wpf;
using AlQaim.Models;
using LiveCharts.Defaults;
using System.Windows.Controls;
using System.Globalization;
using System.Collections.ObjectModel; 
using System.Linq; 

namespace AlQaim
{

    public partial class LT_Dashboard : Page, INotifyPropertyChanged
    {
        private DispatcherTimer _timer;

        // --- Chart Properties ---
        private SeriesCollection _testResultsSeries;
        public SeriesCollection TestResultsSeries { get => _testResultsSeries; set { _testResultsSeries = value; OnPropertyChanged(); } }
        private SeriesCollection _testTypesSeries;
        public SeriesCollection TestTypesSeries { get => _testTypesSeries; set { _testTypesSeries = value; OnPropertyChanged(); } }
        private SeriesCollection _sampleSourcesSeries;
        public SeriesCollection SampleSourcesSeries { get => _sampleSourcesSeries; set { _sampleSourcesSeries = value; OnPropertyChanged(); } }
        private SeriesCollection _pendingSamplesSeries;
        public SeriesCollection PendingSamplesSeries { get => _pendingSamplesSeries; set { _pendingSamplesSeries = value; OnPropertyChanged(); } }
        private SeriesCollection _pendingResultsSeries;
        public SeriesCollection PendingResultsSeries { get => _pendingResultsSeries; set { _pendingResultsSeries = value; OnPropertyChanged(); } }
        private SeriesCollection _completedTestsSeries;
        public SeriesCollection CompletedTestsSeries { get => _completedTestsSeries; set { _completedTestsSeries = value; OnPropertyChanged(); } }
        private SeriesCollection _newRequestsSeries;
        public SeriesCollection NewRequestsSeries { get => _newRequestsSeries; set { _newRequestsSeries = value; OnPropertyChanged(); } }

        // --- Axis Labels ---
        private string[] _testDates;
        public string[] TestDates { get => _testDates; set { _testDates = value; OnPropertyChanged(); } }
        private string[] _sourceLabels;
        public string[] SourceLabels { get => _sourceLabels; set { _sourceLabels = value; OnPropertyChanged(); } }

        // --- Property for Admin Tasks List ---
        private ObservableCollection<AdminTask> _adminTasks; // Using ObservableCollection
        public ObservableCollection<AdminTask> AdminTasks
        {
            get => _adminTasks;
            set { _adminTasks = value; OnPropertyChanged(); }
        }

        public LT_Dashboard()
        {
            InitializeComponent();
            DataContext = this; // Crucial for bindings

            // Initialize Collections
            AdminTasks = new ObservableCollection<AdminTask>();
            PendingSamplesSeries = new SeriesCollection();
            PendingResultsSeries = new SeriesCollection();
            CompletedTestsSeries = new SeriesCollection();
            NewRequestsSeries = new SeriesCollection();
            TestResultsSeries = new SeriesCollection();
            TestTypesSeries = new SeriesCollection();
            SampleSourcesSeries = new SeriesCollection();


            // Setup Timer
            _timer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // Handle Page Unloaded event to stop timer
            this.Unloaded += Page_Unloaded;

            UpdateDateTime(); // Initial call
            LoadDashboardData();
            InitializeAnimations();

            // Wire up task button clicks (Example - Consider Command Binding for MVVM)
            // Ensure the Button in XAML DataTemplate has: Click="TaskActionButton_Click"
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateDateTime();
        }

        private void UpdateDateTime()
        {
            DateTimeText.Text = DateTime.Now.ToString("f", CultureInfo.CurrentCulture);
        }

        private void LoadDashboardData()
        {
            // * TODO: Replace all placeholder data with actual DB/API calls *


            // === Metrics Card Data & Sparklines ===
            int pendingSamples = 24;
            int pendingResults = 38;
            int completedTests = 156;
            int newRequests = 42;
            // TODO: Fetch actual counts and sparkline data...

            PendingSamplesSeries.Clear();
            PendingSamplesSeries.Add(new LineSeries { Title = "PendingSamplesSpark", Values = new ChartValues<double> { 18, 22, 16, 20, 24, 22, 24 }, PointGeometry = null, LineSmoothness = 0.8, Fill = new SolidColorBrush(Color.FromArgb(60, 46, 134, 222)), StrokeThickness = 1.5 });
            PendingResultsSeries.Clear();
            PendingResultsSeries.Add(new LineSeries { Title = "PendingResultsSpark", Values = new ChartValues<double> { 28, 32, 36, 34, 38, 32, 38 }, PointGeometry = null, LineSmoothness = 0.8, Fill = new SolidColorBrush(Color.FromArgb(60, 247, 159, 31)), StrokeThickness = 1.5 });
            CompletedTestsSeries.Clear();
            CompletedTestsSeries.Add(new LineSeries { Title = "CompletedTestsSpark", Values = new ChartValues<double> { 120, 135, 148, 136, 145, 150, 156 }, PointGeometry = null, LineSmoothness = 0.8, Fill = new SolidColorBrush(Color.FromArgb(60, 38, 222, 129)), StrokeThickness = 1.5 });
            NewRequestsSeries.Clear();
            NewRequestsSeries.Add(new LineSeries { Title = "NewRequestsSpark", Values = new ChartValues<double> { 36, 42, 38, 45, 40, 38, 42 }, PointGeometry = null, LineSmoothness = 0.8, Fill = new SolidColorBrush(Color.FromArgb(60, 252, 92, 101)), StrokeThickness = 1.5 });

         
            // TODO: Fetch actual chart data based on selected TimeRangeComboBox value
            TestDates = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" }; // Update based on actual data range
            TestResultsSeries.Clear();
            TestResultsSeries.Add(new LineSeries { Title = "Completed", Values = new ChartValues<double> { 18, 24, 26, 22, 28, 16, 22 }, PointGeometry = DefaultGeometries.Circle, PointGeometrySize = 8, LineSmoothness = 0.6, StrokeThickness = 2 });
            TestResultsSeries.Add(new LineSeries { Title = "Pending", Values = new ChartValues<double> { 12, 15, 14, 10, 9, 8, 10 }, PointGeometry = DefaultGeometries.Circle, PointGeometrySize = 8, LineSmoothness = 0.6, StrokeThickness = 2 });

            TestTypesSeries.Clear();
            TestTypesSeries.Add(new PieSeries { Title = "Blood Tests", Values = new ChartValues<ObservableValue> { new ObservableValue(42) }, DataLabels = true, LabelPoint = p => $"{p.Y}" });
            TestTypesSeries.Add(new PieSeries { Title = "Urine Analysis", Values = new ChartValues<ObservableValue> { new ObservableValue(28) }, DataLabels = true, LabelPoint = p => $"{p.Y}" });
            TestTypesSeries.Add(new PieSeries { Title = "Tissue Samples", Values = new ChartValues<ObservableValue> { new ObservableValue(16) }, DataLabels = true, LabelPoint = p => $"{p.Y}" });
            TestTypesSeries.Add(new PieSeries { Title = "Other", Values = new ChartValues<ObservableValue> { new ObservableValue(14) }, DataLabels = true, LabelPoint = p => $"{p.Y}" });

            SourceLabels = new[] { "ER Dept", "Clinic A", "Clinic B", "Cardiology", "Oncology" }; // Update based on actual data
            SampleSourcesSeries.Clear();
            SampleSourcesSeries.Add(new ColumnSeries { Title = "Samples Received", Values = new ChartValues<double> { 24, 32, 18, 15, 12 }, MaxColumnWidth = 40 });


            // === Notifications Data ===
            // TODO: Fetch notification data dynamically and populate NotificationsPanel if needed.


            // === Load Admin Tasks Data ===
            AdminTasks.Clear(); // Clear previous items if reloading
            // TODO: Replace this sample data with data loaded from your MySQL database assigned to this LT
            AdminTasks.Add(new AdminTask { TaskId = 101, Title = "Calibrate Centrifuge #3", DueDate = DateTime.Now.AddDays(1), Priority = "Normal", Status = "Pending" });
            AdminTasks.Add(new AdminTask { TaskId = 102, Title = "Restock Pipette Tips (Box A)", DueDate = DateTime.Now.AddDays(2), Priority = "Normal", Status = "Pending" });
            AdminTasks.Add(new AdminTask { TaskId = 103, Title = "Review QC Logs for Hematology Analyzer", DueDate = DateTime.Now.AddHours(4), Priority = "Urgent", Status = "Pending" });
            AdminTasks.Add(new AdminTask { TaskId = 104, Title = "Perform Daily Maintenance on Autoanalyzer", DueDate = DateTime.Now.AddDays(-1), Priority = "Normal", Status = "Completed" });
            AdminTasks.Add(new AdminTask { TaskId = 105, Title = "Inventory Check - Reagents Shelf 2", DueDate = DateTime.Now.AddDays(5), Priority = "Normal", Status = "Pending" });


            // --- Update UI Elements (Count-up animations) ---
            ApplyCountUpAnimation(PendingSamplesCount, pendingSamples);
            ApplyCountUpAnimation(PendingResultsCount, pendingResults);
            ApplyCountUpAnimation(CompletedTestsCount, completedTests);
            ApplyCountUpAnimation(NewRequestsCount, newRequests);

            // Notify UI about changes (especially if replacing collection instances, though ObservableCollection handles adds/removes)
            OnPropertyChanged(nameof(PendingSamplesSeries));
            OnPropertyChanged(nameof(PendingResultsSeries));
            OnPropertyChanged(nameof(CompletedTestsSeries));
            OnPropertyChanged(nameof(NewRequestsSeries));
            OnPropertyChanged(nameof(TestResultsSeries));
            OnPropertyChanged(nameof(TestDates));
            OnPropertyChanged(nameof(TestTypesSeries));
            OnPropertyChanged(nameof(SampleSourcesSeries));
            OnPropertyChanged(nameof(SourceLabels));
            OnPropertyChanged(nameof(AdminTasks));
        }

        private void InitializeAnimations()
        {
            // Apply fade-in animations
            ApplyFadeInAnimation(PendingSamplesCard, 0.0);
            ApplyFadeInAnimation(PendingResultsCard, 0.1);
            ApplyFadeInAnimation(CompletedTestsCard, 0.2);
            ApplyFadeInAnimation(NewRequestsCard, 0.3);
            // ApplyFadeInAnimation(MainChartCard, 0.4); // Add x:Name to main chart Border if needed
            ApplyFadeInAnimation(AdminTasksCard, 0.5); // Ensure x:Name="AdminTasksCard" exists on the Border
        }

        // --- Animation Helper Methods ---
        private void ApplyFadeInAnimation(UIElement element, double delayInSeconds = 0)
        {
            if (element == null) return;
            element.Opacity = 0;

            var fadeInAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.5),
                BeginTime = TimeSpan.FromSeconds(delayInSeconds)
            };
            fadeInAnimation.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };

            Storyboard.SetTarget(fadeInAnimation, element);
            Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(UIElement.OpacityProperty));

            var storyboard = new Storyboard();
            storyboard.Children.Add(fadeInAnimation);
            storyboard.Begin();
        }

        // In Admin_Dashboard.xaml.cs

        private void ApplyCountUpAnimation(TextBlock textBlock, double targetValue, double delayInSeconds = 0.4)
        {
            // Initial check: If textBlock is null when the method is called, don't proceed.
            if (textBlock == null)
            {
                System.Diagnostics.Debug.WriteLine($"[Admin_Dashboard] WARN: ApplyCountUpAnimation called with null TextBlock.");
                return;
            }

            var startValue = 0d;
            textBlock.Text = "0"; // Initialize display

            // Create the main timer for the animation ticks
            var timer = new DispatcherTimer(TimeSpan.FromMilliseconds(20), DispatcherPriority.Render, (s, e) =>
            {
                // --- Safely get the timer and check its state ---
                var currentTimer = s as DispatcherTimer;
                if (currentTimer == null) return; // Should not happen, but safety first

                // --- Check if the TextBlock is still loaded and accessible ---
                // This is the most crucial check for this specific exception.
                if (!textBlock.IsLoaded)
                {
                    currentTimer.Stop(); // Stop the timer if the target TextBlock is no longer loaded
                    currentTimer.Tag = null; // Clear the tag
                    System.Diagnostics.Debug.WriteLine($"[Admin_Dashboard] INFO: Stopping count-up animation for unloaded TextBlock (Name: {textBlock.Name}).");
                    return;
                }

                // --- Check if Tag is valid (should be TimeSpan) ---
                if (!(currentTimer.Tag is TimeSpan elapsed))
                {
                    // Tag is null or not a TimeSpan, indicates an issue (e.g., timer stopped prematurely)
                    currentTimer.Stop();
                    System.Diagnostics.Debug.WriteLine($"[Admin_Dashboard] WARN: Invalid or null Tag found in ApplyCountUpAnimation timer tick. Stopping timer for TextBlock (Name: {textBlock.Name}).");
                    return;
                }

                TimeSpan totalDuration = TimeSpan.FromSeconds(1.2);

                if (totalDuration.TotalSeconds <= 0)
                {
                    textBlock.Text = Math.Floor(targetValue).ToString("N0");
                    currentTimer.Stop();
                    currentTimer.Tag = null;
                    return;
                }

                double rawProgress = Math.Min(1.0, elapsed.TotalSeconds / totalDuration.TotalSeconds);
                double easedProgress = 1 - Math.Pow(1 - rawProgress, 3); // Cubic Ease Out
                var currentFrameValue = startValue + (targetValue - startValue) * easedProgress;

                // --- Update the TextBlock (Now known to be loaded) ---
                textBlock.Text = Math.Floor(currentFrameValue).ToString("N0"); // Line 285 approx - Now safer

                // --- Check completion and update Tag ---
                if (rawProgress >= 1.0)
                {
                    textBlock.Text = Math.Floor(targetValue).ToString("N0"); // Ensure final value
                    currentTimer.Stop();
                    currentTimer.Tag = null; // Clean up
                }
                else
                {
                    // Update the Tag for the next tick
                    currentTimer.Tag = elapsed + TimeSpan.FromMilliseconds(20);
                }
            }, Application.Current.Dispatcher); // Ensure it runs on the UI thread

            // Create and start the delay timer
            var delayTimer = new DispatcherTimer(DispatcherPriority.Render) { Interval = TimeSpan.FromSeconds(delayInSeconds) };
            delayTimer.Tick += (sender, eventArgs) =>
            {
                // --- Start the main animation timer after the delay ---
                // Check if 'timer' is still valid before accessing it
                if (timer != null)
                {
                    timer.Tag = TimeSpan.Zero; // Initialize elapsed time for count-up timer
                    timer.Start();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[Admin_Dashboard] ERROR: Main 'timer' was null in delayTimer Tick for TextBlock (Name: {textBlock?.Name}).");
                }

                // Stop the delay timer itself
                (sender as DispatcherTimer)?.Stop();
            };

            // --- Add handler to stop the main timer if the TextBlock unloads ---
            // This provides an extra layer of safety to stop the timer explicitly
            // when the control unloads, rather than just relying on the IsLoaded check inside the tick.
            RoutedEventHandler unloadHandler = null;
            unloadHandler = (sender, args) => {
                timer?.Stop(); // Stop the main timer
                timer = null; // Release reference
                delayTimer?.Stop(); // Stop the delay timer if it's somehow still running
                delayTimer = null;
                textBlock.Unloaded -= unloadHandler; // Unsubscribe self
                System.Diagnostics.Debug.WriteLine($"[Admin_Dashboard] INFO: Cleaned up count-up timer on Unloaded event for TextBlock (Name: {textBlock.Name}).");
            };
            textBlock.Unloaded += unloadHandler;


            delayTimer.Start(); // Start the delay
        }

        // --- Event Handler for Task Action Buttons ---
        private void TaskActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is AdminTask task) // Use the shared AdminTask class
            {
                if (task.Status != "Completed")
                {
                    // TODO: Implement logic to mark the task as completed in the database
                    // Example: bool success = await MarkTaskCompleteInDbAsync(task.TaskId);

                    // If database update was successful:
                    // if (success) {
                    task.Status = "Completed"; // Update the object. UI updates due to ObservableCollection and INotifyPropertyChanged.
                    button.IsEnabled = false; // Optionally disable the button
                    button.Content = "Done"; // Change button text
                    MessageBox.Show($"Task '{task.Title}' marked as completed (Simulated DB Update)."); // Use Title
                                                                                                        // } else { // Handle DB update failure }
                }
            }
        }

        // --- Other Event Handlers ---
        private void TimeRangeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Add logic to reload main chart data (TestResultsSeries, TestTypesSeries, SampleSourcesSeries)
            // based on the selected time range in TimeRangeComboBox.
            // Example:
            // if (TimeRangeComboBox.SelectedItem is ComboBoxItem selectedItem) {
            //    string range = selectedItem.Content.ToString();
            //    LoadMainChartDataForRange(range); // Call a method to fetch and update chart series
            // }
        }

        // --- INotifyPropertyChanged implementation ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // --- Page Unloaded ---
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // Stop the timer when the page is unloaded
            _timer?.Stop();
            this.Unloaded -= Page_Unloaded; // Unsubscribe self
                                            // Stop any other running timers
        }
    }

   
}