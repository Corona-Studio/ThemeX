# ThemeX

![0124](https://github.com/user-attachments/assets/f32cc9e2-6969-45b1-9bf7-f82ec3d7509d)

## [English](https://github.com/Corona-Studio/ThemeX/blob/main/README.md)

![GitHub](https://img.shields.io/github/license/corona-studio/themex?logo=github&style=for-the-badge)
![Maintenance](https://img.shields.io/maintenance/yes/2026?logo=diaspora&style=for-the-badge)
![DotNet Version](https://img.shields.io/badge/.NET-10-blue?style=for-the-badge)

一个现代化、流畅风格的 Avalonia UI 主题和控件库，提供精美的控件、流畅的动画和开箱即用的完整样式。

由日冕工作室开发与维护。

## 功能特性

### 自定义控件

| 控件 | 描述 |
| ---- | ---- |
| `Badge` | 徽章控件，可根据背景自动调整前景色 |
| `MultiProgressBar` | 多段式进度条，支持自动颜色渐变 |
| `ProgressRing` | 圆形进度指示器，支持不确定模式 |
| `IconPack` | 使用 Segoe 图标字体、支持可选叠加字形的单视觉节点图标控件 |
| `SimpleLineChart` | 轻量级实时折线图可视化 |
| `PrimaryButton` | 样式化的主要操作按钮 |
| `SmoothScrollViewer` | 带有平滑滚动动画的滚动视图 |

### 动画效果

| 效果 | 描述 |
| ---- | ---- |
| `RippleEffect` | Material Design 风格的交互涟漪动画 |
| `RevealEffect` | Windows Fluent Design 悬浮高亮效果 |
| `AnimationLayer` | 用于构建自定义动画的基础层 |

### 主题设置

ThemeX 提供了强大的 `ThemeSettings` 系统，用于运行时主题自定义：

- **主题色系统**：设置单一主题色，自动生成亮色/暗色/透明度变体
- **动画控制**：全局启用/禁用动画，自定义持续时间和缓动函数
- **流畅滚动**：切换平滑滚动行为
- **亚克力效果**：配置亚克力着色颜色

### 完整样式

ThemeX 为 30 多个 Avalonia 控件提供了专业设计的样式：

- Button、CheckBox、RadioButton、ToggleSwitch、ToggleButton
- TextBox、ComboBox、AutoCompleteBox、Slider
- ListBox、DataGrid、Menu、ContextMenu
- ScrollViewer、ScrollBar、Expander、SplitView
- ProgressBar、ToolTip、Window 等等...

## 多平台支持

| 平台 | 状态 |
| ---- | ---- |
| Windows | :white_check_mark: |
| macOS | :white_check_mark: |
| Linux | :white_check_mark: |

## 安装方式

### 方式一：项目引用

克隆 ThemeX 仓库并在解决方案中添加项目引用：

```xml
<ProjectReference Include="path/to/ThemeX/ThemeX.csproj" />
```

### 方式二：直接源码集成

将 ThemeX 源代码文件夹复制到您的解决方案中，并添加项目引用。

## 快速开始

### 1. 在 App.axaml 中添加 ThemeX

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:themeX="clr-namespace:ThemeX;assembly=ThemeX">
    <Application.Styles>
        <themeX:ThemeX />
    </Application.Styles>
</Application>
```

### 2. 自定义主题设置（可选）

```csharp
// 在 App.axaml.cs 或启动代码中
var themeSettings = Application.Current!.Styles.OfType<ThemeX.ThemeX>()
    .FirstOrDefault()?.Resources
    .MergedDictionaries.OfType<ThemeSettings>().FirstOrDefault();

if (themeSettings != null)
{
    // 设置主题色
    themeSettings.SetAccentColor(Color.Parse("#0078D7"));
    
    // 配置动画
    themeSettings.SetEnableAnimation(true);
    themeSettings.SetAnimationDuration(TimeSpan.FromMilliseconds(300));
    themeSettings.SetAnimationEasing(new SplineEasing(0.1, 0.9, 0.2));
    
    // 启用流畅滚动
    themeSettings.SetEnableFluentScroll(true);
}
```

### 3. 使用自定义控件

```xml
<Window xmlns:controls="clr-namespace:ThemeX.Controls;assembly=ThemeX"
        xmlns:scroll="clr-namespace:ThemeX.Controls.Scroll;assembly=ThemeX">
    
    <!-- 徽章 -->
    <controls:Badge Background="#0078D7" Symbol="&#xE77B;">
        新功能
    </controls:Badge>
    
    <!-- 进度环 -->
    <controls:ProgressRing Value="65" 
                           Minimum="0" 
                           Maximum="100" />
    
    <!-- 多段进度条 -->
    <controls:MultiProgressBar Values="{Binding Values}"
                               Tags="{Binding Tags}"
                               FillColor="#0078D7" />
    
    <!-- 平滑滚动视图 -->
    <scroll:SmoothScrollViewer UseSmoothScrolling="True"
                               SmoothScrollingStep="200">
        <!-- 内容 -->
    </scroll:SmoothScrollViewer>
    
</Window>
```

### 4. 使用动画效果

```xml
<Window xmlns:animations="clr-namespace:ThemeX.Controls.Animations;assembly=ThemeX">
    
    <!-- 涟漪效果按钮 -->
    <Button>
        <animations:RippleEffect>
            点击我
        </animations:RippleEffect>
    </Button>
    
    <!-- 悬浮高亮效果 -->
    <Border>
        <animations:RevealEffect HaloBrush="#40FFFFFF">
            悬浮以显示效果
        </animations:RevealEffect>
    </Border>
    
</Window>
```

## 主题色资源

当您设置主题色时，ThemeX 会自动生成以下资源：

| 资源键 | 描述 |
| ------ | ---- |
| `ThemeX.AccentColor` | 基础主题色画刷 |
| `ThemeX.AccentColor.Lighter1` ~ `Lighter5` | 递进式亮色变体 |
| `ThemeX.AccentColor.Darker1` ~ `Darker5` | 递进式暗色变体 |
| `ThemeX.AccentColor.Transparent` | 半透明主题色 |
| `ThemeX.AccentColor.Foreground` | 自动计算的对比前景色 |

## 辅助工具类

ThemeX 包含多个辅助类：

| 辅助类 | 描述 |
| ------ | ---- |
| `ColorHelper` | 亮度、WCAG 对比度及颜色处理辅助方法 |
| `AnimationHelper` | Composition 动画创建工具 |
| `BindingHelper` | 编译绑定辅助方法 |
| `GeometryHelper` | 路径和几何计算 |

## 依赖项

- [Avalonia](https://avaloniaui.net/) (11.3.10+)
- [CommunityToolkit.Diagnostics](https://github.com/CommunityToolkit/dotnet) - 诊断工具

## 许可证

MIT 许可证。这意味着您可以出于任何目的修改或使用我们的代码，但版权声明和许可声明应包含在您软件的所有副本或实质性部分中。

## 免责声明

ThemeX 与 Microsoft 或 Avalonia UI 没有任何关联。

## 耻辱堂

我们将在此列出所有未遵守 MIT 许可证使用我们代码的程序。
