using CustomAPITemplate.DB.Models;
using CustomAPITemplate.DB.Repositories.Interfaces;

namespace CustomAPITemplate.DB.Repositories;

public class ExampleRepository : Repository<int, Example>, IExampleRepository
{
    public ExampleRepository(AppDbContext context)
        :base(context)
    {

    }
}