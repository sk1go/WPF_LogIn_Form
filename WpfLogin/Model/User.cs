namespace WpfLogin.Model
{
    /// <summary>
    /// Model of a 'user'.
    /// </summary>
    public class User
    {
        string _login;

        /// <summary>
        /// User's login.
        /// </summary>
        public string Login
        {
            get { return _login; }
            set { _login = value; }
        }
    }
}
