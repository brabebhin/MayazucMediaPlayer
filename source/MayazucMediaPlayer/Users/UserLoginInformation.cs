using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls;
using MayazucMediaPlayer.Common;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.UserInput;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace MayazucMediaPlayer.Users
{
    public class UserLoginInformation : ObservableObject
    {
        public string ServiceLogo
        {
            get;
            set;
        }

        public string ServiceName
        {
            get;
            set;
        }

        string _userName, _password;
        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                if (_userName == value) return;

                _userName = value;
                NotifyPropertyChanged(nameof(UserName));
            }
        }

        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                if (_password == value) return;

                _password = value;
                NotifyPropertyChanged(nameof(Password));
            }
        }

        public IRelayCommand<object> SaveCommand
        {
            get;
            private set;
        }

        public IRelayCommand<object> DeleteCommand
        {
            get;
            private set;
        }

        public IRelayCommand<object> LoadPassword
        {
            get;
            private set;
        }

        private readonly PasswordCredential WindowsCredential;

        public ILoginProvider ServiceProvider
        {
            get;
            private set;
        }

        public UserLoginInformation(PasswordCredential windowsCredential, ILoginProvider loginProvider)
        {
            WindowsCredential = windowsCredential;
            UserName = WindowsCredential.UserName;
            ServiceName = windowsCredential.Resource;
            SaveCommand = new AsyncRelayCommand<object>(Save);
            DeleteCommand = new AsyncRelayCommand<object>(Delete);
            LoadPassword = new RelayCommand<object>(LoadPass);
            ServiceProvider = loginProvider;
        }

        private void LoadPass(object inappNotification)
        {
            try
            {
                WindowsCredential.RetrievePassword();
                Password = WindowsCredential.Password;

                InAppNotification notificationRoot = inappNotification as InAppNotification;
                notificationRoot.Show("Password retrieved from storage", 2000);
            }
            catch { }
        }

        private async Task Delete(object inappNotification)
        {
            try
            {
                RemoveAllCredentials();
                Password = "";
                UserName = "";
                await ServiceProvider.LogoutAsync();
                InAppNotification notificationRoot = inappNotification as InAppNotification;
                notificationRoot.Show("Credentials deleted", 2000);
            }
            catch { }
        }

        private void RemoveAllCredentials()
        {
            PasswordVault vault = new PasswordVault();
            var allLogins = vault.RetrieveAll().Where(x => x.Resource == ServiceName);
            foreach (var c in allLogins)
            {
                vault.Remove(c);
            }
        }

        private async Task Save(object inappNotification)
        {
            InAppNotification notificationRoot = inappNotification as InAppNotification;

            try
            {
                if (!string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Password))
                {
                    WindowsCredential.UserName = UserName;
                    WindowsCredential.Password = Password;

                    try
                    {
                        PasswordVault vault = new PasswordVault();
                        //vault.Remove(WindowsCredential);
                        try
                        {
                            RemoveAllCredentials();
                        }
                        catch { }
                        vault.Add(WindowsCredential);
                        await ServiceProvider.LogoutAsync();
                        if (await ServiceProvider.LoginAsync())
                        {
                            notificationRoot.Show("Credentials saved", 2000);
                        }
                        else
                        {
                            notificationRoot.Content = new TextBlock()
                            {
                                Text = "Credentials saved but they could not be verified by remote service",
                                Foreground = new SolidColorBrush(Colors.Red)
                            };
                            notificationRoot.Show(6000);

                        }
                    }
                    catch (Exception eex)
                    {
                        notificationRoot.Content = new TextBlock()
                        {
                            Text = "Could not check credentials - service may be unreachable - " + eex.HResult,
                            Foreground = new SolidColorBrush(Colors.Red)
                        };
                        notificationRoot.Show(6000);
                    }
                }
            }
            catch (Exception ex)
            {

                notificationRoot.Content = new TextBlock()
                {
                    Text = "Operation could not be completed - " + ex.HResult,
                    Foreground = new SolidColorBrush(Colors.Red)
                };
                notificationRoot.Show(6000);
            }
        }
    }
}
