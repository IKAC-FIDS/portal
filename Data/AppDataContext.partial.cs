using Dapper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TES.Common.Extensions;
using TES.Data.DataModel;
using TES.Data.SearchParameter;
using Enums = TES.Common.Enumerations;

namespace TES.Data
{
    public partial class AppDataContext
    {
        public static AppDataContext Create()
        {
            return new AppDataContext();
        }

        public async Task<List<BranchRankingData>> GetBranchRankingDataByOrganizationUnit(int year, int month, long? branchId, bool isSupervisionUser, bool isBranchUser, bool isTehranBranchManagement, bool isCountyBranchManagement, CancellationToken cancellationToken)
        {
            var branchFilter = "";
            if (branchId.HasValue)
            {
                if (isBranchUser)
                    branchFilter = $" AND b.Id = {branchId}";

                if (isSupervisionUser)
                    branchFilter = $" AND b.ParentId = {branchId}";
            }

            var daysInMonth = DateTimeExtensions.DaysInMonth(year, month);
            var fromRevokeDate = $"{year}/{month}/1".ToMiladiDate().AbsoluteStart();
            var toInstallationDate = $"{year}/{month}/{daysInMonth}".ToMiladiDate().AbsoluteEnd();

            var sqlCmd = $@"select b.Id,
	                   b.Title, 
	                   sum(iif(isnull(t.BuyTransactionAmount, 0) >= 20000000 OR isnull(t.BuyTransactionCount, 0) >= 60, 1, 0)) as HighTransactionCount,  -- پر تراکنش
	                   sum(iif(isnull(t.BuyTransactionAmount, 0) > 9999 and isnull(t.BuyTransactionAmount, 0) < 20000000 AND isnull(t.BuyTransactionCount, 0) < 60, 1, 0)) as LowTransactionCount, -- کم تراکنش
                       sum(iif(isnull(t.BuyTransactionAmount, 0) >= 0 and isnull(t.BuyTransactionAmount, 0) <= 9999, 1, 0)) as WithoutTransactionCount, -- فاقد تراکنش
	                   sum(isnull(t.BuyTransactionAmount, 0)) as TotalTransactionSum,
	                   sum(isnull(t.BuyTransactionCount, 0)) as TotalTransactionCount,
	                   sum(iif(dt.IsWireless= 1, 1, 0)) as WirelessTerminalCount,
	                   sum(iif(dt.IsWireless= 0, 1, 0)) as WithWireTerminalCount
                from psp.Terminal tr 
                left join psp.[TransactionSum] t on tr.TerminalNo = t.TerminalNo and t.PersianLocalYear = {year} and t.PersianLocalMonth = {month}     
                join psp.DeviceType dt on dt.Id = tr.DeviceTypeId
                join dbo.OrganizationUnit b on b.id = tr.BranchId
                where (tr.InstallationDate is not null and tr.InstallationDate <= '{toInstallationDate:yyyy-MM-dd HH:mm:ss.fff}') and (tr.RevokeDate is null or tr.RevokeDate >= '{fromRevokeDate:yyyy-MM-dd HH:mm:ss.fff}')  {branchFilter}";

            if (isTehranBranchManagement)
                sqlCmd += $" and b.CityId = {(long)Enums.City.Tehran} ";

            if (isCountyBranchManagement)
                sqlCmd += $" and b.CityId != {(long)Enums.City.Tehran} ";

            sqlCmd += "group by b.Id, b.Title;";

            return await Database.SqlQuery<BranchRankingData>(sqlCmd).ToListAsync(cancellationToken);
        }

        public async Task<List<BranchRankingData>> GetBranchRankingDataByBranchManagment(int year, int month, long? branchId, bool isSupervisionUser, bool isBranchUser, bool isTehranBranchManagement, bool isCountyBranchManagement, CancellationToken cancellationToken)
        {
            var branchFilter = "";
            if (branchId.HasValue)
            {
                if (isBranchUser)
                    branchFilter = $" AND b.Id = {branchId}";

                if (isSupervisionUser)
                    branchFilter = $" AND b.ParentId = {branchId}";
            }

            var daysInMonth = DateTimeExtensions.DaysInMonth(year, month);
            var fromRevokeDate = $"{year}/{month}/1".ToMiladiDate().AbsoluteStart();
            var toInstallationDate = $"{year}/{month}/{daysInMonth}".ToMiladiDate().AbsoluteEnd();

            var sqlCmd = $@"select p.Id,
	                   p.Title, 
	                   sum(iif(isnull(t.BuyTransactionAmount, 0) >= 20000000 OR isnull(t.BuyTransactionCount, 0) >= 60, 1, 0)) as HighTransactionCount,  -- پر تراکنش
	                   sum(iif(isnull(t.BuyTransactionAmount, 0) > 9999 and isnull(t.BuyTransactionAmount, 0) < 20000000 AND isnull(t.BuyTransactionCount, 0) < 60, 1, 0)) as LowTransactionCount, -- کم تراکنش
                       sum(iif(isnull(t.BuyTransactionAmount, 0) >= 0 and isnull(t.BuyTransactionAmount, 0) <= 9999, 1, 0)) as WithoutTransactionCount, -- فاقد تراکنش
	                   sum(isnull(t.BuyTransactionAmount, 0)) as TotalTransactionSum,
	                   sum(isnull(t.BuyTransactionCount, 0)) as TotalTransactionCount,
	                   sum(iif(dt.IsWireless= 1, 1, 0)) as WirelessTerminalCount,
	                   sum(iif(dt.IsWireless= 0, 1, 0)) as WithWireTerminalCount
                from psp.Terminal tr 
                left join psp.[TransactionSum] t on tr.TerminalNo = t.TerminalNo and t.PersianLocalYear = {year} and t.PersianLocalMonth = {month}     
                join psp.DeviceType dt on dt.Id = tr.DeviceTypeId
                join dbo.OrganizationUnit b on b.id = tr.BranchId
                join dbo.OrganizationUnit p on p.id = b.ParentId
                where (tr.InstallationDate is not null and tr.InstallationDate <= '{toInstallationDate:yyyy-MM-dd HH:mm:ss.fff}') and (tr.RevokeDate is null or tr.RevokeDate >= '{fromRevokeDate:yyyy-MM-dd HH:mm:ss.fff}')  {branchFilter}";

            if (isTehranBranchManagement)
                sqlCmd += $" and b.CityId = {(long)Enums.City.Tehran} ";

            if (isCountyBranchManagement)
                sqlCmd += $" and b.CityId != {(long)Enums.City.Tehran} ";

            sqlCmd += "group by p.Id, p.Title;";

            return await Database.SqlQuery<BranchRankingData>(sqlCmd).ToListAsync(cancellationToken);
        }

        public async Task<List<BranchRankingData>> GetBranchRankingDataByCity(int year, int month, long? branchId, bool isSupervisionUser, bool isBranchUser, bool isTehranBranchManagement, bool isCountyBranchManagement, CancellationToken cancellationToken)
        {
            var branchFilter = "";
            if (branchId.HasValue)
            {
                if (isBranchUser)
                    branchFilter = $" AND b.Id = {branchId}";

                if (isSupervisionUser)
                    branchFilter = $" AND b.ParentId = {branchId}";
            }

            var daysInMonth = DateTimeExtensions.DaysInMonth(year, month);
            var fromRevokeDate = $"{year}/{month}/1".ToMiladiDate().AbsoluteStart();
            var toInstallationDate = $"{year}/{month}/{daysInMonth}".ToMiladiDate().AbsoluteEnd();

            var sqlCmd = $@"select c.Id as Id,
	                   c.Title as Title, 
	                   sum(iif(isnull(t.BuyTransactionAmount, 0) >= 20000000 OR isnull(t.BuyTransactionCount, 0) >= 60, 1, 0)) as HighTransactionCount,  -- پر تراکنش
	                   sum(iif(isnull(t.BuyTransactionAmount, 0) > 9999 and isnull(t.BuyTransactionAmount, 0) < 20000000 AND isnull(t.BuyTransactionCount, 0) < 60, 1, 0)) as LowTransactionCount, -- کم تراکنش
                       sum(iif(isnull(t.BuyTransactionAmount, 0) >= 0 and isnull(t.BuyTransactionAmount, 0) <= 9999, 1, 0)) as WithoutTransactionCount, -- فاقد تراکنش
	                   sum(isnull(t.BuyTransactionAmount, 0)) as TotalTransactionSum,
	                   sum(isnull(t.BuyTransactionCount, 0)) as TotalTransactionCount,
	                   sum(iif(dt.IsWireless= 1, 1, 0)) as WirelessTerminalCount,
	                   sum(iif(dt.IsWireless= 0, 1, 0)) as WithWireTerminalCount
                from psp.Terminal tr 
                left join psp.[TransactionSum] t on tr.TerminalNo = t.TerminalNo and t.PersianLocalYear = {year} and t.PersianLocalMonth = {month}     
                join psp.DeviceType dt on dt.Id = tr.DeviceTypeId
                join dbo.OrganizationUnit b on b.id = tr.BranchId
                join dbo.City c on c.Id = b.CityId
               where (tr.InstallationDate is not null and tr.InstallationDate <= '{toInstallationDate:yyyy-MM-dd HH:mm:ss.fff}') and (tr.RevokeDate is null or tr.RevokeDate >= '{fromRevokeDate:yyyy-MM-dd HH:mm:ss.fff}')  {branchFilter}";

            if (isTehranBranchManagement)
                sqlCmd += $" and b.CityId = {(long)Enums.City.Tehran} ";

            if (isCountyBranchManagement)
                sqlCmd += $" and b.CityId != {(long)Enums.City.Tehran} ";

            sqlCmd += "group by c.Id, c.Title;";

            return await Database.SqlQuery<BranchRankingData>(sqlCmd).ToListAsync(cancellationToken);
        }

        public async Task<List<BranchRankingDataByState>> GetBranchRankingDataByState(int year, int month, long? branchId, bool isSupervisionUser, bool isBranchUser, bool isTehranBranchManagement, bool isCountyBranchManagement, CancellationToken cancellationToken)
        {
            var branchFilter = "";
            if (branchId.HasValue)
            {
                if (isBranchUser)
                    branchFilter = $" AND b.Id = {branchId}";

                if (isSupervisionUser)
                    branchFilter = $" AND b.ParentId = {branchId}";
            }

            var daysInMonth = DateTimeExtensions.DaysInMonth(year, month);
            var fromRevokeDate = $"{year}/{month}/1".ToMiladiDate().AbsoluteStart();
            var toInstallationDate = $"{year}/{month}/{daysInMonth}".ToMiladiDate().AbsoluteEnd();

            var sqlCmd = $@"select s.Id as Id,
	                   s.Title as Title, 
	                   sum(iif(isnull(t.BuyTransactionAmount, 0) >= 20000000 OR isnull(t.BuyTransactionCount, 0) >= 60, 1, 0)) as HighTransactionCount,  -- پر تراکنش
	                   sum(iif(isnull(t.BuyTransactionAmount, 0) > 9999 and isnull(t.BuyTransactionAmount, 0) < 20000000 AND isnull(t.BuyTransactionCount, 0) < 60, 1, 0)) as LowTransactionCount, -- کم تراکنش
                       sum(iif(isnull(t.BuyTransactionAmount, 0) >= 0 and isnull(t.BuyTransactionAmount, 0) <= 9999, 1, 0)) as WithoutTransactionCount, -- فاقد تراکنش
	                   sum(isnull(t.BuyTransactionAmount, 0)) as TotalTransactionSum,
	                   sum(isnull(t.BuyTransactionCount, 0)) as TotalTransactionCount,
	                   sum(iif(dt.IsWireless= 1, 1, 0)) as WirelessTerminalCount,
	                   sum(iif(dt.IsWireless= 0, 1, 0)) as WithWireTerminalCount
                from psp.Terminal tr  
                left join psp.[TransactionSum] t on tr.TerminalNo = t.TerminalNo and t.PersianLocalYear = {year} and t.PersianLocalMonth = {month}                 
                join psp.DeviceType dt on dt.Id = tr.DeviceTypeId
                join dbo.OrganizationUnit b on b.id = tr.BranchId
                join dbo.City c on c.Id = b.CityId
                join dbo.State s on s.Id = c.StateId
                where (tr.InstallationDate is not null and tr.InstallationDate <= '{toInstallationDate:yyyy-MM-dd HH:mm:ss.fff}') and (tr.RevokeDate is null or tr.RevokeDate >= '{fromRevokeDate:yyyy-MM-dd HH:mm:ss.fff}')  {branchFilter}";

            if (isTehranBranchManagement)
                sqlCmd += $" and b.CityId = {(long)Enums.City.Tehran} ";

            if (isCountyBranchManagement)
                sqlCmd += $" and b.CityId != {(long)Enums.City.Tehran} ";

            sqlCmd += "group by s.Id, s.Title;";

            return await Database.SqlQuery<BranchRankingDataByState>(sqlCmd).ToListAsync(cancellationToken);
        }

