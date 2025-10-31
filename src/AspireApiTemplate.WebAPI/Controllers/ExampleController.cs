using AspireApiTemplate.Contracts.Models;
using AspireApiTemplate.Contracts.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspireApiTemplate.WebAPI.Controllers;

[Route("[controller]")]
public class ExampleController(IExampleService exampleService) : ControllerBase
{
    #region Private Members

    private readonly IExampleService _exampleService = exampleService;

    #endregion Private Members

    #region Public Methods

    [HttpGet]
    public ActionResult<IEnumerable<ExampleCore>> GetExamples()
    {
        var examples = _exampleService.GetExamples();
        return examples.Any() ? Ok(examples) : NotFound();
    }

    #endregion Public Methods
}