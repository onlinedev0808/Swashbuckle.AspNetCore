﻿using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json.Linq;
using Swashbuckle.Swagger.Annotations;
using Swashbuckle.Fixtures.Extensions;

namespace Swashbuckle.Fixtures.ApiDescriptions
{
    public class ActionFixtures
    {
        public void ReturnsVoid()
        { }

        public int ReturnsInt()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> ReturnsEnumerable()
        {
            throw new NotImplementedException();
        }

        public ComplexType ReturnsComplexType()
        {
            throw new NotImplementedException();
        }

        public JObject ReturnsJObject()
        {
            throw new NotImplementedException();
        }

        public IActionResult ReturnsActionResult()
        {
            throw new NotImplementedException();
        }

        public void AcceptsNothing()
        { }

        public void AcceptsStringFromRoute([FromRoute]string param)
        { }

        public void AcceptsStringFromQuery([FromQuery]string param)
        { }

        public void AcceptsComplexTypeFromQuery([FromQuery]ComplexType param)
        { }

        public void AcceptsArrayFromQuery([FromQuery]IEnumerable<string> param)
        { }

        public void AcceptsStringFromHeader([FromHeader]string param)
        { }

        public void AcceptsComplexTypeFromBody([FromBody]ComplexType param)
        { }

        [Obsolete]
        public void MarkedObsolete()
        { }

        /// <summary>
        /// summary for AnnotatedWithSummaryAndRemarksXml
        /// </summary>
        /// <remarks>
        /// type remarks
        /// </remarks>
        public void AnnotatedWithSummaryAndRemarksXml()
        {}

        /// <param name="param1">description for parma1</param>
        /// <param name="param2">description for param2</param>
        public void AnnotatedWithParamsXml(int param1, IEnumerable<ComplexType> param2)
        {}

        [SwaggerOperation("CustomOperationId", Tags = new[] { "customTag" }, Schemes = new[] { "customScheme" })]
        public void AnnotatedWithSwaggerOperation()
        { }

        [SwaggerResponseRemoveDefaults]
        public void AnnotatedWithSwaggerResponseRemoveDefaults()
        {}

        [SwaggerResponse(HttpStatusCode.Created, "ComplexType Created", typeof(int))]
        [SwaggerResponse(HttpStatusCode.Accepted, "ComplexType Initiated", typeof(int))]
        public int AnnotatedWithSwaggerResponses(ComplexType param)
        {
            throw new NotImplementedException();
        }

        [SwaggerOperationFilter(typeof(VendorExtensionsOperationFilter))]
        public void AnnotatedWithSwaggerOperationFilter()
        { }
   }
}