using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AetherUI.Input.Core;
using AetherUI.Input.Events;
using AetherUI.Input.Gestures;
using AetherUI.Input.HitTesting;
using AetherUI.Input.Routing;

namespace AetherUI.Input.Pipeline
{
    /// <summary>
    /// 输入管道
    /// </summary>
    public class InputPipeline : IDisposable
    {
        private readonly InputPipelineConfiguration _configuration;
        private readonly ConcurrentQueue<InputEvent> _inputQueue = new();
        private readonly List<IInputProcessor> _processors = new();
        private readonly AdvancedHitTestEngine _hitTestEngine;
        private readonly AdvancedEventRouter _eventRouter;
        private readonly AdvancedGestureEngine _gestureEngine;
        private readonly InputStatistics _statistics = new();
        
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly Task _processingTask;
        private volatile bool _isDisposed;

        /// <summary>
        /// 输入事件处理完成
        /// </summary>
        public event EventHandler<InputEventProcessedEventArgs>? InputEventProcessed;

        /// <summary>
        /// 管道错误事件
        /// </summary>
        public event EventHandler<PipelineErrorEventArgs>? PipelineError;

        /// <summary>
        /// 初始化输入管道
        /// </summary>
        /// <param name="configuration">管道配置</param>
        /// <param name="hitTestEngine">命中测试引擎</param>
        /// <param name="eventRouter">事件路由器</param>
        /// <param name="gestureEngine">手势引擎</param>
        public InputPipeline(
            InputPipelineConfiguration? configuration = null,
            AdvancedHitTestEngine? hitTestEngine = null,
            AdvancedEventRouter? eventRouter = null,
            AdvancedGestureEngine? gestureEngine = null)
        {
            _configuration = configuration ?? InputPipelineConfiguration.Default;
            _hitTestEngine = hitTestEngine ?? new AdvancedHitTestEngine();
            _eventRouter = eventRouter ?? new AdvancedEventRouter();
            _gestureEngine = gestureEngine ?? new AdvancedGestureEngine();

            // 注册默认处理器
            RegisterDefaultProcessors();

            // 启动处理任务
            _processingTask = Task.Run(ProcessInputLoop, _cancellationTokenSource.Token);

            Debug.WriteLine("输入管道已初始化");
        }

        /// <summary>
        /// 提交输入事件
        /// </summary>
        /// <param name="inputEvent">输入事件</param>
        public void SubmitEvent(InputEvent inputEvent)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(InputPipeline));

            if (inputEvent == null)
                throw new ArgumentNullException(nameof(inputEvent));

            _inputQueue.Enqueue(inputEvent);
            _statistics.TotalEventsSubmitted++;

