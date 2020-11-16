using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Configuration_Tests")]
namespace CxAnalytix.Configuration
{
	[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public sealed class SecureConfigSectionAttribute : System.Attribute
	{
		public String SensitiveStringProp { get; set; }

		internal bool IsPropSet(Type objType, Object obj)
		{
			if (obj == null)
				return false;

			if (String.IsNullOrEmpty(SensitiveStringProp) )
				return true;

			var propValue = objType.InvokeMember(SensitiveStringProp, System.Reflection.BindingFlags.GetProperty, null, obj, null);
			return !String.IsNullOrEmpty(propValue as String);
		}
	}
}

