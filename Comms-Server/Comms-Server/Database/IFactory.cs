namespace Comms_Server.Database
{
	public interface IFactory
	{
		Task<T?> GetAsync<T>(Guid id) where T : class;
		Task<IEnumerable<T>> GetAllAsync<T>() where T : class;
		Task AddAsync<T>(T entity) where T : class;
		Task UpdateAsync<T>(T entity) where T : class;
		Task DeleteAsync<T>(T entity) where T : class;
		Task Save();
	}
}
