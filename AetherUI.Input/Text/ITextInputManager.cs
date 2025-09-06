using System;
using System.Collections.Generic;
using AetherUI.Input.Core;
using AetherUI.Input.Events;

namespace AetherUI.Input.Text
{
    /// <summary>
    /// 文本输入管理器接口
    /// </summary>
    public interface ITextInputManager
    {
        /// <summary>
        /// 文本输入事件
        /// </summary>
        event EventHandler<TextInputEvent>? TextInput;

        /// <summary>
        /// IME组合开始事件
        /// </summary>
        event EventHandler<ImeCompositionEventArgs>? CompositionStarted;

        /// <summary>
        /// IME组合更新事件
        /// </summary>
        event EventHandler<ImeCompositionEventArgs>? CompositionUpdated;

        /// <summary>
        /// IME组合结束事件
        /// </summary>
        event EventHandler<ImeCompositionEventArgs>? CompositionEnded;

        /// <summary>
        /// 当前文本输入目标
        /// </summary>
        ITextInputTarget? CurrentTarget { get; }

        /// <summary>
        /// 是否正在进行IME组合
        /// </summary>
        bool IsComposing { get; }

        /// <summary>
        /// 当前组合文本
        /// </summary>
        string CompositionText { get; }

        /// <summary>
        /// 设置文本输入目标
        /// </summary>
        /// <param name="target">文本输入目标</param>
        void SetTextInputTarget(ITextInputTarget? target);

        /// <summary>
        /// 启用文本输入
        /// </summary>
        /// <param name="rect">文本输入区域</param>
        void EnableTextInput(Rect rect);

        /// <summary>
        /// 禁用文本输入
        /// </summary>
        void DisableTextInput();

        /// <summary>
        /// 更新文本输入区域
        /// </summary>
        /// <param name="rect">文本输入区域</param>
        void UpdateTextInputRect(Rect rect);

        /// <summary>
        /// 处理键盘事件
        /// </summary>
        /// <param name="keyboardEvent">键盘事件</param>
        /// <returns>是否处理了事件</returns>
        bool ProcessKeyboardEvent(KeyboardEvent keyboardEvent);

        /// <summary>
        /// 获取支持的输入法列表
        /// </summary>
        /// <returns>输入法列表</returns>
        IEnumerable<InputMethodInfo> GetAvailableInputMethods();

        /// <summary>
        /// 设置当前输入法
        /// </summary>
        /// <param name="inputMethodId">输入法ID</param>
        void SetInputMethod(string inputMethodId);

        /// <summary>
        /// 获取当前输入法
        /// </summary>
        /// <returns>当前输入法信息</returns>
        InputMethodInfo? GetCurrentInputMethod();
    }

    /// <summary>
    /// 文本输入目标接口
    /// </summary>
    public interface ITextInputTarget
    {
        /// <summary>
        /// 是否可编辑
        /// </summary>
        bool IsEditable { get; }

        /// <summary>
        /// 是否只读
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// 文本内容
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// 光标位置
        /// </summary>
        int CaretPosition { get; set; }

        /// <summary>
        /// 选择开始位置
        /// </summary>
        int SelectionStart { get; set; }

        /// <summary>
        /// 选择长度
        /// </summary>
        int SelectionLength { get; set; }

        /// <summary>
        /// 最大文本长度
        /// </summary>
        int MaxLength { get; }

        /// <summary>
        /// 文本输入区域
        /// </summary>
        Rect InputRect { get; }

        /// <summary>
        /// 字体信息
        /// </summary>
        FontInfo FontInfo { get; }

        /// <summary>
        /// 插入文本
        /// </summary>
        /// <param name="text">要插入的文本</param>
        /// <param name="position">插入位置</param>
        void InsertText(string text, int position);

        /// <summary>
        /// 删除文本
        /// </summary>
        /// <param name="start">开始位置</param>
        /// <param name="length">删除长度</param>
        void DeleteText(int start, int length);

        /// <summary>
        /// 替换文本
        /// </summary>
        /// <param name="start">开始位置</param>
        /// <param name="length">替换长度</param>
        /// <param name="newText">新文本</param>
        void ReplaceText(int start, int length, string newText);

        /// <summary>
        /// 获取字符位置的矩形
        /// </summary>
        /// <param name="position">字符位置</param>
        /// <returns>字符矩形</returns>
        Rect GetCharacterRect(int position);

        /// <summary>
        /// 根据点击位置获取字符位置
        /// </summary>
        /// <param name="point">点击位置</param>
        /// <returns>字符位置</returns>
        int GetCharacterPosition(Point point);

        /// <summary>
        /// 验证文本输入
        /// </summary>
        /// <param name="text">输入文本</param>
        /// <returns>是否有效</returns>
        bool ValidateInput(string text);
    }

    /// <summary>
    /// IME组合事件参数
    /// </summary>
    public class ImeCompositionEventArgs : EventArgs
    {
        /// <summary>
        /// 组合文本
        /// </summary>
        public string CompositionText { get; }

        /// <summary>
        /// 光标位置
        /// </summary>
        public int CursorPosition { get; }

        /// <summary>
        /// 选择开始位置
        /// </summary>
        public int SelectionStart { get; }

        /// <summary>
        /// 选择长度
        /// </summary>
        public int SelectionLength { get; }

        /// <summary>
        /// 候选词列表
        /// </summary>
        public IReadOnlyList<string> Candidates { get; }

        /// <summary>
        /// 选中的候选词索引
        /// </summary>
        public int SelectedCandidateIndex { get; }

