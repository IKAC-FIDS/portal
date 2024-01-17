using System;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TES.Data.Domain;

namespace TES.Data
{
    
    
    public partial class AppDataContext : IdentityDbContext<User, Role, long, UserLogin, UserRole, UserClaim>
    {
        public AppDataContext()
            : base("name=AppDataContext")
        {
        }

        public virtual DbSet<AutomatedTaskLog> AutomatedTaskLogs { get; set; }
        public  virtual  DbSet<UpdateJob> UpdateJob { get; set; }
        public  virtual  DbSet<UpdateWageTask> UpdateWageTask { get; set; }

        public  virtual  DbSet<MessageSubject> MessageSubjects { get; set; }

        
        
        public  virtual  DbSet<TotalWageReport> TotalWageReport { get; set; }
        public  virtual  DbSet<UpdateJobDetails> UpdateJobDetails { get; set; }
        public virtual DbSet<AddressComponent> AddressComponents { get; set; }
        public virtual DbSet<City> Cities { get; set; }
        public  virtual DbSet<BranchConnector> BranchConnector { get; set; }
        public  virtual DbSet<TaskError> TaskErrors { get; set; }
        public virtual DbSet<Nationality> Nationalities { get; set; }
        public virtual DbSet<OrganizationUnit> OrganizationUnits { get; set; }
        public virtual DbSet<RegionalMunicipality> RegionalMunicipalities { get; set; }
        public virtual DbSet<State> States { get; set; }
        public virtual DbSet<ActivityType> ActivityTypes { get; set; }
        public virtual DbSet<ChangeAccountRequest> ChangeAccountRequests { get; set; }
        public virtual DbSet<DeviceType> DeviceTypes { get; set; }
        public virtual DbSet<DocumentType> DocumentTypes { get; set; }
        public virtual DbSet<Guild> Guilds { get; set; }
        public virtual DbSet<Marketer> Marketers { get; set; }
        public virtual DbSet<MerchantProfile> MerchantProfiles { get; set; }
        
        public virtual DbSet<RemovedMerchantProfile> RemovedMerchantProfile { get; set; }
        public virtual DbSet<MerchantProfileDocument> MerchantProfileDocuments { get; set; }
        public virtual DbSet<Psp> Psps { get; set; }
        public virtual DbSet<Seriall> Serialls { get; set; }
        public virtual DbSet<NormalRep> NormalReps { get; set; }
        public virtual DbSet<Device_Card> Device_Cards { get; set; }
        public virtual DbSet<Markerter> Markerters { get; set; }

        public virtual DbSet<BranchTerminal> BranchTerminal { get; set; }
        public  virtual DbSet<NotFoundTerminal> NotFoundTerminal { get; set; }
        public  virtual  DbSet<CustomerCategory> CustomerCategory { get; set; }
        public virtual DbSet<PspAgent> PspAgents { get; set; }
        public virtual DbSet<RequestStatus> RequestStatus { get; set; }
        public virtual DbSet<RevokeReason> RevokeReasons { get; set; }
        public virtual DbSet<RevokeRequest> RevokeRequests { get; set; }
        public virtual DbSet<RemovedTerminal> RemovedTerminals { get; set; }
        public virtual DbSet<Terminal> Terminals { get; set; }
        public virtual  DbSet<_damage> _damage { get; set; }

        public virtual  DbSet<_tempSarmayeh> _tempSarmayeh { get; set; }
        public virtual DbSet<CalculateResult> CalculateResults { get; set; }
        public virtual DbSet<CustomerStatusResult> CustomerStatusResults { get; set; }

        public virtual  DbSet<ImportTest> ImportTests { get; set; }
        
        public virtual DbSet<TerminalDocument> TerminalDocuments { get; set; }
        public virtual DbSet<TerminalStatus> TerminalStatus { get; set; }

        public virtual DbSet<BranchPermissionType> BranchPermissionType { get; set; }
        public virtual DbSet<BranchPermission> BranchPermission { get; set; }
        public virtual DbSet<News> News { get; set; }
        public virtual DbSet<NewsDocument> NewsDocument { get; set; }

        #region  CardRequest

        public virtual DbSet<CardRequest> CardRequest { get; set; }
        public virtual DbSet<CardRequestReply> CardRequestReplies { get; set; }
        public virtual DbSet<CardRequestDocument> CardRequestDocuments { get; set; }
        public virtual DbSet<CardRequestReplyDocument> CardRequestReplyDocuments { get; set; }
        public  virtual DbSet<CardType> CardType { get; set; }
        public  virtual  DbSet<CardTemplate> CardTemplate { get; set; }

