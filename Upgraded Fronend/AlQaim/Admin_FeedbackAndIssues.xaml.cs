using MahApps.Metro.IconPacks; 
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading; 
using System.Threading.Tasks; 
using Microsoft.Win32; 
using System.IO;        
using System.Text;     
using System.Linq;
using System.Diagnostics; 
using System.ComponentModel; 

namespace AlQaim
{
    #region Data Classes (Define these based on your actual data model)

    // Represents a feedback entry
    public class FeedbackItem : INotifyPropertyChanged // Implement INotifyPropertyChanged if live updates are needed
    {
        private string _status;
        private string _adminNotes;

        public string SubmissionId { get; set; }
        public string SubmittedBy { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string FeedbackSource { get; set; }
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }
        public string FeedbackMessage { get; set; }
        public string ContactEmail { get; set; } // Needed for Reply functionality
        public string AdminNotes // For notes added in the modal
        {
            get => _adminNotes;
            set { _adminNotes = value; OnPropertyChanged(nameof(AdminNotes)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Represents an issue entry
    public class IssueItem : INotifyPropertyChanged // Implement INotifyPropertyChanged if live updates are needed
    {
        private string _status;
        private string _assignedTo;
        private string _resolutionNotes;

        public string IssueId { get; set; }
        public string ReportedBy { get; set; }
        public string IssueType { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }
        public string Description { get; set; }
        public string AssignedTo
        {
            get => _assignedTo;
            set { _assignedTo = value; OnPropertyChanged(nameof(AssignedTo)); }
        }
        public string ResolutionNotes // For notes added in the modal
        {
            get => _resolutionNotes;
            set { _resolutionNotes = value; OnPropertyChanged(nameof(ResolutionNotes)); }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    #endregion

    /// <summary>
    /// Interaction logic for Admin_FeedbackAndIssues.xaml
    /// </summary>
    public partial class Admin_FeedbackAndIssues : Page
    {
        // Collections for data binding
        private List<FeedbackItem> feedbackItems;
        private List<IssueItem> issueItems;
        private CollectionViewSource feedbackViewSource;
        private CollectionViewSource issueViewSource;

        // To store the item being viewed/edited in the modal
        private FeedbackItem _selectedFeedbackItem;
        private IssueItem _selectedIssueItem;

        // --- Fields for deletion confirmation ---
        private FeedbackItem _feedbackToDelete;
        private IssueItem _issueToDelete;
        // ----------------------------------------

        public Admin_FeedbackAndIssues()
        {
            InitializeComponent();
            // Load sample or real data
            LoadData();
            SetupViewSources();
            // Bind the view sources to the DataGrids AFTER they are initialized
            dgFeedback.ItemsSource = feedbackViewSource.View;
            dgIssues.ItemsSource = issueViewSource.View;

            // Set initial state
            UpdateNoDataVisibility();
        }

        #region Data Loading and Setup

        /// <summary>
        /// Loads feedback and issue data. Replace sample data loading with database access.
        /// </summary>
        private void LoadData()
        {
            ShowProcessingOverlay(true);
            try
            {
                // ***** Replace LoadSampleData() with your actual database fetching logic *****
                LoadSampleData();
                // ***************************************************************************

                // If view sources exist, update their source (needed for refresh)
                if (feedbackViewSource != null) feedbackViewSource.Source = feedbackItems;
                if (issueViewSource != null) issueViewSource.Source = issueItems;

                // Ensure views are refreshed after loading new data
                RefreshViews();
            }
            catch (Exception ex)
            {
                ShowTemporaryMessage(errorMessage, $"Error loading data: {ex.Message}", true);
                // Initialize with empty lists if loading fails
                feedbackItems = new List<FeedbackItem>();
                issueItems = new List<IssueItem>();
            }
            finally
            {
                ShowProcessingOverlay(false);
            }
        }


        /// <summary>
        /// Creates and configures the collection view sources for filtering and sorting.
        /// </summary>
        private void SetupViewSources()
        {
            feedbackViewSource = new CollectionViewSource { Source = feedbackItems };
            issueViewSource = new CollectionViewSource { Source = issueItems };

            // Enable sorting (optional, relies on DataGrid columns SortMemberPath)
            feedbackViewSource.SortDescriptions.Add(new SortDescription("SubmissionDate", ListSortDirection.Descending));
            issueViewSource.SortDescriptions.Add(new SortDescription("SubmissionDate", ListSortDirection.Descending));

            // Setup filtering predicates
            feedbackViewSource.Filter += ApplyFeedbackFilters;
            issueViewSource.Filter += ApplyIssueFilters;
        }

        /// <summary>
        /// Loads sample data for demonstration purposes.
        /// </summary>
        private void LoadSampleData()
        {
            // Sample feedback data
            feedbackItems = new List<FeedbackItem>
            {
                new FeedbackItem
                {
                    SubmissionId = "FBK-1001",
                    SubmittedBy = "John Doe",
                    SubmissionDate = DateTime.Now.AddDays(-5).Date,
                    FeedbackSource = "Email",
                    Status = "Pending",
                    FeedbackMessage = "The lab results were delivered promptly, and the staff was very professional. I appreciate the quick service.",
                    ContactEmail = "john.doe@example.com",
                    AdminNotes = ""
                },
                new FeedbackItem
                {
                    SubmissionId = "FBK-1002",
                    SubmittedBy = "Sarah Johnson",
                    SubmissionDate = DateTime.Now.AddDays(-3).Date,
                    FeedbackSource = "SMS",
                    Status = "Reviewed",
                    FeedbackMessage = "I had to wait too long for my appointment. The testing process was efficient, but the waiting time was excessive.",
                    ContactEmail = "sarah.j@example.com",
                    AdminNotes = "Followed up with reception regarding wait times."
                },
                new FeedbackItem
                {
                    SubmissionId = "FBK-1003",
                    SubmittedBy = "Anonymous",
                    SubmissionDate = DateTime.Now.AddDays(-1).Date,
                    FeedbackSource = "Online Form",
                    Status = "Action Taken",
                    FeedbackMessage = "The online portal for accessing test results is confusing to navigate. It would be helpful to have a more user-friendly interface.",
                    ContactEmail = "",
                    AdminNotes = "UI/UX team notified. Portal redesign scheduled for Q3."
                }
            };

            // Sample issue data
            issueItems = new List<IssueItem>
            {
                new IssueItem
                {
                    IssueId = "ISS-1001",
                    ReportedBy = "Dr. Michael Brown (Lab Tech)",
                    IssueType = "Equipment",
                    SubmissionDate = DateTime.Now.AddDays(-7).Date,
                    Status = "In Progress",
                    Description = "The centrifuge in Lab 3 is making unusual noises during operation. It may need maintenance or replacement.",
                    AssignedTo = "John Smith (Maintenance)",
                    ResolutionNotes = "Scheduled maintenance for next week."
                },
                new IssueItem
                {
                    IssueId = "ISS-1002",
                    ReportedBy = "Lisa Chen (Lab Tech)",
                    IssueType = "Inventory",
                    SubmissionDate = DateTime.Now.AddDays(-2).Date,
                    Status = "New",
                    Description = "We're running low on reagent kits for the blood analyzer. Need to restock as soon as possible.",
                    AssignedTo = "Not Assigned",
                    ResolutionNotes = ""
                },
                new IssueItem
                {
                    IssueId = "ISS-1003",
                    ReportedBy = "Patient Feedback (FBK-1004)", // Can link back if needed
                    IssueType = "Service",
                    SubmissionDate = DateTime.Now.AddDays(-1).Date,
                    Status = "Resolved",
                    Description = "Multiple patients have reported that they did not receive their test results via email as promised.",
                    AssignedTo = "Jessica Brown (Lab Manager)",
                    ResolutionNotes = "Email notification system checked and fixed. Resent affected results."
                }
            };
        }

        /// <summary>
        /// Updates the visibility of the no data panel based on whether there's data in the *currently selected* tab's view.
        /// </summary>
        private void UpdateNoDataVisibility()
        {
            bool hasData = false;

            if (tabFeedbackIssues == null || feedbackViewSource == null || issueViewSource == null)
                return; // Controls not ready yet

            try // View might be null briefly during initialization or refresh
            {
                if (tabFeedbackIssues.SelectedIndex == 0) // Feedback tab
                {
                    hasData = feedbackViewSource.View != null && !feedbackViewSource.View.IsEmpty;
                }
                else // Issues tab
                {
                    hasData = issueViewSource.View != null && !issueViewSource.View.IsEmpty;
                }
            }
            catch { /* Ignore potential null reference during transitions */ }


            noDataPanel.Visibility = hasData ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion

        #region Filtering and Searching

        /// <summary>
        /// Applies filters to the feedback data based on search text and combo box selection.
        /// </summary>
        private void ApplyFeedbackFilters(object sender, FilterEventArgs e)
        {
            if (e.Item is not FeedbackItem feedback || cmbFilter == null || txtSearch == null)
            {
                e.Accepted = false; // Don't accept if item is wrong type or controls not ready
                return;
            }


            bool matchesSearch = true;
            bool matchesCombo = true;
            string searchText = txtSearch.Text.ToLowerInvariant().Trim();
            string filterText = (cmbFilter.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "All Items";

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                matchesSearch = feedback.SubmissionId.ToLowerInvariant().Contains(searchText) ||
                                feedback.SubmittedBy.ToLowerInvariant().Contains(searchText) ||
                                feedback.FeedbackMessage.ToLowerInvariant().Contains(searchText) ||
                                feedback.FeedbackSource.ToLowerInvariant().Contains(searchText) ||
                                feedback.Status.ToLowerInvariant().Contains(searchText);
            }

            // Apply combo box filter only if search matches
            if (matchesSearch && filterText != "All Items")
            {
                matchesCombo = filterText switch
                {
                    "Feedback Only" => true, // Always true if we are in this filter method
                    "Issues Only" => false,  // Never true if we are in this filter method
                    "Pending Status" => feedback.Status == "Pending",
                    "Resolved/Reviewed" => feedback.Status == "Reviewed" || feedback.Status == "Action Taken",
                    _ => true // Should not happen with defined items
                };
            }

            e.Accepted = matchesSearch && matchesCombo;
        }


        /// <summary>
        /// Applies filters to the issue data based on search text and combo box selection.
        /// </summary>
        private void ApplyIssueFilters(object sender, FilterEventArgs e)
        {
            if (e.Item is not IssueItem issue || cmbFilter == null || txtSearch == null)
            {
                e.Accepted = false; // Don't accept if item is wrong type or controls not ready
                return;
            }

            bool matchesSearch = true;
            bool matchesCombo = true;
            string searchText = txtSearch.Text.ToLowerInvariant().Trim();
            string filterText = (cmbFilter.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "All Items";

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                matchesSearch = issue.IssueId.ToLowerInvariant().Contains(searchText) ||
                                issue.ReportedBy.ToLowerInvariant().Contains(searchText) ||
                                issue.Description.ToLowerInvariant().Contains(searchText) ||
                                issue.IssueType.ToLowerInvariant().Contains(searchText) ||
                                issue.Status.ToLowerInvariant().Contains(searchText) ||
                                issue.AssignedTo.ToLowerInvariant().Contains(searchText);
            }

            // Apply combo box filter only if search matches
            if (matchesSearch && filterText != "All Items")
            {
                matchesCombo = filterText switch
                {
                    "Feedback Only" => false, // Never true here
                    "Issues Only" => true,  // Always true here
                    "Pending Status" => issue.Status == "New" || issue.Status == "In Progress",
                    "Resolved/Reviewed" => issue.Status == "Resolved",
                    _ => true
                };
            }

            e.Accepted = matchesSearch && matchesCombo;
        }


        /// <summary>
        /// Refreshes the data views when search text changes.
        /// </summary>
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshViews();
        }

        /// <summary>
        /// Refreshes the data views when filter selection changes.
        /// </summary>
        private void CmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshViews();
        }

        /// <summary>
        /// Refresh both data views by reapplying filters and updating visibility.
        /// </summary>
        private void RefreshViews()
        {
            // Ensure view sources are not null before refreshing
            feedbackViewSource?.View?.Refresh();
            issueViewSource?.View?.Refresh();
            UpdateNoDataVisibility(); // Update visibility based on current filters/tab
        }

        #endregion

        #region General UI Event Handlers (Refresh, Export, TabChange)

        /// <summary>
        /// Handles the Refresh button click. Clears filters and reloads data.
        /// </summary>
        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            // Clear filters visually
            txtSearch.Text = string.Empty;
            if (cmbFilter.Items.Count > 0) cmbFilter.SelectedIndex = 0;

            // Reload data (replace LoadSampleData with your actual data fetch)
            LoadData(); // LoadData handles processing overlay and messages

            // Show success message (optional, as LoadData might show errors)
            // Add a small delay to ensure processing overlay is gone
            await Task.Delay(100); // Small delay
            if (processingOverlay.Visibility == Visibility.Collapsed) // Only show success if no error occurred during load
            {
                // Using the standard toast now for consistency, if available
                // If toastNotification is not added, revert to ShowTemporaryMessage
                // ShowToast("Data refreshed successfully.", PackIconMaterialKind.Refresh);
                ShowTemporaryMessage(successMessage, "Data refreshed successfully.");
            }
        }

        /// <summary>
        /// Handles the Export button click. Exports the currently visible data grid content to CSV.
        /// </summary>
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            DataGrid currentGrid = (tabFeedbackIssues.SelectedIndex == 0) ? dgFeedback : dgIssues;
            string defaultFileName = (tabFeedbackIssues.SelectedIndex == 0) ? "FeedbackExport.csv" : "IssuesExport.csv";

            if (currentGrid.Items.Count == 0)
            {
                ShowTemporaryMessage(errorMessage, "There is no data to export.", true);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV file (*.csv)|*.csv",
                Title = "Export Data to CSV",
                FileName = defaultFileName
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                ShowProcessingOverlay(true);
                try
                {
                    StringBuilder csvContent = new StringBuilder();

                    // Add header row
                    List<string> headers = currentGrid.Columns.Select(column => column.Header.ToString()).ToList();
                    // Exclude the 'Actions' column header if it exists
                    headers.Remove("Actions");
                    csvContent.AppendLine(string.Join(",", headers));

                    // Add data rows (using the View to get filtered/sorted data)
                    foreach (var item in currentGrid.ItemsSource) // Use ItemsSource which should be the View
                    {
                        List<string> fields = new List<string>();
                        foreach (DataGridColumn column in currentGrid.Columns)
                        {
                            if (column.Header.ToString() == "Actions") continue; // Skip actions column

                            string cellValue = "";
                            if (column is DataGridTextColumn textColumn)
                            {
                                var binding = textColumn.Binding as Binding;
                                if (binding != null)
                                {
                                    var property = item.GetType().GetProperty(binding.Path.Path);
                                    if (property != null)
                                    {
                                        var value = property.GetValue(item, null);
                                        cellValue = value?.ToString() ?? "";
                                        // Handle potential DateTime formatting if needed
                                        if (value is DateTime dt)
                                        {
                                            // Apply the same format as in the grid if possible, or a default one
                                            cellValue = dt.ToString("MM/dd/yyyy HH:mm");
                                        }
                                    }
                                }
                            }
                            else if (column is DataGridTemplateColumn templateColumn)
                            {
                                // Attempt to get text from common controls within the template (basic example)
                                // This is complex and might need specific handling based on your templates
                                // For the status column:
                                if (column.Header.ToString() == "Status")
                                {

                                    // Easier: Just bind Status directly if possible, or get property value
                                    var statusProperty = item.GetType().GetProperty("Status");
                                    if (statusProperty != null) cellValue = statusProperty.GetValue(item)?.ToString() ?? "";
                                }
                                // Add more template column handling if needed
                            }

                            // Escape commas and quotes for CSV
                            cellValue = $"\"{cellValue.Replace("\"", "\"\"")}\"";
                            fields.Add(cellValue);
                        }
                        csvContent.AppendLine(string.Join(",", fields));
                    }


                    File.WriteAllText(saveFileDialog.FileName, csvContent.ToString());
                 
                    ShowTemporaryMessage(successMessage, "Data exported successfully.");
                }
                catch (Exception ex)
                {
                  
                    ShowTemporaryMessage(errorMessage, $"Export failed: {ex.Message}", true);
                }
                finally
                {
                    ShowProcessingOverlay(false);
                }
            }
        }


        /// <summary>
        /// Handles tab selection changes to update the 'No Data' panel visibility.
        /// </summary>
        private void TabFeedbackIssues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ensure the event is from the TabControl itself, not internal elements
            if (e.Source is TabControl)
            {
                RefreshViews(); // Refreshing views also updates the NoData panel visibility correctly
            }
        }


