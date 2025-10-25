using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AspireApiTemplate.Data;

internal class DesignTimeTemplateContextFactory : IDesignTimeDbContextFactory<TemplateContext>
{
    #region Public Methods

    public TemplateContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TemplateContext>();
        optionsBuilder.UseSqlServer("Server=.;Database=DesignTime;Trusted_Connection=True;");
        return new TemplateContext(optionsBuilder.Options);
    }

    #endregion Public Methods
}