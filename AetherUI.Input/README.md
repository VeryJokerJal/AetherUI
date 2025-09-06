# AetherUI.Input - 高性能跨平台输入事件管线

## 🚀 概述

AetherUI.Input 是一个现代化的、高性能的跨平台UI输入事件处理框架，提供从平台原始事件到应用层事件的完整处理管线。该框架经过5个迭代的开发，实现了生产级的输入处理系统，包括Windows平台支持、高级命中测试、事件路由、手势识别和完整的输入管道。

## ✨ 核心特性

### 📱 多平台支持
- **Windows**: 完整的Win32消息处理和WM_POINTER支持
- **跨平台架构**: 可扩展到macOS、Linux、iOS、Android等平台

### ⚡ 高性能架构
- **异步输入管道**: 高吞吐量、低延迟的事件处理
- **智能缓存系统**: 空间索引、变换缓存、路由缓存
- **批量处理**: 事件合并和批量处理优化

## 架构设计

### 核心模块

1. **平台抽象层 (Platform Layer)**
   - `IPlatformInputProvider`: 平台输入提供者接口
   - 支持 Windows、macOS、Linux、iOS、Android 等平台
   - 接收原始输入事件并转发到管线

2. **事件标准化层 (Normalization Layer)**
   - `InputEvent`: 所有输入事件的基类
   - `PointerEvent`: 指针事件（鼠标、触摸、笔）
   - `KeyboardEvent`: 键盘事件
   - `TextInputEvent`: 文本输入事件
   - `GestureEvent`: 手势事件

3. **命中测试系统 (Hit Testing)**
   - `IHitTestable`: 可命中测试的元素接口
   - `IHitTestEngine`: 命中测试引擎
   - 支持变换、裁剪、透明度、Z顺序等复杂场景

4. **事件路由系统 (Event Routing)**
   - `IEventRouter`: 事件路由器接口
   - 支持隧道（Preview）、直接、冒泡三种路由策略
   - 支持 Handled/HandledToo 机制

5. **捕获系统 (Capture System)**
   - `IInputCaptureManager`: 输入捕获管理器
   - 按指针ID捕获，支持显式/隐式捕获
   - 自动释放规则和嵌套捕获支持

6. **焦点系统 (Focus System)**
   - `IFocusManager`: 焦点管理器
   - 逻辑焦点与键盘焦点分离
   - Tab导航、焦点域、方向导航

7. **手势识别 (Gesture Recognition)**
   - `IGestureRecognizer`: 手势识别器接口
   - 支持点击、双击、长按、拖拽、捏合、旋转等手势
   - 多指操作和复杂手势支持

8. **文本输入/IME (Text Input)**
   - `ITextInputManager`: 文本输入管理器
   - IME组合事件支持
   - 光标/选区反馈

9. **诊断系统 (Diagnostics)**
   - `IInputDiagnostics`: 输入诊断接口
   - 事件日志、性能统计、跟踪
   - 可视化调试支持

## 设计原则

### SOLID 原则
- **单一职责**: 每个接口和类都有明确的单一职责
- **开闭原则**: 通过接口扩展，对修改关闭
- **里氏替换**: 所有实现都可以替换接口
- **接口隔离**: 小接口，避免强制依赖不需要的功能
- **依赖倒置**: 依赖抽象而非具体实现

### 性能优化
- **命中测试**: 平均 O(log n) 复杂度，使用空间索引
- **对象池**: 减少频繁分配，降低 GC 压力
- **路径缓存**: 缓存事件路由路径
- **延迟计算**: 变换矩阵等昂贵操作延迟计算

### 线程安全
- **UI线程绑定**: 所有输入事件在UI线程处理
- **平台事件封送**: 通过Dispatcher封送到UI线程
- **异步支持**: 手势识别等支持异步处理

### 可测试性
- **依赖注入**: 所有依赖都可注入
- **接口驱动**: 便于Mock和单元测试
- **事件注入**: 支持测试事件注入
- **状态隔离**: 无全局状态，便于并行测试

## 事件处理流程

```
原始平台事件 → 标准化 → 命中测试 → 捕获处理 → 手势识别 → 事件路由 → 焦点处理 → 文本输入 → 诊断记录
```

### 详细流程

1. **原始事件接收**: 平台提供者接收Win32/Cocoa/X11等原始事件
2. **事件标准化**: 转换为框架内部的统一事件格式
3. **命中测试**: 确定事件的目标元素
4. **捕获处理**: 检查是否有元素捕获了该指针
5. **手势识别**: 识别复杂手势模式
6. **事件路由**: 按隧道→直接→冒泡顺序路由事件
7. **焦点处理**: 处理焦点变化和键盘导航
8. **文本输入**: 处理文本输入和IME组合
9. **诊断记录**: 记录性能指标和调试信息

