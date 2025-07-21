using System.Security.Claims;

namespace LMBackend
{
    public static class MyClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            if (user == null)
                return Guid.Empty;

            // In my JWT, the user id is stored in the subject of the claim. Also check NameIdentifier just to be safe.
            string userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? user.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Guid.Empty;
            if (Guid.TryParse(userId, out Guid parsed))
                return parsed;
            return Guid.Empty;
        }
    }
}
