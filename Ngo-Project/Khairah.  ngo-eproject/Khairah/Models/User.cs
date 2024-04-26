using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Khairah_.Models;

public partial class User
{
    
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    
    public string UserEmail { get; set; } = null!;


    public string UserPassword { get; set; } = null!;

    public string? UserImage { get; set; }

    public int? UserRoleId { get; set; }

    public virtual UserRole? UserRole { get; set; }
}
