using System;
using System.Collections.Generic;

namespace Khairah_.Models;

public partial class CauseType
{
    public int CTypeId { get; set; }

    public string? CTypeName { get; set; }

    public DateTime? CCreatedDate { get; set; }

    public virtual ICollection<Cause> Causes { get; set; } = new List<Cause>();
}
