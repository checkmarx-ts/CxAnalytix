using System;

namespace CxAnalytix.Configuration.Utils
{
    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class RecordNameConfigAttribute : Attribute
    {
        public RecordNameConfigAttribute() { }
    }
}
