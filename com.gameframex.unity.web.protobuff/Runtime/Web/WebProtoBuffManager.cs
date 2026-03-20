using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GameFrameX.Runtime;

namespace GameFrameX.Web.ProtoBuff.Runtime
{
    public partial class WebProtoBuffManager : GameFrameworkModule, IWebProtoBuffManager
    {
        private readonly StringBuilder m_StringBuilder = new StringBuilder(256);
        private readonly MemoryStream m_MemoryStream;
        private float m_Timeout = 5f;

        public WebProtoBuffManager()
        {
            MaxConnectionPerServer = 8;
            m_MemoryStream = new MemoryStream();
            Timeout = 5f;
        }

        public float Timeout
        {
            get { return m_Timeout; }
            set
            {
                m_Timeout = value;
                RequestTimeout = TimeSpan.FromSeconds(value);
            }
        }

        public int MaxConnectionPerServer { get; set; }

        public TimeSpan RequestTimeout { get; set; }

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            lock (m_StringBuilder)
            {
                UpdateProtoBuf(elapseSeconds, realElapseSeconds);
            }
        }

        public override void Shutdown()
        {
            ShutdownProtoBuf();
            m_MemoryStream.Dispose();
        }

        private string UrlHandler(string url, Dictionary<string, string> queryString)
        {
            m_StringBuilder.Clear();
            m_StringBuilder.Append(url);
            if (queryString != null && queryString.Count > 0)
            {
                if (!url.EndsWithFast("?"))
                {
                    m_StringBuilder.Append("?");
                }

                foreach (var kv in queryString)
                {
                    m_StringBuilder.AppendFormat("{0}={1}&", kv.Key, kv.Value);
                }

                url = m_StringBuilder.ToString(0, m_StringBuilder.Length - 1);
                m_StringBuilder.Clear();
            }

            return url;
        }
    }
}
