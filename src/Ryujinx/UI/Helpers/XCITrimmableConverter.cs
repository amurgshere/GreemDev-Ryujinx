using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Ryujinx.Ava.Common.Locale;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Ryujinx.Ava.UI.Helpers
{
    internal class XCITrimmableConverter : IMultiValueConverter
    {
        public static XCITrimmableConverter Instance = new();

        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Any(it => it is UnsetValueType))
            {
                return BindingOperations.DoNothing;
            }
            
            if (values.Count != 2 || !targetType.IsAssignableFrom(typeof(string)))
            {
                return null;
            }

            if (values is not [bool Trimmable, bool Untrimmable])
            {
                return null;
            }

            return (Trimmable & Untrimmable) ? LocaleManager.Instance[LocaleKeys.TitleXCITrimmableBothLabel] :
                Trimmable ? LocaleManager.Instance[LocaleKeys.TitleXCITrimmableLabel] : 
                Untrimmable ? LocaleManager.Instance[LocaleKeys.TitleXCIUntrimmableLabel] :
                String.Empty;

            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
