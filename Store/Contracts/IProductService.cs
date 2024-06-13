using StoreAPI.Models;
using System.Collections.Generic;
using System.ServiceModel;

namespace StoreAPI.Contracts
{

    [ServiceContract]
    public interface IProductService
    {
        [OperationContract]
        Product GetProduct(int id);

        [OperationContract]
        List<Product> GetAllProducts();

        [OperationContract]
        string AddProduct(Product product);

        [OperationContract]
        string UpdateProduct(Product product);

        [OperationContract]
        string DeleteProduct(int id);
    }
}
