using AspireApiTemplate.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AspireApiTemplate.Data;

public class TemplateContext(DbContextOptions<TemplateContext> options) : DbContext(options)
{
    #region Public Properties

    public DbSet<Example> Examples => Set<Example>();

    #endregion Public Properties
}