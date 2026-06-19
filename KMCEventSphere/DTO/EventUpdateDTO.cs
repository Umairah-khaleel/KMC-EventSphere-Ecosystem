using System.ComponentModel.DataAnnotations;

public class EventUpdateDTO
{
    public DateTime? Date { get; set; }

    public string? Location { get; set; } 

    [Range(1, int.MaxValue)]
    public int? SeatsAvailable { get; set; }
}