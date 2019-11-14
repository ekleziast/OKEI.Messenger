using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MessengerWPF.Util
{
    public class DesignUtil
    {
        public static List<GradientStop> GenerateRandomGradient()
        {
            Random rnd = new Random();
            List<GradientStop> gradients = new List<GradientStop>();

            for (int i = 0; i < rnd.Next(3, 10); i++)
            {
                gradients.Add(new GradientStop { Color = Color.FromRgb(Convert.ToByte(rnd.Next(0, 255)), Convert.ToByte(rnd.Next(0, 255)), Convert.ToByte(rnd.Next(0, 255))), Offset = rnd.NextDouble() });
            }

            return gradients;
        }
    }
}
