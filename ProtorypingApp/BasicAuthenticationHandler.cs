using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebApplication1
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IHttpContextAccessor httpContextAccessor)
            : base(options, logger, encoder, clock)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetTenant()
        {
            var httpContext = _httpContextAccessor?.HttpContext;

            // In OData this is always null
            var routeData = httpContext?.GetRouteData();
            var tenant = routeData?.Values["tenant"]?.ToString();

            // Work around to let this work on odata controllers (where RouteAttribute is missing)
            if (string.IsNullOrEmpty(tenant))
            {
                var path = httpContext?.Request?.Path.Value.ToLowerInvariant();
                var indexOfOdataSegment = path?.IndexOf("/odata/") ?? 0;
                tenant = path?.Substring(1, indexOfOdataSegment - 1);
            }

            return tenant;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Missing Authorization Header");

            try {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
                var username = credentials[0];
                var password = credentials[1];
              
                var tenant = GetTenant();

                if (string.IsNullOrEmpty(tenant))
                {
                    return AuthenticateResult.Fail("Unknown tenant");
                }

                if(string.IsNullOrEmpty(username) || username != password)
                    return AuthenticateResult.Fail("Wrong username or password");
        }
            catch (Exception e)
            {
                return AuthenticateResult.Fail("Unable to authenticate");
            }

            var claims = new[] {
                new Claim("Tenant", "tenant id")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.Headers["WWW-Authenticate"] = "Basic realm=\"Oh my OData\", charset=\"UTF-8\"";
            await base.HandleChallengeAsync(properties);
        }
    }
}
