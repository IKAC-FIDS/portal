using System;
using System.Linq;
using System.Threading.Tasks;
using DNTScheduler;
using TES.Data;
using TES.Data.Domain;
using Enums = TES.Common.Enumerations;

namespace TES.Merchant.Web.UI.WebTasks
{
    public class UpdateBlockBranchPermissionTask : ScheduledTaskTemplate
    {
        public override string Name => "بروزرسانی گزارش دسترسی شعب";

        public override int Order => 7;

        public override async Task RunAsync()
        {
            using (var dataContext = new AppDataContext())
            {
                dataContext.AutomatedTaskLogs.Add(new AutomatedTaskLog
                {
                    ActivityTitle = "شروع",
                    ExecutionTime = DateTime.Now,
                    TaskName = "بروزرسانی گزارش دسترسی شعب"
                });


                var branchPermissions = dataContext.BranchPermission.Select(bp => new
                {
                    bp.StatusId,
                    bp.BranchId
                }).ToList();

                var organizationUnits = dataContext.OrganizationUnits.Select(ou => ou).ToList();


                foreach (var organizationUnit in organizationUnits)
                {
                    var branchPermission =
                        branchPermissions.FirstOrDefault(bp => bp.BranchId == organizationUnit.Id);
                    if (branchPermission is null)
                        continue;
                    switch (branchPermission.StatusId)
                    {
                        case 1:
                            organizationUnit.DisableNewTerminalRequest = false;
                            organizationUnit.DisableWirelessTerminalRequest = false;
                            break;
                        case 2:
                            organizationUnit.DisableNewTerminalRequest = false;
                            organizationUnit.DisableWirelessTerminalRequest = true;
                            break;
                        default:
                            organizationUnit.DisableNewTerminalRequest = true;
                            organizationUnit.DisableWirelessTerminalRequest = true;
                            break;
                    }
                }

                await dataContext.SaveChangesAsync();

                dataContext.AutomatedTaskLogs.Add(new AutomatedTaskLog
                {
                    ActivityTitle = "پایان",
                    ExecutionTime = DateTime.Now,
                    TaskName = "بروزرسانی گزارش دسترسی شعب",
                    Message = ""
                });

                await dataContext.SaveChangesAsync();
            }
        }

        public override bool RunAt(DateTime utcNow)
        {
            var now = utcNow.ToLocalTime();

            return (now.Hour == 23 && now.Minute == 1 && now.Second == 1)
                ;
        }
    }
}