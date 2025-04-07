using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using System.Windows.Threading;       
using MahApps.Metro.IconPacks;         
using System.Windows.Media.Animation;  
using System.Windows.Media;           

namespace AlQaim
{
    // InventoryItem class definition (ensure this is accessible, e.g., defined here or in a separate Models file)
    public class InventoryItem
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public double Quantity { get; set; }
        public string Unit { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string SupplierName { get; set; }
        public string ItemType { get; set; }
    }

    /// <summary>
    /// Interaction logic for Admin_InventoryManagement.xaml
    /// </summary>
    public partial class Admin_InventoryManagement : Page // Consider IDisposable for cleanup if needed
    {
        private ObservableCollection<InventoryItem> inventoryItems;
        private CollectionViewSource inventoryViewSource;
        private InventoryItem selectedItem; // Holds the item selected in the grid or being edited
        private bool isEditing = false;
        private InventoryItem _currentInventoryItemToDelete; // Holds item pending delete confirmation

        // --- Additions for Toast ---
        private DispatcherTimer _toastTimer;
        // --- End Additions ---

        public Admin_InventoryManagement()
        {
            InitializeComponent();

            // --- Additions for Toast ---
            _toastTimer = new DispatcherTimer();
            _toastTimer.Interval = TimeSpan.FromSeconds(3); // Adjust duration as needed
            _toastTimer.Tick += ToastTimer_Tick;
            // --- End Additions ---

            LoadInventoryData(); // Load data after timer init
            SetupViewSource();   // Setup view source after data load
        }

        // --- Additions for Toast ---
        private void ToastTimer_Tick(object sender, EventArgs e)
        {
            _toastTimer.Stop();
            HideToast();
        }
        // --- End Additions ---

        #region UI Event Handlers

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter(); // Centralize filtering
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadInventoryData(); // Reloads data
            txtSearch.Clear(); // Clear search text
            if (inventoryViewSource?.View != null)
            {
                inventoryViewSource.View.Filter = null; // Clear any active filter
            }
            // --- Add Toast ---
            ShowToast("Inventory data refreshed", PackIconMaterialKind.Refresh, useInfoColor: true);
            // --- End Add Toast ---
        }

        private void BtnAddInventory_Click(object sender, RoutedEventArgs e)
        {
            ShowAddEditModal(false); // Show modal for adding
        }

        private void DgInventory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Update selectedItem based on DataGrid selection
            selectedItem = dgInventory.SelectedItem as InventoryItem;
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            InventoryItem itemToEdit = null;
            var button = sender as Button;

            // Prioritize item from the button's context (clicked row)
            if (button != null)
            {
                itemToEdit = button.DataContext as InventoryItem;
            }

            // Fallback to the currently selected item in the grid if button context fails
            if (itemToEdit == null)
            {
                itemToEdit = dgInventory.SelectedItem as InventoryItem;
            }

            if (itemToEdit != null)
            {
                selectedItem = itemToEdit; // Ensure selectedItem is set for the modal
                ShowAddEditModal(true); // Show modal for editing
            }
            else
            {
                // Inform user no item is selected
                ShowToast("Please select an item to edit.", PackIconMaterialKind.AlertCircleOutline);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            InventoryItem itemToDelete = null;
            var button = sender as Button;

            // Prioritize item from the button's context (clicked row)
            if (button != null)
            {
                itemToDelete = button.DataContext as InventoryItem;
            }

            // Fallback to the currently selected item in the grid if button context fails
            if (itemToDelete == null)
            {
                itemToDelete = dgInventory.SelectedItem as InventoryItem;
            }

            if (itemToDelete != null)
            {
                // Store the item for confirmation action
                _currentInventoryItemToDelete = itemToDelete;
                // Update confirmation message
                txtConfirmationMessage.Text = $"Are you sure you want to delete '{itemToDelete.ItemName}' (ID: {itemToDelete.ItemID})?";
                // Show the confirmation overlay
                confirmationOverlay.Visibility = Visibility.Visible;
            }
            else
            {
                // Inform user no item is selected
                ShowToast("Please select an item to delete.", PackIconMaterialKind.AlertCircleOutline);
            }
        }

        // Handles the "Cancel" button on the confirmation dialog
        private void BtnCancelConfirmation_Click(object sender, RoutedEventArgs e)
        {
            confirmationOverlay.Visibility = Visibility.Collapsed;
            _currentInventoryItemToDelete = null; // Clear the item pending deletion
        }

