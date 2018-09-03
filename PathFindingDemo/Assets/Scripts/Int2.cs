/*
 * Copyright (c) 2016 地水火风
 *
 * time:   2017-12-27
 * desc:   GridAStar
 * author:  ZhonglinGuo
 */

using System;

namespace MissQ
{
	// 整形的二维坐标
	[System.Serializable]
	public struct Int2 : IEquatable<Int2>
	{
		public int x;
		public int y;

		public Int2(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public static Int2 operator +(Int2 a, Int2 b)
		{
			return new Int2(a.x + b.x, a.y + b.y);
		}

		public static Int2 operator -(Int2 a, Int2 b)
		{
			return new Int2(a.x - b.x, a.y - b.y);
		}

		public override bool Equals(object obj)
		{
			return obj is Int2 && Equals((Int2)obj);
		}

		public bool Equals(Int2 other)
		{
			return x == other.x &&
				   y == other.y;
		}

		public override int GetHashCode()
		{
			var hashCode = 1502939027;
			hashCode = hashCode * -1521134295 + base.GetHashCode();
			hashCode = hashCode * -1521134295 + x.GetHashCode();
			hashCode = hashCode * -1521134295 + y.GetHashCode();
			return hashCode;
		}

		public static bool operator ==(Int2 lhs, Int2 rhs)
		{
			return (lhs.x == rhs.x && lhs.y == rhs.y);
		}

		public static bool operator !=(Int2 lhs, Int2 rhs)
		{
			return !(lhs.x == rhs.x && lhs.y == rhs.y);
		}

		public override string ToString()
		{
			return string.Format("[{0}, {1}]", x, y);
		}
	}
}