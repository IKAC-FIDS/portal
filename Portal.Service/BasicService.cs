 
using Portal.IService;
using TES.Data; 
using System.Collections.Generic; 
using System.Linq;
using System.Security.Principal; 
using TES.Data.Domain;
using TES.Security;

using Enums = TES.Common.Enumerations;
namespace Portal.Service
{
    public class BasicService : IBasicService
    {
        private readonly AppDataContext _dataContext;

        public BasicService(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }
        public List<Psp> GetPspList()
        {
             return _dataContext.Psps
                    
                    .ToList();
        }
    }

    public class CustomerService : ICustomerService
    {
        private readonly AppDataContext _dataContext;

        public CustomerService(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public List<CustomerStatusResult> GetCustomerData(IPrincipal User, string viewModelCustomerId, int viewModelMonth,
            int viewModelYear, long? currentUserBranchId  , List<int>  viewModelSpecial = null, List<int>  viewModelLowTransaction= null)
        {
            var query =   _dataContext.CustomerStatusResults.AsQueryable();
         
            if (User.IsBranchUser())
            {
                query = query.Where(x => x.BranchId == currentUserBranchId);
            }

            if (User.IsSupervisionUser())
            {
                query = query.Where(x => x.BranchId == currentUserBranchId || x.Branch.ParentId == currentUserBranchId);
            }

            if (User.IsTehranBranchManagementUser())
            {
                query = query.Where(x => x.Branch.CityId == (long) Enums.City.Tehran);
            }

            if (User.IsCountyBranchManagementUser())
            {
                query = query.Where(x => x.Branch.CityId != (long) Enums.City.Tehran);
            }

            if (viewModelSpecial != null)
            {
                query = query.Where(a => a.Special.HasValue && a.Special.Value == (viewModelSpecial.FirstOrDefault( ) == 1));
            }

            if (viewModelLowTransaction != null)
            {
                query = query.Where(a => a.IsGood.HasValue && a.IsGood.Value == (viewModelLowTransaction.FirstOrDefault( ) == 1));

            }
            return query

                .Where(b => b.IsGoodYear == viewModelYear && b.IsGoodMonth == viewModelMonth
                                                          && (string.IsNullOrEmpty(viewModelCustomerId) 
                                                              || b.CustomerId.Contains(viewModelCustomerId))).ToList();

        }
    }
}