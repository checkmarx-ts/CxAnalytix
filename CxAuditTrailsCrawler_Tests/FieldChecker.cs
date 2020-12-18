using CxAnalytix.AuditTrails.Crawler;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CxAuditTrailsCrawler_Tests
{
	internal class FieldChecker
	{

		public static bool TypeHasMatchingTableNameProps (Type t)
		{
			return TypeHasMatchingTableNameElements<PropertyInfo>(t.GetProperty);
		}

		public static bool TypeHasMatchingTableNameMethods(Type t)
		{
			return TypeHasMatchingTableNameElements<MethodInfo> (t.GetMethod);
		}


		private static bool TypeHasMatchingTableNameElements<T>(Func<String, T> dataFunc)
		{
			var constFields = typeof(CxAuditTrailTableNameConsts).GetFields();
			bool missingField = false;

			foreach (var field in constFields)
			{
				missingField = dataFunc(field.Name) == null;

				if (missingField)
					break;
			}

			return !missingField;

		}


	}
}
