using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ThemeX.Converters;

public sealed class ValidationErrorsToStringConverter : SingleWayConverter
{
    public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not IEnumerable<object> errors) return value;

        var messages = new HashSet<string>();
        foreach (var error in errors)
        {
            switch (error)
            {
                case string text when !string.IsNullOrEmpty(text):
                    messages.Add(text);
                    break;
                case AggregateException agg:
                    foreach (var inner in agg.InnerExceptions)
                        messages.Add(inner.Message);
                    break;
                case Exception ex:
                    messages.Add(ex.Message);
                    break;
                case ValidationResult result when !string.IsNullOrEmpty(result.ErrorMessage):
                    messages.Add(result.ErrorMessage);
                    break;
            }
        }

        return messages;
    }
}
