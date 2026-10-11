using Claims.Auditing;
using Claims.Core.Entities;
using Claims.Core.Enums;
using Claims.Core.Interfaces;
using Claims.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Claims.Controllers;

[ApiController]
[Route("[controller]")]
public class CoversController : ControllerBase
{
    private readonly ICoverRepository _coverRepository;
    private readonly ILogger<CoversController> _logger;
    private readonly IAuditer _auditer;
    private readonly ICoverCalculator _coverCalculator;

    public CoversController(ICoverRepository coverRepository, ICoverCalculator coverCalculator, IAuditer auditer, ILogger<CoversController> logger)
    {
        _coverRepository = coverRepository;
        _logger = logger;
        _auditer = auditer;
        _coverCalculator = coverCalculator;
    }

    /// <summary>
    /// Calculates the estimated premium based on the coverage dates and type.
    /// </summary>
    /// <param name="startDate">The date the insurance cover begins.</param>
    /// <param name="endDate">The date the insurance cover ends.</param>
    /// <param name="coverType">The classification tier of the coverage.</param>
    /// <returns>The total calculated premium amount as a decimal.</returns>
    [HttpGet("compute")]
    public ActionResult<decimal> ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType)
    {
        _logger.LogInformation($"Computing premium for Cover with type: {coverType}");

        if (startDate >= endDate)
        {
            _logger.LogWarning($"Premium can not be computed if start date: {startDate} is not eariler than end date: {endDate}");
            return BadRequest("The start date must be earlier than the end date.");
        }

        var computedPremium = _coverCalculator.ComputePremium(startDate, endDate, coverType);
        _logger.LogInformation($"Premium computed for Cover with Type: {coverType} and Premium: {computedPremium}");
        return Ok(computedPremium);
    }

    /// <summary>
    /// Retrieves a list of all insurance covers in the system.
    /// </summary>
    /// <returns>A collection of covers.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cover>>> GetAsync()
    {
        _logger.LogInformation("Retrieving a list of all covers from the database.");

        var results = await _coverRepository.GetCoverAsync();
        _logger.LogInformation($"Successfully retrieved {results.Count()} covers.");
        return Ok(results);
    }

    /// <summary>
    /// Retrieves the details of a specific cover by its unique identifier.
    /// </summary>
    /// <param name="id">The unique string ID of the cover.</param>
    /// <returns>The requested cover object if found; otherwise, null.</returns>
    [HttpGet("{id}", Name = "GetCoverById")]
    public async Task<ActionResult<Cover>> GetAsync(string id)
    {
        _logger.LogInformation($"Fetching cover details for ID: {id}.");

        var cover = await _coverRepository.GetCoverAsync(id);
        if (cover == null)
        {
            _logger.LogWarning($"Cover retrieval failed: Cover with ID {id} was not found.");
            return NotFound();
        }

        _logger.LogInformation($"Cover details retrieved for ID: {id}.");
        return Ok(cover);
    }

    /// <summary>
    /// Creates and stores a new insurance cover in the system with calculated Premium.
    /// </summary>
    /// <param name="cover">The cover data to create.</param>
    /// <returns>The newly created cover object including its generated ID and calculated Premium.</returns>
    [HttpPost]
    public async Task<ActionResult<Cover>> CreateAsync(Cover cover)
    {
        _logger.LogInformation($"Initiating cover registration for type: {cover.Type} .");

        if (cover.StartDate < DateTime.UtcNow.Date)
        {
            _logger.LogWarning($"Cover validation failed: Start date {cover.StartDate} cannot be set in the past.");
            return BadRequest("The cover start date cannot be in the past.");
        }

        if (cover.EndDate > cover.StartDate.AddYears(1))
        {
            _logger.LogWarning($"Cover validation failed: Requested period crosses the 1-year threshold. Start date: {cover.StartDate}, End date: {cover.EndDate}).");
            return BadRequest("The total cover period cannot exceed 1 year.");
        }

        if (cover.StartDate >= cover.EndDate)
        {
            _logger.LogWarning($"Cover validation failed: The start date: {cover.StartDate} must be earlier than the end date: {cover.EndDate}.");
            return BadRequest("The start date must be earlier than the end date.");
        }

        cover.Premium = _coverCalculator.ComputePremium(cover.StartDate, cover.EndDate, cover.Type);
        await _coverRepository.CreateCoverAsync(cover);
        _auditer.AuditCover(cover.Id, "POST");
        _logger.LogInformation($"Successfully generated premium: {cover.Premium}, created and audited Cover with ID: {cover.Id}.");
        return CreatedAtRoute("GetCoverById", new { id = cover.Id }, cover);
    }

    /// <summary>
    /// Permanently removes a specific cover from the system.
    /// </summary>
    /// <param name="id">The unique string ID of the cover to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        _logger.LogInformation($"Initiating deletion request for Cover ID: {id}.");

        _auditer.AuditCover(id, "DELETE");
        await _coverRepository.DeleteCoverAsync(id);
        _logger.LogInformation($"Cover with ID {id} was successfully deleted and audited.");
        return NoContent();
    }
}
