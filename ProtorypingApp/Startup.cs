using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using WebApplication1.Controllers;

namespace WebApplication1
{
    public class Startup
    {
        public Startup(IConfiguration configuration) { Configuration = configuration; }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddAuthentication("BasicAuthentication")
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

            services.AddMvc(options => options.EnableEndpointRouting = false)
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddOData();
            services.AddODataQueryFilter(new EnableQueryAttribute
            {
                AllowedQueryOptions = AllowedQueryOptions.All,
                PageSize = 3,
                MaxNodeCount = 5,
            });
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Needed to be able to get RouteData from HttpContext through the IHttpContextAccessor
            app.UseEndpointRouting();
            // Needed to secure the application using the standard Authorize attribute
            app.UseAuthentication();

            // OData entity model builder
            var builder = new ODataConventionModelBuilder(app.ApplicationServices);
            var values = builder.EntitySet<Value>(nameof(Value) + "s");
            values.EntityType.Filter(Microsoft.AspNet.OData.Query.QueryOptionSetting.Disabled, nameof(Value.Id));
            values.EntityType.Filter(Microsoft.AspNet.OData.Query.QueryOptionSetting.Allowed, nameof(Value.Name));

            // ************************************************
            //            app.UseMvc();
            //            app.UseOData("odata", "{tenant}/odata", builder.GetEdmModel());
            // ************************************************

            // ************************************************
            // solution suggested in the answer: https://stackoverflow.com/a/58055963/2482439
            app.UseRouter(routeBuilder => {
                var templatePrefix = "{tenant}/odata";
                var template = templatePrefix + "/{*any}";
                routeBuilder.MapMiddlewareRoute(template, appBuilder => {
                    appBuilder.UseAuthentication();
                    appBuilder.UseMvc();
                    appBuilder.UseOData("odata", templatePrefix, builder.GetEdmModel());
                });
            });
            // ************************************************

            // ************************************************
            //            app.UseMvc(routeBuilder =>
            //            {
            //                // Map OData routing adding token for the tenant based url
            //                routeBuilder.MapODataServiceRoute("odata", "{tenant}/odata", builder.GetEdmModel());
            //                routeBuilder.Select().Filter().OrderBy().MaxTop(3).Count().Expand().SkipToken();
            //                routeBuilder.SetDefaultQuerySettings(new DefaultQuerySettings
            //                {
            //                    MaxTop = 3
            //                });
            //
            //                routeBuilder.SetDefaultODataOptions(new ODataOptions{
            //                        UrlKeyDelimiter = ODataUrlKeyDelimiter.Slash
            //                });
            //
            //                // Needed to allow the injection of OData classes
            //                routeBuilder.EnableDependencyInjection();
            //            });
            // ************************************************
        }
    }
}
