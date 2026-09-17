using System;
using System.Collections.Generic;
using Godot;

namespace FairyGUI.Utils
{
    /// <summary>
    /// FGUI 路径帮助类（对应 Unity 侧 com.gameframex.unity.fairygui.unity 的 PathFinderHelper）。
    /// 根据 UI 对象生成路径字符串，或按路径查找 UI 对象，保持双端接口一致。
    /// </summary>
    public static class PathFinderHelper
    {
        /// <summary>
        /// 根据 UI 对象获取路径
        /// </summary>
        /// <param name="o">UI 对象</param>
        /// <returns>UI 所在路径</returns>
        public static string GetUIPath(GObject o)
        {
            var ls = new List<string>();
            SearchParent(o, ls);
            ls.Reverse();
            return string.Join("/", ls);
        }

        private static void SearchParent(GObject o, List<string> st)
        {
            if (o.parent != null)
            {
                st.Add(o.name);
                SearchParent(o.parent, st);
            }
            else
            {
                st.Add(o.name);
            }
        }

        /// <summary>
        /// 根据路径获取 FUI 对象
        /// </summary>
        /// <param name="path">UI 路径</param>
        /// <returns>UI 对象</returns>
        public static GObject GetUIFromPath(string path)
        {
            //GRoot / UISynthesisScene / ContentBox / ListSelect / 1990197248 / icon

            string[] arr = path.Split(new char[] { '/', }, StringSplitOptions.RemoveEmptyEntries);

            var q = new Queue<string>();
            foreach (var pathName in arr)
            {
                if (pathName == "GRoot")
                {
                    continue;
                }

                q.Enqueue(pathName);
            }

            try
            {
                GObject child = SearchChild(GRoot.inst, q);
                return child;
            }
            catch (Exception exception)
            {
                GD.PushError("error uiPath : can not found ui by this path :" + path + ", error : " + exception.Message);
            }

            return null;
        }

        private static GObject SearchChild(GComponent o, Queue<string> queue)
        {
            //防错
            if (queue.Count <= 0)
            {
                return o;
            }

            string path = queue.Dequeue();
            GObject child = null;
            if (path[0] == '$')
            {
                child = o.GetChild(path);
                if (child == null)
                {
                    string at = path.Substring(1);
                    int index = int.Parse(at);

                    if (index < 0 || index >= o.numChildren)
                    {
                        throw new Exception("eror path");
                    }

                    child = o.GetChildAt(index);
                }
            }
            else
            {
                child = o.GetChild(path);
            }

            if (child == null)
            {
                throw new Exception("error path");
            }

            if (queue.Count <= 0)
            {
                // 说明没有下级了
                return child;
            }

            if (child is GComponent)
            {
                return SearchChild(child as GComponent, queue);
            }

            return null;
        }
    }
}
