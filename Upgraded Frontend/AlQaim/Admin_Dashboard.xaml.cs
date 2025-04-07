using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel; 
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using LiveCharts;
using AlQaim.Models;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using FontAwesome.Sharp; 

namespace AlQaim
{
 

    public partial class Admin_Dashboard : Page, INotifyPropertyChanged
    {
        private DispatcherTimer _timer;

        // --- Chart Data ---
        public SeriesCollection DailyTestsSeries { get; set; }
        public string[] DailyTestDates { get; set; }
        public SeriesCollection DiagnosesSeries { get; set; }
        public ObservableCollection<Tuple<string, int>> TopIssuesData { get; set; }

        // --- Task Management ---
        public ObservableCollection<AdminTask> AdminTasks { get; set; }

        // --- Activity Feed ---
        public ObservableCollection<ActivityItem> ActivityFeed { get; set; }

        public Admin_Dashboard()
        {
            InitializeComponent();
            DataContext = this; // Set DataContext for binding

            // Initialize Collections
            DailyTestsSeries = new SeriesCollection();
            DiagnosesSeries = new SeriesCollection();
            TopIssuesData = new ObservableCollection<Tuple<string, int>>();
            AdminTasks = new ObservableCollection<AdminTask>();
            ActivityFeed = new ObservableCollection<ActivityItem>();

            // Setup Timer for Clock
            _timer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // Handle Page Unloaded event to stop timer
            this.Unloaded += Page_Unloaded;

            UpdateDateTime(); // Initial call
            LoadDashboardData(); // Call the single, correct method
            InitializeAnimations();

            // TODO: Load saved notes from storage/DB
            // AdminNotesTextBox.Text = LoadNotesFromSomewhere();
        }

