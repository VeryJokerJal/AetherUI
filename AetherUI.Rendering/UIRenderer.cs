using System;
using System.Diagnostics;
using OpenTK.Mathematics;
using AetherUI.Core;
using AetherUI.Layout;

namespace AetherUI.Rendering
{
    /// <summary>
    /// UI渲染器，负责渲染UI元素树
    /// </summary>
    public class UIRenderer : IDisposable
    {
        private readonly RenderContext _renderContext;
        private readonly ShaderManager _shaderManager;
        private readonly GeometryRenderer _geometryRenderer;
        private readonly FontRenderer _fontRenderer;
        private bool _disposed = false;

        #region 属性

        /// <summary>
        /// 着色器管理器
        /// </summary>
        public ShaderManager ShaderManager => _shaderManager;

        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化UI渲染器
        /// </summary>
        /// <param name="renderContext">渲染上下文</param>
        public UIRenderer(RenderContext renderContext)
        {
            _renderContext = renderContext ?? throw new ArgumentNullException(nameof(renderContext));
            
            // 创建着色器管理器
            _shaderManager = new ShaderManager();
            _shaderManager.CreateDefaultShaders();

            // 创建几何渲染器
            _geometryRenderer = new GeometryRenderer(_shaderManager);

            // 创建字体渲染器
            _fontRenderer = new FontRenderer(_shaderManager);

            Debug.WriteLine("UIRenderer initialized");
        }

        #endregion

        #region 渲染方法

        /// <summary>
        /// 渲染UI元素
        /// </summary>
        /// <param name="element">要渲染的元素</param>
        public void RenderElement(UIElement element)
        {
            if (element == null || element.Visibility == Visibility.Collapsed)
                return;

            Debug.WriteLine($"Rendering element: {element.GetType().Name}");

            // 开始批量渲染
            _geometryRenderer.BeginBatch();

            // 渲染元素及其子元素
            RenderElementRecursive(element, Matrix4.Identity);

            // 结束批量渲染
            _geometryRenderer.EndBatch(_renderContext.MVPMatrix);
        }

        /// <summary>
        /// 递归渲染元素
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="transform">变换矩阵</param>
        private void RenderElementRecursive(UIElement element, Matrix4 transform)
        {
            if (element.Visibility == Visibility.Collapsed)
                return;

            // 计算元素的变换矩阵
            Matrix4 elementTransform = transform;

            // 渲染元素本身
            RenderElementVisual(element, elementTransform);

            // 渲染子元素
            if (element is Panel panel)
            {
                foreach (UIElement child in panel.Children)
                {
                    RenderElementRecursive(child, elementTransform);
                }
            }
            else if (element is Border border && border.Child != null)
            {
                RenderElementRecursive(border.Child, elementTransform);
            }
            else if (element is Card card)
            {
                if (card.Header != null)
                    RenderElementRecursive(card.Header, elementTransform);
                if (card.Content != null)
                    RenderElementRecursive(card.Content, elementTransform);
                if (card.Footer != null)
                    RenderElementRecursive(card.Footer, elementTransform);
            }
        }

        /// <summary>
        /// 渲染元素的视觉效果
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="transform">变换矩阵</param>
        private void RenderElementVisual(UIElement element, Matrix4 transform)
        {
            Rect bounds = new Rect(0, 0, element.RenderSize.Width, element.RenderSize.Height);

            switch (element)
            {
                case Button button:
                    RenderButton(button, bounds);
                    break;

                case TextBlock textBlock:
                    RenderTextBlock(textBlock, bounds);
                    break;

                case Border border:
                    RenderBorder(border, bounds);
                    break;

                case Card card:
                    RenderCard(card, bounds);
                    break;

                case Panel panel:
                    RenderPanel(panel, bounds);
                    break;

                default:
                    RenderDefaultElement(element, bounds);
                    break;
            }
        }

        /// <summary>
        /// 渲染按钮
        /// </summary>
        /// <param name="button">按钮</param>
        /// <param name="bounds">边界</param>
        private void RenderButton(Button button, Rect bounds)
        {
            // 按钮背景
            Vector4 backgroundColor = new Vector4(0.3f, 0.6f, 1.0f, 0.8f);
            _geometryRenderer.DrawRoundedRect(bounds, backgroundColor, 4);

            // 按钮边框
            Vector4 borderColor = new Vector4(0.2f, 0.4f, 0.8f, 1.0f);
            _geometryRenderer.DrawRectBorder(bounds, borderColor, 1);

            Debug.WriteLine($"Rendered Button: {bounds}");
        }

