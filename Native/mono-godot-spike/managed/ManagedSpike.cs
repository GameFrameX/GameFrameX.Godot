using System;
using System.Runtime.CompilerServices;

namespace ManagedSpike
{
    // C1 Spike-B/C：热重载与双向交互验证。同一源码经 DefineConstants 构建为
    // ManagedSpikeV1 / ManagedSpikeV2 两个程序集，验证多版本共存与静态状态隔离。
    // Spike-C 增补：数组 marshalling、实例对象生命周期、InternalCall 反向调用。
    public static class Spike
    {
        private static int counter = 0;

        public static string Hello()
        {
#if SPIKE_VERSION_TWO
            return "hello from mono assembly version two";
#else
            return "hello from mono assembly version one";
#endif
        }

        public static int Increment()
        {
            counter += 1;
            return counter;
        }

        public static int Add(int leftValue, int rightValue)
        {
            return leftValue + rightValue;
        }


        private static int tickCount = 0;

        // 帧驱动入口：热更循环 demo 用，返回版本标记 + 累计 tick 数
        // （各程序集版本静态状态独立，切换后从零计数）
        public static string Tick(int frameIndex)
        {
            tickCount += 1;
#if SPIKE_VERSION_TWO
            return "logic version two tick " + tickCount.ToString() + " at frame " + frameIndex.ToString();
#else
            return "logic version one tick " + tickCount.ToString() + " at frame " + frameIndex.ToString();
#endif
        }
        // 数组跨边界：native 构造 int[] 传入，托管求和返回
        public static int SumValues(int[] values)
        {
            int total = 0;
            foreach (int value in values)
            {
                total += value;
            }
            return total;
        }


        // blittable 结构体按值跨边界：native 侧以相同内存布局传参/接返回
        public struct NativeVector3
        {
            public float X;
            public float Y;
            public float Z;

            public NativeVector3(float x, float y, float z)
            {
                X = x;
                Y = y;
                Z = z;
            }
        }

        public static NativeVector3 MovePoint(NativeVector3 point, float delta)
        {
            return new NativeVector3(point.X + delta, point.Y + delta, point.Z + delta);
        }

        public static double EchoDouble(double value)
        {
            return value * 2.0;
        }
        // InternalCall：托管声明，实现由 native 侧经 mono_add_internal_call 注册
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern int NativeMultiply(int leftValue, int rightValue);

        public static int MultiplyThroughNative(int leftValue, int rightValue)
        {
            return NativeMultiply(leftValue, rightValue);
        }

        public static string ThrowIf(bool shouldThrow)
        {
            if (shouldThrow)
            {
#if SPIKE_VERSION_TWO
                throw new InvalidOperationException("spike-b expected exception from version two");
#else
                throw new InvalidOperationException("spike-b expected exception from version one");
#endif
            }
#if SPIKE_VERSION_TWO
            return "no throw from version two";
#else
            return "no throw from version one";
#endif
        }
    }

    // 实例对象生命周期：native 构造实例、多次驱动实例方法、读取状态
    public sealed class SpikeInstance
    {
        private int accumulatedValue = 0;
        private readonly string identity;

        public SpikeInstance(string identity)
        {
            this.identity = identity;
        }

        public int AddValue(int amount)
        {
            accumulatedValue += amount;
            return accumulatedValue;
        }

        public string Describe()
        {
            // 用拼接而非插值/Format：避开 Exception/SR 资源路径的已知崩溃面
            return "instance " + identity + " accumulated " + accumulatedValue.ToString();
        }
    }
}
