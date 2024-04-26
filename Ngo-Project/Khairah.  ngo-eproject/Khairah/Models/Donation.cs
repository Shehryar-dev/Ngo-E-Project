using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace Khairah_.Models;


public partial class Donation
{
    [Key]
    public int DId { get; set; }

  
    [DisplayName("Select Amount")]
    public decimal? DAmount { get; set; }

    [Required]
    [DisplayName("First Name")]
    public string? DFirstname { get; set; }

    [Required]
    [DisplayName("Last Name")]
    public string? DLastname { get; set; }

    [Required]
    [DisplayName("Email")]
    public string? DEmail { get; set; }

    [Required]
    [DisplayName("Address")]
    public string? DAddress { get; set; }

    [Required]
    [DisplayName("Motivate Message")]
    public string? DMessage { get; set; }

    public DateTime? DDate { get; set; }


    public int? DCauseId { get; set; }

    public virtual Cause? DCause { get; set; }


}