            // 如果队列太长，发出警告
            if (_inputQueue.Count > _configuration.MaxQueueSize)
            {
                Debug.WriteLine($"警告: 输入队列过长 ({_inputQueue.Count})，可能存在性能问题");
            }
        }

        /// <summary>
        /// 批量提交输入事件
        /// </summary>
        /// <param name="inputEvents">输入事件集合</param>
        public void SubmitEvents(IEnumerable<InputEvent> inputEvents)
        {
            if (inputEvents == null)
                throw new ArgumentNullException(nameof(inputEvents));

            foreach (var inputEvent in inputEvents)
            {
                SubmitEvent(inputEvent);
            }
        }

        /// <summary>
        /// 添加输入处理器
        /// </summary>
        /// <param name="processor">输入处理器</param>
        public void AddProcessor(IInputProcessor processor)
        {
            if (processor != null && !_processors.Contains(processor))
            {
                _processors.Add(processor);
                _processors.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                Debug.WriteLine($"输入处理器已添加: {processor.GetType().Name} (优先级: {processor.Priority})");
            }
        }

        /// <summary>
        /// 移除输入处理器
        /// </summary>
        /// <param name="processor">输入处理器</param>
        public void RemoveProcessor(IInputProcessor processor)
        {
            if (_processors.Remove(processor))
            {
                Debug.WriteLine($"输入处理器已移除: {processor.GetType().Name}");
            }
        }

        /// <summary>
        /// 设置根元素
        /// </summary>
        /// <param name="rootElement">根元素</param>
        public void SetRootElement(IHitTestable rootElement)
        {
            // 这里可以设置命中测试的根元素
            Debug.WriteLine($"根元素已设置: {rootElement}");
        }

        /// <summary>
        /// 获取统计信息
        /// </summary>
        /// <returns>输入统计信息</returns>
        public InputStatistics GetStatistics()
        {
            _statistics.QueueSize = _inputQueue.Count;
            _statistics.ProcessorCount = _processors.Count;
            return _statistics;
        }

        /// <summary>
        /// 清空输入队列
        /// </summary>
        public void ClearQueue()
        {
            while (_inputQueue.TryDequeue(out _)) { }
            Debug.WriteLine("输入队列已清空");
        }

        /// <summary>
        /// 输入处理循环
        /// </summary>
        private async Task ProcessInputLoop()
        {
            Debug.WriteLine("输入处理循环已启动");

            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (_inputQueue.TryDequeue(out InputEvent? inputEvent))
                    {
                        await ProcessInputEvent(inputEvent);
                    }
                    else
                    {
                        // 没有事件时短暂等待
                        await Task.Delay(_configuration.ProcessingIntervalMs, _cancellationTokenSource.Token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("输入处理循环已取消");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"输入处理循环异常: {ex.Message}");
                OnPipelineError(new PipelineErrorEventArgs(ex, "ProcessingLoop"));
            }

            Debug.WriteLine("输入处理循环已结束");
        }

        /// <summary>
        /// 处理单个输入事件
        /// </summary>
        private async Task ProcessInputEvent(InputEvent inputEvent)
        {
            var stopwatch = Stopwatch.StartNew();
            var context = new InputProcessingContext(inputEvent);

            try
            {
                // 应用输入处理器
                foreach (var processor in _processors)
                {
                    if (processor.CanProcess(inputEvent))
                    {
                        var result = await processor.ProcessAsync(inputEvent, context);
                        context.ProcessorResults.Add(processor.GetType().Name, result);

                        if (result.ShouldStopProcessing)
                        {
                            break;
                        }
                    }
                }

                // 处理指针事件
                if (inputEvent is PointerEvent pointerEvent)
                {
                    await ProcessPointerEvent(pointerEvent, context);
                }

                // 处理键盘事件
                else if (inputEvent is KeyboardEvent keyboardEvent)
                {
                    await ProcessKeyboardEvent(keyboardEvent, context);
                }

                _statistics.TotalEventsProcessed++;
                _statistics.TotalProcessingTime += stopwatch.Elapsed;

                // 触发事件处理完成事件
                OnInputEventProcessed(new InputEventProcessedEventArgs(inputEvent, context, stopwatch.Elapsed));
            }
            catch (Exception ex)
            {
                _statistics.TotalErrors++;
                Debug.WriteLine($"处理输入事件失败: {ex.Message}");
                OnPipelineError(new PipelineErrorEventArgs(ex, "ProcessInputEvent", inputEvent));
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        /// <summary>
        /// 处理指针事件
        /// </summary>
        private async Task ProcessPointerEvent(PointerEvent pointerEvent, InputProcessingContext context)
        {
            // 手势识别
            var gestureHandled = _gestureEngine.ProcessPointerEvent(pointerEvent);
            context.Data["GestureHandled"] = gestureHandled;

            // 如果手势已处理，可能不需要继续路由
            if (gestureHandled && _configuration.StopRoutingAfterGesture)
            {
                return;
            }

            // 命中测试
            var hitTestResult = await Task.Run(() => 
                _hitTestEngine.HitTest(context.RootElement, pointerEvent.Position));

            context.HitTestResult = hitTestResult;

            if (hitTestResult.IsHit && hitTestResult.HitElement != null)
            {
                // 创建路由事件参数
                var routedEvent = CreateRoutedEvent(pointerEvent);
                if (routedEvent != null)
                {
                    var eventArgs = new RoutedEventArgs(routedEvent, hitTestResult.HitElement);
                    
                    // 路由事件
                    _eventRouter.RouteEvent(hitTestResult.HitElement, eventArgs, hitTestResult.HitPath);
                    context.Data["EventRouted"] = true;
                }
            }
        }

        /// <summary>
        /// 处理键盘事件
        /// </summary>
        private async Task ProcessKeyboardEvent(KeyboardEvent keyboardEvent, InputProcessingContext context)
        {
            // 键盘事件通常路由到焦点元素
            if (context.FocusedElement != null)
            {
                var routedEvent = CreateRoutedEvent(keyboardEvent);
                if (routedEvent != null)
                {
                    var eventArgs = new RoutedEventArgs(routedEvent, context.FocusedElement);
                    _eventRouter.RouteEvent(context.FocusedElement, eventArgs);
                    context.Data["EventRouted"] = true;
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// 创建路由事件
        /// </summary>
        private RoutedEventDefinition? CreateRoutedEvent(InputEvent inputEvent)
        {
            // 这里应该根据输入事件类型创建相应的路由事件
            // 简化实现，返回null
            return null;
        }

        /// <summary>
        /// 注册默认处理器
        /// </summary>
        private void RegisterDefaultProcessors()
        {
            AddProcessor(new InputValidationProcessor());
            AddProcessor(new InputFilterProcessor());
            AddProcessor(new InputTransformProcessor());
            AddProcessor(new InputLoggingProcessor());
        }

        /// <summary>
        /// 触发输入事件处理完成事件
        /// </summary>
        private void OnInputEventProcessed(InputEventProcessedEventArgs e)
        {
            InputEventProcessed?.Invoke(this, e);
        }

        /// <summary>
        /// 触发管道错误事件
        /// </summary>
        private void OnPipelineError(PipelineErrorEventArgs e)
        {
            PipelineError?.Invoke(this, e);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            try
            {
                _cancellationTokenSource.Cancel();
                _processingTask.Wait(TimeSpan.FromSeconds(5));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"停止输入管道失败: {ex.Message}");
            }

            _cancellationTokenSource.Dispose();
            Debug.WriteLine("输入管道已释放");
        }
    }

    /// <summary>
    /// 输入事件处理完成事件参数
    /// </summary>
    public class InputEventProcessedEventArgs : EventArgs
    {
        /// <summary>
        /// 输入事件
        /// </summary>
        public InputEvent InputEvent { get; }

        /// <summary>
        /// 处理上下文
        /// </summary>
        public InputProcessingContext Context { get; }

        /// <summary>
        /// 处理时间
        /// </summary>
        public TimeSpan ProcessingTime { get; }

        /// <summary>
        /// 初始化输入事件处理完成事件参数
        /// </summary>
        public InputEventProcessedEventArgs(InputEvent inputEvent, InputProcessingContext context, TimeSpan processingTime)
        {
            InputEvent = inputEvent ?? throw new ArgumentNullException(nameof(inputEvent));
            Context = context ?? throw new ArgumentNullException(nameof(context));
            ProcessingTime = processingTime;
        }

        public override string ToString() =>
            $"Processed {InputEvent.GetType().Name} in {ProcessingTime.TotalMilliseconds:F2}ms";
    }

    /// <summary>
    /// 管道错误事件参数
    /// </summary>
    public class PipelineErrorEventArgs : EventArgs
    {
        /// <summary>
        /// 异常
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// 错误阶段
        /// </summary>
        public string Stage { get; }

        /// <summary>
        /// 相关输入事件
        /// </summary>
        public InputEvent? InputEvent { get; }

        /// <summary>
        /// 初始化管道错误事件参数
        /// </summary>
        public PipelineErrorEventArgs(Exception exception, string stage, InputEvent? inputEvent = null)
        {
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
            Stage = stage ?? throw new ArgumentNullException(nameof(stage));
            InputEvent = inputEvent;
        }

        public override string ToString() =>
            $"Pipeline error in {Stage}: {Exception.Message}";
    }
}
