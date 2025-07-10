using System.Security.Claims;

namespace LMBackend
{
    public static class MyClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            // In my JWT, the user id is stored in the subject of the claim. Also check NameIdentifier just to be safe.
            string userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? user.FindFirst("sub")?.Value;

            return userId != null ? Guid.Parse(userId) : Guid.Empty;
        }
    }
}
