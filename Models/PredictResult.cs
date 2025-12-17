namespace StudentSuccessPredictor.Models;

public class PredictResult
{
    public string StudentId { get; set; } = "";
    public string FullName { get; set; } = "";
    public float PredictedG3 { get; set; }
    public string Interpretation { get; set; } = "";
}
