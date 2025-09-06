using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AetherUI.Input.Core;
using AetherUI.Input.Events;
using AetherUI.Input.HitTesting;

namespace AetherUI.Input.Pipeline
{
    /// <summary>
    /// 输入处理器接口
    /// </summary>
    public interface IInputProcessor
    {
        /// <summary>
        /// 处理器优先级（数值越小优先级越高）
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// 处理器名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 检查是否可以处理指定事件
        /// </summary>
        /// <param name="inputEvent">输入事件</param>
        /// <returns>是否可以处理</returns>
        bool CanProcess(InputEvent inputEvent);

        /// <summary>
        /// 异步处理输入事件
        /// </summary>
        /// <param name="inputEvent">输入事件</param>
        /// <param name="context">处理上下文</param>
        /// <returns>处理结果</returns>
        Task<InputProcessingResult> ProcessAsync(InputEvent inputEvent, InputProcessingContext context);
    }

    /// <summary>
    /// 输入处理上下文
    /// </summary>
    public class InputProcessingContext
    {
        /// <summary>
        /// 原始输入事件
        /// </summary>
        public InputEvent OriginalEvent { get; }

        /// <summary>
        /// 根元素
        /// </summary>
        public IHitTestable? RootElement { get; set; }

        /// <summary>
        /// 焦点元素
        /// </summary>
        public object? FocusedElement { get; set; }

        /// <summary>
        /// 命中测试结果
        /// </summary>
        public HitTestResult? HitTestResult { get; set; }

        /// <summary>
        /// 处理器结果
        /// </summary>
        public Dictionary<string, InputProcessingResult> ProcessorResults { get; } = new();

        /// <summary>
        /// 自定义数据
        /// </summary>
        public Dictionary<string, object> Data { get; } = new();

        /// <summary>
        /// 处理开始时间
        /// </summary>
        public DateTime StartTime { get; }

        /// <summary>
        /// 初始化输入处理上下文
        /// </summary>
        /// <param name="originalEvent">原始输入事件</param>
        public InputProcessingContext(InputEvent originalEvent)
        {
            OriginalEvent = originalEvent ?? throw new ArgumentNullException(nameof(originalEvent));
            StartTime = DateTime.UtcNow;
        }

        /// <summary>
        /// 获取处理时长
        /// </summary>
        public TimeSpan ProcessingDuration => DateTime.UtcNow - StartTime;
    }

    /// <summary>
    /// 输入处理结果
    /// </summary>
    public class InputProcessingResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// 是否应该停止后续处理
        /// </summary>
        public bool ShouldStopProcessing { get; set; }

        /// <summary>
        /// 修改后的事件（如果有）
        /// </summary>
        public InputEvent? ModifiedEvent { get; set; }

        /// <summary>
        /// 处理消息
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// 处理时间
        /// </summary>
        public TimeSpan ProcessingTime { get; set; }

        /// <summary>
        /// 自定义数据
        /// </summary>
        public Dictionary<string, object> Data { get; } = new();

        /// <summary>
        /// 创建成功结果
        /// </summary>
        public static InputProcessingResult CreateSuccess(string? message = null)
        {
            return new InputProcessingResult { Success = true, Message = message };
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        public static InputProcessingResult CreateFailure(string message)
        {
            return new InputProcessingResult { Success = false, Message = message };
        }

        /// <summary>
        /// 创建停止处理结果
        /// </summary>
        public static InputProcessingResult CreateStop(string? message = null)
        {
            return new InputProcessingResult { Success = true, ShouldStopProcessing = true, Message = message };
        }

        public override string ToString() =>
            $"{(Success ? "Success" : "Failure")}{(ShouldStopProcessing ? " (Stop)" : "")} - {Message}";
    }

    /// <summary>
    /// 输入验证处理器
    /// </summary>
    public class InputValidationProcessor : IInputProcessor
    {
        /// <summary>
        /// 处理器优先级
        /// </summary>
        public int Priority => 10;

