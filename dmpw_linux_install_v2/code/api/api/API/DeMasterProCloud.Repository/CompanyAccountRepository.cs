using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using System;
using Microsoft.Extensions.Options;
using DeMasterProCloud.DataModel.Api;
using System.Collections.Generic;

namespace DeMasterProCloud.Repository
{

    /// <summary>
    /// UserLogin repository interface
    /// </summary>
    public interface ICompanyAccountRepository : IGenericRepository<CompanyAccount>
    {
        CompanyAccount GetCompanyAccountById(int companyAccountId);
        IQueryable<Account> GetAccountsByCompanyId(int companyId);
        IQueryable<CompanyAccount> GetCompanyAccountByCompany(int companyId);
        List<int?> GetCompanyIdsByAccount(int accountId);
        CompanyAccount GetCompanyAccountByCompanyAndAccount(int companyId, int accountId);
        List<CompanyAccount> GetCompanyAccountByAccount(int accountId);
    }

    /// <summary>
    /// UserLogin repository
    /// </summary>
    public class CompanyAccountRepository : GenericRepository<CompanyAccount>, ICompanyAccountRepository
    {
        private readonly AppDbContext _dbContext;
        public CompanyAccountRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
        }

        public IQueryable<Account> GetAccountsByCompanyId(int companyId)
        {
            return _dbContext.CompanyAccount
                .Include(m => m.Account)
                .Include(m => m.Company)
                .Include(m => m.DynamicRole)
                .Where(m => m.CompanyId == companyId).Select(x => x.Account);
        }

        ///// <summary>
        ///// Get root account by company
        ///// </summary>
        ///// <param name="companyId"></param>
        ///// <returns></returns>
        //public CompanyAccount GetCompanyAccountByCompany(int companyId)
        //{
        //    return Get(c => c.CompanyId == companyId);
         
        //}

        public CompanyAccount GetCompanyAccountById(int companyAccountId)
        {
            return _dbContext.CompanyAccount.Include(m => m.Account).Include(m => m.Company).Include(m => m.DynamicRole)
                .Where(m => m.Id == companyAccountId).FirstOrDefault();
        }

        public CompanyAccount GetCompanyAccountByCompanyAndAccount(int companyId, int accountId)
        {
            return _dbContext.CompanyAccount
                .Include(m => m.Company)
                .Include(m => m.Account).ThenInclude(m => m.User)
                .Include(m => m.DynamicRole)
                .Where(c => c.AccountId == accountId && c.CompanyId == companyId).FirstOrDefault();
        }

        public List<CompanyAccount> GetCompanyAccountByAccount(int accountId)
        {
            return _dbContext.CompanyAccount.Where(c => c.AccountId == accountId).ToList();
        }

        public List<int?> GetCompanyIdsByAccount(int accountId)
        {
            return _dbContext.CompanyAccount.Where(c => c.AccountId == accountId).Select(c => c.CompanyId).ToList();
        }

        IQueryable<CompanyAccount> ICompanyAccountRepository.GetCompanyAccountByCompany(int companyId)
        {
            //throw new NotImplementedException();
            return _dbContext.CompanyAccount.Include(m => m.DynamicRole).Include(m => m.Account).Where(m => m.CompanyId == companyId);
        }
    }
}