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
//  Any legal disputes or liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  CNB  仓库：https://cnb.cool/GameFrameX
//  CNB Repository: https://cnb.cool/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 按顺序执行 URL 故障转移，单个 URL 内做有界重试。
    /// </summary>
    public static class UrlFailoverRunner
    {
        /// <summary>
        /// 执行 URL 故障转移。
        /// </summary>
        /// <param name="urls">URL 主备列表。</param>
        /// <param name="maxAttemptsPerUrl">每个 URL 内部的总尝试次数上限（含初次）。</param>
        /// <param name="retryDelayMs">重试之间的延迟毫秒数。</param>
        /// <param name="attempt">单次尝试委托。</param>
        /// <param name="onProgress">进度回调（url, 当前尝试序号从 1 起, 总尝试数）。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>故障转移最终结果。</returns>
        public static Task<UrlFailoverResult> ExecuteAsync(IReadOnlyList<string> urls, int maxAttemptsPerUrl, int retryDelayMs, Func<string, Task<UrlAttemptResult>> attempt, Action<string, int, int> onProgress = null,
            CancellationToken cancellationToken = default)
        {
            if (urls == null)
            {
                throw new ArgumentNullException(nameof(urls));
            }

            if (urls.Count == 0)
            {
                throw new ArgumentException("URL list must contain at least one URL.", nameof(urls));
            }

            if (maxAttemptsPerUrl < 1)
            {
                throw new ArgumentException("Max attempts per URL must be greater than zero.", nameof(maxAttemptsPerUrl));
            }

            if (retryDelayMs < 0)
            {
                throw new ArgumentException("Retry delay must be zero or greater.", nameof(retryDelayMs));
            }

            if (attempt == null)
            {
                throw new ArgumentNullException(nameof(attempt));
            }

            return ExecuteCoreAsync(urls, maxAttemptsPerUrl, retryDelayMs, attempt, onProgress, cancellationToken);
        }

        private static async Task<UrlFailoverResult> ExecuteCoreAsync(IReadOnlyList<string> urls, int maxAttemptsPerUrl, int retryDelayMs, Func<string, Task<UrlAttemptResult>> attempt, Action<string, int, int> onProgress,
            CancellationToken cancellationToken)
        {
            var lastFailedUrl = string.Empty;
            var lastErrorMessage = string.Empty;

            for (var urlIndex = 0; urlIndex < urls.Count; urlIndex++)
            {
                var url = urls[urlIndex] ?? string.Empty;

                for (var attemptIndex = 1; attemptIndex <= maxAttemptsPerUrl; attemptIndex++)
                {
                    if (onProgress != null)
                    {
                        onProgress(url, attemptIndex, maxAttemptsPerUrl);
                    }

                    var result = await attempt(url);
                    if (result.Success)
                    {
                        return UrlFailoverResult.Succeed();
                    }

                    lastFailedUrl = url;
                    lastErrorMessage = result.ErrorMessage;

                    if (attemptIndex < maxAttemptsPerUrl && retryDelayMs > 0)
                    {
                        await Task.Delay(retryDelayMs, cancellationToken);
                    }
                }
            }

            return UrlFailoverResult.Fail(lastFailedUrl, lastErrorMessage);
        }
    }
}
