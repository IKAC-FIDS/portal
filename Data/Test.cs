//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace TES.Data
//{
//    internal class Test
//    {
//        public void test()
//        {
//            #region branchRanking

//            var branchRanking = workbook.branchRankings.Add("رتبه بندی شعب");

//            branchRanking.Row(1).Height = 50;
//            var headerbranchRankingRowStyle = branchRanking.Row(1).Style;
//            headerbranchRankingRowStyle.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
//            headerbranchRankingRowStyle.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#022349"));
//            headerbranchRankingRowStyle.Font.Color.SetColor(ColorTranslator.FromHtml("#fff"));
//            headerbranchRankingRowStyle.Font.Bold = true;
//            headerbranchRankingRowStyle.Font.Size = 12;
//            headerbranchRankingRowStyle.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
//            headerbranchRankingRowStyle.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

//            var cellsStyle = branchRanking.Cells.Style;
//            cellsStyle.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
//            cellsStyle.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

//            branchRanking.Column(1).Width = 16;
//            branchRanking.Column(2).Width = 16;
//            branchRanking.Column(3).Width = 16;
//            branchRanking.Column(4).Width = 16;
//            branchRanking.Column(5).Width = 16;
//            branchRanking.Column(6).Width = 16;
//            branchRanking.Column(7).Width = 16;
//            branchRanking.Column(8).Width = 16;
//            branchRanking.Column(9).Width = 16;
//            branchRanking.Column(10).Width = 16;
//            branchRanking.Column(11).Width = 16;
//            branchRanking.Column(12).Width = 16;
//            branchRanking.Column(13).Width = 16;
//            branchRanking.Column(14).Width = 16;
//            branchRanking.Column(15).Width = 16;
//            branchRanking.Column(16).Width = 16;
//            branchRanking.Column(17).Width = 16;
//            branchRanking.Column(18).Width = 16;
//            branchRanking.Column(19).Width = 16;
//            branchRanking.Column(20).Width = 16;
//            branchRanking.Column(21).Width = 16;
//            branchRanking.Column(22).Width = 16;
//            branchRanking.Column(23).Width = 16;
//            branchRanking.Column(24).Width = 16;
//            branchRanking.Column(25).Width = 16;
//            branchRanking.Column(26).Width = 16;
//            branchRanking.Column(27).Width = 16;
//            branchRanking.Column(28).Width = 16;
//            branchRanking.Column(29).Width = 16;
//            branchRanking.Column(30).Width = 16;
//            branchRanking.Column(31).Width = 16;
//            branchRanking.Column(32).Width = 16;
//            branchRanking.Column(33).Width = 16;
//            branchRanking.Column(34).Width = 16;
//            branchRanking.Column(35).Width = 16;
//            branchRanking.Column(36).Width = 16;
//            branchRanking.Column(37).Width = 16;
//            branchRanking.Column(38).Width = 16;
//            branchRanking.Column(39).Width = 16;
//            branchRanking.Column(40).Width = 16;
//            branchRanking.Column(41).Width = 16;
//            branchRanking.Column(42).Width = 16;
//            branchRanking.Column(43).Width = 16;
//            branchRanking.Column(44).Width = 16;
//            branchRanking.Column(45).Width = 16;
//            branchRanking.Column(46).Width = 16;
//            branchRanking.Column(47).Width = 16;
//            branchRanking.Column(48).Width = 16;
//            branchRanking.Column(49).Width = 16;
//            branchRanking.Column(50).Width = 16;
//            branchRanking.Column(51).Width = 16;
//            branchRanking.Column(52).Width = 16;
//            branchRanking.Column(53).Width = 16;
//            branchRanking.Column(54).Width = 16;
//            branchRanking.Column(55).Width = 16;
//            branchRanking.Column(56).Width = 16;
//            branchRanking.Column(57).Width = 16;
//            branchRanking.Column(58).Width = 16;
//            branchRanking.Column(59).Width = 16;
//            branchRanking.Column(60).Width = 16;
//            branchRanking.Column(61).Width = 16;
//            branchRanking.Column(62).Width = 16;
//            branchRanking.Column(63).Width = 16;


