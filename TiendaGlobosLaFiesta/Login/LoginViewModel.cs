using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using TiendaGlobosLaFiesta.Data;

namespace TiendaGlobosLaFiesta.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        public event Action<string> OnLoginSuccess;

        private string _username;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        private string _message;
        public string Message
        {
            get => _message;
            set { _message = value; OnPropertyChanged(); }
        }

        private bool _isLoginInProgress;
        public bool IsLoginInProgress
        {
            get => _isLoginInProgress;
            set { _isLoginInProgress = value; OnPropertyChanged(); }
        }

        private bool _isRememberMe;
        public bool IsRememberMe
        {
            get => _isRememberMe;
            set { _isRememberMe = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }

        private int _intentosFallidos = 0;
        private const int MAX_INTENTOS = 5;
        private const int BLOQUEO_SEGUNDOS = 30;
        private int _segundosRestantes;
        private bool _estaBloqueado = false;

        public LoginViewModel()
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.UsuarioGuardado))
            {
                Username = Properties.Settings.Default.UsuarioGuardado;
                IsRememberMe = true;
            }

            LoginCommand = new RelayCommand(async (parameter) => await ExecuteLogin(parameter), CanExecuteLogin);
        }

        private bool CanExecuteLogin(object parameter)
        {
            return !IsLoginInProgress && !_estaBloqueado;
        }

        private async Task ExecuteLogin(object parameter)
        {
            IsLoginInProgress = true;
            Message = "";

            string password = parameter as string;

            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(password))
            {
                Message = "Por favor, ingresa usuario y contraseña.";
                IsLoginInProgress = false;
                return;
            }

            string rolDelUsuario = string.Empty;
            string mensajeError = string.Empty;

            try
            {
                bool loginExitoso = await Task.Run(() =>
                    AuthService.Login(Username, password, out rolDelUsuario, out mensajeError));

                if (loginExitoso)
                {
                    Properties.Settings.Default.UsuarioGuardado = IsRememberMe ? Username : string.Empty;
                    Properties.Settings.Default.Save();

                    _intentosFallidos = 0;
                    _estaBloqueado = false;
                    OnLoginSuccess?.Invoke(rolDelUsuario);
                }
                else
                {
                    _intentosFallidos++;
                    Message = string.IsNullOrEmpty(mensajeError)
                        ? $"Usuario o contraseña incorrectos. Intento {_intentosFallidos}/{MAX_INTENTOS}"
                        : mensajeError;

                    if (_intentosFallidos >= MAX_INTENTOS)
                        await ActivarBloqueoAsync();
                }
            }
            finally
            {
                IsLoginInProgress = false;
            }
        }

        private async Task ActivarBloqueoAsync()
        {
            _estaBloqueado = true;
            _segundosRestantes = BLOQUEO_SEGUNDOS;

            while (_segundosRestantes > 0)
            {
                Message = $"Demasiados intentos. Intenta de nuevo en {_segundosRestantes} segundos.";
                await Task.Delay(1000);
                _segundosRestantes--;
            }

            _intentosFallidos = 0;
            _estaBloqueado = false;
            Message = "Puedes intentar de nuevo.";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}