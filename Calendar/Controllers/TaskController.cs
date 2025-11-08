using Calendar.Data;
using Calendar.Models;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.Controllers;

public class TaskController : Controller
{
    public TaskController(DB_Service db)
    {
        _db = db;
    }
    #region GET Methods
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Details()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Edit()
    {
        return View();
    }

    #endregion
    #region POST Methods
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
        await CalculateSchedule(nuevaTarea);
        await _db.UpdateContainerTasks(1, nuevaTarea);
        return View();
    }

    #endregion
    #region Private Logic
    private readonly DB_Service _db;
    private static double CalculatePriority(DateOnly date, int hours)
    {

        double daysleft = (date.ToDateTime(new TimeOnly()) - DateTime.Now).TotalDays;
        double priority = hours / (daysleft + 1);
        return priority;
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
            List<TaskItem> allTasks = [.. tasksWorker, task];
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
    
    #endregion
}
