using System.Collections.Generic;
using AutoMapper;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataModel.Setting;
using Newtonsoft.Json;

namespace DeMasterProCloud.Api.Infrastructure.Mapper
{
    /// <summary>
    /// SettingMapping class
    /// </summary>
    public class SettingMapping : Profile
    {
        /// <summary>
        /// Setting Mapping
        /// </summary>
        public SettingMapping()
        {
            CreateMap<Setting, SettingEditModel>()
                .ForMember(dest => dest.Value,
                    opt => opt.MapFrom(src => JsonConvert.DeserializeObject<List<string>>(src.Value)));
            CreateMap<SettingModel, Setting>()
                .ForMember(dest => dest.Key, opt => opt.Ignore())
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => JsonConvert.SerializeObject(src.Value)));
            
            CreateMap<LanguageModel, LanguageDetailModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Tag, opt => opt.MapFrom(src => src.Tag));
            
            CreateMap<LanguageDetailModel, LanguageModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Tag, opt => opt.MapFrom(src => src.Tag));

        }
    }
}
