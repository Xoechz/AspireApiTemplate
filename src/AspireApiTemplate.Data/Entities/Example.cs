using System.ComponentModel.DataAnnotations;

namespace AspireApiTemplate.Data.Entities;

public class Example
{
    #region Public Properties

    [Key]
    public required int ExampleKey { get; set; }

    #endregion Public Properties
}