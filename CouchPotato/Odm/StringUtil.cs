using System.Globalization;
using System.Text;

namespace CouchPotato.Odm {
  internal class StringUtil {
    public static string ToCamelCase(string s) {
      if (string.IsNullOrEmpty(s)) {
        return s;
      }
      if (!char.IsUpper(s[0])) {
        return s;
      }
      StringBuilder stringBuilder = new StringBuilder();
      int num = 0;
      while (num < s.Length) {
        bool length = num + 1 < s.Length;
        if (num == 0 || !length || char.IsUpper(s[num + 1])) {
          char lower = char.ToLower(s[num], CultureInfo.InvariantCulture);
          stringBuilder.Append(lower);
          num++;
        }
        else {
          stringBuilder.Append(s.Substring(num));
          break;
        }
      }
      return stringBuilder.ToString();
    }
  }
}
