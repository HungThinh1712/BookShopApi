using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.DatabaseSettings
{
    public class BookShopDatabaseSettings: IBookShopDatabaseSettings
    {
        public string BooksCollectionName { get; set; }
        public string TypesCollectionName { get; set; }
        public string UsersCollectionName { get; set; }
        public string TagsCollectionName { get; set; }
        public string AuthorsCollectionName { get; set; }
        public string OrdersCollectionName { get; set; }
        public string PaymentsCollectionName { get; set; }
        public string CommentsCollectionName { get; set; }
        public string SlidesCollectionName { get; set; }
        public string ShoppingCartsCollectionName { get; set; }
        public string PublishingHousesCollectionName { get; set; }
        public string EnteredInvoicesCollectionName { get; set; }
        public string ProvincesCollectionName { get; set; }
        public string WardsCollectionName { get; set; }
        public string DistrictsCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IBookShopDatabaseSettings
    {
        string BooksCollectionName { get; set; }
        string TypesCollectionName { get; set; }
        string TagsCollectionName { get; set; }
        string UsersCollectionName { get; set; }
        string AuthorsCollectionName { get; set; }
        string OrdersCollectionName { get; set; }
        string PaymentsCollectionName { get; set; }
        string CommentsCollectionName { get; set; }
        string SlidesCollectionName { get; set; }
        string ShoppingCartsCollectionName { get; set; }
        string PublishingHousesCollectionName { get; set; }
        string EnteredInvoicesCollectionName { get; set; }
        string ProvincesCollectionName { get; set; }
        string WardsCollectionName { get; set; }
        string DistrictsCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
