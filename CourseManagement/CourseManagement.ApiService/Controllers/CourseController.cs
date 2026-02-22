using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using CourseManagement.ApiService.DTO;
using CourseManagement.ApiService.Services;

namespace CourseManagement.ApiService.Controllers;

/// <summary>
/// Контроллер для сущности типа Курс
/// </summary>
/// <param name="generator">Генератор курсов</param>
/// <param name="cache">Кэш</param>
/// <param name="logger">Логгер</param>
/// <param name="configuration">Конфигурация</param>
[ApiController]
[Route("course-management")]
public class CourseController(CourseGenerator generator, IDistributedCache cache, ILogger<CourseController> logger, IConfiguration configuration) : ControllerBase
{
    /// <summary>
    /// GET-запрос на генерацию курса
    /// </summary>
    /// <param name="id">Идентификатор курса</param>
    /// <returns>Сгенерированный курс</returns>
    [HttpGet]
    public async Task<ActionResult<CourseDto>> GetCourse(int? id)
    {
        using (logger.BeginScope(new
        {
            RequestId = Guid.NewGuid(),
            ResourceType = "Course",
            ResourceId = id,
            Operation = "GetCourse"
        }))
        {
            try
            {
                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("Processing request for course {ResourceId}", id);

                var cacheKey = $"course:{id ?? 0}";

                try
                {
                    var cachedCourse = await cache.GetStringAsync(cacheKey);
                    if (cachedCourse != null)
                    {
                        var course = JsonSerializer.Deserialize<CourseDto>(cachedCourse);

                        if (logger.IsEnabled(LogLevel.Information))
                            logger.LogInformation("Cache hit for course {ResourceId}. Course data: {@Course}", id, course);

                        return Ok(course);
                    }

                    if (logger.IsEnabled(LogLevel.Information))
                        logger.LogInformation("Cache miss for course {ResourceId}, generating new", id);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Сache is unavailable");
                }

                var newCourse = generator.GenerateOne(id);

                var cacheDuration = configuration.GetValue<double?>("Cache:DurationMinutes") ?? 5;
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheDuration)
                };

                try
                {
                    var serializedCourse = JsonSerializer.Serialize(newCourse);
                    await cache.SetStringAsync(cacheKey, serializedCourse, options);
                    if (logger.IsEnabled(LogLevel.Information))
                        logger.LogInformation("Course {ResourceId} generated and cached. Course details: {@Course}", id, newCourse);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Сache is unavailable");
                }

                return Ok(newCourse);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing course {ResourceId}", id);
                return Problem("Internal server error", statusCode: 500);
            }
        }
    }
}