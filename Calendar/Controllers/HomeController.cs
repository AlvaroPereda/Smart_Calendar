using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Calendar.Models;
using Calendar.Data;
using System.Text.Json;
using System.Threading.Tasks;

namespace Calendar.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly DB_Service _db;

    public HomeController(ILogger<HomeController> logger, DB_Service db)
    {
        _logger = logger;
        _db = db;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
    public IActionResult Privacy()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Index(string task, DateOnly date, int hours)
    {
        TaskItem nuevaTarea = new()
        {
            Title = task,
            Deadline = date,
            Hours = hours,
            Priority = CalculatePriority(date, hours)
        };
        nuevaTarea = await CalculateSchedule(nuevaTarea);
        await _db.UpdateContainerTasks(1, nuevaTarea);
        return View();
    }

    private async Task<TaskItem> CalculateSchedule(TaskItem task)
    {
        Worker worker = await GetWorker();
        List<Schedule> horarios = worker.Schedules;
        List<TaskItem> tasksWorker = worker.ContainerTasks;
        if (tasksWorker.Count == 0)
        {
            var result = horarios[0].StartTime.AddHours(task.Hours);
            task.Start = DateTime.Today.Add(horarios[0].StartTime.ToTimeSpan());
            task.End = DateTime.Today.Add(result.ToTimeSpan());
            return task;
        }
        else
        {
            List<TaskItem> allTasks = [..tasksWorker, task];
            allTasks.Sort((a, b) => b.Priority.CompareTo(a.Priority));

            DateTime startTime = DateTime.Today.Add(horarios[0].StartTime.ToTimeSpan());

            foreach (var t in allTasks)
            {
                t.Start = startTime;
                t.End = startTime.AddHours(t.Hours);
                startTime = t.End;
            }
        }

        return task;
    }
    private static double CalculatePriority(DateOnly date, int hours)
    {

        double daysleft = (date.ToDateTime(new TimeOnly()) - DateTime.Now).TotalDays;
        double priority = hours / (daysleft + 1);
        return priority;
    }

    public async Task<IActionResult> GetTasks()
    {
        var tasks = await _db.GetAllTasks();
        return Json(tasks);
    }

    public async Task<IActionResult> CreateWorker()
    {
        Worker worker = new()
        {
            Name = "Juan Perez",
            Schedules =
            [
                new() { StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(13, 0) },
                new() { StartTime = new TimeOnly(15, 0), EndTime = new TimeOnly(17, 0) }
            ]
        };
        await _db.AddWorker(worker);
        return Ok();
    }

    public async Task<Worker> GetWorker()
    {
        Worker worker = new()
        {
            Name = "Default Worker",
            Schedules =
            [
                new() { StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(13, 0) },
                new() { StartTime = new TimeOnly(15, 0), EndTime = new TimeOnly(17, 0) }
            ]
        };
        return await _db.GetWorkerById(1) ?? worker;
    }
    public async Task<List<Worker>> GetWorkers()
    {
        List<Worker> workers = await _db.GetWorkers();
        return workers;
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
