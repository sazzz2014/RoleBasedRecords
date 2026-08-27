using Microsoft.AspNetCore.Mvc;
using RoleBasedRecords.Api.Auth;
using RoleBasedRecords.Application.Records;

namespace RoleBasedRecords.Api.Controllers;

[ApiController]
[Route("api/records")]
public sealed class RecordsController(
    RecordService recordService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<DataRecordResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DataRecordResponse>>> List(
        CancellationToken cancellationToken)
    {
        return Ok(await recordService.ListAsync(User.GetRole(), cancellationToken));
    }
}
