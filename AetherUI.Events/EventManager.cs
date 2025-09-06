using System;
using System.Collections.Generic;
using System.Diagnostics;
using AetherUI.Core;

namespace AetherUI.Events
{
    /// <summary>
    /// 事件管理器，负责路由事件的传播
    /// </summary>
    public static class EventManager
    {
        /// <summary>
        /// 触发路由事件
        /// </summary>
        /// <param name="source">事件源</param>
        /// <param name="args">事件参数</param>
        public static void RaiseEvent(object source, RoutedEventArgs args)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            if (args.RoutedEvent == null)
                throw new ArgumentException("RoutedEvent cannot be null", nameof(args));

            args.Source = source;
            args.OriginalSource ??= source;
switch (args.RoutedEvent.RoutingStrategy)
            {
                case RoutingStrategy.Direct:
                    RaiseDirectEvent(source, args);
                    break;
                case RoutingStrategy.Bubble:
                    RaiseBubbleEvent(source, args);
                    break;
                case RoutingStrategy.Tunnel:
                    RaiseTunnelEvent(source, args);
                    break;
            }
        }

        /// <summary>
        /// 触发直接事件
        /// </summary>
        /// <param name="source">事件源</param>
        /// <param name="args">事件参数</param>
        private static void RaiseDirectEvent(object source, RoutedEventArgs args)
        {
            if (source is IInputElement inputElement)
            {
                inputElement.RaiseEvent(args);
            }
        }

        /// <summary>
        /// 触发冒泡事件
        /// </summary>
        /// <param name="source">事件源</param>
        /// <param name="args">事件参数</param>
        private static void RaiseBubbleEvent(object source, RoutedEventArgs args)
        {
            List<IInputElement> route = BuildEventRoute(source, true);
foreach (IInputElement element in route)
            {
                if (args.Handled)
                    break;

                args.Source = element;
                element.RaiseEvent(args);
}
        }

        /// <summary>
        /// 触发隧道事件
        /// </summary>
        /// <param name="source">事件源</param>
        /// <param name="args">事件参数</param>
        private static void RaiseTunnelEvent(object source, RoutedEventArgs args)
        {
            List<IInputElement> route = BuildEventRoute(source, false);
foreach (IInputElement element in route)
            {
                if (args.Handled)
                    break;

                args.Source = element;
                element.RaiseEvent(args);
}
        }

        /// <summary>
        /// 构建事件路由
        /// </summary>
        /// <param name="source">事件源</param>
        /// <param name="isBubbling">是否为冒泡路由</param>
        /// <returns>事件路由列表</returns>
        private static List<IInputElement> BuildEventRoute(object source, bool isBubbling)
        {
            List<IInputElement> route = new List<IInputElement>();

            if (source is IInputElement currentElement)
            {
                // 构建从源到根的路径
                List<IInputElement> pathToRoot = new List<IInputElement>();
                
                while (currentElement != null)
                {
                    pathToRoot.Add(currentElement);
                    currentElement = GetParent(currentElement);
                }

                if (isBubbling)
                {
                    // 冒泡：从源到根
                    route.AddRange(pathToRoot);
                }
                else
                {
                    // 隧道：从根到源
                    for (int i = pathToRoot.Count - 1; i >= 0; i--)
                    {
                        route.Add(pathToRoot[i]);
                    }
                }
            }

            return route;
        }

        /// <summary>
        /// 获取元素的父元素
        /// </summary>
        /// <param name="element">当前元素</param>
        /// <returns>父元素，如果没有则返回null</returns>
        private static IInputElement? GetParent(IInputElement element)
        {
            // 这里需要根据实际的UI元素层次结构来实现
            // 暂时返回null，后续在实现UI元素时会完善
            return null;
        }
    }

    /// <summary>
    /// 输入元素接口，支持事件处理
    /// </summary>
    public interface IInputElement
    {
        /// <summary>
        /// 添加事件处理器
        /// </summary>
        /// <param name="routedEvent">路由事件</param>
        /// <param name="handler">事件处理器</param>
        void AddHandler(RoutedEvent routedEvent, Delegate handler);

        /// <summary>
        /// 移除事件处理器
        /// </summary>
        /// <param name="routedEvent">路由事件</param>
        /// <param name="handler">事件处理器</param>
        void RemoveHandler(RoutedEvent routedEvent, Delegate handler);

        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="args">事件参数</param>
        void RaiseEvent(RoutedEventArgs args);
    }

    /// <summary>
    /// 事件处理器信息
    /// </summary>
    internal class EventHandlerInfo
    {
        public RoutedEvent RoutedEvent { get; }
        public Delegate Handler { get; }
        public bool HandledEventsToo { get; }

        public EventHandlerInfo(RoutedEvent routedEvent, Delegate handler, bool handledEventsToo = false)
        {
            RoutedEvent = routedEvent;
            Handler = handler;
            HandledEventsToo = handledEventsToo;
        }
    }
}
