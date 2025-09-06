using System;
using System.Diagnostics;
using AetherUI.Input.Core;
using AetherUI.Input.Events;
using AetherUI.Input.Gestures;

namespace AetherUI.Input.Examples
{
    /// <summary>
    /// 多点触控手势示例
    /// </summary>
    public class MultiTouchGestureExample
    {
        private AdvancedGestureEngine? _gestureEngine;

        /// <summary>
        /// 运行示例
        /// </summary>
        public void Run()
        {
            Debug.WriteLine("开始多点触控手势示例");

            try
            {
                // 初始化手势引擎
                InitializeGestureEngine();

                // 测试单点手势
                TestSinglePointerGestures();

                // 测试多点手势
                TestMultiPointerGestures();

                // 测试复杂手势序列
                TestComplexGestureSequence();

                // 显示统计信息
                ShowStatistics();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"示例运行失败: {ex.Message}");
            }

            Debug.WriteLine("多点触控手势示例完成");
        }

        /// <summary>
        /// 初始化手势引擎
        /// </summary>
        private void InitializeGestureEngine()
        {
            var config = new GestureConfiguration
            {
                TapTimeoutMs = 300,
                DoubleTapIntervalMs = 500,
                LongPressDelayMs = 800,
                DragThreshold = 10.0,
                MaxTapMovement = 5.0,
                PinchThreshold = 5.0,
                RotationThreshold = Math.PI / 36, // 5度
                SwipeMinDistance = 50.0,
                SwipeMinVelocity = 100.0
            };

            _gestureEngine = new AdvancedGestureEngine(config);

            // 订阅事件
            _gestureEngine.GestureRecognized += OnGestureRecognized;
            _gestureEngine.MultiTouchGestureRecognized += OnMultiTouchGestureRecognized;

            Debug.WriteLine("手势引擎初始化完成");
        }

        /// <summary>
        /// 测试单点手势
        /// </summary>
        private void TestSinglePointerGestures()
        {
            Debug.WriteLine("\n=== 单点手势测试 ===");

            var device = new InputDevice(InputDeviceType.Touch, 0, "TouchScreen");

            // 测试点击
            Debug.WriteLine("\n--- 点击测试 ---");
            SimulatePointerEvent(PointerEventType.Pressed, new Point(100, 100), PointerId.Touch(1), device, 1000);
            SimulatePointerEvent(PointerEventType.Released, new Point(100, 100), PointerId.Touch(1), device, 1100);

            // 测试长按
            Debug.WriteLine("\n--- 长按测试 ---");
            SimulatePointerEvent(PointerEventType.Pressed, new Point(200, 200), PointerId.Touch(2), device, 2000);
            System.Threading.Thread.Sleep(900); // 模拟长按
            SimulatePointerEvent(PointerEventType.Released, new Point(200, 200), PointerId.Touch(2), device, 2900);

            // 测试拖拽
            Debug.WriteLine("\n--- 拖拽测试 ---");
            SimulatePointerEvent(PointerEventType.Pressed, new Point(300, 300), PointerId.Touch(3), device, 3000);
            SimulatePointerEvent(PointerEventType.Moved, new Point(320, 320), PointerId.Touch(3), device, 3050);
            SimulatePointerEvent(PointerEventType.Moved, new Point(350, 350), PointerId.Touch(3), device, 3100);
            SimulatePointerEvent(PointerEventType.Released, new Point(380, 380), PointerId.Touch(3), device, 3200);

            // 测试滑动
            Debug.WriteLine("\n--- 滑动测试 ---");
            SimulatePointerEvent(PointerEventType.Pressed, new Point(400, 400), PointerId.Touch(4), device, 4000);
            SimulatePointerEvent(PointerEventType.Released, new Point(500, 400), PointerId.Touch(4), device, 4100);
        }

        /// <summary>
        /// 测试多点手势
        /// </summary>
        private void TestMultiPointerGestures()
        {
            Debug.WriteLine("\n=== 多点手势测试 ===");

            var device = new InputDevice(InputDeviceType.Touch, 0, "TouchScreen");

            // 测试双指点击
            Debug.WriteLine("\n--- 双指点击测试 ---");
            SimulatePointerEvent(PointerEventType.Pressed, new Point(100, 100), PointerId.Touch(1), device, 5000);
            SimulatePointerEvent(PointerEventType.Pressed, new Point(200, 100), PointerId.Touch(2), device, 5010);
            SimulatePointerEvent(PointerEventType.Released, new Point(100, 100), PointerId.Touch(1), device, 5200);
            SimulatePointerEvent(PointerEventType.Released, new Point(200, 100), PointerId.Touch(2), device, 5210);

            // 测试捏合手势
            Debug.WriteLine("\n--- 捏合手势测试 ---");
            SimulatePointerEvent(PointerEventType.Pressed, new Point(150, 150), PointerId.Touch(1), device, 6000);
            SimulatePointerEvent(PointerEventType.Pressed, new Point(250, 150), PointerId.Touch(2), device, 6010);
            SimulatePointerEvent(PointerEventType.Moved, new Point(140, 150), PointerId.Touch(1), device, 6100);
            SimulatePointerEvent(PointerEventType.Moved, new Point(260, 150), PointerId.Touch(2), device, 6110);
            SimulatePointerEvent(PointerEventType.Moved, new Point(130, 150), PointerId.Touch(1), device, 6200);
            SimulatePointerEvent(PointerEventType.Moved, new Point(270, 150), PointerId.Touch(2), device, 6210);
            SimulatePointerEvent(PointerEventType.Released, new Point(130, 150), PointerId.Touch(1), device, 6300);
            SimulatePointerEvent(PointerEventType.Released, new Point(270, 150), PointerId.Touch(2), device, 6310);

            // 测试旋转手势
            Debug.WriteLine("\n--- 旋转手势测试 ---");
            SimulatePointerEvent(PointerEventType.Pressed, new Point(200, 200), PointerId.Touch(1), device, 7000);
            SimulatePointerEvent(PointerEventType.Pressed, new Point(300, 200), PointerId.Touch(2), device, 7010);
            SimulatePointerEvent(PointerEventType.Moved, new Point(200, 250), PointerId.Touch(1), device, 7100);
            SimulatePointerEvent(PointerEventType.Moved, new Point(250, 150), PointerId.Touch(2), device, 7110);
            SimulatePointerEvent(PointerEventType.Moved, new Point(150, 250), PointerId.Touch(1), device, 7200);
            SimulatePointerEvent(PointerEventType.Moved, new Point(250, 100), PointerId.Touch(2), device, 7210);
            SimulatePointerEvent(PointerEventType.Released, new Point(150, 250), PointerId.Touch(1), device, 7300);
            SimulatePointerEvent(PointerEventType.Released, new Point(250, 100), PointerId.Touch(2), device, 7310);
        }

