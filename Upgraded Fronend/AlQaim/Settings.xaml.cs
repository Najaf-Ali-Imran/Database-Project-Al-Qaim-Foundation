using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace AlQaim
{

    public partial class Settings : Page
    {
        private string _userEmail;
        private string _userName;
        private string _userRole;
        private string _themePreference;

        public Settings()
        {
            InitializeComponent();
            LoadUserData();
            SetTheme(_themePreference);
        }

       
        private void LoadUserData()
        {
            // In a real application, you would fetch this data from your service or database
            _userName = "Admin";
            _userEmail = "Admin@AlQaim.org";
            _userRole = "Administrator";
            _themePreference = "Light";

            // Update UI
            txtUsername.Text = _userName;
            txtEmail.Text = _userEmail;
            txtRole.Text = _userRole;
            cboTheme.SelectedIndex = _themePreference == "Dark" ? 1 : 0;

            // Set avatar initials based on username
            if (!string.IsNullOrEmpty(_userName) && _userName.Length > 0)
            {
                string initials = _userName.Substring(0, 1);
                if (_userName.Contains(" "))
                {
                    int spaceIndex = _userName.IndexOf(" ");
                    if (spaceIndex + 1 < _userName.Length)
                    {
                        initials += _userName.Substring(spaceIndex + 1, 1);
                    }
                }
                AvatarInitials.Text = initials.ToUpper();
            }
        }

        /// Handles the password change functionality
        /// </summary>
        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string currentPassword = txtCurrentPassword.Password;
            string newPassword = txtNewPassword.Password;
            string confirmPassword = txtConfirmPassword.Password;

            // Basic validation
            if (string.IsNullOrEmpty(currentPassword))
            {
                MessageBox.Show("Please enter your current password.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("Please enter a new password.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("New password and confirmation do not match.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // In a real application, you would validate the current password against the stored password
            // and then update the password in your database or authentication service

            // For this example, we'll just show a success message
            MessageBox.Show("Password changed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // Clear password fields
            txtCurrentPassword.Clear();
            txtNewPassword.Clear();
            txtConfirmPassword.Clear();
        }

        /// <summary>
        /// Saves the user settings
        /// </summary>
        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            // Update email from the UI
            _userEmail = txtEmail.Text;

            // Save notification preferences
            bool testCompletionAlerts = chkTestCompletionAlerts.IsChecked ?? false;

            bool appointmentReminders = chkAppointmentReminders.IsChecked ?? false;
            bool autoSaveReports = chkAutoSaveReports.IsChecked ?? false;



            // In a real application, you would save these settings to your user settings service or database

            MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Handles theme selection changes
        /// </summary>
        private void Theme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboTheme.SelectedItem != null)
            {
                string selectedTheme = ((ComboBoxItem)cboTheme.SelectedItem).Content.ToString();
                _themePreference = selectedTheme;
                SetTheme(selectedTheme);
            }
        }

        /// <summary>
        /// Sets the application theme
        /// </summary>
        private void SetTheme(string theme)
        {
      

            if (theme == "Dark")
            {
                
            }
            else
            {
                // Apply light theme (default)
            }
        }

        /// <summary>
        /// Opens hyperlinks in the default browser
        /// </summary>
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            // Open URL in default browser
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true;
        }

        /// <summary>
        /// Handles the Download User Manual button click
        /// </summary>
        private void DownloadManual_Click(object sender, RoutedEventArgs e)
        {
            // In a real application, you would download or open the user manual PDF
            // For this example, we'll just show a message
            MessageBox.Show("The user manual would be downloaded or opened here.", "User Manual", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}