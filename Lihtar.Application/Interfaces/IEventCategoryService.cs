using Lihtar.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lihtar.Application.Interfaces
{
    public interface IEventCategoryService
    {
        Task<List<EventCategoryDto>> GetAllAsync();
        Task<EventCategoryDto?> GetByIdAsync(Guid id);
        Task CreateAsync(EventCategoryDto dto);
        Task UpdateAsync(EventCategoryDto dto);
        Task DeleteAsync(Guid id);
    }
}
