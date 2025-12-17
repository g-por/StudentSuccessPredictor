using Microsoft.AspNetCore.Mvc;
using StudentSuccessPredictor.Models;
using StudentSuccessPredictor.Services;

namespace StudentSuccessPredictor.Controllers;

public class PredictController : Controller
{
    private readonly StudentJournalStore _store;
    private readonly MlStudentModelService _ml;

    public PredictController(StudentJournalStore store, MlStudentModelService ml)
    {
        _store = store;
        _ml = ml;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var students = _store.GetAll().OrderBy(s => s.FullName).ToList();
        return View(students);
    }

    [HttpPost]
    public IActionResult PredictOne(PredictRequest req)
    {
        var s = _store.GetById(req.StudentId);
        if (s == null) return NotFound();

        var g3 = _ml.PredictG3(s);
        s.PredictedG3 = g3;
        s.LastUpdatedUtc = DateTimeOffset.UtcNow;
        _store.Update(s);

        var result = new PredictResult
        {
            StudentId = s.Id,
            FullName = s.FullName,
            PredictedG3 = g3,
            Interpretation = MlStudentModelService.Interpret(g3)
        };

        return View("Result", result);
    }

    [HttpPost]
    public IActionResult PredictAll()
    {
        var all = _store.GetAll();
        foreach (var s in all)
        {
            var g3 = _ml.PredictG3(s);
            s.PredictedG3 = g3;
            s.LastUpdatedUtc = DateTimeOffset.UtcNow;
            _store.Update(s);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Retrain()
    {
        _ml.TrainAndSave();
        return RedirectToAction(nameof(Index));
    }
}
