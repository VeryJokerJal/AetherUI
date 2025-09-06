using System;
using System.Diagnostics;
using AetherUI.Input.Core;
using AetherUI.Input.Events;
using AetherUI.Input.TextInput;

namespace AetherUI.Input.Examples
{
    /// <summary>
    /// 文本输入示例
    /// </summary>
    public class TextInputExample
    {
        private TextInputManager? _textInputManager;
        private ExampleTextInputContext? _textBoxContext;
        private ExampleTextInputContext? _textAreaContext;

        /// <summary>
        /// 运行示例
        /// </summary>
        public void Run()
        {
            Debug.WriteLine("开始文本输入示例");

            try
            {
                // 初始化文本输入管理器
                InitializeTextInputManager();

                // 创建文本输入上下文
                CreateTextInputContexts();

                // 测试基本文本输入
                TestBasicTextInput();

                // 测试IME输入
                TestIMEInput();

                // 测试键盘导航
                TestKeyboardNavigation();

                // 显示统计信息
                ShowStatistics();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"示例运行失败: {ex.Message}");
            }

            Debug.WriteLine("文本输入示例完成");
        }

        /// <summary>
        /// 初始化文本输入管理器
        /// </summary>
        private void InitializeTextInputManager()
        {
            var imeManager = new IMEManager();
            _textInputManager = new TextInputManager(imeManager);

            // 订阅事件
            _textInputManager.TextInput += OnTextInput;
            _textInputManager.ContextActivated += OnContextActivated;
            _textInputManager.ContextDeactivated += OnContextDeactivated;

            Debug.WriteLine("文本输入管理器初始化完成");
        }

        /// <summary>
        /// 创建文本输入上下文
        /// </summary>
        private void CreateTextInputContexts()
        {
            // 创建单行文本框上下文
            _textBoxContext = new ExampleTextInputContext(
                "TextBox1",
                new ExampleTextElement("TextBox"),
                TextInputType.Text,
                false,
                100);

            // 创建多行文本区域上下文
            _textAreaContext = new ExampleTextInputContext(
                "TextArea1",
                new ExampleTextElement("TextArea"),
                TextInputType.Text,
                true,
                1000);

            // 注册上下文
            _textInputManager?.RegisterContext(_textBoxContext);
            _textInputManager?.RegisterContext(_textAreaContext);

            Debug.WriteLine("文本输入上下文已创建");
        }

        /// <summary>
        /// 测试基本文本输入
        /// </summary>
        private void TestBasicTextInput()
        {
            Debug.WriteLine("\n=== 基本文本输入测试 ===");

            if (_textInputManager == null || _textBoxContext == null)
                return;

            // 激活文本框
            _textInputManager.ActivateContext(_textBoxContext.Id);

            var keyboard = new InputDevice(InputDeviceType.Keyboard, 0, "Keyboard");

            // 模拟输入 "Hello"
            Debug.WriteLine("\n--- 输入 'Hello' ---");
            SimulateTextInput("Hello", keyboard, 1000);

            // 模拟退格删除
            Debug.WriteLine("\n--- 退格删除 ---");
            SimulateKeyPress(Key.Backspace, keyboard, 2000);
            SimulateKeyPress(Key.Backspace, keyboard, 2100);

            // 模拟插入文本
            Debug.WriteLine("\n--- 插入 'p!' ---");
            SimulateTextInput("p!", keyboard, 3000);

            Debug.WriteLine($"最终文本: '{_textBoxContext.Text}'");
        }

        /// <summary>
        /// 测试IME输入
        /// </summary>
        private void TestIMEInput()
        {
            Debug.WriteLine("\n=== IME输入测试 ===");

            if (_textInputManager == null || _textAreaContext == null)
                return;

            // 激活文本区域
            _textInputManager.ActivateContext(_textAreaContext.Id);

            var keyboard = new InputDevice(InputDeviceType.Keyboard, 0, "Keyboard");

            // 模拟IME组合输入
            Debug.WriteLine("\n--- IME组合输入 '你好' ---");
            
            // 开始组合
            var compositionStart = new TextInputEvent(
                4000,
                keyboard,
                TextInputEventType.CompositionStart,
                "");
            _textInputManager.ProcessTextInputEvent(compositionStart);

            // 组合更新
            var compositionUpdate1 = new TextInputEvent(
                4100,
                keyboard,
                TextInputEventType.CompositionUpdate,
                "",
                "ni",
                new TextRange(0, 2));
            _textInputManager.ProcessTextInputEvent(compositionUpdate1);

            var compositionUpdate2 = new TextInputEvent(
                4200,
                keyboard,
                TextInputEventType.CompositionUpdate,
                "",
                "nihao",
                new TextRange(0, 5));
            _textInputManager.ProcessTextInputEvent(compositionUpdate2);

            // 组合结束
            var compositionEnd = new TextInputEvent(
                4300,
                keyboard,
                TextInputEventType.CompositionEnd,
                "你好");
            _textInputManager.ProcessTextInputEvent(compositionEnd);

            Debug.WriteLine($"IME输入后文本: '{_textAreaContext.Text}'");
        }

