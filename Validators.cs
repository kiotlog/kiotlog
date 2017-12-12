using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace KiotlogDB
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class NotNullOrEmptyCollectionAttribute : RequiredAttribute
    {
        public override bool IsValid(object value)
        {
            // var collection = value as ICollection;
            // if (collection != null)
            // {
            //     return collection.Count != 0;
            // }

            var enumerable = value as IEnumerable;
            return enumerable != null && enumerable.GetEnumerator().MoveNext();
        }
    }
}