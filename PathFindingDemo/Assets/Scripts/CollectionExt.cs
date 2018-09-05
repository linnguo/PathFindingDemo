using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class CollectionExt
{
	public static T PopMin<T>(this List<T> list, Func<T, T, int> compare)
	{
		if (list.Count > 0)
		{
			int minIdx = 0;
			for (int i = 1; i < list.Count; ++i)
			{
				if (compare(list[i], list[minIdx]) < 0)
				{
					minIdx = i;
				}
			}
			T result = list[minIdx];
			list.RemoveAt(minIdx);
			return result;
		}
		
		return default(T);
	}
}
