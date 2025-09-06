# AetherUI.Demo - 完整演示程序

## 📖 概述

AetherUI.Demo 是 AetherUI 框架的完整演示程序，展示了框架的所有核心功能和特性。这个演示程序提供了一个交互式的环境，让开发者能够直观地了解和体验 AetherUI 框架的强大功能。

## 🚀 快速开始

### 运行演示程序

```bash
# 在 AetherUI 解决方案根目录下
dotnet run --project AetherUI.Demo
```

### 系统要求

- .NET 9.0 或更高版本
- 支持 OpenGL 4.0+ 的显卡
- Windows 10/11, macOS 10.15+, 或 Linux (Ubuntu 18.04+)

## 🎯 演示内容

### 1. 布局系统演示
**功能**: 展示所有8种布局容器的使用效果
- **StackPanel**: 线性堆叠布局
- **Grid**: 网格布局系统
- **Canvas**: 绝对定位画布
- **DockPanel**: 停靠布局
- **WrapPanel**: 自动换行布局
- **UniformGrid**: 均匀网格
- **Border**: 边框容器
- **Card**: 卡片容器

**特色**: 实时展示不同布局容器的排列效果和嵌套能力

### 2. 基础控件演示
**功能**: 展示Button、TextBlock等基础控件
- 不同样式的按钮（默认、主要、危险）
- 不同大小的控件（小、中、大）
- 控件状态管理（正常、禁用、悬停）
- 文本样式和对齐方式
- 颜色和字体效果

**特色**: 完整的控件样式系统展示

### 3. XAML解析演示
**功能**: 展示XAML标记语言的解析和渲染
- 完整的XAML语法支持
- 嵌套控件结构
- 属性绑定和设置
- 布局属性配置
- 错误处理和提示

**示例XAML**:
```xml
<StackPanel Orientation="Vertical">
    <TextBlock Text="Hello AetherUI" FontSize="18"/>
    <Button Content="Click Me" Background="Blue"/>
</StackPanel>
```

### 4. JSON配置演示
**功能**: 展示JSON格式UI配置的解析
- 轻量级JSON格式
- 动态配置支持
- 程序化生成友好
- 跨平台兼容性

**示例JSON**:
```json
{
    "Type": "StackPanel",
    "Orientation": "Vertical",
    "Children": [
        {
            "Type": "TextBlock",
            "Text": "Hello AetherUI",
            "FontSize": 18
        }
    ]
}
```

### 5. 数据绑定演示
**功能**: 展示MVVM模式的数据绑定和命令
- 属性变化通知 (INotifyPropertyChanged)
- 命令绑定 (ICommand)
- 双向数据绑定
- 实时数据更新
- ViewModel 生命周期管理

**特色**: 完整的MVVM架构实现

### 6. 事件系统演示
**功能**: 展示鼠标、键盘等事件处理
- 路由事件（冒泡和捕获）
- 鼠标事件（点击、悬停、移动）
- 键盘事件（按键、释放）
- 事件传播机制
- 自定义事件处理

**特色**: 完整的事件系统架构

### 7. 渲染效果演示
**功能**: 展示OpenGL渲染管道的视觉效果
- 几何图形渲染（矩形、圆角、边框）
- 颜色和渐变效果
- 透明度和混合
- 几何变换（缩放、旋转、位移）
- 批量渲染优化
- 性能测试和统计

**特色**: 现代OpenGL渲染管道展示

### 8. 综合功能演示
**功能**: 展示框架的综合应用能力
- 完整的应用程序界面
- 多模块集成
- 实时数据仪表板
- 交互控制面板
- 导航系统
- 状态管理

**特色**: 真实应用程序的完整实现

## 🎮 交互指南

### 菜单导航
程序启动后会显示演示菜单：
```
可用的演示场景：
================
1. 布局系统演示 - 展示所有8种布局容器的使用效果
2. 基础控件演示 - 展示Button、TextBlock等基础控件
3. XAML解析演示 - 展示XAML标记语言的解析和渲染
4. JSON配置演示 - 展示JSON格式UI配置的解析
5. 数据绑定演示 - 展示MVVM模式的数据绑定和命令
6. 事件系统演示 - 展示鼠标、键盘等事件处理
7. 渲染效果演示 - 展示OpenGL渲染管道的视觉效果
8. 综合功能演示 - 展示框架的综合应用能力
9. 运行所有演示
0. 退出
```

### 窗口控制
- **ESC键**: 关闭当前演示窗口
- **鼠标交互**: 支持点击、悬停、拖拽等操作
- **键盘输入**: 支持各种按键事件

### 控制台输出
程序会在控制台输出详细的运行信息：
- 演示场景切换
- 事件触发记录
- 性能测试结果
- 错误和警告信息

## 🔧 技术架构

### 项目依赖
```xml
<ProjectReference Include="../AetherUI.Core/AetherUI.Core.csproj" />
<ProjectReference Include="../AetherUI.Events/AetherUI.Events.csproj" />
<ProjectReference Include="../AetherUI.Layout/AetherUI.Layout.csproj" />
<ProjectReference Include="../AetherUI.Rendering/AetherUI.Rendering.csproj" />
<ProjectReference Include="../AetherUI.Xaml/AetherUI.Xaml.csproj" />
<ProjectReference Include="../AetherUI.Compiler/AetherUI.Compiler.csproj" />
```

### 核心类结构
- **DemoApplication**: 主应用程序类
- **DemoViewModel**: 演示用的ViewModel
- **DemoScene**: 演示场景数据结构
- **各种Demos类**: 具体的演示实现

### 设计模式
- **MVVM模式**: 数据绑定和视图分离
- **命令模式**: 用户操作的封装
- **工厂模式**: 演示场景的创建
- **观察者模式**: 属性变化通知

## 📊 性能特性

### 渲染性能
- **VAO/VBO优化**: 现代OpenGL缓冲区管理
- **批量渲染**: 减少Draw Call次数
- **着色器缓存**: 程序重用和优化
- **视锥体剔除**: 不可见对象剔除

### 内存管理
- **对象池**: 减少GC压力
- **纹理管理**: 内存优化
- **事件订阅**: 自动清理机制

## 🐛 故障排除

### 常见问题

**Q: 程序启动失败，提示OpenGL错误**
A: 确保显卡驱动支持OpenGL 4.0+，更新显卡驱动程序

**Q: XAML/JSON解析失败**
A: 检查语法格式，参考演示中的示例代码

**Q: 事件不响应**
A: 确保事件处理器正确绑定，检查控制台错误信息

**Q: 渲染效果异常**
A: 检查OpenGL上下文是否正确创建，查看调试输出

### 调试信息
程序使用 `Debug.WriteLine` 输出详细的调试信息，可以在IDE的输出窗口中查看。

## 🔮 扩展开发

### 添加新的演示
1. 在 `Demos` 文件夹中创建新的演示类
2. 实现 `CreateXXXDemo()` 方法
3. 在 `DemoApplication.CreateDemoScenes()` 中注册新演示

### 自定义控件
1. 继承 `UIElement` 或 `FrameworkElement`
2. 实现渲染逻辑
3. 添加到演示中展示效果

### 性能测试
使用内置的性能测试功能，或添加自定义的基准测试。

## 📝 许可证

本演示程序遵循与 AetherUI 框架相同的许可证。

## 🤝 贡献

欢迎提交问题报告、功能请求和代码贡献。请确保：
1. 代码符合项目规范
2. 添加适当的注释和文档
3. 包含必要的测试用例

---

**AetherUI.Demo** - 体验现代化UI框架的强大功能！
