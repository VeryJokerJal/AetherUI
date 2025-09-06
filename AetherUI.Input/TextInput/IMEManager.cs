using System;
using System.Collections.Generic;
using System.Diagnostics;
using AetherUI.Input.Core;
using AetherUI.Input.Events;

namespace AetherUI.Input.TextInput
{
    /// <summary>
    /// IME管理器接口
    /// </summary>
    public interface IIMEManager
    {
        /// <summary>
        /// 是否启用IME
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// 当前IME状态
        /// </summary>
        IMEState State { get; }

        /// <summary>
        /// 组合文本
        /// </summary>
        string CompositionText { get; }

        /// <summary>
        /// 候选列表
        /// </summary>
        IReadOnlyList<string> Candidates { get; }

        /// <summary>
        /// 选中的候选项索引
        /// </summary>
        int SelectedCandidateIndex { get; }

        /// <summary>
        /// 组合事件
        /// </summary>
        event EventHandler<CompositionEventArgs>? CompositionChanged;

        /// <summary>
        /// 文本确认事件
        /// </summary>
        event EventHandler<TextCommittedEventArgs>? TextCommitted;

        /// <summary>
        /// 开始组合
        /// </summary>
        /// <param name="context">文本输入上下文</param>
        void StartComposition(ITextInputContext context);

        /// <summary>
        /// 更新组合
        /// </summary>
        /// <param name="compositionText">组合文本</param>
        /// <param name="selection">选择范围</param>
        void UpdateComposition(string compositionText, TextRange? selection = null);

        /// <summary>
        /// 结束组合
        /// </summary>
        /// <param name="committedText">确认的文本</param>
        void EndComposition(string? committedText = null);

        /// <summary>
        /// 取消组合
        /// </summary>
        void CancelComposition();

        /// <summary>
        /// 设置候选列表
        /// </summary>
        /// <param name="candidates">候选列表</param>
        /// <param name="selectedIndex">选中索引</param>
        void SetCandidates(IEnumerable<string> candidates, int selectedIndex = 0);

        /// <summary>
        /// 选择候选项
        /// </summary>
        /// <param name="index">候选项索引</param>
        void SelectCandidate(int index);
    }

    /// <summary>
    /// IME状态
    /// </summary>
    public enum IMEState
    {
        /// <summary>
        /// 禁用
        /// </summary>
        Disabled,

        /// <summary>
        /// 空闲
        /// </summary>
        Idle,

        /// <summary>
        /// 组合中
        /// </summary>
        Composing,

        /// <summary>
        /// 候选选择中
        /// </summary>
        Selecting
    }

    /// <summary>
    /// IME管理器实现
    /// </summary>
    public class IMEManager : IIMEManager
    {
        private bool _isEnabled = true;
        private IMEState _state = IMEState.Idle;
        private string _compositionText = string.Empty;
        private readonly List<string> _candidates = new();
        private int _selectedCandidateIndex = -1;
        private ITextInputContext? _currentContext;