        /// <summary>
        /// 处理器名称
        /// </summary>
        public string Name => "InputValidation";

        /// <summary>
        /// 检查是否可以处理指定事件
        /// </summary>
        public bool CanProcess(InputEvent inputEvent)
        {
            return true; // 验证处理器处理所有事件
        }

        /// <summary>
        /// 异步处理输入事件
        /// </summary>
        public async Task<InputProcessingResult> ProcessAsync(InputEvent inputEvent, InputProcessingContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // 验证事件基本属性
                if (inputEvent.Timestamp == 0)
                {
                    return InputProcessingResult.CreateFailure("事件时间戳无效");
                }

                if (inputEvent.Device == null)
                {
                    return InputProcessingResult.CreateFailure("事件设备信息缺失");
                }

                // 验证指针事件
                if (inputEvent is PointerEvent pointerEvent)
                {
                    if (pointerEvent.Position.X < 0 || pointerEvent.Position.Y < 0)
                    {
                        return InputProcessingResult.CreateFailure("指针位置无效");
                    }
                }

                // 验证键盘事件
                if (inputEvent is KeyboardEvent keyboardEvent)
                {
                    if (keyboardEvent.Key == Key.None)
                    {
                        return InputProcessingResult.CreateFailure("键盘按键无效");
                    }
                }

                await Task.CompletedTask;
                return InputProcessingResult.CreateSuccess("事件验证通过");
            }
            finally
            {
                stopwatch.Stop();
            }
        }
    }

    /// <summary>
    /// 输入过滤处理器
    /// </summary>
    public class InputFilterProcessor : IInputProcessor
    {
        private readonly HashSet<InputDeviceType> _blockedDeviceTypes = new();
        private readonly HashSet<Key> _blockedKeys = new();

        /// <summary>
        /// 处理器优先级
        /// </summary>
        public int Priority => 20;

        /// <summary>
        /// 处理器名称
        /// </summary>
        public string Name => "InputFilter";

        /// <summary>
        /// 阻止设备类型
        /// </summary>
        /// <param name="deviceType">设备类型</param>
        public void BlockDeviceType(InputDeviceType deviceType)
        {
            _blockedDeviceTypes.Add(deviceType);
        }

        /// <summary>
        /// 允许设备类型
        /// </summary>
        /// <param name="deviceType">设备类型</param>
        public void AllowDeviceType(InputDeviceType deviceType)
        {
            _blockedDeviceTypes.Remove(deviceType);
        }

        /// <summary>
        /// 阻止按键
        /// </summary>
        /// <param name="key">按键</param>
        public void BlockKey(Key key)
        {
            _blockedKeys.Add(key);
        }

        /// <summary>
        /// 允许按键
        /// </summary>
        /// <param name="key">按键</param>
        public void AllowKey(Key key)
        {
            _blockedKeys.Remove(key);
        }

        /// <summary>
        /// 检查是否可以处理指定事件
        /// </summary>
        public bool CanProcess(InputEvent inputEvent)
        {
            return true; // 过滤处理器处理所有事件
        }

        /// <summary>
        /// 异步处理输入事件
        /// </summary>
        public async Task<InputProcessingResult> ProcessAsync(InputEvent inputEvent, InputProcessingContext context)
        {
            // 检查设备类型过滤
            if (_blockedDeviceTypes.Contains(inputEvent.Device.DeviceType))
            {
                return InputProcessingResult.CreateStop($"设备类型 {inputEvent.Device.DeviceType} 被阻止");
            }

            // 检查按键过滤
            if (inputEvent is KeyboardEvent keyboardEvent && _blockedKeys.Contains(keyboardEvent.Key))
            {
                return InputProcessingResult.CreateStop($"按键 {keyboardEvent.Key} 被阻止");
            }

            await Task.CompletedTask;
            return InputProcessingResult.CreateSuccess("事件通过过滤");
        }
    }

    /// <summary>
    /// 输入变换处理器
    /// </summary>
    public class InputTransformProcessor : IInputProcessor
    {
        /// <summary>
        /// 处理器优先级
        /// </summary>
        public int Priority => 30;

        /// <summary>
        /// 处理器名称
        /// </summary>
        public string Name => "InputTransform";

        /// <summary>
        /// 坐标变换函数
        /// </summary>
        public Func<Point, Point>? CoordinateTransform { get; set; }

        /// <summary>
        /// 检查是否可以处理指定事件
        /// </summary>
        public bool CanProcess(InputEvent inputEvent)
        {
            return inputEvent is PointerEvent && CoordinateTransform != null;
        }

        /// <summary>
        /// 异步处理输入事件
        /// </summary>
        public async Task<InputProcessingResult> ProcessAsync(InputEvent inputEvent, InputProcessingContext context)
        {
            if (inputEvent is PointerEvent pointerEvent && CoordinateTransform != null)
            {
                var transformedPosition = CoordinateTransform(pointerEvent.Position);
                
                // 创建变换后的事件
                var transformedEvent = new PointerEvent(
                    pointerEvent.Timestamp,
                    pointerEvent.Device,
                    pointerEvent.PointerId,
                    transformedPosition,
                    pointerEvent.EventType,
                    pointerEvent.PressedButtons,
                    pointerEvent.ChangedButton);

                await Task.CompletedTask;
                return new InputProcessingResult
                {
                    Success = true,
                    ModifiedEvent = transformedEvent,
                    Message = $"坐标变换: {pointerEvent.Position} -> {transformedPosition}"
                };
            }

            await Task.CompletedTask;
            return InputProcessingResult.CreateSuccess("无需变换");
        }
    }

    /// <summary>
    /// 输入日志处理器
    /// </summary>
    public class InputLoggingProcessor : IInputProcessor
    {
        private readonly bool _logAllEvents;
        private readonly HashSet<Type> _loggedEventTypes = new();

        /// <summary>
        /// 处理器优先级
        /// </summary>
        public int Priority => 1000; // 最低优先级，最后执行

        /// <summary>
        /// 处理器名称
        /// </summary>
        public string Name => "InputLogging";

        /// <summary>
        /// 初始化输入日志处理器
        /// </summary>
        /// <param name="logAllEvents">是否记录所有事件</param>
        public InputLoggingProcessor(bool logAllEvents = false)
        {
            _logAllEvents = logAllEvents;
        }

        /// <summary>
        /// 启用事件类型日志
        /// </summary>
        /// <param name="eventType">事件类型</param>
        public void EnableLogging(Type eventType)
        {
            _loggedEventTypes.Add(eventType);
        }

        /// <summary>
        /// 禁用事件类型日志
        /// </summary>
        /// <param name="eventType">事件类型</param>
        public void DisableLogging(Type eventType)
        {
            _loggedEventTypes.Remove(eventType);
        }

        /// <summary>
        /// 检查是否可以处理指定事件
        /// </summary>
        public bool CanProcess(InputEvent inputEvent)
        {
            return _logAllEvents || _loggedEventTypes.Contains(inputEvent.GetType());
        }

        /// <summary>
        /// 异步处理输入事件
        /// </summary>
        public async Task<InputProcessingResult> ProcessAsync(InputEvent inputEvent, InputProcessingContext context)
        {
            var eventInfo = $"[Input] {inputEvent.GetType().Name} from {inputEvent.Device.Name} at {inputEvent.Timestamp}";
            
            if (inputEvent is PointerEvent pointerEvent)
            {
                eventInfo += $" - {pointerEvent.EventType} at {pointerEvent.Position}";
            }
            else if (inputEvent is KeyboardEvent keyboardEvent)
            {
                eventInfo += $" - {keyboardEvent.EventType} key {keyboardEvent.Key}";
            }

            Debug.WriteLine(eventInfo);

            await Task.CompletedTask;
            return InputProcessingResult.CreateSuccess("事件已记录");
        }
    }
}
