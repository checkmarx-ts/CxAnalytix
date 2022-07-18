using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.CxAuditTrails.DB
{
	public class TableDoesNotExistException : Exception
	{
		public TableDoesNotExistException (String dbName, String schemaName, String tableName) :
			base ($"Table {dbName}.{schemaName}.{tableName} does not exist.")
		{

		}

		public TableDoesNotExistException(String schemaName, String tableName) :
			base($"Table {schemaName}.{tableName} does not exist.")
		{

		}

		public TableDoesNotExistException(String tableName) :
			base($"Table {tableName} does not exist.")
		{

		}
	}
}
