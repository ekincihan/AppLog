using System.Text;

namespace AppLog.Core.Model.Helper
{
    public static class StringHelper
    {
        
        public static string ConvertTurkishCharToEnglishChar(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding iso = Encoding.GetEncoding("windows-1254");
            byte[] utfBytes = Encoding.UTF8.GetBytes(text);
            byte[] isoBytes = Encoding.Convert(Encoding.UTF8, iso, utfBytes);

            return iso.GetString(isoBytes);

        }
    }
}
