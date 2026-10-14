using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Services;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetCategoriesByEventAsync(int eventId);
    Task<Category> CreateCategoryAsync(int eventId, int organiserId, CategoryDto dto);
    Task<Category?> UpdateCategoryAsync(int id, int organiserId, CategoryDto dto);
    Task<bool> DeleteCategoryAsync(int id, int organiserId);
}