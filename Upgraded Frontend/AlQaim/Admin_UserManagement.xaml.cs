using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;        
using MahApps.Metro.IconPacks;        
using System.Windows.Media.Animation; 

namespace AlQaim
{
    public partial class Admin_UserManagement : Page 
    {
        // Observable collection for binding to the DataGrid
        private ObservableCollection<User> _users;

        // Collection view source for filtering
        private CollectionViewSource _usersViewSource;

        // Current user for editing
        private User _currentUser;

        // Flag to determine if we're in edit mode
        private bool _isEditMode;

        // Field to temporarily store the user pending deletion
        private User _userToDelete; // Already exists, good.

        // --- Additions for Toast ---
        private DispatcherTimer _toastTimer;
        // --- End Additions ---

        public Admin_UserManagement()
        {
            InitializeComponent();

            // Initialize collections
            _users = new ObservableCollection<User>();
            _usersViewSource = new CollectionViewSource { Source = _users };

            // Set DataGrid's ItemsSource
            dgUsers.ItemsSource = _usersViewSource.View;

            // --- Additions for Toast ---
            // Initialize the toast timer
            _toastTimer = new DispatcherTimer();
            _toastTimer.Interval = TimeSpan.FromSeconds(3); // Adjust duration as needed
            _toastTimer.Tick += ToastTimer_Tick; // Use a separate method or lambda
            // --- End Additions ---

            // Load initial data
            LoadUsers();
        }

        // --- Additions for Toast ---
        private void ToastTimer_Tick(object sender, EventArgs e)
        {
            _toastTimer.Stop();
            HideToast();
        }
        // --- End Additions ---


        #region Data Operations
        // User model class (assuming it's defined below or elsewhere)
        // public class User { ... } // Keep your User class

        // Load users from the database (mock data for now)
        private void LoadUsers()
        {
            // ... (your existing LoadUsers code)
            _users.Clear();
            _users.Add(new User { UserId = 1, Username = "admin", Email = "admin@alqaim.org", Role = "Admin" });
            _users.Add(new User { UserId = 2, Username = "doctor1", Email = "doctor1@alqaim.org", Role = "Doctor" });
            _users.Add(new User { UserId = 3, Username = "lab_tech", Email = "labtech@alqaim.org", Role = "Lab Technician" });
            _usersViewSource.View.Refresh();
        }

        // Add a new user
        private void AddUser(User user)
        {
            // ... (your existing AddUser code)
            user.UserId = _users.Count > 0 ? _users.Max(u => u.UserId) + 1 : 1;
            _users.Add(user);
            _usersViewSource.View.Refresh();
        }

        // Update an existing user
        private void UpdateUser(User user)
        {
            // ... (your existing UpdateUser code)
            User existingUser = _users.FirstOrDefault(u => u.UserId == user.UserId);
            if (existingUser != null)
            {
                existingUser.Username = user.Username;
                existingUser.Email = user.Email;
                existingUser.Role = user.Role;
                // Note: Don't update password here unless explicitly handled
                _usersViewSource.View.Refresh();
            }
        }

        // Delete a user
        private void DeleteUser(User user)
        {
            // ... (your existing DeleteUser code)
            _users.Remove(user);
            _usersViewSource.View.Refresh();
        }

        #endregion

        #region Event Handlers

        // Search TextBox TextChanged event
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // ... (your existing search code)
            if (_usersViewSource != null && _usersViewSource.View != null)
            {
                _usersViewSource.View.Filter = user =>
                {
                    if (user is User userObj)
                    {
                        string searchText = txtSearch.Text.ToLower();
                        return string.IsNullOrEmpty(searchText) ||
                               userObj.Username.ToLower().Contains(searchText) ||
                               userObj.Email.ToLower().Contains(searchText) ||
                               userObj.Role.ToLower().Contains(searchText);
                    }
                    return false;
                };
            }
        }

