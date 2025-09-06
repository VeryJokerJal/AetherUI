using System;
using System.Diagnostics;
using System.Numerics;
using AetherUI.Input.Core;
using AetherUI.Input.HitTesting;

namespace AetherUI.Input.Examples
{
    /// <summary>
    /// 高级命中测试示例
    /// </summary>
    public class AdvancedHitTestExample
    {
        private AdvancedHitTestEngine? _hitTestEngine;
        private ComplexVisualElement? _rootElement;

        /// <summary>
        /// 运行示例
        /// </summary>
        public void Run()
        {
            Debug.WriteLine("开始高级命中测试示例");

            try
            {
                // 创建命中测试引擎
                var options = new HitTestOptions
                {
                    UseBoundingBoxOnly = false,
                    MaxDepth = 10
                };
                _hitTestEngine = new AdvancedHitTestEngine(options);

                // 创建复杂的可视树
                CreateComplexVisualTree();

                // 执行各种命中测试
                TestBasicHitTesting();
                TestTransformedElements();
                TestClippedElements();
                TestZOrderTesting();
                TestRegionTesting();
                TestPerformance();

                // 显示统计信息
                ShowStatistics();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"示例运行失败: {ex.Message}");
            }

            Debug.WriteLine("高级命中测试示例完成");
        }

        /// <summary>
        /// 创建复杂的可视树
        /// </summary>
        private void CreateComplexVisualTree()
        {
            // 创建根元素
            _rootElement = new ComplexVisualElement("Root", new Rect(0, 0, 1000, 800));

            // 创建容器
            var container1 = new ComplexVisualElement("Container1", new Rect(50, 50, 400, 300))
            {
                Transform = TransformUtils.CreateTranslation(10, 10),
                ZIndex = 1
            };

            var container2 = new ComplexVisualElement("Container2", new Rect(300, 200, 400, 300))
            {
                Transform = TransformUtils.CreateRotation(Math.PI / 6), // 30度旋转
                ZIndex = 2
            };

            // 创建子元素
            var button1 = new ComplexVisualElement("Button1", new Rect(20, 20, 100, 40))
            {
                ZIndex = 10
            };

            var button2 = new ComplexVisualElement("Button2", new Rect(150, 20, 100, 40))
            {
                Transform = TransformUtils.CreateScale(1.2, 1.2),
                ZIndex = 5
            };

            var textBox = new ComplexVisualElement("TextBox", new Rect(20, 80, 200, 30))
            {
                ClipBounds = new Rect(0, 0, 150, 30) // 裁剪
            };

            // 创建嵌套元素
            var nestedContainer = new ComplexVisualElement("NestedContainer", new Rect(50, 150, 200, 100))
            {
                Transform = TransformUtils.CreateRotation(Math.PI / 4, new Point(100, 50)), // 围绕中心旋转45度
                Opacity = 0.8f
            };

            var nestedButton = new ComplexVisualElement("NestedButton", new Rect(10, 10, 80, 30))
            {
                ZIndex = 20
            };

            // 构建树结构
            container1.AddChild(button1);
            container1.AddChild(button2);
            container1.AddChild(textBox);
            container1.AddChild(nestedContainer);

            nestedContainer.AddChild(nestedButton);

            var overlayButton = new ComplexVisualElement("OverlayButton", new Rect(100, 100, 80, 30))
            {
                ZIndex = 15
            };
            container2.AddChild(overlayButton);

            _rootElement.AddChild(container1);
            _rootElement.AddChild(container2);

            Debug.WriteLine("复杂可视树已创建");
            
            // 显示树统计
            var stats = VisualTreeWalker.CalculateStats(_rootElement);
            Debug.WriteLine($"可视树统计: {stats}");
        }

        /// <summary>
        /// 测试基本命中测试
        /// </summary>
        private void TestBasicHitTesting()
        {
            Debug.WriteLine("\n=== 基本命中测试 ===");

            var testPoints = new[]
            {
                new Point(80, 80),   // Button1
                new Point(200, 80),  // Button2
                new Point(100, 130), // TextBox
                new Point(500, 500), // 空白区域
                new Point(150, 250)  // NestedButton (经过变换)
            };

            foreach (var point in testPoints)
            {
                var result = _hitTestEngine!.HitTest(_rootElement, point);
                Debug.WriteLine($"点 {point}: {(result.IsHit ? $"命中 {result.HitElement}" : "未命中")}");
            }
        }

        /// <summary>
        /// 测试变换元素
        /// </summary>
        private void TestTransformedElements()
        {
            Debug.WriteLine("\n=== 变换元素测试 ===");

            // 测试旋转元素
            var rotatedPoints = new[]
            {
                new Point(400, 300), // Container2中心附近
                new Point(450, 250), // 旋转后的区域
            };

            foreach (var point in rotatedPoints)
            {
                var result = _hitTestEngine!.HitTest(_rootElement, point);
                Debug.WriteLine($"旋转测试点 {point}: {(result.IsHit ? $"命中 {result.HitElement}" : "未命中")}");
            }
        }

