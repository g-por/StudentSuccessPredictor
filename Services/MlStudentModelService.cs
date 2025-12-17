using Microsoft.ML;
using StudentSuccessPredictor.Models;
using StudentSuccessPredictor.Models.Ml;

namespace StudentSuccessPredictor.Services;

public class MlStudentModelService
{
    private readonly IWebHostEnvironment _env;
    private readonly MLContext _ml;
    private readonly string _modelPath;

    private ITransformer? _model;
    private PredictionEngine<StudentInputRow, StudentPrediction>? _engine;
    private readonly object _lock = new();

    public MlStudentModelService(IWebHostEnvironment env)
    {
        _env = env;
        _ml = new MLContext(seed: 42);
        _modelPath = Path.Combine(_env.ContentRootPath, "App_Data", "model.zip");
    }

    public void EnsureModelReady()
    {
        lock (_lock)
        {
            if (_model != null && _engine != null) return;

            if (File.Exists(_modelPath))
            {
                using var fs = File.OpenRead(_modelPath);
                _model = _ml.Model.Load(fs, out _);
                _engine = _ml.Model.CreatePredictionEngine<StudentInputRow, StudentPrediction>(_model);
                return;
            }

            TrainAndSave();
        }
    }

    public void TrainAndSave()
    {
        lock (_lock)
        {
            var dataDir = Path.Combine(_env.ContentRootPath, "App_Data");
            var mat = Path.Combine(dataDir, "uci_student_mat.csv");
            var por = Path.Combine(dataDir, "uci_student_por.csv");
            var custom = Path.Combine(dataDir, "custom_training_rows.csv");

            var rows = new List<StudentInputRow>();
            rows.AddRange(CsvUciLoader.LoadUciFile(mat));
            rows.AddRange(CsvUciLoader.LoadUciFile(por));
            rows.AddRange(CsvUciLoader.LoadCustomRows(custom));

            if (rows.Count < 50)
                throw new InvalidOperationException("Недостатньо даних для навчання. Перевірте, що UCI CSV файли лежать в App_Data.");

            var trainData = _ml.Data.LoadFromEnumerable(rows);

            var pipeline =
                _ml.Transforms.Categorical.OneHotEncoding("SexEncoded", nameof(StudentInputRow.Sex))
                  .Append(_ml.Transforms.Categorical.OneHotEncoding("MJobEncoded", nameof(StudentInputRow.MotherJob)))
                  .Append(_ml.Transforms.Categorical.OneHotEncoding("FJobEncoded", nameof(StudentInputRow.FatherJob)))
                  .Append(_ml.Transforms.Concatenate("Features",
                        nameof(StudentInputRow.Age),
                        nameof(StudentInputRow.PriorScore),
                        nameof(StudentInputRow.Absences),
                        nameof(StudentInputRow.Failures),
                        nameof(StudentInputRow.Higher),
                        nameof(StudentInputRow.Tutoring),
                        "SexEncoded",
                        "MJobEncoded",
                        "FJobEncoded"))
                  .Append(_ml.Regression.Trainers.FastTree(labelColumnName: nameof(StudentInputRow.G3), featureColumnName: "Features"));

            var model = pipeline.Fit(trainData);

            Directory.CreateDirectory(Path.GetDirectoryName(_modelPath)!);
            using (var fs = File.Create(_modelPath))
                _ml.Model.Save(model, trainData.Schema, fs);

            _model = model;
            _engine = _ml.Model.CreatePredictionEngine<StudentInputRow, StudentPrediction>(_model);
        }
    }

    public float PredictG3(JournalStudent s)
    {
        EnsureModelReady();

        lock (_lock)
        {
            if (_engine == null)
                throw new InvalidOperationException("Prediction engine не ініціалізовано.");

            if (s.AgeYears == null)
                throw new InvalidOperationException("Дата народження не задана.");

            var input = new StudentInputRow
            {
                Age = s.AgeYears.Value,
                Sex = s.Sex,
                MotherJob = s.MotherJob,
                FatherJob = s.FatherJob,

                Absences = s.Absences,
                Failures = s.Failures,
                Higher = s.Higher ? 1f : 0f,
                Tutoring = s.Tutoring ? 1f : 0f,

                PriorScore = s.PriorScore,
                G3 = 0
            };

            var pred = _engine.Predict(input);

            var g3 = Math.Clamp(pred.Score, 0f, 20f);
            return g3 * 5f;
        }
    }


    public static string Interpret(float g3)
    {
        if (g3 >= 80) return "Висока ймовірність успішного навчання";
        if (g3 >= 70) return "Середня успішність";
        if (g3 >= 45) return "Ризик зниження успішності";
        return "Високий ризик неуспішності (потрібна підтримка/план покращення)";
    }
}
