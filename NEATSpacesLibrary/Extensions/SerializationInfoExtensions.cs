using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace NEATSpacesLibrary.Extensions
{
    public static class SerializationInfoExtensions
    {
        public static T GetValue<T>(this SerializationInfo self, string name)
        {
            return (T)self.GetValue(name, typeof(T));
        }
    }
}
