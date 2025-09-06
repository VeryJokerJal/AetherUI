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
            RenderElementRecursive(element, Matrix4.Identity);
        }

        /// <summary>
        /// 递归渲染元素
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="parentTransform">父元素的变换矩阵</param>
        private void RenderElementRecursive(UIElement element, Matrix4 parentTransform)
        {
            if (element.Visibility == Visibility.Collapsed)
                return;

            // 获取元素的布局位置
            Rect layoutRect = element.LayoutRect;

            // 计算元素的变换矩阵（包含位置偏移）
            Matrix4 elementTransform = parentTransform * Matrix4.CreateTranslation((float)layoutRect.X, (float)layoutRect.Y, 0);

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
            // 使用元素的渲染尺寸，位置信息已经包含在变换矩阵中
            Rect bounds = new Rect(0, 0, element.RenderSize.Width, element.RenderSize.Height);

            // 保存当前的模型矩阵
            Matrix4 previousModelMatrix = _renderContext.ModelMatrix;

            // 应用元素的变换矩阵
            _renderContext.ModelMatrix = transform;

            try
            {
                // 开始批量渲染
                _geometryRenderer.BeginBatch();

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

                // 结束批量渲染，使用当前的MVP矩阵
                _geometryRenderer.EndBatch(_renderContext.MVPMatrix);
            }
            finally
            {
                // 恢复之前的模型矩阵
                _renderContext.ModelMatrix = previousModelMatrix;
            }
        }

        /// <summary>
        /// 渲染按钮
        /// </summary>
        /// <param name="button">按钮</param>
        /// <param name="bounds">边界</param>
        private void RenderButton(Button button, Rect bounds)
        {
            string backgroundColorString = button.Background;
            string borderColorString = button.BorderBrush;
            double cornerRadius = button.CornerRadius;
            Vector4 backgroundColor = ParseColorToVector4(backgroundColorString);
            Vector4 borderColor = ParseColorToVector4(borderColorString);

            // 渲染按钮背景（使用圆角）
            _geometryRenderer.DrawRoundedRect(bounds, backgroundColor, (float)cornerRadius);

            // 渲染按钮边框
            _geometryRenderer.DrawRectBorder(bounds, borderColor, 1);

            // 渲染按钮文本内容
            if (button.Content != null)
            {
                RenderButtonContent(button, bounds);
            }
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
}
            catch (Exception ex)
            {
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
}

        /// <summary>
        /// 渲染按钮内容
        /// </summary>
        /// <param name="button">按钮</param>
        /// <param name="bounds">边界</param>
        private void RenderButtonContent(Button button, Rect bounds)
        {
            if (button.Content == null)
                return;

            string contentText = button.Content.ToString() ?? "";
            if (string.IsNullOrEmpty(contentText))
                return;

            try
            {
                // 获取按钮的内边距
                Thickness padding = button.Padding;

                // 计算内容区域
                Rect contentBounds = new Rect(
                    bounds.X + padding.Left,
                    bounds.Y + padding.Top,
                    Math.Max(0, bounds.Width - padding.Horizontal),
                    Math.Max(0, bounds.Height - padding.Vertical));

                // 创建字体信息（使用按钮的前景色）
                FontInfo fontInfo = new FontInfo(
                    "Microsoft YaHei",  // 默认字体
                    14.0,               // 默认字体大小
                    FontWeight.Normal,  // 默认字体粗细
                    FontStyle.Normal,   // 默认字体样式
                    button.Foreground); // 使用按钮的前景色

                // 测量文本尺寸
                TextMetrics metrics = _fontRenderer.GetTextMetrics(contentText, fontInfo);

                // 计算文本位置（居中对齐）
                double textX = contentBounds.X + Math.Max(0, (contentBounds.Width - metrics.Width) / 2);
                double textY = contentBounds.Y + Math.Max(0, (contentBounds.Height - metrics.Height) / 2);

                // 渲染文本
                _fontRenderer.RenderText(
                    contentText,
                    fontInfo,
                    new Vector2((float)textX, (float)textY),
                    _renderContext.MVPMatrix);
}
            catch (Exception ex)
            {
}
        }

        /// <summary>
        /// 解析颜色字符串为Vector4
        /// </summary>
        /// <param name="colorString">颜色字符串（支持十六进制格式如 #3498DB 或颜色名称）</param>
        /// <returns>Vector4颜色值</returns>
        private Vector4 ParseColorToVector4(string colorString)
        {
            try
            {
                System.Drawing.Color color;

                if (colorString.StartsWith("#"))
                {
                    // 解析十六进制颜色
                    color = System.Drawing.ColorTranslator.FromHtml(colorString);
                }
                else
                {
                    // 解析颜色名称
                    color = System.Drawing.Color.FromName(colorString);
                }

                // 转换为Vector4（归一化到0-1范围）
                return new Vector4(
                    color.R / 255.0f,
                    color.G / 255.0f,
                    color.B / 255.0f,
                    color.A / 255.0f);
            }
            catch (Exception ex)
            {
                return new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            }
        }

        /// <summary>
        /// 清理渲染缓存
        /// </summary>
        public void ClearCaches()
        {
            try
            {
                _fontRenderer?.ClearCaches();

                // 几何渲染器通常不需要清理缓存，因为它是即时渲染的
}
            catch (Exception ex)
            {
}
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
_fontRenderer?.Dispose();
                    _geometryRenderer?.Dispose();
                    _shaderManager?.Dispose();
}

                _disposed = true;
            }
        }

        #endregion
    }
}
