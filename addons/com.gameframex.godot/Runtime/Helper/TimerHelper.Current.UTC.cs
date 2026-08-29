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
//  or infringe upon the legitimate rights and interests of others as prohibited by laws and regulations.
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any legal disputes or liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
// 
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository:  https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

// 迁移备注：迁移自 Unity 基准 com.gameframex.unity/Runtime/Time/TimerHelper.Current.UTC.cs。
// 基准中的 CurrentTimeWithUtcFullString() / CurrentDateTimeWithUtcFormat() 已由既有 TimerHelper.Current.cs 提供同名实现，不再重复迁移。

using System;

namespace GameFrameX.Runtime
{
    public partial class TimerHelper
    {
        /// <summary>
        /// 获取当前UTC时区时间，格式为HHmmss的整数。
        /// </summary>
        /// <remarks>
        /// Gets the current UTC time as an integer in HHmmss format.
        /// This method converts the current UTC time to a 6-digit integer:
        /// - First 2 digits represent hours (24-hour format)
        /// - Middle 2 digits represent minutes
        /// - Last 2 digits represent seconds
        /// Internally calls CurrentTimeWithUtcFullString() to get the string and then converts to integer.
        /// </remarks>
        /// <returns>返回一个6位整数，表示当前UTC时间。例如：143045表示14:30:45 / Returns a 6-digit integer representing the current UTC time. For example: 143045 represents 14:30:45</returns>
        public static int CurrentTimeWithUtc()
        {
            return Convert.ToInt32(CurrentTimeWithUtcFullString());
        }

        /// <summary>
        /// 获取当前UTC时间。
        /// </summary>
        /// <remarks>
        /// Gets the current UTC time.
        /// This method returns the current UTC time (Coordinated Universal Time).
        /// Compared to local time, there will be a time zone offset.
        /// Mainly used for scenarios that require a unified time standard.
        /// </remarks>
        /// <returns>当前UTC时间 / The current UTC time</returns>
        public static DateTime GetNowWithUtc()
        {
            return DateTime.UtcNow;
        }
    }
}