        public async Task<List<TerminalExportData>> GetTerminalExportData(TerminalSearchParameters searchParams)
        {
            var queryBuilder = new StringBuilder();

            queryBuilder.AppendLine("SELECT ");
            queryBuilder.AppendLine("t.Id AS TerminalId,");
            queryBuilder.AppendLine("t.MerchantProfileId,");
            queryBuilder.AppendLine("t.ShebaNo,");
            queryBuilder.AppendLine("t.StatusId,");
            queryBuilder.AppendLine("t.MerchantNo,");
            queryBuilder.AppendLine("t.TerminalNo,");
            queryBuilder.AppendLine("t.SubmitTime,");
            queryBuilder.AppendLine("t.Title AS TerminalTitle,");
            queryBuilder.AppendLine("t.AccountNo,");
            
            queryBuilder.AppendLine("t.IsGood,");
            queryBuilder.AppendLine("t.IsGoodValue,");
            queryBuilder.AppendLine("t.IsGoodMonth,");
            queryBuilder.AppendLine("t.IsGoodYear,");
            queryBuilder.AppendLine("t.IsActive,");
            queryBuilder.AppendLine("t.LowTransaction,");
            queryBuilder.AppendLine("case when  IIF(trn.Price > 20000000 OR trn.TotalCount > 60, 1, 0) = 1 then 1 else  0 end as 'IsActived',");

          
            queryBuilder.AppendLine(
                "case when   DATEDIFF(day,  t.BatchDate, GETDATE())  > 5 and (t.StatusId = 5  or t.StatusId = 6 ) and (t.InstallationDate is null)    " +
                "then DATEDIFF(day,  t.BatchDate, GETDATE()) - 5  else '-' end as 'InstallDelayDays' ,");
            
            queryBuilder.AppendLine("t.InstallationDate,");
            queryBuilder.AppendLine("t.RevokeDate,");
            queryBuilder.AppendLine("t.ErrorComment,");
            queryBuilder.AppendLine("t.TaxPayerCode,");
            queryBuilder.AppendLine("parentBranch.Title AS ParentBranchTitle,");
            queryBuilder.AppendLine("t.Address,");
            queryBuilder.AppendLine("t.TelCode,");
            queryBuilder.AppendLine("t.Tel,");
            queryBuilder.AppendLine("t.PostCode,");
            queryBuilder.AppendLine("t.BlockDocumentDate,");
            queryBuilder.AppendLine("t.BlockDocumentNumber,");
            queryBuilder.AppendLine("t.BlockAccountNumber,");
            queryBuilder.AppendLine("t.BlockPrice,");
            //queryBuilder.AppendLine("t.IsVip,");
            queryBuilder.AppendLine("g.Title AS GuildTitle,");
            queryBuilder.AppendLine("pg.Title AS ParentGuildTitle,");
            queryBuilder.AppendLine("mr.Title AS MarketerTitle,");
            queryBuilder.AppendLine("t.BatchDate,");
            queryBuilder.AppendLine("t.ContractNo,");
            queryBuilder.AppendLine("c.Title AS CityTitle,");
            queryBuilder.AppendLine("t.BranchId,");
            queryBuilder.AppendLine("branch.Title AS BranchTitle,");
            queryBuilder.AppendLine("mp.IsLegalPersonality,");
            queryBuilder.AppendLine("mp.EnglishFatherName,");
            
            queryBuilder.AppendLine("mp.HomeAddress,");
            queryBuilder.AppendLine("mp.HomePostCode,");
            
            
            queryBuilder.AppendLine("mp.EnglishFirstName,");
            queryBuilder.AppendLine("mp.EnglishLastName,");
            queryBuilder.AppendLine("mp.NationalCode,");
            queryBuilder.AppendLine("mp.FirstName,");
            queryBuilder.AppendLine("mp.LastName,");
            queryBuilder.AppendLine("mp.Mobile,");
            queryBuilder.AppendLine("mp.Birthdate,");
            queryBuilder.AppendLine("mp.IdentityNumber,");
            queryBuilder.AppendLine("mp.FatherName,");
            queryBuilder.AppendLine("mp.LegalNationalCode,");
            queryBuilder.AppendLine("mp.CompanyRegistrationDate,");
            queryBuilder.AppendLine("mp.CompanyRegistrationNumber,");
            queryBuilder.AppendLine("mp.IsMale,");
            queryBuilder.AppendLine("t.MarketerId,");
            queryBuilder.AppendLine("s.Title AS StateTitle,");
            queryBuilder.AppendLine("p.Title AS PspTitle,");
            queryBuilder.AppendLine("u.FullName AS SubmitterUserFullName,");
            queryBuilder.AppendLine("ts.Title AS StatusTitle,");
            queryBuilder.AppendLine("dt.Title AS DeviceTypeTitle,");
            queryBuilder.AppendLine("ISNULL(trn.Price, 0) AS SumOfTransactions,");
            queryBuilder.AppendLine("ISNULL(trn.TotalCount, 0) AS TransactionCount,");
            //
            queryBuilder.AppendLine($@"
                CASE
                WHEN InstallationDate IS NULL  {(searchParams.FromTransactionDate.HasValue ? $"OR (dbo.PersianDatePart(t.InstallationDate, 'YEAR') >= {searchParams.FromTransactionDate.GetPersianYear()} AND dbo.PersianDatePart(t.InstallationDate, 'MONTH') > {searchParams.FromTransactionDate.GetPersianMonth()})" : "")} THEN NULL
                WHEN (ISNULL(trn.Price, 0) >= 20000000 OR ISNULL(trn.TotalCount, 0) >= 60) THEN N'پر تراکنش'
                WHEN (ISNULL(trn.Price, 0) > 9999 AND ISNULL(trn.Price, 0) < 20000000 AND ISNULL(trn.TotalCount, 0) < 60) THEN N'کم تراکنش'
                WHEN (ISNULL(trn.Price, 0) >= 0 AND ISNULL(trn.Price, 0) <= 9999) THEN N'فاقد تراکنش'
                ELSE NULL END AS TransactionStatusText,");
            
      
            queryBuilder.AppendLine("IIF(trn.Price > 20000000 OR trn.TotalCount > 60, 1, 0) AS IsActive");
            queryBuilder.AppendLine("  FROM psp.Terminal t");
            queryBuilder.AppendLine("LEFT JOIN (SELECT tr.TerminalNo, SUM(tr.BuyTransactionAmount) AS Price, Sum(tr.BuyTransactionCount) AS TotalCount ");
            queryBuilder.AppendLine("			 FROM psp.[TransactionSum] tr ");

            var transWhereClause = new List<string>();

            if (searchParams.FromTransactionDate.HasValue)
            {
                transWhereClause.Add($"tr.PersianLocalYear >= {searchParams.FromTransactionDate.GetPersianYear()} AND tr.PersianLocalMonth >= {searchParams.FromTransactionDate.GetPersianMonth()}");
            }

            if (searchParams.ToTransactionDate.HasValue)
            {
                transWhereClause.Add($"tr.PersianLocalYear <= {searchParams.ToTransactionDate.GetPersianYear()} AND tr.PersianLocalMonth <= {searchParams.ToTransactionDate.GetPersianMonth()}");
            }

            if (transWhereClause.Any())
            {
                queryBuilder.AppendLine(" WHERE ");
                queryBuilder.AppendLine(string.Join(" AND ", transWhereClause));
            }

            queryBuilder.AppendLine("			GROUP BY tr.TerminalNo) trn ON trn.TerminalNo = t.TerminalNo");

            queryBuilder.AppendLine("JOIN psp.MerchantProfile mp on mp.Id = t.MerchantProfileId");
            queryBuilder.AppendLine("JOIN dbo.City c ON c.Id = t.CityId");
            queryBuilder.AppendLine("JOIN dbo.State s ON s.Id = c.StateId");
            queryBuilder.AppendLine("LEFT JOIN psp.TerminalStatus ts ON ts.Id = t.StatusId");
            queryBuilder.AppendLine("JOIN psp.DeviceType dt ON dt.Id = t.DeviceTypeId");
            queryBuilder.AppendLine("LEFT JOIN psp.Psp p ON p.Id = t.PspId");
            queryBuilder.AppendLine("JOIN dbo.[User] u ON u.Id = t.UserId");
            queryBuilder.AppendLine("LEFT JOIN dbo.OrganizationUnit branch ON branch.Id = t.BranchId");
            queryBuilder.AppendLine("LEFT JOIN dbo.OrganizationUnit parentBranch ON parentBranch.Id = branch.ParentId");
            queryBuilder.AppendLine("JOIN psp.Guild g ON g.Id = t.GuildId");
            queryBuilder.AppendLine("LEFT JOIN psp.Guild pg ON pg.Id = g.ParentId");
            queryBuilder.AppendLine("JOIN psp.Marketer mr ON mr.Id = t.MarketerId");

            AppendWhereClause(queryBuilder, searchParams);

            List<TerminalExportData> result;
            var cmd = (SqlCommand)Database.Connection.CreateCommand();
            cmd.CommandText = queryBuilder.ToString();

            try
            {
                await Database.Connection.OpenAsync();

                var reader = await cmd.ExecuteReaderAsync();
                result = ((IObjectContextAdapter)this).ObjectContext.Translate<TerminalExportData>(reader).ToList();
                reader.Close();
            }
            finally
            {
                Database.Connection.Close();
            }

            return result;
        }
     public async Task<List<TerminalExportData>> GetRemovedTerminalExportData(TerminalSearchParameters searchParams)
        {
            var queryBuilder = new StringBuilder();

            queryBuilder.AppendLine("SELECT ");
            queryBuilder.AppendLine("t.Id AS TerminalId,");
            queryBuilder.AppendLine("t.MerchantProfileId,");
            queryBuilder.AppendLine("t.ShebaNo,");
            queryBuilder.AppendLine("t.StatusId,");
            queryBuilder.AppendLine("t.MerchantNo,");
            queryBuilder.AppendLine("t.TerminalNo,");
            queryBuilder.AppendLine("t.SubmitTime,");
            queryBuilder.AppendLine("t.Title AS TerminalTitle,");
            queryBuilder.AppendLine("t.AccountNo,");
            queryBuilder.AppendLine("t.InstallationDate,");
            queryBuilder.AppendLine("t.RevokeDate,");
            queryBuilder.AppendLine("t.ErrorComment,");
            
                   
            queryBuilder.AppendLine("t.IsGood,");
            queryBuilder.AppendLine("t.IsGoodValue,");
            queryBuilder.AppendLine("t.IsGoodMonth,");
            queryBuilder.AppendLine("t.IsGoodYear,");
            
            queryBuilder.AppendLine("t.LowTransaction,");
            queryBuilder.AppendLine(" 0 As  IsActive,");

            
          
            queryBuilder.AppendLine(
                "case when  t.BatchDate is not null and DATEDIFF(day,  t.BatchDate, GETDATE())  > 5   and (t.StatusId = 5  or t.StatusId = 6 )  " +
                "then DATEDIFF(day,  t.BatchDate, GETDATE())  else 0 end as 'InstallDelayDays' ,");
            

            queryBuilder.AppendLine("t.TaxPayerCode,");
            queryBuilder.AppendLine("parentBranch.Title AS ParentBranchTitle,");
            queryBuilder.AppendLine("t.Address,");
            queryBuilder.AppendLine("t.TelCode,");
            queryBuilder.AppendLine("t.Tel,");
            queryBuilder.AppendLine("t.PostCode,");
            queryBuilder.AppendLine("t.BlockDocumentDate,");
            queryBuilder.AppendLine("t.BlockDocumentNumber,");
            queryBuilder.AppendLine("t.BlockAccountNumber,");
            queryBuilder.AppendLine("t.BlockPrice,"); 
            queryBuilder.AppendLine("g.Title AS GuildTitle,");
            queryBuilder.AppendLine("pg.Title AS ParentGuildTitle,");
            queryBuilder.AppendLine("mr.Title AS MarketerTitle,");
            queryBuilder.AppendLine("t.BatchDate,");
            queryBuilder.AppendLine("t.ContractNo,");
            queryBuilder.AppendLine("c.Title AS CityTitle,");
            queryBuilder.AppendLine("t.BranchId,");
            queryBuilder.AppendLine("branch.Title AS BranchTitle,");
            queryBuilder.AppendLine("mp.IsLegalPersonality,");
            queryBuilder.AppendLine("mp.EnglishFatherName,");
            
            queryBuilder.AppendLine("mp.HomeAddress,");
            queryBuilder.AppendLine("mp.HomePostCode,");
            
            
            queryBuilder.AppendLine("mp.EnglishFirstName,");
            queryBuilder.AppendLine("mp.EnglishLastName,");
            queryBuilder.AppendLine("mp.NationalCode,");
            queryBuilder.AppendLine("mp.FirstName,");
            queryBuilder.AppendLine("mp.LastName,");
            queryBuilder.AppendLine("mp.Mobile,");
            queryBuilder.AppendLine("mp.Birthdate,");
            queryBuilder.AppendLine("mp.IdentityNumber,");
            queryBuilder.AppendLine("mp.FatherName,");
            queryBuilder.AppendLine("mp.LegalNationalCode,");
            queryBuilder.AppendLine("mp.CompanyRegistrationDate,");
            queryBuilder.AppendLine("mp.CompanyRegistrationNumber,");
            queryBuilder.AppendLine("mp.IsMale,");
            queryBuilder.AppendLine("t.MarketerId,");
            queryBuilder.AppendLine("s.Title AS StateTitle,");
            queryBuilder.AppendLine("p.Title AS PspTitle,");
            queryBuilder.AppendLine("u.FullName AS SubmitterUserFullName,");
            queryBuilder.AppendLine("ts.Title AS StatusTitle,");
            queryBuilder.AppendLine("dt.Title AS DeviceTypeTitle,");
            queryBuilder.AppendLine("ISNULL(trn.Price, 0) AS SumOfTransactions,");
            queryBuilder.AppendLine("ISNULL(trn.TotalCount, 0) AS TransactionCount,");
            //
            queryBuilder.AppendLine($@"
                CASE
                WHEN InstallationDate IS NULL  {(searchParams.FromTransactionDate.HasValue ? $"OR (dbo.PersianDatePart(t.InstallationDate, 'YEAR') >= {searchParams.FromTransactionDate.GetPersianYear()} AND dbo.PersianDatePart(t.InstallationDate, 'MONTH') > {searchParams.FromTransactionDate.GetPersianMonth()})" : "")} THEN NULL
                WHEN (ISNULL(trn.Price, 0) >= 20000000 OR ISNULL(trn.TotalCount, 0) >= 60) THEN N'پر تراکنش'
                WHEN (ISNULL(trn.Price, 0) > 9999 AND ISNULL(trn.Price, 0) < 20000000 AND ISNULL(trn.TotalCount, 0) < 60) THEN N'کم تراکنش'
                WHEN (ISNULL(trn.Price, 0) >= 0 AND ISNULL(trn.Price, 0) <= 9999) THEN N'فاقد تراکنش'
                ELSE NULL END AS TransactionStatusText");
            
      
         
            queryBuilder.AppendLine("  FROM psp.RemovedTerminal t");
            queryBuilder.AppendLine("LEFT JOIN (SELECT tr.TerminalNo, SUM(tr.BuyTransactionAmount) AS Price, Sum(tr.BuyTransactionCount) AS TotalCount ");
            queryBuilder.AppendLine("			 FROM psp.[TransactionSum] tr ");

            var transWhereClause = new List<string>();

            if (searchParams.FromTransactionDate.HasValue)
            {
                transWhereClause.Add($"tr.PersianLocalYear >= {searchParams.FromTransactionDate.GetPersianYear()} AND tr.PersianLocalMonth >= {searchParams.FromTransactionDate.GetPersianMonth()}");
            }

            if (searchParams.ToTransactionDate.HasValue)
            {
                transWhereClause.Add($"tr.PersianLocalYear <= {searchParams.ToTransactionDate.GetPersianYear()} AND tr.PersianLocalMonth <= {searchParams.ToTransactionDate.GetPersianMonth()}");
            }

            if (transWhereClause.Any())
            {
                queryBuilder.AppendLine(" WHERE ");
                queryBuilder.AppendLine(string.Join(" AND ", transWhereClause));
            }

            queryBuilder.AppendLine(" GROUP BY tr.TerminalNo) trn ON trn.TerminalNo = t.TerminalNo");

            queryBuilder.AppendLine("JOIN psp.RemovedMerchantProfile mp on mp.Id = t.MerchantProfileId");
            queryBuilder.AppendLine("JOIN dbo.City c ON c.Id = t.CityId");
            queryBuilder.AppendLine("JOIN dbo.State s ON s.Id = c.StateId");
            queryBuilder.AppendLine("LEFT JOIN psp.TerminalStatus ts ON ts.Id = t.StatusId");
            queryBuilder.AppendLine("JOIN psp.DeviceType dt ON dt.Id = t.DeviceTypeId");
            queryBuilder.AppendLine("LEFT JOIN psp.Psp p ON p.Id = t.PspId");
            queryBuilder.AppendLine("JOIN dbo.[User] u ON u.Id = t.UserId");
            queryBuilder.AppendLine("LEFT JOIN dbo.OrganizationUnit branch ON branch.Id = t.BranchId");
            queryBuilder.AppendLine("LEFT JOIN dbo.OrganizationUnit parentBranch ON parentBranch.Id = branch.ParentId");
            queryBuilder.AppendLine("JOIN psp.Guild g ON g.Id = t.GuildId");
            queryBuilder.AppendLine("LEFT JOIN psp.Guild pg ON pg.Id = g.ParentId");
            queryBuilder.AppendLine("JOIN psp.Marketer mr ON mr.Id = t.MarketerId");

            AppendWhereClause(queryBuilder, searchParams);

            List<TerminalExportData> result;
            var cmd = (SqlCommand)Database.Connection.CreateCommand();
            cmd.CommandText = queryBuilder.ToString();

            try
            {
                await Database.Connection.OpenAsync();

                var reader = await cmd.ExecuteReaderAsync();
                result = ((IObjectContextAdapter)this).ObjectContext.Translate<TerminalExportData>(reader).ToList();
                reader.Close();
            }
            finally
            {
                Database.Connection.Close();
            }

            return result;
        }

        public async Task<Tuple<List<TerminalData>, int>> GetTerminalData(TerminalSearchParameters searchParams, string orderCaluse, bool retriveTotalPageCount, int pageNo, int pageSize)
        {
            var queryBuilder = new StringBuilder();

            queryBuilder.AppendLine("WITH tbl AS (");
            queryBuilder.AppendLine("SELECT ");
            queryBuilder.AppendLine("t.Id AS TerminalId,");
            queryBuilder.AppendLine("t.MerchantProfileId,");
            queryBuilder.AppendLine("t.ShebaNo,");
            queryBuilder.AppendLine("t.DeviceTypeId,");
            queryBuilder.AppendLine("c.StateId,");
            queryBuilder.AppendLine("t.PspId,");
            queryBuilder.AppendLine("t.TopiarId,");
            queryBuilder.AppendLine("t.UserId,");
            queryBuilder.AppendLine("t.StatusId,");
            queryBuilder.AppendLine("t.MerchantNo,");
            queryBuilder.AppendLine("t.TerminalNo,");
            queryBuilder.AppendLine("t.LowTransaction,");
            queryBuilder.AppendLine("t.IsActive,");
            queryBuilder.AppendLine("t.TransactionCount,");
            queryBuilder.AppendLine("t.TransactionValue,");

            
            queryBuilder.AppendLine(
                "case when   DATEDIFF(day,  t.BatchDate, GETDATE())  > 5 and (t.StatusId = 5  or t.StatusId = 6 ) and (t.InstallationDate is null)    " +
                "then DATEDIFF(day,  t.BatchDate, GETDATE()) - 5  else '-' end as 'InstallDelayDays' ,");
            queryBuilder.AppendLine("case when  IIF(trn.Price > 20000000 OR trn.TotalCount > 60, 1, 0) = 1 then 1 else  0 end as 'IsActived',");

           
                
            queryBuilder.AppendLine("t.IsGood,");
            queryBuilder.AppendLine("t.IsGoodValue,");
            queryBuilder.AppendLine("t.IsGoodMonth,");
            queryBuilder.AppendLine("t.IsGoodYear,");

            
            
            //queryBuilder.AppendLine("t.IsVip,");
            queryBuilder.AppendLine("t.Title AS TerminalTitle,");
            queryBuilder.AppendLine("mp.FirstName + ' ' + mp.LastName AS FullName,");
            queryBuilder.AppendLine("mp.NationalCode,");
            queryBuilder.AppendLine("t.AccountNo,");
            queryBuilder.AppendLine("c.Title AS CityTitle,");
            queryBuilder.AppendLine("t.MarketerId,");
            queryBuilder.AppendLine("t.InstallationDate,");
            queryBuilder.AppendLine("ISNULL(trn.Price, 0) AS SumOfTransactions,");
            queryBuilder.AppendLine("ISNULL(trn.TotalCount, 0) AS CountOfTransactions,");

            queryBuilder.AppendLine($@"
                CASE
                WHEN InstallationDate IS NULL     {(searchParams.FromTransactionDate.HasValue ? $"OR (dbo.PersianDatePart(t.InstallationDate, 'YEAR') >= {searchParams.FromTransactionDate.GetPersianYear()} AND dbo.PersianDatePart(t.InstallationDate, 'MONTH') > {searchParams.FromTransactionDate.GetPersianMonth()})" : "")} THEN NULL
                WHEN (ISNULL(trn.Price, 0) >= 20000000 OR ISNULL(trn.TotalCount, 0) >= 60) THEN CAST({Enums.TransactionStatus.HighTransaction.ToByte()} AS TINYINT)
                WHEN (ISNULL(trn.Price, 0) > 9999 AND ISNULL(trn.Price, 0) < 20000000 AND ISNULL(trn.TotalCount, 0) < 60) THEN CAST({Enums.TransactionStatus.LowTransaction.ToByte()} AS TINYINT)
                WHEN (ISNULL(trn.Price, 0) >= 0 AND ISNULL(trn.Price, 0) <= 9999) THEN CAST({Enums.TransactionStatus.WithoutTransaction.ToByte()} AS TINYINT)
                ELSE NULL END AS TransactionStatus");

          //  queryBuilder.AppendLine("IIF(trn.Price > 20000000 OR trn.TotalCount > 60, 1, 0) AS IsActive");
            queryBuilder.AppendLine("  FROM psp.Terminal t");

            queryBuilder.AppendLine("LEFT JOIN (SELECT tr.TerminalNo, SUM(tr.BuyTransactionAmount) AS Price, Sum(tr.BuyTransactionCount) AS TotalCount ");
         
            
            queryBuilder.AppendLine("			 FROM psp.[TransactionSum] tr ");

            var transWhereClause = new List<string>();

            // TODO
            if (searchParams.FromTransactionDate.HasValue)
            {
                transWhereClause.Add($"tr.PersianLocalYear >= {searchParams.FromTransactionDate.GetPersianYear()} AND tr.PersianLocalMonth >= {searchParams.FromTransactionDate.GetPersianMonth()}");
            }

          
            

            if (searchParams.ToTransactionDate.HasValue)
            {
                transWhereClause.Add($"tr.PersianLocalYear <= {searchParams.ToTransactionDate.GetPersianYear()} AND tr.PersianLocalMonth <= {searchParams.ToTransactionDate.GetPersianMonth()}");
            }

            if (transWhereClause.Any())
            {
                queryBuilder.AppendLine(" WHERE ");
                queryBuilder.AppendLine(string.Join(" AND ", transWhereClause));
            }

            queryBuilder.AppendLine("			GROUP BY tr.TerminalNo) trn ON trn.TerminalNo = t.TerminalNo");
            queryBuilder.AppendLine("JOIN psp.MerchantProfile mp on mp.Id = t.MerchantProfileId");
            queryBuilder.AppendLine("JOIN dbo.City c ON c.Id = t.CityId");

            if (searchParams.IsWireless.HasValue)
            {
                queryBuilder.AppendLine("JOIN psp.DeviceType dt ON dt.Id = t.DeviceTypeId");
            }

            if (searchParams.ParentGuildId.HasValue)
            {
                queryBuilder.AppendLine("JOIN psp.Guild g ON g.Id = t.GuildId");
            }

            if (searchParams.IsSupervisionUser || searchParams.BranchId.HasValue || searchParams.IsTehranBranchManagment || searchParams.IsCountyBranchManagment)
            {
                queryBuilder.AppendLine("JOIN dbo.OrganizationUnit branch ON branch.Id = t.BranchId");
            }

            AppendWhereClause(queryBuilder, searchParams);


            queryBuilder.AppendLine(")");

            queryBuilder.AppendLine("SELECT * FROM (");
            queryBuilder.AppendLine("SELECT tbl.*,");
            queryBuilder.AppendLine("s.Title AS StateTitle,");
            queryBuilder.AppendLine("p.Title AS PspTitle,");
            queryBuilder.AppendLine("u.FullName AS SubmitterUserFullName,");
            queryBuilder.AppendLine("ts.Title AS StatusTitle,");
            queryBuilder.AppendLine("dt.Title AS DeviceTypeTitle");
            queryBuilder.AppendLine("FROM tbl");
            queryBuilder.AppendLine("JOIN psp.DeviceType dt ON dt.Id = tbl.DeviceTypeId");
            queryBuilder.AppendLine("JOIN dbo.State s ON s.Id = tbl.StateId");
            queryBuilder.AppendLine("LEFT JOIN psp.TerminalStatus ts ON ts.Id = tbl.StatusId");
            queryBuilder.AppendLine("LEFT JOIN psp.Psp p ON p.Id = tbl.PspId");
            queryBuilder.AppendLine("JOIN dbo.[User] u ON u.Id = tbl.UserId");
            queryBuilder.AppendLine(") t");

            // if (searchParams.JustActive.HasValue)
            // {
            //     queryBuilder.AppendLine(searchParams.JustActive.Value
            //         ? " WHERE t.IsActive = 1"
            //         : " WHERE t.IsActive = 0");
            // }

            queryBuilder.AppendFormat(" ORDER BY {0} ", string.IsNullOrWhiteSpace(orderCaluse) ? "TerminalId DESC" : orderCaluse);
            queryBuilder.AppendFormat("OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", pageNo * pageSize, pageSize);
            queryBuilder.AppendLine(";");

            if (retriveTotalPageCount)
            {
                AppendTotalRowCountQuery(queryBuilder, searchParams);
            }

            List<TerminalData> result;
            var totalRowsCount = 0;
            var cmd = (SqlCommand)Database.Connection.CreateCommand();
            cmd.CommandText = queryBuilder.ToString();

            try
            {
                await Database.Connection.OpenAsync();

                var reader = await cmd.ExecuteReaderAsync();
                result = ((IObjectContextAdapter)this).ObjectContext.Translate<TerminalData>(reader).ToList();

                if (retriveTotalPageCount)
                {
                    await reader.NextResultAsync();
                    totalRowsCount = ((IObjectContextAdapter)this).ObjectContext.Translate<int>(reader).First();
                }

                reader.Close();
            }
            finally
            {
                Database.Connection.Close();
            }

            return new Tuple<List<TerminalData>, int>(result, totalRowsCount);
        }

            public async Task<Tuple<List<TerminalData>, int>> GetRemovedTerminalData(TerminalSearchParameters searchParams, string orderCaluse, bool retriveTotalPageCount, int pageNo, int pageSize)
                {
                    var queryBuilder = new StringBuilder();
        
                    queryBuilder.AppendLine("WITH tbl AS (");
                    queryBuilder.AppendLine("SELECT ");
                    queryBuilder.AppendLine("t.Id AS TerminalId,");
                    queryBuilder.AppendLine("t.MerchantProfileId,");
                    queryBuilder.AppendLine("t.ShebaNo,");
                    queryBuilder.AppendLine("t.DeviceTypeId,");
                    queryBuilder.AppendLine("c.StateId,");
                    queryBuilder.AppendLine("t.PspId,");
                    queryBuilder.AppendLine("t.TopiarId,");
                    queryBuilder.AppendLine("t.UserId,");
                    queryBuilder.AppendLine("t.StatusId,");
                    queryBuilder.AppendLine("t.MerchantNo,");
                    queryBuilder.AppendLine("t.TerminalNo,");
                    queryBuilder.AppendLine("t.LowTransaction,");
                    queryBuilder.AppendLine("t.IsActive,");

                    
                    queryBuilder.AppendLine("t.IsGood,");
                    queryBuilder.AppendLine("t.IsGoodValue,");
                    queryBuilder.AppendLine("t.IsGoodMonth,");
                    queryBuilder.AppendLine("t.IsGoodYear,");
        
                    
                    
                    //queryBuilder.AppendLine("t.IsVip,");
                    queryBuilder.AppendLine("t.Title AS TerminalTitle,");
                    queryBuilder.AppendLine("mp.FirstName + ' ' + mp.LastName AS FullName,");
                    queryBuilder.AppendLine("mp.NationalCode,");
                    queryBuilder.AppendLine("t.AccountNo,");
                    queryBuilder.AppendLine("c.Title AS CityTitle,");
                    queryBuilder.AppendLine("t.MarketerId,");
                    queryBuilder.AppendLine("t.InstallationDate,");
                    queryBuilder.AppendLine("ISNULL(trn.Price, 0) AS SumOfTransactions,");
                    queryBuilder.AppendLine("ISNULL(trn.TotalCount, 0) AS CountOfTransactions,");
        
                    queryBuilder.AppendLine($@"
                        CASE
                        WHEN InstallationDate IS NULL     {(searchParams.FromTransactionDate.HasValue ? $"OR (dbo.PersianDatePart(t.InstallationDate, 'YEAR') >= {searchParams.FromTransactionDate.GetPersianYear()} AND dbo.PersianDatePart(t.InstallationDate, 'MONTH') > {searchParams.FromTransactionDate.GetPersianMonth()})" : "")} THEN NULL
                        WHEN (ISNULL(trn.Price, 0) >= 20000000 OR ISNULL(trn.TotalCount, 0) >= 60) THEN CAST({Enums.TransactionStatus.HighTransaction.ToByte()} AS TINYINT)
                        WHEN (ISNULL(trn.Price, 0) > 9999 AND ISNULL(trn.Price, 0) < 20000000 AND ISNULL(trn.TotalCount, 0) < 60) THEN CAST({Enums.TransactionStatus.LowTransaction.ToByte()} AS TINYINT)
                        WHEN (ISNULL(trn.Price, 0) >= 0 AND ISNULL(trn.Price, 0) <= 9999) THEN CAST({Enums.TransactionStatus.WithoutTransaction.ToByte()} AS TINYINT)
                        ELSE NULL END AS TransactionStatus");
        
                  //  queryBuilder.AppendLine("IIF(trn.Price > 20000000 OR trn.TotalCount > 60, 1, 0) AS IsActive");
                    queryBuilder.AppendLine("  FROM psp.RemovedTerminal t");
        
                    queryBuilder.AppendLine("LEFT JOIN (SELECT tr.TerminalNo, SUM(tr.BuyTransactionAmount) AS Price, Sum(tr.BuyTransactionCount) AS TotalCount ");
                 
                    
                    queryBuilder.AppendLine("			 FROM psp.[TransactionSum] tr ");
        
                    var transWhereClause = new List<string>();
        
                    // TODO
                    if (searchParams.FromTransactionDate.HasValue)
                    {
                        transWhereClause.Add($"tr.PersianLocalYear >= {searchParams.FromTransactionDate.GetPersianYear()} AND tr.PersianLocalMonth >= {searchParams.FromTransactionDate.GetPersianMonth()}");
                    }
        
                  
                    
        
                    if (searchParams.ToTransactionDate.HasValue)
                    {
                        transWhereClause.Add($"tr.PersianLocalYear <= {searchParams.ToTransactionDate.GetPersianYear()} AND tr.PersianLocalMonth <= {searchParams.ToTransactionDate.GetPersianMonth()}");
                    }
        
                    if (transWhereClause.Any())
                    {
                        queryBuilder.AppendLine(" WHERE ");
                        queryBuilder.AppendLine(string.Join(" AND ", transWhereClause));
                    }
        
                    queryBuilder.AppendLine("			GROUP BY tr.TerminalNo) trn ON trn.TerminalNo = t.TerminalNo");
                    queryBuilder.AppendLine("JOIN psp.RemovedMerchantProfile mp on mp.Id = t.RemovedMerchantProfileId");
                    queryBuilder.AppendLine("JOIN dbo.City c ON c.Id = t.CityId");
        
                    if (searchParams.IsWireless.HasValue)
                    {
                        queryBuilder.AppendLine("JOIN psp.DeviceType dt ON dt.Id = t.DeviceTypeId");
                    }
        
                    if (searchParams.ParentGuildId.HasValue)
                    {
                        queryBuilder.AppendLine("JOIN psp.Guild g ON g.Id = t.GuildId");
                    }
        
                    if (searchParams.IsSupervisionUser || searchParams.BranchId.HasValue || searchParams.IsTehranBranchManagment || searchParams.IsCountyBranchManagment)
                    {
                        queryBuilder.AppendLine("JOIN dbo.OrganizationUnit branch ON branch.Id = t.BranchId");
                    }
        
                    AppendWhereClause(queryBuilder, searchParams);
        
        
                    queryBuilder.AppendLine(")");
        
                    queryBuilder.AppendLine("SELECT * FROM (");
                    queryBuilder.AppendLine("SELECT tbl.*,");
                    queryBuilder.AppendLine("s.Title AS StateTitle,");
                    queryBuilder.AppendLine("p.Title AS PspTitle,");
                    queryBuilder.AppendLine("u.FullName AS SubmitterUserFullName,");
                    queryBuilder.AppendLine("ts.Title AS StatusTitle,");
                    queryBuilder.AppendLine("dt.Title AS DeviceTypeTitle");
                    queryBuilder.AppendLine("FROM tbl");
                    queryBuilder.AppendLine("JOIN psp.DeviceType dt ON dt.Id = tbl.DeviceTypeId");
                    queryBuilder.AppendLine("JOIN dbo.State s ON s.Id = tbl.StateId");
                    queryBuilder.AppendLine("LEFT JOIN psp.TerminalStatus ts ON ts.Id = tbl.StatusId");
                    queryBuilder.AppendLine("LEFT JOIN psp.Psp p ON p.Id = tbl.PspId");
                    queryBuilder.AppendLine("JOIN dbo.[User] u ON u.Id = tbl.UserId");
                    queryBuilder.AppendLine(") t");
        
                    if (searchParams.JustActive.HasValue)
                    {
                        queryBuilder.AppendLine(searchParams.JustActive.Value
                            ? " WHERE IsActive = 1"
                            : " WHERE IsActive = 0");
                    }
        
                    queryBuilder.AppendFormat(" ORDER BY {0} ", string.IsNullOrWhiteSpace(orderCaluse) ? "TerminalId DESC" : orderCaluse);
                    queryBuilder.AppendFormat("OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", pageNo * pageSize, pageSize);
                    queryBuilder.AppendLine(";");
        
                    if (retriveTotalPageCount)
                    {
                        AppendTotalRowCountRemovedTerminalQuery(queryBuilder, searchParams);
                    }
        
                    List<TerminalData> result;
                    var totalRowsCount = 0;
                    var cmd = (SqlCommand)Database.Connection.CreateCommand();
                    cmd.CommandText = queryBuilder.ToString();
        
                    try
                    {
                        await Database.Connection.OpenAsync();
        
                        var reader = await cmd.ExecuteReaderAsync();
                        result = ((IObjectContextAdapter)this).ObjectContext.Translate<TerminalData>(reader).ToList();
        
                        if (retriveTotalPageCount)
                        {
                            await reader.NextResultAsync();
                            totalRowsCount = ((IObjectContextAdapter)this).ObjectContext.Translate<int>(reader).First();
                        }
        
                        reader.Close();
                    }
                    finally
                    {
                        Database.Connection.Close();
                    }
        
                    return new Tuple<List<TerminalData>, int>(result, totalRowsCount);
                }

        public List<ChangeAccountRequestData> GetChangeAccountRequestData(RequestSearchParameters searchParams, bool retriveTotalPageCount, int? pageNo, int? pageSize, out int totalRowsCount)
        {
            var queryBuilder = new StringBuilder();

            queryBuilder.AppendLine("SELECT");
            queryBuilder.AppendLine("request.Id AS ChangeAccountRequestId,");
            queryBuilder.AppendLine("request.TerminalNo,");
            queryBuilder.AppendLine("request.Error,");
            queryBuilder.AppendLine("request.StatusId,");
            queryBuilder.AppendLine("rs.Title AS RequestStatus,");
            queryBuilder.AppendLine("request.SubmitTime,");
            queryBuilder.AppendLine("request.ShebaNo,");
            queryBuilder.AppendLine("request.AccountNo AS RequestedAccountNo,");
            queryBuilder.AppendLine("t.AccountNo AS CurrentTerminalAccountNo,");
            queryBuilder.AppendLine("p.Title AS PspTitle,");
            queryBuilder.AppendLine("t.PspId,");
            queryBuilder.AppendLine("t.ContractNo,");
            queryBuilder.AppendLine("t.Id AS TerminalId,");
            queryBuilder.AppendLine("request.CurrentAccountNo AS OldAccountNo,");
            queryBuilder.AppendLine("request.Result,");
            queryBuilder.AppendLine("u.FullName AS SubmitterUserFullName,");
            queryBuilder.AppendLine("branch.Title AS BranchTitle,");
            queryBuilder.AppendLine("ts.Title AS TerminalStatus,");
            queryBuilder.AppendLine("dt.Title AS DeviceTypeTitle");
            queryBuilder.AppendLine("FROM psp.ChangeAccountRequest request");
            queryBuilder.AppendLine("JOIN psp.Terminal t on t.TerminalNo = request.TerminalNo");
            queryBuilder.AppendLine("JOIN psp.MerchantProfile mp on mp.Id = t.MerchantProfileId");
            queryBuilder.AppendLine("LEFT JOIN psp.RequestStatus rs ON rs.Id = request.StatusId");
            queryBuilder.AppendLine("LEFT JOIN psp.TerminalStatus ts ON ts.Id = t.StatusId");
            queryBuilder.AppendLine("LEFT JOIN psp.Psp p ON p.Id = t.PspId");
            queryBuilder.AppendLine("JOIN dbo.[User] u ON u.Id = request.UserId");
            queryBuilder.AppendLine("JOIN dbo.OrganizationUnit branch ON branch.Id = request.BranchId");
            queryBuilder.AppendLine("JOIN psp.DeviceType dt ON dt.Id = t.DeviceTypeId");

            AppendRequestWhereClause(queryBuilder, searchParams);

            queryBuilder.AppendFormat(" ORDER BY ChangeAccountRequestId DESC ");

            if (pageNo.HasValue && pageSize.HasValue)
                queryBuilder.AppendFormat("OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY; ", pageNo * pageSize, pageSize);

            if (retriveTotalPageCount)
            {
                queryBuilder.AppendLine("SELECT COUNT(request.Id) ");
                queryBuilder.AppendLine("  FROM psp.ChangeAccountRequest request LEFT JOIN psp.Terminal t ON request.TerminalNo = t.TerminalNo");

                if (searchParams.IsSupervisionUser || searchParams.IsTehranBranchManagment || searchParams.IsCountyBranchManagment)
                {
                    queryBuilder.AppendLine("JOIN dbo.OrganizationUnit branch ON branch.Id = request.BranchId");
                }

                if (searchParams.IsWireless.HasValue)
                {
                    queryBuilder.AppendLine("JOIN psp.DeviceType dt on dt.Id = t.DeviceTypeId");
                }

                if (!string.IsNullOrEmpty(searchParams.NationalCode))
                {
                    queryBuilder.AppendLine("JOIN psp.MerchantProfile mp on mp.Id = t.MerchantProfileId");
                }

                AppendRequestWhereClause(queryBuilder, searchParams);
            }

            List<ChangeAccountRequestData> result;
            totalRowsCount = 0;
            var cmd = (SqlCommand)Database.Connection.CreateCommand();
            cmd.CommandText = queryBuilder.ToString();

            try
            {
                Database.Connection.Open();

                var reader = cmd.ExecuteReader();
                result = ((IObjectContextAdapter)this).ObjectContext.Translate<ChangeAccountRequestData>(reader).ToList();

                if (retriveTotalPageCount)
                {
                    reader.NextResult();
                    totalRowsCount = ((IObjectContextAdapter)this).ObjectContext.Translate<int>(reader).First();
                }

                reader.Close();
            }
            finally
            {
                Database.Connection.Close();
            }

            return result;
        }

        public List<RevokeRequestData> GetRevokeRequestData(RequestSearchParameters searchParams, bool retriveTotalPageCount, int? pageNo, int? pageSize, out int totalRowsCount)
        {
            var queryBuilder = new StringBuilder();

            queryBuilder.AppendLine("SELECT");
            queryBuilder.AppendLine("request.Id AS RevokeRequestId,");
            queryBuilder.AppendLine("request.TerminalNo,");
            queryBuilder.AppendLine("request.StatusId,");
            queryBuilder.AppendLine("rs.Title AS RequestStatus,");
            queryBuilder.AppendLine("request.SubmitTime,");
            queryBuilder.AppendLine("t.BranchId,");
            queryBuilder.AppendLine("p.Title AS PspTitle,");
            queryBuilder.AppendLine("t.PspId,");
            queryBuilder.AppendLine("t.ContractNo,");
            queryBuilder.AppendLine("t.Id AS TerminalId,");
            queryBuilder.AppendLine("request.Result,");
            queryBuilder.AppendLine("rre1.Title AS SecondReasonTitle,");
            queryBuilder.AppendLine("u.FullName AS SubmitterUserFullName,");
            queryBuilder.AppendLine("branch.Title AS BranchTitle,");
            queryBuilder.AppendLine("ts.Title AS TerminalStatus,");
            queryBuilder.AppendLine("rre.Title AS ReasonTitle,");
            queryBuilder.AppendLine("dt.Title AS DeviceTypeTitle");
            queryBuilder.AppendLine("FROM psp.RevokeRequest request");
            queryBuilder.AppendLine("JOIN psp.RevokeReason rre on rre.Id = request.ReasonId");
            queryBuilder.AppendLine("JOIN psp.Terminal t on t.TerminalNo = request.TerminalNo");
            queryBuilder.AppendLine("LEFT JOIN psp.RevokeReason rre1 on rre1.Id = request.SecondReasonId");
            queryBuilder.AppendLine("LEFT JOIN psp.RequestStatus rs ON rs.Id = request.StatusId");
            queryBuilder.AppendLine("LEFT JOIN psp.TerminalStatus ts ON ts.Id = t.StatusId");
            queryBuilder.AppendLine("LEFT JOIN psp.Psp p ON p.Id = t.PspId");
            queryBuilder.AppendLine("JOIN dbo.[User] u ON u.Id = request.UserId");
            queryBuilder.AppendLine("JOIN dbo.OrganizationUnit branch ON branch.Id = t.BranchId");
            queryBuilder.AppendLine("JOIN psp.DeviceType dt ON dt.Id = t.DeviceTypeId");

            AppendRequestWhereClause(queryBuilder, searchParams);

            queryBuilder.AppendFormat(" ORDER BY RevokeRequestId DESC ");

            if (pageNo.HasValue && pageSize.HasValue)
                queryBuilder.AppendFormat("OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY; ", pageNo * pageSize, pageSize);

            if (retriveTotalPageCount)
            {
                queryBuilder.AppendLine("SELECT COUNT(request.Id) ");
                queryBuilder.AppendLine("  FROM psp.RevokeRequest request LEFT JOIN psp.Terminal t ON request.TerminalNo = t.TerminalNo");

                if (searchParams.IsSupervisionUser || searchParams.IsTehranBranchManagment || searchParams.IsCountyBranchManagment)
                {
                    queryBuilder.AppendLine("JOIN dbo.OrganizationUnit branch ON branch.Id = t.BranchId");
                }

                if (searchParams.IsWireless.HasValue)
                {
                    queryBuilder.AppendLine("JOIN psp.DeviceType dt on dt.Id = t.DeviceTypeId");
                }

                if (!string.IsNullOrEmpty(searchParams.NationalCode))
                {
                    queryBuilder.AppendLine("JOIN psp.MerchantProfile mp on mp.Id = t.MerchantProfileId");
                }

                AppendRequestWhereClause(queryBuilder, searchParams);
            }

            List<RevokeRequestData> result;
            totalRowsCount = 0;
            var cmd = (SqlCommand)Database.Connection.CreateCommand();
            cmd.CommandText = queryBuilder.ToString();

            try
            {
                Database.Connection.Open();

                var reader = cmd.ExecuteReader();
                result = ((IObjectContextAdapter)this).ObjectContext.Translate<RevokeRequestData>(reader).ToList();

                if (retriveTotalPageCount)
                {
                    reader.NextResult();
                    totalRowsCount = ((IObjectContextAdapter)this).ObjectContext.Translate<int>(reader).First();
                }

                reader.Close();
            }
            finally
            {
                Database.Connection.Close();
            }

            return result;
        }

        public async Task<Tuple<List<TerminalPmData>, int>> GetTerminalPmData(byte? pspId, int year, int month, bool retriveTotalPageCount, long? branchId, bool isBranchUser, bool isSupervisionUser, bool isTehranBranchManagement, bool isCountyBranchManagement, int? pageNo, int? pageSize)
        {
            var date = $"{year}/{month - 1}/{1}".ToMiladiDate();
            var fromDate = date.AbsoluteStart();
            var toDate = date.AddMonths(2).AbsoluteEnd();

            var queryBuilder = new StringBuilder();

            var whereClause = string.Empty;
            if (branchId.HasValue)
            {
                if (isBranchUser)
                    whereClause = $" AND b.Id = {branchId}";

                if (isSupervisionUser)
                    whereClause = $" AND b.ParentId = {branchId}";
            }

            if (isTehranBranchManagement)
                whereClause += $" and b.CityId = {(long)Enums.City.Tehran} ";

            if (isCountyBranchManagement)
                whereClause += $" and b.CityId != {(long)Enums.City.Tehran} ";

            queryBuilder.AppendLine("SELECT");
            queryBuilder.AppendLine("t.TerminalNo,");
            queryBuilder.AppendLine("t.StatusId,");
            queryBuilder.AppendLine("ts.Title AS StatusTitle,");
            queryBuilder.AppendLine("t.PspId,");
            queryBuilder.AppendLine("p.Title AS PspTitle,");
            queryBuilder.AppendLine("dt.Title AS DeviceTypeTitle,");
            queryBuilder.AppendLine("dt.IsWireless,");
            queryBuilder.AppendLine("tpm.PmTime");
            queryBuilder.AppendLine("FROM psp.Terminal t");
            queryBuilder.AppendLine("JOIN OrganizationUnit b on b.Id = t.BranchId");
            queryBuilder.AppendLine(@" left join psp.TerminalPm tpm on tpm.TerminalNo = t.TerminalNo");
            queryBuilder.AppendFormat(@"                             and (tpm.PmTime >= '{0:yyyy-MM-dd}' and tpm.PmTime <= '{1:yyyy-MM-dd HH:mm:ss.fff}')", fromDate, toDate);
            queryBuilder.AppendLine("LEFT JOIN psp.TerminalStatus ts ON ts.Id = t.StatusId");
            queryBuilder.AppendLine("LEFT JOIN psp.Psp p ON p.Id = t.PspId");
            queryBuilder.AppendLine("LEFT JOIN psp.DeviceType dt ON dt.Id = t.DeviceTypeId");
            queryBuilder.AppendFormat(" WHERE t.StatusId != {0}", Enums.TerminalStatus.Deleted.ToByte()).AppendLine();
            queryBuilder.AppendLine(whereClause);
            queryBuilder.AppendFormat(@" and (t.InstallationDate is not null  and datediff(MONTH,t.InstallationDate,'{0:yyyy-MM-dd}') + 1 > 1) ", fromDate);
            queryBuilder.AppendFormat(@"  and (t.RevokeDate is null or (t.RevokeDate >= '{0:yyyy-MM-dd}' and t.RevokeDate <= '{1:yyyy-MM-dd HH:mm:ss.fff}'))", fromDate, toDate);


            if (pspId != (byte)Enums.PspCompany.All)
            {
                if (pspId.HasValue)
                {
                    queryBuilder.AppendFormat(" AND t.PspId = {0}", pspId);
                }
                else
                {
                    queryBuilder.Append(" AND t.PspId IS NULL");
                }
            }

            queryBuilder.AppendFormat(" ORDER BY t.Id DESC ");

            if (pageNo.HasValue && pageSize.HasValue)
            {
                queryBuilder.AppendFormat("OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY; ", pageNo * pageSize, pageSize);
            }

            if (retriveTotalPageCount)
            {
                queryBuilder.AppendLine("SELECT COUNT(t.Id) ");
                queryBuilder.AppendLine("  FROM psp.Terminal t ");
                queryBuilder.AppendLine(" JOIN OrganizationUnit b on b.Id = t.BranchId");
                queryBuilder.AppendFormat(" WHERE t.StatusId != {0}", Enums.TerminalStatus.Deleted.ToByte()).AppendLine();
                queryBuilder.AppendLine(whereClause);
                queryBuilder.AppendFormat(@" and (t.InstallationDate is not null  and datediff(MONTH,t.InstallationDate,'{0:yyyy-MM-dd}') + 1 > 1) ", fromDate);
                queryBuilder.AppendFormat(@"  and (t.RevokeDate is null or (t.RevokeDate >= '{0:yyyy-MM-dd}' and t.RevokeDate <= '{1:yyyy-MM-dd HH:mm:ss.fff}'))", fromDate, toDate);

                if (pspId != (byte)Enums.PspCompany.All)
                {
                    if (pspId.HasValue)
                    {
                        queryBuilder.AppendFormat(" AND t.PspId = {0}", pspId);
                    }
                    else
                    {
                        queryBuilder.Append(" AND t.PspId IS NULL");
                    }
                }
            }

            List<TerminalPmData> result;
            int totalRowsCount = 0;
            var cmd = (SqlCommand)Database.Connection.CreateCommand();
            cmd.CommandText = queryBuilder.ToString();

            try
            {
                await Database.Connection.OpenAsync();

                var reader = await cmd.ExecuteReaderAsync();
                result = ((IObjectContextAdapter)this).ObjectContext.Translate<TerminalPmData>(reader).ToList();

                if (retriveTotalPageCount)
                {
                    await reader.NextResultAsync();
                    totalRowsCount = ((IObjectContextAdapter)this).ObjectContext.Translate<int>(reader).First();
                }

                reader.Close();
            }
            finally
            {
                Database.Connection.Close();
            }

            return new Tuple<List<TerminalPmData>, int>(result, totalRowsCount);
        }

        public async Task<Tuple<List<TerminalEmData>, int>> GetTerminalEmData(byte? pspId, int year, int month, bool retriveTotalPageCount, long? branchId, bool isBranchUser, bool isSupervisionUser, bool isTehranBranchManagement, bool isCountyBranchManagement, int? pageNo, int? pageSize)
        {
            var date = $"{year}/{month}/{1}".ToMiladiDate();
            var daysInMonth = DateTimeExtensions.DaysInMonth(year, month);
            var fromDate = date.AbsoluteStart();
            var toDate = date.AddDays(daysInMonth - 1).AbsoluteEnd();

            var queryBuilder = new StringBuilder();

            var whereClause = string.Empty;
            if (branchId.HasValue)
            {
                if (isBranchUser)
                    whereClause = $" AND b.Id = {branchId}";

                if (isSupervisionUser)
                    whereClause = $" AND b.ParentId = {branchId}";
            }

            if (isTehranBranchManagement)
                whereClause += $" and b.CityId = {(long)Enums.City.Tehran} ";

            if (isCountyBranchManagement)
                whereClause += $" and b.CityId != {(long)Enums.City.Tehran} ";

            queryBuilder.AppendLine("with tbl as (");
            queryBuilder.AppendLine("SELECT");
            queryBuilder.AppendLine("t.TerminalNo,");
            queryBuilder.AppendLine("t.StatusId,");
            queryBuilder.AppendLine("ts.Title AS StatusTitle,");
            queryBuilder.AppendLine("t.PspId,");
            queryBuilder.AppendLine("te.RequestEmTime,");
            queryBuilder.AppendLine("te.EmTime,");
            queryBuilder.AppendLine("(select count(*) from Holiday h where h.Date between te.RequestEmTime and te.EmTime) as HolidayCount,");
            queryBuilder.AppendLine("datediff(DAY, te.RequestEmTime,  te.EmTime) - 1 as DelayDays,");
            queryBuilder.AppendLine("p.Title AS PspTitle,");
            queryBuilder.AppendLine("dt.Title AS DeviceTypeTitle,");
            queryBuilder.AppendLine("dt.IsWireless");
            queryBuilder.AppendLine("FROM psp.Terminal t");
            queryBuilder.AppendLine("JOIN psp.TerminalEm te on te.TerminalNo = t.TerminalNo");
            queryBuilder.AppendLine("JOIN OrganizationUnit b on b.Id = t.BranchId");
            queryBuilder.AppendLine("LEFT JOIN psp.TerminalStatus ts ON ts.Id = t.StatusId");
            queryBuilder.AppendLine("LEFT JOIN psp.Psp p ON p.Id = t.PspId");
            queryBuilder.AppendLine("LEFT JOIN psp.DeviceType dt ON dt.Id = t.DeviceTypeId");

            queryBuilder.AppendFormat(" WHERE t.StatusId != {0}", Enums.TerminalStatus.Deleted.ToByte()).AppendLine();
            queryBuilder.AppendLine(whereClause);
            queryBuilder.AppendFormat(" AND te.RequestEmTime BETWEEN '{0:yyyy-MM-dd}' AND '{1:yyyy-MM-dd HH:mm:ss.fff}'", fromDate, toDate);
            queryBuilder.AppendFormat("and (t.RevokeDate is null or (t.RevokeDate >= '{0:yyyy-MM-dd}' and t.RevokeDate <= '{1:yyyy-MM-dd HH:mm:ss.fff}'))", fromDate, toDate);

            if (pspId != (byte)Enums.PspCompany.All)
            {
                if (pspId.HasValue)
                {
                    queryBuilder.AppendFormat(" AND t.PspId = {0}", pspId);
                }
                else
                {
                    queryBuilder.Append(" AND t.PspId IS NULL");
                }
            }

            queryBuilder.AppendLine(")");

            queryBuilder.AppendLine("select * from tbl ");

            queryBuilder.AppendFormat(" ORDER BY tbl.TerminalNo DESC ");

            if (pageNo.HasValue && pageSize.HasValue)
            {
                queryBuilder.AppendFormat("OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY; ", pageNo * pageSize, pageSize);
            }

            if (retriveTotalPageCount)
            {
                queryBuilder.AppendLine("with tbl as (");
                queryBuilder.AppendLine("SELECT");
                queryBuilder.AppendLine("t.TerminalNo,");
                queryBuilder.AppendLine("(select count(*) from Holiday h where h.Date between te.RequestEmTime and te.EmTime) as HolidayCount,");
                queryBuilder.AppendLine("datediff(DAY, te.RequestEmTime,  te.EmTime) + 1 as DelayDays");
                queryBuilder.AppendLine("FROM psp.Terminal t");
                queryBuilder.AppendLine("JOIN OrganizationUnit b on b.Id = t.BranchId");
                queryBuilder.AppendLine("JOIN psp.TerminalEm te on te.TerminalNo = t.TerminalNo");
                queryBuilder.AppendFormat(" WHERE t.StatusId != {0}", Enums.TerminalStatus.Deleted.ToByte()).AppendLine();
                queryBuilder.AppendLine(whereClause);
                queryBuilder.AppendFormat(" AND te.RequestEmTime BETWEEN '{0:yyyy-MM-dd}' AND '{1:yyyy-MM-dd HH:mm:ss.fff}'", fromDate, toDate);
                queryBuilder.AppendFormat("and (t.RevokeDate is null or (t.RevokeDate >= '{0:yyyy-MM-dd}' and t.RevokeDate <= '{1:yyyy-MM-dd HH:mm:ss.fff}'))", fromDate, toDate);

                if (pspId != (byte)Enums.PspCompany.All)
                {
                    if (pspId.HasValue)
                    {
                        queryBuilder.AppendFormat(" AND t.PspId = {0}", pspId);
                    }
                    else
                    {
                        queryBuilder.Append(" AND t.PspId IS NULL");
                    }
                }

                queryBuilder.AppendLine(")");

                queryBuilder.AppendLine("select COUNT(tbl.TerminalNo) from tbl where iif((tbl.DelayDays - tbl.HolidayCount) < 0,0,(tbl.DelayDays - tbl.HolidayCount)) > 1 ");
            }

            List<TerminalEmData> result;
            int totalRowsCount = 0;
            var cmd = (SqlCommand)Database.Connection.CreateCommand();
            cmd.CommandText = queryBuilder.ToString();

            try
            {
                await Database.Connection.OpenAsync();

                var reader = await cmd.ExecuteReaderAsync();
                result = ((IObjectContextAdapter)this).ObjectContext.Translate<TerminalEmData>(reader).ToList();

                if (retriveTotalPageCount)
                {
                    await reader.NextResultAsync();
                    totalRowsCount = ((IObjectContextAdapter)this).ObjectContext.Translate<int>(reader).First();
                }

                reader.Close();
            }
            finally
            {
                Database.Connection.Close();
            }

            return new Tuple<List<TerminalEmData>, int>(result, totalRowsCount);
        }

        public async Task<List<InstallationDelayData>> GetInstallationDelayData(DateTime fromDate, DateTime toDate, int? delay, byte? pspId, long? branchId, bool? justInstalledTerminals, bool isSupervisionUser, bool isBranchUser, bool isTehranBranchManagement, bool isCountyBranchManagement)
        {
            fromDate = fromDate.AbsoluteStart();
            toDate = toDate.AbsoluteEnd();
            var startOfCurrentYear = $"1396/1/1".ToMiladiDate();

            List<byte> statusList;

            if (!justInstalledTerminals.HasValue)
            {
                statusList = new List<byte>
                {
                    Enums.TerminalStatus.ReadyForAllocation.ToByte(),
                    Enums.TerminalStatus.Allocated.ToByte(),
                    Enums.TerminalStatus.Test.ToByte(),
                    Enums.TerminalStatus.Installed.ToByte()
                };
            }
            else if (justInstalledTerminals.Value)
                statusList = new List<byte> { Enums.TerminalStatus.Installed.ToByte() };
            else
            {
                statusList = new List<byte>
                {
                    Enums.TerminalStatus.ReadyForAllocation.ToByte(),
                    Enums.TerminalStatus.Allocated.ToByte(),
                    Enums.TerminalStatus.Test.ToByte()
                };
            }

            var whereClause = string.Empty;
            if (branchId.HasValue)
            {
                if (isBranchUser)
                    whereClause = $" AND b.Id = {branchId}";

                if (isSupervisionUser)
                    whereClause = $" AND b.ParentId = {branchId}";
            }

            if (isTehranBranchManagement)
                whereClause += $" and b.CityId = {(long)Enums.City.Tehran} ";

            if (isCountyBranchManagement)
                whereClause += $" and b.CityId != {(long)Enums.City.Tehran} ";

            var sqlCommandText = $@"with tbl_main as (
                select t.TerminalNo,                        
                       dbo.F_AddDayExceptHolidays(t.BatchDate, {delay ?? 0}) as MustInstallationDate,
                       dateadd(day, {delay ?? 0}, t.BatchDate) as MustInstallationDateWithoutHoliday,
                       t.BatchDate,
                       t.StatusId,
                       t.DeviceTypeId,
                       t.PspId,
                       t.BranchId,
                       t.InstallationDate,
                       isnull(t.InstallationDate, '{toDate:yyyy-MM-dd HH:mm:ss.fff}') as CalcInstallationDate 
                  from psp.terminal t
                 where t.BatchDate is not null
                   and t.SubmitTime >= '{startOfCurrentYear:yyyy-MM-dd}'
                   and t.BatchDate >= '{startOfCurrentYear:yyyy-MM-dd}')

                ,tbl as (
                select t.InstallationDate, t.TerminalNo, t.StatusId, p.title as PspTitle,s.title as StatusTitle,t.BatchDate, t.PspId,t.BranchId,dt.IsWireless, dt.Title as DeviceTypeTitle, b.Title as BranchTitle,
					   CAST(iif(t.MustInstallationDate < '{fromDate:yyyy-MM-dd}', '{fromDate:yyyy-MM-dd}', t.MustInstallationDate) as datetime) as StartDate, 
                       CAST(iif(t.MustInstallationDateWithoutHoliday < '{fromDate:yyyy-MM-dd}', '{fromDate:yyyy-MM-dd}', t.MustInstallationDateWithoutHoliday) as datetime) as StartDateWithoutHoliday, 
	                   CAST(iif(t.CalcInstallationDate > '{toDate:yyyy-MM-dd HH:mm:ss.fff}', '{toDate:yyyy-MM-dd HH:mm:ss.fff}', t.CalcInstallationDate) as datetime) as CalcInstallationDate
                  from tbl_main t
                  join psp.DeviceType dt on dt.Id = t.DeviceTypeId
				  join psp.Psp p on t.PspId = p.Id
				  join psp.TerminalStatus s on s.id = t.StatusId
				  join OrganizationUnit b on b.Id = t.BranchId
                 where t.StatusId in ({String.Join(",", statusList)})    
                   {whereClause}
                   and t.CalcInstallationDate > t.MustInstallationDateWithoutHoliday
                   {(pspId != (byte)Enums.PspCompany.All ? (pspId.HasValue ? $"and t.PspId = {pspId}" : "AND t.PspId IS NULL") : "")}
                   and ((t.BatchDate between '{fromDate:yyyy-MM-dd}' and '{toDate:yyyy-MM-dd HH:mm:ss.fff}') or
                        (t.CalcInstallationDate between '{fromDate:yyyy-MM-dd}' and '{toDate:yyyy-MM-dd HH:mm:ss.fff}') or
                        (t.BatchDate <= '{fromDate:yyyy-MM-dd}' and t.CalcInstallationDate >= '{toDate:yyyy-MM-dd HH:mm:ss.fff}'))                   
                    )

                ,tbl2 as (
                select tbl.*,
                       datediff(DAY, tbl.StartDateWithoutHoliday,  tbl.CalcInstallationDate) - 1 as DelayCountWithoutHoliday,
	                   datediff(DAY, tbl.StartDate,  tbl.CalcInstallationDate) - 1 - (select count(*) from Holiday h where h.Date between tbl.StartDate and tbl.CalcInstallationDate) as DelayCount
                 from tbl)

                select tbl2.*, dbo.ToPersianDateTime(tbl2.StartDate,'yyyy-MM-dd') as StartDate, dbo.ToPersianDateTime(tbl2.CalcInstallationDate,'yyyy-MM-dd') as CalcInstallationDate

                  from tbl2 
				  where DelayCountWithoutHoliday >= {delay ?? 0}
                    ORDER BY tbl2.StartDate";

            List<InstallationDelayData> result;
            var cmd = (SqlCommand)Database.Connection.CreateCommand();
            cmd.CommandText = sqlCommandText;

            try
            {
                Database.Connection.Open();

                var reader = await cmd.ExecuteReaderAsync();
                result = ((IObjectContextAdapter)this).ObjectContext.Translate<InstallationDelayData>(reader).ToList();
                reader.Close();
            }
            finally
            {
                Database.Connection.Close();
            }

            return result;
        }

        public async Task<List<long>> GetChildOrganizationUnits(long parentBranchId)
        {
            return (await Database.Connection.QueryAsync<long>("SELECT * FROM dbo.GetChildOrganizationUnits(@ParentBranchId)", new { parentBranchId })).ToList();
        }

        private void AppendRequestWhereClause(StringBuilder queryBuilder, RequestSearchParameters searchParams)
        {
            // درخواست تغییر حساب هایی که برای پذیرنده های حذف شده ثبت شدند را نمی آوریم
            queryBuilder.AppendFormat(" WHERE t.StatusId != {0}", Enums.TerminalStatus.Deleted.ToByte()).AppendLine();

            if (searchParams.IsTehranBranchManagment) // اگر نقش کاربر اداره امور شعب تهران بود فقط شعبه های تهران را ببیند
            {
                queryBuilder.AppendFormat(" AND branch.CityId = {0}", (long)Enums.City.Tehran);
            }

            if (searchParams.IsCountyBranchManagment) // اگر نقش کاربر اداره امور شعب شهرستان بود تمامی شعب غیر از تهران را ببیند
            {
                queryBuilder.AppendFormat(" AND branch.CityId != {0}", (long)Enums.City.Tehran);
            }

            if (searchParams.IsBranchUser) // اگر نقش کاربر شعبه بود فقط اطلاعات خودش را ببیند
            {
                queryBuilder.AppendFormat(" AND t.BranchId = {0}", searchParams.CurrentUserBranchId);
            }

            if (searchParams.IsSupervisionUser) // اگر نقش کاربر سرپرستی بود اطلاعات خودش و زیر مجموعه هایش را ببیند
            {
                queryBuilder.AppendFormat(" AND t.BranchId IN (select * from  dbo.GetChildOrganizationUnits({0})) ", searchParams.CurrentUserBranchId);
            }

            if (searchParams.BranchId.HasValue)
                queryBuilder.AppendFormat(" AND t.BranchId = {0}", searchParams.BranchId);

            if (searchParams.IsWireless.HasValue)
                queryBuilder.AppendFormat(" AND dt.IsWireless = {0}", Convert.ToInt32(searchParams.IsWireless));

            if (searchParams.FromRequestDate.HasValue)
                queryBuilder.AppendFormat(" AND CONVERT(DATE, request.SubmitTime) >= '{0:yyyy-MM-dd}'", searchParams.FromRequestDate.Value);

            if (searchParams.ToRequestDate.HasValue)
                queryBuilder.AppendFormat(" AND CONVERT(DATE, request.SubmitTime) <= '{0:yyyy-MM-dd}'", searchParams.ToRequestDate.Value);

            if (searchParams.RequestStatusId.HasValue)
                queryBuilder.AppendFormat(" AND request.StatusId = {0}", searchParams.RequestStatusId);

            if (searchParams.PspId != (byte)Enums.PspCompany.All)
            {
                if (searchParams.PspId.HasValue)
                    queryBuilder.AppendFormat(" AND t.PspId = {0}", searchParams.PspId);
                else
                    queryBuilder.Append(" AND t.PspId IS NULL");
            }

            if (!string.IsNullOrEmpty(searchParams.TerminalNo))
                queryBuilder.AppendFormat(" AND request.TerminalNo = '{0}'", searchParams.TerminalNo);

            if (!string.IsNullOrEmpty(searchParams.NationalCode))
                queryBuilder.AppendFormat(" AND mp.NationalCode = '{0}'", searchParams.NationalCode);

            if (!string.IsNullOrEmpty(searchParams.CustomerNumber))
                queryBuilder.AppendFormat(" AND t.AccountNo LIKE '%-{0}-%'", searchParams.CustomerNumber);
        }

        private void AppendWhereClause(StringBuilder queryBuilder, TerminalSearchParameters searchParams)
        {
            var andList = new List<string>();
            if (searchParams.IsTehranBranchManagment || searchParams.IsCountyBranchManagment)
            {
                // اگر نقش کاربر اداره امور شعب تهران بود فقط شعبه های تهران را ببیند
                if (searchParams.IsTehranBranchManagment)
                    andList.Add($"branch.CityId = {(long)Enums.City.Tehran}");

                // اگر نقش کاربر اداره امور شعب شهرستان بود تمامی شعب غیر از تهران را ببیند
                if (searchParams.IsCountyBranchManagment)
                    andList.Add($"branch.CityId != {(long)Enums.City.Tehran}");
            }
            else
            {
                if (searchParams.IsBranchUser) // اگر نقش کاربر شعبه بود فقط اطلاعات خودش را ببیند
                    andList.Add($"t.BranchId = {searchParams.CurrentUserBranchId}");

                if (searchParams.IsSupervisionUser) // اگر نقش کاربر سرپرستی بود اطلاعات خودش و زیر مجموعه هایش را ببیند
                    andList.Add($"t.BranchId IN (select * from  dbo.GetChildOrganizationUnits({searchParams.CurrentUserBranchId}))");
            }

            if (searchParams.BranchId.HasValue)
                andList.Add($"t.BranchId IN (select * from  dbo.GetChildOrganizationUnits({searchParams.BranchId.Value})) ");

            if (searchParams.TerminalId.HasValue)
                andList.Add($"t.Id = {searchParams.TerminalId}");

            //if (searchParams.JustVip)
            //    andList.Add("t.IsVip = 1");

            if (searchParams.PspId != (byte)Enums.PspCompany.All)
                andList.Add(searchParams.PspId.HasValue ? $"t.PspId = {searchParams.PspId}" : "t.PspId IS NULL");

            if (!string.IsNullOrEmpty(searchParams.TerminalNo))
                andList.Add($"t.TerminalNo = '{searchParams.TerminalNo}'");

            if (!string.IsNullOrEmpty(searchParams.MerchantNo))
                andList.Add($"t.MerchantNo = '{searchParams.MerchantNo}'");

            if (searchParams.MarketerId.HasValue)
                andList.Add($"t.MarketerId = {searchParams.MarketerId}");

            if (searchParams.DeviceTypeId.HasValue)
                andList.Add($"t.DeviceTypeId = {searchParams.DeviceTypeId}");

            if (!string.IsNullOrEmpty(searchParams.Title))
                andList.Add($"Title = '{searchParams.Title}'");

            if (searchParams.IsLegalPersonality.HasValue)
                andList.Add($"mp.IsLegalPersonality = {(searchParams.IsLegalPersonality.Value == true? "1" : "0")}");

            if (!string.IsNullOrEmpty(searchParams.NationalCode))
                andList.Add($"mp.NationalCode LIKE '%{searchParams.NationalCode}%'");

            if (searchParams.IsMale.HasValue)
                andList.Add($"mp.IsMale = {searchParams.IsMale}");

            if (!string.IsNullOrEmpty(searchParams.Mobile))
                andList.Add($"mp.Mobile = '{searchParams.Mobile}'");

            if (!string.IsNullOrEmpty(searchParams.CustomerNumber))
                andList.Add($"t.AccountNo LIKE '%-{searchParams.CustomerNumber.PadLeft(8, '0')}-%'");

            if (!string.IsNullOrEmpty(searchParams.FullName))
                andList.Add($"(mp.FirstName + ' ' + mp.LastName LIKE N'%{searchParams.FullName}%')");

            if (searchParams.ParentGuildId.HasValue)
                andList.Add($"g.ParentId = {searchParams.ParentGuildId}");

            if (searchParams.StateId.HasValue)
                andList.Add($"c.StateId = {searchParams.StateId}");

            if (searchParams.CityId.HasValue)
                andList.Add($"t.CityId = {searchParams.CityId}");

            if (!string.IsNullOrEmpty(searchParams.AccountNo))
                andList.Add($"t.AccountNo = '{searchParams.AccountNo}'");

            if (searchParams.FromSubmitTime.HasValue)
                andList.Add($"CONVERT(DATE, t.SubmitTime) >= '{searchParams.FromSubmitTime.Value:yyyy-MM-dd}'");

            if (searchParams.ToSubmitTime.HasValue)
                andList.Add($"CONVERT(DATE, t.SubmitTime) <= '{searchParams.ToSubmitTime.Value:yyyy-MM-dd}'");

            if (searchParams.FromInstallationDate.HasValue)
                andList.Add($"CONVERT(DATE, t.InstallationDate) >= '{searchParams.FromInstallationDate.Value:yyyy-MM-dd}'");

            if (searchParams.ToInstallationDate.HasValue)
                andList.Add($"CONVERT(DATE, t.InstallationDate) <= '{searchParams.ToInstallationDate.Value:yyyy-MM-dd}'");

            if (searchParams.FromBatchDate.HasValue)
                andList.Add($"CONVERT(DATE, t.BatchDate) >= '{searchParams.FromBatchDate.Value:yyyy-MM-dd}'");

            if (searchParams.ToBatchDate.HasValue)
                andList.Add($"CONVERT(DATE, t.BatchDate) <= '{searchParams.ToBatchDate.Value:yyyy-MM-dd}'");

            if (searchParams.FromRevokeDate.HasValue)
                andList.Add($"CONVERT(DATE, t.RevokeDate) >= '{searchParams.FromRevokeDate.Value:yyyy-MM-dd}'");

            if (searchParams.ToRevokeDate.HasValue)
                andList.Add($"CONVERT(DATE, t.RevokeDate) <= '{searchParams.ToRevokeDate.Value:yyyy-MM-dd}'");

            if (searchParams.FromTransactionCount.HasValue)
                andList.Add($"trn.TotalCount >= {searchParams.FromTransactionCount}");

            if (searchParams.ToTransactionCount.HasValue)
                andList.Add($"trn.TotalCount <= {searchParams.ToTransactionCount}");

            if (searchParams.FromTransactionPrice.HasValue)
                andList.Add($"trn.Price >= {searchParams.FromTransactionPrice}");

            if (searchParams.ToTransactionPrice.HasValue)
                andList.Add($"trn.Price <= {searchParams.ToTransactionPrice}");

            if (!string.IsNullOrEmpty(searchParams.TaxPayerCode))
                andList.Add($"t.TaxPayerCode = '{searchParams.TaxPayerCode}'");

            
            if (searchParams.TransactionStatusList != null && searchParams.TransactionStatusList.Any())
            {
                 var orList = new List<string>();
                 if (searchParams.TransactionStatusList.Contains(Enums.TransactionStatus.LowTransaction))
                 {
                     orList.Add(" t.LowTransaction = 1");
                 }
                   if (searchParams.TransactionStatusList.Contains(Enums.TransactionStatus.HighTransaction))
                 {
                     orList.Add(" t.LowTransaction = 0");
                 }
                   if (searchParams.TransactionStatusList.Contains(Enums.TransactionStatus.All))
                   {
                       orList.Add(" t.LowTransaction = 0 or t.LowTransaction = 1 or t.LowTransaction is null");
                   }
                   if (orList.Any())
                   {
                       if (andList.Any())

                       {
                           andList.Add($@"  ( {string.Join(" OR ", orList)} )  ");
                       }
                       else
                       {
                           andList.Add($@"   ( {string.Join(" OR ", orList)} )  ");
                       }
                   }
              
            }
            if (searchParams.TerminalTransactionStatusList != null && searchParams.TerminalTransactionStatusList.Any())
            {
                var orList = new List<string>();
                if (searchParams.TerminalTransactionStatusList.Contains(Enums.TransactionStatus.LowTransaction))
                {
                    orList.Add(" t.IsGood = 0");
                }
                 if (searchParams.TerminalTransactionStatusList.Contains(Enums.TransactionStatus.HighTransaction))
                {
                    orList.Add(" t.IsGood = 1");
                }
                
                 if (orList.Any())
                {
                    if (andList.Any())

                    {
                      
                        andList.Add($@"   ( {string.Join(" OR ", orList)} )  ");
                    }
                    else
                    {
                        andList.Add($@"   ( {string.Join(" OR ", orList)} )  ");
                    }
                 }
            }
            
            
            if (searchParams.StatusIdList.Any())
            {
                var ta = searchParams.StatusIdList.Where(b => b != 30 && b!= 31).ToList();
                if(ta.Any())
                    andList.Add($"t.StatusId IN ({string.Join(",",  ta)})");
            }
            if (searchParams.StatusIdList.Contains(30))
            {
                andList.Add($" t.MarketerId != 2 and   (t.DeviceTypeId = 7  or t.DeviceTypeId = 8 or t.DeviceTypeId = 3)  and     (t.BlockDocumentStatusId  =  2 or t.BlockDocumentStatusId  is null ) and t.StatusId != 9 and t.StatusId != 18  and t.StatusId != 16 and t.TerminalNo not in (select TerminalNo from psp.RevokeRequest)    ");
   
            }
             if (searchParams.StatusIdList.Contains(31))
            {
                andList.Add($"( t.StatusId  = 5  or t.StatusId  = 6) and t.BatchDate is not null and DATEDIFF(day,  t.BatchDate, GETDATE())  > 5  and t.InstallationDate  is null       ");
   
            }
            if (searchParams.IsWireless.HasValue)
                andList.Add($"dt.IsWireless = {Convert.ToInt32(searchParams.IsWireless)}");

            if (searchParams.RevokeRequest && searchParams.ChangeAccountRequest)
            {
                andList.Add("(EXISTS (SELECT 1 FROM [psp].[RevokeRequest] re WHERE re.TerminalNo = t.TerminalNo) OR EXISTS (SELECT 1 FROM psp.ChangeAccountRequest car WHERE car.TerminalNo = t.TerminalNo))");
            }
            else
            {
                if (searchParams.ChangeAccountRequest)
                    andList.Add("EXISTS (SELECT 1 FROM psp.ChangeAccountRequest car WHERE car.TerminalNo = t.TerminalNo)");

                if (searchParams.RevokeRequest)
                    andList.Add("EXISTS (SELECT 1 FROM [psp].[RevokeRequest] re WHERE re.TerminalNo = t.TerminalNo)");
            }

            if (searchParams.JustActive.HasValue)
            {
                andList.Add(searchParams.JustActive.Value
                    ? " t.IsActive = 1 "
                    : "t.IsActive = 0");
            }
            
            
            

            if (andList.Any())
            {
                if (andList.Count > 1)
                {
                    queryBuilder.AppendLine($"WHERE {andList.First()} AND");
                    andList.RemoveAt(0);
                    queryBuilder.AppendLine(string.Join(" AND ", andList));
                }
                else if (andList.Count == 1)
                {
                    queryBuilder.AppendLine($" WHERE {andList.First()}");
                }
            }
        }

        private void AppendTotalRowCountQuery(StringBuilder queryBuilder, TerminalSearchParameters searchParams)
        {
            queryBuilder.AppendLine("SELECT COUNT(DISTINCT t.Id) ");
            queryBuilder.AppendLine("  FROM psp.Terminal t");

            if (searchParams.ParentGuildId.HasValue)
            {
                queryBuilder.AppendLine("JOIN psp.Guild g ON g.Id = t.GuildId");
            }

            if (searchParams.IsSupervisionUser || searchParams.BranchId.HasValue || searchParams.IsTehranBranchManagment || searchParams.IsCountyBranchManagment)
            {
                queryBuilder.AppendLine("JOIN dbo.OrganizationUnit branch ON branch.Id = t.BranchId");
            }

            if (searchParams.IsLegalPersonality.HasValue ||
                searchParams.IsMale.HasValue ||
                !string.IsNullOrEmpty(searchParams.NationalCode) ||
                !string.IsNullOrEmpty(searchParams.Mobile) ||
                !string.IsNullOrEmpty(searchParams.FullName))
            {
                queryBuilder.AppendLine("JOIN psp.MerchantProfile mp on mp.Id = t.MerchantProfileId");
            }

            if (searchParams.FromTransactionCount.HasValue ||
                searchParams.ToTransactionCount.HasValue ||
                searchParams.FromTransactionPrice.HasValue ||
                searchParams.ToTransactionPrice.HasValue ||
                searchParams.FromTransactionDate.HasValue ||
                searchParams.ToTransactionDate.HasValue ||
                searchParams.JustActive.HasValue ||
                searchParams.TransactionStatusList != null && searchParams.TransactionStatusList.Any())
            {
                queryBuilder.AppendLine("LEFT JOIN (SELECT tr.TerminalNo, SUM(tr.BuyTransactionAmount) AS Price, Sum(tr.BuyTransactionCount) AS TotalCount FROM psp.[TransactionSum] tr");

                var transWhereClause = new List<string>();

                if (searchParams.FromTransactionDate.HasValue)
                {
                    transWhereClause.Add($"tr.PersianLocalYear >= {searchParams.FromTransactionDate.GetPersianYear()} AND tr.PersianLocalMonth >= {searchParams.FromTransactionDate.GetPersianMonth()}");
                }

                if (searchParams.ToTransactionDate.HasValue)
                {
                    transWhereClause.Add($"tr.PersianLocalYear <= {searchParams.ToTransactionDate.GetPersianYear()} AND tr.PersianLocalMonth <= {searchParams.ToTransactionDate.GetPersianMonth()}");
                }

                if (transWhereClause.Any())
                {
                    queryBuilder.AppendLine(" WHERE ");
                    queryBuilder.AppendLine(string.Join(" AND ", transWhereClause));
                }

                queryBuilder.AppendLine("			GROUP BY tr.TerminalNo) trn ON trn.TerminalNo = t.TerminalNo");
            }

            if (searchParams.IsWireless.HasValue)
            {
                queryBuilder.AppendLine("JOIN psp.DeviceType dt on dt.Id = t.DeviceTypeId");
            }

            if (searchParams.StateId.HasValue)
            {
                queryBuilder.AppendLine("JOIN dbo.City c ON c.Id = t.CityId");
            }

            AppendWhereClause(queryBuilder, searchParams);
        }
    private void AppendTotalRowCountRemovedTerminalQuery(StringBuilder queryBuilder, TerminalSearchParameters searchParams)
        {
            queryBuilder.AppendLine("SELECT COUNT(DISTINCT t.Id) ");
            queryBuilder.AppendLine("  FROM psp.RemovedTerminal t");

            if (searchParams.ParentGuildId.HasValue)
            {
                queryBuilder.AppendLine("JOIN psp.Guild g ON g.Id = t.GuildId");
            }

            if (searchParams.IsSupervisionUser || searchParams.BranchId.HasValue || searchParams.IsTehranBranchManagment || searchParams.IsCountyBranchManagment)
            {
                queryBuilder.AppendLine("JOIN dbo.OrganizationUnit branch ON branch.Id = t.BranchId");
            }

            if (searchParams.IsLegalPersonality.HasValue ||
                searchParams.IsMale.HasValue ||
                !string.IsNullOrEmpty(searchParams.NationalCode) ||
                !string.IsNullOrEmpty(searchParams.Mobile) ||
                !string.IsNullOrEmpty(searchParams.FullName))
            {
                queryBuilder.AppendLine("JOIN psp.MerchantProfile mp on mp.Id = t.MerchantProfileId");
            }

            if (searchParams.FromTransactionCount.HasValue ||
                searchParams.ToTransactionCount.HasValue ||
                searchParams.FromTransactionPrice.HasValue ||
                searchParams.ToTransactionPrice.HasValue ||
                searchParams.FromTransactionDate.HasValue ||
                searchParams.ToTransactionDate.HasValue ||
                searchParams.JustActive.HasValue ||
                searchParams.TransactionStatusList != null && searchParams.TransactionStatusList.Any())
            {
                queryBuilder.AppendLine("LEFT JOIN (SELECT tr.TerminalNo, SUM(tr.BuyTransactionAmount) AS Price, Sum(tr.BuyTransactionCount) AS TotalCount FROM psp.[TransactionSum] tr");

                var transWhereClause = new List<string>();

                if (searchParams.FromTransactionDate.HasValue)
                {
                    transWhereClause.Add($"tr.PersianLocalYear >= {searchParams.FromTransactionDate.GetPersianYear()} AND tr.PersianLocalMonth >= {searchParams.FromTransactionDate.GetPersianMonth()}");
                }

                if (searchParams.ToTransactionDate.HasValue)
                {
                    transWhereClause.Add($"tr.PersianLocalYear <= {searchParams.ToTransactionDate.GetPersianYear()} AND tr.PersianLocalMonth <= {searchParams.ToTransactionDate.GetPersianMonth()}");
                }

                if (transWhereClause.Any())
                {
                    queryBuilder.AppendLine(" WHERE ");
                    queryBuilder.AppendLine(string.Join(" AND ", transWhereClause));
                }

                queryBuilder.AppendLine("			GROUP BY tr.TerminalNo) trn ON trn.TerminalNo = t.TerminalNo");
            }

            if (searchParams.IsWireless.HasValue)
            {
                queryBuilder.AppendLine("JOIN psp.DeviceType dt on dt.Id = t.DeviceTypeId");
            }

            if (searchParams.StateId.HasValue)
            {
                queryBuilder.AppendLine("JOIN dbo.City c ON c.Id = t.CityId");
            }

            AppendWhereClause(queryBuilder, searchParams);
        }

        public async Task<List<BranchRankingData>> LastSixMonthTransactionStatus(int fromYearMonth, int toYearMonth, long? branchId, bool isSupervisionUser, bool isBranchUser, bool isTehranBranchManagement, bool isCountyBranchManagement)
        {
            var branchFilter = "";
            if (branchId.HasValue)
            {
                if (isBranchUser)
                    branchFilter = $" AND b.Id = {branchId}";
                if (isSupervisionUser)
                    branchFilter = $" AND b.ParentId = {branchId}";
            }

            var sqlCmd = $@"select t.PersianLocalYearMonth,
	                   sum(iif(isnull(t.BuyTransactionAmount, 0) >= 20000000 OR isnull(t.BuyTransactionCount, 0) >= 60, 1, 0)) as HighTransactionCount,  -- پر تراکنش
	                   sum(iif(isnull(t.BuyTransactionAmount, 0) > 9999 and isnull(t.BuyTransactionAmount, 0) < 20000000 AND isnull(t.BuyTransactionCount, 0) < 60, 1, 0)) as LowTransactionCount, -- کم تراکنش
                       sum(iif(isnull(t.BuyTransactionAmount, 0) >= 0 and isnull(t.BuyTransactionAmount, 0) <= 9999, 1, 0)) as WithoutTransactionCount -- فاقد تراکنش
                from psp.Terminal tr 
                join psp.[TransactionSum] t on tr.TerminalNo = t.TerminalNo     
                join dbo.OrganizationUnit b on b.id = tr.BranchId
               where t.PersianLocalYearMonth >= {fromYearMonth} AND t.PersianLocalYearMonth <= {toYearMonth} AND tr.InstallationDate is not null {branchFilter}";

            if (isTehranBranchManagement)
                sqlCmd += $" and tr.CityId = {(long)Enums.City.Tehran} ";

            if (isCountyBranchManagement)
                sqlCmd += $" and tr.CityId != {(long)Enums.City.Tehran} ";

            sqlCmd += "group by t.PersianLocalYearMonth;";

            return await Database.SqlQuery<BranchRankingData>(sqlCmd).ToListAsync();
        }

        public async Task<Tuple<bool, bool>> CheckBranchLimitations(long? currentUserBranchId)
        {
            var disableNewTerminalRequest = false;
            var disableWirelessTerminalRequest = false;
            if (currentUserBranchId.HasValue)
            {
                var currentUserBranchInfo = await OrganizationUnits.Where(x => x.Id == currentUserBranchId)
                    .Select(x => new { x.DisableNewTerminalRequest, x.DisableWirelessTerminalRequest })
                    .FirstOrDefaultAsync();

                disableNewTerminalRequest = currentUserBranchInfo?.DisableNewTerminalRequest ?? false;
                disableWirelessTerminalRequest = currentUserBranchInfo?.DisableWirelessTerminalRequest ?? false;
            }

            return Tuple.Create(disableNewTerminalRequest, disableWirelessTerminalRequest);
        }
    }
}