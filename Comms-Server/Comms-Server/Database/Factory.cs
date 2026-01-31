using System.Linq.Expressions;
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
		/// <returns>All entities of type <typeparamref name="T"/>.</returns>
		public async Task<IEnumerable<T>> GetAllAsync<T>() where T : class
		{
			return await _context.Set<T>().ToListAsync();
		}

		/// <summary>
		/// Determines whether any entity of type <typeparamref name="T"/> exists in the database
		/// that satisfies the specified predicate.
		/// </summary>
		/// <typeparam name="T">Entity type.</typeparam>
		public async Task<bool> ExistsAsync<T>(Expression<Func<T, bool>> predicate) where T : class
		{
			return await _context.Set<T>().AnyAsync(predicate);
		}

		/// <summary>
		/// Add a new entity and track it (does NOT call SaveChanges).
		/// </summary>
		/// <typeparam name="T">Entity type.</typeparam>
		/// <param name="entity">Entity to add.</param>
		public async Task AddAsync<T>(T entity) where T : class
		{
			await _context.Set<T>().AddAsync(entity);
		}

		/// <summary>
		/// Create a new instance of <typeparamref name="T"/> using a public parameterless constructor,
		/// attach it to the change tracker and return it (does NOT call SaveChanges).
		/// </summary>
		/// <typeparam name="T">Entity type.</typeparam>
		/// <returns>Tracked entity.</returns>
		public T New<T>() where T : class, new()
		{
			var entity = new T();
			_context.Set<T>().Add(entity);
			return entity;
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
		public async Task SaveAsync()
		{
			await _context.SaveChangesAsync();
		}
	}
}
