using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using AlQaim.Models; 
using AlQaim.Converters; 
using MahApps.Metro.IconPacks; 
using System.Windows.Threading;
using System.Windows.Media.Animation;

namespace AlQaim
{
    /// <summary>
    /// Interaction logic for Admin_Requests.xaml
    /// </summary>
    public partial class Admin_Requests : Page // Consider IDisposable
    {
        // Observable collection to hold the request data
        private ObservableCollection<LabRequest> _labRequests;
        // Collection view for filtering
        private ICollectionView _requestsView;
        // Currently selected request (for modal)
        private LabRequest _selectedRequest;

        // --- Additions for Toast ---
        private DispatcherTimer _toastTimer;
        // --- End Additions ---

        public Admin_Requests()
        {
            InitializeComponent();

            // --- Additions for Toast ---
            _toastTimer = new DispatcherTimer();
            _toastTimer.Interval = TimeSpan.FromSeconds(3); // Standard duration
            _toastTimer.Tick += ToastTimer_Tick;
            // --- End Additions ---

            LoadRequestData(); // Load data after timer init
            SetupCollectionView(); // Setup view after data load
        }

        // --- Additions for Toast ---
        private void ToastTimer_Tick(object sender, EventArgs e)
        {
            _toastTimer.Stop();
            HideToast();
        }
        // --- End Additions ---

        // Loads request data (replace with actual DB logic)
        private void LoadRequestData()
        {
            try
            {
                // In a real application, this would come from a database
                _labRequests = new ObservableCollection<LabRequest>
                {
                     new LabRequest { RequestId = "REQ-1001", RequestType = "Test Request", SubmittedBy = "John Doe", SubmissionDate = DateTime.Now.AddDays(-1), Description = "Blood test for patient P-10025", Status = "Pending", CanApprove = true, CanReject = true, PatientId = "P-10025", PatientName = "Ahmed Ali", TestType = "Complete Blood Count (CBC)", DoctorNotes = "Patient has been showing signs of fatigue. Check for anemia and infection markers." },
                     new LabRequest { RequestId = "REQ-1002", RequestType = "Inventory Restock", SubmittedBy = "Sarah Johnson", SubmissionDate = DateTime.Now.AddDays(-2), Description = "Low stock on blood collection tubes", Status = "Approved", CanApprove = false, CanReject = false, ItemCode = "INV-5621", ItemName = "Blood Collection Tubes (EDTA)", CurrentStock = "5 units", RequestedQuantity = "20 units", Priority = "High" },
                     new LabRequest { RequestId = "REQ-1003", RequestType = "Equipment Maintenance", SubmittedBy = "Mike Williams", SubmissionDate = DateTime.Now.AddDays(-3), Description = "Centrifuge needs maintenance", Status = "Rejected", CanApprove = false, CanReject = false, EquipmentId = "EQ-8795", EquipmentName = "Centrifuge Model XL-5000", MaintenanceType = "Preventive", LastMaintenance = "01/15/2025", EquipmentPriority = "Medium", IssueDescription = "Machine making unusual noise during operation. Required for routine preventive maintenance as per schedule." },
                     new LabRequest { RequestId = "REQ-1004", RequestType = "Test Request", SubmittedBy = "Rachel Green", SubmissionDate = DateTime.Now.AddHours(-12), Description = "Urinalysis for patient P-10045", Status = "Pending", CanApprove = true, CanReject = true, PatientId = "P-10045", PatientName = "Fatima Khan", TestType = "Complete Urinalysis", DoctorNotes = "Patient complains of pain during urination. Check for infection markers." },
                     new LabRequest { RequestId = "REQ-1005", RequestType = "Inventory Restock", SubmittedBy = "Daniel Brown", SubmissionDate = DateTime.Now.AddDays(-1), Description = "Low stock on test reagents", Status = "Pending", CanApprove = true, CanReject = true, ItemCode = "INV-6734", ItemName = "Glucose Test Strips", CurrentStock = "10 boxes", RequestedQuantity = "25 boxes", Priority = "Medium" }
                };

                // Update ItemsSource only if view exists, otherwise SetupCollectionView will handle it
                if (_requestsView != null)
                {
                    _requestsView = CollectionViewSource.GetDefaultView(_labRequests); // Recreate view if needed
                    dgRequests.ItemsSource = _requestsView;
                    _requestsView.Filter = RequestsFilter; // Re-apply filter function
                }
                else
                {
                    SetupCollectionView(); // Initial setup
                }

            }
            catch (Exception ex)
            {
                // Use the new standard toast for errors
                ShowToast($"Error loading request data: {ex.Message}", PackIconMaterialKind.AlertCircleOutline);
                _labRequests = new ObservableCollection<LabRequest>(); // Ensure collection is not null
                if (_requestsView != null)
                {
                    _requestsView = CollectionViewSource.GetDefaultView(_labRequests);
                    dgRequests.ItemsSource = _requestsView;
                }
            }
        }

