using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreAPI.Controllers;
using StoreAPI.Data;
using StoreAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Store.Tests
{
    public class UsersControllerTests : IDisposable
    {
        private readonly StoreContext _context;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            var options = new DbContextOptionsBuilder<StoreContext>()
                .UseInMemoryDatabase(databaseName: "TestUserDb")
                .Options;
            _context = new StoreContext(options);
            _context.Database.EnsureCreated();

            _controller = new UsersController(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetUsers_ReturnsAllUsers()
        {
            _context.Users.AddRange(
                new User { Id = 1, Username = "User 1" },
                new User { Id = 2, Username = "User 2" }
            );
            _context.SaveChanges();

            var result = await _controller.GetUsers();

            var actionResult = Assert.IsType<ActionResult<IEnumerable<User>>>(result);
            var users = Assert.IsType<List<User>>(actionResult.Value);
            Assert.Equal(2, users.Count);
        }

        [Fact]
        public async Task GetUser_ReturnsUser_WhenUserExists()
        {
            var expectedUser = new User { Id = 1, Username = "Alice" };
            _context.Users.Add(expectedUser);
            _context.SaveChanges();

            var result = await _controller.GetUser(1);

            var actionResult = Assert.IsType<ActionResult<User>>(result);
            var user = Assert.IsType<User>(actionResult.Value);
            Assert.Equal(expectedUser.Id, user.Id);
        }

        [Fact]
        public async Task PostUser_AddsUser_WhenCalled()
        {
            var newUser = new User { Username = "Bob" };

            var result = await _controller.PostUser(newUser);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var user = Assert.IsType<User>(createdAtActionResult.Value);
            Assert.Equal(newUser.Username, user.Username);
            Assert.True(_context.Users.Any(u => u.Username == newUser.Username));
        }

        [Fact]
        public async Task PutUser_UpdatesUser_WhenUserExists()
        {
            var existingUser = new User { Id = 1, Username = "Charlie" };
            _context.Users.Add(existingUser);
            _context.SaveChanges();
            var updatedUser = new User { Id = 1, Username = "Charlie Updated" };

            var result = await _controller.PutUser(1, updatedUser);

            Assert.IsType<NoContentResult>(result);
            var user = _context.Users.Find(1);
            Assert.Equal(updatedUser.Username, user.Username);
        }

        [Fact]
        public async Task DeleteUser_RemovesUser_WhenUserExists()
        {
            var user = new User { Id = 1, Username = "David" };
            _context.Users.Add(user);
            _context.SaveChanges();

            var result = await _controller.DeleteUser(1);

            Assert.IsType<NoContentResult>(result);
            Assert.False(_context.Users.Any(u => u.Id == 1));
        }
    }
}
