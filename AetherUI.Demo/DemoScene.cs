using System;
using AetherUI.Core;

namespace AetherUI.Demo
{
    /// <summary>
    /// 演示类型
    /// </summary>
    public enum DemoType
    {
        /// <summary>
        /// 基于C#代码的演示
        /// </summary>
        CodeBased,

        /// <summary>
        /// 基于XAML的演示
        /// </summary>
        XamlBased,

        /// <summary>
        /// 基于JSON的演示
        /// </summary>
        JsonBased,

        /// <summary>
        /// 交互式演示
        /// </summary>
        Interactive
    }

    /// <summary>
    /// 演示场景
    /// </summary>
    public class DemoScene
    {
        /// <summary>
        /// 演示名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 演示描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 演示类型
        /// </summary>
        public DemoType Type { get; set; }

        /// <summary>
        /// 创建UI元素的方法
        /// </summary>
        public Func<UIElement> CreateMethod { get; set; } = () => new AetherUI.Layout.StackPanel();
    }
}
