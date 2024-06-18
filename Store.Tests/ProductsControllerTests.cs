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
    public class ProductsControllerTests : IDisposable
    {
        private readonly StoreContext _context;
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            var options = new DbContextOptionsBuilder<StoreContext>()
                .UseInMemoryDatabase(databaseName: "TestProductDb")
                .Options;
            _context = new StoreContext(options);
            _controller = new ProductsController(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetProducts_ReturnsAllProducts()
        {
            _context.Products.AddRange(
                new Product { Id = 1, Name = "Product 1", Price = 100 },
                new Product { Id = 2, Name = "Product 2", Price = 150 }
            );
            _context.SaveChanges();

            var result = await _controller.GetProducts();

            var actionResult = Assert.IsType<ActionResult<IEnumerable<Product>>>(result);
            var products = Assert.IsType<List<Product>>(actionResult.Value);
            Assert.Equal(2, products.Count);
        }

        [Fact]
        public async Task GetProduct_ReturnsProduct_WhenProductExists()
        {
            var expectedProduct = new Product { Id = 1, Name = "New Product", Price = 200 };
            _context.Products.Add(expectedProduct);
            _context.SaveChanges();

            var result = await _controller.GetProduct(1);

            var actionResult = Assert.IsType<ActionResult<Product>>(result);
            var product = Assert.IsType<Product>(actionResult.Value);
            Assert.Equal(expectedProduct.Id, product.Id);
        }

        [Fact]
        public async Task PostProduct_AddsProduct_WhenCalled()
        {
            var newProduct = new Product { Name = "New Product", Price = 100 };

            var result = await _controller.PostProduct(newProduct);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var product = Assert.IsType<Product>(createdAtActionResult.Value);
            Assert.Equal(newProduct.Name, product.Name);
            Assert.True(_context.Products.Any(p => p.Name == newProduct.Name));
        }

        [Fact]
        public async Task PutProduct_UpdatesProduct_WhenProductExists()
        {
            var existingProduct = new Product { Id = 1, Name = "Old Product", Price = 100 };
            _context.Products.Add(existingProduct);
            _context.SaveChanges();

            var updatedProduct = new Product { Id = 1, Name = "Updated Product", Price = 150 };

            var result = await _controller.PutProduct(1, updatedProduct);

            Assert.IsType<NoContentResult>(result);
            var product = _context.Products.Find(1);
            Assert.Equal(updatedProduct.Name, product.Name);
            Assert.Equal(updatedProduct.Price, product.Price);
        }

        [Fact]
        public async Task DeleteProduct_RemovesProduct_WhenProductExists()
        {
            var product = new Product { Id = 1, Name = "Product to Delete", Price = 200 };
            _context.Products.Add(product);
            _context.SaveChanges();

            var result = await _controller.DeleteProduct(1);

            Assert.IsType<NoContentResult>(result);
            Assert.False(_context.Products.Any(p => p.Id == 1));
        }
    }

}
