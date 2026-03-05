using System.Text.RegularExpressions;

namespace photoCon.Helper
{
    public class InputValidator
    {
        public bool IsValidUsername(string username)
        {
            // Check if username is letters only OR alphanumeric and has a minimum of 6 characters
            return Regex.IsMatch(username, "^[a-zA-Z]{6,}$") || Regex.IsMatch(username, "^[a-zA-Z][a-zA-Z0-9]{5,}$");
        }


        public bool IsValidPassword(string password)
        {
            // Check if password is alphanumeric and at least 6 characters
            return Regex.IsMatch(password, "^[a-zA-Z0-9]{6,}$");
        }

        public bool IsValidName(string name)
        {
            // Check if name contains only letters and optional whitespaces
            return Regex.IsMatch(name, "^[a-zA-Z- ]+$");
        }

        public bool IsNullString(string param_)
        {
            return string.IsNullOrEmpty(param_);
        }

        public bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, "(^[a-zA-Z]([a-zA-Z0-9._-]*[@@][a-zA-Z]+[.][a-zA-Z]{1,3})$)");
        }
    }
}
