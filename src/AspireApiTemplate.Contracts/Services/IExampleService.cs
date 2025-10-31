using AspireApiTemplate.Contracts.Models;

namespace AspireApiTemplate.Contracts.Services;

public interface IExampleService
{
    IEnumerable<ExampleCore> GetExamples();
}