        // Sets up the CollectionView for filtering
        private void SetupCollectionView()
        {
            if (_labRequests == null) _labRequests = new ObservableCollection<LabRequest>(); // Ensure collection exists

            _requestsView = CollectionViewSource.GetDefaultView(_labRequests);
            _requestsView.Filter = RequestsFilter; // Assign filter predicate
            dgRequests.ItemsSource = _requestsView; // Bind DataGrid
        }

        // Filter predicate for the requests view
        private bool RequestsFilter(object item)
        {
            if (!(item is LabRequest request)) return false;

            // Type filter
            bool typeMatch = true;
            if (cmbRequestType.SelectedItem is ComboBoxItem selectedTypeItem && cmbRequestType.SelectedIndex > 0)
            {
                typeMatch = request.RequestType == selectedTypeItem.Content.ToString();
            }

            // Search filter (case-insensitive)
            string searchText = txtSearch.Text?.ToLowerInvariant() ?? string.Empty;
            bool searchMatch = string.IsNullOrEmpty(searchText) ||
                               (request.RequestId?.ToLowerInvariant().Contains(searchText) ?? false) ||
                               (request.RequestType?.ToLowerInvariant().Contains(searchText) ?? false) ||
                               (request.SubmittedBy?.ToLowerInvariant().Contains(searchText) ?? false) ||
                               (request.Description?.ToLowerInvariant().Contains(searchText) ?? false) ||
                               (request.Status?.ToLowerInvariant().Contains(searchText) ?? false) ||
                               (request.PatientName?.ToLowerInvariant().Contains(searchText) ?? false) ||
                               (request.EquipmentName?.ToLowerInvariant().Contains(searchText) ?? false) ||
                               (request.ItemName?.ToLowerInvariant().Contains(searchText) ?? false);

            return typeMatch && searchMatch;
        }


        #region Event Handlers

        // Handles text changes in the search box to filter the list
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _requestsView?.Refresh(); // Refresh the view to apply the filter
        }

