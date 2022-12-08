using ESourcing.Sourcing.Data.Interface;
using ESourcing.Sourcing.Entities;
using ESourcing.Sourcing.Repositories.Interface;
using MongoDB.Driver;

namespace ESourcing.Sourcing.Repositories
{
    public class BidRepository : IBidRepository
    {
        private readonly ISourcingContext _context;
        public BidRepository(ISourcingContext context)
        {
            _context = context;
        }

        public async Task<List<Bid>> GetBidsByAuctionId(string id)
        {
            FilterDefinition<Bid> filter = Builders<Bid>.Filter.Eq(p => p.AuctionId, id);
            List<Bid> bids = await _context.Bids.Find(filter).ToListAsync();

            bids = bids.OrderByDescending(p => p.CreatedAt)
                        .GroupBy(p => p.SellerUserName)
                        .Select(p => new Bid
                        {
                            AuctionId = p.FirstOrDefault().AuctionId,
                            ProductId = p.FirstOrDefault().ProductId,
                            SellerUserName = p.FirstOrDefault().SellerUserName,
                            Price = p.FirstOrDefault().Price,
                            CreatedAt = p.FirstOrDefault().CreatedAt,
                            Id = p.FirstOrDefault().Id
                        }).ToList();
            return bids;
        }

        public async Task<Bid> GetWinnerBid(string id)
        {
           List<Bid> bids = await GetBidsByAuctionId(id);
            return bids.OrderByDescending(p => p.Price).FirstOrDefault();

        }

        public async Task SendBid(Bid bid)
        {
            await _context.Bids.InsertOneAsync(bid);
        }
    }
}
