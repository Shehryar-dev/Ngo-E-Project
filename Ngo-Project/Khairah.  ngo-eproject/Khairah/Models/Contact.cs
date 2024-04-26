using System;
using System.Collections.Generic;

namespace Khairah_.Models;

public partial class Contact
{
    public int ContId { get; set; }

    public string ContName { get; set; } = null!;

    public string ContEmail { get; set; } = null!;

    public string ContPhone { get; set; } = null!;

    public string ContAddress { get; set; } = null!;

    public string? ContMessage { get; set; }

    public DateTime? CreatedAt { get; set; }
}
