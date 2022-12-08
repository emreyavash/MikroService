using ESourcing.Sourcing.Entities;
using ESourcing.Sourcing.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ESourcing.Sourcing.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuctionController : ControllerBase
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly ILogger<AuctionController> _logger;

        public AuctionController(IAuctionRepository auctionRepository, ILogger<AuctionController> logger)
        {
            _auctionRepository = auctionRepository;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Auction>),(int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<Auction>>> GetAuctions()
        {
            var auctions = await _auctionRepository.GetAuctions();
            return Ok(auctions);
        }

        [HttpGet("{id:length(24)}",Name ="GetAuction")]
        [ProducesResponseType(typeof(Auction), (int)HttpStatusCode.OK)]
        [ProducesResponseType( (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<Auction>> GetAction(string id)
        {
            var auction = await _auctionRepository.GetAuction(id);
            if (auction == null)
            {
                _logger.LogError($"Auction {id} hasn't been found in database");
                return NotFound();
            }
            return Ok(auction);
        }

        [HttpPost]
        [ProducesResponseType(typeof(Auction), (int)HttpStatusCode.Created)]
        public async Task<ActionResult<Auction>> CreateAuction(Auction auction)
        {
            await _auctionRepository.Create(auction);

            return CreatedAtRoute("GetAuction", new { id = auction.Id }, auction);
        }

        [HttpPut]
        [ProducesResponseType(typeof(Auction), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<Auction>> UpdateAuction(Auction auction)
        {
            var updateAuction =await _auctionRepository.Update(auction);
            return Ok(updateAuction);
        }

        [HttpDelete("{id:length(24)}")]
        [ProducesResponseType(typeof(Auction),(int)HttpStatusCode.OK)]
        public async Task<ActionResult<Auction>> DeleteAuction(string id)
        {
            var deleteAuction = await _auctionRepository.Delete(id);
            return Ok(deleteAuction);
        }

    }
}
