//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using GameFrameX.Runtime;

namespace GameFrameX.Download.Runtime
{
    /// <summary>
    /// 使用 UnityWebRequest 实现的下载代理辅助器。
    /// </summary>
    public sealed partial class UnityWebRequestDownloadAgentHelper : DownloadAgentHelperBase
    {
        private const int CachedBytesLength = 0x1000;
        private static readonly HttpClient s_HttpClient = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        });

        private readonly byte[] m_CachedBytes = new byte[CachedBytesLength];
        private readonly object m_Lock = new object();
        private bool m_Disposed = false;
        private int m_RequestVersion = 0;
        private long m_DownloadedLength = 0L;
        private CancellationTokenSource m_CancellationTokenSource = null;

        private EventHandler<DownloadAgentHelperUpdateBytesEventArgs> m_DownloadAgentHelperUpdateBytesEventHandler = null;
        private EventHandler<DownloadAgentHelperUpdateLengthEventArgs> m_DownloadAgentHelperUpdateLengthEventHandler = null;
        private EventHandler<DownloadAgentHelperCompleteEventArgs> m_DownloadAgentHelperCompleteEventHandler = null;
        private EventHandler<DownloadAgentHelperErrorEventArgs> m_DownloadAgentHelperErrorEventHandler = null;

        /// <summary>
        /// 下载代理辅助器更新数据流事件。
        /// </summary>
        public override event EventHandler<DownloadAgentHelperUpdateBytesEventArgs> DownloadAgentHelperUpdateBytes
        {
            add { m_DownloadAgentHelperUpdateBytesEventHandler += value; }
            remove { m_DownloadAgentHelperUpdateBytesEventHandler -= value; }
        }

        /// <summary>
        /// 下载代理辅助器更新数据大小事件。
        /// </summary>
        public override event EventHandler<DownloadAgentHelperUpdateLengthEventArgs> DownloadAgentHelperUpdateLength
        {
            add { m_DownloadAgentHelperUpdateLengthEventHandler += value; }
            remove { m_DownloadAgentHelperUpdateLengthEventHandler -= value; }
        }

        /// <summary>
        /// 下载代理辅助器完成事件。
        /// </summary>
        public override event EventHandler<DownloadAgentHelperCompleteEventArgs> DownloadAgentHelperComplete
        {
            add { m_DownloadAgentHelperCompleteEventHandler += value; }
            remove { m_DownloadAgentHelperCompleteEventHandler -= value; }
        }

        /// <summary>
        /// 下载代理辅助器错误事件。
        /// </summary>
        public override event EventHandler<DownloadAgentHelperErrorEventArgs> DownloadAgentHelperError
        {
            add { m_DownloadAgentHelperErrorEventHandler += value; }
            remove { m_DownloadAgentHelperErrorEventHandler -= value; }
        }

        /// <summary>
        /// 通过下载代理辅助器下载指定地址的数据。
        /// </summary>
        /// <param name="downloadUri">下载地址。</param>
        /// <param name="userData">用户自定义数据。</param>
        public override void Download(string downloadUri, object userData)
        {
            if (m_DownloadAgentHelperUpdateBytesEventHandler == null || m_DownloadAgentHelperUpdateLengthEventHandler == null || m_DownloadAgentHelperCompleteEventHandler == null || m_DownloadAgentHelperErrorEventHandler == null)
            {
                Log.Fatal("Download agent helper handler is invalid.");
                return;
            }

            StartDownload(downloadUri, null);
        }

        /// <summary>
        /// 通过下载代理辅助器下载指定地址的数据。
        /// </summary>
        /// <param name="downloadUri">下载地址。</param>
        /// <param name="fromPosition">下载数据起始位置。</param>
        /// <param name="userData">用户自定义数据。</param>
        public override void Download(string downloadUri, long fromPosition, object userData)
        {
            if (m_DownloadAgentHelperUpdateBytesEventHandler == null || m_DownloadAgentHelperUpdateLengthEventHandler == null || m_DownloadAgentHelperCompleteEventHandler == null || m_DownloadAgentHelperErrorEventHandler == null)
            {
                Log.Fatal("Download agent helper handler is invalid.");
                return;
            }

            StartDownload(downloadUri, new RangeHeaderValue(fromPosition, null));
        }

        /// <summary>
        /// 通过下载代理辅助器下载指定地址的数据。
        /// </summary>
        /// <param name="downloadUri">下载地址。</param>
        /// <param name="fromPosition">下载数据起始位置。</param>
        /// <param name="toPosition">下载数据结束位置。</param>
        /// <param name="userData">用户自定义数据。</param>
        public override void Download(string downloadUri, long fromPosition, long toPosition, object userData)
        {
            if (m_DownloadAgentHelperUpdateBytesEventHandler == null || m_DownloadAgentHelperUpdateLengthEventHandler == null || m_DownloadAgentHelperCompleteEventHandler == null || m_DownloadAgentHelperErrorEventHandler == null)
            {
                Log.Fatal("Download agent helper handler is invalid.");
                return;
            }

            StartDownload(downloadUri, new RangeHeaderValue(fromPosition, toPosition));
        }

        /// <summary>
        /// 重置下载代理辅助器。
        /// </summary>
        public override void Reset()
        {
            CancellationTokenSource cancellationTokenSource = null;
            lock (m_Lock)
            {
                m_RequestVersion++;
                cancellationTokenSource = m_CancellationTokenSource;
                m_CancellationTokenSource = null;
                m_DownloadedLength = 0L;
            }

            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }

            Array.Clear(m_CachedBytes, 0, CachedBytesLength);
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        /// <param name="disposing">释放资源标记。</param>
        protected override void Dispose(bool disposing)
        {
            if (m_Disposed)
            {
                return;
            }

            if (disposing)
            {
                Reset();
            }

            m_Disposed = true;
            base.Dispose(disposing);
        }

        private void StartDownload(string downloadUri, RangeHeaderValue rangeHeaderValue)
        {
            CancellationTokenSource previousCancellationTokenSource = null;
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            int requestVersion;
            lock (m_Lock)
            {
                m_RequestVersion++;
                requestVersion = m_RequestVersion;
                previousCancellationTokenSource = m_CancellationTokenSource;
                m_CancellationTokenSource = cancellationTokenSource;
                m_DownloadedLength = 0L;
            }

            if (previousCancellationTokenSource != null)
            {
                previousCancellationTokenSource.Cancel();
                previousCancellationTokenSource.Dispose();
            }

            _ = ProcessDownloadAsync(downloadUri, rangeHeaderValue, requestVersion, cancellationTokenSource.Token);
        }

        private async Task ProcessDownloadAsync(string downloadUri, RangeHeaderValue rangeHeaderValue, int requestVersion, CancellationToken cancellationToken)
        {
            try
            {
                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, downloadUri);
                if (rangeHeaderValue != null)
                {
                    request.Headers.Range = rangeHeaderValue;
                }

                using HttpResponseMessage response = await s_HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    HandleError(requestVersion, (int)response.StatusCode == RangeNotSatisfiableErrorCode, $"{(int)response.StatusCode} {response.ReasonPhrase}");
                    return;
                }

                using System.IO.Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                while (true)
                {
                    int length = await stream.ReadAsync(m_CachedBytes.AsMemory(0, CachedBytesLength), cancellationToken);
                    if (length <= 0)
                    {
                        break;
                    }

                    if (!IsCurrentRequest(requestVersion))
                    {
                        return;
                    }

                    m_DownloadedLength += length;

                    DownloadAgentHelperUpdateBytesEventArgs downloadAgentHelperUpdateBytesEventArgs = DownloadAgentHelperUpdateBytesEventArgs.Create(m_CachedBytes, 0, length);
                    m_DownloadAgentHelperUpdateBytesEventHandler(this, downloadAgentHelperUpdateBytesEventArgs);
                    ReferencePool.Release(downloadAgentHelperUpdateBytesEventArgs);

                    DownloadAgentHelperUpdateLengthEventArgs downloadAgentHelperUpdateLengthEventArgs = DownloadAgentHelperUpdateLengthEventArgs.Create(length);
                    m_DownloadAgentHelperUpdateLengthEventHandler(this, downloadAgentHelperUpdateLengthEventArgs);
                    ReferencePool.Release(downloadAgentHelperUpdateLengthEventArgs);
                }

                if (!IsCurrentRequest(requestVersion))
                {
                    return;
                }

                DownloadAgentHelperCompleteEventArgs downloadAgentHelperCompleteEventArgs = DownloadAgentHelperCompleteEventArgs.Create(m_DownloadedLength);
                m_DownloadAgentHelperCompleteEventHandler(this, downloadAgentHelperCompleteEventArgs);
                ReferencePool.Release(downloadAgentHelperCompleteEventArgs);
            }
            catch (OperationCanceledException)
            {
            }
            catch (HttpRequestException exception)
            {
                HandleError(requestVersion, false, exception.Message);
            }
            catch (Exception exception)
            {
                HandleError(requestVersion, false, exception.ToString());
            }
        }

        private bool IsCurrentRequest(int requestVersion)
        {
            lock (m_Lock)
            {
                return requestVersion == m_RequestVersion;
            }
        }

        private void HandleError(int requestVersion, bool deleteDownloading, string errorMessage)
        {
            if (!IsCurrentRequest(requestVersion))
            {
                return;
            }

            DownloadAgentHelperErrorEventArgs downloadAgentHelperErrorEventArgs = DownloadAgentHelperErrorEventArgs.Create(deleteDownloading, errorMessage);
            m_DownloadAgentHelperErrorEventHandler(this, downloadAgentHelperErrorEventArgs);
            ReferencePool.Release(downloadAgentHelperErrorEventArgs);
        }
    }
}
