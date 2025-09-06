# AetherUI 按钮样式功能

## 概述

本文档介绍了为 AetherUI.Layout.Button 控件新增的默认视觉样式属性，包括颜色配置和圆角弧度支持。

## 🎨 新增功能

### 1. 默认颜色配置

#### 背景颜色 (Background)
- **默认值**: `#3498DB` (蓝色)
- **类型**: `string`
- **支持格式**: 十六进制颜色值 (如 `#3498DB`) 或颜色名称 (如 `Blue`)
- **用途**: 设置按钮的背景颜色

```csharp
Button button = new Button
{
    Background = "#E74C3C"  // 红色背景
};
```

#### 前景颜色 (Foreground)
- **默认值**: `#FFFFFF` (白色)
- **类型**: `string`
- **支持格式**: 十六进制颜色值或颜色名称
- **用途**: 设置按钮文本的颜色

```csharp
Button button = new Button
{
    Foreground = "#2C3E50"  // 深灰色文本
};
```

#### 边框颜色 (BorderBrush)
- **默认值**: `#2980B9` (深蓝色)
- **类型**: `string`
- **支持格式**: 十六进制颜色值或颜色名称
- **用途**: 设置按钮边框的颜色

```csharp
Button button = new Button
{
    BorderBrush = "#C0392B"  // 深红色边框
};
```

### 2. 圆角弧度支持

#### 圆角弧度 (CornerRadius)
- **默认值**: `7.0` (像素)
- **类型**: `double`
- **范围**: `0.0` 到任意正数
- **用途**: 设置按钮的圆角弧度

```csharp
Button button = new Button
{
    CornerRadius = 15.0  // 15像素圆角
};
```

## 🔧 技术实现

### 依赖属性

新增了四个依赖属性，支持数据绑定和属性更改通知：

```csharp
public static readonly DependencyProperty BackgroundProperty;
public static readonly DependencyProperty ForegroundProperty;
public static readonly DependencyProperty BorderBrushProperty;
public static readonly DependencyProperty CornerRadiusProperty;
```

### 属性访问器

提供了类型安全的属性访问器：

```csharp
public string Background { get; set; }
public string Foreground { get; set; }
public string BorderBrush { get; set; }
public double CornerRadius { get; set; }
```

### 渲染逻辑更新

在 `UIRenderer.cs` 中更新了按钮渲染逻辑：

- **颜色解析**: `ParseColorToVector4()` 方法支持十六进制颜色解析
- **内容渲染**: `RenderButtonContent()` 方法渲染按钮文本
- **圆角渲染**: 使用 `DrawRoundedRect()` 支持圆角背景

## 📝 使用示例

### 基础用法

```csharp
// 使用默认样式
Button defaultButton = new Button
{
    Content = "默认按钮"
    // 自动使用默认颜色和圆角
};

// 自定义样式
Button customButton = new Button
{
    Content = "自定义按钮",
    Background = "#E74C3C",    // 红色背景
    Foreground = "#FFFFFF",    // 白色文本
    BorderBrush = "#C0392B",   // 深红色边框
    CornerRadius = 12.0        // 12像素圆角
};
```

### 主题按钮

```csharp
// 成功按钮
Button successButton = new Button
{
    Content = "成功",
    Background = "#2ECC71",
    Foreground = "#FFFFFF",
    BorderBrush = "#27AE60",
    CornerRadius = 8.0
};

// 警告按钮
Button warningButton = new Button
{
    Content = "警告",
    Background = "#F39C12",
    Foreground = "#FFFFFF",
    BorderBrush = "#E67E22",
    CornerRadius = 8.0
};

// 危险按钮
Button dangerButton = new Button
{
    Content = "危险",
    Background = "#E74C3C",
    Foreground = "#FFFFFF",
    BorderBrush = "#C0392B",
    CornerRadius = 8.0
};
```

### 动态样式修改

```csharp
Button dynamicButton = new Button
{
    Content = "点击改变样式"
};

bool isToggled = false;
dynamicButton.Click += (s, e) =>
{
    isToggled = !isToggled;
    if (isToggled)
    {
        dynamicButton.Background = "#E67E22";
        dynamicButton.CornerRadius = 20.0;
    }
    else
    {
        dynamicButton.Background = "#3498DB";
        dynamicButton.CornerRadius = 7.0;
    }
};
```

## 🧪 测试和验证

### 运行测试

1. **主演示程序**:
   ```bash
   cd AetherUI.Demo
   dotnet run
   # 选择 "4" 进入按钮样式测试
   ```

2. **独立样式测试**:
   ```bash
   cd AetherUI.Demo
   dotnet run --project ButtonStyleTest.csproj
   ```

3. **属性验证程序**:
   ```bash
   cd AetherUI.Demo
   dotnet run ValidateButtonStyles.cs
   ```

### 测试脚本

运行 PowerShell 测试脚本：
```powershell
cd AetherUI.Demo
.\TestButtonStyles.ps1
```

## 🎯 测试要点

### 默认样式验证
- 验证新创建的按钮使用正确的默认颜色
- 确认默认圆角弧度为 7 像素
- 检查默认样式的视觉效果

### 自定义样式测试
- 测试十六进制颜色值的正确解析
- 验证不同圆角弧度的渲染效果
- 确认颜色属性的实时修改

### 兼容性测试
- 确保现有代码不受影响
- 验证向后兼容性
- 测试与其他控件的集成

### 性能测试
- 验证颜色解析的性能
- 测试圆角渲染的效率
- 确认窗口调整时的稳定性

## 🔍 调试信息

程序运行时会输出详细的调试信息：

```
Button background changed from #3498DB to #E74C3C
Button corner radius changed from 7 to 15
Rendered Button: (10, 20, 200, 40), Background: #E74C3C, Border: #C0392B, CornerRadius: 15
Rendered Button Content: '自定义按钮' at (50.0, 30.0) with color #FFFFFF
```

## 🚀 扩展建议

1. **渐变背景**: 支持线性和径向渐变
2. **阴影效果**: 添加可配置的阴影属性
3. **动画支持**: 颜色和圆角的过渡动画
4. **主题系统**: 预定义的按钮主题
5. **状态样式**: 悬停、按下、禁用状态的样式

## 📋 更新日志

### v1.0.0 - 按钮样式功能
- ✅ 添加 Background 属性 (默认: #3498DB)
- ✅ 添加 Foreground 属性 (默认: #FFFFFF)
- ✅ 添加 BorderBrush 属性 (默认: #2980B9)
- ✅ 添加 CornerRadius 属性 (默认: 7.0)
- ✅ 实现十六进制颜色解析
- ✅ 更新按钮渲染逻辑
- ✅ 添加按钮内容文本渲染
- ✅ 保持向后兼容性
- ✅ 创建测试和验证程序

---

*此功能增强了 AetherUI 框架的按钮控件，提供了现代化的视觉样式配置能力，同时保持了框架的简洁性和性能。*
