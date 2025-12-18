namespace StudentSuccessPredictor.Models.Ml;

public class StudentInputRow
{
    public float Age { get; set; }
    public string Sex { get; set; } = "F";
    public string MotherJob { get; set; } = "other";
    public string FatherJob { get; set; } = "other";
    public float Absences { get; set; }
    public float Failures { get; set; }
    public float Higher { get; set; }    // 1/0
    public float Tutoring { get; set; }  // 1/0
    public float PriorScore { get; set; }

    // Label
    public float G3 { get; set; }
}
