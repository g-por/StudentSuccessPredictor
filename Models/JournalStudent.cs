using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StudentSuccessPredictor.Models;

public class JournalStudent
{
    [Required]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [Required, StringLength(120)]
    public string FullName { get; set; } = "";

    [Required]
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }


    [JsonIgnore]
    public int? AgeYears
    {
        get
        {
            if (DateOfBirth == null) return null;

            var dob = DateOfBirth.Value.Date;
            var today = DateTime.Today;
            var age = today.Year - dob.Year;
            if (dob > today.AddYears(-age)) age--;
            return Math.Max(0, age);
        }
    }

    [Required, RegularExpression("M|F")]
    public string Sex { get; set; } = "F";
    [Range(0, 150)]
    public int Absences { get; set; } = 0;

    [Range(0, 3)]
    public int Failures { get; set; } = 0;

    public bool Higher { get; set; } = true;

    public bool Tutoring { get; set; } = false;

    [Required]
    public string MotherJob { get; set; } = "other";

    [Required]
    public string FatherJob { get; set; } = "other";

    [Range(0, 100)]
    public float PriorScore { get; set; }

    public float? PredictedG3 { get; set; }
    public DateTimeOffset? LastUpdatedUtc { get; set; }


}
