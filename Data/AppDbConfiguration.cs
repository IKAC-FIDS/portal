using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace TES.Data
{
    //public class AppDbConfiguration : DbConfiguration
    //{
    //    public AppDbConfiguration()
    //    {
    //        //SetModelStore(new DefaultDbModelStore(AppDomain.CurrentDomain.GetData("DataDirectory").ToString()));
    //        //SetDatabaseInitializer<AppDataContext>(null);

    //        SetDatabaseInitializer(new DropCreateDatabaseAlways<AppDataContext>());
    //        using (var context = new AppDataContext())
    //        {
    //            context.Database.Initialize(force: true);
    //        }
    //    }
    //}
}