//            //branchRanking.Column(55).Width = 10;
//            branchRanking.Cells[1, 1].Value = "ردیف";
//            branchRanking.Cells[1, 2].Value = "شماره ترمينال";
//            branchRanking.Cells[1, 3].Value = "سريال دستگاه";
//            branchRanking.Cells[1, 4].Value = "مالک دستگاه";
//            branchRanking.Cells[1, 5].Value = "psp";
//            branchRanking.Cells[1, 6].Value = "تعداد تراکنش خريد " + GetMonthValue.GetMonth(month) + " 1402";
//            branchRanking.Cells[1, 7].Value = "مبلغ تراکنش خريد " + GetMonthValue.GetMonth(month) + " 1402";
//            branchRanking.Cells[1, 8].Value = "وضعيت تعداد تراکنش " + GetMonthValue.GetMonth(month) + " 1402";
//            branchRanking.Cells[1, 9].Value = "وضعيت مبلغ تراکنش " + GetMonthValue.GetMonth(month) + " 1402";
//            branchRanking.Cells[1, 10].Value = "وضعيت فعاليت دستورالعمل " + GetMonthValue.GetMonth(month) + " 1402";
//            branchRanking.Cells[1, 11].Value = "وضعيت فعاليت شاپرکي " + GetMonthValue.GetMonth(month) + " 1402";
//            branchRanking.Cells[1, 12].Value = "تعداد تراکنش قبض " + GetMonthValue.GetMonth(month) + " 1402";
//            branchRanking.Cells[1, 13].Value = "جمع مبلغ تراکنش قبض " + GetMonthValue.GetMonth(month) + " 1402";
//            branchRanking.Cells[1, 14].Value = "تعداد تراکنش شارژ " + GetMonthValue.GetMonth(month) + " 1402";
//            branchRanking.Cells[1, 15].Value = "جمع مبلغ تراکنش شارژ " + GetMonthValue.GetMonth(month) + " 1402";
//            branchRanking.Cells[1, 16].Value = "تعداد مانده گيري " + GetMonthValue.GetMonth(month) + " 1402";
//            branchRanking.Cells[1, 17].Value = "تعداد تراکنش خريد " + GetMonthValue.GetMonth(month - 1) + " 1402";
//            branchRanking.Cells[1, 18].Value = "مبلغ تراکنش خريد " + GetMonthValue.GetMonth(month - 1) + " 1402";
//            branchRanking.Cells[1, 19].Value = "وضعيت تعداد تراکنش " + GetMonthValue.GetMonth(month - 1) + " 1402";
//            branchRanking.Cells[1, 20].Value = "وضعيت مبلغ تراکنش " + GetMonthValue.GetMonth(month - 1) + " 1402";
//            branchRanking.Cells[1, 21].Value = "وضعيت فعاليت دستورالعمل " + GetMonthValue.GetMonth(month - 1) + " 1402";
//            branchRanking.Cells[1, 22].Value = "وضعيت فعاليت شاپرکي " + GetMonthValue.GetMonth(month - 1) + " 1402";
//            branchRanking.Cells[1, 23].Value = "تعداد تراکنش خريد " + GetMonthValue.GetMonth(month - 2) + " 1402";
//            branchRanking.Cells[1, 24].Value = "مبلغ تراکنش خريد " + GetMonthValue.GetMonth(month - 2) + " 1402";
//            branchRanking.Cells[1, 25].Value = "وضعيت تعداد تراکنش " + GetMonthValue.GetMonth(month - 2) + " 1402";
//            branchRanking.Cells[1, 26].Value = "وضعيت مبلغ تراکنش " + GetMonthValue.GetMonth(month - 2) + " 1402";
//            branchRanking.Cells[1, 27].Value = "وضعيت فعاليت دستورالعمل " + GetMonthValue.GetMonth(month - 2) + " 1402";
//            branchRanking.Cells[1, 28].Value = "وضعيت فعاليت شاپرکي " + GetMonthValue.GetMonth(month - 2) + " 1402";
//            branchRanking.Cells[1, 29].Value = "تعداد تراکنش خريد " + GetMonthValue.GetMonth(month - 3) + " 1402";
//            branchRanking.Cells[1, 30].Value = "مبلغ تراکنش خريد " + GetMonthValue.GetMonth(month - 3) + " 1402";
//            branchRanking.Cells[1, 31].Value = "وضعيت تعداد تراکنش " + GetMonthValue.GetMonth(month - 3) + " 1402";
//            branchRanking.Cells[1, 32].Value = "وضعيت مبلغ تراکنش " + GetMonthValue.GetMonth(month - 3) + " 1402";
//            branchRanking.Cells[1, 33].Value = "وضعيت فعاليت دستورالعمل " + GetMonthValue.GetMonth(month - 3) + " 1402";
//            branchRanking.Cells[1, 34].Value = "وضعيت فعاليت شاپرکي " + GetMonthValue.GetMonth(month - 3) + " 1402";
//            branchRanking.Cells[1, 35].Value = "شماره پذيرنده";
//            branchRanking.Cells[1, 36].Value = "شماره قرارداد";
//            branchRanking.Cells[1, 37].Value = "فروشگاه";
//            branchRanking.Cells[1, 38].Value = "شماره حساب";
//            branchRanking.Cells[1, 39].Value = "شبا";
//            branchRanking.Cells[1, 40].Value = "شماره مشتري";
//            branchRanking.Cells[1, 41].Value = "کد شعبه";
//            branchRanking.Cells[1, 42].Value = "نام شعبه";
//            branchRanking.Cells[1, 43].Value = "منطقه شعبه";
//            branchRanking.Cells[1, 44].Value = "استان بانک مرکزي";
//            branchRanking.Cells[1, 45].Value = "استان";
//            branchRanking.Cells[1, 46].Value = "شهر";
//            branchRanking.Cells[1, 47].Value = "نوع دستگاه";
//            branchRanking.Cells[1, 48].Value = "مدل دستگاه";
//            branchRanking.Cells[1, 49].Value = "بازاريابي توسط";
//            branchRanking.Cells[1, 50].Value = "کد ملي";
//            branchRanking.Cells[1, 51].Value = "نام";
//            branchRanking.Cells[1, 52].Value = "نام خانوادگي";
//            branchRanking.Cells[1, 53].Value = "مدير فروشکاه";
//            branchRanking.Cells[1, 54].Value = "وضعيت";
//            branchRanking.Cells[1, 55].Value = "صنف";
//            branchRanking.Cells[1, 56].Value = "آدرس فروشگاه";
//            branchRanking.Cells[1, 57].Value = "تلفن فروشگاه";
//            branchRanking.Cells[1, 58].Value = "موبايل";
//            branchRanking.Cells[1, 59].Value = "تاريخ درخواست";
//            branchRanking.Cells[1, 60].Value = "کد باز";
//            branchRanking.Cells[1, 61].Value = "تاريخ نصب";
//            branchRanking.Cells[1, 62].Value = "ماه نصب";
//            branchRanking.Cells[1, 63].Value = "تاريخ ابطال";
//            branchRanking.Cells[1, 64].Value = "ماه ابطال";

