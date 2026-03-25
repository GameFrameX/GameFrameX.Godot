using System;
using Godot;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// 游戏框架单例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract partial class GameFrameworkMonoSingleton<T> : Node where T : Node
    {
        private static T _instance;

        protected GameFrameworkMonoSingleton()
        {
        }

        /// <summary>
        /// 单例对象
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    // In Godot, we typically use AutoLoad singletons or find nodes in the scene
                    // This is a simplified approach - for production use AutoLoad pattern
                    var rootNode = Engine.GetMainLoop() as SceneTree;
                    if (rootNode != null)
                    {
                        _instance = rootNode.Root.FindChild(typeof(T).Name, true, false) as T;
                    }
                }

                if (_instance == null)
                {
                    var insObj = new Node();
                    _instance = insObj as T;
                    if (_instance == null)
                    {
                        // If T is not the same type as the created node, we need a different approach
                        // For now, we'll use reflection to create an instance
                        _instance = Activator.CreateInstance<T>();
                    }

                    _instance.Name = "[Singleton]" + typeof(T).Name;

                    // In Godot, use AutoLoad singleton pattern instead of DontDestroyOnLoad
                    // For now, we'll just add it to the scene
                    var rootNode = Engine.GetMainLoop() as SceneTree;
                    if (rootNode != null && _instance.GetParent() == null)
                    {
                        rootNode.Root.AddChild(_instance);
                    }
                }

                return _instance;
            }
        }
    }
}