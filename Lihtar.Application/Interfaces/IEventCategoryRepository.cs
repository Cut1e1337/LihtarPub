using Lihtar.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lihtar.Application.Interfaces
{
    public interface IEventCategoryRepository
    {
        Task<List<EventCategory>> GetAllAsync();
        Task<EventCategory?> GetByIdAsync(Guid id);
        Task AddAsync(EventCategory entity);
        void Remove(EventCategory entity);
        Task SaveChangesAsync();
    }
}
