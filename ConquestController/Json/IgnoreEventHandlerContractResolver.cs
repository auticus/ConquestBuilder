using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ConquestController.Json
{
    public class IgnoreEventHandlerContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (typeof(EventHandler).IsAssignableFrom(property.PropertyType))
            {
                property.Ignored = true;
            }

            return property;
        }
    }
}