        /// <summary>
        /// 初始化IME组合事件参数
        /// </summary>
        /// <param name="compositionText">组合文本</param>
        /// <param name="cursorPosition">光标位置</param>
        /// <param name="selectionStart">选择开始位置</param>
        /// <param name="selectionLength">选择长度</param>
        /// <param name="candidates">候选词列表</param>
        /// <param name="selectedCandidateIndex">选中的候选词索引</param>
        public ImeCompositionEventArgs(
            string compositionText,
            int cursorPosition,
            int selectionStart,
            int selectionLength,
            IReadOnlyList<string>? candidates = null,
            int selectedCandidateIndex = -1)
        {
            CompositionText = compositionText ?? throw new ArgumentNullException(nameof(compositionText));
            CursorPosition = cursorPosition;
            SelectionStart = selectionStart;
            SelectionLength = selectionLength;
            Candidates = candidates ?? Array.Empty<string>();
            SelectedCandidateIndex = selectedCandidateIndex;
        }

        public override string ToString() =>
            $"ImeComposition: '{CompositionText}' cursor={CursorPosition} selection=[{SelectionStart},{SelectionLength}]";
    }

    /// <summary>
    /// 输入法信息
    /// </summary>
    public class InputMethodInfo
    {
        /// <summary>
        /// 输入法ID
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// 输入法名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 输入法显示名称
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// 语言代码
        /// </summary>
        public string LanguageCode { get; }

        /// <summary>
        /// 是否为默认输入法
        /// </summary>
        public bool IsDefault { get; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; }

        /// <summary>
        /// 初始化输入法信息
        /// </summary>
        /// <param name="id">输入法ID</param>
        /// <param name="name">输入法名称</param>
        /// <param name="displayName">输入法显示名称</param>
        /// <param name="languageCode">语言代码</param>
        /// <param name="isDefault">是否为默认输入法</param>
        /// <param name="isEnabled">是否启用</param>
        public InputMethodInfo(
            string id,
            string name,
            string displayName,
            string languageCode,
            bool isDefault = false,
            bool isEnabled = true)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            LanguageCode = languageCode ?? throw new ArgumentNullException(nameof(languageCode));
            IsDefault = isDefault;
            IsEnabled = isEnabled;
        }

        public override string ToString() => $"{DisplayName} ({LanguageCode})";
    }

    /// <summary>
    /// 字体信息
    /// </summary>
    public class FontInfo
    {
        /// <summary>
        /// 字体族名称
        /// </summary>
        public string FontFamily { get; }

        /// <summary>
        /// 字体大小
        /// </summary>
        public double FontSize { get; }

        /// <summary>
        /// 字体样式
        /// </summary>
        public FontStyle FontStyle { get; }

        /// <summary>
        /// 字体粗细
        /// </summary>
        public FontWeight FontWeight { get; }

        /// <summary>
        /// 初始化字体信息
        /// </summary>
        /// <param name="fontFamily">字体族名称</param>
        /// <param name="fontSize">字体大小</param>
        /// <param name="fontStyle">字体样式</param>
        /// <param name="fontWeight">字体粗细</param>
        public FontInfo(
            string fontFamily,
            double fontSize,
            FontStyle fontStyle = FontStyle.Normal,
            FontWeight fontWeight = FontWeight.Normal)
        {
            FontFamily = fontFamily ?? throw new ArgumentNullException(nameof(fontFamily));
            FontSize = fontSize;
            FontStyle = fontStyle;
            FontWeight = fontWeight;
        }

        public override string ToString() => $"{FontFamily} {FontSize}pt {FontStyle} {FontWeight}";
    }

    /// <summary>
    /// 字体样式
    /// </summary>
    public enum FontStyle
    {
        /// <summary>
        /// 正常
        /// </summary>
        Normal,

        /// <summary>
        /// 斜体
        /// </summary>
        Italic,

        /// <summary>
        /// 倾斜
        /// </summary>
        Oblique
    }

    /// <summary>
    /// 字体粗细
    /// </summary>
    public enum FontWeight
    {
        /// <summary>
        /// 细体
        /// </summary>
        Thin = 100,

        /// <summary>
        /// 超细体
        /// </summary>
        ExtraLight = 200,

        /// <summary>
        /// 细体
        /// </summary>
        Light = 300,

        /// <summary>
        /// 正常
        /// </summary>
        Normal = 400,

        /// <summary>
        /// 中等
        /// </summary>
        Medium = 500,

        /// <summary>
        /// 半粗体
        /// </summary>
        SemiBold = 600,

        /// <summary>
        /// 粗体
        /// </summary>
        Bold = 700,

        /// <summary>
        /// 超粗体
        /// </summary>
        ExtraBold = 800,

        /// <summary>
        /// 黑体
        /// </summary>
        Black = 900
    }

    /// <summary>
    /// 文本输入配置
    /// </summary>
    public class TextInputConfiguration
    {
        /// <summary>
        /// 是否启用IME
        /// </summary>
        public bool EnableIme { get; set; } = true;

        /// <summary>
        /// 是否启用自动完成
        /// </summary>
        public bool EnableAutoComplete { get; set; } = false;

        /// <summary>
        /// 是否启用拼写检查
        /// </summary>
        public bool EnableSpellCheck { get; set; } = false;

        /// <summary>
        /// 光标闪烁间隔（毫秒）
        /// </summary>
        public int CaretBlinkIntervalMs { get; set; } = 500;

        /// <summary>
        /// 重复按键延迟（毫秒）
        /// </summary>
        public int KeyRepeatDelayMs { get; set; } = 500;

        /// <summary>
        /// 重复按键间隔（毫秒）
        /// </summary>
        public int KeyRepeatIntervalMs { get; set; } = 50;

        /// <summary>
        /// 默认配置
        /// </summary>
        public static TextInputConfiguration Default { get; } = new();
    }
}
