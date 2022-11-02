using System;
using System.Globalization;
using System.Windows.Data;

namespace Zadanie_3.Konwerter
{
	public class Multipleby100 : IValueConverter
    {
        public object Convert(object v, Type type, object p, CultureInfo cultureInfo)
        {
            return Math.Round((float)v*100);
        }
        public object ConvertBack(object v, Type type, object p, CultureInfo cultureInfo)
        {
            return (double)v/100;
        }
    }
}