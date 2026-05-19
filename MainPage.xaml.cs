using AppFitnessTrackerReal.db;
using AppFitnessTrackerReal.Models;

namespace AppFitnessTrackerReal
{
    public partial class MainPage : ContentPage
    {
        private bool _loading = false;

        public MainPage()
        {
            InitializeComponent();
        }

        private async Task LoginUserAsync()
        {
            if (_loading) return;
            _loading = true;
            // regex for email validation [a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}
            try
            {
                if (string.IsNullOrWhiteSpace(loginStr.Text) ||
                    string.IsNullOrWhiteSpace(PasswordStr.Text) ||
                    string.IsNullOrWhiteSpace(EmailStr.Text))
                {
                    await DisplayAlertAsync("Error", "All fields are required.", "OK");
                    return;
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(EmailStr.Text, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                {
                    await DisplayAlertAsync("Error", "Invalid email format.", "OK");
                    return;
                }
                User newUser = new User
                {
                    Name = loginStr.Text,
                    Password = PasswordStr.Text,
                    Email = EmailStr.Text,
                };

                await Db.AddUser(newUser);
                await Shell.Current.GoToAsync("DashBord");
            }
            finally
            {
                _loading = false;
            }
        }

        private async void OnLoginClicked(object? sender, EventArgs e)
        {
            await LoginUserAsync();
        }
    }
}
