using AutoMapper;
using CustomAPITemplate.Contract.V1;
using CustomAPITemplate.Core.Excel;
using CustomAPITemplate.DB.Models;
using CustomAPITemplate.DB.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomAPITemplate.Controllers.V1;

public class ExampleController(IExampleRepository _repository, IMapper _mapper)
    : BasicV1Controller<int, Example, ExampleRequest, ExampleResponse, IExampleRepository>(_repository, _mapper)
{

    //TODO: Delete
    [HttpGet]
    [Route("report")]
    [AllowAnonymous]
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
            ColumnProperties =
            [
                new() { PropertyName = nameof(Example.Id), HeaderName = "Id Header" },
                new() { PropertyName = nameof(Example.Test1), HeaderName = "Test1 Header" },
                new() { PropertyName = nameof(Example.Test2), HeaderName = "Test2 Header" },
                new() { PropertyName = nameof(Example.CreatorUserId), HeaderName = "CreatorUserId Header" },
                new() { PropertyName = nameof(Example.UpdateUserId), HeaderName = "UpdateUserId Header" },
                new() { PropertyName = nameof(Example.CreatorUser), HeaderName = "CreatorUser Header" },
                new() { PropertyName = nameof(Example.HostIP), HeaderName = "HostIP Header" },
            ],
            Data = dataResponse.Value.Cast<object>().ToList()
        };

        var excelSheetData2 = new ExcelSheetData
        {
            SheetName = "Example Report 2",
            ColumnProperties =
            [
                new() { PropertyName = nameof(Example.Id), HeaderName = "Id Header" },
                new() { PropertyName = nameof(Example.Test1), HeaderName = "Test1 Header" },
                new() { PropertyName = nameof(Example.Test2), HeaderName = "Test2 Header" },
                new() { PropertyName = nameof(Example.CreationTime), HeaderName = "CreationTime Header" },
                new() { PropertyName = nameof(Example.UpdateTime), HeaderName = "UpdateTime Header", Format = "dd.mm.yyyy hh:mm" },
                new() { PropertyName = "Value", HeaderName = "Value Header", Format = "#,##00" },
                new() { PropertyName = "Value2", HeaderName = "Value2 Header" },
            ],
            Data = dataResponse.Value.OrderByDescending(x => x.Id).Select(x =>
            {
                var value = Random.Shared.NextDouble() + Random.Shared.Next();
                return new
                {
                    x.Id,
                    x.Test1,
                    x.Test2,
                    x.CreationTime,
                    x.UpdateTime,
                    Value = value,
                    Value2 = value,
                };
            }).Cast<object>().ToList()
        };

        var excelHelper = new ExcelHelper(excelSheetData, excelSheetData2);

        var bytes = excelHelper.Create();
        return File(bytes, "application/octet-stream", $"ExampleReport-{DateTime.UtcNow:yyyyMMddHHmmssfff}.xlsx");
    }
}