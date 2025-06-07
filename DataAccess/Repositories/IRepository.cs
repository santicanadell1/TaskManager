namespace DataAccess;

public interface IRepository<T>
{
    public List<T> GetAll();
    public T Get(Func<T, bool> filter);
    public void Add(T entity);
    public void Update(T entity);
    public void Delete(T entity);
}