using System.Security.Claims;

namespace TenderquickServer.Services
{
    // Every mutating service needs "who did this" for audit and sign-off rows. Resolving the
    // claims once here keeps that parsing out of each service.
    public class CurrentUser
    {
        private readonly IHttpContextAccessor _http;

        public CurrentUser(IHttpContextAccessor http)
        {
            _http = http;
        }

        private ClaimsPrincipal? Principal => _http.HttpContext?.User;

        public int? Id
        {
            get
            {
                var sub = Principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Principal?.FindFirstValue("sub");
                return int.TryParse(sub, out var id) ? id : null;
            }
        }

        public string Name => Principal?.FindFirstValue(ClaimTypes.Name) ?? "System";

        public string? Email => Principal?.FindFirstValue(ClaimTypes.Email);

        public string? Role => Principal?.FindFirstValue(ClaimTypes.Role);
    }
}
