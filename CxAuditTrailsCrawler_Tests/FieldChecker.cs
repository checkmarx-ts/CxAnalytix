using CxAnalytix.AuditTrails.Crawler;
using System;
using System.Collections.Generic;
using System.Text;

namespace CxAuditTrailsCrawler_Tests
{
	internal class FieldChecker
	{

		public static bool TypeHasMatchingTableNameProps (Type t)
		{
			var constFields = typeof(CxAuditTrailTableNameConsts).GetFields();
			bool missingField = false;

			foreach (var field in constFields)
			{
				missingField = t.GetProperty(field.Name) == null;

				if (missingField)
					break;
			}

			return !missingField;
		}
	}
}
