using System;
using System.Collections.Generic;
using System.Diagnostics;
using AetherUI.Input.Core;
using AetherUI.Input.Events;

namespace AetherUI.Input.TextInput
{
    /// <summary>
    /// 文本输入上下文接口
    /// </summary>
    public interface ITextInputContext
    {
        /// <summary>
        /// 上下文ID
        /// </summary>
        string Id { get; }

        /// <summary>
        /// 关联的元素
        /// </summary>
        object Element { get; }

        /// <summary>
        /// 当前文本
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// 光标位置
        /// </summary>
        int CursorPosition { get; set; }

        /// <summary>
        /// 选择范围
        /// </summary>
        TextRange? Selection { get; set; }

        /// <summary>
        /// 是否只读
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// 是否启用多行
        /// </summary>
        bool IsMultiline { get; }

        /// <summary>
        /// 最大长度
        /// </summary>
        int MaxLength { get; }

        /// <summary>
        /// 输入类型
        /// </summary>
        TextInputType InputType { get; }

        /// <summary>
        /// 文本变化事件
        /// </summary>
        event EventHandler<TextChangedEventArgs>? TextChanged;

        /// <summary>
        /// 光标位置变化事件
        /// </summary>
        event EventHandler<CursorPositionChangedEventArgs>? CursorPositionChanged;

        /// <summary>
        /// 选择变化事件
        /// </summary>
        event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

        /// <summary>
        /// 插入文本
        /// </summary>
        /// <param name="text">要插入的文本</param>
        /// <param name="position">插入位置</param>
        /// <returns>是否成功</returns>
        bool InsertText(string text, int? position = null);

        /// <summary>
        /// 删除文本
        /// </summary>
        /// <param name="range">删除范围</param>
        /// <returns>是否成功</returns>
        bool DeleteText(TextRange range);

        /// <summary>
        /// 替换文本
        /// </summary>
        /// <param name="range">替换范围</param>
        /// <param name="newText">新文本</param>
        /// <returns>是否成功</returns>
        bool ReplaceText(TextRange range, string newText);

        /// <summary>
        /// 获取指定范围的文本
        /// </summary>
        /// <param name="range">文本范围</param>
        /// <returns>文本内容</returns>
        string GetText(TextRange range);
    }

    /// <summary>
    /// 文本输入类型
    /// </summary>
    public enum TextInputType
    {
        /// <summary>
        /// 普通文本
        /// </summary>
        Text,

        /// <summary>
        /// 密码
        /// </summary>
        Password,

        /// <summary>
        /// 数字
        /// </summary>
        Number,

        /// <summary>
        /// 电子邮件
        /// </summary>
        Email,

        /// <summary>
        /// URL
        /// </summary>
        Url,

        /// <summary>
        /// 电话号码
        /// </summary>
        Phone,

        /// <summary>
        /// 搜索
        /// </summary>
        Search
    }

    /// <summary>
    /// 文本输入管理器
    /// </summary>
    public class TextInputManager
    {
        private readonly Dictionary<string, ITextInputContext> _contexts = new();
        private readonly IIMEManager _imeManager;
        private ITextInputContext? _activeContext;

        /// <summary>
        /// 活动的文本输入上下文
        /// </summary>
        public ITextInputContext? ActiveContext => _activeContext;

        /// <summary>
        /// IME管理器
        /// </summary>
        public IIMEManager IMEManager => _imeManager;

        /// <summary>
        /// 文本输入事件
        /// </summary>
        public event EventHandler<TextInputEventArgs>? TextInput;

        /// <summary>
        /// 上下文激活事件
        /// </summary>
        public event EventHandler<ContextActivatedEventArgs>? ContextActivated;

        /// <summary>
        /// 上下文停用事件
        /// </summary>
        public event EventHandler<ContextDeactivatedEventArgs>? ContextDeactivated;

        /// <summary>
        /// 初始化文本输入管理器
        /// </summary>
        /// <param name="imeManager">IME管理器</param>
        public TextInputManager(IIMEManager? imeManager = null)
        {
            _imeManager = imeManager ?? new IMEManager();
            
            // 订阅IME事件
            _imeManager.CompositionChanged += OnCompositionChanged;
            _imeManager.TextCommitted += OnTextCommitted;
        }