        // --- Single Correct Definition of LoadDashboardData ---
        private void LoadDashboardData()
        {
          
            // === 1. Lab/Test Metrics Data ===
            int pendingSamples = 15;
            int pendingResults = 8;
            int completedTests = 92;
            int newRequests = 5;
            // Apply animations (ensure TextBlock names match XAML)
            ApplyCountUpAnimation(PendingSamplesCountText, pendingSamples, 0.0); // Stagger delays
            ApplyCountUpAnimation(PendingResultsCountText, pendingResults, 0.1);
            ApplyCountUpAnimation(CompletedTestsCountText, completedTests, 0.2);
            ApplyCountUpAnimation(NewRequestsCountText, newRequests, 0.3);

            // === 2. Patient Metrics Data ===
            // TODO: Fetch actual patient counts from Database
            int totalPatients = 1256;
            int newPatientsToday = 7;
            // Apply animations (ensure TextBlock names match XAML)
            ApplyCountUpAnimation(TotalPatientsCountText, totalPatients, 0.4); // Continue stagger
            ApplyCountUpAnimation(NewPatientsCountText, newPatientsToday, 0.5);

            // === 3. Appointment Metrics Data ===
            // TODO: Fetch actual appointment counts for today/relevant period from Database
            int scheduledAppts = 45;
            int completedAppts = 32;
            int noShowAppts = 3;
            int cancelledAppts = 5;
            // Apply animations (ensure TextBlock names match XAML)
            ApplyCountUpAnimation(ScheduledApptsCountText, scheduledAppts, 0.6);
            ApplyCountUpAnimation(CompletedApptsCountText, completedAppts, 0.7);
            ApplyCountUpAnimation(NoShowApptsCountText, noShowAppts, 0.8);
            ApplyCountUpAnimation(CancelledApptsCountText, cancelledAppts, 0.9);

            // === 4. Patient Details & Trends ===
            TopIssuesData.Clear();
            // TODO: Fetch real data for Top Issues
            TopIssuesData.Add(Tuple.Create("Delayed Sample Collection", 5));
            TopIssuesData.Add(Tuple.Create("Equipment Downtime", 3));
            TopIssuesData.Add(Tuple.Create("Incorrect Test Request", 2));
            OnPropertyChanged(nameof(TopIssuesData));

            DiagnosesSeries.Clear();
            // TODO: Fetch real data for Diagnoses
            DiagnosesSeries.Add(new PieSeries { Title = "Hypertension", Values = new ChartValues<ObservableValue> { new ObservableValue(35) }, DataLabels = true, LabelPoint = p => $"{p.Y}", Fill = (SolidColorBrush)FindResource("AccentColor") });
            DiagnosesSeries.Add(new PieSeries { Title = "Diabetes", Values = new ChartValues<ObservableValue> { new ObservableValue(28) }, DataLabels = true, LabelPoint = p => $"{p.Y}", Fill = (SolidColorBrush)FindResource("SuccessColor") });
            DiagnosesSeries.Add(new PieSeries { Title = "Infection", Values = new ChartValues<ObservableValue> { new ObservableValue(18) }, DataLabels = true, LabelPoint = p => $"{p.Y}", Fill = (SolidColorBrush)FindResource("WarningColor") });
            DiagnosesSeries.Add(new PieSeries { Title = "Anemia", Values = new ChartValues<ObservableValue> { new ObservableValue(12) }, DataLabels = true, LabelPoint = p => $"{p.Y}", Fill = (SolidColorBrush)FindResource("InfoColor") });
            DiagnosesSeries.Add(new PieSeries { Title = "Other", Values = new ChartValues<ObservableValue> { new ObservableValue(7) }, DataLabels = true, LabelPoint = p => $"{p.Y}", Fill = (SolidColorBrush)FindResource("TextSecondary") });
            OnPropertyChanged(nameof(DiagnosesSeries));


            // === 5. Main Chart Data (Daily Tests) ===
            LoadChartData("Last 7 Days"); // Initial load


            // === 6. Task Management Data ===
            AdminTasks.Clear();
            // TODO: Fetch real task data
            AdminTasks.Add(new AdminTask { TaskId = 201, Title = "Approve Overtime Requests", DueDate = DateTime.Today.AddHours(17), Priority = "High", Status = "Pending", AssigneeName = "Admin" });
            AdminTasks.Add(new AdminTask { TaskId = 202, Title = "Review Monthly Budget Report", DueDate = DateTime.Today.AddDays(2), Priority = "Medium", Status = "Pending", AssigneeName = "Admin" });
            AdminTasks.Add(new AdminTask { TaskId = 203, Title = "Schedule Team Meeting", DueDate = DateTime.Today.AddDays(1), Priority = "Medium", Status = "In Progress", AssigneeName = "Jane D." });
            OnPropertyChanged(nameof(AdminTasks));


            // === 7. Activity Feed Data ===
            ActivityFeed.Clear();
            var now = DateTime.Now;
            // TODO: Fetch real activity data
            ActivityFeed.Add(new ActivityItem { Icon = IconChar.CheckCircle, IconForeground = (SolidColorBrush)FindResource("SuccessColor"), IconBackground = (SolidColorBrush)FindResource("SuccessColor"), Description = "Test Results Submitted", Details = "Batch #5890 results approved.", Timestamp = now.AddMinutes(-5) });
            ActivityFeed.Add(new ActivityItem { Icon = IconChar.UserPlus, IconForeground = (SolidColorBrush)FindResource("AccentColor"), IconBackground = (SolidColorBrush)FindResource("AccentColor"), Description = "New Technician Account Created", Details = "User 'LauraB' added.", Timestamp = now.AddMinutes(-25) });
            ActivityFeed.Add(new ActivityItem { Icon = IconChar.ExclamationTriangle, IconForeground = (SolidColorBrush)FindResource("WarningColor"), IconBackground = (SolidColorBrush)FindResource("WarningColor"), Description = "Inventory Alert: Low Stock", Details = "Reagent Kit 'XYZ' below threshold.", Timestamp = now.AddHours(-1) });
            OnPropertyChanged(nameof(ActivityFeed));
        }


        // --- Timer and Date/Time Update ---
        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateDateTime();
        }

        private void UpdateDateTime()
        {
            DateTimeText.Text = DateTime.Now.ToString("f", CultureInfo.CurrentCulture);
        }


