using Bogus;
using CourseManagement.ApiService.Models;

namespace CourseManagement.ApiService.Services;

public class CourseGenerator
{
    private readonly Faker<Course> _courseFaker;

    public CourseGenerator()
    {
        _courseFaker = new Faker<Course>("ru")
            .RuleFor(c => c.Title, f => f.Commerce.Department())
            .RuleFor(c => c.Description, f => f.Lorem.Paragraph())
            .RuleFor(c => c.LecturesCount, f => f.Random.Int(5, 30))
            .RuleFor(c => c.PracticesCount, f => f.Random.Int(5, 30))
            .RuleFor(c => c.LaboratoriesCount, f => f.Random.Int(0, 15))
            .RuleFor(c => c.TotalHours, (f, c) => c.LecturesCount * 1.5 + c.PracticesCount * 1.5 + c.LaboratoriesCount * 3.0)
            .RuleFor(c => c.Lector, f => f.Name.FullName())
            .RuleFor(c => c.Department, f => f.Commerce.Department())
            .RuleFor(c => c.StartDate, f => f.Date.Future(1))
            .RuleFor(c => c.EndDate, (f, c) => c.StartDate.AddMonths(f.Random.Int(1, 6)))
            .RuleFor(c => c.MaxStudents, f => f.Random.Int(10, 100))
            .RuleFor(c => c.EnrolledStudents, (f, c) => f.Random.Int(0, c.MaxStudents))
            .RuleFor(c => c.Status, f => f.PickRandom("Планируется", "Идёт набор", "В процессе", "Завершён", "Отменён"))
            .RuleFor(c => c.Level, f => f.PickRandom("Начальный", "Средний", "Продвинутый"))
            .RuleFor(c => c.Price, f => f.Finance.Amount(5000, 100000))
            .RuleFor(c => c.Format, f => f.PickRandom("Онлайн", "Офлайн", "Смешанный"));
    }

    public Course GenerateOne(int? id = null)
    {
        var course = _courseFaker.Generate();
        course.Id = id ?? new Randomizer().Int(1, 10000);
        return course;
    }
}
