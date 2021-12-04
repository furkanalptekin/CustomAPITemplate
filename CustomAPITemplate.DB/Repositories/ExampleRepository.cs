using CustomAPITemplate.DB.Models;
using CustomAPITemplate.DB.Repositories.Interfaces;

namespace CustomAPITemplate.DB.Repositories;

public class ExampleRepository : Repository<Example>, IExampleRepository
{
    public ExampleRepository(AppDbContext context)
        :base(context)
    {

    }
}