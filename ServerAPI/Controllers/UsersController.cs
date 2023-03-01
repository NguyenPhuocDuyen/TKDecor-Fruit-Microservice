﻿using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ServerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUnitOfWork _db;
        private readonly IConfiguration _configuration;

        public UsersController(IConfiguration configuration, IUnitOfWork db)
        {
            _configuration = configuration;
            _db = db;
        }

        //GET: api/Users
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return Ok(await _db.User.GetAllAsync());
        }

        //// GET: api/Users/5s
        //[HttpGet("{id}")]
        //public async Task<ActionResult<User>> GetUser(int id)
        //{
        //    var user = await _db.User.GetFirstOrDefaultAsync(filter: x => x.Id == id, includeProperties: "Role");

        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    return user;
        //}

        //// PUT: api/Users/5
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutUser(int id, User user)
        //{
        //    if (id != user.Id)
        //    {
        //        return BadRequest();
        //    }

        //    try
        //    {
        //        _db.User.Update(user);
        //        await _db.SaveAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!await UserExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            var u = await _db.User.GetFirstOrDefaultAsync(
                filter: x=>x.Email.ToLower().Trim()
                .Equals(user.Email.ToLower().Trim()));
            if (u == null)
            {
                _db.User.Add(user);
                await _db.SaveAsync();
                return u;
            }

            return BadRequest(u);
        }

        //// DELETE: api/Users/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteUser(int id)
        //{
        //    var user = await _db.User.GetFirstOrDefaultAsync(x => x.Id == id);
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    _db.User.Remove(user);
        //    await _db.SaveAsync();

        //    return NoContent();
        //}

        //private async Task<bool> UserExists(int id)
        //{
        //    var user = await _db.User.GetFirstOrDefaultAsync(x => x.Id == id);
        //    if (user == null)
        //    {
        //        return false;
        //    }
        //    return true;
        //}

        // POST: api/Users
        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<ActionResult<User>> Login(User user)
        {
            var u = await _db.User.GetFirstOrDefaultAsync(
                filter: x=>x.Email.ToLower().Trim()
                .Equals(user.Email.ToLower().Trim())
                && x.Password.ToLower().Trim()
                .Equals(user.Password.ToLower().Trim()),
                includeProperties: "Role"
                );

            if (u == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            //tao handler
            var tokenHandler = new JwtSecurityTokenHandler();
            //encoding key in json
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            //set description for token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, u.Id.ToString()),
                    new Claim(ClaimTypes.Role, u.Role.Description.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            //create token
            var token = tokenHandler.CreateToken(tokenDescriptor);
            //convert token to string
            var tokenString = tokenHandler.WriteToken(token);
            u.Password = tokenString;

            return u;
        }
    }
}