        // Handles the "Confirm" (e.g., "Delete") button on the confirmation dialog
        private void BtnConfirmAction_Click(object sender, RoutedEventArgs e)
        {
            if (_currentInventoryItemToDelete != null)
            {
                // Call the actual deletion logic (which now includes toast messages)
                DeleteInventoryItem(_currentInventoryItemToDelete);
                _currentInventoryItemToDelete = null; // Clear after attempting deletion
            }
            // Always hide the overlay after action attempt
            confirmationOverlay.Visibility = Visibility.Collapsed;
        }

        // Handles the close button (X) on the Add/Edit modal
        private void BtnCloseModal_Click(object sender, RoutedEventArgs e)
        {
            CloseModal();
        }

        // Handles the "Cancel" button on the Add/Edit modal
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseModal();
        }

        // Handles the "Save" button on the Add/Edit modal
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validate form input first
            if (ValidateForm())
            {
                // If valid, attempt to save (method now handles success/error toasts)
                SaveInventoryItem();
                // Note: CloseModal is now called inside SaveInventoryItem upon SUCCESS only
            }
            // If validation fails, toast is shown inside ValidateForm, do nothing further here.
        }

        // Handles the "View Usage" button click
        private void btnInventoryUsage_Click(object sender, RoutedEventArgs e)
        {
            NavigationService nav = NavigationService.GetNavigationService(this);
            if (nav != null)
            {
                // Navigate to the Inventory Usage page
                nav.Navigate(new Admin_InventoryUsage());
            }
            else
            {
                // Show error if navigation is not possible
                ShowToast("Navigation service not available.", PackIconMaterialKind.AlertCircleOutline);
            }
        }

        // Optional: Cleanup on page unload
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_toastTimer != null && _toastTimer.IsEnabled)
            {
                _toastTimer.Stop();
            }
        }

        #endregion

        #region Helper Methods

        // Loads inventory data (simulated from DB) into the ObservableCollection
        private void LoadInventoryData()
        {
            try
            {
                // Replace with your actual database retrieval logic
                var itemsFromDb = GetInventoryItemsFromDatabase();
                inventoryItems = new ObservableCollection<InventoryItem>(itemsFromDb);

                // Ensure the view source is updated or set if it exists/doesn't exist
                if (inventoryViewSource != null)
                {
                    inventoryViewSource.Source = inventoryItems;
                    inventoryViewSource.View?.Refresh(); // Refresh view if source changes
                }
                else
                {
                    SetupViewSource(); // Setup if called before view source exists
                }
            }
            catch (Exception ex)
            {
                // Show error toast if data loading fails
                ShowToast($"Error loading inventory: {ex.Message}", PackIconMaterialKind.AlertCircleOutline);
                // Ensure the collection is at least initialized to prevent null errors
                inventoryItems = new ObservableCollection<InventoryItem>();
                if (inventoryViewSource != null)
                {
                    inventoryViewSource.Source = inventoryItems; // Update source even if empty
                }
            }
        }

        // Sets up the CollectionViewSource for filtering/sorting
        private void SetupViewSource()
        {
            // Ensure inventoryItems is initialized before creating the view source
            if (inventoryItems == null)
            {
                inventoryItems = new ObservableCollection<InventoryItem>();
            }
            inventoryViewSource = new CollectionViewSource { Source = inventoryItems };
            // Bind the DataGrid to the View of the CollectionViewSource
            dgInventory.ItemsSource = inventoryViewSource.View;
        }

        // Applies text filter to the inventory view
        private void ApplyFilter()
        {
            if (inventoryViewSource?.View == null) return; // Safety check

            // Use the Filter property of ICollectionView for efficient filtering
            inventoryViewSource.View.Filter = item =>
            {
                var inventoryItem = item as InventoryItem;
                if (inventoryItem == null) return false; // Should not happen with typed collection

                string searchText = txtSearch.Text?.ToLower() ?? string.Empty; // Handle null search text
                if (string.IsNullOrWhiteSpace(searchText)) return true; // No filter applied

                // Check multiple fields for the search text
                return (inventoryItem.ItemName?.ToLower().Contains(searchText) ?? false) ||
                       (inventoryItem.SupplierName?.ToLower().Contains(searchText) ?? false) ||
                       (inventoryItem.ItemType?.ToLower().Contains(searchText) ?? false) ||
                       (inventoryItem.ItemID.ToString().Contains(searchText)); // Include ID in search
            };
            // Refresh is usually not needed when setting ICollectionView.Filter
        }

        // Shows the Add/Edit modal dialog
        private void ShowAddEditModal(bool isEdit)
        {
            isEditing = isEdit; // Set the mode (add or edit)
            modalTitle.Text = isEdit ? "Edit Inventory Item" : "Add New Inventory Item";

            // Disable Item ID field when editing, enable when adding
            txtItemID.IsEnabled = !isEdit;

            if (isEdit && selectedItem != null)
            {
                // Populate the form fields with data from the selected item
                txtItemID.Text = selectedItem.ItemID.ToString();
                txtItemName.Text = selectedItem.ItemName;
                txtQuantity.Text = selectedItem.Quantity.ToString();
                // Set ComboBox selection based on string value (safer)
                cmbUnit.SelectedItem = cmbUnit.Items.OfType<ComboBoxItem>().FirstOrDefault(cbi => cbi.Content.ToString() == selectedItem.Unit);
                dtpExpiryDate.SelectedDate = selectedItem.ExpiryDate; // DatePicker handles DateTime
                txtSupplierName.Text = selectedItem.SupplierName;
                cmbItemType.SelectedItem = cmbItemType.Items.OfType<ComboBoxItem>().FirstOrDefault(cbi => cbi.Content.ToString() == selectedItem.ItemType);
            }
            else // Adding a new item
            {
                ClearForm(); // Ensure form is empty
                // Optional: Pre-fill Item ID if needed, e.g., GetNextAvailableID();
            }

            // Make the modal visible
            modalOverlay.Visibility = Visibility.Visible;
            // Set focus to the first editable field
            txtItemName.Focus();
        }

        // Closes the Add/Edit modal dialog
        private void CloseModal()
        {
            modalOverlay.Visibility = Visibility.Collapsed;
            ClearForm(); // Clear form fields
            selectedItem = null; // Reset selected item state
            isEditing = false;   // Reset edit mode
        }

        // Clears all input fields in the Add/Edit modal
        private void ClearForm()
        {
            txtItemID.Clear();
            txtItemName.Clear();
            txtQuantity.Clear();
            cmbUnit.SelectedIndex = -1; // Reset ComboBox
            dtpExpiryDate.SelectedDate = null; // Clear DatePicker
            txtSupplierName.Clear();
            cmbItemType.SelectedIndex = -1; // Reset ComboBox
        }

        // Validates the input fields in the Add/Edit modal
        private bool ValidateForm()
        {
            // Item ID validation
            if (string.IsNullOrWhiteSpace(txtItemID.Text))
            {
                // Only strictly required if manually entered (depends on workflow)
                // If auto-generated, this check might be different or removed.
                ShowToast("Item ID is required.", PackIconMaterialKind.AlertCircleOutline);
                txtItemID.Focus();
                return false;
            }
            if (!int.TryParse(txtItemID.Text, out int itemId) || itemId <= 0)
            {
                ShowToast("Please enter a valid positive Item ID.", PackIconMaterialKind.AlertCircleOutline);
                txtItemID.Focus();
                return false;
            }
            // Check for duplicate ID only when ADDING
            if (!isEditing && inventoryItems.Any(i => i.ItemID == itemId))
            {
                ShowToast($"Item ID '{itemId}' already exists. Please use a unique ID.", PackIconMaterialKind.AlertCircleOutline);
                txtItemID.Focus();
                return false;
            }

            // Item Name validation
            if (string.IsNullOrWhiteSpace(txtItemName.Text))
            {
                ShowToast("Item Name is required.", PackIconMaterialKind.AlertCircleOutline);
                txtItemName.Focus();
                return false;
            }

            // Quantity validation
            if (string.IsNullOrWhiteSpace(txtQuantity.Text) || !double.TryParse(txtQuantity.Text, out double quantity) || quantity < 0)
            {
                ShowToast("Please enter a valid non-negative Quantity.", PackIconMaterialKind.AlertCircleOutline);
                txtQuantity.Focus();
                return false;
            }

            // Unit validation
            if (cmbUnit.SelectedItem == null)
            {
                ShowToast("Please select a Unit.", PackIconMaterialKind.AlertCircleOutline);
                cmbUnit.Focus();
                return false;
            }

            // Expiry Date validation (optional: prevent past dates)
            if (dtpExpiryDate.SelectedDate.HasValue && dtpExpiryDate.SelectedDate.Value.Date < DateTime.Today)
            {
                ShowToast("Expiry Date cannot be in the past.", PackIconMaterialKind.CalendarAlert);
                dtpExpiryDate.Focus();
                return false;
            }
            // Optional: Require expiry date?
            // if (!dtpExpiryDate.SelectedDate.HasValue) { /* ... show toast ... */ return false; }


            // Item Type validation
            if (cmbItemType.SelectedItem == null)
            {
                ShowToast("Please select an Item Type.", PackIconMaterialKind.AlertCircleOutline);
                cmbItemType.Focus();
                return false;
            }

            // Add any other required field validations (e.g., Supplier Name)
            // if (string.IsNullOrWhiteSpace(txtSupplierName.Text)) { /* ... show toast ... */ return false; }

            return true; // All validations passed
        }

        // Saves the item (either updates existing or adds new)
        private void SaveInventoryItem()
        {
            try
            {
                // Safely parse values from the form
                int itemId = int.Parse(txtItemID.Text);
                double quantity = double.Parse(txtQuantity.Text);
                // Get string content from selected ComboBoxItems
                string unit = (cmbUnit.SelectedItem as ComboBoxItem)?.Content?.ToString();
                DateTime expiryDate = dtpExpiryDate.SelectedDate ?? DateTime.MaxValue; // Use a sensible default like MaxValue if null is allowed, otherwise validate earlier
                string supplierName = txtSupplierName.Text; // Allow empty? Validate if required
                string itemType = (cmbItemType.SelectedItem as ComboBoxItem)?.Content?.ToString();

                if (isEditing && selectedItem != null) // Handle EDIT case
                {
                    // Find the item in the current collection to update
                    var itemToUpdate = inventoryItems.FirstOrDefault(i => i.ItemID == selectedItem.ItemID);
                    if (itemToUpdate == null)
                    {
                        ShowToast("Item to update not found. It might have been deleted. Please refresh.", PackIconMaterialKind.AlertCircleOutline);
                        return; // Exit save operation
                    }

                    // Update properties of the found item
                    itemToUpdate.ItemName = txtItemName.Text;
                    itemToUpdate.Quantity = quantity;
                    itemToUpdate.Unit = unit;
                    itemToUpdate.ExpiryDate = expiryDate;
                    itemToUpdate.SupplierName = supplierName;
                    itemToUpdate.ItemType = itemType;

                    UpdateInventoryItemInDatabase(itemToUpdate); // Persist changes to DB (Simulated)
                    ShowToast($"Item '{itemToUpdate.ItemName}' updated successfully.", PackIconMaterialKind.ContentSaveEdit);
                    inventoryViewSource.View.Refresh(); // Refresh view to show changes
                    CloseModal(); // Close modal on successful update
                }
                else // Handle ADD case
                {
                    // ID uniqueness was already checked in ValidateForm
                    var newItem = new InventoryItem
                    {
                        ItemID = itemId,
                        ItemName = txtItemName.Text,
                        Quantity = quantity,
                        Unit = unit,
                        ExpiryDate = expiryDate,
                        SupplierName = supplierName,
                        ItemType = itemType
                    };

                    AddInventoryItemToDatabase(newItem); // Persist new item to DB (Simulated)
                    inventoryItems.Add(newItem); // Add to the ObservableCollection (UI updates automatically)
                    ShowToast($"Item '{newItem.ItemName}' added successfully.", PackIconMaterialKind.PlusBox);
                    // No need to refresh view source explicitly for adds with ObservableCollection
                    CloseModal(); // Close modal on successful add
                }
            }
            catch (FormatException ex) // Catch errors during parsing (e.g., invalid number)
            {
                ShowToast($"Invalid input format: {ex.Message}", PackIconMaterialKind.AlertCircleOutline);
                // Keep modal open for correction
            }
            catch (Exception ex) // Catch other potential errors during save
            {
                ShowToast($"Error saving inventory item: {ex.Message}", PackIconMaterialKind.AlertCircleOutline);
                // Keep modal open for correction or cancellation
            }
        }

        // Deletes the specified inventory item
        private void DeleteInventoryItem(InventoryItem item)
        {
            // Simulate database deletion first (or call actual DB method)
            try
            {
                // --- TODO: Implement actual database deletion logic here ---
                // DeleteInventoryItemFromDatabase(item); // Example call

                // If DB deletion is successful (or simulated success), remove from UI collection
                if (inventoryItems.Contains(item))
                {
                    inventoryItems.Remove(item);
                    // ObservableCollection updates UI automatically, Refresh usually not needed
                    ShowToast($"Item '{item.ItemName}' deleted successfully.", PackIconMaterialKind.Delete);
                }
                else
                {
                    // Item wasn't found in the list, maybe already deleted?
                    ShowToast($"Item '{item.ItemName}' not found in the current list.", PackIconMaterialKind.AlertCircleOutline);
                }
            }
            catch (Exception ex) // Catch errors during DB deletion or UI removal
            {
                ShowToast($"Error deleting item '{item.ItemName}': {ex.Message}", PackIconMaterialKind.AlertCircleOutline);
            }
        }

        #endregion

        #region Database Interaction Methods (Placeholders)

        // Simulates fetching data from a database
        private List<InventoryItem> GetInventoryItemsFromDatabase()
        {
            Console.WriteLine("Fetching inventory items from DB (Simulated)...");
            // Example data
            return new List<InventoryItem>
            {
                new InventoryItem { ItemID = 1, ItemName = "Gloves - Medium", Quantity = 95, Unit = "boxes", ExpiryDate = new DateTime(2025, 12, 31), SupplierName = "Medical Supplies Inc.", ItemType = "Disposable" },
                new InventoryItem { ItemID = 2, ItemName = "Paracetamol 500mg", Quantity = 48, Unit = "bottles", ExpiryDate = new DateTime(2026, 08, 15), SupplierName = "Pharma Co.", ItemType = "Medicine" },
                new InventoryItem { ItemID = 3, ItemName = "Syringes 5ml", Quantity = 240, Unit = "pieces", ExpiryDate = new DateTime(2027, 02, 28), SupplierName = "Global MedTech", ItemType = "Consumable" },
                new InventoryItem { ItemID = 4, ItemName = "Ethanol 99%", Quantity = 8.5, Unit = "liters", ExpiryDate = new DateTime(2026, 11, 30), SupplierName = "Chem Supplies Ltd.", ItemType = "Reagent" },
                new InventoryItem { ItemID = 5, ItemName = "Microscope Slides", Quantity = 450, Unit = "pieces", ExpiryDate = DateTime.MaxValue, SupplierName = "Lab Essentials", ItemType = "Equipment" } // No expiry
            };
        }

        // Simulates adding an item to the database
        private void AddInventoryItemToDatabase(InventoryItem item)
        {
            // TODO: Replace with actual database insertion logic
            Console.WriteLine($"Adding item {item.ItemID} - {item.ItemName} to DB (Simulated)");
            // Simulate potential DB failure? For now, assume success.
        }

        // Simulates updating an item in the database
        private void UpdateInventoryItemInDatabase(InventoryItem item)
        {
            // TODO: Replace with actual database update logic
            Console.WriteLine($"Updating item {item.ItemID} - {item.ItemName} in DB (Simulated)");
            // Simulate potential DB failure? For now, assume success.
        }

        // Note: The actual DB deletion call should happen within DeleteInventoryItem method above
        // This placeholder is just for conceptual separation if needed.
        // private void DeleteInventoryItemFromDatabase(InventoryItem item) { /* DB Call */ }

        #endregion

        #region Toast Notification Methods

        // Shows a toast notification message
        private void ShowToast(string message, PackIconMaterialKind iconKind, bool useInfoColor = false)
        {
            // Ensure execution on the UI thread
            Dispatcher.Invoke(() => {
                toastMessage.Text = message;
                toastIcon.Kind = iconKind;

                // Determine background color based on message type/icon
                SolidColorBrush backgroundBrush;
                if (iconKind == PackIconMaterialKind.AlertCircleOutline || iconKind == PackIconMaterialKind.Alert || iconKind == PackIconMaterialKind.CalendarAlert)
                {
                    // Error/Warning
                    backgroundBrush = FindResource("DangerColor") as SolidColorBrush ?? new SolidColorBrush(Colors.Red);
                }
                else if (useInfoColor || iconKind == PackIconMaterialKind.Refresh || iconKind == PackIconMaterialKind.Information)
                {
                    // Informational
                    backgroundBrush = FindResource("InfoColor") as SolidColorBrush ?? new SolidColorBrush(Colors.DodgerBlue);
                }
                else // Default to Success
                {
                    backgroundBrush = FindResource("PrimaryAccentColor") as SolidColorBrush ?? new SolidColorBrush(Colors.Green);
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
            Dispatcher.Invoke(() => {
                var fadeOut = new DoubleAnimation { From = toastNotification.Opacity, To = 0, Duration = TimeSpan.FromMilliseconds(300) };
                // When animation completes, set visibility to Collapsed
                fadeOut.Completed += (s, e) => {
                    // Check if it's still faded out (another toast might have appeared quickly)
                    if (toastNotification.Opacity < 0.1)
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