        // Refresh Button Click event
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
            // --- Add Toast ---
            ShowToast("User list refreshed", PackIconMaterialKind.Refresh);
            // --- End Add Toast ---
        }

        // Add User Button Click event
        private void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            // ... (your existing add user setup code)
            _isEditMode = false;
            _currentUser = new User();
            modalTitle.Text = "Add New User";
            txtUsername.Text = string.Empty;
            txtEmail.Text = string.Empty;
            txtPassword.Password = string.Empty;
            if (cmbRole.Items.Count > 0) cmbRole.SelectedIndex = 0; // Default selection
            modalOverlay.Visibility = Visibility.Visible;
            txtUsername.Focus(); // Focus the first field
        }

        // Edit Button Click event
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // ... (your existing edit user setup code)
            if (((Button)sender).DataContext is User selectedUser)
            {
                _isEditMode = true;
                _currentUser = selectedUser;
                modalTitle.Text = "Edit User";
                txtUsername.Text = selectedUser.Username;
                txtEmail.Text = selectedUser.Email;
                txtPassword.Password = string.Empty; // Clear password for editing

                for (int i = 0; i < cmbRole.Items.Count; i++)
                {
                    ComboBoxItem item = (ComboBoxItem)cmbRole.Items[i];
                    if (item.Content.ToString() == selectedUser.Role)
                    {
                        cmbRole.SelectedIndex = i;
                        break;
                    }
                }
                modalOverlay.Visibility = Visibility.Visible;
                txtUsername.Focus(); // Focus the first field
            }
        }


        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            // ... (your existing delete confirmation setup)
            if (((Button)sender).DataContext is User selectedUser)
            {
                _userToDelete = selectedUser;
                txtConfirmationMessage.Text = $"Are you sure you want to delete user '{selectedUser.Username}'? This action cannot be undone.";
                confirmationOverlay.Visibility = Visibility.Visible;
            }
        }

        // Cancel button: hide the dialog, do nothing
        private void BtnCancelConfirmation_Click(object sender, RoutedEventArgs e)
        {
            confirmationOverlay.Visibility = Visibility.Collapsed;
            _userToDelete = null;
        }

        // Confirm button: delete the user, then hide the dialog
        private void BtnConfirmAction_Click(object sender, RoutedEventArgs e)
        {
            if (_userToDelete != null)
            {
                DeleteUser(_userToDelete);
                // --- Add Toast ---
                ShowToast("User deleted successfully", PackIconMaterialKind.Delete);
                // --- End Add Toast ---
                _userToDelete = null; // Clear after deletion
            }
            confirmationOverlay.Visibility = Visibility.Collapsed;
        }


        // Save Button Click event
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // --- Modify Validation ---
            // Validate input
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                //MessageBox.Show("Username is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ShowToast("Username is required", PackIconMaterialKind.Alert);
                txtUsername.Focus();
                return;
            }

            // Basic Email validation (you might want a more robust regex)
            if (string.IsNullOrWhiteSpace(txtEmail.Text) || !txtEmail.Text.Contains("@") || !txtEmail.Text.Contains("."))
            {
                //MessageBox.Show("A valid Email is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ShowToast("A valid Email is required", PackIconMaterialKind.Alert);
                txtEmail.Focus();
                return;
            }

            // Password required only when adding a new user OR if the password field is not empty during edit
            if (!_isEditMode && string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                //MessageBox.Show("Password is required for new users.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ShowToast("Password is required for new users", PackIconMaterialKind.Alert);
                txtPassword.Focus();
                return;
            }
            // Optional: Add password complexity rules here if needed

            if (cmbRole.SelectedItem == null)
            {
                //MessageBox.Show("Please select a role.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ShowToast("Please select a role", PackIconMaterialKind.Alert);
                cmbRole.Focus();
                return;
            }
            // --- End Modify Validation ---

            // Update the current user with form values
            _currentUser.Username = txtUsername.Text;
            _currentUser.Email = txtEmail.Text;
            _currentUser.Role = ((ComboBoxItem)cmbRole.SelectedItem).Content.ToString();

            // Handle password (In real app: HASH the password!)
            if (!string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                // If you were hashing:
                // _currentUser.PasswordHash = HashPassword(txtPassword.Password);
                _currentUser.Password = txtPassword.Password; // Storing plain text ONLY for this example
            }
            else if (!_isEditMode)
            {
                // This case should have been caught by validation above, but as a safeguard:
                ShowToast("Password is required for new users", PackIconMaterialKind.Alert);
                txtPassword.Focus();
                return;
            }
            // If editing and password is blank, typically you don't change the existing password


            // Save changes
            if (_isEditMode)
            {
                UpdateUser(_currentUser);
                // --- Add Toast ---
                ShowToast("User updated successfully", PackIconMaterialKind.Check);
                // --- End Add Toast ---
            }
            else
            {
                AddUser(_currentUser);
                // --- Add Toast ---
                ShowToast("User added successfully", PackIconMaterialKind.Check);
                // --- End Add Toast ---
            }

            // Close the modal
            modalOverlay.Visibility = Visibility.Collapsed;
        }

        // Cancel Button Click event
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            modalOverlay.Visibility = Visibility.Collapsed;
        }

        // Close Modal Button Click event
        private void BtnCloseModal_Click(object sender, RoutedEventArgs e)
        {
            modalOverlay.Visibility = Visibility.Collapsed;
        }

        // DataGrid Selection Changed event
        private void DgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Optional: Handle selection changes if needed
        }

        // --- Additions for Toast ---
        private void ShowToast(string message, PackIconMaterialKind iconKind)
        {
            toastMessage.Text = message;
            toastIcon.Kind = iconKind;

            // Set background color based on icon (optional, for visual distinction)
            if (iconKind == PackIconMaterialKind.Alert || iconKind == PackIconMaterialKind.AlertCircleOutline)
            {
                toastNotification.Background = FindResource("RejectedColor") as System.Windows.Media.SolidColorBrush; // Use your error color
            }
            else if (iconKind == PackIconMaterialKind.Delete)
            {
                toastNotification.Background = FindResource("PrimaryAccentColor") as System.Windows.Media.SolidColorBrush; // Use a warning/info color
            }
            else
            {
                toastNotification.Background = FindResource("PrimaryAccentColor") as System.Windows.Media.SolidColorBrush; // Default success color
            }


            toastNotification.Visibility = Visibility.Visible;

            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            toastNotification.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            _toastTimer.Start();
        }

        private void HideToast()
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            fadeOut.Completed += (s, e) => toastNotification.Visibility = Visibility.Collapsed;
            toastNotification.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        private void BtnCloseToast_Click(object sender, RoutedEventArgs e)
        {
            _toastTimer.Stop(); // Stop timer if manually closed
            HideToast();
        }
        // --- End Additions ---


        // Optional: Add cleanup if needed when the page is unloaded
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_toastTimer != null && _toastTimer.IsEnabled)
            {
                _toastTimer.Stop();
            }
        }


        #endregion
    }

    // User model class (Ensure this exists)
    public class User // You might have this in a separate file
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } // WARNING: Store HASHED passwords in real apps!
        public string Role { get; set; }

       
    }


    
   public class IsNullOrEmptyConverter : IValueConverter
   {
       public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
       {
           return string.IsNullOrEmpty(value as string);
       }

       public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
       {
           throw new NotImplementedException();
       }
   }
   
 
}