using System;
using System.Collections.Generic;
using GameFrameX.Runtime;
using Godot;

namespace GameFrameX.UI.GDGUI.Runtime
{
    /// <summary>
    /// GDGUI 按钮扩展。
    /// </summary>
    public static class GDGUIButtonExtension
    {
        private static readonly Dictionary<BaseButton, Dictionary<Delegate, Action>> s_CallbackMap = new Dictionary<BaseButton, Dictionary<Delegate, Action>>();

        /// <summary>
        /// 添加按钮点击事件。
        /// </summary>
        /// <param name="self">按钮节点。</param>
        /// <param name="action">点击回调。</param>
        public static void Add(this BaseButton self, Action action)
        {
            GameFrameworkGuard.NotNull(self, nameof(self));
            GameFrameworkGuard.NotNull(action, nameof(action));
            var wrapper = action;
            self.Pressed += wrapper;
            CacheCallback(self, action, wrapper);
        }

        /// <summary>
        /// 移除按钮点击事件。
        /// </summary>
        /// <param name="self">按钮节点。</param>
        /// <param name="action">点击回调。</param>
        public static void Remove(this BaseButton self, Action action)
        {
            GameFrameworkGuard.NotNull(self, nameof(self));
            GameFrameworkGuard.NotNull(action, nameof(action));
            if (!TryGetWrapper(self, action, out var wrapper))
            {
                return;
            }

            self.Pressed -= wrapper;
            RemoveCachedCallback(self, action);
        }

        /// <summary>
        /// 清空按钮点击事件。
        /// </summary>
        /// <param name="self">按钮节点。</param>
        public static void Clear(this BaseButton self)
        {
            GameFrameworkGuard.NotNull(self, nameof(self));
            if (!s_CallbackMap.TryGetValue(self, out var callbacks))
            {
                return;
            }

            foreach (var pair in callbacks)
            {
                self.Pressed -= pair.Value;
            }

            s_CallbackMap.Remove(self);
        }

        /// <summary>
        /// 设置按钮点击事件。
        /// </summary>
        /// <param name="self">按钮节点。</param>
        /// <param name="action">点击回调。</param>
        public static void Set(this BaseButton self, Action action)
        {
            GameFrameworkGuard.NotNull(action, nameof(action));
            self.Clear();
            self.Add(action);
        }

        /// <summary>
        /// 设置按钮点击事件并透传用户数据。
        /// </summary>
        /// <param name="self">按钮节点。</param>
        /// <param name="action">点击回调。</param>
        /// <param name="userData">用户数据。</param>
        public static void Set(this BaseButton self, Action<object> action, object userData)
        {
            GameFrameworkGuard.NotNull(self, nameof(self));
            GameFrameworkGuard.NotNull(action, nameof(action));
            self.Clear();
            Action wrapper = () => action(userData);
            self.Pressed += wrapper;
            CacheCallback(self, action, wrapper);
        }

        /// <summary>
        /// 缓存按钮回调映射关系。
        /// </summary>
        /// <param name="self">按钮节点。</param>
        /// <param name="origin">原始回调。</param>
        /// <param name="wrapper">包装回调。</param>
        private static void CacheCallback(BaseButton self, Delegate origin, Action wrapper)
        {
            if (!s_CallbackMap.TryGetValue(self, out var callbacks))
            {
                callbacks = new Dictionary<Delegate, Action>();
                s_CallbackMap[self] = callbacks;
            }

            callbacks[origin] = wrapper;
        }

        /// <summary>
        /// 获取包装回调。
        /// </summary>
        /// <param name="self">按钮节点。</param>
        /// <param name="origin">原始回调。</param>
        /// <param name="wrapper">包装回调。</param>
        /// <returns>是否存在。</returns>
        private static bool TryGetWrapper(BaseButton self, Delegate origin, out Action wrapper)
        {
            wrapper = null;
            return s_CallbackMap.TryGetValue(self, out var callbacks) && callbacks.TryGetValue(origin, out wrapper);
        }

        /// <summary>
        /// 移除缓存回调。
        /// </summary>
        /// <param name="self">按钮节点。</param>
        /// <param name="origin">原始回调。</param>
        private static void RemoveCachedCallback(BaseButton self, Delegate origin)
        {
            if (!s_CallbackMap.TryGetValue(self, out var callbacks))
            {
                return;
            }

            callbacks.Remove(origin);
            if (callbacks.Count == 0)
            {
                s_CallbackMap.Remove(self);
            }
        }
    }
}