        /// <summary>
        /// 渲染文本块
        /// </summary>
        /// <param name="textBlock">文本块</param>
        /// <param name="bounds">边界</param>
        private void RenderTextBlock(TextBlock textBlock, Rect bounds)
        {
            if (string.IsNullOrEmpty(textBlock.Text))
                return;

            try
            {
                // 创建字体信息
                var fontInfo = new FontInfo(
                    textBlock.FontFamily,
                    textBlock.FontSize,
                    textBlock.FontWeight,
                    textBlock.FontStyle,
                    textBlock.Foreground);

                // 测量文本尺寸
                var metrics = _fontRenderer.GetTextMetrics(textBlock.Text, fontInfo);

                // 计算文本位置（居中对齐）
                double textX = bounds.X + Math.Max(0, (bounds.Width - metrics.Width) / 2);
                double textY = bounds.Y + Math.Max(0, (bounds.Height - metrics.Height) / 2);

                // 渲染文本
                _fontRenderer.RenderText(
                    textBlock.Text,
                    fontInfo,
                    new Vector2((float)textX, (float)textY),
                    _renderContext.MVPMatrix);

                Debug.WriteLine($"Rendered TextBlock: '{textBlock.Text}' at {bounds} with font {fontInfo.Family} {fontInfo.Size}px");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error rendering TextBlock '{textBlock.Text}': {ex.Message}");

                // 降级到简单矩形渲染
                Vector4 textColor = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
                double textHeight = Math.Min(bounds.Height * 0.6, textBlock.FontSize);
                Rect textRect = new Rect(
                    bounds.X + 2,
                    bounds.Y + (bounds.Height - textHeight) / 2,
                    bounds.Width - 4,
                    textHeight);

                _geometryRenderer.DrawRect(textRect, textColor);
            }
        }

        /// <summary>
        /// 渲染边框
        /// </summary>
        /// <param name="border">边框</param>
        /// <param name="bounds">边界</param>
        private void RenderBorder(Border border, Rect bounds)
        {
            // 背景
            if (border.Background != null)
            {
                Vector4 backgroundColor = new Vector4(0.95f, 0.95f, 0.95f, 0.8f);
                _geometryRenderer.DrawRoundedRect(bounds, backgroundColor, border.CornerRadius);
            }

            // 边框
            if (border.BorderThickness.Left > 0 || border.BorderThickness.Top > 0 || 
                border.BorderThickness.Right > 0 || border.BorderThickness.Bottom > 0)
            {
                Vector4 borderColor = new Vector4(0.6f, 0.6f, 0.6f, 1.0f);
                double thickness = Math.Max(Math.Max(border.BorderThickness.Left, border.BorderThickness.Top),
                                          Math.Max(border.BorderThickness.Right, border.BorderThickness.Bottom));
                _geometryRenderer.DrawRectBorder(bounds, borderColor, thickness);
            }

            Debug.WriteLine($"Rendered Border: {bounds}");
        }

        /// <summary>
        /// 渲染卡片
        /// </summary>
        /// <param name="card">卡片</param>
        /// <param name="bounds">边界</param>
        private void RenderCard(Card card, Rect bounds)
        {
            // 卡片阴影（简化）
            Vector4 shadowColor = new Vector4(0.0f, 0.0f, 0.0f, 0.1f);
            Rect shadowBounds = new Rect(bounds.X + card.Elevation, bounds.Y + card.Elevation, 
                                       bounds.Width, bounds.Height);
            _geometryRenderer.DrawRoundedRect(shadowBounds, shadowColor, card.CornerRadius);

            // 卡片背景
            Vector4 backgroundColor = new Vector4(1.0f, 1.0f, 1.0f, 0.95f);
            _geometryRenderer.DrawRoundedRect(bounds, backgroundColor, card.CornerRadius);

            // 卡片边框
            Vector4 borderColor = new Vector4(0.9f, 0.9f, 0.9f, 1.0f);
            _geometryRenderer.DrawRectBorder(bounds, borderColor, 1);

            Debug.WriteLine($"Rendered Card: {bounds}");
        }

        /// <summary>
        /// 渲染面板
        /// </summary>
        /// <param name="panel">面板</param>
        /// <param name="bounds">边界</param>
        private void RenderPanel(Panel panel, Rect bounds)
        {
            // 面板背景（半透明）
            Vector4 backgroundColor = panel.GetType().Name switch
            {
                "StackPanel" => new Vector4(1.0f, 0.8f, 0.6f, 0.2f),
                "Grid" => new Vector4(0.8f, 1.0f, 0.8f, 0.2f),
                "Canvas" => new Vector4(1.0f, 0.9f, 0.9f, 0.2f),
                "DockPanel" => new Vector4(0.9f, 0.9f, 1.0f, 0.2f),
                "WrapPanel" => new Vector4(0.9f, 1.0f, 0.9f, 0.2f),
                "UniformGrid" => new Vector4(1.0f, 0.9f, 1.0f, 0.2f),
                _ => new Vector4(0.8f, 0.8f, 0.8f, 0.2f)
            };

            _geometryRenderer.DrawRect(bounds, backgroundColor);

            Debug.WriteLine($"Rendered Panel ({panel.GetType().Name}): {bounds}");
        }

        /// <summary>
        /// 渲染默认元素
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="bounds">边界</param>
        private void RenderDefaultElement(UIElement element, Rect bounds)
        {
            // 默认元素背景
            Vector4 backgroundColor = new Vector4(0.7f, 0.7f, 0.7f, 0.3f);
            _geometryRenderer.DrawRect(bounds, backgroundColor);

            // 默认元素边框
            Vector4 borderColor = new Vector4(0.5f, 0.5f, 0.5f, 0.8f);
            _geometryRenderer.DrawRectBorder(bounds, borderColor, 1);

            Debug.WriteLine($"Rendered Default Element ({element.GetType().Name}): {bounds}");
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否正在释放</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Debug.WriteLine("Disposing UIRenderer...");

                    _fontRenderer?.Dispose();
                    _geometryRenderer?.Dispose();
                    _shaderManager?.Dispose();

                    Debug.WriteLine("UIRenderer disposed");
                }

                _disposed = true;
            }
        }

        #endregion
    }
}
