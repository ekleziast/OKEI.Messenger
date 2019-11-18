using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace MessengerWPF
{
    public class ConversationMemberColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Guid ID = (Guid)value;
            if(MessengerClient.GetInstant().Person.ID == ID)
            {
                return new SolidColorBrush(Color.FromRgb(0, 255, 0));
            }
            else
            {
                return new SolidColorBrush(Color.FromRgb(0, 0, 255));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
