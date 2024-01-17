using System;
using MongoDB.Driver;
using Newtonsoft.Json;
using RestSharp; 

namespace TES.Merchant.Web.UI.WebTasks
{
    public static class AddTerminalToMongo
    {
        private const string ConnectionString = "mongodb://reza:123456@doc01.tes.ir:22222";
 
     
        public   static void Add(TerminalMongo  terminal )
        {

            if(string.IsNullOrEmpty(terminal.TerminalNo))
                return;
            
            terminal.CreateDate = DateTime.Now;
            
            var   dbClient = new MongoClient(ConnectionString);
            var db = dbClient.GetDatabase("WageDb");
            var terminals = db.GetCollection<TerminalMongo>("PortalTerminal");
        
            var indexKeysDefinition = Builders<TerminalMongo>.IndexKeys.Ascending(hamster => hamster.TerminalNo);
            var t =    terminals.Indexes.CreateOneAsync(new CreateIndexModel<TerminalMongo>(indexKeysDefinition)).Result;
            
            var filter = Builders<TerminalMongo>.Filter.Eq("TerminalNo",  terminal.TerminalNo);
           
            var doc = terminals.Find(filter).FirstOrDefault();
            if (doc != null) return;
            
            
            terminals.InsertOne(terminal);
                
            var client = new RestClient("http://doc01.tes.ir:8004/api/sms");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            var text ="مشتری گرامی\n به شبکه پایانه های فروشگاهی بانک سرمایه خوش آمدید.\n خواهشمند است جهت آگاهی از نحوه مواجهه با هرگونه مشکل و ارائه پیشنهادات سازنده خود به آدرس https://b2n.ir/n26687 مراجعه فرمایید.";


            var body = new
            {
                Text = text,
                PhoneNumbers = new[] {terminal.PhoneNumber},
                TrackNumber = terminal.Id.ToString(),
                Source = "OldPoratl_NewTerminal"
            };
 
            request.AddParameter("application/json", JsonConvert.SerializeObject(body),  ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);


        }

    }
}