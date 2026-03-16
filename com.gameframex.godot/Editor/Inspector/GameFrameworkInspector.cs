using Godot;
using UnityEditor;

namespace GameFrameX.Editor
{
    /// <summary>
    /// 游戏框架 Inspector 抽象类。
    /// </summary>
    public abstract partial class GameFrameworkInspector : EditorInspectorPlugin
    {
        protected const string NoneOptionName = "<None>";
        private bool m_IsCompiling = false;

        public override bool _CanHandle(GodotObject @object)
        {
            return base._CanHandle(@object);
        }

        /// <summary>
        /// 绘制事件。
        /// </summary>
        public override void _ParseBegin(GodotObject @object)
        {
            base._ParseBegin(@object);

            
            if (m_IsCompiling && !EditorServer.IsSourceCodeChanging())
            {
                m_IsCompiling = false;
                OnCompileComplete();
            }
            else if (!m_IsCompiling && EditorApplication.isCompiling)
            {
                m_IsCompiling = true;
                OnCompileStart();
            }
        }

        /// <summary>
        /// 编译开始事件。
        /// </summary>
        protected virtual void OnCompileStart()
        {
        }

        /// <summary>
        /// 编译完成事件。
        /// </summary>
        protected virtual void OnCompileComplete()
        {
        }

        protected bool IsPrefabInHierarchy(GodotObject obj)
        {
            if (obj == null)
            {
                return false;
            }

/*#if UNITY_2018_3_OR_NEWER
            return PrefabUtility.GetPrefabAssetType(obj) != PrefabAssetType.Regular;
#else
            return PrefabUtility.GetPrefabType(obj) != PrefabType.Prefab;
#endif*/
            return true;
        }
    }
}