using CustomAPITemplate.DB.Models;

namespace CustomAPITemplate.DB.Repositories;

public class ExampleRepository : Repository<int, Example>, IExampleRepository
{
    public ExampleRepository(AppDbContext context)
        : base(context)
    {

    }
}