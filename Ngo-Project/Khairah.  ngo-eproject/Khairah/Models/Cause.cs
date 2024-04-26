using System;
using System.Collections.Generic;

namespace Khairah_.Models;

public partial class Cause
{
    public int CId { get; set; }

    public string CName { get; set; } = null!;

    public string? CDesc { get; set; }

    public string? CImage { get; set; }

    public decimal? CGoalAmount { get; set; }

    public decimal? CRaisedAmount { get; set; } = 0;

    public int? CauseTypeId { get; set; }

    public DateTime? CCreatedAt { get; set; }

    public virtual CauseType? CauseType { get; set; }

    public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();
}
