using System;
using System.Diagnostics;
using AutoMapper;

namespace DeMasterProCloud.Api.Infrastructure.Mapper
{
    public static class MapperInstance
    {
        private static readonly Lazy<IMapper> _mapper = new Lazy<IMapper>(InitializeMapper);

        public static IMapper Mapper => _mapper.Value;

        private static IMapper InitializeMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
                // services.AddAutoMapper(typeof(Startup).Assembly);
            });

            return config.CreateMapper();
        }
    }
}