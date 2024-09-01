using Ecclesia.DataAccess.Data;
using Ecclesia.DataAccess.Repository.IRepository;
using Ecclesia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecclesia.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Product obj)
        {
            var objfromDb = _db.Products.FirstOrDefault(u => u.Id == obj.Id);
            if (objfromDb != null)
            {
                objfromDb.Title = obj.Title;
                objfromDb.ISBN = obj.ISBN;
                objfromDb.ListPrice = obj.ListPrice;
                objfromDb.Description = obj.Description;
                objfromDb.CategoryId = obj.CategoryId;
                if (obj.ImageUrl != null)
                {
                    objfromDb.ImageUrl = obj.ImageUrl;
                }
            }
        }
    }
}
