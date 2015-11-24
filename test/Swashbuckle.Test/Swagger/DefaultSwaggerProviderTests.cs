﻿using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Xunit;
using Swashbuckle.Swagger.Fixtures.ApiDescriptions;
using Swashbuckle.Swagger.Fixtures.Extensions;

namespace Swashbuckle.Swagger
{
    public class DefaultSwaggerProviderTests
    {
        [Fact]
        public void GetSwagger_ReturnsDefaultInfoVersionAndTitle()
        {
            var swagger = Subject().GetSwagger("v1");

            Assert.Equal("v1", swagger.Info.Version);
            Assert.Equal("API V1", swagger.Info.Title);
        }

        [Fact]
        public void GetSwagger_IncludesPathItem_PerRelativePathSansQueryString()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", "collection1", nameof(ActionFixtures.ReturnsEnumerable))
                .Add("GET", "collection1/{id}", nameof(ActionFixtures.ReturnsComplexType))
                .Add("GET", "collection2", nameof(ActionFixtures.AcceptsStringFromQuery))
                .Add("PUT", "collection2", nameof(ActionFixtures.ReturnsVoid))
                .Add("GET", "collection2/{id}", nameof(ActionFixtures.ReturnsComplexType))
            );

            var swagger = subject.GetSwagger("v1");

