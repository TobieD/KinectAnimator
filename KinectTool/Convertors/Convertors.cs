using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Meshy;
using SharpDX.Collections;

namespace KinectTool.Convertors
{
    public class DoubleToInt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var doubleV = value is double ? (double) value : 0;
            return (int) doubleV;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var intV = value is int ? (int) value : 0;
            return intV;
        }
    }

    public class BoolToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var nulBool = value as bool?;

            return nulBool == true ? Visibility.Visible : Visibility.Hidden;
        }

        //target to source
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var vis = value as Visibility?;

            return vis == Visibility.Visible ? true : false;
        }
    }

    public class BoolInvertVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var nulBool = value as bool?;

            return nulBool == true ? Visibility.Hidden : Visibility.Visible;
        }

        //target to source
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var vis = value as Visibility?;
            return vis != Visibility.Visible;
        }
    }

    public class CollectionSizeToBool: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var c = value as ObservableDictionary<string,AnimationClip>;


            if (c == null)
                return false;

            return (c.Count != 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
}
