using System.Windows;
using System.Windows.Data;

namespace ClearAzureQueues.Converters {

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : AbstractBooleanConverter<Visibility> {

        public BooleanToVisibilityConverter() : base(Visibility.Visible, Visibility.Collapsed) {
        }
    }
}
