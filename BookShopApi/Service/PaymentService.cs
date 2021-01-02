using BookShopApi.DatabaseSettings;
using BookShopApi.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookShopApi.Service
{
    public class PaymentService
    {
        private readonly IMongoCollection<Payment> _payments;

        public PaymentService(IBookShopDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _payments = database.GetCollection<Payment>(settings.PaymentsCollectionName);
        }

        public async Task<List<Payment>> GetAsync() =>
            await _payments.Find(payment => true).ToListAsync();

        public async Task<Payment> GetAsync(string id) =>
           await _payments.Find<Payment>(payment => payment.Id == id).FirstOrDefaultAsync();

        public async Task<Payment> CreateAsync(Payment payment)
        {
            await _payments.InsertOneAsync(payment);
            return payment;
        }

        public async Task UpdateAsync(string id, Payment paymentIn) =>
           await _payments.ReplaceOneAsync(payment => payment.Id == id, paymentIn);


        public async Task RemoveAsync(string id) =>
           await _payments.DeleteOneAsync(payment => payment.Id == id);
    }
}
