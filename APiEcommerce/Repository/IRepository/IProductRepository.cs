using System;
using APiEcommerce.Models;

namespace APiEcommerce.Repository.IRepository;

public interface IProductRepository
{
    ICollection<Product> GetProducts();
    ICollection<Product> GetProductsInPages(int pagNumber, int pageSize);
    int GetTotalProducts();
    ICollection<Product> GetProductsForCategory(int categoryId);
    ICollection<Product> SearchProducts(string name);

    Product? GetProduct(int id);
    bool BuyProduct(string name, int stock);
    bool ProductExists(int id);
    bool ProductExists(string name);
    bool CreateProduct(Product product);
    bool UpdateProduct(Product product);
    bool DeleteProduct(Product product);
    bool Save();
}
