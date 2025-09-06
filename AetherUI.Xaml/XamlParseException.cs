using System;

namespace AetherUI.Xaml
{
    /// <summary>
    /// XAML解析异常
    /// </summary>
    public class XamlParseException : Exception
    {
        /// <summary>
        /// 初始化XAML解析异常
        /// </summary>
        public XamlParseException()
        {
        }

        /// <summary>
        /// 使用指定消息初始化XAML解析异常
        /// </summary>
        /// <param name="message">异常消息</param>
        public XamlParseException(string message) : base(message)
        {
        }

        /// <summary>
        /// 使用指定消息和内部异常初始化XAML解析异常
        /// </summary>
        /// <param name="message">异常消息</param>
        /// <param name="innerException">内部异常</param>
        public XamlParseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
