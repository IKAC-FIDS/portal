using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using EntityFramework.Extensions;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using Serilog.Formatting.Json;
using StructureMap.Building.Interception;
using TES.Data;
using TES.Data.Domain;
using TES.Merchant.Web.UI.Service.Models;
using TES.Merchant.Web.UI.Service.Models.Parsian.NewModels;
using TES.Merchant.Web.UI.ViewModels.PardakhtNovin;

namespace TES.Merchant.Web.UI.Service
{
    public class PardakhtNovinService : IDisposable
    {
        private const string BaseAddress = "https://jamservice.pna.co.ir/services/api/BankService";

        private const string UserName = "sarmayehbank";
    //    private const string Password = "147852";
        private const string Password = "Aa@123456";

        //کاربری:sarmayehbank
        //پسورد:Aa@123456

        public PardakhtNovinService()
        {
        }

        public AuthenticationHeaderValue AuthorizationHeader()
        {
            return
                new AuthenticationHeaderValue(
                    "Basic", Convert.ToBase64String(
                        Encoding.ASCII.GetBytes(
            $"{UserName}:{Password}")));

            //request.Headers.Add(System.Net.HttpRequestHeader.Authorization,
            //Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "username", "password"))));
        }

        //2-4
        public AddFileResponse AddFile(AddFileRequest input,long terminalId)
        {
            AddFileResponse methodResponse;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");
                httpClient.DefaultRequestHeaders.Authorization = AuthorizationHeader();
                using (var response = httpClient.PostAsync(BaseAddress + "/AddFile", content))
                {
                    response.Result.EnsureSuccessStatusCode();
                    var apiResponse = response.Result.Content.ReadAsStringAsync().Result;

                    methodResponse = JsonConvert.DeserializeObject<AddFileResponse>(apiResponse);
                }
            }

            var pardakhtNovinRequest = new PardakhtNovinRequest
            {
                Input = JsonConvert.SerializeObject(input),
                Method = "AddFile",
                TerminalId = terminalId,
                Module = "-",
                Result = JsonConvert.SerializeObject(methodResponse),
                TrackId = DateTime.Now.Ticks
            };
            var datacontext = new AppDataContext();
            datacontext.PardakhtNovinRequests.Add(pardakhtNovinRequest);
            datacontext.SaveChanges();

            return methodResponse;
        }


        //2-5
        public GetCustomerByCodeResponse GetCustomerByCode(GetCustomerByCodeRequest input)
        {
            GetCustomerByCodeResponse methodResponse;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");
                httpClient.DefaultRequestHeaders.Authorization = AuthorizationHeader();
                using (var response = httpClient.PostAsync(BaseAddress + "/GetCustomerByCode", content))
                {
                    response.Result.EnsureSuccessStatusCode();
                    var apiResponse = response.Result.Content.ReadAsStringAsync().Result;

                    methodResponse = JsonConvert.DeserializeObject<GetCustomerByCodeResponse>(apiResponse);
                }
            }

