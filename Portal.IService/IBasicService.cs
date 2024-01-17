using System.Collections.Generic;
using System.Security.Principal;
using TES.Common.Enumerations;
using TES.Data.Domain;

namespace Portal.IService
{
    public interface IBasicService
    {
        List<Psp> GetPspList();
    }

    public interface ICustomerService
    {
        List<CustomerStatusResult> GetCustomerData(IPrincipal modelCustomerId, string viewModelCustomerId, int viewModelMonth,
            int viewModelYear, long? currentUserBranchId ,  List<int>  viewModelSpecial = null, List<int>  viewModelLowTransaction= null);

      
    }
}