using System;
using APiEcommerce.Models;
using APiEcommerce.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace APiEcommerce.Repository;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _db;
    public ProductRepository(ApplicationDbContext db)
    {
        _db=db;
    }
    public bool BuyProduct(string name, int stock)
    {
        if (string.IsNullOrWhiteSpace(name) || stock <= 0) return false;

        var product = _db.Products.FirstOrDefault(p => p.Name.ToLower().Trim() == name.ToLower().Trim());
        if (product == null || product.Stock < stock) return false;
        product.Stock -= stock;
        _db.Products.Update(product);
        return Save();
    }

    public bool CreateProduct(Product product)
    {
        if (product == null) return false;
        product.CreationDate = DateTime.Now;
        product.UpdateDate = DateTime.Now;
        _db.Products.Add(product);
        return Save();
    }

    public bool DeleteProduct(Product product)
    {
        if (product == null) return false;
        _db.Products.Remove(product);
        return Save();

    }

    public Product? GetProduct(int id)
    {
        if (id <= 0) return null;

        return _db.Products
        .Include(p => p.Category)
        .FirstOrDefault(p => p.ProductId == id);
        
    }

    public ICollection<Product> GetProducts()
    {
      return  _db.Products
      .Include(p => p.Category)
      .OrderBy(p => p.Name)
      .ToList();
    }

    public ICollection<Product> GetProductsForCategory(int categoryId)
    {
        if (categoryId <= 0) return new List<Product>();
        return _db.Products
        .Include(p => p.Category)
        .Where(p => p.CategoryId == categoryId)
        .OrderBy(p => p.Name)
        .ToList();
    }

    public bool ProductExists(int id)
    {
        if (id <= 0) return false;
        return _db.Products.Any(p => p.ProductId == id);
    }

    public bool ProductExists(string name)
    {
             if (string.IsNullOrWhiteSpace(name)) return false;
        return _db.Products
        .Any(p => p.Name
        .ToLower().Trim() == name.ToLower().Trim());
    }

    public bool Save()
    {
        return _db.SaveChanges() >= 0 ;
    }

    public ICollection<Product> SearchProducts(string name)
    {
        IQueryable<Product> query = _db.Products;
        name = name.Trim().ToLower();

        var results = _db.Products
        .Include(p=> p.Category)
        .Where(p =>
            p.Name.ToLower().Trim().Contains(name)||
            p.Descripton.ToLower().Trim().Contains(name))
        .OrderBy(p => p.Name)
        .ToList();

    return results;
    }

    public bool UpdateProduct(Product product)
    {
        if (product == null) return false;
        
        product.UpdateDate = DateTime.Now;
        _db.Products.Update(product);
        return Save();
    }
}
