using System;
using System.Configuration;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MySql.Data.MySqlClient;

namespace AlQaim
{
    public partial class LoginWindow : Window
    {
        private bool isEnglish = true;

        public LoginWindow()
        {
            InitializeComponent();
            ApplyLanguageResources("en");
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void LanguageToggle_Click(object sender, RoutedEventArgs e)
        {
            isEnglish = !isEnglish;
            string languageCode = isEnglish ? "en" : "ur";
            ApplyLanguageResources(languageCode);
            LanguageToggle.IsChecked = !isEnglish;
        }

        private void ApplyLanguageResources(string languageCode)
        {
            Uri resourceUri = new Uri($"/AlQaim;component/StringResources.{languageCode}.xaml", UriKind.Relative);
            ResourceDictionary currentDict = null;
            foreach (ResourceDictionary dict in Application.Current.Resources.MergedDictionaries)
            {
                if (dict.Source != null && (dict.Source.OriginalString.Contains("StringResources.en.xaml") ||
                                             dict.Source.OriginalString.Contains("StringResources.ur.xaml")))
                {
                    currentDict = dict;
                    break;
                }
            }
            if (currentDict != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(currentDict);
            }
            ResourceDictionary newDict = new ResourceDictionary { Source = resourceUri };
            Application.Current.Resources.MergedDictionaries.Add(newDict);

            CultureInfo culture = new CultureInfo(languageCode == "ur" ? "ur-PK" : "en-US");
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;
            bool isAdmin = AdminLoginToggle.IsChecked ?? false;
            bool rememberMe = RememberMeCheckBox.IsChecked ?? false;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool authenticated = AuthenticateUser(username, password, isAdmin);
            if (authenticated)
            {
                if (isAdmin)
                {
                    
                    Admin_MainWindow adminWindow = new Admin_MainWindow();
                    adminWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Lab Technician login successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private bool AuthenticateUser(string username, string password, bool isAdmin)
        {
            bool isAuthenticated = false;
            string connectionString = ConfigurationManager.ConnectionStrings["alqaimDB"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM User WHERE username = @username AND password = @password";
                    query += isAdmin ? " AND role = 'Admin'" : " AND role = 'LabTechnician'";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);
                    int result = Convert.ToInt32(cmd.ExecuteScalar());
                    isAuthenticated = (result > 0);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error connecting to the database: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return isAuthenticated;
        }

        private void AdminLoginToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (UserLoginToggle != null)
                UserLoginToggle.IsChecked = false;
        }

        private void UserLoginToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (AdminLoginToggle != null)
                AdminLoginToggle.IsChecked = false;
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Please contact your system administrator to reset your password.",
                            "Forgot Password", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RequestAccess_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Please contact your system administrator to request access.",
                            "Request Access", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SupportButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("For support, please contact:\nEmail: support@alqaimfoundation.org\nPhone: +92-XXX-XXXXXXX",
                            "Contact Support", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // This event handler is intentionally left blank.
        }
    }
}
