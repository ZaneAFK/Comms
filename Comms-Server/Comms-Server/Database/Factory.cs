using Comms_Server.Database.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Comms_Server.Database
{
	public class Factory : IFactory
	{
		private readonly CommsDbContext _context;

		public Factory(CommsDbContext context)
		{
			_context = context;
		}

		/// <summary>
		/// Retrieve an entity by its primary key.
		/// </summary>
		/// <typeparam name="T">Entity type.</typeparam>
		/// <param name="id">Entity identifier.</param>
		/// <returns>The entity if found; otherwise null.</returns>
		public async Task<T?> GetAsync<T>(Guid id) where T : class
		{
			return await _context.Set<T>().FindAsync(id);
		}

		/// <summary>
		/// Retrieve all entities of the given type.
		/// </summary>
		/// <typeparam name="T">Entity type.</typeparam>
		/// <returns>All entities of type T.</returns>
		public async Task<IEnumerable<T>> GetAllAsync<T>() where T : class
		{
			return await _context.Set<T>().ToListAsync();
		}

		/// <summary>
		/// Add a new entity and persist immediately.
		/// </summary>
		/// <typeparam name="T">Entity type.</typeparam>
		/// <param name="entity">Entity to add.</param>
		public async Task AddAsync<T>(T entity) where T : class
		{
			await _context.Set<T>().AddAsync(entity);
			await _context.SaveChangesAsync();
		}

		/// <summary>
		/// Mark an entity as updated and persist immediately.
		/// </summary>
		/// <typeparam name="T">Entity type.</typeparam>
		/// <param name="entity">Entity to update.</param>
		public async Task UpdateAsync<T>(T entity) where T : class
		{
			_context.Set<T>().Update(entity);
			await _context.SaveChangesAsync();
		}

		/// <summary>
		/// Remove an entity and persist immediately.
		/// </summary>
		/// <typeparam name="T">Entity type.</typeparam>
		/// <param name="entity">Entity to remove.</param>
		public async Task DeleteAsync<T>(T entity) where T : class
		{
			_context.Set<T>().Remove(entity);
			await _context.SaveChangesAsync();
		}

		/// <summary>
		/// Persist any pending changes tracked by the DbContext.
		/// </summary>
		public async Task Save()
		{
			await _context.SaveChangesAsync();
		}
	}
}