//            var rowNumber = 2;

//            foreach (var item in data)
//            {
//                branchRanking.Cells[rowNumber, 1].Value = rowNumber - 1;
//                branchRanking.Cells[rowNumber, 2].Value = item.TerminalNo;
//                branchRanking.Cells[rowNumber, 3].Value = item.DeviceSerial;
//                branchRanking.Cells[rowNumber, 4].Value = item.DeviceOwner;
//                branchRanking.Cells[rowNumber, 5].Value = item.PSP;
//                branchRanking.Cells[rowNumber, 6].Value = item.CountTransactionBuyCurrentMonth;
//                branchRanking.Cells[rowNumber, 7].Value = item.AmountTransactionBuyCurrentMonth;
//                branchRanking.Cells[rowNumber, 8].Value = item.StatusCountTransactionCurrentMonth;
//                branchRanking.Cells[rowNumber, 9].Value = item.StatusAmountTransactionCurrentMonth;
//                branchRanking.Cells[rowNumber, 10].Value = item.StatusActivityInstructionsCurrentMonth;
//                branchRanking.Cells[rowNumber, 11].Value = item.StatusActivityShaparakCurrentMonth;
//                branchRanking.Cells[rowNumber, 12].Value = item.CountTransactionBillCurrentMonth;
//                branchRanking.Cells[rowNumber, 13].Value = item.SumAmountTransactionBillCurrentMonth;
//                branchRanking.Cells[rowNumber, 14].Value = item.CountChargeTransactionCurrentMonth;
//                branchRanking.Cells[rowNumber, 15].Value = item.SumAmountChargeTransactionCurrentMonth;
//                branchRanking.Cells[rowNumber, 16].Value = item.CountBalanceCurrentMonth;
//                branchRanking.Cells[rowNumber, 17].Value = item.CountTransactionBuyMonth_1;
//                branchRanking.Cells[rowNumber, 18].Value = item.AmountTransactionBuyMonth_1;
//                branchRanking.Cells[rowNumber, 19].Value = item.StatusCountTransactionMonth_1;
//                branchRanking.Cells[rowNumber, 20].Value = item.StatusAmountTransactionMonth_1;
//                branchRanking.Cells[rowNumber, 21].Value = item.StatusActivityInstructionsMonth_1;
//                branchRanking.Cells[rowNumber, 22].Value = item.StatusActivityShaparakMonth_1;
//                branchRanking.Cells[rowNumber, 23].Value = item.CountTransactionBuyMonth_2;
//                branchRanking.Cells[rowNumber, 24].Value = item.AmountTransactionBuyMonth_2;
//                branchRanking.Cells[rowNumber, 25].Value = item.StatusCountTransactionMonth_2;
//                branchRanking.Cells[rowNumber, 26].Value = item.StatusAmountTransactionMonth_2;
//                branchRanking.Cells[rowNumber, 27].Value = item.StatusActivityInstructionsMonth_2;
//                branchRanking.Cells[rowNumber, 28].Value = item.StatusActivityShaparakMonth_2;
//                branchRanking.Cells[rowNumber, 29].Value = item.CountTransactionBuyMonth_3;
//                branchRanking.Cells[rowNumber, 30].Value = item.AmountTransactionBuyMonth_3;
//                branchRanking.Cells[rowNumber, 31].Value = item.StatusCountTransactionMonth_3;
//                branchRanking.Cells[rowNumber, 32].Value = item.StatusAmountTransactionMonth_3;
//                branchRanking.Cells[rowNumber, 33].Value = item.StatusActivityInstructionsMonth_3;
//                branchRanking.Cells[rowNumber, 34].Value = item.StatusActivityShaparakMonth_3;
//                branchRanking.Cells[rowNumber, 35].Value = item.MerchandNumber;
//                branchRanking.Cells[rowNumber, 36].Value = item.ContractNumber;
//                branchRanking.Cells[rowNumber, 37].Value = item.Store;
//                branchRanking.Cells[rowNumber, 38].Value = item.AccountNumber;
//                branchRanking.Cells[rowNumber, 39].Value = item.Sheba;
//                branchRanking.Cells[rowNumber, 40].Value = item.CustomerNumber;
//                branchRanking.Cells[rowNumber, 41].Value = item.BranchCode;
//                branchRanking.Cells[rowNumber, 42].Value = item.BranchName;
//                branchRanking.Cells[rowNumber, 43].Value = item.BranchArea;
//                branchRanking.Cells[rowNumber, 44].Value = item.CentralBankProvince;
//                branchRanking.Cells[rowNumber, 45].Value = item.State;
//                branchRanking.Cells[rowNumber, 46].Value = item.City;
//                branchRanking.Cells[rowNumber, 47].Value = item.DeviceType;
//                branchRanking.Cells[rowNumber, 48].Value = item.DeviceModel;
//                branchRanking.Cells[rowNumber, 49].Value = item.MarketingBy;
//                branchRanking.Cells[rowNumber, 50].Value = item.NationalCode;
//                branchRanking.Cells[rowNumber, 51].Value = item.Name;
//                branchRanking.Cells[rowNumber, 52].Value = item.Family;
//                branchRanking.Cells[rowNumber, 53].Value = item.StoreManager;
//                branchRanking.Cells[rowNumber, 54].Value = item.Status;
//                branchRanking.Cells[rowNumber, 55].Value = item.business;
//                branchRanking.Cells[rowNumber, 56].Value = item.StoreAddress;
//                branchRanking.Cells[rowNumber, 57].Value = item.StoreTel;
//                branchRanking.Cells[rowNumber, 58].Value = item.Mobile;
//                branchRanking.Cells[rowNumber, 59].Value = item.RequestDate;
//                branchRanking.Cells[rowNumber, 60].Value = item.OpenCode;
//                branchRanking.Cells[rowNumber, 61].Value = item.InstallDate;
//                branchRanking.Cells[rowNumber, 62].Value = item.InstallMonth;
//                branchRanking.Cells[rowNumber, 63].Value = item.CancellationDate;
//                branchRanking.Cells[rowNumber, 64].Value = item.CancellationMonth;
//                rowNumber++;

//            }


//            #endregion
//        }
//    }
//}
