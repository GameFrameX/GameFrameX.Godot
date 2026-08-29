using System.Threading;

namespace System
{
    /// <summary>
    /// 线程私有random对象
    /// </summary>
    public static class ThreadLocalRandom
    {
        private static int _seed = Environment.TickCount;

        // 迁移备注：_customSeed 与非 readonly 的 _rng 为 Unity 基准 SetSeed 增量所需（2026-08 补齐）。
        private static int? _customSeed;

        private static ThreadLocal<Random> _rng = new ThreadLocal<Random>(() => new Random(_customSeed ?? Interlocked.Increment(ref _seed)));

        /// <summary>
        /// The current random number seed available to this thread
        /// </summary>
        public static Random Current
        {
            get { return _rng.Value; }
        }

        /// <summary>
        /// 设置随机种子（用于测试或可重现的随机序列）
        /// </summary>
        /// <remarks>
        /// Sets a custom seed for the random number generator, useful for testing or reproducible random sequences.
        /// </remarks>
        /// <param name="seed">随机种子值 / The seed value for the random number generator</param>
        public static void SetSeed(int seed)
        {
            _customSeed = seed;
            var oldRng = _rng;
            _rng = new ThreadLocal<Random>(() => new Random(_customSeed ?? Interlocked.Increment(ref _seed)));
            oldRng.Dispose();
        }

        /// <summary>
        /// 获取Int64范围内的随机数
        /// </summary>
        /// <remarks>
        /// Generates a random number within the Int64 range.
        /// </remarks>
        /// <returns>返回一个随机的Int64值 / A random Int64 value</returns>
        public static long NextInt64()
        {
            var bytes = new byte[8];
            Current.NextBytes(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }

        /// <summary>
        /// 获取UInt64范围内的随机数
        /// </summary>
        /// <remarks>
        /// Generates a random number within the UInt64 range.
        /// </remarks>
        /// <returns>返回一个随机的UInt64值 / A random UInt64 value</returns>
        public static ulong NextUInt64()
        {
            var bytes = new byte[8];
            Current.NextBytes(bytes);
            return BitConverter.ToUInt64(bytes, 0);
        }
    }
}
