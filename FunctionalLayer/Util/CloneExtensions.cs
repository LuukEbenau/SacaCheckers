using System;
using System.Collections.Generic;
using System.Linq;

namespace FunctionalLayer.Util
{
	public static class CloneExtensions
	{
		public static IEnumerable<TType> Clone<TType>(this IEnumerable<TType> items) where TType : ICloneable
		{
			return items.Select(item => (TType)item.Clone());
		}

		//public static TType Clone<TType>(this TType obj) where TType : ICloneable {
		//    return (TType)obj.Clone();
		//}
	}
}