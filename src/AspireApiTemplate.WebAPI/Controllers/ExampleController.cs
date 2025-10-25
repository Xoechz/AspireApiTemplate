using AspireApiTemplate.Contracts.Models;
using AspireApiTemplate.Logic.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspireApiTemplate.Contracts.Controllers;

public class ExampleController(ExampleService exampleService) : ControllerBase
{
    #region Private Members

    private readonly ExampleService _exampleService = exampleService;

    #endregion Private Members

    #region Public Methods

    [HttpGet("examples")]
    public ActionResult<IEnumerable<ExampleCore>> GetExamples()
    {
        var examples = _exampleService.GetExamples();
        return examples.Any() ? Ok(examples) : NotFound();
    }

    #endregion Public Methods
}