        #endregion

        #region  Storage

        public  virtual DbSet<Storage> Storage { get; set; }
        public virtual  DbSet<StorageLog> StorageLogs { get; set; }
        #endregion
        
        
        #region  Messeage
        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<MessageReply> MessageReplies { get; set; }
        public virtual DbSet<MessageDocument> MessageDocuments { get; set; }
        public virtual DbSet<MessageReplyDocument> MessageReplyDocuments { get; set; }
        public virtual DbSet<MessageStatus> MessageStatuses { get; set; }
        #endregion
     
        
        #region  Damage
        public virtual DbSet<DamageRequest> DamageRequest { get; set; }
        public virtual DbSet<DamageRequestReply>  DamageRequestReply { get; set; }
        public virtual DbSet<DamageRequestDocument> DamageRequestDocument { get; set; }
        public virtual DbSet<DamageRequestReplyDocument> DamageRequestReplyDocument { get; set; }
        public virtual DbSet<DamageRequestStatus> DamageRequestStatus { get; set; }
        #endregion
        
        public virtual DbSet<TerminalWageReport> TerminalWageReport { get; set; }
        public virtual DbSet<NewTerminalWageReport> NewTerminalWageReport { get; set; }

        public virtual DbSet<WageTransaction> WageTransaction { get; set; }
        
        public virtual DbSet<TransactionSum> TransactionSums { get; set; }
        public virtual DbSet<StatusSituation> StatusSituations { get; set; }
        public virtual DbSet<Holiday> Holidays { get; set; }
        public virtual DbSet<TerminalEm> TerminalEms { get; set; }
        public virtual DbSet<TerminalPm> TerminalPms { get; set; }
        public virtual DbSet<CompanyMonthlyTask> CompanyMonthlyTasks { get; set; }
        public virtual DbSet<UserActivityLog> UserActivityLogs { get; set; }
        public virtual DbSet<TerminalNote> TerminalNotes { get; set; }
      
        public virtual DbSet<Invoice> Invoices { get; set; }
        public virtual DbSet<InvoiceType> InvoiceTypes { get; set; }
        public virtual DbSet<BlockDocumentStatus> BlockDocumentStatuses { get; set; }
        public virtual DbSet<CustomerNumberBlackList> CustomerNumberBlackLists { get; set; }
        public virtual DbSet<IrankishRequest> IrankishRequest  { get; set; }
        public virtual DbSet<PardakhtNovinRequest> PardakhtNovinRequests { get; set; }

        public virtual DbSet<ParsianRequest> ParsianRequests { get; set; }
        public virtual DbSet<ParsianRequest2> ParsianRequests2 { get; set; }
        public virtual DbSet<ParsianRequestForInfo> ParsianRequestForInfo { get; set; }
        public virtual DbSet<TempReport1And2Data> TempReport1And2Datas { get; set; }
        public virtual DbSet<TempReport3Data> TempReport3Datas { get; set; }
        public virtual DbSet<TempReport4Data> TempReport4Datas { get; set; }
        public virtual DbSet<TempReport5Data> TempReport5Datas { get; set; }
        public virtual DbSet<TempReport6Data> TempReport6Datas { get; set; }
        public virtual DbSet<TempReport7Data> TempReport7Datas { get; set; }
        public virtual DbSet<TempReport8Data> TempReport8Datas { get; set; }

        #region  AssingRules

        
        public virtual DbSet<PspBranchRate> PspBranchRate { get; set; }
        public virtual DbSet<RuleType> RuleType { get; set; }
        public virtual DbSet<RuleDefinition> RuleDefinition { get; set; }
        public virtual DbSet<RuleOrder> RuleOrder { get; set; }
        public virtual DbSet<RulePspWeight> RulePspWeight { get; set; }

        #endregion

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<UserClaim>().ToTable("UserClaim");
            modelBuilder.Entity<UserRole>().ToTable("UserRole");
            modelBuilder.Entity<UserLogin>().ToTable("UserLogin");
            modelBuilder.Entity<Role>().ToTable("Role");
            modelBuilder.Entity<User>().ToTable("User");

            modelBuilder.Entity<Invoice>()
                .Property(x => x.CoefficientReward)
                .HasPrecision(5, 4);