        /// <summary>
        /// 测试裁剪元素
        /// </summary>
        private void TestClippedElements()
        {
            Debug.WriteLine("\n=== 裁剪元素测试 ===");

            var clippedPoints = new[]
            {
                new Point(120, 130), // TextBox内部（应该命中）
                new Point(200, 130), // TextBox外部但在元素边界内（应该被裁剪）
            };

            foreach (var point in clippedPoints)
            {
                var result = _hitTestEngine!.HitTest(_rootElement, point);
                Debug.WriteLine($"裁剪测试点 {point}: {(result.IsHit ? $"命中 {result.HitElement}" : "未命中")}");
            }
        }

        /// <summary>
        /// 测试Z顺序
        /// </summary>
        private void TestZOrderTesting()
        {
            Debug.WriteLine("\n=== Z顺序测试 ===");

            // 测试重叠区域
            var overlapPoint = new Point(350, 250);
            var allResults = _hitTestEngine!.HitTestAll(_rootElement, overlapPoint);

            Debug.WriteLine($"重叠点 {overlapPoint} 的所有命中:");
            foreach (var result in allResults)
            {
                Debug.WriteLine($"  - {result.HitElement} (Z={result.HitElement?.ZIndex})");
            }
        }

        /// <summary>
        /// 测试区域测试
        /// </summary>
        private void TestRegionTesting()
        {
            Debug.WriteLine("\n=== 区域测试 ===");

            var testRegion = new Rect(100, 100, 200, 150);
            var regionResults = _hitTestEngine!.HitTestRegion(_rootElement, testRegion);

            Debug.WriteLine($"区域 {testRegion} 内的元素:");
            foreach (var result in regionResults)
            {
                Debug.WriteLine($"  - {result.HitElement}");
            }
        }

        /// <summary>
        /// 测试性能
        /// </summary>
        private void TestPerformance()
        {
            Debug.WriteLine("\n=== 性能测试 ===");

            var random = new Random();
            var testCount = 10000;
            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < testCount; i++)
            {
                var point = new Point(random.NextDouble() * 1000, random.NextDouble() * 800);
                _hitTestEngine!.HitTest(_rootElement, point);
            }

            stopwatch.Stop();
            var avgTime = stopwatch.Elapsed.TotalMilliseconds / testCount;
            Debug.WriteLine($"执行 {testCount} 次命中测试，平均耗时: {avgTime:F4}ms");
        }

        /// <summary>
        /// 显示统计信息
        /// </summary>
        private void ShowStatistics()
        {
            Debug.WriteLine("\n=== 统计信息 ===");

            var cacheStats = _hitTestEngine!.GetCacheStats();
            Debug.WriteLine($"缓存统计: 空间索引={cacheStats.spatialCount}, 变换缓存={cacheStats.transformCount}, 边界缓存={cacheStats.boundsCount}");

            // 遍历可视树
            Debug.WriteLine("\n可视树遍历 (Z顺序):");
            foreach (var element in VisualTreeWalker.Traverse(_rootElement!, VisualTreeWalker.TraversalStrategy.ZOrder))
            {
                Debug.WriteLine($"  - {element} (Z={element.ZIndex})");
            }
        }
    }

    /// <summary>
    /// 复杂可视元素
    /// </summary>
    public class ComplexVisualElement : ExampleElement
    {
        /// <summary>
        /// 初始化复杂可视元素
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="bounds">边界</param>
        public ComplexVisualElement(string name, Rect bounds) : base(name, bounds)
        {
        }

        /// <summary>
        /// 精确命中测试
        /// </summary>
        /// <param name="point">测试点</param>
        /// <returns>是否命中</returns>
        public override bool HitTest(Point point)
        {
            // 基本边界测试
            if (!Bounds.Contains(point))
                return false;

            // 模拟复杂形状测试（例如圆形）
            if (Name.Contains("Button"))
            {
                var center = Bounds.Center;
                var radius = Math.Min(Bounds.Width, Bounds.Height) / 2;
                var distance = Math.Sqrt(Math.Pow(point.X - center.X, 2) + Math.Pow(point.Y - center.Y, 2));
                return distance <= radius;
            }

            return true;
        }
    }

    /// <summary>
    /// 高级命中测试示例程序
    /// </summary>
    public static class AdvancedHitTestExampleProgram
    {
        /// <summary>
        /// 运行示例
        /// </summary>
        public static void RunExample()
        {
            var example = new AdvancedHitTestExample();
            example.Run();
        }
    }
}
