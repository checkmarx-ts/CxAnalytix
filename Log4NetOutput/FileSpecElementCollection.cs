using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CxAnalytix.Out.Log4NetOutput
{

    [ConfigurationCollection(typeof(FileSpecElement))]
    public class FileSpecElementCollection : ConfigurationElementCollection, IEnumerable<FileSpecElement>
    {
        private static readonly String ELEM_NAME = "spec";


        protected override bool IsElementName(string elementName)
        {
            return elementName.Equals(ELEM_NAME,
              StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool IsReadOnly()
        {
            return false;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new FileSpecElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as FileSpecElement).MatchSpec;
        }

        IEnumerator<FileSpecElement> IEnumerable<FileSpecElement>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        protected override string ElementName
        {
            get
            {
                return ELEM_NAME;
            }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMapAlternate;
            }
        }


    }
}
