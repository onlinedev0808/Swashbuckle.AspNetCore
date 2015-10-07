﻿using System;
using System.Collections.Generic;
using Xunit;
using Newtonsoft.Json.Serialization;
using Swashbuckle.Swagger.Fixtures;

namespace Swashbuckle.Swagger.XmlComments
{
    public class ApplyXmlTypeCommentsTests
    {
        [Theory]
        [InlineData(typeof(XmlAnnotatedType), "summary for XmlAnnotatedType")]
        [InlineData(typeof(XmlAnnotatedWithNestedType.NestedType), "summary for NestedType")]
        [InlineData(typeof(XmlAnnotatedGenericType<string>), "summary for XmlAnnotatedGenericType")]
        public void Apply_SetsDescription_FromClassSummaryTag(
            Type type,
            string expectedDescription)
        {
            var schema = new Schema
            {
                Properties = new Dictionary<string, Schema>()
            };
            var filterContext = FilterContextFor(type);

            Subject().Apply(schema, filterContext);

            Assert.Equal(expectedDescription, schema.Description);
        }

        [Theory]
        [InlineData(typeof(XmlAnnotatedType), "Property", "summary for Property")]
        [InlineData(typeof(XmlAnnotatedSubType), "BaseProperty", "summary for BaseProperty")]
        [InlineData(typeof(XmlAnnotatedGenericType<string>), "GenericProperty", "summary for GenericProperty")]
        public void Apply_SetsPropertyDescriptions_FromPropertySummaryTag(
            Type type,
            string propertyName,
            string expectedDescription)
        {
            var schema = new Schema
            {
                Properties = new Dictionary<string, Schema>()
                {
                    { propertyName, new Schema() }
                }
            };
            var filterContext = FilterContextFor(type);

            Subject().Apply(schema, filterContext);

            Assert.Equal(expectedDescription, schema.Properties[propertyName].Description);
        }

        private ModelFilterContext FilterContextFor(Type type)
        {
            var jsonObjectContract = new DefaultContractResolver().ResolveContract(type);
            return new ModelFilterContext(type, (jsonObjectContract as JsonObjectContract), null);
        }

        private ApplyXmlTypeComments Subject()
        {
            return new ApplyXmlTypeComments("Fixtures/XmlComments.xml");
        }
    }
}