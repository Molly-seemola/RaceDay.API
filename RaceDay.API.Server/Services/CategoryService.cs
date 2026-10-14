using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Services;

public class CategoryService : ICategoryService
{
    private readonly RaceDayDbContext _context;

    public CategoryService(RaceDayDbContext context)
    {
        _context = context;
    }

    // ✅ 1. Get all categories for an event
    public async Task<IEnumerable<Category>> GetCategoriesByEventAsync(int eventId)
    {
        return await _context.Categories
            .Where(c => c.EventId == eventId)
            .ToListAsync();
    }

    // ✅ 2. Create a new category
    public async Task<Category> CreateCategoryAsync(int eventId, int organiserId, CategoryDto dto)
    {
        var eventEntity = await _context.Events
            .Include(e => e.Organiser)
            .FirstOrDefaultAsync(e => e.EventId == eventId);

        if (eventEntity == null)
            throw new KeyNotFoundException("Event not found");

        if (eventEntity.Organiser.UserId != organiserId)
            throw new UnauthorizedAccessException("You can only add categories to your own events");

        var category = new Category
        {
            EventId = eventId,
            CategoryName = dto.CategoryName,
            Distance = dto.Distance,
            EntryFee = dto.EntryFee,
            CategoryStartTime = dto.CategoryStartTime,
            MaxParticipants = dto.MaxParticipants,
            CurrentCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    // ✅ 3. Update an existing category
    public async Task<Category?> UpdateCategoryAsync(int id, int organiserId, CategoryDto dto)
    {
        var category = await _context.Categories
            .Include(c => c.Event)
                .ThenInclude(e => e.Organiser)
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (category == null)
            return null;

        if (category.Event.Organiser.UserId != organiserId)
            return null;

        category.CategoryName = dto.CategoryName;
        category.Distance = dto.Distance;
        category.EntryFee = dto.EntryFee;
        category.CategoryStartTime = dto.CategoryStartTime;
        category.MaxParticipants = dto.MaxParticipants;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return category;
    }

    // ✅ 4. Delete a category (THIS WAS MISSING!)
    public async Task<bool> DeleteCategoryAsync(int id, int organiserId)
    {
        var category = await _context.Categories
            .Include(c => c.Event)
                .ThenInclude(e => e.Organiser)
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (category == null)
            return false;

        if (category.Event.Organiser.UserId != organiserId)
            return false;

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }
}