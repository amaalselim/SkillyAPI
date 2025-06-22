using Microsoft.EntityFrameworkCore;
using Skilly.Core.Entities;
using Skilly.Persistence.Abstract;
using Skilly.Persistence.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Persistence.Implementation
{
    public class UserRepository : IGenericRepository<User>
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(User user)
        {
            await _context.users.AddAsync(user);    
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var user= await _context.users.FindAsync(id);   
            _context.users.Remove(user);
            await _context.SaveChangesAsync();
        }



        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.users.ToListAsync();
        }

        public async Task<User> GetByIdAsync(string id)
        {
            return await _context.users.FindAsync(id);
        }

        public async Task UpdateAsync(User user)
        {
            _context.users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
