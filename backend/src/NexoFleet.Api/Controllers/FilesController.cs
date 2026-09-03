using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexoFleet.Api.Dtos.Files;
using NexoFleet.Application.Abstractions.Storage;

namespace NexoFleet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/files")]
public sealed class FilesController(IFileStorage fileStorage) : ControllerBase
{
    /// <summary>Sube un archivo al almacenamiento seguro del sistema.</summary>
    /// <param name="file">Archivo multipart a cargar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<FileUploadResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FileUploadResponse>> Upload(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Archivo no válido",
                Detail = "No se ha proporcionado un archivo válido para la carga."
            });
        }

        await using var stream = file.OpenReadStream();
        var storageKey = await fileStorage.UploadAsync(
            stream,
            file.FileName,
            file.ContentType,
            cancellationToken);

        var response = new FileUploadResponse(
            file.FileName,
            storageKey,
            file.ContentType,
            file.Length);

        return Ok(response);
    }

    /// <summary>Descarga un archivo previamente almacenado.</summary>
    /// <param name="storageKey">Clave o ruta relativa del archivo almacenado.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpGet("{**storageKey}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(
        [FromRoute] string storageKey,
        CancellationToken cancellationToken)
    {
        var stream = await fileStorage.DownloadAsync(storageKey, cancellationToken);
        if (stream is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Archivo no encontrado",
                Detail = $"No se encontró el archivo con clave '{storageKey}'."
            });
        }

        var fileName = Path.GetFileName(storageKey);
        return File(stream, "application/octet-stream", fileName);
    }
}
