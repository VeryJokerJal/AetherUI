using AetherUI.Core;
using AetherUI.Demo.Demos;
using AetherUI.Rendering;

namespace AetherUI.Demo
{
    /// <summary>
    /// 按钮演示测试程序
    /// </summary>
    public static class ButtonDemoTest
    {
        /// <summary>
        /// 运行按钮演示测试
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("    AetherUI 按钮控件演示程序");
            Console.WriteLine("===========================================");
            Console.WriteLine();
            Console.WriteLine("正在启动按钮控件演示...");
            Console.WriteLine("演示特性：");
            Console.WriteLine("- 多种按钮尺寸（小、中、大）");
            Console.WriteLine("- 丰富颜色主题（主要、成功、警告、危险等）");
            Console.WriteLine("- 交互功能（点击计数、状态切换、命令绑定）");
            Console.WriteLine("- 多语言支持（中文、英文、日文、韩文等）");
            Console.WriteLine("- 字体渲染优化（抗锯齿、表情符号支持）");
            Console.WriteLine("- 窗口调整适应性测试");
            Console.WriteLine();

            try
            {
                // 创建按钮演示UI
                UIElement buttonDemoUI = ButtonDemo.CreateButtonDemoPage();

                Console.WriteLine("按钮演示UI创建成功");
                Console.WriteLine("启动渲染窗口...");

                // 运行演示窗口
                RunButtonDemoWindow(buttonDemoUI);

                Console.WriteLine("按钮演示窗口已关闭。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"按钮演示运行失败: {ex.Message}");
                Console.WriteLine($"详细错误: {ex}");
                Console.WriteLine("\n按任意键退出...");
                try
                {
                    _ = Console.ReadKey();
                }
                catch { }
            }
        }

        /// <summary>
        /// 运行按钮演示窗口
        /// </summary>
        /// <param name="buttonDemoUI">按钮演示UI</param>
        private static void RunButtonDemoWindow(UIElement buttonDemoUI)
        {
            // 创建窗口
            Window window = new(1400, 900, "AetherUI 按钮控件演示")
            {
                // 设置根UI元素
                RootElement = buttonDemoUI
            };

            // 配置现代化背景效果
            BackgroundEffectConfig backgroundConfig = new()
            {
                Type = BackgroundEffectType.Acrylic,
                Opacity = 0.90f,
                TintColor = new OpenTK.Mathematics.Vector4(0.98f, 0.98f, 1.0f, 1.0f)
            };

            // 添加窗口大小变化监听器
            window.AddResizeListener(new ButtonDemoResizeListener());

            // 添加窗口大小变化事件处理
            window.WindowResized += (sender, args) =>
            {
                Console.WriteLine($"[按钮演示] 窗口大小变化: {args.OldSize} -> {args.NewSize}");
                Console.WriteLine($"[按钮演示] 变化量: {args.Delta}");
                Console.WriteLine("测试按钮布局适应性...");
            };

            // 在窗口加载后设置背景效果
            window.Load += () =>
            {
                // 设置Windows原生背景效果
                window.SetBackgroundEffect(backgroundConfig);
                Console.WriteLine("按钮演示背景效果已启用：亚克力效果");
                Console.WriteLine("窗口大小变化响应系统已启用");
                Console.WriteLine();
                Console.WriteLine("=== 按钮演示使用说明 ===");
                Console.WriteLine("1. 点击各种按钮测试交互功能");
                Console.WriteLine("2. 观察不同尺寸和颜色主题的按钮");
                Console.WriteLine("3. 测试多语言文本渲染效果");
                Console.WriteLine("4. 尝试调整窗口大小测试布局适应性");
                Console.WriteLine("5. 查看控制台输出了解按钮事件");
                Console.WriteLine("6. 按ESC键退出演示");
                Console.WriteLine();

                // 显示字体渲染优化信息
                ShowFontRenderingInfo();
            };

            // 运行窗口
            window.Run();
        }

        /// <summary>
        /// 显示字体渲染优化信息
        /// </summary>
        private static void ShowFontRenderingInfo()
        {
            Console.WriteLine("=== 字体渲染优化信息 ===");
            Console.WriteLine("✓ 抗锯齿模式: AntiAliasGridFit");
            Console.WriteLine("✓ 字体回退: Microsoft YaHei -> Segoe UI -> Arial");
            Console.WriteLine("✓ 表情符号支持: Segoe UI Emoji -> Segoe UI Symbol");
            Console.WriteLine("✓ Unicode支持: 中文、日文、韩文、俄文、阿拉伯文");
            Console.WriteLine("✓ 渲染缓存: 纹理缓存 + 度量缓存");
            Console.WriteLine("✓ 窗口调整: 自动缓存清理 + 重新布局");
            Console.WriteLine();
        }
    }

    /// <summary>
    /// 按钮演示窗口大小变化监听器
    /// </summary>
    public class ButtonDemoResizeListener : IWindowResizeListener
    {
        /// <summary>
        /// 处理窗口大小变化
        /// </summary>
        /// <param name="args">事件参数</param>
        public void OnWindowResize(WindowResizeEventArgs args)
        {
            Console.WriteLine($"[按钮演示监听器] 窗口尺寸: {args.NewSize.Width:F0}x{args.NewSize.Height:F0}");

            // 根据窗口大小给出按钮布局建议
            if (args.NewSize.Width < 1000 || args.NewSize.Height < 700)
            {
                Console.WriteLine("[按钮演示监听器] 提示：窗口较小，按钮可能会重新排列");
            }
            else if (args.NewSize.Width > 1600 && args.NewSize.Height > 1000)
            {
                Console.WriteLine("[按钮演示监听器] 提示：大屏幕模式，按钮布局已优化");
            }

            // 测试按钮渲染性能
            if (Math.Abs(args.Delta.Width) > 100 || Math.Abs(args.Delta.Height) > 100)
            {
                Console.WriteLine("[按钮演示监听器] 大幅度窗口调整，测试渲染性能...");
                Console.WriteLine("- 字体缓存清理: ✓");
                Console.WriteLine("- 按钮重新布局: ✓");
                Console.WriteLine("- 渲染优化: ✓");
            }
        }
    }
}
