using System;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.Xml.Linq;
using WpfLogin.Helpers;
using WpfLogin.Model;
using System.Xml;
using System.Windows.Controls;

namespace WpfLogin.ViewModel
{
    public class UserViewModel : ObservableObject
    {
        private User _user;

        public UserViewModel()
        {
            _user = new User { Login = "User_1" };
            this.CloseWindowCommand = new RelayCommand<Window>(this.CloseWindow);
        }
        
        public RelayCommand<Window> CloseWindowCommand { get; private set; }

        public User User
        {
            get
            {
                return _user;
            }
            set
            {
                _user = value;
            }
        }
        
        public string UserName
        {
            get { return User.Login; }
            set
            {
                if (User.Login != value)
                {
                    User.Login = value;
                    RaisePropertyChanged("UserName");
                }
            }
        }

        #region LogIn
        void LogInExecute(object obj)
        {
            PasswordBox pwBox = obj as PasswordBox;
            var pswd = GetPswdMD5(pwBox.Password);
            try
            {
                XDocument doc = XDocument.Load("Users.xml");
                var lgn = doc.Element("USERS").Elements("User")
                                             .Where(user => user.Element("Login").Value == UserName).First();
                var buf = lgn.Element("Password").Value.ToString();
                if ((buf == pswd))
                {
                    MessageBox.Show("LogIn successful", "Congatulations!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Wrong user name or password", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                MessageBox.Show("Wrong user name or password", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        bool CanLogInExecute(object obj)
        {
            PasswordBox pwBox;
            int pswdLength = 0;
            if (obj != null)
            {
                pwBox = obj as PasswordBox;
                pswdLength = pwBox.Password.Length;
            }
            if ((UserName != null) && (UserName.Length >= 3) && (pswdLength >= 3) && (File.Exists("Users.xml")))
                return true;
            return false;
        }

        public ICommand LogIn { get { return new RelayCommand<object>(LogInExecute, CanLogInExecute); } }
        #endregion

        #region Add New User
        void AddUserExecute(object obj)
        {
            if (_user == null)
                return;

            if (!File.Exists("Users.xml"))
            {
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.Indent = true;

                using (XmlWriter xmlWriter = XmlWriter.Create("Users.xml", xmlWriterSettings))
                {
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("USERS");

                    xmlWriter.WriteStartElement("User");
                    xmlWriter.WriteElementString("Login", UserName);
                    PasswordBox pwBox = obj as PasswordBox;
                    var pswd = GetPswdMD5(pwBox.Password);
                    xmlWriter.WriteElementString("Password", pswd);
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndDocument();

                    xmlWriter.Close();
                }
            }
            else
            {
                XDocument doc = XDocument.Load("Users.xml");
                XElement users = doc.Element("USERS");
                var a=doc.Element("USERS").Elements("User").Where(user => user.Element("Login").Value == UserName).Any();
                if (!a)
                {
                    PasswordBox pwBox = obj as PasswordBox;
                    var pswd = GetPswdMD5(pwBox.Password);
                    users.Add
                       (
                       new XElement("User", new XElement("Login", UserName), new XElement("Password", pswd))
                       );
                    doc.Save("Users.xml");
                    MessageBox.Show("User added successful", "Congatulations!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("User already exist", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; 
                }
            }
            
        }

        bool CanAddUserExecute(object obj)
        {
            PasswordBox pwBox;
            int pswdLength = 0;
            if (obj != null)
            {
                pwBox = obj as PasswordBox;
                pswdLength = pwBox.Password.Length;
            }
            if ((UserName != null && (UserName.Length >= 3) && (pswdLength >= 3)))
                return true;
            return false;
        }

        public ICommand AddUser { get { return new RelayCommand<object>(AddUserExecute, CanAddUserExecute); } }
        #endregion

        private string GetPswdMD5(string spwd)
        {
            string result = string.Empty;
            var inputData = UnicodeEncoding.Unicode.GetBytes(spwd);
            result = UnicodeEncoding.Unicode.GetString(new MD5CryptoServiceProvider().ComputeHash(inputData));
            return result;
        }

        private void CloseWindow(Window window)
        {
            if (window != null)
            {
                window.Close();
            }
        }
    }
}
