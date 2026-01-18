using Avalonia;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;

namespace ThemeX.Helpers;

public static class BindingHelper
{
    public static IDisposable CompiledBind(
        this AvaloniaObject bindTo,
        AvaloniaProperty bindToProperty,
        string bindingPath,
        object source,
        Type propertyType,
        Func<object, object?> getter,
        Action<object, object?>? setter = null)
    {
        var clrProperty = new ClrPropertyInfo(
            bindingPath,
            getter,
            setter,
            propertyType);
        var factory = new Func<WeakReference<object?>, IPropertyInfo, IPropertyAccessor>(
            PropertyInfoAccessorFactory.CreateInpcPropertyAccessor);
        var path = new CompiledBindingPathBuilder().Property(clrProperty, factory);
        var cb = new CompiledBindingExtension
        {
            Source = source,
            Path = path.Build()
        };

        return bindTo.Bind(bindToProperty, cb);
    }
}