using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.PersistenceLayer.Repositories.IRepositories
{
    public interface IQuery<T>
    {
        IQueryable<T> Query();
    }
}