        /// <summary>
        /// Placeholder for selection changed in Feedback DataGrid. Can be used later if needed.
        /// </summary>
        private void DgFeedback_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Can be used to enable/disable bulk actions based on selection, etc.
            _selectedFeedbackItem = dgFeedback.SelectedItem as FeedbackItem; // Keep track if needed
        }

        /// <summary>
        /// Placeholder for selection changed in Issues DataGrid. Can be used later if needed.
        /// </summary>
        private void DgIssues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Can be used to enable/disable bulk actions based on selection, etc.
            _selectedIssueItem = dgIssues.SelectedItem as IssueItem; // Keep track if needed
        }


        #endregion

        #region Feedback Actions (Grid Buttons)

        /// <summary>
        /// Handles the 'View Details' button click for a feedback item. Opens the details modal.
        /// </summary>
        private void BtnViewFeedback_Click(object sender, RoutedEventArgs e)
        {
            _selectedFeedbackItem = GetItemFromButton<FeedbackItem>(sender);
            if (_selectedFeedbackItem == null) return;

            // Populate the feedback modal fields
            txtFeedbackDetailsTitle.Text = $"Feedback Details ({_selectedFeedbackItem.SubmissionId})";
            txtFeedbackId.Text = _selectedFeedbackItem.SubmissionId;
            txtFeedbackSubmittedBy.Text = _selectedFeedbackItem.SubmittedBy;
            txtFeedbackSource.Text = _selectedFeedbackItem.FeedbackSource;
            txtFeedbackDate.Text = _selectedFeedbackItem.SubmissionDate.ToString("MM/dd/yyyy");
            ctrlFeedbackStatus.Content = _selectedFeedbackItem.Status; // Bind status for color coding
            txtFeedbackEmail.Text = string.IsNullOrWhiteSpace(_selectedFeedbackItem.ContactEmail) ? "N/A" : _selectedFeedbackItem.ContactEmail;
            txtFeedbackMessage.Text = _selectedFeedbackItem.FeedbackMessage;
            txtFeedbackAdminNotes.Text = _selectedFeedbackItem.AdminNotes ?? ""; // Load existing notes

            // Set the ComboBox to the current status
            foreach (ComboBoxItem item in cmbUpdateFeedbackStatus.Items)
            {
                if (item.Content.ToString() == _selectedFeedbackItem.Status)
                {
                    cmbUpdateFeedbackStatus.SelectedItem = item;
                    break;
                }
            }

            // Show the modal
            feedbackDetailsOverlay.Visibility = Visibility.Visible;
        }


        /// <summary>
        /// Handles the 'Reply' button click directly from the grid row.
        /// Opens the default email client if an email is available.
        /// </summary>
        private void BtnReplyFeedback_Click(object sender, RoutedEventArgs e)
        {
            var feedbackItem = GetItemFromButton<FeedbackItem>(sender);
            if (feedbackItem == null) return;

            if (!string.IsNullOrWhiteSpace(feedbackItem.ContactEmail))
            {
                try
                {
                    // Basic mailto link - consider URL encoding for subject/body if adding defaults
                    Process.Start(new ProcessStartInfo($"mailto:{feedbackItem.ContactEmail}") { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    // ShowToast($"Could not open email client: {ex.Message}", PackIconMaterialKind.Alert); // Use toast if available
                    ShowTemporaryMessage(errorMessage, $"Could not open email client: {ex.Message}", true);
                }
            }
            else
            {
                ShowTemporaryMessage(errorMessage, "No contact email available for this feedback.", true);
            }
        }


        /// <summary>
        /// Handles the 'Delete' button click for a feedback item. Shows the confirmation dialog.
        /// </summary>
        private void BtnDeleteFeedback_Click(object sender, RoutedEventArgs e)
        {
            var feedbackItemToDelete = GetItemFromButton<FeedbackItem>(sender);
            if (feedbackItemToDelete == null) return;

            // Store the item to delete
            _feedbackToDelete = feedbackItemToDelete;
            _issueToDelete = null; // Ensure only one type is stored

            // Configure and show the confirmation dialog
            txtConfirmationMessage.Text = $"Are you sure you want to delete feedback '{feedbackItemToDelete.SubmissionId}'? This action cannot be undone.";
            txtConfirmActionText.Text = "Delete"; // Keep simple
            confirmationOverlay.Visibility = Visibility.Visible;
        }


        #endregion

        #region Issue Actions (Grid Buttons)

        /// <summary>
        /// Handles the 'View Details' button click for an issue item. Opens the details modal.
        /// </summary>
        private void BtnViewIssue_Click(object sender, RoutedEventArgs e)
        {
            _selectedIssueItem = GetItemFromButton<IssueItem>(sender);
            if (_selectedIssueItem == null) return;

            // Populate the issue modal fields
            txtIssueDetailsTitle.Text = $"Issue Details ({_selectedIssueItem.IssueId})";
            txtIssueId.Text = _selectedIssueItem.IssueId;
            txtIssueReportedBy.Text = _selectedIssueItem.ReportedBy;
            txtIssueType.Text = _selectedIssueItem.IssueType;
            txtIssueDate.Text = _selectedIssueItem.SubmissionDate.ToString("MM/dd/yyyy");
            ctrlIssueStatus.Content = _selectedIssueItem.Status; // Bind status for color coding
            txtIssueAssignedTo.Text = string.IsNullOrWhiteSpace(_selectedIssueItem.AssignedTo) ? "Not Assigned" : _selectedIssueItem.AssignedTo;
            txtIssueDescription.Text = _selectedIssueItem.Description;
            txtIssueResolutionNotes.Text = _selectedIssueItem.ResolutionNotes ?? ""; // Load existing notes


            // Set the Status ComboBox
            foreach (ComboBoxItem item in cmbUpdateIssueStatus.Items)
            {
                if (item.Content.ToString() == _selectedIssueItem.Status)
                {
                    cmbUpdateIssueStatus.SelectedItem = item;
                    break;
                }
            }

            // Set the Assignee ComboBox (load actual technicians/staff here)
            // For demo, we check if the current assignee is in the list, otherwise select "Not Assigned"
            bool assigneeFound = false;
            foreach (ComboBoxItem item in cmbAssignIssue.Items)
            {
                // Compare ignoring case and potentially trimming extra info like "(Role)"
                string comboAssignee = item.Content.ToString().Split('(')[0].Trim();
                string currentAssignee = _selectedIssueItem.AssignedTo.Split('(')[0].Trim();

                if (string.Equals(comboAssignee, currentAssignee, StringComparison.OrdinalIgnoreCase))
                {
                    cmbAssignIssue.SelectedItem = item;
                    assigneeFound = true;
                    break;
                }
            }
            if (!assigneeFound)
            {
                cmbAssignIssue.SelectedIndex = 0; // Default to "Not Assigned"
            }

            // Show the modal
            issueDetailsOverlay.Visibility = Visibility.Visible;
        }


        /// <summary>
        /// Handles the 'Assign' button click directly from the grid row.
        /// For simplicity, this will just open the details modal where assignment can be done.
        /// </summary>
        private void BtnAssignIssue_Click(object sender, RoutedEventArgs e)
        {
            // Just open the details modal, assignment happens inside
            BtnViewIssue_Click(sender, e);
        }

        /// <summary>
        /// Handles the 'Delete' button click for an issue item. Shows the confirmation dialog.
        /// </summary>
        private void BtnDeleteIssue_Click(object sender, RoutedEventArgs e)
        {
            var issueToDelete = GetItemFromButton<IssueItem>(sender);
            if (issueToDelete == null) return;

            // Store the item to delete
            _issueToDelete = issueToDelete;
            _feedbackToDelete = null; // Ensure only one type is stored

            // Configure and show the confirmation dialog
            txtConfirmationMessage.Text = $"Are you sure you want to delete issue '{issueToDelete.IssueId}'? This action cannot be undone.";
            txtConfirmActionText.Text = "Delete"; // Keep simple
            confirmationOverlay.Visibility = Visibility.Visible;
        }


        #endregion

        #region Feedback Modal Actions

        /// <summary>
        /// Closes the feedback details modal.
        /// </summary>
        private void BtnCloseFeedbackDetails_Click(object sender, RoutedEventArgs e)
        {
            feedbackDetailsOverlay.Visibility = Visibility.Collapsed;
            _selectedFeedbackItem = null; // Clear selection
        }

        /// <summary>
        /// Updates the status of the currently viewed feedback item.
        /// </summary>
        private async void BtnUpdateFeedbackStatus_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFeedbackItem == null || cmbUpdateFeedbackStatus.SelectedItem == null) return;

            string newStatus = (cmbUpdateFeedbackStatus.SelectedItem as ComboBoxItem)?.Content.ToString();
            string notes = txtFeedbackAdminNotes.Text;

            if (newStatus == _selectedFeedbackItem.Status && notes == _selectedFeedbackItem.AdminNotes)
            {
                //ShowToast("No changes detected.", PackIconMaterialKind.InformationOutline); // Use toast if available
                ShowTemporaryMessage(errorMessage, "No changes detected.", true);
                return; // No change
            }

            ShowProcessingOverlay(true);
            await Task.Delay(200); // Simulate DB update

            try
            {
                // ----- TODO: Add actual database update logic here -----
                // Update _selectedFeedbackItem.Status and _selectedFeedbackItem.AdminNotes in DB
                bool updatedInDb = true; // Assume success for demo
                // ---------------------------------------------------------

                if (updatedInDb)
                {
                    // Update the item in the underlying collection
                    _selectedFeedbackItem.Status = newStatus;
                    _selectedFeedbackItem.AdminNotes = notes;

                    // Refresh the specific item in the view if INotifyPropertyChanged is implemented,
                    // otherwise refresh the whole view. Refreshing the view is simpler.
                    RefreshViews();

                    // Update the status control inside the modal as well
                    ctrlFeedbackStatus.Content = newStatus;

                    //ShowToast("Feedback status and notes updated.", PackIconMaterialKind.Check); // Use toast if available
                    ShowTemporaryMessage(successMessage, "Feedback status and notes updated.");
                    // Optionally close modal after update:
                    // feedbackDetailsOverlay.Visibility = Visibility.Collapsed;
                    // _selectedFeedbackItem = null;
                }
                else
                {
                    //ShowToast("Failed to update feedback in the database.", PackIconMaterialKind.Alert); // Use toast if available
                    ShowTemporaryMessage(errorMessage, "Failed to update feedback in the database.", true);
                }

            }
            catch (Exception ex)
            {
                //ShowToast($"Error updating feedback: {ex.Message}", PackIconMaterialKind.Alert); // Use toast if available
                ShowTemporaryMessage(errorMessage, $"Error updating feedback: {ex.Message}", true);
            }
            finally
            {
                ShowProcessingOverlay(false);
            }
        }


        /// <summary>
        /// Handles the 'Reply' button click within the feedback modal.
        /// </summary>
        private void BtnReplyFeedbackModal_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFeedbackItem == null) return;

            if (!string.IsNullOrWhiteSpace(_selectedFeedbackItem.ContactEmail))
            {
                try
                {
                    Process.Start(new ProcessStartInfo($"mailto:{_selectedFeedbackItem.ContactEmail}") { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    //ShowToast($"Could not open email client: {ex.Message}", PackIconMaterialKind.Alert); // Use toast if available
                    ShowTemporaryMessage(errorMessage, $"Could not open email client: {ex.Message}", true);
                }
            }
            else
            {
                //ShowToast("No contact email available for this feedback.", PackIconMaterialKind.EmailOff); // Use toast if available
                ShowTemporaryMessage(errorMessage, "No contact email available for this feedback.", true);
            }
        }

        /// <summary>
        /// Handles the 'Close' button click in the feedback modal footer.
        /// </summary>
        private void BtnCloseFeedbackModal_Click(object sender, RoutedEventArgs e)
        {
            BtnCloseFeedbackDetails_Click(sender, e); // Same action as the close button in the header
        }

        #endregion

        #region Issue Modal Actions

        /// <summary>
        /// Closes the issue details modal.
        /// </summary>
        private void BtnCloseIssueDetails_Click(object sender, RoutedEventArgs e)
        {
            issueDetailsOverlay.Visibility = Visibility.Collapsed;
            _selectedIssueItem = null; // Clear selection
        }

        /// <summary>
        /// Updates the status of the currently viewed issue item.
        /// </summary>
        private async void BtnUpdateIssueStatus_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedIssueItem == null || cmbUpdateIssueStatus.SelectedItem == null) return;

            string newStatus = (cmbUpdateIssueStatus.SelectedItem as ComboBoxItem)?.Content.ToString();
            string notes = txtIssueResolutionNotes.Text;

            if (newStatus == _selectedIssueItem.Status && notes == _selectedIssueItem.ResolutionNotes)
            {
                //ShowToast("No changes detected in status or notes.", PackIconMaterialKind.InformationOutline); // Use toast if available
                ShowTemporaryMessage(errorMessage, "No changes detected in status or notes.", true);
                return; // No change
            }


            ShowProcessingOverlay(true);
            await Task.Delay(200); // Simulate DB update

            try
            {
                // ----- TODO: Add actual database update logic here -----
                // Update _selectedIssueItem.Status and _selectedIssueItem.ResolutionNotes in DB
                bool updatedInDb = true; // Assume success for demo
                                         // ---------------------------------------------------------

                if (updatedInDb)
                {
                    // Update the item in the underlying collection
                    _selectedIssueItem.Status = newStatus;
                    _selectedIssueItem.ResolutionNotes = notes;

                    // Refresh the view
                    RefreshViews();

                    // Update the status control inside the modal
                    ctrlIssueStatus.Content = newStatus;

                    //ShowToast("Issue status and notes updated.", PackIconMaterialKind.Check); // Use toast if available
                    ShowTemporaryMessage(successMessage, "Issue status and notes updated.");
                    // Optionally close modal after update:
                    // issueDetailsOverlay.Visibility = Visibility.Collapsed;
                    // _selectedIssueItem = null;
                }
                else
                {
                    //ShowToast("Failed to update issue in the database.", PackIconMaterialKind.Alert); // Use toast if available
                    ShowTemporaryMessage(errorMessage, "Failed to update issue in the database.", true);
                }
            }
            catch (Exception ex)
            {
                //ShowToast($"Error updating issue status: {ex.Message}", PackIconMaterialKind.Alert); // Use toast if available
                ShowTemporaryMessage(errorMessage, $"Error updating issue status: {ex.Message}", true);
            }
            finally
            {
                ShowProcessingOverlay(false);
            }
        }

        /// <summary>
        /// Assigns the currently viewed issue to the selected person.
        /// </summary>
        private async void BtnAssignIssueModal_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedIssueItem == null || cmbAssignIssue.SelectedItem == null) return;

            string newAssignee = (cmbAssignIssue.SelectedItem as ComboBoxItem)?.Content.ToString();

            // Check if assignee actually changed
            string currentAssigneeSimple = string.IsNullOrWhiteSpace(_selectedIssueItem.AssignedTo) ? "Not Assigned" : _selectedIssueItem.AssignedTo.Split('(')[0].Trim();
            string newAssigneeSimple = newAssignee.Split('(')[0].Trim();

            if (string.Equals(currentAssigneeSimple, newAssigneeSimple, StringComparison.OrdinalIgnoreCase))

            {
                // ShowToast("Issue is already assigned to this person or status.", PackIconMaterialKind.InformationOutline); // Use toast if available
                ShowTemporaryMessage(errorMessage, "Issue is already assigned to this person or status.", true);
                return; // No change
            }

            ShowProcessingOverlay(true);
            await Task.Delay(200); // Simulate DB update

            try
            {
                // ----- TODO: Add actual database update logic here -----
                // Update _selectedIssueItem.AssignedTo in DB
                bool updatedInDb = true; // Assume success for demo
                // ---------------------------------------------------------

                if (updatedInDb)
                {
                    // Update the item in the underlying collection
                    _selectedIssueItem.AssignedTo = newAssignee;

                    // Refresh the view
                    RefreshViews();

                    // Update the text block inside the modal
                    txtIssueAssignedTo.Text = newAssignee;

                    //ShowToast("Issue assigned successfully.", PackIconMaterialKind.AccountCheck); // Use toast if available
                    ShowTemporaryMessage(successMessage, "Issue assigned successfully.");
                    // Optionally close modal after update:
                    // issueDetailsOverlay.Visibility = Visibility.Collapsed;
                    // _selectedIssueItem = null;
                }
                else
                {
                    //ShowToast("Failed to assign issue in the database.", PackIconMaterialKind.Alert); // Use toast if available
                    ShowTemporaryMessage(errorMessage, "Failed to assign issue in the database.", true);
                }

            }
            catch (Exception ex)
            {
                //ShowToast($"Error assigning issue: {ex.Message}", PackIconMaterialKind.Alert); // Use toast if available
                ShowTemporaryMessage(errorMessage, $"Error assigning issue: {ex.Message}", true);
            }
            finally
            {
                ShowProcessingOverlay(false);
            }
        }


        /// <summary>
        /// Handles the 'Close' button click in the issue modal footer.
        /// </summary>
        private void BtnCloseIssueModal_Click(object sender, RoutedEventArgs e)
        {
            BtnCloseIssueDetails_Click(sender, e); // Same action
        }

        #endregion

        #region Confirmation Dialog Handlers (NEW)

        /// <summary>
        /// Handles the Cancel button click on the confirmation dialog.
        /// </summary>
        private void BtnCancelConfirmation_Click(object sender, RoutedEventArgs e)
        {
            // Hide the overlay and clear pending deletion items
            confirmationOverlay.Visibility = Visibility.Collapsed;
            _feedbackToDelete = null;
            _issueToDelete = null;
        }

        /// <summary>
        /// Handles the Confirm button click on the confirmation dialog. Performs the deletion.
        /// </summary>
        private async void BtnConfirmAction_Click(object sender, RoutedEventArgs e)
        {
            // Hide the overlay first
            confirmationOverlay.Visibility = Visibility.Collapsed;

            if (_feedbackToDelete != null)
            {
                // --- Feedback Deletion Logic ---
                ShowProcessingOverlay(true);
                FeedbackItem itemToDelete = _feedbackToDelete; // Capture for use in messages
                _feedbackToDelete = null; // Clear immediately to prevent double-clicks

                await Task.Delay(200); // Simulate backend operation

                try
                {
                    // ----- TODO: Add actual database deletion logic here -----
                    bool deletedFromDb = true; // Assume success for demo
                    // ---------------------------------------------------------

                    if (deletedFromDb)
                    {
                        feedbackItems.Remove(itemToDelete);
                        RefreshViews(); // Refresh the DataGrid view
                                        //ShowToast($"Feedback '{itemToDelete.SubmissionId}' deleted successfully.", PackIconMaterialKind.Delete); // Use toast if available
                        ShowTemporaryMessage(successMessage, $"Feedback '{itemToDelete.SubmissionId}' deleted successfully.");
                    }
                    else
                    {
                        //ShowToast("Failed to delete feedback from the database.", PackIconMaterialKind.Alert); // Use toast if available
                        ShowTemporaryMessage(errorMessage, "Failed to delete feedback from the database.", true);
                    }
                }
                catch (Exception ex)
                {
                    //ShowToast($"Error deleting feedback: {ex.Message}", PackIconMaterialKind.Alert); // Use toast if available
                    ShowTemporaryMessage(errorMessage, $"Error deleting feedback: {ex.Message}", true);
                }
                finally
                {
                    ShowProcessingOverlay(false);
                }
                // --- End Feedback Deletion ---
            }
            else if (_issueToDelete != null)
            {
                // --- Issue Deletion Logic ---
                ShowProcessingOverlay(true);
                IssueItem itemToDelete = _issueToDelete; // Capture for use in messages
                _issueToDelete = null; // Clear immediately

                await Task.Delay(200); // Simulate backend operation

                try
                {
                    // ----- TODO: Add actual database deletion logic here -----
                    bool deletedFromDb = true; // Assume success for demo
                    // ---------------------------------------------------------

                    if (deletedFromDb)
                    {
                        issueItems.Remove(itemToDelete);
                        RefreshViews(); // Refresh the DataGrid view
                        //ShowToast($"Issue '{itemToDelete.IssueId}' deleted successfully.", PackIconMaterialKind.Delete); // Use toast if available
                        ShowTemporaryMessage(successMessage, $"Issue '{itemToDelete.IssueId}' deleted successfully.");
                    }
                    else
                    {
                        //ShowToast("Failed to delete issue from the database.", PackIconMaterialKind.Alert); // Use toast if available
                        ShowTemporaryMessage(errorMessage, "Failed to delete issue from the database.", true);
                    }
                }
                catch (Exception ex)
                {
                    //ShowToast($"Error deleting issue: {ex.Message}", PackIconMaterialKind.Alert); // Use toast if available
                    ShowTemporaryMessage(errorMessage, $"Error deleting issue: {ex.Message}", true);
                }
                finally
                {
                    ShowProcessingOverlay(false);
                }
                // --- End Issue Deletion ---
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Generic helper to get the DataContext (ViewModel/DataItem) from a button within a DataGrid row.
        /// </summary>
        /// <typeparam name="T">The expected type of the DataContext.</typeparam>
        /// <param name="sender">The Button object that was clicked.</param>
        /// <returns>The DataContext object, or null if not found or not of type T.</returns>
        private T GetItemFromButton<T>(object sender) where T : class
        {
            if (sender is Button button)
            {
                return button.DataContext as T;
            }
            return null;
        }

        /// <summary>
        /// Shows or hides the processing overlay.
        /// </summary>
        /// <param name="show">True to show, False to hide.</param>
        private void ShowProcessingOverlay(bool show)
        {
            processingOverlay.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Displays a temporary success or error message bar at the top of the content area.
        /// </summary>
        /// <param name="messageElement">The Border element (successMessage or errorMessage).</param>
        /// <param name="message">The text to display.</param>
        /// <param name="isError">True if it's an error message, false for success.</param>
        private async void ShowTemporaryMessage(Border messageElement, string message, bool isError = false)
        {
            TextBlock textBlock = isError ? txtErrorMessage : txtSuccessMessage;
            textBlock.Text = message;

            // Ensure the correct message element is used
            Border actualMessageElement = isError ? errorMessage : successMessage;

            // Hide the other message element if it's visible
            if (isError) successMessage.Visibility = Visibility.Collapsed;
            else errorMessage.Visibility = Visibility.Collapsed;


            actualMessageElement.Visibility = Visibility.Visible;

            // Hide the message after a delay
            await Task.Delay(3000); // Show for 3 seconds

            // Check if it's still the same message AND the same element is visible before hiding
            if (textBlock.Text == message && actualMessageElement.Visibility == Visibility.Visible)
            {
                actualMessageElement.Visibility = Visibility.Collapsed;
            }
        }


        // --- Optional: Add Toast Notification Logic if you prefer it over the temp messages ---
        // If you add this, replace calls to ShowTemporaryMessage with ShowToast
        // and ensure the toastNotification XAML exists.
        /*
        private DispatcherTimer _toastTimer;

        private void InitializeToastTimer() // Call this in constructor
        {
            _toastTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            _toastTimer.Tick += (s, e) =>
            {
                _toastTimer.Stop();
                HideToast();
            };
        }

        private void ShowToast(string message, PackIconMaterialKind iconKind)
        {
            if (toastNotification == null) // Check if toast exists in XAML
            {
                // Fallback or log error if toast doesn't exist
                ShowTemporaryMessage(iconKind == PackIconMaterialKind.Alert ? errorMessage : successMessage, message, iconKind == PackIconMaterialKind.Alert);
                return;
            }

            toastMessage.Text = message;
            toastIcon.Kind = iconKind;

            // Set background color based on type (optional, but nice)
            toastNotification.Background = (iconKind == PackIconMaterialKind.Alert || iconKind == PackIconMaterialKind.Cancel || iconKind == PackIconMaterialKind.CloseCircle)
                ? (SolidColorBrush)FindResource("ErrorColor") // Assuming ErrorColor is defined
                : (SolidColorBrush)FindResource("PrimaryAccentColor"); // Default or Success

            toastNotification.Visibility = Visibility.Visible;

            var fadeIn = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            toastNotification.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            if (_toastTimer == null) InitializeToastTimer(); // Ensure timer is initialized
            _toastTimer.Start();
        }

        private void HideToast()
        {
             if (toastNotification == null || toastNotification.Visibility == Visibility.Collapsed) return;

            var fadeOut = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = toastNotification.Opacity, // Fade from current opacity
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            fadeOut.Completed += (s, e) =>
            {
                // Ensure Opacity is set back to 1 for next time IF animation didn't complete fully? No, Vis=Collapsed handles it.
                toastNotification.Visibility = Visibility.Collapsed;
                // Optional: Clear animation to prevent conflicts if shown again quickly
                toastNotification.BeginAnimation(UIElement.OpacityProperty, null);
            };
            toastNotification.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        private void BtnCloseToast_Click(object sender, RoutedEventArgs e)
        {
            if (_toastTimer != null && _toastTimer.IsEnabled) _toastTimer.Stop(); // Stop timer if manually closed
            HideToast();
        }
        */
        // Don't forget to call InitializeToastTimer() in the constructor if using toasts


        #endregion


    } 


} 