            var pardakhtNovinRequest = new PardakhtNovinRequest
            {
                Input = JsonConvert.SerializeObject(input),
                Method = "GetCustomerByCode",
                Module = "-",
                TrackId = DateTime.Now.Ticks
            };
            var datacontext = new AppDataContext();
            datacontext.PardakhtNovinRequests.Add(pardakhtNovinRequest);
            datacontext.SaveChanges();
            return methodResponse;
        }

        //2-6
        public AddNewCustomerResponse AddNewCustomer(AddNewCustomerRequest input , long terminalId)
        {
            AddNewCustomerResponse methodResponse;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");
                httpClient.DefaultRequestHeaders.Authorization = AuthorizationHeader();
                using (var response = httpClient.PostAsync(BaseAddress + "/AddNewCustomer", content))
                {
                    response.Result.EnsureSuccessStatusCode();
                    var apiResponse = response.Result.Content.ReadAsStringAsync().Result;

                    methodResponse = JsonConvert.DeserializeObject<AddNewCustomerResponse>(apiResponse);
                }
            }

            var pardakhtNovinRequest = new PardakhtNovinRequest
            {
                Input = JsonConvert.SerializeObject(input),
                Method = "AddNewCustomer",
                Module = "-",
                TrackId = DateTime.Now.Ticks ,
                TerminalId =  terminalId,
                Result  = JsonConvert.SerializeObject(methodResponse),
            };
            var datacontext = new AppDataContext();
            datacontext.PardakhtNovinRequests.Add(pardakhtNovinRequest);
            datacontext.SaveChanges();
            return methodResponse;
        }

        //2-7
        public AddNewCustomerResponse UpdateCustomer(UpdateCustomerRequest input, long terminalId)
        {
            AddNewCustomerResponse methodResponse;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");
                httpClient.DefaultRequestHeaders.Authorization = AuthorizationHeader();
                using (var response = httpClient.PostAsync(BaseAddress + "/UpdateCustomer", content))
                {
                    response.Result.EnsureSuccessStatusCode();
                    var apiResponse = response.Result.Content.ReadAsStringAsync().Result;

                    methodResponse = JsonConvert.DeserializeObject<AddNewCustomerResponse>(apiResponse);
                }
            }

            var pardakhtNovinRequest = new PardakhtNovinRequest
            {
                Input = JsonConvert.SerializeObject(input),
                Method = "UpdateCustomer",
                Module = "-",
                Result = JsonConvert.SerializeObject(methodResponse),
                TerminalId = terminalId,
                TrackId = DateTime.Now.Ticks
            };
            var datacontext = new AppDataContext();
            datacontext.PardakhtNovinRequests.Add(pardakhtNovinRequest);
            datacontext.SaveChanges();
            return methodResponse;
        }


        //2-8 
        public GetBankResponse GetBank(GetBankRequest input)
        {
            GetBankResponse methodResponse;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");
                httpClient.DefaultRequestHeaders.Authorization = AuthorizationHeader();


                using (var response = httpClient.PostAsync(BaseAddress + "/GetBanks", content))
                {
                    response.Result.EnsureSuccessStatusCode();
                    var apiResponse = response.Result.Content.ReadAsStringAsync().Result;
                    methodResponse = JsonConvert.DeserializeObject<GetBankResponse>(apiResponse);
                }
            }

            var pardakhtNovinRequest = new PardakhtNovinRequest
            {
                Input = JsonConvert.SerializeObject(input),
                Method = "GetBank",
                Module = "-",
                Result = JsonConvert.SerializeObject(methodResponse),
                TrackId = DateTime.Now.Ticks
            };
            var datacontext = new AppDataContext();
            datacontext.PardakhtNovinRequests.Add(pardakhtNovinRequest);
            datacontext.SaveChanges();
            return methodResponse;
        }


        //2-9
        public GetBranchListResponse GetBranchList(GetBranchListRequest input)
        {
            GetBranchListResponse methodResponse;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");
                httpClient.DefaultRequestHeaders.Authorization = AuthorizationHeader();
                using (var response = httpClient.PostAsync(BaseAddress + "/GetBranchList", content))
                {
                    response.Result.EnsureSuccessStatusCode();
                    var apiResponse = response.Result.Content.ReadAsStringAsync().Result;

                    methodResponse = JsonConvert.DeserializeObject<GetBranchListResponse>(apiResponse);
                }
            }

            var pardakhtNovinRequest = new PardakhtNovinRequest
            {
                Input = JsonConvert.SerializeObject(input),
                Method = "GetBranchList",
                Module = "-",
                TrackId = DateTime.Now.Ticks
            };
            var datacontext = new AppDataContext();
            datacontext.PardakhtNovinRequests.Add(pardakhtNovinRequest);
            datacontext.SaveChanges();
            return methodResponse;
        }

        //2-10
        public GetGuildsResponse GetGuilds(GetGuildsRequest input)
        {
            GetGuildsResponse methodResponse;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");
                httpClient.DefaultRequestHeaders.Authorization = AuthorizationHeader();
                using (var response = httpClient.PostAsync(BaseAddress + "/GetGuilds", content))
                {
                    response.Result.EnsureSuccessStatusCode();
                    var apiResponse = response.Result.Content.ReadAsStringAsync().Result;

                    methodResponse = JsonConvert.DeserializeObject<GetGuildsResponse>(apiResponse);
                }
            }

            var pardakhtNovinRequest = new PardakhtNovinRequest
            {
                Input = JsonConvert.SerializeObject(input),
                Method = "GetBranchList",
                Module = "-",
                TrackId = DateTime.Now.Ticks
            };
            var datacontext = new AppDataContext();
            datacontext.PardakhtNovinRequests.Add(pardakhtNovinRequest);
            datacontext.SaveChanges();
            return methodResponse;
        }

        //2-15
        public AddNewRequestResponse AddNewRequest(AddNewRequestRequestWithDocs input,long terminalId)
        {
            input.Data.GuildSupplementaryCode = input.Data.GuildSupplementaryCode.PadLeft(8,'0');
            AddNewRequestResponse methodResponse;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");
                httpClient.DefaultRequestHeaders.Authorization = AuthorizationHeader();
                using (var response = httpClient.PostAsync(BaseAddress + "/AddNewRequest", content))
                {
                    response.Result.EnsureSuccessStatusCode();
                    var apiResponse = response.Result.Content.ReadAsStringAsync().Result;

                    methodResponse = JsonConvert.DeserializeObject<AddNewRequestResponse>(apiResponse);
                }
            }

            var pardakhtNovinRequest = new PardakhtNovinRequest
            {
                Input = JsonConvert.SerializeObject(input),
                Method = "AddNewRequest",
                Module = "-",
                TerminalId = terminalId,
                Result = JsonConvert.SerializeObject(methodResponse),
                TrackId = DateTime.Now.Ticks
            };
            var datacontext = new AppDataContext();
            datacontext.PardakhtNovinRequests.Add(pardakhtNovinRequest);
            datacontext.SaveChanges();
            return methodResponse;
        }
        public AddNewRequestResponse AddNewRequest(AddNewRequestRequest input,long terminalId)
        {
            AddNewRequestResponse methodResponse;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");
                httpClient.DefaultRequestHeaders.Authorization = AuthorizationHeader();
                using (var response = httpClient.PostAsync(BaseAddress + "/AddNewRequest", content))
                {
                    response.Result.EnsureSuccessStatusCode();
                    var apiResponse = response.Result.Content.ReadAsStringAsync().Result;

                    methodResponse = JsonConvert.DeserializeObject<AddNewRequestResponse>(apiResponse);
                }
            }

            var pardakhtNovinRequest = new PardakhtNovinRequest
            {
                Input = JsonConvert.SerializeObject(input),
                Method = "AddNewRequest",
                Module = "-",
                TerminalId = terminalId,
                Result = JsonConvert.SerializeObject(methodResponse),
                TrackId = DateTime.Now.Ticks
            };
            var datacontext = new AppDataContext();
            datacontext.PardakhtNovinRequests.Add(pardakhtNovinRequest);
            datacontext.SaveChanges();
            return methodResponse;
        }

        //2-16
        public GetRequestDetailByFollowupCodeResponse GetRequestDetailByFollowupCode(
            GetRequestDetailByFollowupCodeRequest input, long terminalId)
        {
            GetRequestDetailByFollowupCodeResponse methodResponse;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");
                httpClient.DefaultRequestHeaders.Authorization = AuthorizationHeader();
                using (var response = httpClient.PostAsync(BaseAddress + "/GetRequestDetailByFollowupCode", content))
                {
                    response.Result.EnsureSuccessStatusCode();
                    var apiResponse = response.Result.Content.ReadAsStringAsync().Result;

                    methodResponse = JsonConvert.DeserializeObject<GetRequestDetailByFollowupCodeResponse>(apiResponse);
                }
            }

            var pardakhtNovinRequest = new PardakhtNovinRequest
            {
                Input = JsonConvert.SerializeObject(input),
                Method = "GetRequestDetailByFollowupCode",
                Result = JsonConvert.SerializeObject(methodResponse),
                TerminalId = terminalId,
                Module = "-",
                TrackId = DateTime.Now.Ticks
            };
            var datacontext = new AppDataContext();
            datacontext.PardakhtNovinRequests.Add(pardakhtNovinRequest);
            datacontext.SaveChanges();
            return methodResponse;
        }

        //2-17
        public GetRequestListResponse GetRequestList(GetRequestListRequest input)
        {
            GetRequestListResponse methodResponse;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");
                httpClient.DefaultRequestHeaders.Authorization = AuthorizationHeader();
                using (var response = httpClient.PostAsync(BaseAddress + "/GetRequestList", content))
                {
                    response.Result.EnsureSuccessStatusCode();
                    var apiResponse = response.Result.Content.ReadAsStringAsync().Result;

                    methodResponse = JsonConvert.DeserializeObject<GetRequestListResponse>(apiResponse);
                }
            }

            var pardakhtNovinRequest = new PardakhtNovinRequest
            {
                Input = JsonConvert.SerializeObject(input),
                Method = "GetRequestList",
                Module = "-",
                TrackId = DateTime.Now.Ticks
            };
            var datacontext = new AppDataContext();
            datacontext.PardakhtNovinRequests.Add(pardakhtNovinRequest);
            datacontext.SaveChanges();
            return methodResponse;
        }

        public UpdateRequestByFollowUpCodeResponse EditAcceptor(long terminalId)
        {
              var result = new UpdateRequestByFollowUpCodeResponse();
            AddNewCustomerRequestDocs docs = new AddNewCustomerRequestDocs();
 
            using (var dataContext = new AppDataContext())
            {
                var terminal = dataContext.Terminals.Include(a=>a.TerminalDocuments).Include(a=>a.MerchantProfile)
                    .Include(a=>a.MerchantProfile.MerchantProfileDocuments).FirstOrDefault(a => a.Id == terminalId);


                var am = GetRequestDetailByFollowupCode(new GetRequestDetailByFollowupCodeRequest()
                {
                    Parameters = new GetRequestDetailByFollowupCodetParameters()
                    {
                        FollowupCode = terminal.FollowupCode,
                        BankFollowupCode = "51"
                    }
                },terminal.Id);
                 foreach (var terminalDocument in terminal.TerminalDocuments)
                {
                    
                        AddFileRequest addFileRequest = new AddFileRequest();
                        addFileRequest.FileName = terminalDocument.FileName;
                        addFileRequest.BinaryDataBase64 = Convert.ToBase64String(terminalDocument.FileData);
                        var addFileResponse = AddFile(addFileRequest,terminal.Id);
                        if (addFileResponse.Status == PardakthNovinStatus.Successed)
                        {
                            var m = UpdateDocument( new UpdateDocumentRequest()
                            {
                                Data = new UpdateDocumentRequestData()
                                {
                                    DocumentAttachment =  addFileResponse.FileServerID.ToString()
                                },
                                Parameters = new UpdateDocumentRequestParameters()
                                {
                                    RequestID = am.Data.RequestID,
                                    DocumentTypeID =  GetPardakhNovinDocTypeId(terminalDocument.DocumentTypeId)
                                }
                            }, terminal.Id );

                        }
                   
                }

                foreach (var terminalDocument in terminal.MerchantProfile.MerchantProfileDocuments)
                {
                    if (  terminalDocument.DocumentTypeId != 1  && 
                        terminalDocument.DocumentTypeId != 11 && terminalDocument.DocumentTypeId != 14   
                       )
                    {
                        AddFileRequest addFileRequest = new AddFileRequest();
                        addFileRequest.FileName = terminalDocument.FileName;
                        addFileRequest.BinaryDataBase64 = Convert.ToBase64String(terminalDocument.FileData);
                        var addFileResponse = AddFile(addFileRequest,terminal.Id);
                        if (addFileResponse.Status == PardakthNovinStatus.Successed)
                        {
                            
                            var m = UpdateDocument( new UpdateDocumentRequest()
                            {
                                Data = new UpdateDocumentRequestData()
                                {
                                    DocumentAttachment =  addFileResponse.FileServerID.ToString()
                                },
                                Parameters = new UpdateDocumentRequestParameters()
                                {
                                    RequestID = am.Data.RequestID ,
                                    DocumentTypeID =  GetPardakhNovinDocTypeId(terminalDocument.DocumentTypeId)
                                }
                            },terminal.Id );

                        }
                    }
                }
             
      
               
                
                var request = UpdateRequestByFollowUpCode( new UpdateRequestByFollowUpCodeRequest
                {

                    // Childs =docs2.Data.Any( )?  new List<AddNewRequestRequestDataDocs>()
                    // {
                    //    docs2
                    // } : new List<AddNewRequestRequestDataDocs>(),
                    Childs  = new List<UpdateRequestByFollowUpCodeRequestaChilds>()
                    {
                        new SubCustomerChild()
                        {
                            Data = new List<SubCustomerData>()
                            {
                                new SubCustomerData()
                                {
                                    CustomerID = terminal.MerchantProfile.PardakhtNovinCustomerId.Value
                                }
                            }
                        },
                        new RequestAccounts()
                        {
                            Data = new List<RequestAccountsData>()
                            {
                                new RequestAccountsData()
                                {
                                    CustomerID = terminal.MerchantProfile.PardakhtNovinCustomerId.Value,
                                    AccountNumber = terminal.AccountNo,
                                    BranchCode = terminal.BranchId.ToString(),
                                    BankID = 25,
                                    SharingPercent = 100,
                                    AccountShabaCode = terminal.ShebaNo
                                    
                                    
                                }
                            }
                        }
                    },
                    Parameters =  new UpdateRequestByFollowUpCodeRequestParameters
                    {
                        FollowupCode = terminal.FollowupCode,
                        BankFollowupCode = "51"
                        
                    },
                    Data = new  UpdateRequestByFollowUpCodeRequestData() 
                    {
                        
                        
                  //       PortType = "POS",
                  //       PosType = getPosTyhpe(terminal.DeviceTypeId),
                  //      
                  //     BankFollowupCode = "51",
                       AccountNumber = terminal.AccountNo,
                  //  //     ActivityLicenseNumberReferenceName = "Test",
                  //    //   ActivityLicenseNumber = "1234567",
                  //     //  BusinessLicenseEndDate=
                  //     BankID = 25,
                  //     TrustKind = getTrustKind(terminal.BlockPrice),
                  //     BranchCode = terminal.Branch.Id.ToString(),
                    WorkTitle = terminal.Title,
                    WorkTitleEng = terminal.EnglishTitle,
                     ShaparakAddressText = terminal.Address,
                  // //  RentEndingDate = DateTime.Now.AddYears(1).ToString("yyyy/MM/dd"),
                   PostalCode = terminal.PostCode,
                    PhoneNumber = terminal.Tel,
                  //   Mobile = terminal.MerchantProfile.Mobile,
                  //   MainCustomerID =  customer.Data.CustomerID,
                   AccountShabaCode = terminal.ShebaNo,
                   CityShaparakCode = terminal.City.Id.ToString(),
                    GuildSupplementaryCode = terminal.GuildId.ToString(),
                  //     OwneringTypeID = 1361,
                  //     HowToAssignID ="4632",
                  //    
                  //     
                  //     //TrustKind = "مسدودی حساب", //todo
                  //     //TrustNumber = terminal.BlockDocumentNumber,//todo
                  //   
                  //     
                   TaxPayerCode = terminal.TaxPayerCode, 
                    }
                }, terminal.Id);

                if (request.Status == PardakthNovinStatus.Successed)
                {
                    //todo
                    terminal.StatusId = 3;
                 //   terminal.FollowupCode = "";
                    terminal.PardakhtEditNovinSaveId = request.SavedID;
                    dataContext.SaveChanges();
                    return request;
                }
                else
                {
                    terminal.StatusId = 4;
                    terminal.ErrorComment = result.ErrorMessage;
                  //  terminal.FollowupCode =  "";
                    terminal.PardakhtEditNovinSaveId = request.SavedID;
                    dataContext.SaveChanges();
                    return request;
                }
            }
            
            
            
            return result; 
        }
        //2-18
        public UpdateRequestByFollowUpCodeResponse UpdateRequestByFollowUpCode(UpdateRequestByFollowUpCodeRequest input,long? terminalId)
        {
            UpdateRequestByFollowUpCodeResponse methodResponse;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");
                httpClient.DefaultRequestHeaders.Authorization = AuthorizationHeader();
                using (var response = httpClient.PostAsync(BaseAddress + "/UpdateRequestByFollowUpCode", content))
                {
                    response.Result.EnsureSuccessStatusCode();
                    var apiResponse = response.Result.Content.ReadAsStringAsync().Result;

                    methodResponse = JsonConvert.DeserializeObject<UpdateRequestByFollowUpCodeResponse>(apiResponse);
                }
            }

            var pardakhtNovinRequest = new PardakhtNovinRequest
            {
                Input = JsonConvert.SerializeObject(input),
                Method = "UpdateRequestByFollowUpCode",
                Module = "-",
                Result = JsonConvert.SerializeObject(methodResponse),
                TerminalId = terminalId,
                TrackId = DateTime.Now.Ticks
            };
            var datacontext = new AppDataContext();
            datacontext.PardakhtNovinRequests.Add(pardakhtNovinRequest);
            datacontext.SaveChanges();
            return methodResponse;
        }

        //2-19
        public UpdateDocumentResponse UpdateDocument(UpdateDocumentRequest input, long terminalId)
        {
            UpdateDocumentResponse methodResponse;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");
                httpClient.DefaultRequestHeaders.Authorization = AuthorizationHeader();
                using (var response = httpClient.PostAsync(BaseAddress + "/UpdateDocument", content))
                {
                    response.Result.EnsureSuccessStatusCode();
                    var apiResponse = response.Result.Content.ReadAsStringAsync().Result;

                    methodResponse = JsonConvert.DeserializeObject<UpdateDocumentResponse>(apiResponse);
                }
            }

            var pardakhtNovinRequest = new PardakhtNovinRequest
            {
                Input = JsonConvert.SerializeObject(input),
                Method = "UpdateDocument",
                Result = JsonConvert.SerializeObject(methodResponse),
                Module = "-",
                TerminalId = terminalId,
                TrackId = DateTime.Now.Ticks
            };
            var datacontext = new AppDataContext();
            datacontext.PardakhtNovinRequests.Add(pardakhtNovinRequest);
            datacontext.SaveChanges();
            return methodResponse;
        }

        //2-20
        public BindSerialToSwitchResponse BindSerialToSwitch(BindSerialToSwitchRequest input)
        {
            BindSerialToSwitchResponse methodResponse;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");
                httpClient.DefaultRequestHeaders.Authorization = AuthorizationHeader();
                using (var response = httpClient.PostAsync(BaseAddress + "/BindSerialToSwitch", content))
                {
                    response.Result.EnsureSuccessStatusCode();
                    var apiResponse = response.Result.Content.ReadAsStringAsync().Result;

                    methodResponse = JsonConvert.DeserializeObject<BindSerialToSwitchResponse>(apiResponse);
                }
            }

            var pardakhtNovinRequest = new PardakhtNovinRequest
            {
                Input = JsonConvert.SerializeObject(input),
                Method = "BindSerialToSwitch",
                Result = JsonConvert.SerializeObject(methodResponse),
                Module = "-",
                TrackId = DateTime.Now.Ticks
            };
            var datacontext = new AppDataContext();
            datacontext.PardakhtNovinRequests.Add(pardakhtNovinRequest);
            datacontext.SaveChanges();
            return methodResponse;
        }

        //2-21
        public AddPosReplacementResponse AddPosReplacement(AddPosReplacementRequest input)
        {
            AddPosReplacementResponse methodResponse;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");
                httpClient.DefaultRequestHeaders.Authorization = AuthorizationHeader();
                using (var response = httpClient.PostAsync(BaseAddress + "/AddPosReplacement", content))
                {
                    response.Result.EnsureSuccessStatusCode();
                    var apiResponse = response.Result.Content.ReadAsStringAsync().Result;

                    methodResponse = JsonConvert.DeserializeObject<AddPosReplacementResponse>(apiResponse);
                }
            }

            var pardakhtNovinRequest = new PardakhtNovinRequest
            {
                Input = JsonConvert.SerializeObject(input),
                Method = "AddPosReplacement",
                Module = "-",
                TrackId = DateTime.Now.Ticks
            };
            var datacontext = new AppDataContext();
            datacontext.PardakhtNovinRequests.Add(pardakhtNovinRequest);
            datacontext.SaveChanges();
            return methodResponse;
        }
         
        public string GetPosModelList(GetBankRequest input)
        {
            string methodResponse;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");
                httpClient.DefaultRequestHeaders.Authorization = AuthorizationHeader();
                using (var response = httpClient.PostAsync(BaseAddress + "/GetPosModelList", content))
                {
                    response.Result.EnsureSuccessStatusCode();
                    var apiResponse = response.Result.Content.ReadAsStringAsync().Result;

                    methodResponse = apiResponse ;
                }
            }

            var pardakhtNovinRequest = new PardakhtNovinRequest
            {
                Input = JsonConvert.SerializeObject(input),
                
                Method = "GetPosModelList",
                Module = "-",
                TrackId = DateTime.Now.Ticks
            };
            var datacontext = new AppDataContext();
            datacontext.PardakhtNovinRequests.Add(pardakhtNovinRequest);
            datacontext.SaveChanges();
            return methodResponse;
        }
        public string GetPosTypeList(GetBankRequest input)
        {
            string methodResponse;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");
                httpClient.DefaultRequestHeaders.Authorization = AuthorizationHeader();
                using (var response = httpClient.PostAsync(BaseAddress + "/GetPosTypeList", content))
                {
                    response.Result.EnsureSuccessStatusCode();
                    var apiResponse = response.Result.Content.ReadAsStringAsync().Result;
                    methodResponse =  (apiResponse);
                }
            }
            var pardakhtNovinRequest = new PardakhtNovinRequest
            {
                Input = JsonConvert.SerializeObject(input),
                Method = "GetPosTypeList",
                Module = "-",
                TrackId = DateTime.Now.Ticks
            };
            var datacontext = new AppDataContext();
            datacontext.PardakhtNovinRequests.Add(pardakhtNovinRequest);
            datacontext.SaveChanges();
            return methodResponse;
        }
        //2-22
        public GetPosReplacementDetailResponse GetPosReplacementDetail(GetPosReplacementDetailRequest input)
        {
            GetPosReplacementDetailResponse methodResponse;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");
                httpClient.DefaultRequestHeaders.Authorization = AuthorizationHeader();
                using (var response = httpClient.PostAsync(BaseAddress + "/GetPosReplacementDetail", content))
                {
                    response.Result.EnsureSuccessStatusCode();
                    var apiResponse = response.Result.Content.ReadAsStringAsync().Result;

                    methodResponse = JsonConvert.DeserializeObject<GetPosReplacementDetailResponse>(apiResponse);
                }
            }

            var pardakhtNovinRequest = new PardakhtNovinRequest
            {
                Input = JsonConvert.SerializeObject(input),
                Method = "GetPosReplacementDetail",
                Module = "-",
                TrackId = DateTime.Now.Ticks
            };
            var datacontext = new AppDataContext();
            datacontext.PardakhtNovinRequests.Add(pardakhtNovinRequest);
            datacontext.SaveChanges();
            return methodResponse;
        }


        //2-23
        public AddAccountChangeResponse AddAccountChange(AddAccountChangeRequest input,long terminalId)
        {
            AddAccountChangeResponse methodResponse;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");
                httpClient.DefaultRequestHeaders.Authorization = AuthorizationHeader();
                using (var response = httpClient.PostAsync(BaseAddress + "/AddAccountChange", content))
                {
                    response.Result.EnsureSuccessStatusCode();
                    var apiResponse = response.Result.Content.ReadAsStringAsync().Result;

                    methodResponse = JsonConvert.DeserializeObject<AddAccountChangeResponse>(apiResponse);
                }
            }

            var pardakhtNovinRequest = new PardakhtNovinRequest
            {
                Input = JsonConvert.SerializeObject(input),
                Method = "AddAccountChange",
                Result =JsonConvert.SerializeObject(methodResponse ),
                Module = "-",
                TerminalId = terminalId,
                TrackId = DateTime.Now.Ticks
            };
            var datacontext = new AppDataContext();
            datacontext.PardakhtNovinRequests.Add(pardakhtNovinRequest);
            datacontext.SaveChanges();
            return methodResponse;
        }

        //2-24
        public GetAccountChangeDetailResponse GetAccountChangeDetail(GetAccountChangeDetailRequest input,long terminalId)
        {
            GetAccountChangeDetailResponse methodResponse;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");
                httpClient.DefaultRequestHeaders.Authorization = AuthorizationHeader();
                using (var response = httpClient.PostAsync(BaseAddress + "/GetAccountChangeDetail", content))
                {
                    response.Result.EnsureSuccessStatusCode();
                    var apiResponse = response.Result.Content.ReadAsStringAsync().Result;

                    methodResponse = JsonConvert.DeserializeObject<GetAccountChangeDetailResponse>(apiResponse);
                }
            }

            var pardakhtNovinRequest = new PardakhtNovinRequest
            {
                Input = JsonConvert.SerializeObject(input),
                Method = "GetAccountChangeDetail",
                Result = JsonConvert.SerializeObject(methodResponse),
                Module = "-",
                TerminalId = terminalId,
                TrackId = DateTime.Now.Ticks
            };
            var datacontext = new AppDataContext();
            datacontext.PardakhtNovinRequests.Add(pardakhtNovinRequest);
            datacontext.SaveChanges();
            return methodResponse;
        }

        //2-25
        public AddEditRequestResponse AddEditRequest(AddEditRequestRequest input)
        {
            AddEditRequestResponse methodResponse;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");
                httpClient.DefaultRequestHeaders.Authorization = AuthorizationHeader();
                using (var response = httpClient.PostAsync(BaseAddress + "/AddEditRequest", content))
                {
                    response.Result.EnsureSuccessStatusCode();
                    var apiResponse = response.Result.Content.ReadAsStringAsync().Result;

                    methodResponse = JsonConvert.DeserializeObject<AddEditRequestResponse>(apiResponse);
                }
            }

            var pardakhtNovinRequest = new PardakhtNovinRequest
            {
                Input = JsonConvert.SerializeObject(input),
                Method = "AddEditRequest",
                Module = "-",
                TrackId = DateTime.Now.Ticks
            };
            var datacontext = new AppDataContext();
            datacontext.PardakhtNovinRequests.Add(pardakhtNovinRequest);
            datacontext.SaveChanges();
            return methodResponse;
        }

        //2-26
        public GetEditRequestResponse GetEditRequest(GetEditRequestRequest input)
        {
            GetEditRequestResponse methodResponse;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");
                httpClient.DefaultRequestHeaders.Authorization = AuthorizationHeader();
                using (var response = httpClient.PostAsync(BaseAddress + "/GetEditRequest", content))
                {
                    response.Result.EnsureSuccessStatusCode();
                    var apiResponse = response.Result.Content.ReadAsStringAsync().Result;

                    methodResponse = JsonConvert.DeserializeObject<GetEditRequestResponse>(apiResponse);
                }
            }

            var pardakhtNovinRequest = new PardakhtNovinRequest
            {
                Input = JsonConvert.SerializeObject(input),
                Method = "GetEditRequest",
                Module = "-",
                TrackId = DateTime.Now.Ticks
            };
            var datacontext = new AppDataContext();
            datacontext.PardakhtNovinRequests.Add(pardakhtNovinRequest);
            datacontext.SaveChanges();
            return methodResponse;
        }

        //2-27
        public AddInstallationRollbackResponse AddInstallationRollback(AddInstallationRollbackRequest input, long terminalId)
        {
            AddInstallationRollbackResponse methodResponse;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");
                httpClient.DefaultRequestHeaders.Authorization = AuthorizationHeader();
                using (var response = httpClient.PostAsync(BaseAddress + "/AddInstallationRollback", content))
                {
                    response.Result.EnsureSuccessStatusCode();
                    var apiResponse = response.Result.Content.ReadAsStringAsync().Result;

                    methodResponse = JsonConvert.DeserializeObject<AddInstallationRollbackResponse>(apiResponse);
                }
            }

            var pardakhtNovinRequest = new PardakhtNovinRequest
            {
                Input = JsonConvert.SerializeObject(input),
                Method = "AddInstallationRollback",
                Result = JsonConvert.SerializeObject(methodResponse),
                TerminalId = terminalId,
                Module = "-",
                TrackId = DateTime.Now.Ticks
            };
            var datacontext = new AppDataContext();
            datacontext.PardakhtNovinRequests.Add(pardakhtNovinRequest);
            datacontext.SaveChanges();
            return methodResponse;
        }


        //2-28
        public GetInstallationRollbackResponse GetInstallationRollback(GetInstallationRollbackRequest input,long terminalId)
        {
            GetInstallationRollbackResponse methodResponse;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");
                httpClient.DefaultRequestHeaders.Authorization = AuthorizationHeader();
                using (var response = httpClient.PostAsync(BaseAddress + "/GetInstallationRollback", content))
                {
                    response.Result.EnsureSuccessStatusCode();
                    var apiResponse = response.Result.Content.ReadAsStringAsync().Result;

                    methodResponse = JsonConvert.DeserializeObject<GetInstallationRollbackResponse>(apiResponse);
                }
            }

            var pardakhtNovinRequest = new PardakhtNovinRequest
            {
                Input = JsonConvert.SerializeObject(input),
                Method = "GetInstallationRollback",
                Result = JsonConvert.SerializeObject(methodResponse),
                TerminalId = terminalId,
                Module = "-",
                TrackId = DateTime.Now.Ticks
            };
            var datacontext = new AppDataContext();
            datacontext.PardakhtNovinRequests.Add(pardakhtNovinRequest);
            datacontext.SaveChanges();
            return methodResponse;
        }

        public void Dispose()
        {
        }

        
        public AddNewRequestResponse AddAcceptor(long terminalId )
        {

            var result = new AddNewRequestResponse();
            AddNewCustomerRequestDocs docs = new AddNewCustomerRequestDocs();
 
            using (var dataContext = new AppDataContext())
            {
                var terminal = dataContext.Terminals.Include(a=>a.TerminalDocuments).Include(a=>a.MerchantProfile)
                    .Include(a=>a.MerchantProfile.MerchantProfileDocuments).FirstOrDefault(a => a.Id == terminalId);
                if (terminal.MerchantProfile.PardakhtNovinCustomerId == null)
                {
                 
                    docs.Data = new List<Document>();
            
                    //1 add files =>
                   
                    foreach (var VARIABLE in terminal.MerchantProfile.MerchantProfileDocuments)
                    {
                         if (VARIABLE.DocumentTypeId == 13 ||
                             VARIABLE.DocumentTypeId == 11  ||
                         VARIABLE.DocumentTypeId == 14 )
                         {
                            AddFileRequest addFileRequest = new AddFileRequest();
                            addFileRequest.FileName = VARIABLE.FileName;
                            addFileRequest.BinaryDataBase64 = Convert.ToBase64String(VARIABLE.FileData);
                            var addFileResponse = AddFile(addFileRequest,terminal.Id);
                            if (addFileResponse.Status == PardakthNovinStatus.Successed)
                            {
                                docs.Data.Add(new Document()
                                {
                                    DocumentAttachment = addFileResponse.FileServerID.ToString(),
                                    DocumentTypeID = GetPardakhNovinDocTypeId(VARIABLE.DocumentTypeId),
                                    DocumentType = GetPardakhNovinDocType(VARIABLE.DocumentTypeId)
            
                                });
                            }
                        }
                    }
            
            
                    AddNewCustomerRequest addNewCustomerRequest = new AddNewCustomerRequest();
            
                    addNewCustomerRequest.Data = new AddNewCustomerRequestData();
                    addNewCustomerRequest.Data.BCID = terminal.MerchantProfile.IdentityNumber.Trim();
                    addNewCustomerRequest.Data.BirthDate = terminal.MerchantProfile.Birthdate;
                    if (terminal.MerchantProfile.IsLegalPersonality)
                    {
                        addNewCustomerRequest.Data.CompanyCode = terminal.MerchantProfile.LegalNationalCode.Trim();
                        addNewCustomerRequest.Data.CompanyFoundationDate = terminal.MerchantProfile.CompanyRegistrationDate;
                        addNewCustomerRequest.Data.CompanyName = terminal.Title;
                        addNewCustomerRequest.Data.CompanyNameEn = terminal.EnglishTitle;
                        addNewCustomerRequest.Data.CompanyRegisterNo =
                            terminal.MerchantProfile.CompanyRegistrationNumber.Trim();
                    }
                    addNewCustomerRequest.Data.CustomerTypeID =
                        terminal.MerchantProfile.IsLegalPersonality ? 1328 : 1327;
                    addNewCustomerRequest.Data.LastName = terminal.MerchantProfile.LastName;
                    addNewCustomerRequest.Data.Email = terminal.Email;
                    addNewCustomerRequest.Data.FatherName = terminal.MerchantProfile.FatherName;
                    addNewCustomerRequest.Data.FirstName = terminal.MerchantProfile.FirstName;
                    //addNewCustomerRequest.Data.ForeignersPervasiveCode=  terminal.MerchantProfile.FirstName;
                    addNewCustomerRequest.Data.FirstName = terminal.MerchantProfile.FirstName;
                    addNewCustomerRequest.Data.Mobile = terminal.MerchantProfile.Mobile;
                    addNewCustomerRequest.Data.NationalCode = terminal.MerchantProfile.NationalCode.Trim();
                    addNewCustomerRequest.Data.FirstNameEN = terminal.MerchantProfile.EnglishFirstName;
                    addNewCustomerRequest.Data.LastNameEN = terminal.MerchantProfile.EnglishLastName;
                    addNewCustomerRequest.Data.FatherNameEn = terminal.MerchantProfile.EnglishFatherName;
                  
                    addNewCustomerRequest.Data.GenderID = terminal.MerchantProfile.IsMale ? 1330 : 1329;
                    // addNewCustomerRequest.CustomerDocument = addNewCustomerRequest.Childs;
                    addNewCustomerRequest.Childs = new List<AddNewCustomerRequestDocs>();
                    addNewCustomerRequest.Childs.Add(docs);
                    var k = AddNewCustomer(addNewCustomerRequest,terminal.Id);
                    if (k.Status == PardakthNovinStatus.Successed)
                    {
            
                        terminal.MerchantProfile.PardakhtNovinCustomerId = k.SavedID;
                        dataContext.SaveChanges();
                    }
                }


                var customer = GetCustomerByCode(new GetCustomerByCodeRequest()
                {
                    Parameters = new GetCustomerByCodeRequestParameters()
                    {
                        CustomerCode = terminal.MerchantProfile.IsLegalPersonality ?  terminal.MerchantProfile.LegalNationalCode
                            : terminal.MerchantProfile.NationalCode
                    }

                });
                var    docs2 = new AddNewRequestRequestDataDocs();
                docs2.Data = new List<Document>();
                foreach (var terminalDocument in terminal.TerminalDocuments)
                {
                    
                        AddFileRequest addFileRequest = new AddFileRequest();
                        addFileRequest.FileName = terminalDocument.FileName;
                        addFileRequest.BinaryDataBase64 = Convert.ToBase64String(terminalDocument.FileData);
                        var addFileResponse = AddFile(addFileRequest,terminal.Id);
                        if (addFileResponse.Status == PardakthNovinStatus.Successed)
                        {
                            docs2.Data.Add(new Document()
                            {
                                DocumentAttachment = addFileResponse.FileServerID.ToString(),
                                DocumentTypeID = GetPardakhNovinDocTypeId(terminalDocument.DocumentTypeId),
                                DocumentType = GetPardakhNovinDocType(terminalDocument.DocumentTypeId)
                            });
                        }
                   
                }

                foreach (var terminalDocument in terminal.MerchantProfile.MerchantProfileDocuments)
                {
                    if (  terminalDocument.DocumentTypeId != 1  && 
                        terminalDocument.DocumentTypeId != 11 && terminalDocument.DocumentTypeId != 14   
                       )
                    {
                        AddFileRequest addFileRequest = new AddFileRequest();
                        addFileRequest.FileName = terminalDocument.FileName;
                        addFileRequest.BinaryDataBase64 = Convert.ToBase64String(terminalDocument.FileData);
                        var addFileResponse = AddFile(addFileRequest,terminal.Id);
                        if (addFileResponse.Status == PardakthNovinStatus.Successed)
                        {
                            docs2.Data.Add(new Document()
                            {
                                DocumentAttachment = addFileResponse.FileServerID.ToString(),
                                DocumentTypeID = GetPardakhNovinDocTypeId(terminalDocument.DocumentTypeId),
                                DocumentType = GetPardakhNovinDocType(terminalDocument.DocumentTypeId)
                            });
                        }
                    }
                }
                if (docs2.Data != null &&  docs2.Data.Any())
                {
                     var request = AddNewRequest( new AddNewRequestRequestWithDocs
                {
            
                    Childs =docs2.Data.Any( )?  new List<AddNewRequestRequestDataDocs>()
                    {
                       docs2
                    } : new List<AddNewRequestRequestDataDocs>(),
                    
                    
                    Data = new AddNewRequestRequestData()
                    {
                        PortType = "POS",
                        PosType =  getPosTyhpe(terminal.DeviceTypeId),
                        TrustKind = getTrustKind(terminal.BlockPrice), 
                        BankFollowupCode = "51",
                        AccountNumber = terminal.AccountNo,
                   //     ActivityLicenseNumberReferenceName = "Test",
                     //   ActivityLicenseNumber = "1234567",
                      //  BusinessLicenseEndDate=
                      BankID = 25,
                      BranchCode = terminal.Branch.Id.ToString(),
                      WorkTitle = terminal.Title,
                      WorkTitleEng = terminal.EnglishTitle,
                      ShaparakAddressText = terminal.Address,
                  //  RentEndingDate = DateTime.Now.AddYears(1).ToString("yyyy/MM/dd"),
                    PostalCode = terminal.PostCode,
                    PhoneNumber = terminal.Tel,
                    Mobile = terminal.MerchantProfile.Mobile,
                    MainCustomerID =  customer.Data.CustomerID,
                    AccountShabaCode = terminal.ShebaNo,
                    CityShaparakCode = terminal.City.Id.ToString(),
                    GuildSupplementaryCode = terminal.GuildId.ToString(),
                      OwneringTypeID = 1361,
                      HowToAssignID ="4632",
                     
                      
                      //TrustKind = "مسدودی حساب", //todo
                      //TrustNumber = terminal.BlockDocumentNumber,//todo
                    
                      
                      TaxPayerCode = terminal.TaxPayerCode, 
                    }
                },terminal.Id);

                if (request.Status == PardakthNovinStatus.Successed)
                {
                    //todo
                    terminal.StatusId = 3;
                    terminal.FollowupCode = request.Data.FollowupCode;
                    terminal.PardakhtNovinSaveId = request.SavedID;
                    dataContext.SaveChanges();
                    return request;
                }
                }
                else
                {
                     var request = AddNewRequest( new AddNewRequestRequest
                {
            
                    // Childs =docs2.Data.Any( )?  new List<AddNewRequestRequestDataDocs>()
                    // {
                    //    docs2
                    // } : new List<AddNewRequestRequestDataDocs>(),
                    
                    
                    Data = new AddNewRequestRequestData()
                    {
                        PortType = "POS",
                        PosType = getPosTyhpe(terminal.DeviceTypeId),
                       
                      BankFollowupCode = "51",
                        AccountNumber = terminal.AccountNo,
                   //     ActivityLicenseNumberReferenceName = "Test",
                     //   ActivityLicenseNumber = "1234567",
                      //  BusinessLicenseEndDate=
                      BankID = 25,
                      TrustKind = getTrustKind(terminal.BlockPrice),
                      BranchCode = terminal.Branch.Id.ToString(),
                      WorkTitle = terminal.Title,
                      WorkTitleEng = terminal.EnglishTitle,
                      ShaparakAddressText = terminal.Address,
                  //  RentEndingDate = DateTime.Now.AddYears(1).ToString("yyyy/MM/dd"),
                    PostalCode = terminal.PostCode,
                    PhoneNumber = terminal.Tel,
                    Mobile = terminal.MerchantProfile.Mobile,
                    MainCustomerID =  customer.Data.CustomerID,
                    AccountShabaCode = terminal.ShebaNo,
                    CityShaparakCode = terminal.City.Id.ToString(),
                    GuildSupplementaryCode = terminal.GuildId.ToString(),
                      OwneringTypeID = 1361,
                      HowToAssignID ="4632",
                     
                      
                      //TrustKind = "مسدودی حساب", //todo
                      //TrustNumber = terminal.BlockDocumentNumber,//todo
                    
                      
                      TaxPayerCode = terminal.TaxPayerCode, 
                    }
                },terminal.Id);

                if (request.Status == PardakthNovinStatus.Successed)
                {
                    //todo
                    terminal.StatusId = 3;
                    terminal.FollowupCode = request.Data.FollowupCode;
                    terminal.PardakhtNovinSaveId = request.SavedID;
                    dataContext.SaveChanges();
                    return request;
                }
                else
                {
                    terminal.StatusId = 4;
                    terminal.ErrorComment = result.ErrorMessage;
                    terminal.FollowupCode = request.Data.FollowupCode;
                    terminal.PardakhtNovinSaveId = request.SavedID;
                    dataContext.SaveChanges();
                    return request;
                }
                }
               
            }
            
            
            return result;
        }

        private string getTrustKind(int? blockprice)
        {
            if(blockprice.HasValue && blockprice.Value != 0)
                return "مسدودی حساب";
            return   "بدون ودیعه";
        }

        private string getPosTyhpe(long deviceTypeId)
        {
            switch (deviceTypeId)
            {
            case 3 :
                return "GPRS";
            case 1 : 
                return "HDLC/LAN";
            }
            return "HDLC/LAN";
        }

        private string GetPardakhNovinDocType(long variableDocumentType)
        {
            switch (variableDocumentType)
            {
                case 0: //کسب
                    return "تصویر جواز کسب";

                case 1:
                    //سند مسدودی
                    return "تصویر اسکن سند ودیعه";

                case 9:
                    //تایید شبا
                    return "تصویر گواهی بانک جهت تاییدیه شبا";

                case 10:
                    //صفحه توضیحات شناسنامه
                    return "صفحه توضیحات شناسنامه";


                case 11://تصویر روی کارت ملی
                    return "تصویر کارت ملی";
                case 13://صفحه اول شناسنامه
                    return "تصویر شناسنامه";
                case 14://تصویر پشت کارت ملی
                    return "تصویر پشت کارت ملی";
                case 15://پروانه کسب یا استشهادنامه
                    return "فرم استشهادنامه محلی";
                case 16://اساسنامه شرکت
                    return "تصویر تمامی صفحات اساسنامه";
                case 17://آگهي آخرين تغييرات و امضاهاي مجاز شركت در روزنامه رسمي
                    return "تصویر اگهی تاسیس";
                case 22://تصویر کارت فراگیر (اتباع)
                    return "0";
            }

            return "0";
        }
        private int GetPardakhNovinDocTypeId(long variableDocumentType)
        {
            switch (variableDocumentType)
            {
                case 0: //کسب
                    return 4500;

                case 1:
                    //سند مسدودی
                    return 4691;

                case 9:
                    //تایید شبا
                    return 4504;

                case 10:
                    //صفحه توضیحات شناسنامه
                    return 4509;


                case 11://تصویر روی کارت ملی
                    return 4495;
                case 13://صفحه اول شناسنامه
                    return 4496;
                case 14://تصویر پشت کارت ملی
                    return 4497;
                case 15://پروانه کسب یا استشهادنامه
                    return 4502;
                case 16://اساسنامه شرکت
                    return 4509;
                case 17://آگهي آخرين تغييرات و امضاهاي مجاز شركت در روزنامه رسمي
                    return 4661;
                case 22://تصویر کارت فراگیر (اتباع)
                    return 0;
            }

            return 0;
        }

        public async Task<SendChangeAccountRequestResponseModel> SendChangeAccountRequest(long changeAccountRequestId,
            string oldAccountNo, string newAccountNo, string newShebaNo, string firstName, string lastName,
            string merchantNo, long branchId, byte[] fileData, int terminalId)
        {
            var datacontext = new AppDataContext();
            var terminal = datacontext.Terminals.FirstOrDefault(a => a.Id == terminalId);
            var request = datacontext.ChangeAccountRequests.FirstOrDefault(a => a.Id == changeAccountRequestId);
           
            
            //1 ==> upload file
            var letterImageSaveId = "";
            var secondLetterImageId = "";

            
              
            AddFileRequest addFileRequest = new AddFileRequest();
            addFileRequest.FileName ="request.pdf";
            addFileRequest.BinaryDataBase64 = Convert.ToBase64String(request.FileData);
            var addFileResponse = AddFile(addFileRequest,terminal.Id);
            if (addFileResponse.Status == PardakthNovinStatus.Successed)
            {
                letterImageSaveId = addFileResponse.FileServerID.ToString();
            }


            addFileRequest = new AddFileRequest();
            addFileRequest.FileName="request.pdf";
            addFileRequest.BinaryDataBase64 = Convert.ToBase64String(request.FileData);
              addFileResponse = AddFile(addFileRequest,terminal.Id);
              if (addFileResponse.Status == PardakthNovinStatus.Successed)
              {
                  secondLetterImageId = addFileResponse.FileServerID.ToString();
              }

              
              
            if (string.IsNullOrEmpty(letterImageSaveId) ||
                string.IsNullOrEmpty(secondLetterImageId))
            {
                request.StatusId =  7;
                datacontext.SaveChanges();
                throw new Exception("بروز خطا در بارگذاری مدارک");
            }
            
            //2=> call changeaccount
            var addAccountChangeresposne =  AddAccountChange(new AddAccountChangeRequest()
            {
                Data = new AddAccountChangeRequestData()
                {
                    Request = terminal.FollowupCode,
                    BankFollowUpCode = "51",
                    TerminalID  = terminal.TerminalNo,
                    NewBankID  = 25,
                    NewBranchCode = request.AccountNo.Split('-')[0],
                    NewAccountNumber = request.AccountNo,
                    NewShabaCode = request.ShebaNo,
                    LetterImage = letterImageSaveId,
                    SecondLetterImage = secondLetterImageId,
                    AddAccountNumber = false,
                    IsMultiAccount = false,
                    


                } 
            }, terminal.Id);
            if (addAccountChangeresposne.Status == PardakthNovinStatus.Successed &&
                addAccountChangeresposne.SavedID != 0)
            {
                request.PardakhtNovinTrackId = addAccountChangeresposne.SavedID.ToString();
                request.StatusId =  2;     
                datacontext.SaveChanges();

            }
            else
            {
                request.StatusId =  7;
                datacontext.SaveChanges();
            }
            return null;
        }

        private int getRollBakReason(byte id)
        {
            switch (id)
            {
                case 1 :
                    return 4391;
                case 2 :
                    return 1375;
                case 3 :
                    return 1374;
                case 4 : return 1375;
                case 5 : return 1375 ;
                case 6 : return 2388  ;
                case 7 : return 4391  ;
                case 8 : return 1375  ;
                case 19 : return 1374   ;
                case 32:
                case 31 : return 1375    ;
                case 33 : return 1374    ;
                case 34 : return 1374    ;
                case 35 : return 2388     ;
            }

            return 4391;
        }
        public async Task<AddInstallationRollbackResponse> SendRevokeRequest(long revokeRequestId, string terminalNo,
            string FollowupCode,
            string reasonTitle, byte id,long? terminalId)
        {
            var rollBakReason = getRollBakReason(id);
            var k = AddInstallationRollback(new AddInstallationRollbackRequest()
            {
                Data = new AddInstallationRollbackRequestData()
                {
                    BankFollowUpCode  = "51",
                    TerminalID = terminalNo,
                    Request = FollowupCode,
                    RollbackReasonID = rollBakReason//todo
                }
            },terminalId.Value);
            return k;
        }

        

        public void AddAcceptorList(List<long> toList)
        {
            throw new NotImplementedException();
        }

        public GetRequestDetailByFollowupCodeResponse Inquery(string  FollowupCode, long terminalId)
        {
            //todo
            var q = GetRequestDetailByFollowupCode(new GetRequestDetailByFollowupCodeRequest()
            {
                Parameters = new GetRequestDetailByFollowupCodetParameters()
                {
                    FollowupCode = FollowupCode,
                    BankFollowupCode = "51"
                }
            },terminalId);

            return q;
            // if (PardakhtNovinSaveId != null)
            // {
            //     AddFileResponse methodResponse;
            //     using (var httpClient = new HttpClient())
            //     {
            //         var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
            //             "application/json");
            //         httpClient.DefaultRequestHeaders.Authorization = AuthorizationHeader();
            //         using (var response = httpClient.PostAsync(BaseAddress + "/AddFile", content))
            //         {
            //             response.Result.EnsureSuccessStatusCode();
            //             var apiResponse = response.Result.Content.ReadAsStringAsync().Result;
            //
            //             methodResponse = JsonConvert.DeserializeObject<AddFileResponse>(apiResponse);
            //         }
            //     }
            //
            //     var pardakhtNovinRequest = new PardakhtNovinRequest
            //     {
            //         Input = JsonConvert.SerializeObject(input),
            //         Method = "AddFile",
            //         Module = "-",
            //         Result = JsonConvert.SerializeObject(methodResponse),
            //         TrackId = DateTime.Now.Ticks
            //     };
            //     var datacontext = new AppDataContext();
            //     datacontext.PardakhtNovinRequests.Add(pardakhtNovinRequest);
            //     datacontext.SaveChanges();
            //
            //     return methodResponse;
            // }
            //
            // return null;
            //GetRequestDetailByFollowupCode
        }
        public   GetAccountChangeDetailResponse  ChangeAccountInquery(long id, string pardakhtNovinTrackId)
        {
            var t = GetAccountChangeDetail(new GetAccountChangeDetailRequest()
            {
              
                Parameters = new GetAccountChangeDetailRequestParameters()
                {
                    AccountChangeID = pardakhtNovinTrackId
                }
            },id);
            return t;
        }
        
        public GetInstallationRollbackResponse  RevokRequestInquery(string terminalNo, long id, byte statusId ,int? savedId)
        {
            var k = GetInstallationRollback(new GetInstallationRollbackRequest()
            {
                Parameters = new GetInstallationRollbackRequestParameters()
                { 
                    InstallationRollbackID = savedId.Value.ToString()
                }
            },id);
            return k;
        }
    }
}