        /// <summary>
        /// 是否启用IME
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    if (!value && _state != IMEState.Disabled)
                    {
                        CancelComposition();
                        _state = IMEState.Disabled;
                    }
                    else if (value && _state == IMEState.Disabled)
                    {
                        _state = IMEState.Idle;
                    }
                }
            }
        }

        /// <summary>
        /// 当前IME状态
        /// </summary>
        public IMEState State => _state;

        /// <summary>
        /// 组合文本
        /// </summary>
        public string CompositionText => _compositionText;

        /// <summary>
        /// 候选列表
        /// </summary>
        public IReadOnlyList<string> Candidates => _candidates;

        /// <summary>
        /// 选中的候选项索引
        /// </summary>
        public int SelectedCandidateIndex => _selectedCandidateIndex;

        /// <summary>
        /// 组合事件
        /// </summary>
        public event EventHandler<CompositionEventArgs>? CompositionChanged;

        /// <summary>
        /// 文本确认事件
        /// </summary>
        public event EventHandler<TextCommittedEventArgs>? TextCommitted;

        /// <summary>
        /// 开始组合
        /// </summary>
        /// <param name="context">文本输入上下文</param>
        public void StartComposition(ITextInputContext context)
        {
            if (!_isEnabled)
                return;

            _currentContext = context ?? throw new ArgumentNullException(nameof(context));
            _state = IMEState.Composing;
            _compositionText = string.Empty;
            _candidates.Clear();
            _selectedCandidateIndex = -1;

            Debug.WriteLine("IME组合开始");
            OnCompositionChanged(new CompositionEventArgs(TextInputEventType.CompositionStart, _compositionText));
        }

        /// <summary>
        /// 更新组合
        /// </summary>
        /// <param name="compositionText">组合文本</param>
        /// <param name="selection">选择范围</param>
        public void UpdateComposition(string compositionText, TextRange? selection = null)
        {
            if (!_isEnabled || _state != IMEState.Composing)
                return;

            _compositionText = compositionText ?? string.Empty;

            Debug.WriteLine($"IME组合更新: '{_compositionText}'");
            OnCompositionChanged(new CompositionEventArgs(TextInputEventType.CompositionUpdate, _compositionText, selection));
        }

        /// <summary>
        /// 结束组合
        /// </summary>
        /// <param name="committedText">确认的文本</param>
        public void EndComposition(string? committedText = null)
        {
            if (!_isEnabled || _state == IMEState.Idle)
                return;

            var finalText = committedText ?? _compositionText;
            
            Debug.WriteLine($"IME组合结束: '{finalText}'");
            
            // 先触发组合结束事件
            OnCompositionChanged(new CompositionEventArgs(TextInputEventType.CompositionEnd, finalText));
            
            // 如果有确认的文本，触发文本确认事件
            if (!string.IsNullOrEmpty(finalText))
            {
                OnTextCommitted(new TextCommittedEventArgs(finalText));
            }

            // 重置状态
            _state = IMEState.Idle;
            _compositionText = string.Empty;
            _candidates.Clear();
            _selectedCandidateIndex = -1;
            _currentContext = null;
        }

        /// <summary>
        /// 取消组合
        /// </summary>
        public void CancelComposition()
        {
            if (!_isEnabled || _state == IMEState.Idle)
                return;

            Debug.WriteLine("IME组合取消");
            
            OnCompositionChanged(new CompositionEventArgs(TextInputEventType.CompositionCancel, _compositionText));

            // 重置状态
            _state = IMEState.Idle;
            _compositionText = string.Empty;
            _candidates.Clear();
            _selectedCandidateIndex = -1;
            _currentContext = null;
        }

        /// <summary>
        /// 设置候选列表
        /// </summary>
        /// <param name="candidates">候选列表</param>
        /// <param name="selectedIndex">选中索引</param>
        public void SetCandidates(IEnumerable<string> candidates, int selectedIndex = 0)
        {
            if (!_isEnabled)
                return;

            _candidates.Clear();
            _candidates.AddRange(candidates ?? throw new ArgumentNullException(nameof(candidates)));
            _selectedCandidateIndex = selectedIndex;

            if (_candidates.Count > 0)
            {
                _state = IMEState.Selecting;
                Debug.WriteLine($"IME候选列表: {_candidates.Count} 项，选中: {_selectedCandidateIndex}");
            }
            else
            {
                _state = IMEState.Composing;
            }
        }

        /// <summary>
        /// 选择候选项
        /// </summary>
        /// <param name="index">候选项索引</param>
        public void SelectCandidate(int index)
        {
            if (!_isEnabled || _state != IMEState.Selecting || index < 0 || index >= _candidates.Count)
                return;

            _selectedCandidateIndex = index;
            var selectedText = _candidates[index];

            Debug.WriteLine($"IME选择候选项: '{selectedText}'");
            
            // 确认选中的候选项
            EndComposition(selectedText);
        }

        /// <summary>
        /// 触发组合变化事件
        /// </summary>
        private void OnCompositionChanged(CompositionEventArgs e)
        {
            CompositionChanged?.Invoke(this, e);
        }

        /// <summary>
        /// 触发文本确认事件
        /// </summary>
        private void OnTextCommitted(TextCommittedEventArgs e)
        {
            TextCommitted?.Invoke(this, e);
        }
    }

    /// <summary>
    /// 组合事件参数
    /// </summary>
    public class CompositionEventArgs : EventArgs
    {
        /// <summary>
        /// 事件类型
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
        /// 初始化组合事件参数
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="compositionText">组合文本</param>
        /// <param name="selection">选择范围</param>
        public CompositionEventArgs(TextInputEventType eventType, string compositionText, TextRange? selection = null)
        {
            EventType = eventType;
            CompositionText = compositionText ?? throw new ArgumentNullException(nameof(compositionText));
            Selection = selection;
        }

        public override string ToString() => $"{EventType}: '{CompositionText}'";
    }

    /// <summary>
    /// 文本确认事件参数
    /// </summary>
    public class TextCommittedEventArgs : EventArgs
    {
        /// <summary>
        /// 确认的文本
        /// </summary>
        public string CommittedText { get; }

        /// <summary>
        /// 初始化文本确认事件参数
        /// </summary>
        /// <param name="committedText">确认的文本</param>
        public TextCommittedEventArgs(string committedText)
        {
            CommittedText = committedText ?? throw new ArgumentNullException(nameof(committedText));
        }

        public override string ToString() => $"Committed: '{CommittedText}'";
    }
}
