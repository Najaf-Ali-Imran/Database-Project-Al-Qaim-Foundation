using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Configuration;


namespace AlQaim
{
    public partial class Admin_MainWindow : Window
    {
        public Admin_MainWindow()
        {
            InitializeComponent();
            this.MouseDown += Window_MouseDown;
        }

        private void SetActiveButton(Button clickedButton)
        {
            // Loop through each child in the MainMenuPanel
            foreach (var child in MainMenuPanel.Children)
            {
                if (child is Button btn)
                {
                    // Reset the style to the default side menu style
                    btn.Style = (Style)FindResource("SideMenuButton");
                }
            }
            if (clickedButton != null)
            {
                clickedButton.Style = (Style)FindResource("SideMenuButtonActive");
            }
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb.Text == "Search...")
                tb.Text = "";
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (string.IsNullOrWhiteSpace(tb.Text))
                tb.Text = "Search...";
        }

        private void LanguageToggle_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(null);
            MainFrame.Navigate(new AlQaim.Settings());
        }

        private void Help(object sender, RoutedEventArgs e)
        {
            SetActiveButton(null);
            MainFrame.Navigate(new AlQaim.Settings());
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SetActiveButton(sender as Button);
            MainFrame.Navigate(new AlQaim.Admin_UserManagement());
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            SetActiveButton(sender as Button);
            MainFrame.Navigate(new AlQaim.Admin_InventoryManagement());

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            SetActiveButton(sender as Button);
            MainFrame.Navigate(new AlQaim.Admin_AppointmentManagement());
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            SetActiveButton(sender as Button);
            MainFrame.Navigate(new AlQaim.Admin_PatientManagement());
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            SetActiveButton(sender as Button);
            MainFrame.Navigate(new AlQaim.Admin_GenerateReports());
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            SetActiveButton(sender as Button);
            MainFrame.Navigate(new AlQaim.Admin_Requests());
        }

        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(sender as Button);
            MainFrame.Navigate(new AlQaim.Admin_Dashboard());

        }

        private void Button_click_7(object sender, RoutedEventArgs e)
        {
            SetActiveButton(sender as Button);
            MainFrame.Navigate(new AlQaim.Admin_FeedbackAndIssues());
        }
    }
}