using System.Text;

namespace photoCon.Helper
{
    public class SanitizedParam
    {
        public string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                if ((str[i] >= '0' && str[i] <= '9')
                    || (str[i] >= 'A' && str[i] <= 'z'
                        || (str[i] == '.' || str[i] == '_')))
                {
                    sb.Append(str[i]);
                }
            }

            return sb.ToString();
        }

        public string JavaScriptStringEncode(string value)
        {
            if (String.IsNullOrEmpty(value))
                return String.Empty;

            StringBuilder builder = new StringBuilder();
            int length = value.Length;
            for (int i = 0; i < length; i++)
            {
                char c = value[i];
                switch (c)
                {
                    case '\b':
                        builder.Append("\\b");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\f':
                        builder.Append("\\f");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\"':
                        builder.Append("\\\"");
                        break;
                    case '\'':
                        builder.Append("\\\'");
                        break;
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '<':
                        builder.Append("\\x3C");
                        break;
                    case '>':
                        builder.Append("\\x3E");
                        break;
                    case '&':
                        builder.Append("\\x26");
                        break;
                    default:
                        builder.Append(c);
                        break;
                }
            }
            return builder.ToString();
        }

    }
}
