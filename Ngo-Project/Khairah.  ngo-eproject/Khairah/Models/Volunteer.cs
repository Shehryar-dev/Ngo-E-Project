using System;
using System.Collections.Generic;

namespace Khairah_.Models;

public partial class Volunteer
{
    public int VId { get; set; }

    public string VName { get; set; } = null!;

    public string VEmail { get; set; } = null!;

    public string VContNum { get; set; } = null!;

    public string? VMessage { get; set; }

    public string VResume { get; set; } = null!;

    public DateTime? VApplyDate { get; set; }
}
