using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Calendar.Models;
using Calendar.Data;

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

    public async Task<IActionResult> GetTasks()
    {
        var worker = await _db.GetAllTasks(1);
        var schedule = worker?.Schedules?.OrderBy(s => s.StartTime).ToList();
        var tasks = worker?.ContainerTasks?.OrderByDescending(t => t.Priority);

        if (schedule == null || tasks == null) return BadRequest("No se encontró el horario o las tareas.");

        int dayOffset = 0; // días desde hoy
        var currentStart = DateTime.Today.AddDays(dayOffset).Add(schedule[0].StartTime.ToTimeSpan());
        var currentEnd = DateTime.Today.AddDays(dayOffset).Add(schedule[0].EndTime.ToTimeSpan());
        int scheduleIndex = 0;

        List<TaskItem> result = new();

        foreach (var task in tasks)
        {
            double hoursLeft = task.Hours;
            DateTime start = currentStart;

                while (hoursLeft > 0)
                {
                    // Si estamos fuera del horario actual, pasamos al siguiente
                    if (start >= currentEnd)
                    {
                        scheduleIndex++;

                        if (scheduleIndex >= schedule.Count)
                        {
                            // Pasamos al siguiente día
                            scheduleIndex = 0;
                            dayOffset++;
                        }

                        start = DateTime.Today.AddDays(dayOffset).Add(schedule[scheduleIndex].StartTime.ToTimeSpan());
                        currentEnd = DateTime.Today.AddDays(dayOffset).Add(schedule[scheduleIndex].EndTime.ToTimeSpan());
                    }

                    // Calculamos cuántas horas quedan en esta franja
                    var availableHours = (currentEnd - start).TotalHours;

                    // Si la tarea cabe en esta franja
                    if (hoursLeft <= availableHours)
                    {
                        result.Add(new TaskItem
                        {
                            Title = task.Title,
                            Priority = task.Priority,
                            Hours = (int)hoursLeft,
                            Start = start,
                            End = start.AddHours(hoursLeft),
                            Deadline = task.Deadline
                        });

                        start = start.AddHours(hoursLeft);
                        currentStart = start;
                        hoursLeft = 0;
                    }
                    else
                    {
                        // Dividimos la tarea en esta franja y seguimos en la siguiente
                        result.Add(new TaskItem
                        {
                            Title = task.Title,
                            Priority = task.Priority,
                            Hours = (int)availableHours,
                            Start = start,
                            End = currentEnd,
                            Deadline = task.Deadline
                        });

                        hoursLeft -= availableHours;
                        start = currentEnd; // Avanzamos a la siguiente franja o día
                    }
                }
        }

        return Json(result);
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
