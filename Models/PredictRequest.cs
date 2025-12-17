using System.ComponentModel.DataAnnotations;

namespace StudentSuccessPredictor.Models;

public class PredictRequest
{
    [Required]
    public string StudentId { get; set; } = "";
}
