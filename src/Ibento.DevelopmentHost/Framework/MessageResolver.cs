using System;
using System.Collections.Generic;
using System.Linq;
using Ibento.DevelopmentHost.Messaging;

namespace Ibento.DevelopmentHost.Framework
{
    public class MessageResolver
    {
        private readonly IReadOnlyDictionary<string, Type> _typeMap;
        private static readonly IReadOnlyDictionary<string, Type> TypeIndex = typeof(Message)
            .Assembly
            .GetTypes()
            .Where(_ =>
                _.Namespace != null &&
                _.Namespace.Equals("Ibento.DevelopmentHost.Messaging") &&
                !_.IsInterface &&
                !_.IsAbstract &&
                typeof(Message).IsAssignableFrom(_))
            .ToDictionary(key => key.Name, value => value);
        
        public static readonly MessageResolver Default = new MessageResolver(TypeIndex);
        
        public MessageResolver(IReadOnlyDictionary<string, Type> typeMap)
        {
            _typeMap = typeMap;
        }

        public Type Resolve(string typeName)
        {
            return TypeIndex[typeName];
        }
    }
}