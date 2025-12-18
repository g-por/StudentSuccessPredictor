using StudentSuccessPredictor.Models.Ml;

namespace StudentSuccessPredictor.Services;

public static class CsvUciLoader
{
    public static IEnumerable<StudentInputRow> LoadUciFile(string filePath)
    {
        if (!File.Exists(filePath)) yield break;

        using var reader = new StreamReader(filePath);
        var header = reader.ReadLine();
        if (header is null) yield break;

        var cols = SplitUciLine(header);
        var idxAge = Array.IndexOf(cols, "age");
        var idxSex = Array.IndexOf(cols, "sex");
        var idxMjob = Array.IndexOf(cols, "Mjob");
        var idxFjob = Array.IndexOf(cols, "Fjob");
        var idxG1 = Array.IndexOf(cols, "G1");
        var idxG2 = Array.IndexOf(cols, "G2");
        var idxG3 = Array.IndexOf(cols, "G3");
        var idxAbsences = Array.IndexOf(cols, "absences");
        var idxFailures = Array.IndexOf(cols, "failures");
        var idxHigher = Array.IndexOf(cols, "higher");
        var idxPaid = Array.IndexOf(cols, "paid");

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = SplitUciLine(line);
            if (parts.Length != cols.Length) continue;

            if (!float.TryParse(parts[idxAge], out var age)) continue;
            var sex = parts[idxSex];
            var mjob = parts[idxMjob];
            var fjob = parts[idxFjob];

            if (!float.TryParse(parts[idxG1], out var g1)) continue;
            if (!float.TryParse(parts[idxG2], out var g2)) continue;
            if (!float.TryParse(parts[idxG3], out var g3)) continue;

            float absences = 0;
            float failures = 0;
            float higher = 0;
            float tutoring = 0;

            if (idxPaid >= 0)
                tutoring = parts[idxPaid].Trim().Equals("yes", StringComparison.OrdinalIgnoreCase) ? 1f : 0f;
            if (idxAbsences >= 0) float.TryParse(parts[idxAbsences], out absences);
            if (idxFailures >= 0) float.TryParse(parts[idxFailures], out failures);
            if (idxHigher >= 0) higher = parts[idxHigher].Trim().Equals("yes", StringComparison.OrdinalIgnoreCase) ? 1f : 0f;
            var prior = ((g1 + g2) / 2f) * 5f;

            yield return new StudentInputRow
            {
                Age = age,
                Sex = NormalizeSex(sex),
                MotherJob = NormalizeJob(mjob),
                FatherJob = NormalizeJob(fjob),
                Absences = absences,
                Failures = failures,
                Higher = higher,
                Tutoring = tutoring,
                PriorScore = prior,
                G3 = g3
            };
        }
    }

    public static IEnumerable<StudentInputRow> LoadCustomRows(string filePath)
    {
        if (!File.Exists(filePath)) yield break;

        using var reader = new StreamReader(filePath);
        var header = reader.ReadLine();
        if (header is null) yield break;


        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var p = line.Split(',', StringSplitOptions.TrimEntries);
            if (p.Length < 10) continue;

            if (!float.TryParse(p[0], out var age)) continue;
            var sex = NormalizeSex(p[1]);
            var mjob = NormalizeJob(p[2]);
            var fjob = NormalizeJob(p[3]);

            if (!float.TryParse(p[4], out var absences)) continue;
            if (!float.TryParse(p[5], out var failures)) continue;

            if (!float.TryParse(p[6], out var higher)) continue;   // 0/1
            if (!float.TryParse(p[7], out var tutoring)) continue; // 0/1

            if (!float.TryParse(p[8], out var prior)) continue;
            if (!float.TryParse(p[9], out var g3)) continue;

            yield return new StudentInputRow
            {
                Age = age,
                Sex = sex,
                MotherJob = mjob,
                FatherJob = fjob,
                Absences = absences,
                Failures = failures,
                Higher = higher,
                Tutoring = tutoring,
                PriorScore = prior,
                G3 = g3
            };
        }
    }

    private static string[] SplitUciLine(string line)
    {

        line = line.Replace("\"", "");
        return line.Split(';', StringSplitOptions.TrimEntries);
    }

    private static string NormalizeSex(string s)
        => s.Trim().Equals("M", StringComparison.OrdinalIgnoreCase) ? "M" : "F";

    private static string NormalizeJob(string s)
    {
        s = s.Trim().ToLowerInvariant();
        return s switch
        {
            "teacher" => "teacher",
            "health" => "health",
            "services" => "services",
            "at_home" => "at_home",
            _ => "other"
        };
    }
}
