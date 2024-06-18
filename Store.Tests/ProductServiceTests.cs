using Microsoft.EntityFrameworkCore;
using StoreAPI.Contracts;
using StoreAPI.Data;
using StoreAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Store.Tests
{
    public class ProductServiceTests : IDisposable
    {
        private readonly StoreContext _context;
        private readonly ProductService _service;

        public ProductServiceTests()
        {
            var options = new DbContextOptionsBuilder<StoreContext>()
                .UseInMemoryDatabase(databaseName: "TestProductDb")
                .Options;
            _context = new StoreContext(options);
            _service = new ProductService(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public void GetAllProducts_ReturnsAllProducts()
        {
            // Arrange
            _context.Products.Add(new Product { Id = 1, Name = "Product 1", Price = 100 });
            _context.Products.Add(new Product { Id = 2, Name = "Product 2", Price = 200 });
            _context.SaveChanges();

            // Act
            var result = _service.GetAllProducts();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, p => p.Name == "Product 1" && p.Price == 100);
        }

        [Fact]
        public void GetProduct_ReturnsProduct_WhenProductExists()
        {
            // Arrange
            _context.Products.Add(new Product { Id = 1, Name = "Product 1", Price = 100 });
            _context.SaveChanges();

            // Act
            var result = _service.GetProduct(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Product 1", result.Name);
        }

        [Fact]
        public void AddProduct_AddsProduct()
        {
            // Arrange
            var newProduct = new Product { Id = 1, Name = "New Product", Price = 100 };

            // Act
            var result = _service.AddProduct(newProduct);

            // Assert
            Assert.Equal("Product New Product added successfully", result);
            Assert.Equal(1, _context.Products.Count());
            Assert.Equal("New Product", _context.Products.Single().Name);
        }

        [Fact]
        public void UpdateProduct_UpdatesExistingProduct()
        {
            // Arrange
            _context.Products.Add(new Product { Id = 1, Name = "Old Product", Price = 100 });
            _context.SaveChanges();
            var updatedProduct = new Product { Id = 1, Name = "Updated Product", Price = 150 };

            // Act
            var result = _service.UpdateProduct(updatedProduct);

            // Assert
            Assert.Equal("Product Updated Product updated successfully", result);
            var product = _context.Products.Find(1);
            Assert.Equal("Updated Product", product.Name);
            Assert.Equal(150, product.Price);
        }

        [Fact]
        public void DeleteProduct_RemovesProduct()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Product to Delete", Price = 100 };
            _context.Products.Add(product);
            _context.SaveChanges();

            // Act
            var result = _service.DeleteProduct(1);

            // Assert
            Assert.Equal("Product deleted successfully", result);
            Assert.Empty(_context.Products);
        }
    }
}