        /// <summary>
        /// 测试复杂手势序列
        /// </summary>
        private void TestComplexGestureSequence()
        {
            Debug.WriteLine("\n=== 复杂手势序列测试 ===");

            var device = new InputDevice(InputDeviceType.Touch, 0, "TouchScreen");

            // 模拟复杂的多点触控序列
            Debug.WriteLine("\n--- 三指操作序列 ---");
            
            // 三个手指同时按下
            SimulatePointerEvent(PointerEventType.Pressed, new Point(100, 200), PointerId.Touch(1), device, 8000);
            SimulatePointerEvent(PointerEventType.Pressed, new Point(200, 200), PointerId.Touch(2), device, 8010);
            SimulatePointerEvent(PointerEventType.Pressed, new Point(300, 200), PointerId.Touch(3), device, 8020);

            // 同时移动
            SimulatePointerEvent(PointerEventType.Moved, new Point(110, 210), PointerId.Touch(1), device, 8100);
            SimulatePointerEvent(PointerEventType.Moved, new Point(210, 210), PointerId.Touch(2), device, 8110);
            SimulatePointerEvent(PointerEventType.Moved, new Point(310, 210), PointerId.Touch(3), device, 8120);

            // 一个手指先抬起
            SimulatePointerEvent(PointerEventType.Released, new Point(110, 210), PointerId.Touch(1), device, 8200);

            // 剩余两个手指继续移动
            SimulatePointerEvent(PointerEventType.Moved, new Point(220, 220), PointerId.Touch(2), device, 8250);
            SimulatePointerEvent(PointerEventType.Moved, new Point(320, 220), PointerId.Touch(3), device, 8260);

            // 最后两个手指抬起
            SimulatePointerEvent(PointerEventType.Released, new Point(220, 220), PointerId.Touch(2), device, 8300);
            SimulatePointerEvent(PointerEventType.Released, new Point(320, 220), PointerId.Touch(3), device, 8310);
        }

        /// <summary>
        /// 模拟指针事件
        /// </summary>
        private void SimulatePointerEvent(PointerEventType eventType, Point position, PointerId pointerId, InputDevice device, uint timestamp)
        {
            var pointerEvent = new PointerEvent(
                timestamp,
                device,
                pointerId,
                position,
                eventType,
                eventType == PointerEventType.Pressed ? PointerButton.Primary : PointerButton.None,
                eventType == PointerEventType.Pressed || eventType == PointerEventType.Released ? PointerButton.Primary : PointerButton.None);

            _gestureEngine?.ProcessPointerEvent(pointerEvent);
        }

        /// <summary>
        /// 处理手势识别事件
        /// </summary>
        private void OnGestureRecognized(object? sender, GestureRecognizedEventArgs e)
        {
            Debug.WriteLine($"[Gesture] {e.GestureType} {e.State} at {e.Position} - {e.Data}");
        }

        /// <summary>
        /// 处理多点触控手势事件
        /// </summary>
        private void OnMultiTouchGestureRecognized(object? sender, MultiTouchGestureEventArgs e)
        {
            Debug.WriteLine($"[MultiTouch] {e.GestureType} {e.State} at {e.Position} - {e.Data}");
        }

        /// <summary>
        /// 显示统计信息
        /// </summary>
        private void ShowStatistics()
        {
            Debug.WriteLine("\n=== 统计信息 ===");

            if (_gestureEngine == null)
                return;

            var stats = _gestureEngine.GetStatistics();
            Debug.WriteLine($"手势引擎统计: {stats}");

            Debug.WriteLine("\n启用的手势识别器:");
            foreach (var recognizer in _gestureEngine.GetAllRecognizers())
            {
                if (recognizer.IsEnabled)
                {
                    Debug.WriteLine($"  - {recognizer.GestureType}: {recognizer.GetCurrentState()}");
                }
            }
        }
    }

    /// <summary>
    /// 多点触控手势示例程序
    /// </summary>
    public static class MultiTouchGestureExampleProgram
    {
        /// <summary>
        /// 运行示例
        /// </summary>
        public static void RunExample()
        {
            var example = new MultiTouchGestureExample();
            example.Run();
        }
    }
}
