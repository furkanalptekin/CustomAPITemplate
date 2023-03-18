using AutoMapper;
using CustomAPITemplate.Contract.V1;
using CustomAPITemplate.Core.Excel;
using CustomAPITemplate.DB.Models;
using CustomAPITemplate.DB.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CustomAPITemplate.Controllers.V1;

public class ExampleController : BasicV1Controller<int, Example, ExampleRequest, ExampleResponse, IExampleRepository>
{
    public ExampleController(IExampleRepository repository, IMapper mapper) 
        : base(repository, mapper)
    {

    }

    //TODO: Delete
    [HttpGet]
    [Route("report")]
    public async Task<IActionResult> Report(CancellationToken token)
    {
        var dataResponse = await _repository.GetAsync(token);
        if (!dataResponse.Success)
        {
            return BadRequest(dataResponse.Results);
        }

        var excelSheetData = new ExcelSheetData
        {
            SheetName = "Example Report",
            HeaderProperties = new List<HeaderProperties>
            {
                new HeaderProperties { PropertyName = nameof(Example.Id), HeaderName = "Id Header" },
                new HeaderProperties { PropertyName = nameof(Example.Test1), HeaderName = "Test1 Header" },
                new HeaderProperties { PropertyName = nameof(Example.Test2), HeaderName = "Test2 Header" },
                new HeaderProperties { PropertyName = nameof(Example.CreatorUserId), HeaderName = "CreatorUserId Header" },
                new HeaderProperties { PropertyName = nameof(Example.UpdateUserId), HeaderName = "UpdateUserId Header" },
                new HeaderProperties { PropertyName = nameof(Example.CreatorUser), HeaderName = "CreatorUser Header" },
                new HeaderProperties { PropertyName = nameof(Example.HostIP), HeaderName = "HostIP Header" },
            },
            Data = dataResponse.Value.Cast<object>().ToList()
        };

        var excelSheetData2 = new ExcelSheetData
        {
            SheetName = "Example Report 2",
            HeaderProperties = new List<HeaderProperties>
            {
                new HeaderProperties { PropertyName = nameof(Example.Id), HeaderName = "Id Header" },
                new HeaderProperties { PropertyName = nameof(Example.Test1), HeaderName = "Test1 Header" },
                new HeaderProperties { PropertyName = nameof(Example.Test2), HeaderName = "Test2 Header" },
                new HeaderProperties { PropertyName = nameof(Example.CreationTime), HeaderName = "CreationTime Header" },
            },
            Data = dataResponse.Value.OrderByDescending(x => x.Id).Cast<object>().ToList()
        };

        var excelHelper = new ExcelHelper(excelSheetData, excelSheetData2);

        var bytes = excelHelper.Create();
        return Ok(bytes);
    }
}