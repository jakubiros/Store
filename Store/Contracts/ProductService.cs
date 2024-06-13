using StoreAPI.Data;
using StoreAPI.Models;

namespace StoreAPI.Contracts
{
    public class ProductService : IProductService
    {
        private readonly StoreContext _context;

        public ProductService(StoreContext context)
        {
            _context = context;
        }

        public Product GetProduct(int id)
        {
            return _context.Products.Find(id);
        }

        public List<Product> GetAllProducts()
        {
            return _context.Products.ToList();
        }

        public string AddProduct(Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
            return $"Product {product.Name} added successfully";
        }

        public string UpdateProduct(Product product)
        {
            var existingProduct = _context.Products.Find(product.Id);
            if (existingProduct == null)
            {
                return "Product not found";
            }

            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            _context.SaveChanges();

            return $"Product {product.Name} updated successfully";
        }

        public string DeleteProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return "Product not found";
            }

            _context.Products.Remove(product);
            _context.SaveChanges();
            return "Product deleted successfully";
        }
    }
}
