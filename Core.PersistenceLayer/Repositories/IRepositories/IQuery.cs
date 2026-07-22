namespace Core.PersistenceLayer.Repositories.IRepositories
{
    public interface IQuery<T>
    {
        IQueryable<T> Query();
    }
}
