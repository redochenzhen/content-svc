using System.Linq;
using System.Text.RegularExpressions;

namespace ContentSvc.WebApi.Helpers
{
    public static class TextHelper
    {
        public static bool HasChinese(this string text)
        {
            if (text == null) return false;
            var p = new Regex(@"\p{IsCJKUnifiedIdeographs}");
            return text.Any(c => p.IsMatch(c.ToString()));
        }
    }
}