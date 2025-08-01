﻿using LMBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Asp.Versioning;

namespace LMBackend.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class UsersController : ControllerBase
{
    private readonly ChatContext _context;
    private readonly IConfiguration _config;

    public UsersController(ChatContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (_context.Users.Any(u => u.Name == request.UserName))
        {
            return BadRequest("Username taken");
        }

        User user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.UserName,
            HashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        User user = _context.Users.SingleOrDefault(u => u.Name == request.UserName);
        if (user == null)
        {
            return NotFound();
        }
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.HashedPassword))
        {
            return Unauthorized();
        }

        string token = GenerateJwtToken(user);

        return Ok(new { token });
    }

    private string GenerateJwtToken(User user)
    {
        Claim[] claims =
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),  // subject claim
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Name)  // unique name claim
        };

        string jwt = _config["Jwt:Key"];  // secret key from app settings
        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt));
        SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // GET: api/Users/5
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        // because chats are lazy-loaded, we need to eagerly load the related chats
        var user = await _context.Users.Include(u => u.Chats).FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }
        UserDto dto = UserDto.FromUser(user);
        return dto;
    }

    /// <summary>
    /// Get my user info and chats.
    /// </summary>
    /// <returns></returns>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<User>> GetMe()
    {
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }
        User user = await _context.Users.Include(u => u.Chats.OrderByDescending(x => x.CreatedTime)).FirstOrDefaultAsync(u => u.Id == userId);
        return Ok(user);
    }

    // DELETE: api/Users/5
    /// <summary>
    /// Delete user and all its chats.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        // Check the JWT id and user id. An user could only delete itself
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Get all items from user and delete them
        foreach (var chat in user.Chats)
        {
            _context.ChatMessages.RemoveRange(chat.Messages);
        }
        _context.Chats.RemoveRange(user.Chats);
        // Remove user lastly
        _context.Users.Remove(user);
        
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class RegisterRequest
{
    public string UserName { get; set; }
    public string Password { get; set; }
}

public class LoginRequest
{
    public string UserName { get; set; }
    public string Password { get; set; }
}
