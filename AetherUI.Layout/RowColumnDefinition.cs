using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using AetherUI.Core;

namespace AetherUI.Layout
{
    /// <summary>
    /// 行定义基类
    /// </summary>
    public abstract class DefinitionBase : DependencyObject
    {
        /// <summary>
        /// 实际尺寸（内部使用）
        /// </summary>
        internal double ActualSize { get; set; }

        /// <summary>
        /// 最小尺寸（内部使用）
        /// </summary>
        internal double MinSize { get; set; }

        /// <summary>
        /// 最大尺寸（内部使用）
        /// </summary>
        internal double MaxSize { get; set; } = double.PositiveInfinity;

        /// <summary>
        /// 星号权重（内部使用）
        /// </summary>
        internal double StarWeight { get; set; }
    }

    /// <summary>
    /// 行定义
    /// </summary>
    public class RowDefinition : DefinitionBase
    {
        #region 依赖属性

        /// <summary>
        /// 高度依赖属性
        /// </summary>
        public static readonly DependencyProperty HeightProperty = DependencyProperty.Register(
            nameof(Height), typeof(GridLength), typeof(RowDefinition),
            new PropertyMetadata(GridLength.Star(), OnHeightChanged));

        /// <summary>
        /// 最小高度依赖属性
        /// </summary>
        public static readonly DependencyProperty MinHeightProperty = DependencyProperty.Register(
            nameof(MinHeight), typeof(double), typeof(RowDefinition),
            new PropertyMetadata(0.0, OnMinHeightChanged));

        /// <summary>
        /// 最大高度依赖属性
        /// </summary>
        public static readonly DependencyProperty MaxHeightProperty = DependencyProperty.Register(
            nameof(MaxHeight), typeof(double), typeof(RowDefinition),
            new PropertyMetadata(double.PositiveInfinity, OnMaxHeightChanged));

        #endregion

        #region 属性

        /// <summary>
        /// 行高度
        /// </summary>
        public GridLength Height
        {
            get => (GridLength)(GetValue(HeightProperty) ?? GridLength.Star());
            set => SetValue(HeightProperty, value);
        }

        /// <summary>
        /// 最小高度
        /// </summary>
        public double MinHeight
        {
            get => (double)(GetValue(MinHeightProperty) ?? 0.0);
            set => SetValue(MinHeightProperty, value);
        }

        /// <summary>
        /// 最大高度
        /// </summary>
        public double MaxHeight
        {
            get => (double)(GetValue(MaxHeightProperty) ?? double.PositiveInfinity);
            set => SetValue(MaxHeightProperty, value);
        }

        /// <summary>
        /// 实际高度
        /// </summary>
        public double ActualHeight => ActualSize;

        #endregion

        #region 属性更改回调

        private static void OnHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RowDefinition row)
            {
}
        }

        private static void OnMinHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RowDefinition row)
            {
                row.MinSize = (double)(e.NewValue ?? 0.0);
}
        }

        private static void OnMaxHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RowDefinition row)
            {
                row.MaxSize = (double)(e.NewValue ?? double.PositiveInfinity);
}
        }

        #endregion
    }

    /// <summary>
    /// 列定义
    /// </summary>
    public class ColumnDefinition : DefinitionBase
    {
        #region 依赖属性

        /// <summary>
        /// 宽度依赖属性
        /// </summary>
        public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(
            nameof(Width), typeof(GridLength), typeof(ColumnDefinition),
            new PropertyMetadata(GridLength.Star(), OnWidthChanged));

        /// <summary>
        /// 最小宽度依赖属性
        /// </summary>
        public static readonly DependencyProperty MinWidthProperty = DependencyProperty.Register(
            nameof(MinWidth), typeof(double), typeof(ColumnDefinition),
            new PropertyMetadata(0.0, OnMinWidthChanged));

        /// <summary>
        /// 最大宽度依赖属性
        /// </summary>
        public static readonly DependencyProperty MaxWidthProperty = DependencyProperty.Register(
            nameof(MaxWidth), typeof(double), typeof(ColumnDefinition),
            new PropertyMetadata(double.PositiveInfinity, OnMaxWidthChanged));

        #endregion

        #region 属性

        /// <summary>
        /// 列宽度
        /// </summary>
        public GridLength Width
        {
            get => (GridLength)(GetValue(WidthProperty) ?? GridLength.Star());
            set => SetValue(WidthProperty, value);
        }

        /// <summary>
        /// 最小宽度
        /// </summary>
        public double MinWidth
        {
            get => (double)(GetValue(MinWidthProperty) ?? 0.0);
            set => SetValue(MinWidthProperty, value);
        }

        /// <summary>
        /// 最大宽度
        /// </summary>
        public double MaxWidth
        {
            get => (double)(GetValue(MaxWidthProperty) ?? double.PositiveInfinity);
            set => SetValue(MaxWidthProperty, value);
        }

        /// <summary>
        /// 实际宽度
        /// </summary>
        public double ActualWidth => ActualSize;

        #endregion

        #region 属性更改回调

        private static void OnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColumnDefinition column)
            {
}
        }

        private static void OnMinWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColumnDefinition column)
            {
                column.MinSize = (double)(e.NewValue ?? 0.0);
}
        }

        private static void OnMaxWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColumnDefinition column)
            {
                column.MaxSize = (double)(e.NewValue ?? double.PositiveInfinity);
}
        }

        #endregion
    }

    /// <summary>
    /// 行定义集合
    /// </summary>
    public class RowDefinitionCollection : Collection<RowDefinition>
    {
        private readonly Grid _owner;

        /// <summary>
        /// 初始化行定义集合
        /// </summary>
        /// <param name="owner">拥有者Grid</param>
        public RowDefinitionCollection(Grid owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        protected override void InsertItem(int index, RowDefinition item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            base.InsertItem(index, item);
            _owner.InvalidateMeasure();
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            _owner.InvalidateMeasure();
        }

        protected override void SetItem(int index, RowDefinition item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            base.SetItem(index, item);
            _owner.InvalidateMeasure();
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            _owner.InvalidateMeasure();
        }
    }

    /// <summary>
    /// 列定义集合
    /// </summary>
    public class ColumnDefinitionCollection : Collection<ColumnDefinition>
    {
        private readonly Grid _owner;

        /// <summary>
        /// 初始化列定义集合
        /// </summary>
        /// <param name="owner">拥有者Grid</param>
        public ColumnDefinitionCollection(Grid owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        protected override void InsertItem(int index, ColumnDefinition item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            base.InsertItem(index, item);
            _owner.InvalidateMeasure();
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            _owner.InvalidateMeasure();
        }

        protected override void SetItem(int index, ColumnDefinition item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            base.SetItem(index, item);
            _owner.InvalidateMeasure();
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            _owner.InvalidateMeasure();
        }
    }
}
