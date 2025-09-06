# AetherUI 解决方案概览

## 🏗️ 项目架构

AetherUI是一个现代化的跨平台UI框架，采用模块化设计，包含9个核心项目：

### 📦 项目列表

| 项目 | 描述 | 依赖关系 | 主要功能 |
|------|------|----------|----------|
| **AetherUI** | 主项目和入口点 | 依赖所有其他项目 | 框架集成和API暴露 |
| **AetherUI.Core** | 核心基础设施 | 无外部依赖 | MVVM、依赖属性、事件接口 |
| **AetherUI.Events** | 事件系统 | 依赖Core | 路由事件、输入事件、命令绑定 |
| **AetherUI.Layout** | 布局容器 | 依赖Core、Events | UI控件、布局算法 |
| **AetherUI.Rendering** | 渲染引擎 | 依赖Core、Layout、OpenTK | OpenGL渲染、窗口管理 |
| **AetherUI.Markup** | 标记语言基础 | 依赖Core | 标记语言解析基础设施 |
| **AetherUI.Xaml** | XAML/JSON解析 | 依赖Core、Layout、Markup | XAML和JSON解析器 |
| **AetherUI.Compiler** | 编译器和热重载 | 依赖Core、Layout、Xaml | 代码生成、热重载 |
| **AetherUI.Designer** | 设计器支持 | 依赖Core | 设计时数据绑定 |

## 🔧 技术栈

- **.NET 9.0** - 最新的.NET平台
- **OpenTK** - OpenGL图形渲染
- **System.Text.Json** - JSON解析
- **Microsoft.CodeAnalysis** - Roslyn编译器服务

## 🚀 核心特性

### 1. 现代渲染管道
- 基于OpenGL的现代渲染架构
- VAO/VBO/EBO缓冲区管理
- 着色器系统和批量渲染优化
- 几何图形渲染（矩形、圆角、线条、椭圆）

### 2. 完整的布局系统
- **StackPanel** - 线性堆叠布局
- **Grid** - 网格布局系统，支持行列定义
- **Canvas** - 绝对定位画布
- **DockPanel** - 停靠布局
- **WrapPanel** - 自动换行布局
- **UniformGrid** - 均匀网格
- **Border** - 边框容器
- **Card** - 卡片容器

### 3. MVVM架构支持
- **ViewModelBase** - 属性变化通知
- **RelayCommand** - 命令实现
- **DependencyProperty** - 依赖属性系统
- **数据绑定** - 双向数据绑定支持

### 4. 事件系统
- **路由事件** - 事件冒泡和捕获
- **输入事件** - 鼠标、键盘事件处理
- **命令绑定** - 命令到UI元素的绑定

### 5. 多格式标记语言支持
- **XAML解析器** - 完整的XAML语法支持
- **JSON解析器** - JSON格式UI配置
- **类型解析** - 动态类型查找和实例化
- **属性绑定** - 标记语言到对象属性的映射

### 6. 编译器和工具链
- **代码生成** - 标记语言到C#代码编译
- **热重载** - 文件变化的实时监控和重载
- **错误处理** - 完整的编译错误报告
- **Roslyn集成** - 使用Microsoft.CodeAnalysis

### 7. 设计时支持
- **设计时检测** - 自动检测设计时环境
- **模拟数据生成** - 自动生成设计时数据
- **设计时绑定** - 设计时数据绑定预览
- **属性管理** - 设计时属性存储和管理

## 📁 项目结构

