# ThemeX

![0124](https://github.com/user-attachments/assets/f32cc9e2-6969-45b1-9bf7-f82ec3d7509d)


## [简体中文](https://github.com/Corona-Studio/ThemeX/blob/master/README_zh_cn.md)

![GitHub](https://img.shields.io/github/license/corona-studio/themex?logo=github&style=for-the-badge)
![Maintenance](https://img.shields.io/maintenance/yes/2026?logo=diaspora&style=for-the-badge)
![DotNet Version](https://img.shields.io/badge/.NET-10-blue?style=for-the-badge)

A modern, fluent-style theme and control library for Avalonia UI, providing beautiful controls, smooth animations, and comprehensive styling out of the box.

Developed and maintained by Corona Studio.

## Features

### Custom Controls

| Control | Description |
| ------- | ----------- |
| `Badge` | A badge control with automatic foreground color adjustment based on background |
| `MultiProgressBar` | Multi-segment progress bar with automatic color gradient |
| `ProgressRing` | Circular progress indicator with indeterminate mode support |
| `IconPack` | Single-visual Segoe icon control with optional overlay glyph support |
| `SimpleLineChart` | Lightweight, real-time line chart visualization |
| `PrimaryButton` | Styled primary action button |
| `SmoothScrollViewer` | ScrollViewer with smooth scrolling animation |

### Animation Effects

| Effect | Description |
| ------ | ----------- |
| `RippleEffect` | Material Design-inspired ripple animation on interaction |
| `RevealEffect` | Windows Fluent Design reveal highlight effect |
| `AnimationLayer` | Base layer for building custom animations |

### Theme Settings

ThemeX provides a powerful `ThemeSettings` system for runtime theme customization:

- **Accent Color System**: Set a single accent color and automatically generate lighter/darker/transparent variants
- **Animation Control**: Enable/disable animations globally, customize duration and easing
- **Fluent Scroll**: Toggle smooth scrolling behavior
- **Acrylic Effects**: Configure acrylic tint colors

### Comprehensive Styles

ThemeX includes professionally designed styles for 30+ Avalonia controls:

- Button, CheckBox, RadioButton, ToggleSwitch, ToggleButton
- TextBox, ComboBox, AutoCompleteBox, Slider
- ListBox, DataGrid, Menu, ContextMenu
- ScrollViewer, ScrollBar, Expander, SplitView
- ProgressBar, ToolTip, Window, and more...

## Multi-Platform Support

| Platform | Status             |
| -------- | ------------------ |
| Windows  | :white_check_mark: |
| macOS    | :white_check_mark: |
| Linux    | :white_check_mark: |

## Installation

### Method 1: Project Reference

Clone the ThemeX repository and add a project reference to your solution:

```xml
<ProjectReference Include="path/to/ThemeX/ThemeX.csproj" />
```

### Method 2: Direct Source Integration

Copy the ThemeX source folder into your solution and add the project reference.

## Quick Start

### 1. Add ThemeX to App.axaml

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:themeX="clr-namespace:ThemeX;assembly=ThemeX">
    <Application.Styles>
        <themeX:ThemeX />
    </Application.Styles>
</Application>
```

### 2. Customize Theme Settings (Optional)

```csharp
// In your App.axaml.cs or startup code
var themeSettings = Application.Current!.Styles.OfType<ThemeX.ThemeX>()
    .FirstOrDefault()?.Resources
    .MergedDictionaries.OfType<ThemeSettings>().FirstOrDefault();

if (themeSettings != null)
{
    // Set accent color
    themeSettings.SetAccentColor(Color.Parse("#0078D7"));
    
    // Configure animations
    themeSettings.SetEnableAnimation(true);
    themeSettings.SetAnimationDuration(TimeSpan.FromMilliseconds(300));
    themeSettings.SetAnimationEasing(new SplineEasing(0.1, 0.9, 0.2));
    
    // Enable fluent scroll
    themeSettings.SetEnableFluentScroll(true);
}
```

### 3. Using Custom Controls

```xml
<Window xmlns:controls="clr-namespace:ThemeX.Controls;assembly=ThemeX"
        xmlns:scroll="clr-namespace:ThemeX.Controls.Scroll;assembly=ThemeX">
    
    <!-- Badge -->
    <controls:Badge Background="#0078D7" Symbol="&#xE77B;">
        New
    </controls:Badge>
    
    <!-- Progress Ring -->
    <controls:ProgressRing Value="65" 
                           Minimum="0" 
                           Maximum="100" />
    
    <!-- Multi Progress Bar -->
    <controls:MultiProgressBar Values="{Binding Values}"
                               Tags="{Binding Tags}"
                               FillColor="#0078D7" />
    
    <!-- Smooth ScrollViewer -->
    <scroll:SmoothScrollViewer UseSmoothScrolling="True"
                               SmoothScrollingStep="200">
        <!-- Content -->
    </scroll:SmoothScrollViewer>
    
</Window>
```

### 4. Using Animation Effects

```xml
<Window xmlns:animations="clr-namespace:ThemeX.Controls.Animations;assembly=ThemeX">
    
    <!-- Ripple Effect Button -->
    <Button>
        <animations:RippleEffect>
            Click Me
        </animations:RippleEffect>
    </Button>
    
    <!-- Reveal Effect -->
    <Border>
        <animations:RevealEffect HaloBrush="#40FFFFFF">
            Hover to reveal
        </animations:RevealEffect>
    </Border>
    
</Window>
```

## Accent Color Resources

When you set an accent color, ThemeX automatically generates the following resources:

| Resource Key | Description |
| ------------ | ----------- |
| `ThemeX.AccentColor` | Base accent color brush |
| `ThemeX.AccentColor.Lighter1` ~ `Lighter5` | Progressively lighter variants |
| `ThemeX.AccentColor.Darker1` ~ `Darker5` | Progressively darker variants |
| `ThemeX.AccentColor.Transparent` | Semi-transparent accent |
| `ThemeX.AccentColor.Foreground` | Auto-calculated foreground for contrast |

## Helper Utilities

ThemeX includes several helper classes:

| Helper | Description |
| ------ | ----------- |
| `ColorHelper` | Image palette extraction, brightness detection, color manipulation |
| `AnimationHelper` | Composition animation creation utilities |
| `BindingHelper` | Compiled binding helpers |
| `GeometryHelper` | Path and geometry calculations |

### Example: Extract Image Palette

```csharp
using ThemeX.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

var image = Image.Load<Rgba32>("image.jpg");
var palette = image.GetPaletteFromBitmap(colorCount: 8);

foreach (var color in palette)
{
    Console.WriteLine($"Color: {color.Color}, Population: {color.Population}");
}
```

## Dependencies

- [Avalonia](https://avaloniaui.net/) (11.3.10+)
- [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp) - Image processing
- [CommunityToolkit.Diagnostics](https://github.com/CommunityToolkit/dotnet) - Diagnostics utilities

## License

MIT. This means that you can modify or use our code for any purpose, however copyright notice and permission notice shall be included in all copies or substantial portions of your software.

## Disclaimer

ThemeX is not affiliated with Microsoft or Avalonia UI.

## Hall of Shame

Here we'll list all programs using our code without obeying MIT License.
