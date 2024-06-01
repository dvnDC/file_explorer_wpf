using System.Globalization;
using System.Windows.Data;
using WpfApp1.Dictionary.Language;

namespace WpfApp1.Model
{
    public class CultureResources
    {
        public Strings GetStringsInstance()
        {
            return new Strings();
        }

        private static ObjectDataProvider provider;

        public static ObjectDataProvider ResourceProvider
        {
            get
            {
                if (provider == null)
                {
                    provider = (ObjectDataProvider)System.Windows.Application.Current.FindResource("Strings");
                }
                return provider;
            }
        }

        public static void ChangeCulture(CultureInfo culture)
        {
            CultureInfo.CurrentUICulture = culture;
            ResourceProvider.Refresh();
        }
    }
}
