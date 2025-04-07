using MahApps.Metro.IconPacks;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using AlQaim.Models; 
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.IO; 
using System.Runtime.CompilerServices; 

namespace AlQaim
{
    public partial class LT_Requests : Page, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) 
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Fields & Properties

        private ObservableCollection<LabRequest> _requestHistory;
        public ObservableCollection<LabRequest> RequestHistory
        {
            get { return _requestHistory; }
            set { _requestHistory = value; OnPropertyChanged(nameof(RequestHistory)); }
        }

        private ICollectionView _requestHistoryView;
        public ICollectionView RequestHistoryView 
        {
            get { return _requestHistoryView; }
            set { _requestHistoryView = value; OnPropertyChanged(nameof(RequestHistoryView)); }
        }

        private LabRequest _selectedRequest; // Currently selected/viewed request
        private DispatcherTimer _toastTimer;
        private string _currentTechnicianName = "Alex Johnson"; // Placeholder - get from actual login/session

   
        private LabRequest _requestToCancel;
      

        #endregion

        public LT_Requests()
        {
            InitializeComponent();
            this.DataContext = this; 

            RequestHistory = new ObservableCollection<LabRequest>();
            RequestHistoryView = CollectionViewSource.GetDefaultView(RequestHistory);
            RequestHistoryView.Filter = FilterRequests; // Set initial filter delegate

            InitializeTimers();
            InitializeFormDefaults();
            LoadRequestHistoryData(); // Load initial history
        }

        #region Initialization
        private void InitializeTimers()
        {
            _toastTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(4) }; // Slightly longer duration
            _toastTimer.Tick += (s, e) => HideToast();
        }

        private void InitializeFormDefaults()
        {
            RequestDatePicker.SelectedDate = DateTime.Today;
            TechnicianNameBlock.Text = _currentTechnicianName; // Set display name
            if (UrgencyLevelComboBox.Items.Count > 1)
                UrgencyLevelComboBox.SelectedIndex = 1; // Default to Medium
        }
        #endregion

        #region Data Loading & Filtering

        private void LoadRequestHistoryData()
        {
            // --- TODO: Replace with actual database call ---
            ShowToast("Loading your request history...", PackIconMaterialKind.DatabaseSyncOutline, isInfo: true, durationSeconds: 2);

            try
            {
                // Simulate DB Load Delay
                var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.5) };
                timer.Tick += (sender, args) =>
                {
                    timer.Stop();
                    RequestHistory.Clear(); // Clear existing data

                    // Sample data specific to the assumed current user "Alex Johnson"
                    RequestHistory.Add(new LabRequest { RequestId = "REQ-L001", RequestType = "Test Re-Request", SubmittedBy = _currentTechnicianName, SubmissionDate = DateTime.Now.AddDays(-5), Description = "CBC needed again for P-012", Status = "Approved", Urgency = "Medium", AdminNotes = "Approved, proceed." });
                    RequestHistory.Add(new LabRequest { RequestId = "REQ-L002", RequestType = "Inventory Restock", SubmittedBy = _currentTechnicianName, SubmissionDate = DateTime.Now.AddDays(-3), Description = "Need more Pipette Tips", Status = "Pending", Urgency = "Low", AttachmentPath = @"C:\lab_files\pipette_inventory.xlsx" });
                    RequestHistory.Add(new LabRequest { RequestId = "REQ-L003", RequestType = "Equipment Maintenance", SubmittedBy = _currentTechnicianName, SubmissionDate = DateTime.Now.AddDays(-1), Description = "Microscope light Flickering", Status = "Rejected", Urgency = "High", AdminNotes = "Maintenance already scheduled for next week." });
                    RequestHistory.Add(new LabRequest { RequestId = "REQ-L004", RequestType = "Software Issue", SubmittedBy = _currentTechnicianName, SubmissionDate = DateTime.Now.AddHours(-4), Description = "LIS login fails randomly", Status = "Pending", Urgency = "Critical" });
                    RequestHistory.Add(new LabRequest { RequestId = "REQ-L005", RequestType = "Other", SubmittedBy = _currentTechnicianName, SubmissionDate = DateTime.Now.AddMinutes(-30), Description = "Query about sample storage protocol", Status = "Pending", Urgency = "Medium" });

                    RequestHistoryView.Refresh();
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                ShowToast($"Error loading history: {ex.Message}", PackIconMaterialKind.AlertCircleOutline, isError: true, durationSeconds: 5);
            }
            // --- End DB Call Placeholder ---
        }

        private bool FilterRequests(object item)
        {
            if (!(item is LabRequest request)) return false;

            // Search Text Filter
            string searchText = txtSearch.Text?.Trim().ToLowerInvariant() ?? ""; // Use InvariantCulture
            bool matchesSearch = true;
            if (!string.IsNullOrWhiteSpace(searchText)) // Use IsNullOrWhiteSpace
            {
                matchesSearch = (request.RequestId?.ToLowerInvariant().Contains(searchText) ?? false) ||
                                (request.RequestType?.ToLowerInvariant().Contains(searchText) ?? false) ||
                                (request.Description?.ToLowerInvariant().Contains(searchText) ?? false) ||
                                (request.Status?.ToLowerInvariant().Contains(searchText) ?? false);
            }

            return matchesSearch;
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            RequestHistoryView?.Refresh();
        }

        #endregion

        #region Event Handlers

        // --- Header ---
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear(); // Clear search on refresh
            LoadRequestHistoryData();
            ShowToast("Request history refreshed", PackIconMaterialKind.Refresh, isInfo: true);
        }

        // --- Form ---
        private void AttachFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "All files (*.*)|*.*|PDF Documents (*.pdf)|*.pdf|Images (*.png;*.jpg)|*.png;*.jpg",
                Title = "Select a File to Attach"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                AttachmentPathTextBox.Text = openFileDialog.FileName;
            }
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // Basic Validation
            if (RequestTypeComboBox.SelectedItem == null)
            {
                ShowError("Please select a request type."); return;
            }
            if (UrgencyLevelComboBox.SelectedItem == null)
            {
                ShowError("Please select an urgency level."); return;
            }
            if (RequestDatePicker.SelectedDate == null)
            {
                ShowError("Please select a date."); return;
            }
            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                ShowError("Please enter a description."); return;
            }

            ErrorMessageBlock.Visibility = Visibility.Collapsed; // Clear previous errors

            try
            {
                // --- TODO: Database Interaction - Save Request ---
                string newRequestId = $"REQ-{DateTime.Now.Ticks % 100000}"; // Simple placeholder ID
                var newRequest = new LabRequest
                {
                    RequestId = newRequestId,
                    RequestType = (RequestTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    SubmittedBy = _currentTechnicianName,
                    SubmissionDate = DateTime.Now,
                    RequestDate = RequestDatePicker.SelectedDate.Value,
                    Urgency = (UrgencyLevelComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    Description = DescriptionTextBox.Text.Trim(),
                    AttachmentPath = AttachmentPathTextBox.Text,
                    Status = "Pending"
                };
                // Example: DatabaseService.SubmitNewRequest(newRequest);
                // --- End DB Save Placeholder ---

                RequestHistory.Insert(0, newRequest);
                RequestHistoryView.Refresh();
                ResetForm();
                ShowToast("Request submitted successfully!", PackIconMaterialKind.CheckCircleOutline);
            }
            catch (Exception ex)
            {
                ShowError($"Failed to submit request: {ex.Message}");
                ShowToast("Error submitting request.", PackIconMaterialKind.AlertCircleOutline, isError: true, durationSeconds: 5);
            }
        }

        // --- History Grid Actions ---
        private void ViewDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string requestId)
            {
                _selectedRequest = RequestHistory.FirstOrDefault(r => r.RequestId == requestId);
                if (_selectedRequest != null)
                {
                    PopulateViewDetailsModal();
                    ShowModal(modalOverlay); // Assuming modalOverlay is the name of your details modal
                }
            }
        }

        private void CancelRequestButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string requestId)
            {
                var request = RequestHistory.FirstOrDefault(r => r.RequestId == requestId);

                if (request == null)
                {
                    ShowToast("Request not found.", PackIconMaterialKind.AlertCircleOutline, isError: true);
                    return;
                }

                // Check if cancellable *before* showing confirmation
                if (request.Status != "Pending")
                {
                    ShowToast("Cannot cancel this request (it may no longer be pending).", PackIconMaterialKind.AlertCircleOutline, isError: true);
                    return;
                }

                // Store the request to be cancelled
                _requestToCancel = request;

                // Configure and show the confirmation dialog
                txtConfirmationMessage.Text = $"Are you sure you want to cancel request {requestId}?";
                // Optional: Change confirm button text if desired
                // (confirmationOverlay.FindName("btnConfirmAction") as Button)?.Content = "Confirm Cancel";

                confirmationOverlay.Visibility = Visibility.Visible; // Show the confirmation overlay
            }
        }

        // --- Modal ---
        private void BtnCloseModal_Click(object sender, RoutedEventArgs e)
        {
            // Assuming modalOverlay is the name of your details modal border
            HideModal(modalOverlay);
        }

        // --- Toast ---
        private void BtnCloseToast_Click(object sender, RoutedEventArgs e)
        {
            HideToast();
        }

        // --- Confirmation Dialog Buttons ---
        private void BtnCancelConfirmation_Click(object sender, RoutedEventArgs e)
        {
            confirmationOverlay.Visibility = Visibility.Collapsed;
            _requestToCancel = null; // Clear the pending request
        }

        private void BtnConfirmAction_Click(object sender, RoutedEventArgs e)
        {
            confirmationOverlay.Visibility = Visibility.Collapsed;

            if (_requestToCancel != null)
            {
                // --- Cancellation Logic ---
                LabRequest requestToCancelNow = _requestToCancel;
                _requestToCancel = null; // Clear immediately

                try
                {
                    // --- TODO: Database Interaction - Update Status ---
                    Console.WriteLine($"Simulating DB update: Cancelling request {requestToCancelNow.RequestId}");
                    // Example: DatabaseService.UpdateRequestStatus(requestToCancelNow.RequestId, "Cancelled");
                    // --- End DB Update Placeholder ---

                    // If DB update is successful:
                    requestToCancelNow.Status = "Cancelled";
                    RequestHistoryView.Refresh();
                    ShowToast($"Request {requestToCancelNow.RequestId} cancelled.", PackIconMaterialKind.Cancel);
                }
                catch (Exception ex)
                {
                    ShowToast($"Error cancelling request: {ex.Message}", PackIconMaterialKind.AlertCircleOutline, isError: true, durationSeconds: 5);
                }
                // --- End Cancellation Logic ---
            }
        }
        // --- End Confirmation Dialog Buttons ---

        #endregion // End Event Handlers

        #region Helper Methods

        private void ShowError(string message)
        {
            ErrorMessageBlock.Text = message;
            ErrorMessageBlock.Visibility = Visibility.Visible;
        }

        private void ResetForm()
        {
            RequestTypeComboBox.SelectedIndex = -1;
            UrgencyLevelComboBox.SelectedIndex = 1; // Medium
            RequestDatePicker.SelectedDate = DateTime.Today;
            DescriptionTextBox.Text = string.Empty;
            AttachmentPathTextBox.Text = string.Empty;
            ErrorMessageBlock.Visibility = Visibility.Collapsed;
        }

        private void PopulateViewDetailsModal()
        {
            if (_selectedRequest == null) return;

            // Assuming your modal controls have these x:Name attributes
            modalRequestId.Text = $"Request ID: {_selectedRequest.RequestId ?? "N/A"}";
            modalStatus.Text = _selectedRequest.Status ?? "N/A";
            modalStatusPill.Background = ConvertStatusToColor(_selectedRequest.Status);

            txtRequestType.Text = _selectedRequest.RequestType ?? "N/A";
            txtSubmissionDate.Text = _selectedRequest.SubmissionDate.ToString("dd-MM-yyyy hh:mm tt");
            txtSubmittedBy.Text = _selectedRequest.SubmittedBy ?? "N/A";
            txtUrgency.Text = _selectedRequest.Urgency ?? "N/A";
            txtDescription.Text = _selectedRequest.Description ?? "N/A";

            // Attachment Section
            if (!string.IsNullOrWhiteSpace(_selectedRequest.AttachmentPath))
            {
                try { txtAttachment.Text = Path.GetFileName(_selectedRequest.AttachmentPath); }
                catch { txtAttachment.Text = "Invalid Path"; }
                attachmentSection.Visibility = Visibility.Visible;
            }
            else { attachmentSection.Visibility = Visibility.Collapsed; }

            // Admin Notes Section
            if (!string.IsNullOrWhiteSpace(_selectedRequest.AdminNotes))
            {
                txtAdminNotes.Text = _selectedRequest.AdminNotes;
                adminNotesSection.Visibility = Visibility.Visible;
            }
            else { adminNotesSection.Visibility = Visibility.Collapsed; }
        }

        private SolidColorBrush ConvertStatusToColor(string status)
        {
            string colorKey;
            switch (status?.ToLowerInvariant()) // Use ToLowerInvariant for case-insensitivity
            {
                case "approved": colorKey = "ApprovedColor"; break;
                case "rejected": colorKey = "RejectedColor"; break;
                case "cancelled": colorKey = "CancelledColor"; break; // Assuming you have CancelledColor
                case "pending": default: colorKey = "PendingColor"; break;
            }
            // Use TryFindResource for safety
            return TryFindResource(colorKey) as SolidColorBrush ?? (TryFindResource("PendingColor") as SolidColorBrush);
        }

        private void ShowModal(Border modal)
        {
            if (modal == null) return; // Safety check
            modal.Visibility = Visibility.Visible;
            var fadeIn = TryFindResource("FadeInAnimation") as Storyboard; // Use TryFindResource
            if (fadeIn != null)
            {
                var localFadeIn = fadeIn.Clone(); // Clone to avoid issues
                Storyboard.SetTarget(localFadeIn, modal);
                localFadeIn.Begin();
            }
            else { modal.Opacity = 1; } // Fallback
        }

        private void HideModal(Border modal)
        {
            if (modal == null) return; // Safety check
            var fadeOut = TryFindResource("FadeOutAnimation") as Storyboard; // Use TryFindResource
            if (fadeOut != null)
            {
                var localFadeOut = fadeOut.Clone(); // Clone to avoid issues
                localFadeOut.Completed += (s, e) => modal.Visibility = Visibility.Collapsed;
                Storyboard.SetTarget(localFadeOut, modal);
                localFadeOut.Begin();
            }
            else { modal.Opacity = 0; modal.Visibility = Visibility.Collapsed; } // Fallback
        }

        private void ShowToast(string message, PackIconMaterialKind iconKind, bool isError = false, bool isInfo = false, double durationSeconds = 4)
        {
            // Ensure toast elements exist
            if (toastNotification == null || toastMessage == null || toastIcon == null) return;

            _toastTimer?.Stop(); // Use null-conditional operator

            toastMessage.Text = message;
            toastIcon.Kind = iconKind;

            // Set background based on type using TryFindResource
            Brush backgroundBrush = FindResource("SuccessColor") as SolidColorBrush; // Default to Success
            if (isError) backgroundBrush = TryFindResource("ErrorColor") as SolidColorBrush ?? backgroundBrush;
            else if (isInfo) backgroundBrush = TryFindResource("InfoColor") as SolidColorBrush ?? backgroundBrush;
            // Add specific color logic if needed for non-error/info/success types
            else { backgroundBrush = TryFindResource("SuccessColor") as SolidColorBrush ?? FindResource("PrimaryAccentColor") as SolidColorBrush; } // Fallback to success or primary

            toastNotification.Background = backgroundBrush;

            toastNotification.Visibility = Visibility.Visible;
            var fadeIn = TryFindResource("FadeInAnimation") as Storyboard; // Use TryFindResource
            if (fadeIn != null)
            {
                var localFadeIn = fadeIn.Clone(); // Clone storyboard
                Storyboard.SetTarget(localFadeIn, toastNotification); // Set target
                localFadeIn.Begin(); // Begin storyboard
            }
            else { toastNotification.Opacity = 1; } // Fallback if animation resource not found

            if (_toastTimer != null) // Check if timer exists
            {
                _toastTimer.Interval = TimeSpan.FromSeconds(durationSeconds);
                _toastTimer.Start();
            }
        }

        // *** CORRECTED HideToast Method (v2) ***
        private void HideToast()
        {
            if (toastNotification == null || toastNotification.Visibility == Visibility.Collapsed) return;

            _toastTimer?.Stop(); // Use null-conditional operator

            // --- Determine the animation ---
            DoubleAnimation opacityAnimation = null;
            // Try finding resource first
            if (TryFindResource("FadeOutAnimation") is Storyboard fadeOutStoryboardResource)
            {
                // Assuming the storyboard in resources primarily controls opacity
                // Find the DoubleAnimation targeting Opacity within the resource
                opacityAnimation = fadeOutStoryboardResource.Children
                                    .OfType<DoubleAnimation>()
                                    .FirstOrDefault(da => Storyboard.GetTargetProperty(da)?.Path == "Opacity");

                // If not found directly, create a default one
                if (opacityAnimation == null)
                {
                    opacityAnimation = new DoubleAnimation
                    {
                        // From will be set by ApplyAnimationClock based on current value
                        To = 0,
                        Duration = fadeOutStoryboardResource.Duration.HasTimeSpan ? fadeOutStoryboardResource.Duration : TimeSpan.FromMilliseconds(300) // Use storyboard duration or default
                    };
                }
                else
                {
                    // Clone the animation from the resource to avoid modifying the original resource
                    opacityAnimation = opacityAnimation.Clone();
                }
            }
            else // If no resource found, create a default animation
            {
                opacityAnimation = new DoubleAnimation
                {
                    // From = toastNotification.Opacity, // Let ApplyAnimationClock handle the From value
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(300) // Default duration
                };
            }

            // --- Setup and Apply Animation ---
            EventHandler fadeOutCompletedHandler = null; // Declare handler variable outside
            fadeOutCompletedHandler = (s, e) =>
            {
                // The sender 's' is the Clock that completed. Cast to base Clock type.
                var clock = s as Clock; // <-- CORRECTED CAST to base Clock

                // Check state. Filling or Stopped are considered completed for our purpose.
                if (clock != null && (clock.CurrentState == ClockState.Filling || clock.CurrentState == ClockState.Stopped))
                {
                    // Set visibility *before* removing animation to prevent flicker
                    toastNotification.Visibility = Visibility.Collapsed;
                    // Clean up animation resources by applying null clock
                    toastNotification.ApplyAnimationClock(UIElement.OpacityProperty, null); // Use ApplyAnimationClock with null
                    // Detach the handler using the correct variable
                    clock.Completed -= fadeOutCompletedHandler; // <-- CORRECTED LINE
                }
                else if (clock == null) // Fallback if sender is somehow not a Clock
                {
                    toastNotification.Visibility = Visibility.Collapsed;
                    toastNotification.ApplyAnimationClock(UIElement.OpacityProperty, null);
                }
            };

            // Create the clock for the specific animation
            AnimationClock animationClock = opacityAnimation.CreateClock();
            // Attach the handler to the animation clock's Completed event
            animationClock.Completed += fadeOutCompletedHandler;

            
            toastNotification.ApplyAnimationClock(UIElement.OpacityProperty, animationClock, HandoffBehavior.SnapshotAndReplace);
        }
       

        #endregion 
    }


} 