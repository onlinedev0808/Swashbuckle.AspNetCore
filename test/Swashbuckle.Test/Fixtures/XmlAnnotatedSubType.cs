﻿namespace Swashbuckle.Swagger.Fixtures
{
    /// <summary>
    /// summary for XmlAnnotatedSubType
    /// </summary>
    public class XmlAnnotatedSubType : XmlAnnotatedBaseType
    {
    }

    public abstract class XmlAnnotatedBaseType
    {
        /// <summary>
        /// summary for BaseProperty
        /// </summary>
        public string BaseProperty { get; set; }
    }
}