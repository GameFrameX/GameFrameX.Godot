// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and relevant international regulations.
//
//  使用本项目须严格遵守相应法律法规及开源许可证之规定。
//  Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//
//  本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//  This project is dual-licensed under the MIT License and Apache License 2.0,
//  完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//  please refer to the LICENSE file in the root directory of the source code for the full license text.
//
//  禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//  It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//  侵犯他人合法权益等法律法规所禁止的行为！
//  or infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any legal disputes and liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using System;
using System.Collections.Generic;
using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;

namespace GameFrameX.UI.Runtime
{
    /// <summary>
    /// UI事件订阅器
    /// </summary>
    public sealed class UIEventSubscriber : IReference
    {
        private readonly GameFrameworkMultiDictionary<string, EventHandler<GameEventArgs>> m_DicEventHandler;

        /// <summary>
        /// 持有者
        /// </summary>
        public object Owner { get; private set; }

        private readonly List<string> m_removeList;

        public UIEventSubscriber()
        {
            m_removeList = new List<string>();
            m_DicEventHandler = new GameFrameworkMultiDictionary<string, EventHandler<GameEventArgs>>();
            Owner = null;
        }

        /// <summary>
        /// 检查订阅
        /// </summary>
        /// <param name="id">消息ID</param>
        /// <param name="handler">处理对象</param>
        /// <exception cref="Exception"></exception>
        public void CheckSubscribe(string id, EventHandler<GameEventArgs> handler)
        {
            if (handler == null)
            {
                throw new Exception("Event handler is invalid.");
            }

            m_DicEventHandler.Add(id, handler);
            GameEntry.GetComponent<EventComponent>().CheckSubscribe(id, handler);
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="id">消息ID</param>
        /// <param name="handler">处理对象</param>
        /// <exception cref="Exception"></exception>
        public void UnSubscribe(string id, EventHandler<GameEventArgs> handler)
        {
            if (!m_DicEventHandler.Remove(id, handler))
            {
                throw new Exception(Utility.Text.Format("Event '{0}' not exists specified handler.", id.ToString()));
            }

            GameEntry.GetComponent<EventComponent>().Unsubscribe(id, handler);
        }

        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="id">消息ID</param>
        /// <param name="e">消息对象</param>
        public void Fire(string id, GameEventArgs e)
        {
            if (m_DicEventHandler.TryGetValue(id, out var handlers))
            {
                foreach (var eventHandler in handlers)
                {
                    try
                    {
                        eventHandler.Invoke(this, e);
                    }
                    catch (Exception exception)
                    {
                        Log.Error(exception);
                    }
                }

                GameEntry.GetComponent<EventComponent>().Fire(this, e);
            }
        }

        /// <summary>
        /// 取消所有订阅
        /// </summary>
        public void UnSubscribeAll(List<string> ignoreList = null)
        {
            if (m_DicEventHandler == null)
            {
                return;
            }

            foreach (var item in m_DicEventHandler)
            {
                if (ignoreList != null && ignoreList.Contains(item.Key))
                {
                    continue;
                }

                m_removeList.Add(item.Key);
                foreach (var eventHandler in item.Value)
                {
                    GameEntry.GetComponent<EventComponent>().Unsubscribe(item.Key, eventHandler);
                }
            }

            if (ignoreList == null)
            {
                m_DicEventHandler.Clear();
            }
            else
            {
                foreach (var key in m_removeList)
                {
                    m_DicEventHandler.RemoveAll(key);
                }
            }
        }

        /// <summary>
        /// 创建事件订阅器
        /// </summary>
        /// <param name="owner">持有者</param>
        /// <returns></returns>
        public static UIEventSubscriber Create(object owner)
        {
            var eventSubscriber = ReferencePool.Acquire<UIEventSubscriber>();
            eventSubscriber.Owner = owner;

            return eventSubscriber;
        }

        /// <summary>
        /// 清理
        /// </summary>
        public void Clear()
        {
            m_DicEventHandler.Clear();
            m_removeList.Clear();
            Owner = null;
        }
    }
}
