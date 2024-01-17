using OfficeOpenXml;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TES.Data;
using TES.Data.Domain;
using TES.Security;
using TES.Web.Core;
using TES.Web.Core.Extensions;

namespace TES.Merchant.Web.UI.Controllers
{
    public class NormalRepController : BaseController
    {
        private readonly AppDataContext _dataContext;
        public NormalRepController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }
        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> ImportFanAva(HttpPostedFileBase file1)
        {
            try
            {
                if (!file1.IsValidFormat(".xlsx"))
                {
                    return JsonWarningMessage("تنها فایل با پسوند .xlsx مجاز می باشد.");
                }
                DataTable NormalReDataTable = new DataTable();
                NormalReDataTable.Columns.Add(new DataColumn("TerminalNum", typeof(string)));
                NormalReDataTable.Columns.Add(new DataColumn("SerialDevice", typeof(string)));
                NormalReDataTable.Columns.Add(new DataColumn("PSP", typeof(string)));
                NormalReDataTable.Columns.Add(new DataColumn("Representative", typeof(string)));
                NormalReDataTable.Columns.Add(new DataColumn("AcceptorNo", typeof(string)));
                NormalReDataTable.Columns.Add(new DataColumn("ContractNo", typeof(string)));
                NormalReDataTable.Columns.Add(new DataColumn("Market", typeof(string)));
                NormalReDataTable.Columns.Add(new DataColumn("AccountNo", typeof(string)));
                NormalReDataTable.Columns.Add(new DataColumn("ShebaNo", typeof(string)));
                NormalReDataTable.Columns.Add(new DataColumn("CustomerNo", typeof(string)));
                NormalReDataTable.Columns.Add(new DataColumn("BranchId", typeof(long)));
                NormalReDataTable.Columns.Add(new DataColumn("Ostan", typeof(string)));
                NormalReDataTable.Columns.Add(new DataColumn("City1", typeof(string)));
                NormalReDataTable.Columns.Add(new DataColumn("DeviceType1", typeof(string)));
                NormalReDataTable.Columns.Add(new DataColumn("DeviceTypeId", typeof(string)));
                NormalReDataTable.Columns.Add(new DataColumn("Marker", typeof(string)));
                NormalReDataTable.Columns.Add(new DataColumn("NationalCode", typeof(string)));
                NormalReDataTable.Columns.Add(new DataColumn("StoreManager", typeof(string)));
                NormalReDataTable.Columns.Add(new DataColumn("Statuse", typeof(string)));
                NormalReDataTable.Columns.Add(new DataColumn("Class", typeof(string)));
                NormalReDataTable.Columns.Add(new DataColumn("Address", typeof(string)));
                NormalReDataTable.Columns.Add(new DataColumn("TelCode", typeof(string)));
                NormalReDataTable.Columns.Add(new DataColumn("Mobile", typeof(string)));
                NormalReDataTable.Columns.Add(new DataColumn("OwnerDevice", typeof(string)));
                NormalReDataTable.Columns.Add(new DataColumn("CardType1", typeof(string)));
                var psp = "";
                using (var package = new ExcelPackage(file1.InputStream))
                {
                    var workSheet = package.Workbook.Worksheets.First();
                    for (var rowNumber = 2; rowNumber <= workSheet.Dimension.End.Row; rowNumber++)
                    {
                        var row = workSheet.Cells[rowNumber, 1, rowNumber, workSheet.Dimension.End.Column];
                        var NormalReDataRowFanAva = NormalReDataTable.NewRow();
                        NormalReDataRowFanAva["TerminalNum"] = (!string.IsNullOrEmpty(row[rowNumber, 2].Text))
                            ? row[rowNumber, 2].Text
                            : "نامشخص";
                        var TerminalNum = NormalReDataRowFanAva["TerminalNum"].ToString();
                        // وجود دارد یا خیر ؟ Terminals  بررسی می شود که آیا شماره ترمینال در جدول  
                        if (_dataContext.Terminals.Any(o => o.TerminalNo == TerminalNum))
                        {
                            try
                            {
                                var dataFanAva = (from t in _dataContext.Terminals
                                    join m in _dataContext.Marketers on t.MarketerId equals m.Id
                                    join c in _dataContext.Cities on t.CityId equals c.Id
                                    where t.TerminalNo == TerminalNum
                                    select new
                                    {
                                        Address = t.Address,
                                        Tel = t.Tel,
                                        Title = m.Title,
                                        CityName = c.Title
                                    }).FirstOrDefault();
                                NormalReDataRowFanAva["Address"] = dataFanAva.Address; //آدرس
                                NormalReDataRowFanAva["TelCode"] = dataFanAva.Tel; //تلفن
                                NormalReDataRowFanAva["City1"] = dataFanAva.CityName; //شهر
                                //بازاریاب 
                                if (!string.IsNullOrEmpty(dataFanAva.Title))
                                {
                                    if (dataFanAva.Title == "PSP")
                                    {
                                        NormalReDataRowFanAva["Marker"] =
                                            dataFanAva.Title.ToString() + NormalReDataRowFanAva["PSP"].ToString();
                                    }
                                    else
                                    {
                                        NormalReDataRowFanAva["Marker"] = dataFanAva.Title;
                                    }
                                }
                                else
                                {
                                    NormalReDataRowFanAva["Marker"] = "نامشخص";
                                }
                            }
                            catch (Exception ex)
                            {
                              Console.WriteLine(ex.Message);
                            }
                        }
                        else // اگر شماره ترمینال در دیتابیس نباشد اطلاعات از اکسل خوانده می شود
                        {
                            //آدرس
                            NormalReDataRowFanAva["Address"] = (!string.IsNullOrEmpty(row[rowNumber, 17].Text))
                                ? row[rowNumber, 17].Text
                                : "نامشخص";
                            //تلفن
                            NormalReDataRowFanAva["TelCode"] = (!string.IsNullOrEmpty(row[rowNumber, 15].Text))
                                ? row[rowNumber, 15].Text
                                : "نامشخص";
                            //شهر
                            NormalReDataRowFanAva["City1"] = (!string.IsNullOrEmpty(row[rowNumber, 11].Text))
                                ? row[rowNumber, 11].Text
                                : "نامشخص";
                        }
                        //این قسمت فیلدهای مشترک هستند که  شماره ترمینال چه در دیتابیس وجود داشته باشد و چه نباشد، در هردو صورت این فیلدها از اکسل خوانده می شوند
                        //حتی اگر شماره ترمینال در دیتابیس موجود باشد برای یکپارچه بودن شماره شبا از اکسل خوانده می شود
                        //شماره شبا
                        NormalReDataRowFanAva["ShebaNo"] = (!string.IsNullOrEmpty(row[rowNumber, 25].Text))
                            ? row[rowNumber, 25].Text
                            : "نامشخص";
                        //شماره حساب  که از شماره شبا به دست می آید
                            NormalReDataRowFanAva["AccountNo"] =
                            NormalReDataRowFanAva["ShebaNo"].ToString().Substring(8, 18);
                        //شماره مشتری که از شماره حساب به دست می آید 
                        NormalReDataRowFanAva["CustomerNo"] =
                            NormalReDataRowFanAva["AccountNo"].ToString().Substring(9, 8);
                        //کد شعبه
                        var BranchId =
                            (!string.IsNullOrEmpty(NormalReDataRowFanAva["AccountNo"].ToString().Substring(0, 4)))
                                ? Convert.ToInt64(NormalReDataRowFanAva["AccountNo"].ToString().Substring(0, 4))
                                : 0;
                        if (BranchId != 0) // کد شعبه در دیتابیس به صورت عددی تعریف شده است
                        {
                            if (BranchId == 0000 || BranchId == 0010 || BranchId == 0011|| BranchId == 0065|| BranchId == 0170||BranchId == 0368||
                                BranchId == 0400 || BranchId == 0820 || BranchId == 0901|| BranchId == 0904|| BranchId == 0958||BranchId == 2033||
                                BranchId == 2306 || BranchId == 6904 || BranchId == 9001)
                            {
                                NormalReDataRowFanAva["BranchId"] = 1010;
                            }
                            else
                            {
                                NormalReDataRowFanAva["BranchId"] = BranchId;
                            }
                        }
                        else
                        {
                            NormalReDataRowFanAva["BranchId"] = 0;
                        }
                        //سریال دستگاه
                        NormalReDataRowFanAva["SerialDevice"] = (!string.IsNullOrEmpty(row[rowNumber, 3].Text))
                            ? row[rowNumber, 3].Text
                            : " نامشخص";
                        //شماره پذیرنده
                        NormalReDataRowFanAva["AcceptorNo"] = (!string.IsNullOrEmpty(row[rowNumber, 4].Text))
                            ? row[rowNumber, 4].Text
                            : "نامشخص";
                        //شماره قرارداد
                        NormalReDataRowFanAva["ContractNo"] = (!string.IsNullOrEmpty(row[rowNumber, 1].Text))
                            ? row[rowNumber, 1].Text
                            : "نامشخص";
                        //نوع دستگاه
                        NormalReDataRowFanAva["DeviceType1"] = (!string.IsNullOrEmpty(row[rowNumber, 36].Text))
                            ? row[rowNumber, 36].Text
                            : "نامشخص";
                        int deviceTypeId = 0;
                        switch (NormalReDataRowFanAva["DeviceType1"])
                        {
                            case "Dialup":
                                deviceTypeId = 1;
                                break;
                            case "LAN POS":
                                deviceTypeId = 2;
                                break;
                            case "GPRS":
                                deviceTypeId = 3;
                                break;
                            case "PDA":
                                deviceTypeId = 6;
                                break;
                            case "MPOS":
                            case "BlueTooth POS":
                                deviceTypeId = 7;
                                break;
                            case "Wifi":
                                deviceTypeId = 8;
                                break;
                            case "PCPOS":
                                deviceTypeId = 9;
                                break;
                            case "IPG":
                                deviceTypeId = 14;
                                break;
                            case "Cacheless ATM":
                                deviceTypeId = 16;
                                break;
                            case "Typical/Gprs":
                                deviceTypeId = 19;
                                break;
                            case "PinPad":
                                deviceTypeId = 20;
                                break;
                            case "Base":
                                deviceTypeId = 21;
                                break;
                        }
                        //شناسه دستگاه
                        NormalReDataRowFanAva["DeviceTypeId"] = deviceTypeId.ToString();
                        //کدملی
                        NormalReDataRowFanAva["NationalCode"] = !string.IsNullOrEmpty(row[rowNumber, 12].Text)
                            ? row[rowNumber, 12].Text
                            : "نامشخص";
                        //استان
                        NormalReDataRowFanAva["Ostan"] = !string.IsNullOrEmpty(row[rowNumber, 13].Text)
                            ? row[rowNumber, 13].Text
                            : "نامشخص";
                        //صنف 
                        NormalReDataRowFanAva["Class"] = !string.IsNullOrEmpty(row[rowNumber, 22].Text)
                            ? row[rowNumber, 22].Text
                            : "نامشخص";
                        //مدیر فروشگاه
                        NormalReDataRowFanAva["StoreManager"] = !string.IsNullOrEmpty(row[rowNumber, 10].Text)
                            ? row[rowNumber, 10].Text
                            : "نامشخص";
                        //فروشگاه یا نام پذیرنده                                                  
                        NormalReDataRowFanAva["Market"] = !string.IsNullOrEmpty(row[rowNumber, 20].Text)
                            ? row[rowNumber, 20].Text
                            : "نامشخص";
                        //وضعیت 
                        NormalReDataRowFanAva["Statuse"] = !string.IsNullOrEmpty(row[rowNumber, 32].Text)
                            ? row[rowNumber, 32].Text
                            : "نامشخص";
                        //موبایل
                        NormalReDataRowFanAva["Mobile"] = !string.IsNullOrEmpty(row[rowNumber, 19].Text)
                            ? row[rowNumber, 19].Text
                            : "نامشخص";
                        //نماینده در اکسل فناوا
                        NormalReDataRowFanAva["Representative"] = !string.IsNullOrEmpty(row[rowNumber, 6].Text)
                            ? row[rowNumber, 6].Text
                            : "نامشخص";
                        NormalReDataRowFanAva["PSP"] = "فن آوا";
                        //مالک دستگاه 
                        var serialDevice = NormalReDataRowFanAva["SerialDevice"];
                        if (!string.IsNullOrEmpty(serialDevice.ToString()))
                        {
                            Seriall ownerDevice = _dataContext.Serialls.FirstOrDefault(a =>
                                a.SerialNo.ToString() == serialDevice.ToString());
                            if (ownerDevice != null)
                            {
                                NormalReDataRowFanAva["OwnerDevice"] = "سرمایه";
                            }
                            else
                            {
                                NormalReDataRowFanAva["OwnerDevice"] = "psp";
                            }
                        }
                        else
                        {
                            NormalReDataRowFanAva["OwnerDevice"] = "نامشخص";
                        }
                        //کارت خوان 
                        var DeviceType = NormalReDataRowFanAva["DeviceType1"];
                        var TypeCardFanAva = "";
                        var listTypeDevice = _dataContext.Device_Cards.Select(a => a.TypeDevice).ToList();
                        // هایی که در اکسل است در جدول دیتابیس هم موجود باشد  DeviceType بررسی میکنیم تمام   
                        if (!string.IsNullOrEmpty(DeviceType.ToString()) && listTypeDevice.Contains(DeviceType))
                        {
                            TypeCardFanAva = _dataContext.Device_Cards
                                .Where(a => a.TypeDevice == DeviceType.ToString()).FirstOrDefault().TypeCard;
                            NormalReDataRowFanAva["CardType1"] = TypeCardFanAva;
                        }
                        else
                        {
                            NormalReDataRowFanAva["CardType1"] = "نامشخص";
                        }

                        psp = NormalReDataRowFanAva["PSP"].ToString();
                        NormalReDataTable.Rows.Add(NormalReDataRowFanAva);
                    }
                }
                //در هربار ورود اطلاعات ، داده های قبل که وارد شده بود پاک می شود
                using (var sqlConnection =
                    new SqlConnection(ConfigurationManager.ConnectionStrings["AppDataContext"].ConnectionString))
                {
                    await sqlConnection.OpenAsync();
                    try
                    {
                        if (psp == "فن آوا")
                        {
                            using (SqlCommand command = new SqlCommand("DELETE FROM  NormalRep where PSP  =N'فن آوا'",sqlConnection))
                            {
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    //.در هربار ورود اطلاعات ، داده های قبل که وارد شده بود پاک می شود
                    using (var transaction = sqlConnection.BeginTransaction())
                    {
                        using (var sqlBulkCopy =
                            new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.KeepIdentity, transaction))
                        {
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("TerminalNum", "TerminalNum"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("SerialDevice",
                                "SerialDevice"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("OwnerDevice", "OwnerDevice"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("PSP", "PSP"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Representative",
                                "Representative"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("AcceptorNo", "AcceptorNo"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ContractNo", "ContractNo"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Market", "Market"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("AccountNo", "AccountNo"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ShebaNo", "ShebaNo"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CustomerNo", "CustomerNo"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("BranchId", "BranchId"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Ostan", "Ostan"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("City1", "City1"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("DeviceType1", "DeviceType1"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("DeviceTypeId",
                                "DeviceTypeId"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CardType1", "CardType1"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Marker", "Marker"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("NationalCode",
                                "NationalCode"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("StoreManager",
                                "StoreManager"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Statuse", "Statuse"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Class", "Class"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Address", "Address"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("TelCode", "TelCode"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Mobile", "Mobile"));
                            sqlBulkCopy.BatchSize = 50000;
                            sqlBulkCopy.BulkCopyTimeout = 100000;
                            sqlBulkCopy.DestinationTableName =
                                $"[{_dataContext.Database.Connection.Database}].[dbo].[NormalRep]";
                            try
                            {
                                await sqlBulkCopy.WriteToServerAsync(NormalReDataTable);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                                transaction.Rollback();
                            }
                            transaction.Commit();
                        }
                    }
                }
                return JsonSuccessMessage("فرآیند وارد نمودن اطلاعات  از طریق فایل با موفقیت انجام شد.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return JsonErrorMessage("فرآیند وارد نمودن اطلاعات  از طریق فایل  انجام نشد.");
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////    
        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> ImportIranKish(HttpPostedFileBase file2)
        {
            try
            {
                if (!file2.IsValidFormat(".xlsx"))
                {
                    return JsonWarningMessage("تنها فایل با پسوند .xlsx مجاز می باشد.");
                }
                DataTable NormalIranKishReDataTable = new DataTable();
                #region
                NormalIranKishReDataTable.Columns.Add(new DataColumn("TerminalNum", typeof(string)));
                NormalIranKishReDataTable.Columns.Add(new DataColumn("OwnerDevice", typeof(string)));
                NormalIranKishReDataTable.Columns.Add(new DataColumn("SerialDevice", typeof(string)));
                NormalIranKishReDataTable.Columns.Add(new DataColumn("PSP", typeof(string)));
                NormalIranKishReDataTable.Columns.Add(new DataColumn("Representative", typeof(string)));
                NormalIranKishReDataTable.Columns.Add(new DataColumn("AcceptorNo", typeof(string)));
                NormalIranKishReDataTable.Columns.Add(new DataColumn("ContractNo", typeof(string)));
                NormalIranKishReDataTable.Columns.Add(new DataColumn("Market", typeof(string)));
                NormalIranKishReDataTable.Columns.Add(new DataColumn("AccountNo", typeof(string)));
                NormalIranKishReDataTable.Columns.Add(new DataColumn("ShebaNo", typeof(string)));
                NormalIranKishReDataTable.Columns.Add(new DataColumn("CustomerNo", typeof(string)));
                NormalIranKishReDataTable.Columns.Add(new DataColumn("BranchId", typeof(long)));
                NormalIranKishReDataTable.Columns.Add(new DataColumn("Ostan", typeof(string)));
                NormalIranKishReDataTable.Columns.Add(new DataColumn("City1", typeof(string)));
                NormalIranKishReDataTable.Columns.Add(new DataColumn("DeviceType1", typeof(string)));
                NormalIranKishReDataTable.Columns.Add(new DataColumn("DeviceTypeId", typeof(string)));
                NormalIranKishReDataTable.Columns.Add(new DataColumn("Marker", typeof(string)));
                NormalIranKishReDataTable.Columns.Add(new DataColumn("NationalCode", typeof(string)));
                NormalIranKishReDataTable.Columns.Add(new DataColumn("StoreManager", typeof(string)));
                NormalIranKishReDataTable.Columns.Add(new DataColumn("Statuse", typeof(string)));
                NormalIranKishReDataTable.Columns.Add(new DataColumn("Class", typeof(string)));
                NormalIranKishReDataTable.Columns.Add(new DataColumn("Address", typeof(string)));
                NormalIranKishReDataTable.Columns.Add(new DataColumn("TelCode", typeof(string)));
                NormalIranKishReDataTable.Columns.Add(new DataColumn("Mobile", typeof(string)));
                NormalIranKishReDataTable.Columns.Add(new DataColumn("CardType1", typeof(string)));
                var psp = "";
                #endregion
                using (var package = new ExcelPackage(file2.InputStream))
                {
                    var workSheet = package.Workbook.Worksheets.First();
                    var totalNumberOfRowsWithoutHeader = workSheet.Dimension.End.Row - 1;
                    for (var rowNumber = 2; rowNumber <= workSheet.Dimension.End.Row; rowNumber++)
                    {
                        var row = workSheet.Cells[rowNumber, 1, rowNumber, workSheet.Dimension.End.Column];
                        var NormalReIranKishDataRow = NormalIranKishReDataTable.NewRow();
                        //شماره ترمینال
                        NormalReIranKishDataRow["TerminalNum"] = (!string.IsNullOrEmpty(row[rowNumber, 4].Text))
                            ? row[rowNumber, 4].Text
                            : "نامشخص";
                        var TerminalNum = NormalReIranKishDataRow["TerminalNum"];
                        // وجود دارد یا نه Terminals  بررسی می شود که آیا شماره ترمینال در جدول  
                        if (_dataContext.Terminals.Any(o => o.TerminalNo == TerminalNum.ToString()))
                        {
                            try
                            {
                                var dataIranKish = (from t in _dataContext.Terminals
                                    join m in _dataContext.Marketers on t.MarketerId equals m.Id
                                    join c in _dataContext.Cities on t.CityId equals c.Id
                                    where t.TerminalNo.ToString() == TerminalNum.ToString()
                                    select new
                                    {
                                        ShebaNo = t.ShebaNo,
                                        Address = t.Address,
                                        Tel = t.Tel,
                                        Title = m.Title,
                                        CityName = c.Title
                                    }).FirstOrDefault();
                                NormalReIranKishDataRow["Address"] = dataIranKish.Address; //آدرس
                                NormalReIranKishDataRow["TelCode"] = dataIranKish.Tel; //تلفن
                                NormalReIranKishDataRow["City1"] = dataIranKish.CityName; //شهر
                                //بازاریاب 
                                if (!string.IsNullOrEmpty(dataIranKish.Title.ToString()))
                                {
                                    if (dataIranKish.Title == "PSP")
                                    {
                                        NormalReIranKishDataRow["Marker"] = dataIranKish.Title.ToString() +
                                                                            NormalReIranKishDataRow["PSP"].ToString();
                                    }
                                    else
                                    {
                                        NormalReIranKishDataRow["Marker"] = dataIranKish.Title;
                                    }
                                }
                                else
                                {
                                    NormalReIranKishDataRow["Marker"] = "نامشخص";
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                        else // اگر شماره پایانه در دیتابیس نبود از اکسل اطلاعات را وارد می کنیم 
                        {
                            //آدرس
                            NormalReIranKishDataRow["Address"] = !string.IsNullOrEmpty(row[rowNumber, 17].Text)
                                ? row[rowNumber, 17].Text
                                : "نامشخص";
                            //تلفن
                            NormalReIranKishDataRow["TelCode"] = !string.IsNullOrEmpty(row[rowNumber, 15].Text)
                                ? row[rowNumber, 15].Text
                                : "نامشخص";
                            //شهر
                            NormalReIranKishDataRow["City1"] = !string.IsNullOrEmpty(row[rowNumber, 11].Text)
                                ? row[rowNumber, 11].Text
                                : "نامشخص";
                            NormalReIranKishDataRow["Marker"] = "نا مشخص";
                        }
                        //این قسمت فیلدهای مشترک هستند که  شماره ترمینال چه در دیتابیس وجود داشته باشد و چه نباشد، در هردو صورت این فیلدها از اکسل خوانده می شوند
                        //حتی اگر شماره ترمینال در دیتابیس موجود باشد برای یکپارچه بودن شماره شبا از اکسل خوانده می شود
                        //شماره شبا
                        NormalReIranKishDataRow["ShebaNo"] = !string.IsNullOrEmpty(row[rowNumber, 6].Text)
                            ? row[rowNumber, 6].Text
                            : "نامشخص";
                        //شماره حساب
                        NormalReIranKishDataRow["AccountNo"] =
                            NormalReIranKishDataRow["ShebaNo"].ToString().Substring(8, 18);
                        //شماره مشتری
                        NormalReIranKishDataRow["CustomerNo"] =
                            NormalReIranKishDataRow["AccountNo"].ToString().Substring(9, 8);
                        //کد شعبه
                        var BranchId =
                            (!string.IsNullOrEmpty(NormalReIranKishDataRow["AccountNo"].ToString().Substring(0, 4)))
                                ? Convert.ToInt64(NormalReIranKishDataRow["AccountNo"].ToString().Substring(0, 4))
                                : 0;
                        if (BranchId != 0)
                        { 
                            if (BranchId == 0000 || BranchId == 0010 || BranchId == 0011|| BranchId == 0065|| BranchId == 0170||BranchId == 0368||
                                BranchId == 0400 || BranchId == 0820 || BranchId == 0901|| BranchId == 0904|| BranchId == 0958||BranchId == 2033||
                                BranchId == 2306 || BranchId == 6904 || BranchId == 9001)
                            {
                                NormalReIranKishDataRow["BranchId"] = 1010;
                            }
                            else
                            {
                                NormalReIranKishDataRow["BranchId"] = BranchId;
                            }
                        }
                        else
                        {
                            NormalReIranKishDataRow["BranchId"] = 0;
                        }
                        //سریال دستگاه 
                        NormalReIranKishDataRow["SerialDevice"] = !string.IsNullOrEmpty(row[rowNumber, 36].Text)
                            ? row[rowNumber, 36].Text
                            : " نامشخص";
                        //شماره پذیرنده
                        NormalReIranKishDataRow["AcceptorNo"] = !string.IsNullOrEmpty(row[rowNumber, 3].Text)
                            ? row[rowNumber, 3].Text
                            : "نامشخص";
                        //شماره قرارداد
                        NormalReIranKishDataRow["ContractNo"] = !string.IsNullOrEmpty(row[rowNumber, 24].Text)
                            ? row[rowNumber, 24].Text
                            : "نامشخص";
                        //نوع دستگاه
                        NormalReIranKishDataRow["DeviceType1"] = !string.IsNullOrEmpty(row[rowNumber, 23].Text)
                            ? row[rowNumber, 22].Text
                            : " نامشخص";
                        int deviceTypeId = 0;
                        switch (NormalReIranKishDataRow["DeviceType1"])
                        {
                            case "Dialup":
                                deviceTypeId = 1;
                                break;
                            case "LAN POS":
                                deviceTypeId = 2;
                                break;
                            case "GPRS":
                                deviceTypeId = 3;
                                break;
                            case "PDA":
                                deviceTypeId = 6;
                                break;
                            case "MPOS":
                            //در داده هایی که از ورودی میگیریم معمولا این عبارت برای دستگاه های  پز را داریم 
                            case "BlueTooth POS":
                                deviceTypeId = 7;
                                break;
                            case "Wifi":
                                deviceTypeId = 8;
                                break;
                            case "PCPOS":
                                deviceTypeId = 9;
                                break;
                            case "IPG":
                                deviceTypeId = 14;
                                break;
                            case "Cacheless ATM":
                                deviceTypeId = 16;
                                break;
                            case "Typical/Gprs":
                                deviceTypeId = 19;
                                break;
                            case "PinPad":
                                deviceTypeId = 20;
                                break;
                            case "Base":
                                deviceTypeId = 21;
                                break;
                        }
                        //شناسه دستگاه
                        NormalReIranKishDataRow["DeviceTypeId"] = deviceTypeId.ToString();
                        //کدملی
                        NormalReIranKishDataRow["NationalCode"] = !string.IsNullOrEmpty(row[rowNumber, 16].Text)
                            ? row[rowNumber, 16].Text
                            : "نامشخص";
                        //استان
                        NormalReIranKishDataRow["Ostan"] = !string.IsNullOrEmpty(row[rowNumber, 10].Text)
                            ? row[rowNumber, 10].Text
                            : "نامشخص";
                        //صنف
                        NormalReIranKishDataRow["Class"] = !string.IsNullOrEmpty(row[rowNumber, 18].Text)
                            ? row[rowNumber, 18].Text
                            : "نامشخص";
                        //مدیر فروشگاه
                        NormalReIranKishDataRow["StoreManager"] = !string.IsNullOrEmpty(row[rowNumber, 37].Text)
                            ? row[rowNumber, 37].Text
                            : "نامشخص";
                        //فروشگاه
                        NormalReIranKishDataRow["Market"] = !string.IsNullOrEmpty(row[rowNumber, 7].Text)
                            ? row[rowNumber, 7].Text
                            : "نامشخص";
                        //وضعیت
                        NormalReIranKishDataRow["Statuse"] = !string.IsNullOrEmpty(row[rowNumber, 30].Text)
                            ? row[rowNumber, 30].Text
                            : "نامشخص";
                        //موبایل
                        NormalReIranKishDataRow["Mobile"] = !string.IsNullOrEmpty(row[rowNumber, 14].Text)
                            ? row[rowNumber, 14].Text
                            : "نامشخص";
                        //پیمانکار
                        NormalReIranKishDataRow["Representative"] = !string.IsNullOrEmpty(row[rowNumber, 125].Text)
                            ? row[rowNumber, 125].Text
                            : "نامشخص";
                        NormalReIranKishDataRow["PSP"] = "ایران کیش";
                        var SerialDevice = NormalReIranKishDataRow["SerialDevice"];
                        if (!string.IsNullOrEmpty(SerialDevice.ToString()))
                        {
                            Seriall OwnerDevice = _dataContext.Serialls
                                .Where(a => a.SerialNo.ToString() == SerialDevice.ToString()).FirstOrDefault();
                            if (OwnerDevice != null)
                            {
                                NormalReIranKishDataRow["OwnerDevice"] = "سرمایه";
                            }
                            else
                            {
                                NormalReIranKishDataRow["OwnerDevice"] = "psp";
                            }
                        }
                        else
                        {
                            NormalReIranKishDataRow["OwnerDevice"] = "نامشخص";
                        }
                        //کارت خوان 
                        var DeviceType = NormalReIranKishDataRow["DeviceType1"];
                        var TypeCardIranKish = "";
                        var listTypeDevice = _dataContext.Device_Cards.Select(a => a.TypeDevice).ToList();
                        // هایی که در اکسل است در جدول دیتابیس هم موجود باشد  DeviceType بررسی میکنیم تمام   
                        if (!string.IsNullOrEmpty(DeviceType.ToString()) && listTypeDevice.Contains(DeviceType))
                        {
                            TypeCardIranKish = _dataContext.Device_Cards
                                .Where(a => a.TypeDevice == DeviceType.ToString()).FirstOrDefault().TypeCard;
                            NormalReIranKishDataRow["CardType1"] = TypeCardIranKish;
                        }
                        else
                        {
                            NormalReIranKishDataRow["CardType1"] = "نامشخص";
                        }
                        psp = NormalReIranKishDataRow["PSP"].ToString();
                        NormalIranKishReDataTable.Rows.Add(NormalReIranKishDataRow);
                    }
                }
                // در هربار ورود اطلاعات ، داده های قبل که وارد شده بود پاک می شود
                using (var sqlConnection =
                    new SqlConnection(ConfigurationManager.ConnectionStrings["AppDataContext"].ConnectionString))
                {
                    await sqlConnection.OpenAsync();
                    try
                    {
                        if (psp == "ایران کیش")
                        {
                            using (SqlCommand command =
                                new SqlCommand("DELETE FROM  NormalRep where PSP  =N'ایران کیش'", sqlConnection))
                            {
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    //می شود Map  در مرحله آخر اطلاعات اطلاعات جدول سی شارپ به جدول دیتابیس
                    using (var transaction = sqlConnection.BeginTransaction())
                    {
                        using (var sqlBulkCopy =
                            new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.KeepIdentity, transaction))
                        {
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("TerminalNum", "TerminalNum"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("SerialDevice",
                                "SerialDevice"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("OwnerDevice", "OwnerDevice"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("PSP", "PSP"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Representative",
                                "Representative"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("AcceptorNo", "AcceptorNo"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ContractNo", "ContractNo"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Market", "Market"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("AccountNo", "AccountNo"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ShebaNo", "ShebaNo"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CustomerNo", "CustomerNo"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("BranchId", "BranchId"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Ostan", "Ostan"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("City1", "City1"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("DeviceType1", "DeviceType1"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("DeviceTypeId",
                                "DeviceTypeId"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CardType1", "CardType1"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Marker", "Marker"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("NationalCode",
                                "NationalCode"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("StoreManager",
                                "StoreManager"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Statuse", "Statuse"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Class", "Class"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Address", "Address"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("TelCode", "TelCode"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Mobile", "Mobile"));
                            sqlBulkCopy.BatchSize = 33000;
                            sqlBulkCopy.BulkCopyTimeout = 30000;
                            sqlBulkCopy.DestinationTableName =
                                $"[{_dataContext.Database.Connection.Database}].[dbo].[NormalRep]";
                            try
                            {
                                await sqlBulkCopy.WriteToServerAsync(NormalIranKishReDataTable);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                                transaction.Rollback();
                            }
                            transaction.Commit();
                        }
                    }
                }
                return JsonSuccessMessage("فرآیند وارد نمودن اطلاعات  از طریق فایل با موفقیت انجام شد.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return JsonErrorMessage("فرآیند وارد نمودن اطلاعات  از طریق فایل  انجام نشد.");
            }
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> ImportParsian(HttpPostedFileBase file3)
        {
            try
            {
                if (!file3.IsValidFormat(".xlsx"))
                {
                    return JsonWarningMessage("تنها فایل با پسوند .xlsx مجاز می باشد.");
                }
                DataTable NormalParsianReDataTable = new DataTable();
                NormalParsianReDataTable.Columns.Add(new DataColumn("TerminalNum", typeof(string)));
                NormalParsianReDataTable.Columns.Add(new DataColumn("SerialDevice", typeof(string)));
                NormalParsianReDataTable.Columns.Add(new DataColumn("PSP", typeof(string)));
                NormalParsianReDataTable.Columns.Add(new DataColumn("Representative", typeof(string)));
                NormalParsianReDataTable.Columns.Add(new DataColumn("AcceptorNo", typeof(string)));
                NormalParsianReDataTable.Columns.Add(new DataColumn("ContractNo", typeof(string)));
                NormalParsianReDataTable.Columns.Add(new DataColumn("Market", typeof(string)));
                NormalParsianReDataTable.Columns.Add(new DataColumn("AccountNo", typeof(string)));
                NormalParsianReDataTable.Columns.Add(new DataColumn("ShebaNo", typeof(string)));
                NormalParsianReDataTable.Columns.Add(new DataColumn("CustomerNo", typeof(string)));
                NormalParsianReDataTable.Columns.Add(new DataColumn("BranchId", typeof(long)));
                NormalParsianReDataTable.Columns.Add(new DataColumn("Ostan", typeof(string)));
                NormalParsianReDataTable.Columns.Add(new DataColumn("City1", typeof(string)));
                NormalParsianReDataTable.Columns.Add(new DataColumn("DeviceType1", typeof(string)));
                NormalParsianReDataTable.Columns.Add(new DataColumn("DeviceTypeId", typeof(string)));
                NormalParsianReDataTable.Columns.Add(new DataColumn("Marker", typeof(string)));
                NormalParsianReDataTable.Columns.Add(new DataColumn("NationalCode", typeof(string)));
                NormalParsianReDataTable.Columns.Add(new DataColumn("StoreManager", typeof(string)));
                NormalParsianReDataTable.Columns.Add(new DataColumn("Statuse", typeof(string)));
                NormalParsianReDataTable.Columns.Add(new DataColumn("Class", typeof(string)));
                NormalParsianReDataTable.Columns.Add(new DataColumn("Address", typeof(string)));
                NormalParsianReDataTable.Columns.Add(new DataColumn("TelCode", typeof(string)));
                NormalParsianReDataTable.Columns.Add(new DataColumn("Mobile", typeof(string)));
                NormalParsianReDataTable.Columns.Add(new DataColumn("OwnerDevice", typeof(string)));
                NormalParsianReDataTable.Columns.Add(new DataColumn("CardType1", typeof(string)));
                var psp = "";
                using (var package = new ExcelPackage(file3.InputStream))
                {
                    var workSheet = package.Workbook.Worksheets.First();
                    var totalNumberOfRowsWithoutHeader = workSheet.Dimension.End.Row - 1;
                    for (var rowNumber = 2; rowNumber <= workSheet.Dimension.End.Row; rowNumber++)
                    {
                        var row = workSheet.Cells[rowNumber, 1, rowNumber, workSheet.Dimension.End.Column];
                        var NormalReParsianDataRow = NormalParsianReDataTable.NewRow();
                        NormalReParsianDataRow["TerminalNum"] = (!string.IsNullOrEmpty(row[rowNumber, 1].Text))
                            ? row[rowNumber, 1].Text
                            : "نامشخص";
                        // ابتدا بررسی می کنیم شماره ترمینال موجود دراکسل در دیتابیس وجود دارد یا خیر ؟
                        var TerminalNum = NormalReParsianDataRow["TerminalNum"];
                        if (_dataContext.Terminals.Any(o => o.TerminalNo == TerminalNum.ToString()))
                        {
                            try
                            {
                                var dataParsian = (from t in _dataContext.Terminals
                                    join m in _dataContext.Marketers on t.MarketerId equals m.Id
                                    join c in _dataContext.Cities on t.CityId equals c.Id
                                    where t.TerminalNo.ToString() == TerminalNum.ToString()
                                    select new
                                    {
                                        Address = t.Address,
                                        Tel = t.Tel,
                                        Title = m.Title,
                                        CityName = c.Title
                                    }).FirstOrDefault();
                                NormalReParsianDataRow["Address"] = dataParsian.Address; //آدرس
                                NormalReParsianDataRow["TelCode"] = dataParsian.Tel; //تلفن
                                NormalReParsianDataRow["City1"] = dataParsian.CityName; //شهر
                                //بازاریاب 
                                if (!string.IsNullOrEmpty(dataParsian.Title.ToString()))
                                {
                                    if (dataParsian.Title == "PSP")
                                    {
                                        NormalReParsianDataRow["Marker"] = dataParsian.Title.ToString() +
                                                                           NormalReParsianDataRow["PSP"].ToString();
                                    }
                                    else
                                    {
                                        NormalReParsianDataRow["Marker"] = dataParsian.Title;
                                    }
                                }
                                else
                                {
                                    NormalReParsianDataRow["Marker"] = "نامشخص";
                                }
                            }
                            catch(Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                        else // اگر شماره ترمینال در دیتابیس نباشد اطلاعات از اکسل خوانده می شود
                        {
                            //آدرس
                            NormalReParsianDataRow["Address"] = !string.IsNullOrEmpty(row[rowNumber, 47].Text)
                                ? row[rowNumber, 47].Text
                                : "نامشخص";
                            // شماره تلفن
                            NormalReParsianDataRow["TelCode"] = !string.IsNullOrEmpty(row[rowNumber, 12].Text)
                                ? row[rowNumber, 12].Text
                                : "نامشخص";
                            //شهر
                            NormalReParsianDataRow["City1"] = !string.IsNullOrEmpty(row[rowNumber, 19].Text)
                                ? row[rowNumber, 19].Text
                                : "نامشخص";
                        }
                        //این قسمت فیلدهای مشترک هستند که  شماره ترمینال چه در دیتابیس وجود داشته باشد و چه نباشد، در هردو صورت این فیلدها از اکسل خوانده می شوند
                        //حتی اگر شماره ترمینال در دیتابیس موجود باشد برای یکپارچه بودن شماره شبا از اکسل خوانده می شود
                        //شماره شبا
                        NormalReParsianDataRow["ShebaNo"] = !string.IsNullOrEmpty(row[rowNumber, 29].Text)
                            ? row[rowNumber, 29].Text
                            : "نامشخص";
                        //شماره حساب
                        NormalReParsianDataRow["AccountNo"] =
                            !string.IsNullOrEmpty(NormalReParsianDataRow["ShebaNo"].ToString())
                                ? NormalReParsianDataRow["ShebaNo"].ToString().Substring(13, 13)
                                : "نامشخص";
                        //شماره مشتری
                        //  در داده های مربوط به پارسیان شماره مشتری  را نمی توان از شماره حساب به دست آورد 
                        NormalReParsianDataRow["CustomerNo"] = !string.IsNullOrEmpty(row[rowNumber, 10].Text)
                            ? row[rowNumber, 10].Text
                            : "نامشخص";
                        //کد شعبه
                        var BranchId =
                            !string.IsNullOrEmpty(NormalReParsianDataRow["AccountNo"].ToString().Substring(0, 4))
                                ? Convert.ToInt64(NormalReParsianDataRow["AccountNo"].ToString().Substring(0, 4))
                                : 0;
                        if (BranchId != 0)
                        { 
                            if (BranchId == 0000 || BranchId == 0010 || BranchId == 0011|| BranchId == 0065|| BranchId == 0170||BranchId == 0368||
                                BranchId == 0400 || BranchId == 0820 || BranchId == 0901|| BranchId == 0904|| BranchId == 0958||BranchId == 2033||
                                BranchId == 2306 || BranchId == 6904 || BranchId == 9001)
                            {
                                NormalReParsianDataRow["BranchId"] = 1010;
                            }
                            else
                            {
                                NormalReParsianDataRow["BranchId"] = BranchId;
                            }
                        }
                        else
                        {
                            NormalReParsianDataRow["BranchId"] = 0;
                        }
                        //سریال دستگاه
                        NormalReParsianDataRow["SerialDevice"] = !string.IsNullOrEmpty(row[rowNumber, 2].Text)
                            ? row[rowNumber, 2].Text
                            : "نامشخص";
                        //شماره پذیرنده
                        NormalReParsianDataRow["AcceptorNo"] = !string.IsNullOrEmpty(row[rowNumber, 10].Text)
                            ? row[rowNumber, 10].Text
                            : "نامشخص";
                        //شماره قرارداد
                        NormalReParsianDataRow["ContractNo"] = !string.IsNullOrEmpty(row[rowNumber, 11].Text)
                            ? row[rowNumber, 11].Text
                            : "نامشخص";
                        //نوع دستگاه 
                        NormalReParsianDataRow["DeviceType1"] = !string.IsNullOrEmpty(row[rowNumber, 17].Text)
                            ? row[rowNumber, 17].Text
                            : "نامشخص";
                        int deviceTypeId = 0;
                        switch (NormalReParsianDataRow["DeviceType1"])
                        {
                            case "Dialup":
                                deviceTypeId = 1;
                                break;
                            case "LAN POS":
                                deviceTypeId = 2;
                                break;
                            case "GPRS":
                                deviceTypeId = 3;
                                break;
                            case "PDA":
                                deviceTypeId = 6;
                                break;
                            case "MPOS":
                            case "BlueTooth POS":
                                deviceTypeId = 7;
                                break;
                            case "Wifi":
                                deviceTypeId = 8;
                                break;
                            case "PCPOS":
                                deviceTypeId = 9;
                                break;
                            case "IPG":
                                deviceTypeId = 14;
                                break;
                            case "Cacheless ATM":
                                deviceTypeId = 16;
                                break;
                            case "Typical/Gprs":
                                deviceTypeId = 19;
                                break;
                            case "PinPad":
                                deviceTypeId = 20;
                                break;
                            case "Base":
                                deviceTypeId = 21;
                                break;
                        }
                        //شناسه دستگاه
                        NormalReParsianDataRow["DeviceTypeId"] = deviceTypeId.ToString();
                        //کد ملی
                        NormalReParsianDataRow["NationalCode"] = !string.IsNullOrEmpty(row[rowNumber, 28].Text)
                            ? row[rowNumber, 28].Text
                            : "نامشخص";
                        //استان
                        NormalReParsianDataRow["Ostan"] = !string.IsNullOrEmpty(row[rowNumber, 20].Text)
                            ? row[rowNumber, 20].Text
                            : "نامشخص";
                        //صنف
                        NormalReParsianDataRow["Class"] = !string.IsNullOrEmpty(row[rowNumber, 8].Text)
                            ? row[rowNumber, 8].Text
                            : "نامشخص";
                        //مدیر فروشگاه
                        NormalReParsianDataRow["StoreManager"] = !string.IsNullOrEmpty(row[rowNumber, 12].Text)
                            ? row[rowNumber, 12].Text
                            : "نامشخص";
                        //فروشگاه 
                        NormalReParsianDataRow["Market"] = !string.IsNullOrEmpty(row[rowNumber, 7].Text)
                            ? row[rowNumber, 7].Text
                            : "نامشخص";
                        //وضعیت
                        NormalReParsianDataRow["Statuse"] = !string.IsNullOrEmpty(row[rowNumber, 5].Text)
                            ? row[rowNumber, 5].Text
                            : "نامشخص";
                        //موبایل
                        NormalReParsianDataRow["Mobile"] = !string.IsNullOrEmpty(row[rowNumber, 14].Text)
                            ? row[rowNumber, 14].Text
                            : "نامشخص";
                        //پیمانکار                                                                                                                       
                        NormalReParsianDataRow["Representative"] = !string.IsNullOrEmpty(row[rowNumber, 6].Text)
                            ? row[rowNumber, 6].Text
                            : "نامشخص";
                        NormalReParsianDataRow["PSP"] = "پارسیان";
                        //مالک دستگاه                                                             
                        var SerialDevice = NormalReParsianDataRow["SerialDevice"];
                        if (!string.IsNullOrEmpty(SerialDevice.ToString()))
                        {
                            Seriall OwnerDevice = _dataContext.Serialls
                                .Where(a => a.SerialNo.ToString() == SerialDevice.ToString()).FirstOrDefault();
                            if (OwnerDevice != null)
                            {
                                NormalReParsianDataRow["OwnerDevice"] = "سرمایه";
                            }
                            else
                            {
                                NormalReParsianDataRow["OwnerDevice"] = "psp";
                            }
                        }
                        else
                        {
                            NormalReParsianDataRow["OwnerDevice"] = "";
                        }
                        //کارت خوان 
                        var DeviceType = NormalReParsianDataRow["DeviceType1"];
                        var TypeCardParsian = "";
                        var listTypeDevice = _dataContext.Device_Cards.Select(a => a.TypeDevice).ToList();
                        // هایی که در اکسل است در جدول دیتابیس هم موجود باشد  DeviceType بررسی میکنیم تمام   
                        if (!string.IsNullOrEmpty(DeviceType.ToString()) && listTypeDevice.Contains(DeviceType))
                        {
                            TypeCardParsian = _dataContext.Device_Cards
                                .Where(a => a.TypeDevice == DeviceType.ToString()).FirstOrDefault().TypeCard;
                            NormalReParsianDataRow["CardType1"] = TypeCardParsian;
                        }
                        else
                        {
                            NormalReParsianDataRow["CardType1"] = "نامشخص";
                        }
                        psp = NormalReParsianDataRow["PSP"].ToString();
                        NormalParsianReDataTable.Rows.Add(NormalReParsianDataRow);
                    }
                }
               // در هربار ورود اطلاعات ، داده های قبل که وارد شده بود پاک می شود
                 using (var sqlConnection =
                        new SqlConnection(ConfigurationManager.ConnectionStrings["AppDataContext"].ConnectionString))
                 {
                    await sqlConnection.OpenAsync();
                    try
                    {
                        if (psp == "پارسیان")
                        {
                            using (SqlCommand command =
                                new SqlCommand("DELETE FROM  NormalRep where PSP  =N'پارسیان'", sqlConnection))
                            {
                                command.ExecuteNonQuery();
                            }
                        } 
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    //می شود Map  در مرحله آخر اطلاعات اطلاعات جدول سی شارپ به جدول دیتابیس
                    using (var transaction = sqlConnection.BeginTransaction())
                    {
                        using (var sqlBulkCopy =
                            new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.KeepIdentity, transaction))
                        {
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("TerminalNum", "TerminalNum"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("SerialDevice",
                                "SerialDevice"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("OwnerDevice", "OwnerDevice"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("PSP", "PSP"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Representative",
                                "Representative"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("AcceptorNo", "AcceptorNo"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ContractNo", "ContractNo"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Market", "Market"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("AccountNo", "AccountNo"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ShebaNo", "ShebaNo"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CustomerNo", "CustomerNo"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("BranchId", "BranchId"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Ostan", "Ostan"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("City1", "City1"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("DeviceType1", "DeviceType1"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("DeviceTypeId",
                                "DeviceTypeId"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CardType1", "CardType1"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Marker", "Marker"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("NationalCode",
                                "NationalCode"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("StoreManager",
                                "StoreManager"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Statuse", "Statuse"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Class", "Class"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Address", "Address"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("TelCode", "TelCode"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Mobile", "Mobile"));
                            sqlBulkCopy.BatchSize = 33000;
                            sqlBulkCopy.BulkCopyTimeout = 30000;
                            sqlBulkCopy.DestinationTableName =
                                $"[{_dataContext.Database.Connection.Database}].[dbo].[NormalRep]";
                            try
                            {
                                await sqlBulkCopy.WriteToServerAsync(NormalParsianReDataTable);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                                transaction.Rollback();
                            }
                            transaction.Commit();
                        }
                    }
                }
                return JsonSuccessMessage("فرآیند وارد نمودن اطلاعات  از طریق فایل با موفقیت انجام شد.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return JsonErrorMessage("فرآیند وارد نمودن اطلاعات  از طریق فایل  انجام نشد.");
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> ImportSerriall(HttpPostedFileBase file4, CancellationToken cancellationToken)
        {
            try
            {
                if (!file4.IsValidFormat(".xlsx"))
                {
                    return JsonWarningMessage("تنها فایل با پسوند .xlsx مجاز می باشد.");
                }
                DataTable SeriallTable = new DataTable();
                SeriallTable.Columns.Add(new DataColumn("SerialNo", typeof(string)));
                SeriallTable.Columns.Add(new DataColumn("Owner", typeof(string)));
                SeriallTable.Columns.Add(new DataColumn("Status", typeof(string)));
                SeriallTable.Columns.Add(new DataColumn("DeviceType", typeof(string)));
                SeriallTable.Columns.Add(new DataColumn("DeviceModel", typeof(string)));
                using (var package = new ExcelPackage(file4.InputStream))
                {
                    var workSheet = package.Workbook.Worksheets.First();
                    var totalNumberOfRowsWithoutHeader = workSheet.Dimension.End.Row - 1;
                    for (var rowNumber = 2; rowNumber <= workSheet.Dimension.End.Row; rowNumber++)
                    {
                        var row = workSheet.Cells[rowNumber, 1, rowNumber, workSheet.Dimension.End.Column];
                        var SeriallDataRow = SeriallTable.NewRow();

                        SeriallDataRow["SerialNo"] = row[rowNumber, 1].Text; //سریال دستگاه 
                        SeriallDataRow["Owner"] = row[rowNumber, 2].Text; //مالک دستگاه 
                        SeriallDataRow["Status"] = row[rowNumber, 3].Text; //وضعیت
                        SeriallDataRow["DeviceType"] = row[rowNumber, 4].Text; //نوع دستگاه 
                        SeriallDataRow["DeviceModel"] = row[rowNumber, 5].Text; //مدل دستگاه 

                        SeriallTable.Rows.Add(SeriallDataRow);
                    }
                }
                using (var sqlConnection =
                    new SqlConnection(ConfigurationManager.ConnectionStrings["AppDataContext"].ConnectionString))
                {
                    await sqlConnection.OpenAsync(cancellationToken);
                    using (var transaction = sqlConnection.BeginTransaction())
                    {
                        using (var sqlBulkCopy =
                            new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.KeepIdentity, transaction))
                        {
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("SerialNo", "SerialNo"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Owner", "Owner"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Status", "Status"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("DeviceType", "DeviceType"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("DeviceModel", "DeviceModel"));
                            sqlBulkCopy.BatchSize = 10000;
                            sqlBulkCopy.BulkCopyTimeout = 10000;
                            sqlBulkCopy.DestinationTableName =
                                $"[{_dataContext.Database.Connection.Database}].[dbo].[Seriall]";
                            try
                            {
                                await sqlBulkCopy.WriteToServerAsync(SeriallTable, cancellationToken);
                            }
                            catch
                            {
                                transaction.Rollback();
                            }

                            transaction.Commit();
                        }
                    }
                }
                return JsonSuccessMessage("فرآیند وارد نمودن اطلاعات سریال دستگاه ها از طریق فایل با موفقیت انجام شد.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return JsonErrorMessage("فرآیند وارد نمودن اطلاعات  از طریق فایل  انجام نشد.");
            }
        }
        /////////////////////////////////////////////////////////////////////
        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> ImportDeviceModel(HttpPostedFileBase file5, CancellationToken cancellationToken)
        {
            try
            {
                if (!file5.IsValidFormat(".xlsx"))
                {
                    return JsonWarningMessage("تنها فایل با پسوند .xlsx مجاز می باشد.");
                }
                DataTable Device_CardTable = new DataTable();
                Device_CardTable.Columns.Add(new DataColumn("TypeDevice", typeof(string)));
                Device_CardTable.Columns.Add(new DataColumn("TypeCard", typeof(string)));
                using (var package = new ExcelPackage(file5.InputStream))
                {
                    var workSheet = package.Workbook.Worksheets.First();
                    var totalNumberOfRowsWithoutHeader = workSheet.Dimension.End.Row - 1;
                    for (var rowNumber = 2; rowNumber <= workSheet.Dimension.End.Row; rowNumber++)
                    {
                        var row = workSheet.Cells[rowNumber, 1, rowNumber, workSheet.Dimension.End.Column];
                        var DeviceCardDataRow = Device_CardTable.NewRow();

                        DeviceCardDataRow["TypeDevice"] = row[rowNumber, 1].Text;
                        DeviceCardDataRow["TypeCard"] = row[rowNumber, 2].Text;
                        Device_CardTable.Rows.Add(DeviceCardDataRow);
                    }
                }
                using (var sqlConnection =
                    new SqlConnection(ConfigurationManager.ConnectionStrings["AppDataContext"].ConnectionString))
                {
                    await sqlConnection.OpenAsync(cancellationToken);
                    using (var transaction = sqlConnection.BeginTransaction())
                    {
                        using (var sqlBulkCopy =
                            new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.KeepIdentity, transaction))
                        {
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("TypeDevice", "TypeDevice"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("TypeCard", "TypeCard"));
                            sqlBulkCopy.BatchSize = 10000;
                            sqlBulkCopy.BulkCopyTimeout = 10000;
                            sqlBulkCopy.DestinationTableName =
                                $"[{_dataContext.Database.Connection.Database}].[dbo].[Device_Card]";
                            try
                            {
                                await sqlBulkCopy.WriteToServerAsync(Device_CardTable, cancellationToken);
                            }
                            catch
                            {
                                transaction.Rollback();
                            }

                            transaction.Commit();
                        }
                    }
                }
                return JsonSuccessMessage("فرآیند وارد نمودن اطلاعات  دستگاه ها از طریق فایل با موفقیت انجام شد.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return JsonErrorMessage("فرآیند وارد نمودن اطلاعات  از طریق فایل  انجام نشد.");
            }
        }
        ///////////////////////////////////////////////////////////////////////
        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> ImportMarketer(HttpPostedFileBase file6, CancellationToken cancellationToken)
        {
            try
            {
                if (!file6.IsValidFormat(".xlsx"))
                {
                    return JsonWarningMessage("تنها فایل با پسوند .xlsx مجاز می باشد.");
                }
                DataTable MarketTable = new DataTable();
                MarketTable.Columns.Add(new DataColumn("MarkerterBy", typeof(string)));
                MarketTable.Columns.Add(new DataColumn("TypeMarkerter", typeof(string)));
                using (var package = new ExcelPackage(file6.InputStream))
                {
                    var workSheet = package.Workbook.Worksheets.First();
                    var totalNumberOfRowsWithoutHeader = workSheet.Dimension.End.Row - 1;
                    for (var rowNumber = 2; rowNumber <= workSheet.Dimension.End.Row; rowNumber++)
                    {
                        var row = workSheet.Cells[rowNumber, 1, rowNumber, workSheet.Dimension.End.Column];
                        var MarketDataRow = MarketTable.NewRow();
                        MarketDataRow["MarkerterBy"] = row[rowNumber, 1].Text;
                        MarketDataRow["TypeMarkerter"] = row[rowNumber, 2].Text;
                        MarketTable.Rows.Add(MarketDataRow);
                    }
                }
                using (var sqlConnection =
                    new SqlConnection(ConfigurationManager.ConnectionStrings["AppDataContext"].ConnectionString))
                {
                    await sqlConnection.OpenAsync(cancellationToken);

                    using (var transaction = sqlConnection.BeginTransaction())
                    {
                        using (var sqlBulkCopy =
                            new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.KeepIdentity, transaction))
                        {
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("MarkerterBy", "MarkerterBy"));
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("TypeMarkerter",
                                "TypeMarkerter"));
                            sqlBulkCopy.BatchSize = 10000;
                            sqlBulkCopy.BulkCopyTimeout = 10000;
                            sqlBulkCopy.DestinationTableName =
                                $"[{_dataContext.Database.Connection.Database}].[dbo].[Markerter]";
                            try
                            {
                                await sqlBulkCopy.WriteToServerAsync(MarketTable, cancellationToken);
                            }
                            catch
                            {
                                transaction.Rollback();
                            }
                            transaction.Commit();
                        }
                    }
                }
                return JsonSuccessMessage("فرآیند وارد نمودن اطلاعات  بازاریابی از طریق فایل با موفقیت انجام شد.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return JsonErrorMessage("فرآیند وارد نمودن اطلاعات  از طریق فایل  انجام نشد.");
            }
        }
    }
}