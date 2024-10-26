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
    internal class XCITrimmableSpaceSavingsConverter : IMultiValueConverter
    {
        private const long _bytesPerMB = 1024 * 1024;
        public static XCITrimmableSpaceSavingsConverter Instance = new();

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

            if (values is not [long PotentialSavingsB, long CurrentSavingsB])
            {
                return null;
            }

            if (CurrentSavingsB < PotentialSavingsB)
            {
                return LocaleManager.Instance.UpdateAndGetDynamicValue(LocaleKeys.TitleXCICanSaveLabel, (PotentialSavingsB - CurrentSavingsB) / _bytesPerMB);
            }
            else 
            {
                return LocaleManager.Instance.UpdateAndGetDynamicValue(LocaleKeys.TitleXCISavingLabel, CurrentSavingsB / _bytesPerMB);
            }

            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
