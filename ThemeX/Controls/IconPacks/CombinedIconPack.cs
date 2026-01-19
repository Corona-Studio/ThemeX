using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using ThemeX.SourceGenerator.Shared;

namespace ThemeX.Controls.IconPacks;

public sealed partial class CombinedIconPack : TemplatedControl
{
    private IconPack? _icon1;

    private IDisposable? _icon1Bind;
    private IconPack? _icon2;
    private IDisposable? _icon2Bind;

    [StyledProp] public partial string Symbol1 { get; set; }

    [StyledProp] public partial string Symbol2 { get; set; }

    [StyledProp] public partial IBrush? Symbol1Foreground { get; set; }

    [StyledProp] public partial IBrush? Symbol2Foreground { get; set; }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _icon1 = e.NameScope.Find<IconPack>("PART_Icon1");
        _icon2 = e.NameScope.Find<IconPack>("PART_Icon2");

        UpdateForegrounds();
    }

    private void UpdateForegrounds()
    {
        if (_icon1 != null && Symbol1Foreground != null)
        {
            _icon1Bind?.Dispose();
            _icon1.ClearValue(ForegroundProperty);
            _icon1Bind = _icon1.Bind(ForegroundProperty, this.GetObservable(Symbol1ForegroundProperty));
        }
        else
        {
            _icon1Bind?.Dispose();
            _icon1Bind = _icon1?.Bind(ForegroundProperty, this.GetObservable(ForegroundProperty));
        }

        if (_icon2 != null && Symbol2Foreground != null)
        {
            _icon2Bind?.Dispose();
            _icon2.ClearValue(ForegroundProperty);
            _icon2Bind = _icon2.Bind(ForegroundProperty, this.GetObservable(Symbol2ForegroundProperty));
        }
        else
        {
            _icon2Bind?.Dispose();
            _icon2Bind = _icon2?.Bind(ForegroundProperty, this.GetObservable(ForegroundProperty));
        }
    }

    partial void OnSymbol1Changed()
    {
        UpdateForegrounds();
    }

    partial void OnSymbol2Changed()
    {
        UpdateForegrounds();
    }
}