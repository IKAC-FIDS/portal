using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace TES.Merchant.Web.UI.WebTasks
{
    public class MongoHelper
    {
        private IMongoDatabase db;

        public MongoHelper(string connectionString, string databaseName)
        {
            //Create new database connection
            var client = new MongoClient(connectionString);
            db = client.GetDatabase(databaseName);
        }

        /// <summary>
        /// Insert new document into collection
        /// </summary>
        /// <typeparam name="T">Document data type</typeparam>
        /// <param name="collectionName">Collection name</param>
        /// <param name="document">Document</param>
        public void InsertDocument<T>(string collectionName, T document)
        {
            var collection = db.GetCollection<T>(collectionName);
            collection.InsertOne(document);
        }
        
        
        public void InsertDocuments<T>(string collectionName, List<T> document)
        {
            var collection = db.GetCollection<T>(collectionName);
            collection.InsertMany(document); 
        }

        /// <summary>
        /// Load all documents in collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        public List<T> LoadAllDocuments<T>(string collectionName)
        {
            var collection = db.GetCollection<T>(collectionName);

            return collection.Find(new BsonDocument()).ToList();
        }

        /// <summary>
        /// Load document by Id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public T LoadDocumentById<T>(string collectionName, Guid id)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("Id", id);

            return collection.Find(filter).First();
        }

        
        public List<T> LoadDocumentByMonthAndYear<T>(string collectionName, int month,int year)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("Month", month);
            filter &= (Builders<T>.Filter.Eq( "Year", year));
            return collection.Find(filter    ).ToList();
        }
        /// <summary>
        /// Update document
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName"></param>
        /// <param name="id"></param>
        /// <param name="document"></param>
        public void UpdateDocument<T>(string collectionName, Guid id, T document)
        {
            var collection = db.GetCollection<T>(collectionName);

            var result = collection.ReplaceOne(
                new BsonDocument("_id", id),
                document,
                new UpdateOptions { IsUpsert = false });
        }

        /// <summary>
        /// Insert document into collection if it does not already exist, or update it if it does
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName"></param>
        /// <param name="id"></param>
        /// <param name="document"></param>
        public void UpsertDocument<T>(string collectionName, Guid id, T document)
        {
            var collection = db.GetCollection<T>(collectionName);

            var result = collection.ReplaceOne(
                new BsonDocument("_id", id),
                document,
                new UpdateOptions { IsUpsert = true });
        }

        /// <summary>
        /// Delete document by Id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName"></param>
        /// <param name="id"></param>
        public void DeleteDocument<T>(string collectionName, Guid id)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("Id", id);
            collection.DeleteOne(filter);

        }
    }

    public class TerminalMongo
    {
      
            [BsonId]
            [BsonRepresentation(BsonType.ObjectId)]
            public  string _id { get; set; }
            
             public long Id { get; set; }

    public string MerchantNo { get; set; }

    public long DeviceTypeId { get; set; }

    public string InstallationDate { get; set; }

    public string LastUpdateTime { get; set; }

    public string RevokeDate { get; set; }

    public string TerminalNo { get; set; }


    public string Title { get; set; }

    public string StepCodeTitle { get; set; }
    public int? StepCode { get; set; }

    public int? InstallStatusId { get; set; }
    public string InstallStatus { get; set; }

    public string Description { get; set; }

    public string EnglishTitle { get; set; }

    public long BranchId { get; set; }

    public string AccountNo { get; set; }

    public string ShebaNo { get; set; }

    public byte StatusId { get; set; }

    public byte? PspId { get; set; }

    public string BatchDate { get; set; }

    public long CityId { get; set; }

    public byte? RegionalMunicipalityId { get; set; }

    public string TelCode { get; set; }

    public string Tel { get; set; }

    public string Address { get; set; }


    public string PostCode { get; set; }

    public long MarketerId { get; set; }

    public string ContractNo { get; set; }

    public string  ContractDate { get; set; }

    public string SubmitTime { get; set; }

    public long UserId { get; set; }

    public long GuildId { get; set; }

    public long MerchantProfileId { get; set; }

    public byte ActivityTypeId { get; set; }

    public string ErrorComment { get; set; }

    public string ShaparakAddressFormat { get; set; }

    public string EnglishAddress { get; set; }

    public string BlockDocumentDate { get; set; }

    public string BlockDocumentNumber { get; set; }

    public string BlockAccountNumber { get; set; }

    public int? BlockPrice { get; set; }

    public byte? PreferredPspId { get; set; }
    public byte? BlockDocumentStatusId { get; set; }

    public string BlockDocumentStatusChangedToRegistredDate { get; set; }


    public string TaxPayerCode { get; set; }

    public string NewParsian { get; set; }


    public int? TopiarId { get; set; }
    public int? CustomerCategoryId { get; set; }
    public string JobUpdated { get; set; }
    public bool IsWireLess { get; set; }
    
          
            public DateTime CreateDate { get; set; }
            public  string PhoneNumber { get; set; }
    }
}