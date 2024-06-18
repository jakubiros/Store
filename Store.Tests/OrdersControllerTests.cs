using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StoreAPI.Controllers;
using StoreAPI.Data;
using StoreAPI.Models;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Store.Tests
{
    public class TestStoreContext : StoreContext
    {
        public TestStoreContext(DbContextOptions<StoreContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("TestDatabase");
            base.OnConfiguring(optionsBuilder);
        }
    }

    public class OrdersControllerTests : IDisposable
    {
        private readonly TestStoreContext _context;
        private readonly OrdersController _controller;

        public OrdersControllerTests()
        {
            var options = new DbContextOptionsBuilder<StoreContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _context = new TestStoreContext(options);
            _controller = new OrdersController(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetOrders_ReturnsAllOrders()
        {
            _context.Orders.Add(new Order { Id = 1, OrderDate = DateTime.Now, UserId = 1 });
            _context.Orders.Add(new Order { Id = 2, OrderDate = DateTime.Now, UserId = 2 });
            _context.SaveChanges();

            var result = await _controller.GetOrders();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var orders = Assert.IsType<List<Order>>(okResult.Value);
            Assert.Equal(2, orders.Count);
        }

        [Fact]
        public async Task GetOrder_ReturnsOrder_WhenOrderExists()
        {
            var order = new Order { Id = 1, OrderDate = DateTime.Now, UserId = 1 };
            _context.Orders.Add(order);
            _context.SaveChanges();

            var result = await _controller.GetOrder(1);

            var actionResult = Assert.IsType<ActionResult<Order>>(result);
            var returnedOrder = Assert.IsType<Order>(actionResult.Value);
            Assert.Equal(1, returnedOrder.Id);
        }

        [Fact]
        public async Task PostOrder_AddsOrder_WhenCalled()
        {
            var newOrder = new Order { Id = 3, OrderDate = DateTime.Now, UserId = 3 };

            var result = await _controller.PostOrder(newOrder);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedOrder = Assert.IsType<Order>(createdAtActionResult.Value);
            Assert.Equal(3, returnedOrder.Id);
            Assert.Equal(1, _context.Orders.Count());
        }

        [Fact]
        public async Task PutOrder_UpdatesOrder_WhenOrderExists()
        {
            var existingOrder = new Order { Id = 1, OrderDate = DateTime.Now, UserId = 1 };
            _context.Orders.Add(existingOrder);
            _context.SaveChanges();

            var updatedOrder = new Order { Id = 1, OrderDate = DateTime.Now.AddDays(1), UserId = 1 };

            var result = await _controller.PutOrder(1, updatedOrder);

            Assert.IsType<NoContentResult>(result);
            Assert.Equal(DateTime.Now.AddDays(1).Date, _context.Orders.Find(1).OrderDate.Date);
        }

        [Fact]
        public async Task DeleteOrder_RemovesOrder_WhenOrderExists()
        {
            var order = new Order { Id = 1, OrderDate = DateTime.Now, UserId = 1 };
            _context.Orders.Add(order);
            _context.SaveChanges();

            var result = await _controller.DeleteOrder(1);

            Assert.IsType<NoContentResult>(result);
            Assert.Empty(_context.Orders);
        }
    }
}

