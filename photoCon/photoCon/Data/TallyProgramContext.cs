using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using photoCon.Dto;
using photoCon.Models;

namespace photoCon.Data
{
    public partial class TallyProgramContext : DbContext
    {
        public TallyProgramContext()
        {
        }

        public TallyProgramContext(DbContextOptions<TallyProgramContext> options)
            : base(options)
        {
        }

        public virtual DbSet<PhotoMetaDatum> PhotoMetaData { get; set; } = null!;
        public virtual DbSet<Category> Categories { get; set; } = null!;
        public virtual DbSet<TblEvent> TblEvents { get; set; } = null!;
        public virtual DbSet<TblJudge> TblJudges { get; set; } = null!;
        public virtual DbSet<TblParameter> TblParameters { get; set; } = null!;
        public virtual DbSet<PhotoLocation> PhotoLocations { get; set; } = null!;
        public virtual DbSet<TblRegion> TblRegions { get; set; } = null!;
        public virtual DbSet<TblStatus> TblStatuses { get; set; } = null!;
        public virtual DbSet<TblTallyTransaction> TblTallyTransactions { get; set; } = null!;
        public virtual DbSet<SeedControlTotal> SeedControlTotals { get; set; } = null!;
        public virtual DbSet<Judge> Judges { get; set; } = null!;
        public virtual DbSet<ProcessParameter> ProcessParameters { get; set; } = null!;
        public virtual DbSet<ImageBatch> ImageBatches { get; set; } = null!;
        public virtual DbSet<DayNumber> DayNumbers { get; set; } = null!;
        public virtual DbSet<ImageScore> ImageScore { get; set; } = null!;
        public virtual DbSet<ImageScoreGrandFinal> ImageScoreGrandFinal { get; set; } = null!;
        public virtual DbSet<ReferenceType> ReferenceType { get; set; } = null!;
        public virtual DbSet<ReferenceCode> ReferenceCode { get; set; } = null!;
        public virtual DbSet<JudgeImageView> JudgeImageView { get; set; } = null!;
        public virtual DbSet<CSV_Details> CSV_PhotoDetails { get; set; } = null!;
        public virtual DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public virtual DbSet<AspNetUsers> AspNetUsers { get; set; } = null!;
        public virtual DbSet<AspNetRoles> AspNetRoles { get; set; } = null!;
        public virtual DbSet<AspNetUserRoles> AspNetUserRoles { get; set; } = null!;
        public virtual DbSet<AuditLog> AuditLog { get; set; } = null!;




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JudgeImageView>().HasNoKey();


            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Category");

