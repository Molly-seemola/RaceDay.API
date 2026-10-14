using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Services;

public interface IResultService
{
    Task<IEnumerable<object>> GetResultsAsync(int? eventId, int? participantId);
    Task<Result> CreateResultAsync(int organiserUserId, ResultDto dto);
    Task<Result?> UpdateResultAsync(int resultId, int organiserUserId, ResultDto dto);
}