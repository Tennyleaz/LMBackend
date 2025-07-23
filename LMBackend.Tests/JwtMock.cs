using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LMBackend.Tests
{
    internal static class JwtMock
    {
        internal static void PrepareMockJwt(ControllerBase controller, Guid userId)
        {
            // Prepare middleware
            var controllerContext = new ControllerContext();
            var httpContext = new DefaultHttpContext();
            controllerContext.HttpContext = httpContext;

            // Mock the User object to return a valid userId in JWT subject
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("sub", userId.ToString())
            }));

            controller.ControllerContext = controllerContext;
        }
    }
}