            modelBuilder.Entity<BlockDocumentStatus>()
                .HasMany(e => e.Terminals)
                .WithOptional(e => e.BlockDocumentStatus)
                .HasForeignKey(e => e.BlockDocumentStatusId)
                .WillCascadeOnDelete(false);

            
         
            
            modelBuilder.Entity<City>()
                .HasMany(e => e.Terminals)
                .WithRequired(e => e.City)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Nationality>()
                .HasMany(e => e.MerchantProfiles)
                .WithRequired(e => e.Nationality)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<OrganizationUnit>()
                .HasMany(e => e.ChangeAccountRequests)
                .WithRequired(e => e.Branch)
                .HasForeignKey(e => e.BranchId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<OrganizationUnit>()
                .HasMany(e => e.Children)
                .WithOptional(e => e.Parent)
                .HasForeignKey(e => e.ParentId);

            modelBuilder.Entity<OrganizationUnit>()
                .HasMany(e => e.CustomerStatusResults)
                .WithRequired(e => e.Branch)
                .HasForeignKey(e => e.BranchId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<OrganizationUnit>()
                .HasMany(e => e.Terminals)
                .WithRequired(e => e.Branch)
                .HasForeignKey(e => e.BranchId)
                
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<State>()
                .Property(e => e.Code)
                .IsUnicode(false);

            modelBuilder.Entity<State>()
                .HasMany(e => e.Cities)
                .WithRequired(e => e.State)
                .WillCascadeOnDelete(false);

            
            modelBuilder.Entity<State>()
                .HasMany(e => e.RegionalMunicipalities)
                .WithRequired(e => e.State)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.ChangeAccountRequests)
                .WithRequired(e => e.User)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.UserActivityLogs)
                .WithOptional(e => e.User)
                .HasForeignKey(e => e.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.MerchantProfiles)
                .WithRequired(e => e.User)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.RevokeRequests)
                .WithRequired(e => e.User)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.Terminals)
                .WithRequired(e => e.User)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ActivityType>()
                .HasMany(e => e.Terminals)
                .WithRequired(e => e.ActivityType)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ChangeAccountRequest>()
                .Property(e => e.AccountNo)
                .IsUnicode(false);

            modelBuilder.Entity<ChangeAccountRequest>()
                .Property(e => e.ShebaNo)
                .IsUnicode(false);

            modelBuilder.Entity<DeviceType>()
                .Property(e => e.Code)
                .IsUnicode(false);

            modelBuilder.Entity<DocumentType>()
                .HasMany(e => e.MerchantProfileDocuments)
                .WithRequired(e => e.DocumentType)
                .WillCascadeOnDelete(false);

            
         
            
            modelBuilder.Entity<DocumentType>()
                .HasMany(e => e.TerminalDocuments)
                .WithRequired(e => e.DocumentType)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity< Guild>()
                .HasMany(e => e.Children)
                .WithOptional(e => e.Parent)
                .HasForeignKey(e => e.ParentId);

            modelBuilder.Entity<MessageReply>()
                .HasMany(x => x.MessageReplyDocuments)
                .WithRequired(x => x.MessageReply)
                .HasForeignKey(x => x.MessageReplyId);

            modelBuilder.Entity<Guild>()
                .HasMany(e => e.Terminals)
                .WithRequired(e => e.Guild)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Marketer>()
                .HasMany(e => e.Terminals)
                .WithRequired(e => e.Marketer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MerchantProfile>()
                .Property(e => e.NationalCode)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<MerchantProfile>()
                .Property(e => e.HomeTel)
                .IsUnicode(false);

            modelBuilder.Entity<MerchantProfile>()
                .Property(e => e.Mobile)
                .IsUnicode(false);

            modelBuilder.Entity<MerchantProfile>()
                .Property(e => e.HomePostCode)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<MerchantProfile>()
                .Property(e => e.LegalNationalCode)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<MerchantProfile>()
                .HasMany(e => e.MerchantProfileDocuments)
                .WithRequired(e => e.MerchantProfile)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MerchantProfile>()
                .HasMany(e => e.Terminals)
                .WithRequired(e => e.MerchantProfile)
                .WillCascadeOnDelete(false);
            
            
         

            modelBuilder.Entity<RemovedMerchantProfile>()
                .HasMany(e => e.Terminals)
                .WithRequired(e => e.RemovedMerchantProfile)
                .WillCascadeOnDelete(false);
            
            

            modelBuilder.Entity<Psp>()
                .HasMany(e => e.PspAgents)
                .WithRequired(e => e.Psp)
                .WillCascadeOnDelete(false);

            
           
            modelBuilder.Entity<_damage>()
                .HasKey(b=>b.terminalId);

           
            modelBuilder.Entity<_tempSarmayeh>()
                .HasKey(b=>b.TerminalNo);

            modelBuilder.Entity<Terminal>()
                .HasOptional(e => e.PreferredPsp)
                .WithMany(e => e.PreferredByTerminals)
                .HasForeignKey(e => e.PreferredPspId);

           

            modelBuilder.Entity<Terminal>()
                .HasOptional(e => e.Psp)
                .WithMany(e => e.Terminals)
                .HasForeignKey(e => e.PspId);

            modelBuilder.Entity<RequestStatus>()
                .HasMany(e => e.ChangeAccountRequests)
                .WithRequired(e => e.Status)
                .HasForeignKey(e => e.StatusId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RequestStatus>()
                .HasMany(e => e.RevokeRequests)
                .WithRequired(e => e.Status)
                .HasForeignKey(e => e.StatusId)
                .WillCascadeOnDelete(false);

          
 

          

            modelBuilder.Entity<Terminal>()
                .Property(e => e.TelCode)
                .IsUnicode(false);

            modelBuilder.Entity<Terminal>()
                .Property(e => e.Tel)
                .IsUnicode(false);

            modelBuilder.Entity<Terminal>()
                .Property(e => e.PostCode)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Terminal>()
                .HasMany(e => e.TerminalDocuments)
                .WithRequired(e => e.Terminal)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TerminalStatus>()
                .HasMany(e => e.Terminals)
                .WithRequired(e => e.Status)
                .HasForeignKey(e => e.StatusId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CardRequestReply>()
                .HasMany(x => x.MessageReplyDocuments)
                .WithRequired(x => x.CardRequestReply)
                .HasForeignKey(x => x.MessageReplyId);
             
            modelBuilder.Entity<CardRequest>()
                .HasRequired(x => x.User)
                .WithMany(x => x.SentCardRequestMessages)
                .HasForeignKey(x => x.UserId)
                .WillCascadeOnDelete(false);
 
            modelBuilder.Entity<CardRequest>()
                .HasRequired<CardType>( x=>x.CardType )
                .WithMany(g => g.CardRequests)
                .HasForeignKey<int>(s => s.CardTypeId);   
            
            modelBuilder.Entity<CardRequest>()
                .HasRequired<OrganizationUnit>( x=>x.OrganizationUnit )
                .WithMany(g => g.CardRequests)
                .HasForeignKey<long>(s => s.OrganizationUnitId); 
            
            modelBuilder.Entity<CardRequest>()
                .HasRequired<CardServiceType>( x=>x.CardServiceType )
                .WithMany(g => g.CardRequests)
                .HasForeignKey<int>(s => s.CardServiceTypeId);   
            
            modelBuilder.Entity<Message>()
                .HasRequired(x => x.MessageSubject)
                .WithMany(x => x.SentMessages)
                .HasForeignKey(x => x.MessageSubjectId)
                .WillCascadeOnDelete(false);


            modelBuilder.Entity<RemovedTerminal>().HasRequired(x=>x.RemovedMerchantProfile)
                .WithMany(b=>b.Terminals)
                .HasForeignKey(x=>x.RemovedMerchantProfileId).WillCascadeOnDelete(false);
            
            modelBuilder.Entity<Message>()
                .HasRequired(x => x.User)
                .WithMany(x => x.SentMessages)
                .HasForeignKey(x => x.UserId)
                .WillCascadeOnDelete(false);
         
            modelBuilder.Entity<Message>()
                .HasRequired(x => x.MessageStatus)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.StatusId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CardRequest>()
                .HasRequired(x => x.CardRequestStatus)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.StatusId)
                .WillCascadeOnDelete(false);
            
            modelBuilder.Entity<Message>()
                .HasOptional(x => x.ReviewerUser)
                .WithMany(x => x.ReviewingMessages)
                .HasForeignKey(x => x.ReviewerUserId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<CardRequest>()
                .HasOptional(x => x.ReviewerUser)
                .WithMany(x => x.ReviewingCardRequestMessages)
                .HasForeignKey(x => x.ReviewerUserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Message>()
                .HasMany(x => x.Replies)
                .WithRequired(x => x.Message)
                .HasForeignKey(x => x.MessageId);
            modelBuilder.Entity<CardRequest>()
                .HasMany(x => x.Replies)
                .WithRequired(x => x.CardRequest)
                .HasForeignKey(x => x.MessageId);
            modelBuilder.Entity<Message>()
                .HasMany(x => x.MessageDocuments)
                .WithRequired(x => x.Message)
                .HasForeignKey(x => x.MessageId);
            modelBuilder.Entity<CardRequest>()
                .HasMany(x => x.MessageDocuments)
                .WithRequired(x => x.CardRequest)
                .HasForeignKey(x => x.MessageId);
            modelBuilder.Entity<OrganizationUnit>()
                .HasOptional(x => x.City)
                .WithMany(x => x.OrganizationUnits)
                .HasForeignKey(x => x.CityId);

            modelBuilder.Entity<TerminalNote>()
                .HasRequired(x => x.Terminal)
                .WithMany(x => x.TerminalNotes)
                .HasForeignKey(x => x.TerminalId);

            // modelBuilder.Entity<TempReport1And2Data>()
            //     .HasRequired(x => x.Status)
            //     .WithMany(x => x.TempReport1And2Datas)
            //     .HasForeignKey(x => x.StatusId);
        }

        public async Task<List<long>> GetIdListFromSequence(int rangeSize, CancellationToken cancellationToken)
        {
            using (var sqlConnection = new SqlConnection(Database.Connection.ConnectionString))
            {
                var sqlCommand = new SqlCommand
                {
                    Connection = sqlConnection,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "sys.sp_sequence_get_range"
                };

                sqlCommand.Parameters.AddWithValue("@sequence_name", "[psp].[MerchantProfile_Sequence]");
                sqlCommand.Parameters.AddWithValue("@range_size", rangeSize);

                var firstValueInRange = new SqlParameter("@range_first_value", SqlDbType.Variant) { Direction = ParameterDirection.Output };
                sqlCommand.Parameters.Add(firstValueInRange);

                await sqlConnection.OpenAsync(cancellationToken);
                await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
                var from = (long)firstValueInRange.Value;
                var to = from + rangeSize;

                var result = new List<long>();
                for (var i = from; i <= to; i++)
                    result.Add(i);

                return result;
            }
        }
    }
    
    public static class LinqExtensions 
    {
        private static PropertyInfo GetPropertyInfo(Type objType, string name)
        {
            var properties = objType.GetProperties();
            var matchedProperty = properties.FirstOrDefault (p => p.Name == name);
            if (matchedProperty == null)
                throw new ArgumentException("name");

            return matchedProperty;
        }
        private static LambdaExpression GetOrderExpression(Type objType, PropertyInfo pi)
        {
            var paramExpr = Expression.Parameter(objType);
            var propAccess = Expression.PropertyOrField(paramExpr, pi.Name);
            var expr = Expression.Lambda(propAccess, paramExpr);
            return expr;
        }
        
        public static IEnumerable<T> OrderByDescending<T>(this IEnumerable<T> query, string name)
        {
            var propInfo = GetPropertyInfo(typeof(T), name);
            var expr = GetOrderExpression(typeof(T), propInfo);

            var method = typeof(Enumerable).GetMethods().FirstOrDefault(m => m.Name == "OrderByDescending" && m.GetParameters().Length == 2);
            var genericMethod = method.MakeGenericMethod(typeof(T), propInfo.PropertyType);     
            return (IEnumerable<T>) genericMethod.Invoke(null, new object[] { query, expr.Compile() });
        }
        public static IQueryable<T> OrderByDescending<T>(this IQueryable<T> query, string name)
        {
            var propInfo = GetPropertyInfo(typeof(T), name);
            var expr = GetOrderExpression(typeof(T), propInfo);

            var method = typeof(Queryable).GetMethods().FirstOrDefault(m => m.Name == "OrderByDescending" && m.GetParameters().Length == 2);
            var genericMethod = method.MakeGenericMethod(typeof(T), propInfo.PropertyType);     
            return (IQueryable<T>) genericMethod.Invoke(null, new object[] { query, expr });
            
        }
        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> query, string name)
        {
            var propInfo = GetPropertyInfo(typeof(T), name);
            var expr = GetOrderExpression(typeof(T), propInfo);

            var method = typeof(Enumerable).GetMethods().FirstOrDefault(m => m.Name == "OrderBy" && m.GetParameters().Length == 2);
            var genericMethod = method.MakeGenericMethod(typeof(T), propInfo.PropertyType);     
            return (IEnumerable<T>) genericMethod.Invoke(null, new object[] { query, expr.Compile() });
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> query, string name)
        {
            var propInfo = GetPropertyInfo(typeof(T), name);
            var expr = GetOrderExpression(typeof(T), propInfo);

            var method = typeof(Queryable).GetMethods().FirstOrDefault(m => m.Name == "OrderBy" && m.GetParameters().Length == 2);
            var genericMethod = method.MakeGenericMethod(typeof(T), propInfo.PropertyType);     
            return (IQueryable<T>) genericMethod.Invoke(null, new object[] { query, expr });
        }
    }
}