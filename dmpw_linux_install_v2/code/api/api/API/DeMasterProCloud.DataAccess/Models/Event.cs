using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DeMasterProCloud.DataAccess.Models
{
    public class Event
    {
        public Event()
        {

        }

        public int Id { get; set; }

        public int Culture { get; set; }

        public int EventNumber { get; set; }

        public string EventName { get; set; }

    }
}