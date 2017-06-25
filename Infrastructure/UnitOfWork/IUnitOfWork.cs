using System.Data.Entity;
using Infrastructure.Repository;

namespace Infrastructure.UnitOfWork
{
    public interface IUnitOfWork
    {
        IRepository<T> RegisterRepository<T>() where T : class, IEntity;
        void Save();
        DbContext DbContext { get; set; }
    }
}
