﻿using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Any;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class OpenApiPrimitiveFactory
    {
        public static IOpenApiPrimitive CreateFrom(object value)
        {
            if (!FactoryMethodMap.TryGetValue(value.GetType(), out Func<object, IOpenApiPrimitive> factoryMethod))
                return null;

            return factoryMethod(value);
        }

        private static readonly Dictionary<Type, Func<object, IOpenApiPrimitive>> FactoryMethodMap = new Dictionary<Type, Func<object, IOpenApiPrimitive>>
        {
            { typeof(string), (value) => new OpenApiString((string)value) },
            { typeof(short), (value) => new OpenApiInteger((int)value) },
            { typeof(ushort), (value) => new OpenApiInteger((int)value) },
            { typeof(int), (value) => new OpenApiInteger((int)value) },
            { typeof(uint), (value) => new OpenApiInteger((int)value) },
            { typeof(long), (value) => new OpenApiLong((long)value) },
            { typeof(ulong), (value) => new OpenApiLong((long)value) },
            { typeof(float), (value) => new OpenApiFloat((float)value) },
            { typeof(double), (value) => new OpenApiDouble((double)value) },
            { typeof(decimal), (value) => new OpenApiDouble((double)value) },
            { typeof(byte), (value) => new OpenApiByte((byte)value) },
            { typeof(sbyte), (value) => new OpenApiByte((byte)value) },
            { typeof(bool), (value) => new OpenApiBoolean((bool)value) },
            { typeof(DateTime), (value) => new OpenApiDate((DateTime)value) },
            { typeof(DateTimeOffset), (value) => new OpenApiDateTime((DateTimeOffset)value) }
        };
    }
}
