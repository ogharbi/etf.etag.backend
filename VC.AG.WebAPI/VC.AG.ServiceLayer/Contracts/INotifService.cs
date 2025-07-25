using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VC.AG.Models.Entities;
using VC.AG.Models.Enums;
using VC.AG.Models.ValuesObject;

namespace VC.AG.ServiceLayer.Contracts
{
    public interface INotifContract
    {

        Task<bool> SendReminder(DateTime? startDate, DateTime? endDate);
        Task<bool> SendNotifications(SiteEntity? rootSite,WfRequest? request,string? comment);
     


    }
}
