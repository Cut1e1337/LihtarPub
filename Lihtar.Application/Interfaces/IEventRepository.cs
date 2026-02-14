using Lihtar.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lihtar.Application.Interfaces
{
    public interface IEventRepository
    {
        Task<List<Event>> GetAllAsync();
        Task<Event?> GetByIdAsync(Guid id);
        Task AddAsync(Event entity);
        void Remove(Event entity);
        Task SaveChangesAsync();
    }
}
