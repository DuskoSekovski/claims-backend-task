using Claims.Auditing;
using Claims.Core.Entities;
using Claims.Core.Interfaces;
using Claims.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;


namespace Claims.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClaimsController : ControllerBase
    {
        private readonly ILogger<ClaimsController> _logger;
        private readonly IClaimRepository _claimsRepository;
        private readonly IAuditer _auditer;
        private readonly ICoverRepository _coverRepository;

        public ClaimsController(ILogger<ClaimsController> logger, IClaimRepository claimsContext, IAuditer auditer, ICoverRepository coverRepository)
        {
            _logger = logger;
            _claimsRepository = claimsContext;
            _auditer = auditer;
            _coverRepository = coverRepository;
        }

        /// <summary>
        /// Retrieves a list of all insurance claims in the system.
        /// </summary>
        /// <returns>A collection of claims.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Claim>>> GetAsync()
        {
            _logger.LogInformation("Retrieving a list of all claims from the database.");

            var claims = await _claimsRepository.GetClaimsAsync();
            _logger.LogInformation($"Successfully retrieved {claims.Count()} claims.");
            return Ok(claims);
        }

        /// <summary>
        /// Creates and stores a new insurance claim in the system.
        /// </summary>
        /// <param name="claim">The claim data to create.</param>
        /// <returns>The newly created claim object including its generated ID.</returns>
        [HttpPost]
        public async Task<ActionResult<Claim>> CreateAsync(Claim claim)
        {
            _logger.LogInformation($"Processing creation request for a new claim named {claim.Name} with Cover ID: {claim.CoverId}.");

            if (claim == null)
            {
                _logger.LogWarning("Claim creation aborted: Request payload was null.");
                return BadRequest("Claim data cannot be null.");
            }

            var relatedCover = await _coverRepository.GetCoverAsync(claim.CoverId);
            if(relatedCover == null)
            {
                _logger.LogWarning($"Claim creation rejected: Cover ID {claim.CoverId} could not be found in the database.");
                return BadRequest($"The related Cover with ID {claim.CoverId} does not exist.");
            }

            if (claim.Created < relatedCover.StartDate || claim.Created > relatedCover.EndDate)
            {
                _logger.LogWarning($"Claim creation rejected: Claim date {claim.Created} sits outside active Cover period ({relatedCover.StartDate} to {relatedCover.EndDate}).");
                return BadRequest($"The claim creation date must be between the related cover's period ({relatedCover.StartDate:yyyy-MM-dd} to {relatedCover.EndDate:yyyy-MM-dd}).");
            }

            await _claimsRepository.CreateClaimAsync(claim);
            _auditer.AuditClaim(claim.Id, "POST");
            _logger.LogInformation($"Successfully created and audited claim with ID: {claim.Id}.");
            return CreatedAtRoute("GetClaimById", new { id = claim.Id }, claim);
        }

        /// <summary>
        /// Permanently removes a specific claim from the system.
        /// </summary>
        /// <param name="id">The unique string ID of the claim to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            _logger.LogInformation($"Initiating deletion request for Claim ID: {id}.");

            _auditer.AuditClaim(id, "DELETE");
            await _claimsRepository.DeleteClaimAsync(id);
            _logger.LogInformation($"Claim with ID {id} was successfully deleted and audited.");
            return NoContent();
        }

        /// <summary>
        /// Retrieves the details of a specific claim by its unique identifier.
        /// </summary>
        /// <param name="id">The unique string ID of the claim.</param>
        /// <returns>The requested claim object if found; otherwise, null.</returns>
        [HttpGet("{id}", Name = "GetClaimById")]
        public async Task<ActionResult<Claim>> GetAsync(string id)
        {
            _logger.LogInformation($"Fetching claim details for ID: {id}.");

            var claim = await _claimsRepository.GetClaimAsync(id);
            if (claim == null)
            {
                _logger.LogWarning($"Claim retrieval failed: Claim with ID {id} was not found.");
                return NotFound();
            }

            _logger.LogInformation($"Claim details retrieved for ID: {id}.");
            return Ok(claim);
        }
    }
}
