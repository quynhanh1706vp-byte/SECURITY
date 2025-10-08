using AutoMapper;
using DeMasterProCloud.DataModel.Visit;

namespace DeMasterProCloud.Api.Infrastructure.Mapper
{
    public class WebhookMapping : Profile
    {
        public WebhookMapping()
        {
            CreateMap<BkavUserInVisitWebhook, SymptomDetailModel>()
                .ForMember(dest => dest.HealthStatus, opt => opt.MapFrom(src => src.HealthStatus == 1));
        }
    }
}