        /// <summary>
        /// 注册文本输入上下文
        /// </summary>
        /// <param name="context">文本输入上下文</param>
        public void RegisterContext(ITextInputContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            _contexts[context.Id] = context;
            
            // 订阅上下文事件
            context.TextChanged += OnContextTextChanged;
            context.CursorPositionChanged += OnContextCursorPositionChanged;
            context.SelectionChanged += OnContextSelectionChanged;

            Debug.WriteLine($"文本输入上下文已注册: {context.Id}");
        }

        /// <summary>
        /// 注销文本输入上下文
        /// </summary>
        /// <param name="contextId">上下文ID</param>
        public void UnregisterContext(string contextId)
        {
            if (_contexts.TryGetValue(contextId, out ITextInputContext? context))
            {
                // 如果是活动上下文，先停用
                if (_activeContext == context)
                {
                    DeactivateContext();
                }

                // 取消订阅事件
                context.TextChanged -= OnContextTextChanged;
                context.CursorPositionChanged -= OnContextCursorPositionChanged;
                context.SelectionChanged -= OnContextSelectionChanged;

                _contexts.Remove(contextId);
                Debug.WriteLine($"文本输入上下文已注销: {contextId}");
            }
        }

        /// <summary>
        /// 激活文本输入上下文
        /// </summary>
        /// <param name="contextId">上下文ID</param>
        public void ActivateContext(string contextId)
        {
            if (!_contexts.TryGetValue(contextId, out ITextInputContext? context))
            {
                Debug.WriteLine($"未找到文本输入上下文: {contextId}");
                return;
            }

            if (_activeContext == context)
                return;

            // 停用当前上下文
            if (_activeContext != null)
            {
                DeactivateContext();
            }

            // 激活新上下文
            _activeContext = context;
            Debug.WriteLine($"文本输入上下文已激活: {contextId}");

            OnContextActivated(new ContextActivatedEventArgs(context));
        }

        /// <summary>
        /// 停用当前文本输入上下文
        /// </summary>
        public void DeactivateContext()
        {
            if (_activeContext == null)
                return;

            var context = _activeContext;
            _activeContext = null;

            // 取消IME组合
            _imeManager.CancelComposition();

            Debug.WriteLine($"文本输入上下文已停用: {context.Id}");
            OnContextDeactivated(new ContextDeactivatedEventArgs(context));
        }

        /// <summary>
        /// 处理键盘事件
        /// </summary>
        /// <param name="keyboardEvent">键盘事件</param>
        /// <returns>是否处理了事件</returns>
        public bool ProcessKeyboardEvent(KeyboardEvent keyboardEvent)
        {
            if (_activeContext == null || _activeContext.IsReadOnly)
                return false;

            switch (keyboardEvent.EventType)
            {
                case KeyboardEventType.KeyDown:
                    return ProcessKeyDown(keyboardEvent);

                case KeyboardEventType.KeyUp:
                    return ProcessKeyUp(keyboardEvent);

                case KeyboardEventType.Char:
                    return ProcessCharInput(keyboardEvent);

                default:
                    return false;
            }
        }

