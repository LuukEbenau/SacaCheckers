using System;
using System.Collections.Generic;
using System.Linq;

namespace Dammen
{
	public class DependencyContainer
	{
		private Dictionary<Type, object> Objects { get; set; } = new Dictionary<Type, object>();

		public TType Resolve<TType>(params object[] parameters) => _resolve<TType, TType>(parameters);

		public IType Resolve<IType, TType>(params object[] parameters) where TType : IType => _resolve<IType, TType>(parameters);

		private IType _resolve<IType, TType>(params object[] parameters)
		{
			var storedPair = this.Objects.FirstOrDefault(o => o.Key == typeof(IType));
			if(!storedPair.Equals(default(KeyValuePair<Type, object>)))
				return (IType)storedPair.Value;

			var obj = (IType)Activator.CreateInstance(typeof(TType), parameters);
			this.Objects.Add(typeof(IType), obj);
			return obj;
		}

		public TType Get<TType>()
		{
			var storedPair = this.Objects.FirstOrDefault(o => o.Key == typeof(TType));
			if(!storedPair.Equals(default(KeyValuePair<Type, object>)))
				return (TType)storedPair.Value;
			return default;
		}

		public void Remove<TType>()
		{
			this.Objects.Remove(typeof(TType));
		}

		public IType Singleton<IType, TType>(TType obj) where TType : IType
		{
			var index = this.Objects.Keys.ToList().IndexOf(obj.GetType());
			if(index != -1) {
				this.Objects[typeof(IType)] = obj;
			}
			this.Objects.Add(typeof(IType), obj);
			return obj;
		}

		public TType Singleton<TType>(TType obj) => Singleton<TType, TType>(obj);
	}
}