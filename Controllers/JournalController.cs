using Microsoft.AspNetCore.Mvc;
using StudentSuccessPredictor.Models;
using StudentSuccessPredictor.Services;

namespace StudentSuccessPredictor.Controllers;

public class JournalController : Controller
{
    private readonly StudentJournalStore _store;

    public JournalController(StudentJournalStore store)
    {
        _store = store;
    }

    public IActionResult Index()
    {
        var students = _store.GetAll().OrderBy(s => s.FullName).ToList();
        return View(students);
    }

    [HttpGet]
    public IActionResult Create() => View(new JournalStudent());

    [HttpPost]
    public IActionResult Create(JournalStudent model)
    {
        if (!ModelState.IsValid) return View(model);

        _store.Add(model);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(string id)
    {
        var s = _store.GetById(id);
        if (s == null) return NotFound();
        return View(s);
    }

    [HttpPost]
    public IActionResult Edit(JournalStudent model)
    {
        if (!ModelState.IsValid) return View(model);

        _store.Update(model);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Delete(string id)
    {
        _store.Delete(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult ResetPredictions()
    {
        _store.ResetPredictions();
        return RedirectToAction(nameof(Index));
    }


}