```
AetherUI/
├── AetherUI/                    # 主项目
├── AetherUI.Core/              # 核心基础设施
│   ├── DependencyObject.cs     # 依赖对象基类
│   ├── DependencyProperty.cs   # 依赖属性系统
│   ├── UIElement.cs            # UI元素基类
│   ├── FrameworkElement.cs     # 框架元素基类
│   ├── Panel.cs                # 面板基类
│   ├── ViewModelBase.cs        # ViewModel基类
│   ├── RelayCommand.cs         # 命令实现
│   ├── DesignTime*.cs          # 设计时支持
│   └── Primitives.cs           # 基础类型
├── AetherUI.Events/            # 事件系统
│   ├── RoutedEvent.cs          # 路由事件
│   ├── EventManager.cs         # 事件管理器
│   ├── InputEvents.cs          # 输入事件
│   └── CommandBinding.cs       # 命令绑定
├── AetherUI.Layout/            # 布局容器
│   ├── StackPanel.cs           # 堆叠面板
│   ├── Grid.cs                 # 网格布局
│   ├── Canvas.cs               # 画布布局
│   ├── DockPanel.cs            # 停靠面板
│   ├── WrapPanel.cs            # 换行面板
│   ├── UniformGrid.cs          # 均匀网格
│   ├── Border.cs               # 边框
│   ├── Card.cs                 # 卡片
│   ├── Button.cs               # 按钮控件
│   └── TextBlock.cs            # 文本块
├── AetherUI.Rendering/         # 渲染引擎
│   ├── AetherWindow.cs         # 主窗口
│   ├── AetherApplication.cs    # 应用程序
│   ├── RenderContext.cs        # 渲染上下文
│   ├── ShaderManager.cs        # 着色器管理
│   ├── GeometryRenderer.cs     # 几何渲染器
│   └── UIRenderer.cs           # UI渲染器
├── AetherUI.Markup/            # 标记语言基础
├── AetherUI.Xaml/              # XAML/JSON解析
│   ├── XamlParser.cs           # XAML解析器
│   ├── XamlLoader.cs           # XAML加载器
│   ├── JsonParser.cs           # JSON解析器
│   └── JsonLoader.cs           # JSON加载器
├── AetherUI.Compiler/          # 编译器
│   ├── AetherCompiler.cs       # 主编译器
│   ├── FileWatcher.cs          # 文件监控
│   └── HotReloadManager.cs     # 热重载管理
├── AetherUI.Designer/          # 设计器支持
└── AetherUI.sln               # 解决方案文件
```

## 🔄 构建和运行

### 构建整个解决方案
```bash
dotnet build AetherUI.sln
```

### 运行各个模块的测试
```bash
# 核心功能测试
dotnet run --project AetherUI.Core

# 布局系统测试
dotnet run --project AetherUI.Layout

# 渲染系统测试
dotnet run --project AetherUI.Rendering

# XAML/JSON解析测试
dotnet run --project AetherUI.Xaml

# 编译器测试
dotnet run --project AetherUI.Compiler
```

## 📊 项目统计

- **总项目数**: 9个
- **代码文件**: 60+ C#文件
- **核心功能**: 16个主要功能模块
- **布局容器**: 8种不同类型
- **渲染特性**: 现代OpenGL管道
- **解析器**: XAML和JSON双重支持
- **工具链**: 编译器、热重载、设计时支持

## 🎯 使用示例

### XAML方式
```xml
<StackPanel Orientation="Vertical">
    <TextBlock Text="Hello AetherUI" FontSize="18" />
    <Button Content="Click Me" />
</StackPanel>
```

### JSON方式
```json
{
    "Type": "StackPanel",
    "Orientation": "Vertical",
    "Children": [
        {
            "Type": "TextBlock",
            "Text": "Hello AetherUI",
            "FontSize": 18
        },
        {
            "Type": "Button",
            "Content": "Click Me"
        }
    ]
}
```

### C#代码方式
```csharp
var stackPanel = new StackPanel
{
    Orientation = Orientation.Vertical
};

stackPanel.Children.Add(new TextBlock
{
    Text = "Hello AetherUI",
    FontSize = 18
});

stackPanel.Children.Add(new Button
{
    Content = "Click Me"
});
```

## 🔮 未来扩展

AetherUI框架已经具备了现代UI框架的核心功能，未来可以扩展：

1. **更多控件** - ListView、TreeView、TabControl等
2. **动画系统** - 属性动画和过渡效果
3. **主题系统** - 可切换的UI主题
4. **国际化** - 多语言支持
5. **性能优化** - 虚拟化和渲染优化
6. **平台特性** - 平台特定的原生集成

---

*AetherUI - 现代化的跨平台UI框架*