            Assert.Equal(new[]
                {
                    "/collection1",
                    "/collection1/{id}",
                    "/collection2",
                    "/collection2/{id}"
                },
                swagger.Paths.Keys.ToArray());
        }

        [Fact]
        public void GetSwagger_IncludesOperation_PerHttpMethodPerRelativePathSansQueryString()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", "collection", nameof(ActionFixtures.ReturnsEnumerable))
                .Add("PUT", "collection/{id}", nameof(ActionFixtures.ReturnsVoid))
                .Add("POST", "collection", nameof(ActionFixtures.ReturnsInt))
                .Add("DELETE", "collection/{id}", nameof(ActionFixtures.ReturnsVoid))
                .Add("PATCH", "collection/{id}", nameof(ActionFixtures.ReturnsVoid))
                // TODO: OPTIONS & HEAD
            );

            var swagger = subject.GetSwagger("v1");

            // GET collection
            var operation = swagger.Paths["/collection"].Get;
            Assert.NotNull(operation);
            Assert.Equal("ReturnsEnumerable", operation.OperationId);
            Assert.Equal(new[] { "application/json", "text/json" }, operation.Produces.ToArray());
            Assert.False(operation.Deprecated);
            // PUT collection/{id}
            operation = swagger.Paths["/collection/{id}"].Put;
            Assert.NotNull(operation);
            Assert.Equal("ReturnsVoid", operation.OperationId);
            Assert.Empty(operation.Produces.ToArray());
            Assert.False(operation.Deprecated);
            // POST collection
            operation = swagger.Paths["/collection"].Post;
            Assert.NotNull(operation);
            Assert.Equal("ReturnsInt", operation.OperationId);
            Assert.Equal(new[] { "application/json", "text/json" }, operation.Produces.ToArray());
            Assert.False(operation.Deprecated);
            // DELETE collection/{id}
            operation = swagger.Paths["/collection/{id}"].Delete;
            Assert.NotNull(operation);
            Assert.Equal("ReturnsVoid", operation.OperationId);
            Assert.Empty(operation.Produces.ToArray());
            Assert.False(operation.Deprecated);
            // PATCH collection
            operation = swagger.Paths["/collection/{id}"].Patch;
            Assert.NotNull(operation);
            Assert.Equal("ReturnsVoid", operation.OperationId);
            Assert.Empty(operation.Produces.ToArray());
            Assert.False(operation.Deprecated);
        }

        [Fact]
        public void GetSwagger_SetsParametersToNull_ForParameterlessActions()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", "collection", nameof(ActionFixtures.AcceptsNothing)));

            var swagger = subject.GetSwagger("v1");

            var operation = swagger.Paths["/collection"].Get;
            Assert.Null(operation.Parameters);
        }

        [Theory]
        [InlineData("collection/{param}", nameof(ActionFixtures.AcceptsStringFromRoute), "path")]
        [InlineData("collection", nameof(ActionFixtures.AcceptsStringFromQuery), "query")]
        [InlineData("collection", nameof(ActionFixtures.AcceptsStringFromHeader), "header")]
        public void GetSwagger_CreatesNonBodyParameter_ForNonBodyParams(
            string routeTemplate,
            string actionFixtureName,
            string expectedIn)
        {
            var subject = Subject(setupApis: apis => apis.Add("GET", routeTemplate, actionFixtureName));

            var swagger = subject.GetSwagger("v1");

            var param = swagger.Paths["/" + routeTemplate].Get.Parameters.First();
            Assert.IsAssignableFrom<NonBodyParameter>(param);
            var nonBodyParam = param as NonBodyParameter;
            Assert.Equal("param", nonBodyParam.Name);
            Assert.Equal(expectedIn, nonBodyParam.In);
            Assert.Equal("string", nonBodyParam.Type);
        }

        [Fact]
        public void GetSwagger_SetsCollectionFormatMulti_ForNonBodyArrayParams()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", "resource", nameof(ActionFixtures.AcceptsArrayFromQuery)));

            var swagger = subject.GetSwagger("v1");

            var param = (NonBodyParameter)swagger.Paths["/resource"].Get.Parameters.First();
            Assert.Equal("multi", param.CollectionFormat);
        }

        [Fact]
        public void GetSwagger_CreatesBodyParam_ForFromBodyParams()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("POST", "collection", nameof(ActionFixtures.AcceptsComplexTypeFromBody)));

            var swagger = subject.GetSwagger("v1");

            var param = swagger.Paths["/collection"].Post.Parameters.First();
            Assert.IsAssignableFrom<BodyParameter>(param);
            var bodyParam = param as BodyParameter;
            Assert.Equal("param", bodyParam.Name);
            Assert.Equal("body", bodyParam.In);
            Assert.NotNull(bodyParam.Schema);
            Assert.Equal("#/definitions/ComplexType", bodyParam.Schema.Ref);
            Assert.Contains("ComplexType", swagger.Definitions.Keys);
        }

        [Theory]
        [InlineData("collection/{param}")]
        [InlineData("collection/{param?}")]
        public void GetSwagger_SetsParameterRequired_ForAllRouteParams(string routeTemplate)
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", routeTemplate, nameof(ActionFixtures.AcceptsStringFromRoute)));

            var swagger = subject.GetSwagger("v1");

            var param = swagger.Paths["/collection/{param}"].Get.Parameters.First();
            Assert.Equal(true, param.Required);
        }

        [Theory]
        [InlineData(nameof(ActionFixtures.AcceptsStringFromQuery))]
        [InlineData(nameof(ActionFixtures.AcceptsStringFromHeader))]
        public void GetSwagger_SetsParameterRequiredFalse_ForQueryAndHeaderParams(string actionFixtureName)
        {
            var subject = Subject(setupApis: apis => apis.Add("GET", "collection", actionFixtureName));

            var swagger = subject.GetSwagger("v1");

            var param = swagger.Paths["/collection"].Get.Parameters.First();
            Assert.Equal(false, param.Required);
        }

        [Fact]
        public void GetSwagger_SetsParameterTypeString_ForUnboundRouteParams()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", "collection/{param}", nameof(ActionFixtures.AcceptsNothing)));

            var swagger = subject.GetSwagger("v1");

            var param = swagger.Paths["/collection/{param}"].Get.Parameters.First();
            Assert.IsAssignableFrom<NonBodyParameter>(param);
            var nonBodyParam = param as NonBodyParameter;
            Assert.Equal("param", nonBodyParam.Name);
            Assert.Equal("path", nonBodyParam.In);
            Assert.Equal("string", nonBodyParam.Type);
        }

        [Fact]
        public void GetSwagger_ExpandsOutQueryParameters_ForComplexParamsWithFromQueryAttribute()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", "collection", nameof(ActionFixtures.AcceptsComplexTypeFromQuery)));

            var swagger = subject.GetSwagger("v1");

            var operation = swagger.Paths["/collection"].Get;
            Assert.Equal(5, operation.Parameters.Count);
            Assert.Equal("Property1", operation.Parameters[0].Name);
            Assert.Equal("Property2", operation.Parameters[1].Name);
            Assert.Equal("Property3", operation.Parameters[2].Name);
            Assert.Equal("Property4", operation.Parameters[3].Name);
            Assert.Equal("Property5", operation.Parameters[4].Name);
        }

        [Theory]
        [InlineData(nameof(ActionFixtures.ReturnsVoid), "204", false)]
        [InlineData(nameof(ActionFixtures.ReturnsEnumerable), "200", true)]
        [InlineData(nameof(ActionFixtures.ReturnsComplexType), "200", true)]
        [InlineData(nameof(ActionFixtures.ReturnsJObject), "200", true)]
        [InlineData(nameof(ActionFixtures.ReturnsActionResult), "200", false)]
        public void GetSwagger_SetsResponseStatusCodeAndSchema_AccordingToActionReturnType(
            string actionFixtureName,
            string expectedStatusCode,
            bool expectsSchema)
        {
            var subject = Subject(setupApis: apis =>
                apis.Add("GET", "collection", actionFixtureName));

            var swagger = subject.GetSwagger("v1");

            var responses = swagger.Paths["/collection"].Get.Responses;
            Assert.Equal(new[] { expectedStatusCode }, responses.Keys.ToArray());
            var response = responses[expectedStatusCode];
            if (expectsSchema)
                Assert.NotNull(response.Schema);
            else
                Assert.Null(response.Schema);
        }

        [Fact]
        public void GetSwagger_SetsDeprecated_ForActionsMarkedObsolete()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", "collection", nameof(ActionFixtures.MarkedObsolete)));

            var swagger = subject.GetSwagger("v1");

            var operation = swagger.Paths["/collection"].Get;
            Assert.True(operation.Deprecated);
        }

        [Fact]
        public void GetSwagger_SupportsOptionToProvideAdditionalApiInfo()
        {
            var subject = Subject(
                configure: opts => opts.SingleApiVersion(new Info { Version = "v3", Title = "My API" }));

            var swagger = subject.GetSwagger("v3");

            Assert.NotNull(swagger.Info);
            Assert.Equal("v3", swagger.Info.Version);
            Assert.Equal("My API", swagger.Info.Title);
            // TODO: More ...
        }

        [Fact]
        public void GetSwagger_SupportsOptionToDescribeMultipleApiVersions()
        {
            var subject = Subject(
                setupApis: apis =>
                {
                    apis.Add("GET", "v1/collection", nameof(ActionFixtures.ReturnsEnumerable));
                    apis.Add("GET", "v2/collection", nameof(ActionFixtures.ReturnsEnumerable));
                },
                configure: opts =>
                {
                    opts.MultipleApiVersions(
                        new []
                        {
                            new Info { Version = "v2", Title = "API V2" },
                            new Info { Version = "v1", Title = "API V1" }
                        },
                        (apiDesc, targetApiVersion) => apiDesc.RelativePath.StartsWith(targetApiVersion));
                });

            var v2Swagger = subject.GetSwagger("v2");
            var v1Swagger = subject.GetSwagger("v1");

            Assert.Equal(new[] { "/v2/collection" }, v2Swagger.Paths.Keys.ToArray());
            Assert.Equal(new[] { "/v1/collection" }, v1Swagger.Paths.Keys.ToArray());
        }

        [Fact]
        public void GetSwagger_SupportsOptionToDefineBasicAuthScheme()
        {
            var subject = Subject(configure: opts =>
                opts.SecurityDefinitions.Add("basic", new BasicAuthScheme
                {
                    Type = "basic",
                    Description = "Basic HTTP Authentication"
                }));

            var swagger = subject.GetSwagger("v1");

            Assert.Contains("basic", swagger.SecurityDefinitions.Keys);
            var scheme = swagger.SecurityDefinitions["basic"];
            Assert.Equal("basic", scheme.Type);
            Assert.Equal("Basic HTTP Authentication", scheme.Description);
        }

        [Fact]
        public void GetSwagger_SupportsOptionToDefineApiKeyScheme()
        {
            var subject = Subject(configure: opts =>
                opts.SecurityDefinitions.Add("apiKey", new ApiKeyScheme
                {
                    Type = "apiKey",
                    Description = "API Key Authentication",
                    Name = "apiKey",
                    In = "header"
                }));

            var swagger = subject.GetSwagger("v1");

            Assert.Contains("apiKey", swagger.SecurityDefinitions.Keys);
            var scheme = swagger.SecurityDefinitions["apiKey"];
            Assert.IsAssignableFrom<ApiKeyScheme>(scheme);
            var apiKeyScheme = scheme as ApiKeyScheme;
            Assert.Equal("apiKey", apiKeyScheme.Type);
            Assert.Equal("API Key Authentication", apiKeyScheme.Description);
            Assert.Equal("apiKey", apiKeyScheme.Name);
            Assert.Equal("header", apiKeyScheme.In);
        }

        [Fact]
        public void GetSwagger_SupportsOptionToDefineOAuth2Scheme()
        {
            var subject = Subject(configure: opts =>
                opts.SecurityDefinitions.Add("oauth2", new OAuth2Scheme
                {
                    Type = "oauth2",
                    Description = "OAuth2 Authorization Code Grant",
                    Flow = "accessCode",
                    AuthorizationUrl = "https://tempuri.org/auth",
                    TokenUrl = "https://tempuri.org/token",
                    Scopes = new Dictionary<string, string>
                    {
                        { "read", "Read access to protected resources" },
                        { "write", "Write access to protected resources" }
                    }
                }));

            var swagger = subject.GetSwagger("v1");

            Assert.Contains("oauth2", swagger.SecurityDefinitions.Keys);
            var scheme = swagger.SecurityDefinitions["oauth2"];
            Assert.IsAssignableFrom<OAuth2Scheme>(scheme);
            var oAuth2Scheme = scheme as OAuth2Scheme;
            Assert.Equal("oauth2", oAuth2Scheme.Type);
            Assert.Equal("OAuth2 Authorization Code Grant", oAuth2Scheme.Description);
            Assert.Equal("accessCode", oAuth2Scheme.Flow);
            Assert.Equal("https://tempuri.org/auth", oAuth2Scheme.AuthorizationUrl);
            Assert.Equal("https://tempuri.org/token", oAuth2Scheme.TokenUrl);
            Assert.Equal(new[] { "read", "write" }, oAuth2Scheme.Scopes.Keys.ToArray());
            Assert.Equal("Read access to protected resources", oAuth2Scheme.Scopes["read"]);
            Assert.Equal("Write access to protected resources", oAuth2Scheme.Scopes["write"]);
        }

        [Fact]
        public void GetSwagger_SupportsOptionToIgnoreObsoleteActions()
        {
            var subject = Subject(
                setupApis: apis =>
                {
                    apis.Add("GET", "collection1", nameof(ActionFixtures.ReturnsEnumerable));
                    apis.Add("GET", "collection2", nameof(ActionFixtures.MarkedObsolete));
                },
                configure: opts => opts.IgnoreObsoleteActions = true);

            var swagger = subject.GetSwagger("v1");

            Assert.Equal(new[] { "/collection1" }, swagger.Paths.Keys.ToArray());
        }

        [Fact]
        public void GetSwagger_SupportsOptionToCustomizeGroupingOfActions()
        {
            var subject = Subject(
                setupApis: apis =>
                {
                    apis.Add("GET", "collection1", nameof(ActionFixtures.ReturnsEnumerable));
                    apis.Add("GET", "collection2", nameof(ActionFixtures.ReturnsInt));
                },
                configure: opts => opts.GroupActionsBy((apiDesc) => apiDesc.RelativePath));

            var swagger = subject.GetSwagger("v1");

            Assert.Equal(new[] { "collection1" }, swagger.Paths["/collection1"].Get.Tags);
            Assert.Equal(new[] { "collection2" }, swagger.Paths["/collection2"].Get.Tags);
        }

        [Fact]
        public void GetSwagger_SupportsOptionToCustomizeOderingOfActionGroups()
        {
            var subject = Subject(
                setupApis: apis =>
                {
                    apis.Add("GET", "A", nameof(ActionFixtures.ReturnsVoid));
                    apis.Add("GET", "B", nameof(ActionFixtures.ReturnsVoid));
                    apis.Add("GET", "F", nameof(ActionFixtures.ReturnsVoid));
                    apis.Add("GET", "D", nameof(ActionFixtures.ReturnsVoid));
                },
                configure: opts =>
                {
                    opts.GroupActionsBy((apiDesc) => apiDesc.RelativePath);
                    opts.OrderActionGroupsBy(new DescendingAlphabeticComparer());
                });

            var swagger = subject.GetSwagger("v1");

            Assert.Equal(new[] { "/F", "/D", "/B", "/A" }, swagger.Paths.Keys.ToArray());
        }

        [Fact]
        public void GetSwagger_SupportsOptionToPostModifyOperations()
        {
            var subject = Subject(
                setupApis: apis =>
                {
                    apis.Add("GET", "collection", nameof(ActionFixtures.ReturnsEnumerable));
                },
                configure: opts =>
                {
                    opts.OperationFilter<VendorExtensionsOperationFilter>();
                });

            var swagger = subject.GetSwagger("v1");
            
            var operation = swagger.Paths["/collection"].Get;
            Assert.NotEmpty(operation.Extensions);
        }

        [Fact]
        public void GetSwagger_SupportsOptionToPostModifySwaggerDocument()
        {
            var subject = Subject(configure: opts =>
                opts.DocumentFilter<VendorExtensionsDocumentFilter>());

            var swagger = subject.GetSwagger("v1");
            
            Assert.NotEmpty(swagger.Extensions);
        }

        [Fact]
        public void GetSwagger_HandlesUnboundRouteParams()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", "{version}/collection", nameof(ActionFixtures.AcceptsNothing)));

            var swagger = subject.GetSwagger("v1");

            var param = swagger.Paths["/{version}/collection"].Get.Parameters.First();
            Assert.Equal("version", param.Name);
            Assert.Equal(true, param.Required);
        }

        [Fact]
        public void GetSwagger_ThrowsInformativeException_OnOverloadedActions()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", "collection", nameof(ActionFixtures.AcceptsNothing))
                .Add("GET", "collection", nameof(ActionFixtures.AcceptsStringFromQuery))
            );

            var exception = Assert.Throws<NotSupportedException>(() => subject.GetSwagger("v1"));
            Assert.Equal(
                "Multiple operations with path 'collection' and method 'GET'. Are you overloading action methods?",
                exception.Message);
        }

        [Fact]
        public void GetSwagger_ThrowsInformativeException_OnUnspecifiedHttpMethod()
        {
            var subject = Subject(setupApis: apis => apis
                .Add(null, "collection", nameof(ActionFixtures.AcceptsNothing)));

            var exception = Assert.Throws<NotSupportedException>(() => subject.GetSwagger("v1"));
            Assert.Equal(
                "Unbounded HTTP verbs for path 'collection'. Are you missing an HttpMethodAttribute?",
                exception.Message);
        }

        private DefaultSwaggerProvider Subject(
            Action<FakeApiDescriptionGroupCollectionProvider> setupApis = null,
            Action<SwaggerDocumentOptions> configure = null)
        {
            var apiDescriptionsProvider = new FakeApiDescriptionGroupCollectionProvider();
            if (setupApis != null) setupApis(apiDescriptionsProvider);

            var options = new SwaggerDocumentOptions();
            if (configure != null) configure(options);

            return new DefaultSwaggerProvider(
                apiDescriptionsProvider,
                new DefaultSchemaRegistryFactory(new JsonSerializerSettings(), new SwaggerSchemaOptions()),
                options
            );
        }
    }
}