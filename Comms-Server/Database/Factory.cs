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

		public async Task<T?> GetAsync<T>(Guid id) where T : class
		{
			return await _context.Set<T>().FindAsync(id);
		}

		public async Task<IEnumerable<T>> GetAllAsync<T>() where T : class
		{
			return await _context.Set<T>().ToListAsync();
		}

		public async Task AddAsync<T>(T entity) where T : class
		{
			await _context.Set<T>().AddAsync(entity);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateAsync<T>(T entity) where T : class
		{
			_context.Set<T>().Update(entity);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteAsync<T>(T entity) where T : class
		{
			_context.Set<T>().Remove(entity);
			await _context.SaveChangesAsync();
		}
	}
}
