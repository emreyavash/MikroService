using ESourcing.Sourcing.Data.Interface;
using ESourcing.Sourcing.Entities;
using ESourcing.Sourcing.Repositories.Interface;
using MongoDB.Driver;

namespace ESourcing.Sourcing.Repositories
{
    public class AuctionRepository : IAuctionRepository
    {
        private readonly ISourcingContext _context;
        public AuctionRepository(ISourcingContext context)
        {
            _context = context;
        }
        public async Task Create(Auction auction)
        {
            await _context.Auctions.InsertOneAsync(auction);
        }

        public async Task<bool> Delete(string id)
        {
            FilterDefinition<Auction> filterDefinition = Builders<Auction>.Filter.Eq(p => p.Id, id);
            DeleteResult deleteResult = await _context.Auctions.DeleteOneAsync(filterDefinition);
            return deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0;
        }

        public async Task<Auction> GetAuction(string id)
        {
           return await _context.Auctions.Find(p=> p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Auction> GetAuctionByName(string name)
        {
            FilterDefinition<Auction> filterDefinition = Builders<Auction>.Filter.Eq(p => p.Name, name);

            return await _context.Auctions.Find(filterDefinition).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Auction>> GetAuctions()
        {
            return await _context.Auctions.Find(p => true).ToListAsync();
        }

        public async Task<bool> Update(Auction auction)
        {
            var updateAuction = await _context.Auctions.ReplaceOneAsync(p => p.Id == auction.Id, auction);
            return updateAuction.IsAcknowledged && updateAuction.ModifiedCount > 0;
        }
    }
}
