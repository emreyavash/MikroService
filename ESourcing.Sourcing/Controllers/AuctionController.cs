using AutoMapper;
using ESourcing.Sourcing.Entities;
using ESourcing.Sourcing.Repositories.Interface;
using EventBusRabbitMQ.Core;
using EventBusRabbitMQ.Events;
using EventBusRabbitMQ.Producer;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ESourcing.Sourcing.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuctionController : ControllerBase
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IBidRepository _bidRepository;
        private readonly IMapper _mapper;
        private readonly EventBusRabbitMQProducer _eventBus;
        private readonly ILogger<AuctionController> _logger;

        public AuctionController(IAuctionRepository auctionRepository, ILogger<AuctionController> logger, IBidRepository bidRepository,IMapper mapper,EventBusRabbitMQProducer eventBus)
        {
            _auctionRepository = auctionRepository;
            _bidRepository = bidRepository;
            _mapper = mapper;
            _eventBus = eventBus;
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
        [HttpPost("CompleteAuction")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        public async Task<ActionResult> CompleteAuction(string id)
        {
            Auction auction = await _auctionRepository.GetAuction(id);
            if(auction == null)
            {
                return NotFound();
            }
            if(auction.Status != (int)Status.Active)
            {
                _logger.LogError("Auction can not be completed");
                return BadRequest();
            }

            Bid bid = await _bidRepository.GetWinnerBid(id);

            if (bid == null)
            {
                return NotFound();
            }

            OrderCreateEvent eventMessage = _mapper.Map<OrderCreateEvent>(bid);
            eventMessage.Quantity = auction.Quantity;

            auction.Status = (int)Status.Closed;
            bool updateResponse = await _auctionRepository.Update(auction);
            if (!updateResponse)
            {
                _logger.LogError("Auction can not be updated");
                return BadRequest();
            }
            try
            {
                _eventBus.Publish(EventBusConstants.OrderCerateQueue,eventMessage);
            }
            catch (Exception ex)
            {

                 _logger.LogError(ex,"Error Pulishing integration event: {EventId} from {AppName} ",eventMessage.Id,"Sourcing");
                throw;
            }
            return Accepted();

        }

        [HttpPost("TestEvent")]
        public ActionResult<OrderCreateEvent> TestEvent()
        {
            OrderCreateEvent eventMessage = new OrderCreateEvent();
            eventMessage.AuctionId = "dummy1";
            eventMessage.ProductId = "dummy_product_1";
            eventMessage.Price = 10;
            eventMessage.Quantity = 100;
            eventMessage.SellerUserName = "test@test.com";
            try
            {
                _eventBus.Publish(EventBusConstants.OrderCerateQueue, eventMessage);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error Pulishing integration event: {EventId} from {AppName} ", eventMessage.Id, "Sourcing");
                throw;
            }
            return Accepted(eventMessage);
        }
    }
}
