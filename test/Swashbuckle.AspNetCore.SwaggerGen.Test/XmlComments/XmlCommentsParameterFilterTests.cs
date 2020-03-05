﻿using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using System.IO;
using System.Xml.XPath;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class XmlCommentsParameterFilterTests
    {
        [Fact]
        public void Apply_SetsDescription_FromPropertySummaryTag()
        {
            var parameter = new OpenApiParameter();
            var propertyInfo = typeof(XmlAnnotatedType).GetProperty(nameof(XmlAnnotatedType.StringProperty));
            var apiParameterDescription = new ApiParameterDescription { };
            var filterContext = new ParameterFilterContext(apiParameterDescription, null, null, propertyInfo: propertyInfo);

            Subject().Apply(parameter, filterContext);

            Assert.Equal("Summary for StringProperty", parameter.Description);
        }

        [Fact]
        public void Apply_SetsDescription_FromActionParamTag()
        {
            var parameter = new OpenApiParameter();
            var parameterInfo = typeof(FakeControllerWithXmlComments)
                .GetMethod(nameof(FakeControllerWithXmlComments.ActionWithParameter))
                .GetParameters()[0];
            var apiParameterDescription = new ApiParameterDescription { };
            var filterContext = new ParameterFilterContext(apiParameterDescription, null, null, parameterInfo: parameterInfo);

            Subject().Apply(parameter, filterContext);

            Assert.Equal("Description for param", parameter.Description);
        }

        [Fact]
        public void Apply_SetsDescription_FromUnderlyingGenericTypeActionParamTag()
        {
            var parameter = new OpenApiParameter();
            var parameterInfo = typeof(FakeConstructedControllerWithXmlComments)
                .GetMethod(nameof(FakeConstructedControllerWithXmlComments.ActionWithGenericTypeParameter))
                .GetParameters()[0];
            var apiParameterDescription = new ApiParameterDescription { };
            var filterContext = new ParameterFilterContext(apiParameterDescription, null, null, parameterInfo: parameterInfo);

            Subject().Apply(parameter, filterContext);

            Assert.Equal("Description for param", parameter.Description);
        }

        private XmlCommentsParameterFilter Subject()
        {
            using (var xmlComments = File.OpenText(GetType().Assembly.GetName().Name + ".xml"))
            {
                return new XmlCommentsParameterFilter(new XPathDocument(xmlComments));
            }
        }
    }
}
