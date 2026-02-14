using Lihtar.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lihtar.Application.Interfaces
{
    public interface IEventService
    {
        Task<List<EventDto>> GetAllAsync();
        Task<EventDto?> GetByIdAsync(Guid id);
        Task CreateAsync(EventDto dto);
        Task UpdateAsync(EventDto dto);
        Task DeleteAsync(Guid id);
    }
}