        /// <summary>
        /// 处理文本输入事件
        /// </summary>
        /// <param name="textInputEvent">文本输入事件</param>
        /// <returns>是否处理了事件</returns>
        public bool ProcessTextInputEvent(TextInputEvent textInputEvent)
        {
            if (_activeContext == null || _activeContext.IsReadOnly)
                return false;

            switch (textInputEvent.EventType)
            {
                case TextInputEventType.TextInput:
                    return ProcessTextInput(textInputEvent);

                case TextInputEventType.CompositionStart:
                    _imeManager.StartComposition(_activeContext);
                    return true;

                case TextInputEventType.CompositionUpdate:
                    _imeManager.UpdateComposition(textInputEvent.CompositionText ?? "", textInputEvent.CompositionSelection);
                    return true;

                case TextInputEventType.CompositionEnd:
                    _imeManager.EndComposition(textInputEvent.Text);
                    return true;

                case TextInputEventType.CompositionCancel:
                    _imeManager.CancelComposition();
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 处理按键按下
        /// </summary>
        private bool ProcessKeyDown(KeyboardEvent keyboardEvent)
        {
            if (_activeContext == null)
                return false;

            switch (keyboardEvent.Key)
            {
                case Key.Backspace:
                    return ProcessBackspace();

                case Key.Delete:
                    return ProcessDelete();

                case Key.Enter:
                    return ProcessEnter();

                case Key.Tab:
                    return ProcessTab();

                case Key.Left:
                case Key.Right:
                case Key.Up:
                case Key.Down:
                    return ProcessArrowKey(keyboardEvent.Key, keyboardEvent.Modifiers);

                case Key.Home:
                case Key.End:
                    return ProcessHomeEnd(keyboardEvent.Key, keyboardEvent.Modifiers);

                default:
                    return false;
            }
        }

        /// <summary>
        /// 处理按键抬起
        /// </summary>
        private bool ProcessKeyUp(KeyboardEvent keyboardEvent)
        {
            // 大多数按键抬起事件不需要特殊处理
            return false;
        }

        /// <summary>
        /// 处理字符输入
        /// </summary>
        private bool ProcessCharInput(KeyboardEvent keyboardEvent)
        {
            if (_activeContext == null || string.IsNullOrEmpty(keyboardEvent.Character))
                return false;

            return _activeContext.InsertText(keyboardEvent.Character);
        }

        /// <summary>
        /// 处理文本输入
        /// </summary>
        private bool ProcessTextInput(TextInputEvent textInputEvent)
        {
            if (_activeContext == null)
                return false;

            var success = _activeContext.InsertText(textInputEvent.Text);
            if (success)
            {
                OnTextInput(new TextInputEventArgs(textInputEvent, _activeContext));
            }

            return success;
        }

        /// <summary>
        /// 处理退格键
        /// </summary>
        private bool ProcessBackspace()
        {
            if (_activeContext == null)
                return false;

            if (_activeContext.Selection.HasValue)
            {
                // 删除选中的文本
                return _activeContext.DeleteText(_activeContext.Selection.Value);
            }
            else if (_activeContext.CursorPosition > 0)
            {
                // 删除光标前的一个字符
                var range = new TextRange(_activeContext.CursorPosition - 1, 1);
                return _activeContext.DeleteText(range);
            }

            return false;
        }

        /// <summary>
        /// 处理删除键
        /// </summary>
        private bool ProcessDelete()
        {
            if (_activeContext == null)
                return false;

            if (_activeContext.Selection.HasValue)
            {
                // 删除选中的文本
                return _activeContext.DeleteText(_activeContext.Selection.Value);
            }
            else if (_activeContext.CursorPosition < _activeContext.Text.Length)
            {
                // 删除光标后的一个字符
                var range = new TextRange(_activeContext.CursorPosition, 1);
                return _activeContext.DeleteText(range);
            }

            return false;
        }

        /// <summary>
        /// 处理回车键
        /// </summary>
        private bool ProcessEnter()
        {
            if (_activeContext == null)
                return false;

            if (_activeContext.IsMultiline)
            {
                return _activeContext.InsertText("\n");
            }

            // 单行文本框，回车键可能触发提交等操作
            return false;
        }

        /// <summary>
        /// 处理Tab键
        /// </summary>
        private bool ProcessTab()
        {
            if (_activeContext == null)
                return false;

            if (_activeContext.IsMultiline)
            {
                return _activeContext.InsertText("\t");
            }

            // 单行文本框，Tab键用于焦点导航
            return false;
        }

        /// <summary>
        /// 处理方向键
        /// </summary>
        private bool ProcessArrowKey(Key key, ModifierKeys modifiers)
        {
            if (_activeContext == null)
                return false;

            var shift = modifiers.HasFlag(ModifierKeys.Shift);
            var ctrl = modifiers.HasFlag(ModifierKeys.Control);

            // 这里应该实现光标移动逻辑
            // 简化实现，只处理左右方向键
            switch (key)
            {
                case Key.Left:
                    if (_activeContext.CursorPosition > 0)
                    {
                        _activeContext.CursorPosition--;
                        return true;
                    }
                    break;

                case Key.Right:
                    if (_activeContext.CursorPosition < _activeContext.Text.Length)
                    {
                        _activeContext.CursorPosition++;
                        return true;
                    }
                    break;
            }

            return false;
        }

        /// <summary>
        /// 处理Home/End键
        /// </summary>
        private bool ProcessHomeEnd(Key key, ModifierKeys modifiers)
        {
            if (_activeContext == null)
                return false;

            switch (key)
            {
                case Key.Home:
                    _activeContext.CursorPosition = 0;
                    return true;

                case Key.End:
                    _activeContext.CursorPosition = _activeContext.Text.Length;
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 处理IME组合变化
        /// </summary>
        private void OnCompositionChanged(object? sender, CompositionEventArgs e)
        {
            Debug.WriteLine($"IME组合变化: {e}");
        }

        /// <summary>
        /// 处理IME文本确认
        /// </summary>
        private void OnTextCommitted(object? sender, TextCommittedEventArgs e)
        {
            if (_activeContext != null)
            {
                _activeContext.InsertText(e.CommittedText);
                Debug.WriteLine($"IME文本确认: '{e.CommittedText}'");
            }
        }

        /// <summary>
        /// 处理上下文文本变化
        /// </summary>
        private void OnContextTextChanged(object? sender, TextChangedEventArgs e)
        {
            Debug.WriteLine($"文本变化: {e}");
        }

        /// <summary>
        /// 处理上下文光标位置变化
        /// </summary>
        private void OnContextCursorPositionChanged(object? sender, CursorPositionChangedEventArgs e)
        {
            Debug.WriteLine($"光标位置变化: {e}");
        }

        /// <summary>
        /// 处理上下文选择变化
        /// </summary>
        private void OnContextSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine($"选择变化: {e}");
        }

        /// <summary>
        /// 触发文本输入事件
        /// </summary>
        private void OnTextInput(TextInputEventArgs e)
        {
            TextInput?.Invoke(this, e);
        }

        /// <summary>
        /// 触发上下文激活事件
        /// </summary>
        private void OnContextActivated(ContextActivatedEventArgs e)
        {
            ContextActivated?.Invoke(this, e);
        }

        /// <summary>
        /// 触发上下文停用事件
        /// </summary>
        private void OnContextDeactivated(ContextDeactivatedEventArgs e)
        {
            ContextDeactivated?.Invoke(this, e);
        }
    }

    // 事件参数类定义
    public class TextInputEventArgs : EventArgs
    {
        public TextInputEvent TextInputEvent { get; }
        public ITextInputContext Context { get; }

        public TextInputEventArgs(TextInputEvent textInputEvent, ITextInputContext context)
        {
            TextInputEvent = textInputEvent;
            Context = context;
        }
    }

    public class ContextActivatedEventArgs : EventArgs
    {
        public ITextInputContext Context { get; }

        public ContextActivatedEventArgs(ITextInputContext context)
        {
            Context = context;
        }
    }

    public class ContextDeactivatedEventArgs : EventArgs
    {
        public ITextInputContext Context { get; }

        public ContextDeactivatedEventArgs(ITextInputContext context)
        {
            Context = context;
        }
    }

    public class TextChangedEventArgs : EventArgs
    {
        public string OldText { get; }
        public string NewText { get; }
        public TextRange ChangedRange { get; }

        public TextChangedEventArgs(string oldText, string newText, TextRange changedRange)
        {
            OldText = oldText;
            NewText = newText;
            ChangedRange = changedRange;
        }

        public override string ToString() => $"'{OldText}' -> '{NewText}' at {ChangedRange}";
    }

    public class CursorPositionChangedEventArgs : EventArgs
    {
        public int OldPosition { get; }
        public int NewPosition { get; }

        public CursorPositionChangedEventArgs(int oldPosition, int newPosition)
        {
            OldPosition = oldPosition;
            NewPosition = newPosition;
        }

        public override string ToString() => $"Cursor: {OldPosition} -> {NewPosition}";
    }

    public class SelectionChangedEventArgs : EventArgs
    {
        public TextRange? OldSelection { get; }
        public TextRange? NewSelection { get; }

        public SelectionChangedEventArgs(TextRange? oldSelection, TextRange? newSelection)
        {
            OldSelection = oldSelection;
            NewSelection = newSelection;
        }

        public override string ToString() => $"Selection: {OldSelection} -> {NewSelection}";
    }
}