        /// <summary>
        /// 测试键盘导航
        /// </summary>
        private void TestKeyboardNavigation()
        {
            Debug.WriteLine("\n=== 键盘导航测试 ===");

            if (_textInputManager == null || _textBoxContext == null)
                return;

            // 激活文本框
            _textInputManager.ActivateContext(_textBoxContext.Id);

            var keyboard = new InputDevice(InputDeviceType.Keyboard, 0, "Keyboard");

            Debug.WriteLine($"当前光标位置: {_textBoxContext.CursorPosition}");

            // 测试Home键
            Debug.WriteLine("\n--- Home键 ---");
            SimulateKeyPress(Key.Home, keyboard, 5000);
            Debug.WriteLine($"Home后光标位置: {_textBoxContext.CursorPosition}");

            // 测试右方向键
            Debug.WriteLine("\n--- 右方向键 ---");
            SimulateKeyPress(Key.Right, keyboard, 5100);
            SimulateKeyPress(Key.Right, keyboard, 5200);
            Debug.WriteLine($"右移后光标位置: {_textBoxContext.CursorPosition}");

            // 测试End键
            Debug.WriteLine("\n--- End键 ---");
            SimulateKeyPress(Key.End, keyboard, 5300);
            Debug.WriteLine($"End后光标位置: {_textBoxContext.CursorPosition}");

            // 测试左方向键
            Debug.WriteLine("\n--- 左方向键 ---");
            SimulateKeyPress(Key.Left, keyboard, 5400);
            Debug.WriteLine($"左移后光标位置: {_textBoxContext.CursorPosition}");
        }

        /// <summary>
        /// 模拟文本输入
        /// </summary>
        private void SimulateTextInput(string text, InputDevice device, uint timestamp)
        {
            var textInputEvent = new TextInputEvent(
                timestamp,
                device,
                TextInputEventType.TextInput,
                text);

            _textInputManager?.ProcessTextInputEvent(textInputEvent);
        }

        /// <summary>
        /// 模拟按键
        /// </summary>
        private void SimulateKeyPress(Key key, InputDevice device, uint timestamp)
        {
            var keyDownEvent = new KeyboardEvent(
                timestamp,
                device,
                key,
                KeyboardEventType.KeyDown,
                ModifierKeys.None);

            var keyUpEvent = new KeyboardEvent(
                timestamp + 50,
                device,
                key,
                KeyboardEventType.KeyUp,
                ModifierKeys.None);

            _textInputManager?.ProcessKeyboardEvent(keyDownEvent);
            _textInputManager?.ProcessKeyboardEvent(keyUpEvent);
        }

        /// <summary>
        /// 显示统计信息
        /// </summary>
        private void ShowStatistics()
        {
            Debug.WriteLine("\n=== 统计信息 ===");

            if (_textInputManager == null)
                return;

            Debug.WriteLine($"活动上下文: {_textInputManager.ActiveContext?.Id ?? "无"}");
            Debug.WriteLine($"IME状态: {_textInputManager.IMEManager.State}");
            Debug.WriteLine($"IME组合文本: '{_textInputManager.IMEManager.CompositionText}'");

            if (_textBoxContext != null)
            {
                Debug.WriteLine($"TextBox文本: '{_textBoxContext.Text}' (光标: {_textBoxContext.CursorPosition})");
            }

            if (_textAreaContext != null)
            {
                Debug.WriteLine($"TextArea文本: '{_textAreaContext.Text}' (光标: {_textAreaContext.CursorPosition})");
            }
        }

        /// <summary>
        /// 处理文本输入事件
        /// </summary>
        private void OnTextInput(object? sender, TextInputEventArgs e)
        {
            Debug.WriteLine($"[TextInput] {e.TextInputEvent} -> {e.Context.Id}");
        }

        /// <summary>
        /// 处理上下文激活事件
        /// </summary>
        private void OnContextActivated(object? sender, ContextActivatedEventArgs e)
        {
            Debug.WriteLine($"[Context] 激活: {e.Context.Id}");
        }

