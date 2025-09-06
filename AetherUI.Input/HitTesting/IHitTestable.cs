using System;
using System.Collections.Generic;
using System.Numerics;
using AetherUI.Input.Core;

namespace AetherUI.Input.HitTesting
{
    /// <summary>
    /// 可命中测试的元素接口
    /// </summary>
    public interface IHitTestable
    {
        /// <summary>
        /// 是否可见
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// 是否可命中测试
        /// </summary>
        bool IsHitTestVisible { get; }

        /// <summary>
        /// 是否启用
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// 元素边界（本地坐标系）
        /// </summary>
        Rect Bounds { get; }

        /// <summary>
        /// 渲染边界（包含变换后的边界）
        /// </summary>
        Rect RenderBounds { get; }

        /// <summary>
        /// 变换矩阵（本地到父级）
        /// </summary>
        Matrix4x4 Transform { get; }

        /// <summary>
        /// 不透明度
        /// </summary>
        float Opacity { get; }

        /// <summary>
        /// Z顺序
        /// </summary>
        int ZIndex { get; }

        /// <summary>
        /// 裁剪区域
        /// </summary>
        Rect? ClipBounds { get; }

        /// <summary>
        /// 父元素
        /// </summary>
        IHitTestable? Parent { get; }

        /// <summary>
        /// 子元素集合
        /// </summary>
        IEnumerable<IHitTestable> Children { get; }

        /// <summary>
        /// 检查点是否在元素内（本地坐标系）
        /// </summary>
        /// <param name="point">点（本地坐标）</param>
        /// <returns>是否命中</returns>
        bool HitTest(Point point);

        /// <summary>
        /// 获取元素到根的变换矩阵
        /// </summary>
        /// <returns>变换矩阵</returns>
        Matrix4x4 GetTransformToRoot();

        /// <summary>
        /// 获取从根到元素的变换矩阵
        /// </summary>
        /// <returns>变换矩阵</returns>
        Matrix4x4 GetTransformFromRoot();
    }

    /// <summary>
    /// 命中测试结果
    /// </summary>
    public class HitTestResult
    {
        /// <summary>
        /// 命中的元素
        /// </summary>
        public IHitTestable? HitElement { get; }

        /// <summary>
        /// 命中点（全局坐标）
        /// </summary>
        public Point GlobalPoint { get; }

        /// <summary>
        /// 命中点（元素本地坐标）
        /// </summary>
        public Point LocalPoint { get; }

        /// <summary>
        /// 命中路径（从根到命中元素）
        /// </summary>
        public IReadOnlyList<IHitTestable> HitPath { get; }

        /// <summary>
        /// 是否命中
        /// </summary>
        public bool IsHit => HitElement != null;

        /// <summary>
        /// 初始化命中测试结果
        /// </summary>
        /// <param name="hitElement">命中的元素</param>
        /// <param name="globalPoint">命中点（全局坐标）</param>
        /// <param name="localPoint">命中点（元素本地坐标）</param>
        /// <param name="hitPath">命中路径</param>
        public HitTestResult(IHitTestable? hitElement, Point globalPoint, Point localPoint, IReadOnlyList<IHitTestable> hitPath)
        {
            HitElement = hitElement;
            GlobalPoint = globalPoint;
            LocalPoint = localPoint;
            HitPath = hitPath ?? Array.Empty<IHitTestable>();
        }

        /// <summary>
        /// 空的命中测试结果
        /// </summary>
        /// <param name="globalPoint">全局点</param>
        /// <returns>空结果</returns>
        public static HitTestResult Empty(Point globalPoint) =>
            new(null, globalPoint, globalPoint, Array.Empty<IHitTestable>());

        public override string ToString() =>
            IsHit ? $"Hit: {HitElement} at {LocalPoint}" : $"Miss at {GlobalPoint}";
    }

    /// <summary>
    /// 命中测试引擎接口
    /// </summary>
    public interface IHitTestEngine
    {
        /// <summary>
        /// 执行命中测试
        /// </summary>
        /// <param name="root">根元素</param>
        /// <param name="point">测试点（全局坐标）</param>
        /// <returns>命中测试结果</returns>
        HitTestResult HitTest(IHitTestable? root, Point point);

        /// <summary>
        /// 执行命中测试，返回所有命中的元素
        /// </summary>
        /// <param name="root">根元素</param>
        /// <param name="point">测试点（全局坐标）</param>
        /// <returns>所有命中的元素（从最上层到最下层）</returns>
        IEnumerable<HitTestResult> HitTestAll(IHitTestable? root, Point point);

