using System;
using System.Collections.Generic;

namespace Khairah_.Models;

public partial class NgoEvent
{
    public int EventId { get; set; }

    public DateTime? EventDate { get; set; }

    public TimeOnly? EventTimeStart { get; set; }

    public TimeOnly? EventTimeEnd { get; set; }

    public string? EventLocation { get; set; }

    public string? EventTitle { get; set; }

    public string? EventDescription { get; set; }

    public string? EventImage { get; set; }
}
