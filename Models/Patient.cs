using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace PatientApi.Models
{ 
public class Patient
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Family { get; set; } = null!;

    public string? GivenName { get; set; }
    public string? MiddleName { get; set; }

    [Required]
    public DateTime BirthDate { get; set; }

    [Required]
    [EnumDataType(typeof(Gender))]
    public Gender Gender { get; set; }

    public bool Active { get; set; }
}

public enum Gender
{
    male,
    female,
    other,
    unknown
}
}
