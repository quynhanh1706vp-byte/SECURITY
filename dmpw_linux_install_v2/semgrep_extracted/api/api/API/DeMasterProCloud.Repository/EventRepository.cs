using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using DeMasterProCloud.Common.Infrastructure;
using Microsoft.AspNetCore.Http;
using DeMasterProCloud.DataAccess.Models;
using System.Globalization;
using System.Threading;
using System.Linq;

namespace DeMasterProCloud.Repository
{
    /// <summary>
    /// Interface for Event repository
    /// </summary>
    public interface IEventRepository : IGenericRepository<Event>
    {
        void AddDefaultEventList();
        IQueryable<Event> GetAllEventTypeByNumber(int eventNumber);
    }

    /// <summary>
    /// EventLog repository
    /// </summary>
    public class EventRepository : GenericRepository<Event>, IEventRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger _logger;
        public EventRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<EventRepository>();
        }

        /// <summary>
        /// Add or update a default account
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public void AddDefaultEventList()
        {
            var events = GetAll();

            if (events != null && events.Any())
            {
                DeleteRange(events.ToList());
            }

            var oldCulture = Thread.CurrentThread.CurrentUICulture;

            // Add default events to DB
            foreach (var culture in Enum.GetValues(typeof(CultureCodes)))
            {
                var cultureCode = culture.GetDescription();
                var cultureCodeNumber = (int)culture;

                Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureCode);

                foreach (var eachEvent in Enum.GetValues(typeof(EventType)))
                {
                    Event _event = new Event()
                    {
                        Culture = cultureCodeNumber,
                        EventNumber = (int)eachEvent,
                        EventName = eachEvent.GetDescription()
                    };

                    Add(_event);

                    //Console.WriteLine("##-## UPDATE EVENT");
                    //Console.WriteLine("##-## EVENT NUMBER : " + _event.EventNumber);
                    //Console.WriteLine("##-## EVENT NAME : " + _event.EventName);
                }
            }

            Thread.CurrentThread.CurrentUICulture = oldCulture;

        }

        public IQueryable<Event> GetAllEventTypeByNumber(int eventNumber)
        {
            try
            {
                return _dbContext.Event.Where(m => m.EventNumber == eventNumber).OrderBy(m => m.Culture);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllEventTypeByNumber");
                return Enumerable.Empty<Event>().AsQueryable();
            }
        }
    }
}