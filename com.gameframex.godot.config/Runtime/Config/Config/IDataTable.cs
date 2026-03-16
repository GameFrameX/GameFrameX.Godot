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
using System.Threading.Tasks;

namespace GameFrameX.Config.Runtime
{
    public interface IDataTable
    {
        /// <summary>
        /// 异步加载
        /// </summary>
        /// <returns></returns>
        Task LoadAsync();

        /// <summary>
        /// 获取数据表中对象的数量
        /// </summary>
        /// <returns></returns>
        int Count { get; }
    }

    /// <summary>
    /// 数据表基础接口
    /// </summary>
    public interface IDataTable<T> : IDataTable where T : class
    {
        /// <summary>
        /// 根据ID获取对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Obsolete("请使用TryGet方法")]
        T Get(int id);

        /// <summary>
        /// 根据ID获取对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Obsolete("请使用TryGet方法")]
        T Get(long id);

        /// <summary>
        /// 根据ID获取对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Obsolete("请使用TryGet方法")]
        T Get(string id);

        /// <summary>
        /// 尝试根据整数ID获取对象
        /// </summary>
        /// <param name="id">要获取的对象的整数ID</param>
        /// <param name="value">当找到对应ID的对象时，返回该对象；否则返回默认值</param>
        /// <returns>如果找到对应ID的对象则返回true，否则返回false</returns>
        bool TryGet(int id, out T value);

        /// <summary>
        /// 尝试根据长整数ID获取对象
        /// </summary>
        /// <param name="id">要获取的对象的长整数ID</param>
        /// <param name="value">当找到对应ID的对象时，返回该对象；否则返回默认值</param>
        /// <returns>如果找到对应ID的对象则返回true，否则返回false</returns>
        bool TryGet(long id, out T value);

        /// <summary>
        /// 尝试根据字符串ID获取对象
        /// </summary>
        /// <param name="id">要获取的对象的字符串ID</param>
        /// <param name="value">当找到对应ID的对象时，返回该对象；否则返回默认值</param>
        /// <returns>如果找到对应ID的对象则返回true，否则返回false</returns>
        bool TryGet(string id, out T value);

        /// <summary>
        /// 根据整数主键获取数据表中的对象
        /// </summary>
        /// <param name="id">要获取的对象的整数主键</param>
        /// <returns>与指定主键关联的数据对象；如果找不到则返回 null</returns>
        T this[int id] { get; }

        /// <summary>
        /// 根据长整数主键获取数据表中的对象
        /// </summary>
        /// <param name="id">要获取的对象的长整数主键</param>
        /// <returns>与指定主键关联的数据对象；如果找不到则返回 null</returns>
        T this[long id] { get; }

        /// <summary>
        /// 根据字符串键获取数据表中的对象
        /// </summary>
        /// <param name="id">要获取的对象在数据表中的字符串键</param>
        /// <returns>与指定键关联的数据对象；如果找不到则返回 null</returns>
        T this[string id] { get; }

        /// <summary>
        /// 获取数据表中第一个对象
        /// </summary>
        /// <returns>如果数据表为空，则返回 null；否则返回第一个对象</returns>
        T FirstOrDefault { get; }

        /// <summary>
        /// 获取数据表中最后一个对象
        /// </summary>
        /// <returns>如果数据表为空，则返回 null；否则返回最后一个对象</returns>
        T LastOrDefault { get; }

        /// <summary>
        /// 获取数据表中所有对象
        /// </summary>
        /// <returns>包含数据表中所有对象的数组</returns>
        T[] All { get; }

        /// <summary>
        /// 获取数据表中所有对象
        /// </summary>
        /// <returns>包含数据表中所有对象的新数组</returns>
        T[] ToArray();

        /// <summary>
        /// 获取数据表中所有对象
        /// </summary>
        /// <returns>包含数据表中所有对象的新列表</returns>
        List<T> ToList();

        /// <summary>
        /// 根据条件查找第一个匹配的对象
        /// </summary>
        /// <param name="func">用于定义匹配条件的函数</param>
        /// <returns>如果找到匹配的对象，则返回该对象；否则返回 null</returns>
        T Find(Func<T, bool> func);

        /// <summary>
        /// 根据条件查找所有匹配的对象
        /// </summary>
        /// <param name="func">用于定义匹配条件的函数</param>
        /// <returns>包含所有匹配对象的数组</returns>
        T[] FindListArray(Func<T, bool> func);

        /// <summary>
        /// 根据条件查找所有匹配的对象
        /// </summary>
        /// <param name="func">用于定义匹配条件的函数</param>
        /// <returns>包含所有匹配对象的列表</returns>
        List<T> FindList(Func<T, bool> func);

        /// <summary>
        /// 对数据表中的每个元素执行指定的操作
        /// </summary>
        /// <param name="func">要对每个元素执行的操作</param>
        void ForEach(Action<T> func);

        /// <summary>
        /// 获取数据表中指定属性的最大值
        /// </summary>
        /// <param name="func">用于获取比较值的函数</param>
        /// <typeparam name="Tk">比较值的类型，必须实现 IComparable 接口</typeparam>
        /// <returns>指定属性的最大值</returns>
        Tk Max<Tk>(Func<T, Tk> func) where Tk : IComparable<Tk>;

        /// <summary>
        /// 获取数据表中指定属性的最小值
        /// </summary>
        /// <param name="func">用于获取比较值的函数</param>
        /// <typeparam name="Tk">比较值的类型，必须实现 IComparable 接口</typeparam>
        /// <returns>指定属性的最小值</returns>
        Tk Min<Tk>(Func<T, Tk> func) where Tk : IComparable<Tk>;

        /// <summary>
        /// 计算数据表中指定属性的总和
        /// </summary>
        /// <param name="func">用于获取求和值的函数</param>
        /// <returns>指定属性的总和</returns>
        int Sum(Func<T, int> func);

        /// <summary>
        /// 计算数据表中指定属性的总和
        /// </summary>
        /// <param name="func">用于获取求和值的函数</param>
        /// <returns>指定属性的总和</returns>
        long Sum(Func<T, long> func);

        /// <summary>
        /// 计算数据表中指定属性的总和
        /// </summary>
        /// <param name="func">用于获取求和值的函数</param>
        /// <returns>指定属性的总和</returns>
        float Sum(Func<T, float> func);

        /// <summary>
        /// 计算数据表中指定属性的总和
        /// </summary>
        /// <param name="func">用于获取求和值的函数</param>
        /// <returns>指定属性的总和</returns>
        double Sum(Func<T, double> func);

        /// <summary>
        /// 计算数据表中指定属性的总和
        /// </summary>
        /// <param name="func">用于获取求和值的函数</param>
        /// <returns>指定属性的总和</returns>
        decimal Sum(Func<T, decimal> func);
    }
}
