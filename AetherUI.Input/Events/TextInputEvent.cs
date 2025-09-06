using System;
using AetherUI.Input.Core;

namespace AetherUI.Input.Events
{
    /// <summary>
    /// 文本输入事件类型
    /// </summary>
    public enum TextInputEventType
    {
        /// <summary>
        /// 文本输入
        /// </summary>
        TextInput,

        /// <summary>
        /// 组合开始
        /// </summary>
        CompositionStart,

        /// <summary>
        /// 组合更新
        /// </summary>
        CompositionUpdate,

        /// <summary>
        /// 组合结束
        /// </summary>
        CompositionEnd,

        /// <summary>
        /// 组合取消
        /// </summary>
        CompositionCancel
    }

    /// <summary>
    /// 文本输入事件
    /// </summary>
    public class TextInputEvent : InputEvent
    {
        /// <summary>
        /// 事件类型
        /// </summary>
        public TextInputEventType EventType { get; }

        /// <summary>
        /// 输入的文本
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// 组合文本（IME）
        /// </summary>
        public string? CompositionText { get; }

        /// <summary>
        /// 组合选择范围
        /// </summary>
        public TextRange? CompositionSelection { get; }

        /// <summary>
        /// 光标位置
        /// </summary>
        public int CursorPosition { get; }

        /// <summary>
        /// 是否可替换
        /// </summary>
        public bool IsReplaceable { get; }

        /// <summary>
        /// 初始化文本输入事件
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <param name="device">输入设备</param>
        /// <param name="eventType">事件类型</param>
        /// <param name="text">输入文本</param>
        /// <param name="compositionText">组合文本</param>
        /// <param name="compositionSelection">组合选择范围</param>
        /// <param name="cursorPosition">光标位置</param>
        /// <param name="isReplaceable">是否可替换</param>
        public TextInputEvent(
            uint timestamp,
            InputDevice device,
            TextInputEventType eventType,
            string text,
            string? compositionText = null,
            TextRange? compositionSelection = null,
            int cursorPosition = -1,
            bool isReplaceable = false)
            : base(timestamp, device)
        {
            EventType = eventType;
            Text = text ?? throw new ArgumentNullException(nameof(text));
            CompositionText = compositionText;
            CompositionSelection = compositionSelection;
            CursorPosition = cursorPosition;
            IsReplaceable = isReplaceable;
        }

        public override string ToString() =>
            $"TextInput: {EventType} '{Text}'" +
            (CompositionText != null ? $" Composition: '{CompositionText}'" : "") +
            (CursorPosition >= 0 ? $" Cursor: {CursorPosition}" : "");
    }

    /// <summary>
    /// 文本范围
    /// </summary>
    public readonly struct TextRange : IEquatable<TextRange>
    {
        /// <summary>
        /// 起始位置
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// 长度
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// 结束位置
        /// </summary>
        public int End => Start + Length;

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => Length == 0;

        /// <summary>
        /// 初始化文本范围
        /// </summary>
        /// <param name="start">起始位置</param>
        /// <param name="length">长度</param>
        public TextRange(int start, int length)
        {
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            Start = start;
            Length = length;
        }

        /// <summary>
        /// 创建从起始到结束位置的范围
        /// </summary>
        /// <param name="start">起始位置</param>
        /// <param name="end">结束位置</param>
        /// <returns>文本范围</returns>
        public static TextRange FromStartEnd(int start, int end)
        {
            if (end < start)
                throw new ArgumentException("结束位置不能小于起始位置");

            return new TextRange(start, end - start);
        }

        /// <summary>
        /// 检查是否包含指定位置
        /// </summary>
        /// <param name="position">位置</param>
        /// <returns>是否包含</returns>
        public bool Contains(int position)
        {
            return position >= Start && position < End;
        }

        /// <summary>
        /// 检查是否与另一个范围重叠
        /// </summary>
        /// <param name="other">另一个范围</param>
        /// <returns>是否重叠</returns>
        public bool Overlaps(TextRange other)
        {
            return Start < other.End && End > other.Start;
        }

        /// <summary>
        /// 获取与另一个范围的交集
        /// </summary>
        /// <param name="other">另一个范围</param>
        /// <returns>交集范围</returns>
        public TextRange? Intersect(TextRange other)
        {
            var start = Math.Max(Start, other.Start);
            var end = Math.Min(End, other.End);

            if (start >= end)
                return null;

            return new TextRange(start, end - start);
        }

        /// <summary>
        /// 获取与另一个范围的并集
        /// </summary>
        /// <param name="other">另一个范围</param>
        /// <returns>并集范围</returns>
        public TextRange Union(TextRange other)
        {
            var start = Math.Min(Start, other.Start);
            var end = Math.Max(End, other.End);
            return new TextRange(start, end - start);
        }

        /// <summary>
        /// 偏移范围
        /// </summary>
        /// <param name="offset">偏移量</param>
        /// <returns>偏移后的范围</returns>
        public TextRange Offset(int offset)
        {
            return new TextRange(Start + offset, Length);
        }

        public bool Equals(TextRange other)
        {
            return Start == other.Start && Length == other.Length;
        }

        public override bool Equals(object? obj)
        {
            return obj is TextRange other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Start, Length);
        }

        public static bool operator ==(TextRange left, TextRange right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TextRange left, TextRange right)
        {
            return !left.Equals(right);
        }

        public override string ToString() => $"[{Start}, {End})";
    }

    /// <summary>
    /// IME组合事件
    /// </summary>
    public class CompositionEvent : InputEvent
    {
        /// <summary>
        /// 组合事件类型
        /// </summary>
        public TextInputEventType EventType { get; }

        /// <summary>
        /// 组合文本
        /// </summary>
        public string CompositionText { get; }

        /// <summary>
        /// 选择范围
        /// </summary>
        public TextRange? Selection { get; }

        /// <summary>
        /// 候选列表
        /// </summary>
        public string[]? Candidates { get; }

        /// <summary>
        /// 选中的候选项索引
        /// </summary>
        public int SelectedCandidateIndex { get; }

        /// <summary>
        /// 是否最终确认
        /// </summary>
        public bool IsCommitted { get; }

        /// <summary>
        /// 初始化组合事件
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <param name="device">输入设备</param>
        /// <param name="eventType">事件类型</param>
        /// <param name="compositionText">组合文本</param>
        /// <param name="selection">选择范围</param>
        /// <param name="candidates">候选列表</param>
        /// <param name="selectedCandidateIndex">选中的候选项索引</param>
        /// <param name="isCommitted">是否最终确认</param>
        public CompositionEvent(
            uint timestamp,
            InputDevice device,
            TextInputEventType eventType,
            string compositionText,
            TextRange? selection = null,
            string[]? candidates = null,
            int selectedCandidateIndex = -1,
            bool isCommitted = false)
            : base(timestamp, device)
        {
            EventType = eventType;
            CompositionText = compositionText ?? throw new ArgumentNullException(nameof(compositionText));
            Selection = selection;
            Candidates = candidates;
            SelectedCandidateIndex = selectedCandidateIndex;
            IsCommitted = isCommitted;
        }

        public override string ToString() =>
            $"Composition: {EventType} '{CompositionText}'" +
            (Selection.HasValue ? $" Selection: {Selection}" : "") +
            (IsCommitted ? " (Committed)" : "");
    }
}