                entity.Property(e => e.CategoryName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PhotoLocation>(entity =>
            {
                entity.HasKey(e => e.PhotoId)
                    .HasName("PK_tblPhotoLocations");

                entity.Property(e => e.DateTimeEncoded).HasColumnType("datetime");

                entity.Property(e => e.EncodedBy)
                    .HasMaxLength(7)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.HashPhotoId)
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .HasColumnName("HashPhotoID");

                entity.Property(e => e.PhotoCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Filler01)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Filler02)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.RepositoryLocation)
                    .HasMaxLength(500)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PhotoMetaDatum>(entity =>
            {
                entity.HasKey(e => e.Index);

                entity.Property(e => e.BitDepth)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CameraMaker)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CameraModel)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Dimension)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ExposureTime)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.FileName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.FlashMode)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.FocalLength)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Fstop)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("FStop");

                entity.Property(e => e.HashPhotoId)
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .HasColumnName("HashPhotoID");

                entity.Property(e => e.Height)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.HorizontalResolution)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ImageUrl)
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .HasColumnName("ImageURL");

                entity.Property(e => e.Isospeed)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("ISOSpeed");

                entity.Property(e => e.MaxAperture)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.MeteringMode)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.MmFocalLength)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ResolutionUnit)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.VerticalResolution)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Width)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SeedControlTotal>(entity =>
            {
                entity.HasKey(e => e.Index);

                entity.ToTable("SeedControlTotal");

                entity.Property(e => e.CategoryId).HasColumnName("CategoryID");

                entity.Property(e => e.ExecutedOn)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Index).ValueGeneratedOnAdd();

                entity.Property(e => e.LastUpdate).HasColumnType("datetime");

                entity.Property(e => e.RegionId).HasColumnName("RegionID");
            });

            modelBuilder.Entity<TblEvent>(entity =>
            {
                entity.HasKey(e => e.EventId);

                entity.ToTable("tblEvent");

                entity.Property(e => e.DateEnd).HasColumnType("datetime");

                entity.Property(e => e.DateStart).HasColumnType("datetime");

                entity.Property(e => e.DateTimeEncoded).HasColumnType("datetime");

                entity.Property(e => e.EncodedBy)
                    .HasMaxLength(7)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.EventName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TblJudge>(entity =>
            {
                entity.HasKey(e => e.JudgeId);

                entity.ToTable("tblJudges");

                entity.Property(e => e.JudgeName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TblParameter>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("tblParameters");

                entity.Property(e => e.ParamDescription)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ParamId).ValueGeneratedOnAdd();

                entity.Property(e => e.ParamValue)
                    .HasMaxLength(50)
                    .IsFixedLength();
            });

            modelBuilder.Entity<TblRegion>(entity =>
            {
                entity.HasKey(e => e.RegionId);

                entity.ToTable("tblRegion");

                entity.Property(e => e.RegionName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TblStatus>(entity =>
            {
                entity.HasKey(e => e.StatusId);

                entity.ToTable("tblStatus");

                entity.Property(e => e.DateTimeEncoded).HasColumnType("datetime");

                entity.Property(e => e.EncodedBy)
                    .HasMaxLength(7)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.StatusDescription)
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TblTallyTransaction>(entity =>
            {
                entity.HasKey(e => e.TransId);

                entity.ToTable("tblTallyTransaction");

                entity.Property(e => e.DateTimeEncoded).HasColumnType("datetime");

                entity.Property(e => e.EncodedBy)
                    .HasMaxLength(7)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Score).HasColumnType("decimal(18, 4)");
            });

            modelBuilder.Entity<Judge>(entity =>
            {
                entity.HasKey(e => e.Index);
                entity.ToTable("Judges"); // Specify the table name if it's different from the class name

                entity.Property(e => e.HashIndex)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.MiddleName)
                    .HasMaxLength(50);

                entity.Property(e => e.EmailAddress)
                    .HasMaxLength(100);

                entity.Property(e => e.MobileNumber)
                    .HasMaxLength(50);

                entity.Property(e => e.Description)
                    .HasMaxLength(maxLength: 255);


            });

            modelBuilder.Entity<ProcessParameter>(entity => 
            {
                entity.HasKey(e => e.Index);
                entity.ToTable("DateParameter"); // Specify the table name if it's different from the class name

                entity.Property(e => e.Process)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.DetailedDescription)
                    .HasMaxLength(250);

                entity.Property(e => e.ParameterValue)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Process)
                    .IsRequired();

                entity.Property(e => e.Filler01)
                    .HasMaxLength(100);

                entity.Property(e => e.Filler02)
                    .HasMaxLength(100);

                entity.Property(e => e.Filler03)
                    .HasMaxLength(100);

                entity.Property(e => e.Filler04)
                    .HasMaxLength(100);

                entity.Property(e => e.Filler05)
                    .HasMaxLength(100);

                entity.Property(e => e.Filler06)
                    .HasMaxLength(100);

                entity.Property(e => e.Filler07)
                    .HasMaxLength(100);

                entity.Property(e => e.Filler08)
                    .HasMaxLength(100);

                entity.Property(e => e.EffectivityDate).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedDateTime).HasColumnType("datetime");

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50);

                entity.Property(e => e.ModifiedDateTime).HasColumnType("datetime");



            });

            modelBuilder.Entity<ImageBatch>(entity =>
            {
                // Primary key
                entity.HasKey(e => e.Index);
                entity.ToTable("ImageBatch");

                // Property configurations
                entity.Property(e => e.ImageHashId)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.Sort)
                    .IsRequired();

                entity.Property(e => e.Date)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.DayNumber); // DayNumber doesn't need a length specification

                entity.Property(e => e.IsActive)
                    .IsRequired();

            });

            modelBuilder.Entity<DayNumber>(entity =>
            {
                entity.ToTable("DayNumber"); // Set the table name to "DayNumber"

                // Primary key
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Date_)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Category)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.DayNumber_)
                    .IsRequired();

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.IsActive)
                    .IsRequired();
            });

            modelBuilder.Entity<ImageScore>(entity =>
            {
                // Primary key
                entity.HasKey(e => e.Index);

                // Property configurations
                entity.Property(e => e.PhotoId)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.Round)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Score)
                    .IsRequired();

                entity.Property(e => e.LastUpdateDate).HasColumnType("datetime");

            });

            modelBuilder.Entity<ImageScoreGrandFinal>(entity =>
            {
                // Primary key
                entity.HasKey(e => e.Index);

                // Property configurations
                entity.Property(e => e.PhotoId)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.Round)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Criteria)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.Score)
                    .IsRequired();

                entity.Property(e => e.LastUpdateDate).HasColumnType("datetime");

            });

            modelBuilder.Entity<ReferenceType>(entity =>
            {
                entity.ToTable("ReferenceType");
                entity.HasKey(e => e.RefTypeID);

                entity.Property(e => e.Index)
                    .IsRequired();

                entity.Property(e => e.RefTypeID)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.RefTypeName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.RefTypeDesc)
                    .HasMaxLength(250)
                    .IsRequired(false); // Allow null values

                entity.Property(e => e.IsActive)
                    .IsRequired();

                entity.Property(e => e.Filler01)
                    .HasMaxLength(100)
                    .IsRequired(false); // Allow null values

                entity.Property(e => e.Filler02)
                    .HasMaxLength(100)
                    .IsRequired(false); // Allow null values

                entity.Property(e => e.Filler03)
                    .HasMaxLength(100)
                    .IsRequired(false); // Allow null values

                entity.Property(e => e.EffectivityDate)
                    .IsRequired();

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedDateTime)
                    .IsRequired();

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsRequired(false); // Allow null values

                entity.Property(e => e.ModifiedDateTime)
                    .IsRequired(false); // Allow null values
            });

            modelBuilder.Entity<ReferenceCode>(entity =>
            {
                entity.HasKey(e => new { e.RefTypeID, e.RefCodeID });

                entity.Property(e => e.Index)
                    .IsRequired();

                entity.Property(e => e.RefTypeID)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.RefCodeID)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.RefCodeName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.RefCodeDesc)
                    .HasMaxLength(250);

                entity.Property(e => e.IsActive)
                    .IsRequired();

                entity.Property(e => e.SortNumber);

                entity.Property(e => e.Filler01)
                    .HasMaxLength(100);

                entity.Property(e => e.Filler02)
                    .HasMaxLength(100);

                entity.Property(e => e.Filler03)
                    .HasMaxLength(100);

                entity.Property(e => e.EffectivityDate)
                    .IsRequired();

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedDateTime)
                    .IsRequired();

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50);

                entity.Property(e => e.ModifiedDateTime);
            });

            modelBuilder.Entity<AspNetUsers>(entity =>
            {
                entity.HasKey(e => new { e.Id });
            });

            modelBuilder.Entity<AspNetRoles>(entity =>
            {
                entity.HasKey(e => new { e.Id });
            });

            modelBuilder.Entity<AspNetUserRoles>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => new { e.LogID });
            });


            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
