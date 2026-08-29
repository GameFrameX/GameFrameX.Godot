using GameFrameX.Runtime;
using Godot;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// Godot Vector 扩展测试（映射自 Unity 基准 UnityEngine.Vector2/3/4Extension）。
    /// </summary>
    public sealed class GodotVectorExtensionTests
    {
        [Fact]
        public void Vector2_ToVector3_MapsXYToXZ()
        {
            var v = new Vector2(1, 2);

            var result = v.ToVector3(5);

            Assert.Equal(new Vector3(1, 5, 2), result);
        }

        [Fact]
        public void Vector2_ToVector4_PadsZwWithZero()
        {
            var result = new Vector2(1, 2).ToVector4();

            Assert.Equal(new Vector4(1, 2, 0, 0), result);
        }

        [Fact]
        public void Vector2_AsVector3_PadsZWithZero()
        {
            var result = new Vector2(1, 2).AsVector3();

            Assert.Equal(new Vector3(1, 2, 0), result);
        }

        [Fact]
        public void Vector3_ToVector2_TakesXZ()
        {
            var result = new Vector3(1, 2, 3).ToVector2();

            Assert.Equal(new Vector2(1, 3), result);
        }

        [Fact]
        public void Vector3I_ToVector3_KeepsComponents()
        {
            var result = new Vector3I(1, 2, 3).ToVector3();

            Assert.Equal(new Vector3(1, 2, 3), result);
        }

        [Fact]
        public void Vector3_ToVector4_PadsWWithZero()
        {
            var result = new Vector3(1, 2, 3).ToVector4();

            Assert.Equal(new Vector4(1, 2, 3, 0), result);
        }

        [Fact]
        public void Vector4_ToVector2_TakesXY()
        {
            var result = new Vector4(1, 2, 3, 4).ToVector2();

            Assert.Equal(new Vector2(1, 2), result);
        }

        [Fact]
        public void Vector4_ToVector3_TakesXYZ()
        {
            var result = new Vector4(1, 2, 3, 4).ToVector3();

            Assert.Equal(new Vector3(1, 2, 3), result);
        }
    }
}