        // --- Chart Loading Logic ---
        private void LoadChartData(string timeRange)
        {
            if (DailyTestsSeries == null)
            {
                System.Diagnostics.Debug.WriteLine("[Admin_Dashboard] ERROR: DailyTestsSeries was null in LoadChartData. Re-initializing.");
                DailyTestsSeries = new SeriesCollection();
            }
            DailyTestsSeries.Clear();

            var accentBrush = FindResource("AccentColor") as SolidColorBrush;
            if (accentBrush == null)
            {
                System.Diagnostics.Debug.WriteLine("[Admin_Dashboard] ERROR: Resource 'AccentColor' not found. Using fallback.");
                accentBrush = new SolidColorBrush(Colors.DodgerBlue);
            }

            try
            {
                // TODO: Replace sample data with data fetched based on timeRange
                if (timeRange == "Last 7 Days")
                {
                    DailyTestDates = Enumerable.Range(0, 7).Select(i => DateTime.Today.AddDays(-6 + i).ToString("ddd")).ToArray();
                    DailyTestsSeries.Add(new LineSeries
                    {
                        Title = "Tests Completed",
                        Values = new ChartValues<double> { 55, 62, 78, 70, 85, 92, 88 },
                        PointGeometry = DefaultGeometries.Circle,
                        PointGeometrySize = 8,
                        LineSmoothness = 0.6,
                        StrokeThickness = 2,
                        Fill = new SolidColorBrush(accentBrush.Color) { Opacity = 0.1 }
                    });
                }
                else if (timeRange == "Last 30 Days")
                {
                    DailyTestDates = Enumerable.Range(0, 4).Select(i => DateTime.Today.AddDays(-29 + i * 7).ToString("MMM d")).ToArray();
                    DailyTestsSeries.Add(new ColumnSeries
                    {
                        Title = "Tests Completed (Weekly Avg)",
                        Values = new ChartValues<double> { 450, 480, 510, 495 },
                        MaxColumnWidth = 30,
                        Fill = accentBrush
                    });
                }
                else
                {
                    DailyTestDates = new string[0];
                    System.Diagnostics.Debug.WriteLine($"[Admin_Dashboard] WARN: Unknown timeRange '{timeRange}' in LoadChartData.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Admin_Dashboard] ERROR creating chart series: {ex.Message}");
                MessageBox.Show($"An error occurred while loading chart data:\n{ex.Message}", "Chart Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            OnPropertyChanged(nameof(DailyTestsSeries));
            OnPropertyChanged(nameof(DailyTestDates));
        }


        // --- Animation Initialization  ---
        private void InitializeAnimations()
        {
            // Fade in Animation use as required 
            // Row 1: Lab Metrics
            ApplyFadeInAnimation(PendingSamplesCard, 0.0);
            ApplyFadeInAnimation(PendingResultsCard, 0.1);
            ApplyFadeInAnimation(CompletedTestsCard, 0.2);
            ApplyFadeInAnimation(NewRequestsCard, 0.3);

            // Row 2: Patient Metrics
            ApplyFadeInAnimation(TotalPatientsCard, 0.4);
            ApplyFadeInAnimation(NewPatientsCard, 0.5);
            // Add fade-in for placeholder patient cards if they exist and have x:Name

            // Row 3: Appointment Metrics
            ApplyFadeInAnimation(ScheduledApptsCard, 0.6);
            ApplyFadeInAnimation(CompletedApptsCard, 0.7);
            ApplyFadeInAnimation(NoShowApptsCard, 0.8);
            ApplyFadeInAnimation(CancelledApptsCard, 0.9);

            // Lower Sections
            ApplyFadeInAnimation(TopIssuesCard, 1.0);     // Adjusted delay
            ApplyFadeInAnimation(DiagnosesCard, 1.05);    // Adjusted delay
            ApplyFadeInAnimation(MainChartCard, 1.1);     // Adjusted delay
            ApplyFadeInAnimation(TasksCard, 1.2);         // Adjusted delay
            ApplyFadeInAnimation(ActivityCard, 1.25);     // Adjusted delay
        }

        // --- Animation Helper Methods ---
        private void ApplyFadeInAnimation(UIElement element, double delayInSeconds = 0)
        {
            if (element == null) return;
            element.Opacity = 0;
            var fadeInAnimation = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromSeconds(0.5), BeginTime = TimeSpan.FromSeconds(delayInSeconds) };
            fadeInAnimation.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            Storyboard.SetTarget(fadeInAnimation, element);
            Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(UIElement.OpacityProperty));
            var storyboard = new Storyboard();
            storyboard.Children.Add(fadeInAnimation);
            storyboard.Begin();
        }

        private void ApplyCountUpAnimation(TextBlock textBlock, double targetValue, double delayInSeconds = 0.4)
        {
            if (textBlock == null) { System.Diagnostics.Debug.WriteLine($"[Admin_Dashboard] WARN: ApplyCountUpAnimation called with null TextBlock."); return; }
            var startValue = 0d;
            textBlock.Text = "0";
            var timer = new DispatcherTimer(TimeSpan.FromMilliseconds(20), DispatcherPriority.Render, (s, e) =>
            {
                var currentTimer = s as DispatcherTimer;
                if (currentTimer == null) return;
                if (!textBlock.IsLoaded) { currentTimer.Stop(); currentTimer.Tag = null; System.Diagnostics.Debug.WriteLine($"[Admin_Dashboard] INFO: Stopping count-up animation for unloaded TextBlock (Name: {textBlock.Name})."); return; }
                if (!(currentTimer.Tag is TimeSpan elapsed)) { currentTimer.Stop(); System.Diagnostics.Debug.WriteLine($"[Admin_Dashboard] WARN: Invalid Tag in ApplyCountUpAnimation timer tick. Stopping timer for TextBlock (Name: {textBlock.Name})."); return; }
                TimeSpan totalDuration = TimeSpan.FromSeconds(1.2);
                if (totalDuration.TotalSeconds <= 0) { textBlock.Text = Math.Floor(targetValue).ToString("N0"); currentTimer.Stop(); currentTimer.Tag = null; return; }
                double rawProgress = Math.Min(1.0, elapsed.TotalSeconds / totalDuration.TotalSeconds);
                double easedProgress = 1 - Math.Pow(1 - rawProgress, 3);
                var currentFrameValue = startValue + (targetValue - startValue) * easedProgress;
                textBlock.Text = Math.Floor(currentFrameValue).ToString("N0");
                if (rawProgress >= 1.0) { textBlock.Text = Math.Floor(targetValue).ToString("N0"); currentTimer.Stop(); currentTimer.Tag = null; } else { currentTimer.Tag = elapsed + TimeSpan.FromMilliseconds(20); }
            }, Application.Current.Dispatcher);
            var delayTimer = new DispatcherTimer(DispatcherPriority.Render) { Interval = TimeSpan.FromSeconds(delayInSeconds) };
            delayTimer.Tick += (sender, eventArgs) => { if (timer != null) { timer.Tag = TimeSpan.Zero; timer.Start(); } else { System.Diagnostics.Debug.WriteLine($"[Admin_Dashboard] ERROR: Main 'timer' was null in delayTimer Tick for TextBlock (Name: {textBlock?.Name})."); } (sender as DispatcherTimer)?.Stop(); };
            RoutedEventHandler unloadHandler = null;
            unloadHandler = (sender, args) => { timer?.Stop(); timer = null; delayTimer?.Stop(); delayTimer = null; textBlock.Unloaded -= unloadHandler; System.Diagnostics.Debug.WriteLine($"[Admin_Dashboard] INFO: Cleaned up count-up timer on Unloaded event for TextBlock (Name: {textBlock.Name})."); };
            textBlock.Unloaded += unloadHandler;
            delayTimer.Start();
        }


        // --- Event Handlers for Buttons ---
        private void SaveNotesButton_Click(object sender, RoutedEventArgs e)
        {
            string notes = AdminNotesTextBox.Text;
            // TODO: Implement logic to save 'notes' to database or local storage
            MessageBox.Show("Notes saved (simulated).", "Save Notes", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Open Add/Edit Task Window/Dialog
            MessageBox.Show("Add New Task dialog should open here.", "Add Task", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void EditTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is int taskId)
            {
                var taskToEdit = AdminTasks.FirstOrDefault(t => t.TaskId == taskId);
                if (taskToEdit != null)
                {
                    // TODO: Open Add/Edit Task Window/Dialog with taskToEdit data
                    MessageBox.Show($"Edit Task dialog for Task ID: {taskId} ({taskToEdit.Title}) should open.", "Edit Task", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else { MessageBox.Show($"Task with ID {taskId} not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning); }
            }
        }

        private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is int taskId)
            {
                var taskToDelete = AdminTasks.FirstOrDefault(t => t.TaskId == taskId);
                if (taskToDelete != null)
                {
                    var result = MessageBox.Show($"Are you sure you want to delete task '{taskToDelete.Title}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        // TODO: Delete task from database FIRST
                        // If DB delete succeeds:
                        AdminTasks.Remove(taskToDelete);
                        MessageBox.Show($"Task ID: {taskId} deleted (simulated).", "Delete Task", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else { MessageBox.Show($"Task with ID {taskId} not found for deletion.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning); }
            }
        }

        private void ChartTimeRange_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChartTimeRangeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string timeRange = selectedItem.Content.ToString();
                if (timeRange == "Custom Range...")
                {
                    // TODO: Implement custom date range picker logic
                    MessageBox.Show("Custom range selection not yet implemented.", "Chart Range", MessageBoxButton.OK, MessageBoxImage.Information);
                    // ChartTimeRangeComboBox.SelectedIndex = 0; // Revert selection
                }
                else { LoadChartData(timeRange); }
            }
        }


        // --- INotifyPropertyChanged Implementation ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // --- Page Unloaded ---
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _timer?.Stop();
            this.Unloaded -= Page_Unloaded;
            // TODO: Add cleanup for any other timers if necessary (e.g., animation timers if not handled by Unloaded event on TextBlock)
        }
    }
}