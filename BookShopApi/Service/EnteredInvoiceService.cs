using BookShopApi.DatabaseSettings;
using BookShopApi.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookShopApi.Service
{
    public class EnteredInvoiceService
    {
        private readonly IMongoCollection<EnteredInvoice> _enteredInvoices;

        public EnteredInvoiceService(IBookShopDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _enteredInvoices = database.GetCollection<EnteredInvoice>(settings.EnteredInvoicesCollectionName);
        }

        public async Task<List<EnteredInvoice>> GetAsync() =>
            await _enteredInvoices.Find(enteredInvoice => true).ToListAsync();

        public async Task<EnteredInvoice> GetAsync(string id) =>
           await _enteredInvoices.Find<EnteredInvoice>(enteredInvoice => enteredInvoice.Id == id).FirstOrDefaultAsync();

        public async Task<EnteredInvoice> CreateAsync(EnteredInvoice enteredInvoice)
        {
            await _enteredInvoices.InsertOneAsync(enteredInvoice);
            return enteredInvoice;
        }

        public async Task UpdateAsync(string id, EnteredInvoice enteredInvoiceIn) =>
           await _enteredInvoices.ReplaceOneAsync(enteredInvoice => enteredInvoice.Id == id, enteredInvoiceIn);


        public async Task RemoveAsync(string id) =>
           await _enteredInvoices.DeleteOneAsync(enteredInvoice => enteredInvoice.Id == id);
    }
}