        /// <summary>
        /// 执行区域命中测试
        /// </summary>
        /// <param name="root">根元素</param>
        /// <param name="rect">测试区域（全局坐标）</param>
        /// <returns>命中的元素集合</returns>
        IEnumerable<HitTestResult> HitTestRegion(IHitTestable? root, Rect rect);
    }

    /// <summary>
    /// 命中测试过滤器
    /// </summary>
    public interface IHitTestFilter
    {
        /// <summary>
        /// 检查元素是否应该参与命中测试
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>是否参与命中测试</returns>
        bool ShouldHitTest(IHitTestable element);

        /// <summary>
        /// 检查元素的子元素是否应该参与命中测试
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>是否测试子元素</returns>
        bool ShouldHitTestChildren(IHitTestable element);
    }

    /// <summary>
    /// 默认命中测试过滤器
    /// </summary>
    public class DefaultHitTestFilter : IHitTestFilter
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        public static DefaultHitTestFilter Instance { get; } = new();

        private DefaultHitTestFilter() { }

        /// <summary>
        /// 检查元素是否应该参与命中测试
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>是否参与命中测试</returns>
        public bool ShouldHitTest(IHitTestable element)
        {
            return element.IsVisible && element.IsHitTestVisible && element.Opacity > 0;
        }

        /// <summary>
        /// 检查元素的子元素是否应该参与命中测试
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>是否测试子元素</returns>
        public bool ShouldHitTestChildren(IHitTestable element)
        {
            return element.IsVisible && element.Opacity > 0;
        }
    }

    /// <summary>
    /// 命中测试选项
    /// </summary>
    public class HitTestOptions
    {
        /// <summary>
        /// 命中测试过滤器
        /// </summary>
        public IHitTestFilter Filter { get; set; } = DefaultHitTestFilter.Instance;

        /// <summary>
        /// 是否包含禁用的元素
        /// </summary>
        public bool IncludeDisabled { get; set; } = false;

        /// <summary>
        /// 是否包含不可见的元素
        /// </summary>
        public bool IncludeInvisible { get; set; } = false;

        /// <summary>
        /// 最大命中深度
        /// </summary>
        public int MaxDepth { get; set; } = int.MaxValue;

        /// <summary>
        /// 是否使用边界框测试（快速但不精确）
        /// </summary>
        public bool UseBoundingBoxOnly { get; set; } = false;

        /// <summary>
        /// 默认选项
        /// </summary>
        public static HitTestOptions Default { get; } = new();
    }

    /// <summary>
    /// 命中测试上下文
    /// </summary>
    public class HitTestContext
    {
        /// <summary>
        /// 测试点（全局坐标）
        /// </summary>
        public Point GlobalPoint { get; }

        /// <summary>
        /// 当前变换矩阵（全局到当前元素）
        /// </summary>
        public Matrix4x4 Transform { get; set; }

        /// <summary>
        /// 当前裁剪区域
        /// </summary>
        public Rect? ClipBounds { get; set; }

        /// <summary>
        /// 当前深度
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// 命中路径
        /// </summary>
        public List<IHitTestable> HitPath { get; }

        /// <summary>
        /// 测试选项
        /// </summary>
        public HitTestOptions Options { get; }

        /// <summary>
        /// 初始化命中测试上下文
        /// </summary>
        /// <param name="globalPoint">测试点</param>
        /// <param name="options">测试选项</param>
        public HitTestContext(Point globalPoint, HitTestOptions? options = null)
        {
            GlobalPoint = globalPoint;
            Transform = Matrix4x4.Identity;
            HitPath = new List<IHitTestable>();
            Options = options ?? HitTestOptions.Default;
        }

        /// <summary>
        /// 获取当前点的本地坐标
        /// </summary>
        /// <returns>本地坐标点</returns>
        public Point GetLocalPoint()
        {
            var globalVector = new Vector4((float)GlobalPoint.X, (float)GlobalPoint.Y, 0, 1);
            var localVector = Vector4.Transform(globalVector, Transform);
            return new Point(localVector.X, localVector.Y);
        }

        /// <summary>
        /// 推入元素到路径
        /// </summary>
        /// <param name="element">元素</param>
        public void PushElement(IHitTestable element)
        {
            HitPath.Add(element);
            Depth++;
        }

        /// <summary>
        /// 弹出元素从路径
        /// </summary>
        public void PopElement()
        {
            if (HitPath.Count > 0)
            {
                HitPath.RemoveAt(HitPath.Count - 1);
                Depth--;
            }
        }
    }
}
