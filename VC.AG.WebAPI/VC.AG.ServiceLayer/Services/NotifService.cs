using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VC.AG.DAO.UnitOfWork;
using VC.AG.Models.Entities;
using VC.AG.Models.Enums;
using VC.AG.Models.Extensions;
using VC.AG.Models.ValuesObject;
using VC.AG.ServiceLayer.Contracts;
using static VC.AG.Models.AppConstants;
using Microsoft.IdentityModel.Tokens;
using VC.AG.Models.Helpers;
using VC.AG.ServiceLayer.Helpers;
using Microsoft.Office.SharePoint.Tools;
using Microsoft.SharePoint.Client;
using VC.AG.Models;
using Microsoft.Graph;
using Microsoft.SharePoint.News.DataModel;
using Microsoft.Extensions.Azure;
using VC.AG.Models.ValuesObject.SPContext;
using System.Security.Claims;
namespace VC.AG.ServiceLayer.Services
{
    public class NotifService(IUnitOfWork uow, IConfiguration config, IMemoryCache cache, ISiteContract siteSvc) : INotifContract
    {
        readonly JobHelper jobHelper = new(uow, config, cache, siteSvc);
        readonly GraphContext graphContext = new(config, cache);

        public Task<bool> SendReminder(ILogger logger)
        {
            throw new NotImplementedException();
        }
    }
}
