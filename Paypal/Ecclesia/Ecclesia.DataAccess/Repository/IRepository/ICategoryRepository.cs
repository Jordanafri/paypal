using Ecclesia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecclesia.DataAccess.Repository.IRepository
{
    public interface ICategoryRepository : IRepository<Category>
    {
        
        void Update(Category obj);
        

    }
}