        // Handles selection changes in the request type filter ComboBox
        private void CmbRequestType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _requestsView?.Refresh(); // Refresh the view to apply the filter
        }

        // Handles the Refresh button click
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // In a real application, this would reload data from the database
                LoadRequestData(); // Reloads data

                // Reset filters
                txtSearch.Clear();
                cmbRequestType.SelectedIndex = 0; // Select "All Requests"

                // Optional: Refresh view again after resetting filters, though LoadData should handle it
                _requestsView?.Refresh();

                // --- Use standard toast for Refresh ---
                ShowToast("Request list refreshed.", PackIconMaterialKind.Refresh, useInfoColor: true);
                // --- End Use standard toast ---
            }
            catch (Exception ex)
            {
                ShowToast($"Error refreshing requests: {ex.Message}", PackIconMaterialKind.AlertCircleOutline);
            }
        }

        // Handles selection changes in the DataGrid
        private void DgRequests_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedRequest = dgRequests.SelectedItem as LabRequest;
        }

        // Handles the View button click in a DataGrid row
        private void BtnView_Click(object sender, RoutedEventArgs e)
        {
            // Get the request associated with the clicked button's row context
            if (sender is Button button && button.DataContext is LabRequest request)
            {
                ShowRequestDetails(request); // Show details in the modal
            }
        }

        // Handles the Approve button click in a DataGrid row
        private void BtnApprove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is LabRequest request)
            {
                // Add confirmation dialog? Or approve directly? Assuming direct for now.
                ApproveRequest(request); // Approve the request
            }
        }

        // Handles the Reject button click in a DataGrid row
        private void BtnReject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is LabRequest request)
            {
                // Add confirmation dialog? Or reject directly? Assuming direct for now.
                RejectRequest(request); // Reject the request
            }
        }

        // Handles the Close button (X) click on the modal
        private void BtnCloseModal_Click(object sender, RoutedEventArgs e)
        {
            modalOverlay.Visibility = Visibility.Collapsed; // Hide the modal
            _selectedRequest = null; // Clear selection when modal closes
        }

        // Handles the Approve button click inside the modal
        private void BtnApproveInModal_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRequest != null)
            {
                ApproveRequest(_selectedRequest); // Approve the currently viewed request
                modalOverlay.Visibility = Visibility.Collapsed; // Close modal
                _selectedRequest = null;
            }
        }

        // Handles the Reject button click inside the modal
        private void BtnRejectInModal_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRequest != null)
            {
                RejectRequest(_selectedRequest); // Reject the currently viewed request
                modalOverlay.Visibility = Visibility.Collapsed; // Close modal
                _selectedRequest = null;
            }
        }

        // Optional: Cleanup timer on page unload
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_toastTimer != null && _toastTimer.IsEnabled)
            {
                _toastTimer.Stop();
            }
        }

        #endregion

        #region Helper Methods

        // Displays the details of the selected request in the modal
        private void ShowRequestDetails(LabRequest request)
        {
            if (request == null) return;

            _selectedRequest = request; // Store the request being viewed

            // Set common modal fields
            modalTitle.Text = $"{request.RequestType} Details";
            modalRequestId.Text = $"Request ID: {request.RequestId}";
            modalStatus.Text = request.Status;

            // Set status pill color using the converter logic
            var colorConverter = FindResource("StatusToColorConverter") as StatusToColorConverter;
            if (colorConverter != null)
            {
                modalStatusPill.Background = colorConverter.Convert(request.Status, typeof(Brush), null, System.Globalization.CultureInfo.CurrentCulture) as Brush ?? Brushes.Gray;
            }
            else
            {
                // Fallback if converter not found
                modalStatusPill.Background = Brushes.Gray;
            }


            txtRequestType.Text = request.RequestType;
            txtSubmissionDate.Text = request.SubmissionDate.ToString("MM/dd/yyyy HH:mm"); // Consistent format
            txtSubmittedBy.Text = request.SubmittedBy;
            txtDescription.Text = request.Description;

            // Reset visibility of specific sections
            testRequestFields.Visibility = Visibility.Collapsed;
            inventoryRequestFields.Visibility = Visibility.Collapsed;
            equipmentRequestFields.Visibility = Visibility.Collapsed;

            // Populate and show the relevant section based on RequestType
            switch (request.RequestType)
            {
                case "Test Request":
                    testRequestFields.Visibility = Visibility.Visible;
                    txtPatientId.Text = request.PatientId ?? "N/A";
                    txtPatientName.Text = request.PatientName ?? "N/A";
                    txtTestType.Text = request.TestType ?? "N/A";
                    txtDoctorNotes.Text = request.DoctorNotes ?? "None";
                    break;

                case "Inventory Restock":
                    inventoryRequestFields.Visibility = Visibility.Visible;
                    txtItemCode.Text = request.ItemCode ?? "N/A";
                    txtItemName.Text = request.ItemName ?? "N/A";
                    txtCurrentStock.Text = request.CurrentStock ?? "N/A";
                    txtRequestedQuantity.Text = request.RequestedQuantity ?? "N/A";
                    txtPriority.Text = request.Priority ?? "N/A";
                    break;

                case "Equipment Maintenance":
                    equipmentRequestFields.Visibility = Visibility.Visible;
                    txtEquipmentId.Text = request.EquipmentId ?? "N/A";
                    txtEquipmentName.Text = request.EquipmentName ?? "N/A";
                    txtMaintenanceType.Text = request.MaintenanceType ?? "N/A";
                    txtLastMaintenance.Text = request.LastMaintenance ?? "N/A";
                    txtEquipmentPriority.Text = request.EquipmentPriority ?? "N/A";
                    txtIssueDescription.Text = request.IssueDescription ?? "None";
                    break;
            }

            // Enable/Disable action buttons in modal based on request state
            btnApproveInModal.IsEnabled = request.CanApprove;
            btnRejectInModal.IsEnabled = request.CanReject;

            // Show the modal
            modalOverlay.Visibility = Visibility.Visible;
        }

        // Approves the given lab request
        private void ApproveRequest(LabRequest request)
        {
            if (request == null || !request.CanApprove) return;

            try
            {
                // --- TODO: Implement actual database update logic here ---
                // UpdateRequestStatusInDatabase(request.RequestId, "Approved");

                // Update ViewModel properties
                request.Status = "Approved";
                request.CanApprove = false; // Cannot approve again
                request.CanReject = false; // Cannot reject after approval

                _requestsView?.Refresh(); // Refresh view to reflect changes
                                          // Use the standard toast notification
                ShowToast($"Request {request.RequestId} approved.", PackIconMaterialKind.CheckCircleOutline);
            }
            catch (Exception ex)
            {
                ShowToast($"Error approving request {request.RequestId}: {ex.Message}", PackIconMaterialKind.AlertCircleOutline);
            }
        }


        // Rejects the given lab request
        private void RejectRequest(LabRequest request)
        {
            if (request == null || !request.CanReject) return;

            try
            {
                // --- TODO: Implement actual database update logic here ---
                // UpdateRequestStatusInDatabase(request.RequestId, "Rejected");

                // Update ViewModel properties
                request.Status = "Rejected";
                request.CanApprove = false; // Cannot approve after rejection
                request.CanReject = false; // Cannot reject again

                _requestsView?.Refresh(); // Refresh view to reflect changes
                                          // Use the standard toast notification
                ShowToast($"Request {request.RequestId} rejected.", PackIconMaterialKind.CloseCircleOutline); // Use Close icon
            }
            catch (Exception ex)
            {
                ShowToast($"Error rejecting request {request.RequestId}: {ex.Message}", PackIconMaterialKind.AlertCircleOutline);
            }
        }

        #endregion

        #region Toast Notification Methods (Standard Implementation)

        // Shows a toast notification message
        private void ShowToast(string message, PackIconMaterialKind iconKind, bool useInfoColor = false)
        {
            // Ensure execution on the UI thread
            Dispatcher.Invoke(() =>
            {
                toastMessage.Text = message;
                toastIcon.Kind = iconKind;

                // Determine background color based on message type/icon
                SolidColorBrush backgroundBrush;
                if (iconKind == PackIconMaterialKind.AlertCircleOutline || iconKind == PackIconMaterialKind.Alert)
                {
                    // Error/Warning
                    backgroundBrush = FindResource("RejectedColor") as SolidColorBrush ?? new SolidColorBrush(Colors.Red); // Use RejectedColor for errors
                }
                else if (iconKind == PackIconMaterialKind.CloseCircleOutline) // Specific icon for rejection confirmation
                {
                    backgroundBrush = FindResource("RejectedColor") as SolidColorBrush ?? new SolidColorBrush(Colors.Red);
                }
                else if (useInfoColor || iconKind == PackIconMaterialKind.Refresh || iconKind == PackIconMaterialKind.Information)
                {
                    // Informational
                    backgroundBrush = FindResource("PrimaryAccentColor") as SolidColorBrush ?? new SolidColorBrush(Colors.DodgerBlue); // Use PrimaryAccent for Info/Refresh
                }
                else // Default to Success (Check icon)
                {
                    backgroundBrush = FindResource("ApprovedColor") as SolidColorBrush ?? new SolidColorBrush(Colors.Green); // Use ApprovedColor for success
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
            Dispatcher.Invoke(() =>
            {
                if (toastNotification.Visibility != Visibility.Visible) return;

                var fadeOut = new DoubleAnimation { From = toastNotification.Opacity, To = 0, Duration = TimeSpan.FromMilliseconds(300) };
                fadeOut.Completed += (s, e) =>
                {
                    if (toastNotification.Opacity < 0.1) // Check again in case interrupted
                    {
                        toastNotification.Visibility = Visibility.Collapsed;
                    }
                };
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

} 
