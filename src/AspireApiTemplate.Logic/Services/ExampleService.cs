using AspireApiTemplate.Contracts.Models;
using AspireApiTemplate.Data;

namespace AspireApiTemplate.Logic.Services;

public class ExampleService(TemplateContext context)
{
    #region Private Members

    private readonly TemplateContext _context = context;

    #endregion Private Members

    #region Public Methods

    public IEnumerable<ExampleCore> GetExamples()
    {
        return _context.Examples.Select(e => new ExampleCore
        {
            ExampleKey = e.ExampleKey
        });
    }

    #endregion Public Methods
}