using System;

namespace GameFrameX.AssetSystem
{
	[AttributeUsage(AttributeTargets.All, Inherited = false)]
	public sealed class AssetSystemPreserveAttribute : Attribute
	{
		public bool AllMembers;
		public bool Conditional;
	}
}
