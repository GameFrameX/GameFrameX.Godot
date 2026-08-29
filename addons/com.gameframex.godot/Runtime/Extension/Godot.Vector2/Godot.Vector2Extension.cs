// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and related international regulations.
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

// 迁移备注：映射自 Unity 基准 com.gameframex.unity/Runtime/Extension/UnityEngine.Vector2/UnityEngine.Vector2Extension.cs。
// UnityEngine.Vector2 -> Godot.Vector2 同名同构类型，方法语义直接映射；分量命名 x/y -> X/Y。

using Godot;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// Vector2 扩展方法
    /// </summary>
    public static class GodotVector2Extension
    {
        /// <summary>
        /// 将 Vector2 转换为 Vector3，使用指定 y 坐标，原 x/y 映射为 x/z（平面坐标升维）。
        /// </summary>
        public static Godot.Vector3 ToVector3(this Vector2 vector2, float y)
        {
            return new Godot.Vector3(vector2.X, y, vector2.Y);
        }

        /// <summary>
        /// 将 Vector2 转换为 Vector4，z/w 分量为 0。
        /// </summary>
        public static Vector4 ToVector4(this Vector2 self)
        {
            return new Vector4(self.X, self.Y, 0, 0);
        }

        /// <summary>
        /// 将 Vector2 直接作为 Vector3 使用，x/y 对应 x/y，z 分量为 0。
        /// </summary>
        public static Godot.Vector3 AsVector3(this Vector2 self)
        {
            return new Godot.Vector3(self.X, self.Y, 0);
        }
    }
}
