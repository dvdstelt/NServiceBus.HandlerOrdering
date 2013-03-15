﻿using System;
using System.Collections.Generic;
using System.Linq;
using NServiceBus;

namespace HandlerOrdering
{
    class OrderHandlers : ISpecifyMessageHandlerOrdering
    {

        public void SpecifyOrder(Order order)
        {
            var handlerDependencies = GetHandlerDependencies();
            var sorted = new TypeSorter(handlerDependencies).Sorted;
            order.GetType()
                 .GetProperty("Types")
                 .SetValue(order, sorted, null);
        }

        public static Dictionary<Type, List<Type>> GetHandlerDependencies()
        {
            return GetHandlerDependencies(Configure.TypesToScan);
        }

        public static Dictionary<Type, List<Type>> GetHandlerDependencies(params Type[] types)
        {
            return GetHandlerDependencies((IEnumerable<Type>)types);
        }

        public static Dictionary<Type, List<Type>> GetHandlerDependencies(IEnumerable<Type> types)
        {
            var dictionary = new Dictionary<Type, List<Type>>();
            foreach (var type in types)
            {
                var interfaces = type.GetInterfaces();
                foreach (var face in interfaces)
                {
                    if (face.GetGenericTypeDefinition() != typeof(IWantToRunAfter<>))
                    {
                        continue;
                    }
                    List<Type> dependencies;
                    if (!dictionary.TryGetValue(type, out dependencies))
                    {
                        dictionary[type] = dependencies = new List<Type>();
                    }
                    dependencies.Add(face.GenericTypeArguments.First());
                }
            }
            return dictionary;
        }

    }
}