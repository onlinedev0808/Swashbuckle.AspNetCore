﻿namespace Swashbuckle.Swagger.Fixtures
{
    public class XmlAnnotatedWithNestedType
    {
        /// <summary>
        /// summary for NestedTypeProperty
        /// </summary>
        public NestedType NestedTypeProperty { get; set; }
       
        /// <summary>
        /// summary for NestedType
        /// </summary>
        public class NestedType
        {
        }
    }
}