## 使用示例

### 基本初始化

```csharp
// 创建输入管理器
var inputManager = InputManagerFactory.CreateInputManager();

// 设置根元素
inputManager.RootElement = myRootElement;

// 初始化（传入窗口句柄）
inputManager.Initialize(windowHandle);

// 订阅事件
inputManager.InputEventProcessed += OnInputEventProcessed;
```

### 自定义手势识别

```csharp
// 创建自定义手势识别器
public class CustomGestureRecognizer : IGestureRecognizer
{
    public string GestureType => "CustomGesture";
    
    public bool ProcessPointerEvent(PointerEvent pointerEvent)
    {
        // 实现自定义手势逻辑
        return false;
    }
}

// 注册手势识别器
inputManager.GestureManager.RegisterRecognizer(new CustomGestureRecognizer());
```

### 焦点管理

```csharp
// 设置焦点
inputManager.FocusManager.SetFocus(myElement);

// Tab导航
inputManager.FocusManager.MoveFocus(FocusNavigationDirection.Next);

// 创建焦点域
var scope = new FocusScope("MyScope");
inputManager.FocusManager.RegisterScope(scope);
```

## 扩展性

### 平台扩展
通过实现 `IPlatformInputProvider` 接口可以轻松添加新平台支持：

```csharp
public class MyPlatformInputProvider : IPlatformInputProvider
{
    public event EventHandler<RawInputEventArgs>? RawInputReceived;
    
    public void Initialize(IntPtr windowHandle)
    {
        // 平台特定的初始化代码
    }
    
    // 实现其他接口方法...
}
```

### 手势扩展
通过实现 `IGestureRecognizer` 接口可以添加新的手势类型：

```csharp
public class PinchGestureRecognizer : IGestureRecognizer
{
    public string GestureType => GestureTypes.Pinch;
    
    public bool ProcessPointerEvent(PointerEvent pointerEvent)
    {
        // 实现捏合手势识别逻辑
        return false;
    }
}
```

## 配置选项

### 输入管理器配置

```csharp
var config = new InputManagerConfiguration
{
    EnableGestures = true,
    EnableFocusManagement = true,
    EnableInputCapture = true,
    EnableHitTestCache = true,
    HitTestCacheSize = 1000,
    EventProcessingTimeoutMs = 5000,
    EnableEventLogging = true,
    LogLevel = InputEventLogLevel.Information
};

var inputManager = InputManagerFactory.CreateInputManager(config);
```

### 手势配置

```csharp
var gestureConfig = new GestureConfiguration
{
    TapTimeoutMs = 300,
    DoubleTapIntervalMs = 500,
    LongPressDelayMs = 1000,
    DragThreshold = 5.0,
    PinchThreshold = 10.0,
    MaxTapMovement = 10.0
};
```

## 性能监控

### 获取统计信息

```csharp
// 获取事件统计
var eventStats = inputManager.Diagnostics.GetEventStatistics();
Console.WriteLine($"总事件数: {eventStats.TotalEvents}");
Console.WriteLine($"平均处理时间: {eventStats.AverageProcessingTimeMs}ms");

// 获取性能统计
var perfStats = inputManager.Diagnostics.GetPerformanceStatistics();
Console.WriteLine($"命中测试平均时间: {perfStats.HitTestPerformance.AverageTimeMs}ms");
```

### 导出诊断数据

```csharp
// 导出为JSON格式
string diagnosticsJson = inputManager.Diagnostics.ExportDiagnostics(DiagnosticsExportFormat.Json);
File.WriteAllText("input_diagnostics.json", diagnosticsJson);
```

## 最佳实践

1. **及时释放资源**: 使用 `using` 语句或手动调用 `Dispose()`
2. **避免长时间处理**: 事件处理器应该快速返回，避免阻塞UI线程
3. **合理使用捕获**: 只在必要时使用指针捕获，及时释放
4. **性能监控**: 在生产环境中启用性能监控，及时发现问题
5. **错误处理**: 在事件处理器中添加适当的错误处理
6. **测试覆盖**: 为自定义手势和事件处理器编写单元测试

## 依赖关系

- **AetherUI.Core**: 基础类型和接口
- **AetherUI.Events**: 现有事件定义（向后兼容）
- **System.Numerics.Vectors**: 数学计算支持

## 版本兼容性

- **.NET 8.0+**: 目标框架
- **C# 12.0+**: 语言版本
- **跨平台**: Windows、macOS、Linux、iOS、Android

## 许可证

本项目采用与 AetherUI 主项目相同的许可证。
