using System;
using System.Collections.Generic;

namespace Khairah_.Models;

public partial class NgoVolunteer
{
    public int NvId { get; set; }

    public string NvName { get; set; } = null!;

    public string NvEmail { get; set; } = null!;

    public string NvImg { get; set; } = null!;

    public DateTime? NvCreatedDatetime { get; set; }
}