        /// <summary>
        /// 处理上下文停用事件
        /// </summary>
        private void OnContextDeactivated(object? sender, ContextDeactivatedEventArgs e)
        {
            Debug.WriteLine($"[Context] 停用: {e.Context.Id}");
        }
    }

    /// <summary>
    /// 示例文本输入上下文
    /// </summary>
    public class ExampleTextInputContext : ITextInputContext
    {
        private string _text = string.Empty;
        private int _cursorPosition = 0;
        private TextRange? _selection;

        /// <summary>
        /// 上下文ID
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// 关联的元素
        /// </summary>
        public object Element { get; }

        /// <summary>
        /// 当前文本
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    var oldText = _text;
                    _text = value;
                    OnTextChanged(new TextChangedEventArgs(oldText, _text, new TextRange(0, _text.Length)));
                }
            }
        }

        /// <summary>
        /// 光标位置
        /// </summary>
        public int CursorPosition
        {
            get => _cursorPosition;
            set
            {
                var newPosition = Math.Max(0, Math.Min(value, _text.Length));
                if (_cursorPosition != newPosition)
                {
                    var oldPosition = _cursorPosition;
                    _cursorPosition = newPosition;
                    OnCursorPositionChanged(new CursorPositionChangedEventArgs(oldPosition, _cursorPosition));
                }
            }
        }

        /// <summary>
        /// 选择范围
        /// </summary>
        public TextRange? Selection
        {
            get => _selection;
            set
            {
                if (_selection != value)
                {
                    var oldSelection = _selection;
                    _selection = value;
                    OnSelectionChanged(new SelectionChangedEventArgs(oldSelection, _selection));
                }
            }
        }

        /// <summary>
        /// 是否只读
        /// </summary>
        public bool IsReadOnly { get; }

        /// <summary>
        /// 是否启用多行
        /// </summary>
        public bool IsMultiline { get; }

        /// <summary>
        /// 最大长度
        /// </summary>
        public int MaxLength { get; }

        /// <summary>
        /// 输入类型
        /// </summary>
        public TextInputType InputType { get; }

        /// <summary>
        /// 文本变化事件
        /// </summary>
        public event EventHandler<TextChangedEventArgs>? TextChanged;

        /// <summary>
        /// 光标位置变化事件
        /// </summary>
        public event EventHandler<CursorPositionChangedEventArgs>? CursorPositionChanged;

        /// <summary>
        /// 选择变化事件
        /// </summary>
        public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

        /// <summary>
        /// 初始化示例文本输入上下文
        /// </summary>
        public ExampleTextInputContext(
            string id,
            object element,
            TextInputType inputType = TextInputType.Text,
            bool isMultiline = false,
            int maxLength = int.MaxValue,
            bool isReadOnly = false)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Element = element ?? throw new ArgumentNullException(nameof(element));
            InputType = inputType;
            IsMultiline = isMultiline;
            MaxLength = maxLength;
            IsReadOnly = isReadOnly;
        }

        /// <summary>
        /// 插入文本
        /// </summary>
        public bool InsertText(string text, int? position = null)
        {
            if (IsReadOnly || string.IsNullOrEmpty(text))
                return false;

            var insertPos = position ?? _cursorPosition;
            if (insertPos < 0 || insertPos > _text.Length)
                return false;

            // 检查长度限制
            if (_text.Length + text.Length > MaxLength)
                return false;

            var oldText = _text;
            _text = _text.Insert(insertPos, text);
            _cursorPosition = insertPos + text.Length;

            OnTextChanged(new TextChangedEventArgs(oldText, _text, new TextRange(insertPos, text.Length)));
            return true;
        }

        /// <summary>
        /// 删除文本
        /// </summary>
        public bool DeleteText(TextRange range)
        {
            if (IsReadOnly || range.Start < 0 || range.End > _text.Length)
                return false;

            var oldText = _text;
            _text = _text.Remove(range.Start, range.Length);
            _cursorPosition = Math.Min(_cursorPosition, range.Start);

            OnTextChanged(new TextChangedEventArgs(oldText, _text, range));
            return true;
        }

        /// <summary>
        /// 替换文本
        /// </summary>
        public bool ReplaceText(TextRange range, string newText)
        {
            if (IsReadOnly)
                return false;

            return DeleteText(range) && InsertText(newText, range.Start);
        }

        /// <summary>
        /// 获取指定范围的文本
        /// </summary>
        public string GetText(TextRange range)
        {
            if (range.Start < 0 || range.End > _text.Length)
                return string.Empty;

            return _text.Substring(range.Start, range.Length);
        }

        /// <summary>
        /// 触发文本变化事件
        /// </summary>
        protected virtual void OnTextChanged(TextChangedEventArgs e)
        {
            TextChanged?.Invoke(this, e);
        }

        /// <summary>
        /// 触发光标位置变化事件
        /// </summary>
        protected virtual void OnCursorPositionChanged(CursorPositionChangedEventArgs e)
        {
            CursorPositionChanged?.Invoke(this, e);
        }

        /// <summary>
        /// 触发选择变化事件
        /// </summary>
        protected virtual void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            SelectionChanged?.Invoke(this, e);
        }

        public override string ToString() => $"TextContext({Id}): '{Text}' [{CursorPosition}]";
    }

    /// <summary>
    /// 示例文本元素
    /// </summary>
    public class ExampleTextElement
    {
        /// <summary>
        /// 元素名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 初始化示例文本元素
        /// </summary>
        /// <param name="name">元素名称</param>
        public ExampleTextElement(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override string ToString() => Name;
    }

    /// <summary>
    /// 文本输入示例程序
    /// </summary>
    public static class TextInputExampleProgram
    {
        /// <summary>
        /// 运行示例
        /// </summary>
        public static void RunExample()
        {
            var example = new TextInputExample();
            example.Run();
        }
    }
}
