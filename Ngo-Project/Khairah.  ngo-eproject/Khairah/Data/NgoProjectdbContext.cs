using System;
using System.Collections.Generic;
using Khairah_.Models;
using Microsoft.EntityFrameworkCore;

namespace Khairah_.Data;

public partial class NgoProjectdbContext : DbContext
{
    public NgoProjectdbContext()
    {
    }

    public NgoProjectdbContext(DbContextOptions<NgoProjectdbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cause> Causes { get; set; }

    public virtual DbSet<CauseType> CauseTypes { get; set; }

    public virtual DbSet<Contact> Contacts { get; set; }

    public virtual DbSet<Donation> Donations { get; set; }

    public virtual DbSet<NgoEvent> NgoEvents { get; set; }

    public virtual DbSet<NgoVolunteer> NgoVolunteers { get; set; }

    public virtual DbSet<OurSponsor> OurSponsors { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<Volunteer> Volunteers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=192.168.100.18,65387;Initial Catalog=NGO_PROJECTdb;Persist Security Info=False;User ID=shehriyar;Password=asdf1234;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cause>(entity =>
        {
            entity.HasKey(e => e.CId).HasName("PK__CAUSES__A9FDEC12250B44CE");

            entity.ToTable("CAUSES");

            entity.Property(e => e.CId).HasColumnName("C_ID");
            entity.Property(e => e.CCreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("C_CREATED_AT");
            entity.Property(e => e.CDesc)
                .HasColumnType("text")
                .HasColumnName("C_DESC");
            entity.Property(e => e.CGoalAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("C_GOAL_AMOUNT");
            entity.Property(e => e.CImage)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("C_IMAGE");
            entity.Property(e => e.CName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("C_NAME");
            entity.Property(e => e.CRaisedAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("C_RAISED_AMOUNT");
            entity.Property(e => e.CauseTypeId).HasColumnName("CAUSE_TYPE_ID");

            entity.HasOne(d => d.CauseType).WithMany(p => p.Causes)
                .HasForeignKey(d => d.CauseTypeId)
                .HasConstraintName("FK__CAUSES__CAUSE_TY__42E1EEFE");
        });

        modelBuilder.Entity<CauseType>(entity =>
        {
            entity.HasKey(e => e.CTypeId).HasName("PK__CAUSE_TY__60686B6DC6341367");

            entity.ToTable("CAUSE_TYPE");

            entity.Property(e => e.CTypeId).HasColumnName("C_TYPE_ID");
            entity.Property(e => e.CCreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("C_CREATED_DATE");
            entity.Property(e => e.CTypeName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("C_TYPE_NAME");
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.ContId).HasName("PK__CONTACT__33DC4C79ED569CED");

            entity.ToTable("CONTACT");

            entity.Property(e => e.ContId).HasColumnName("CONT_ID");
            entity.Property(e => e.ContAddress)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("CONT_ADDRESS");
            entity.Property(e => e.ContEmail)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("CONT_EMAIL");
            entity.Property(e => e.ContMessage)
                .HasColumnType("text")
                .HasColumnName("CONT_MESSAGE");
            entity.Property(e => e.ContName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("CONT_NAME");
            entity.Property(e => e.ContPhone)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("CONT_PHONE");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("CREATED_AT");
        });

        modelBuilder.Entity<Donation>(entity =>
        {
            entity.HasKey(e => e.DId).HasName("PK__DONATION__76B8FF7DBC418462");

            entity.ToTable("DONATIONS");

            entity.Property(e => e.DId).HasColumnName("D_ID");
            entity.Property(e => e.DAddress)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("D_ADDRESS");
            entity.Property(e => e.DAmount)
                .HasColumnType("decimal(19, 2)")
                .HasColumnName("D_AMOUNT");
            entity.Property(e => e.DCauseId).HasColumnName("D_CAUSE_ID");
            entity.Property(e => e.DDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("D_DATE");
            entity.Property(e => e.DEmail)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("D_EMAIL");
            entity.Property(e => e.DFirstname)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("D_FIRSTNAME");
            entity.Property(e => e.DLastname)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("D_LASTNAME");
            entity.Property(e => e.DMessage)
                .HasColumnType("text")
                .HasColumnName("D_MESSAGE");

            entity.HasOne(d => d.DCause).WithMany(p => p.Donations)
                .HasForeignKey(d => d.DCauseId)
                .HasConstraintName("FK__DONATIONS__D_CAU__0D44F85C");
        });

        modelBuilder.Entity<NgoEvent>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__NGO_EVEN__241A510C57D9A4C6");

            entity.ToTable("NGO_EVENTS");

            entity.Property(e => e.EventId).HasColumnName("EVENT_ID");
            entity.Property(e => e.EventDate)
                .HasColumnType("datetime")
                .HasColumnName("EVENT_DATE");
            entity.Property(e => e.EventDescription)
                .HasColumnType("text")
                .HasColumnName("EVENT_DESCRIPTION");
            entity.Property(e => e.EventImage)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("EVENT_IMAGE");
            entity.Property(e => e.EventLocation)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("EVENT_LOCATION");
            entity.Property(e => e.EventTimeEnd).HasColumnName("EVENT_TIME_END");
            entity.Property(e => e.EventTimeStart).HasColumnName("EVENT_TIME_START");
            entity.Property(e => e.EventTitle)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("EVENT_TITLE");
        });

        modelBuilder.Entity<NgoVolunteer>(entity =>
        {
            entity.HasKey(e => e.NvId).HasName("PK__NGO_VOLU__E505EF9A8C2487EF");

            entity.ToTable("NGO_VOLUNTEERS");

            entity.HasIndex(e => e.NvEmail, "UQ__NGO_VOLU__D887D4854996082B").IsUnique();

            entity.Property(e => e.NvId).HasColumnName("NV_ID");
            entity.Property(e => e.NvCreatedDatetime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("NV_CREATED_DATETIME");
            entity.Property(e => e.NvEmail)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("NV_EMAIL");
            entity.Property(e => e.NvImg)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("NV_IMG");
            entity.Property(e => e.NvName)
                .HasMaxLength(55)
                .IsUnicode(false)
                .HasColumnName("NV_NAME");
        });

        modelBuilder.Entity<OurSponsor>(entity =>
        {
            entity.HasKey(e => e.SponsorId).HasName("PK__OUR_SPON__8A9CEEE026821A3D");

            entity.ToTable("OUR_SPONSOR");

            entity.Property(e => e.SponsorId).HasColumnName("SPONSOR_ID");
            entity.Property(e => e.SponsorLogo)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("SPONSOR_LOGO");
            entity.Property(e => e.SponsorName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("SPONSOR_NAME");
            entity.Property(e => e.SponsorshipDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("SPONSORSHIP_DATE");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__USERS__F3BEEBFFACB870EF");

            entity.ToTable("USERS");

            entity.HasIndex(e => e.UserEmail, "UQ__USERS__43CA3168B491D69E").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("USER_ID");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("USER_EMAIL");
            entity.Property(e => e.UserImage)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("USER_IMAGE");
            entity.Property(e => e.UserName)
                .HasMaxLength(75)
                .IsUnicode(false)
                .HasColumnName("USER_NAME");
            entity.Property(e => e.UserPassword)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("USER_PASSWORD");
            entity.Property(e => e.UserRoleId).HasColumnName("USER_ROLE_ID");

            entity.HasOne(d => d.UserRole).WithMany(p => p.Users)
                .HasForeignKey(d => d.UserRoleId)
                .HasConstraintName("FK__USERS__USER_ROLE__19DFD96B");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__USER_ROL__5AC4D2226249A81D");

            entity.ToTable("USER_ROLE");

            entity.Property(e => e.RoleId).HasColumnName("ROLE_ID");
            entity.Property(e => e.RoleName)
                .HasMaxLength(55)
                .IsUnicode(false)
                .HasColumnName("ROLE_NAME");
        });

        modelBuilder.Entity<Volunteer>(entity =>
        {
            entity.HasKey(e => e.VId).HasName("PK__VOLUNTEE__B35D77AC623A410C");

            entity.ToTable("VOLUNTEER");

            entity.HasIndex(e => e.VEmail, "UQ__VOLUNTEE__A2C4F8455B0BC07B").IsUnique();

            entity.Property(e => e.VId).HasColumnName("V_ID");
            entity.Property(e => e.VApplyDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("V_APPLY_DATE");
            entity.Property(e => e.VContNum)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("V_CONT_NUM");
            entity.Property(e => e.VEmail)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("V_EMAIL");
            entity.Property(e => e.VMessage)
                .HasColumnType("text")
                .HasColumnName("V_MESSAGE");
            entity.Property(e => e.VName)
                .HasMaxLength(55)
                .IsUnicode(false)
                .HasColumnName("V_NAME");
            entity.Property(e => e.VResume)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("V_RESUME");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
