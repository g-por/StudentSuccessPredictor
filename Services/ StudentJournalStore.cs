using System.Text.Json;
using StudentSuccessPredictor.Models;

namespace StudentSuccessPredictor.Services;

public class StudentJournalStore
{
    private readonly string _path;
    private static readonly object _lock = new();

    public StudentJournalStore(IWebHostEnvironment env)
    {
        _path = Path.Combine(env.ContentRootPath, "App_Data", "journal.json");
        Console.WriteLine("JOURNAL PATH = " + _path);
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        if (!File.Exists(_path)) File.WriteAllText(_path, "[]");
    }

    public List<JournalStudent> GetAll()
    {
        lock (_lock)
        {
            var json = File.ReadAllText(_path);

            var list = JsonSerializer.Deserialize<List<JournalStudent>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // ← КЛЮЧОВИЙ РЯДОК
                }
            ) ?? new();

            return list;
        }
    }

    public JournalStudent? GetById(string id)
        => GetAll().FirstOrDefault(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    public void ResetPredictions()
    {
        var list = GetAll();
        foreach (var s in list) s.PredictedG3 = null;
        SaveAll(list);
    }

    public void Add(JournalStudent student)
    {
        lock (_lock)
        {
            var all = GetAll();
            student.LastUpdatedUtc = DateTimeOffset.UtcNow;
            all.Add(student);
            SaveAll(all);
        }
    }

    public void Update(JournalStudent student)
    {
        lock (_lock)
        {
            var all = GetAll();
            var idx = all.FindIndex(s => s.Id.Equals(student.Id, StringComparison.OrdinalIgnoreCase));
            if (idx < 0) return;

            student.LastUpdatedUtc = DateTimeOffset.UtcNow;
            all[idx] = student;
            SaveAll(all);
        }
    }

    public void Delete(string id)
    {
        lock (_lock)
        {
            var all = GetAll();
            all.RemoveAll(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            SaveAll(all);
        }
    }

    private void SaveAll(List<JournalStudent> all)
    {
        var json = JsonSerializer.Serialize(all, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_path, json);
    }
}
