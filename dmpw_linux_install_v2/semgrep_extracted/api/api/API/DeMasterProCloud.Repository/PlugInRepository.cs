using System;
using System.Collections.Generic;
using System.Linq;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.PlugIn;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace DeMasterProCloud.Repository
{
    public interface IPlugInRepository : IGenericRepository<PlugIn>
    {
        void AddPlugInDefault(int companyId, IConfiguration configuration);
        
        PlugIn GetPlugInByCompany(int companyId);

        List<EnumModel> GetListCardTypeByPlugIn(int companyId);
    }
    
    public class PlugInRepository : GenericRepository<PlugIn>, IPlugInRepository
    {
        private readonly AppDbContext _dbContext;
        
        public PlugInRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
        }
         
        public void AddPlugInDefault(int companyId, IConfiguration configuration)
        {
            var company = Get(m => m.CompanyId == companyId);
            if (company == null)
            {
                var pluginSettings = configuration.GetSection("DefaultPlugin")?.Get<List<PlugInSettingModel>>();
                var plugIns = new Dictionary<string, bool>();
                var plugInsDescription = new Dictionary<string, string>();
                foreach (var pluginSetting in pluginSettings)
                {
                    plugIns.Add(pluginSetting.Name, pluginSetting.IsEnable);
                    plugInsDescription.Add(pluginSetting.Name, pluginSetting.Description);
                }
                
                var json = JsonConvert.SerializeObject(plugIns);
                var jsonDescription = JsonConvert.SerializeObject(plugInsDescription);
                
                var plugIn = new PlugIn()
                {
                    CompanyId = companyId,
                    PlugIns = json,
                    PlugInsDescription = jsonDescription
                };
                Add(plugIn);
            }
        }

        public PlugIn GetPlugInByCompany(int companyId)
        {
            return Get(m => m.CompanyId == companyId);
        }

        public List<EnumModel> GetListCardTypeByPlugIn(int companyId)
        {
            var plugins = GetPlugInByCompany(companyId);
            List<int> excludeCardType = new List<int>();
            excludeCardType.Add((short)CardType.VehicleId);
            excludeCardType.Add((short)CardType.VehicleMotoBikeId);
            if (!string.IsNullOrEmpty(plugins.PlugIns))
            {
                var plugin = JsonConvert.DeserializeObject<PlugIns>(plugins.PlugIns);
                if (plugin != null)
                {
                    if (!plugin.QrCode)
                    {
                        excludeCardType.Add((short)CardType.QrCode);
                        excludeCardType.Add((short)CardType.NFCPhone);
                    }

                    if (!plugin.PassCode)
                    {
                        excludeCardType.Add((short)CardType.PassCode);
                    }

                    if (!plugin.CameraPlugIn)
                    {
                        excludeCardType.Add((short)CardType.HFaceId);
                    }

                    if (!plugin.Vein)
                    {
                        excludeCardType.Add((short)CardType.Vein);
                    }
                }
            }

            var type = typeof(CardType);
            return Enum.GetNames(type)
                .Select(name => new EnumModel()
                {
                    Id = (int)Enum.Parse(type, name),
                    Name = Enum.Parse(type, name).GetDescription()
                })
                .Where(m => !excludeCardType.Contains(m.Id))
                .ToList();
        }
    }
}