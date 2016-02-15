using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J.SharePoint.Lists.Attributes
{
    public class SPFieldMetadataCollection<T> : IEnumerable<SPFieldMetadata> where T : SPTypedListItem
    {
        private IEnumerable<SPFieldMetadata> _fields = null;
        private IEnumerable<SPFieldMetadata> Fields
        {
            get
            {
                if (_fields == null)
                    _fields = SPFieldMetadata.GetMetadata(typeof(T));

                return _fields;
            }
        }

        public SPFieldMetadata this[string name]
        {
            get
            {
                return Fields.Where(f => f.InternalName == name || f.Title == name).FirstOrDefault();
            }
        }

        public IEnumerator<SPFieldMetadata> GetEnumerator()
        {
            return Fields.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Fields.GetEnumerator();
        }
    }
}
