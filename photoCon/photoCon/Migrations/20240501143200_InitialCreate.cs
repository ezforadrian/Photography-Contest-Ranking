using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace photoCon.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "Category",
            //    columns: table => new
            //    {
            //        CategoryId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        CategoryName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Category", x => x.CategoryId);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "DayNumber",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Date_ = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
            //        Category = table.Column<string>(type: "nchar(10)", maxLength: 10, nullable: false),
            //        DayNumber_ = table.Column<int>(type: "int", nullable: false),
            //        Status = table.Column<string>(type: "nchar(10)", maxLength: 10, nullable: false),
            //        IsActive = table.Column<bool>(type: "bit", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_DayNumber", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "ImageBatch",
            //    columns: table => new
            //    {
            //        Index = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        ImageHashId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            //        Sort = table.Column<int>(type: "int", nullable: false),
            //        Date = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
            //        DayNumber = table.Column<int>(type: "int", nullable: false),
            //        IsActive = table.Column<bool>(type: "bit", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ImageBatch", x => x.Index);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Judges",
            //    columns: table => new
            //    {
            //        Index = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        HashIndex = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            //        LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
            //        FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
            //        MiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        EmailAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
            //        MobileNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
            //        Filler01 = table.Column<int>(type: "int", nullable: true),
            //        Filler02 = table.Column<int>(type: "int", nullable: true),
            //        CreatedBy = table.Column<int>(type: "int", nullable: false),
            //        CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        ModifiedBy = table.Column<int>(type: "int", nullable: true),
            //        ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Judges", x => x.Index);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "PhotoLocations",
            //    columns: table => new
            //    {
            //        PhotoId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        HashPhotoID = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
            //        RepositoryLocation = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
            //        RegionId = table.Column<int>(type: "int", nullable: false),
            //        CategoryId = table.Column<int>(type: "int", nullable: false),
            //        PhotoCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
            //        EncodedBy = table.Column<string>(type: "char(7)", unicode: false, fixedLength: true, maxLength: 7, nullable: true),
            //        DateTimeEncoded = table.Column<DateTime>(type: "datetime", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_tblPhotoLocations", x => x.PhotoId);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "PhotoMetaData",
            //    columns: table => new
            //    {
            //        Index = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        HashPhotoID = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
            //        FileName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
            //        Dimension = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
            //        Width = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
            //        Height = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
            //        HorizontalResolution = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
            //        VerticalResolution = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
            //        BitDepth = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
            //        ResolutionUnit = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
            //        ImageURL = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
            //        CameraMaker = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
            //        CameraModel = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
            //        FStop = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
            //        ExposureTime = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
            //        ISOSpeed = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
            //        FocalLength = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
            //        MaxAperture = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
            //        MeteringMode = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
            //        FlashMode = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
            //        MmFocalLength = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_PhotoMetaData", x => x.Index);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "ProcessParameter",
            //    columns: table => new
            //    {
            //        Index = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Process = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
            //        Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
            //        Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            //        DetailedDescription = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
            //        ParameterValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            //        IsActive = table.Column<int>(type: "int", nullable: false),
            //        Filler01 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
            //        Filler02 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
            //        Filler03 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
            //        EffectivityDate = table.Column<DateTime>(type: "datetime", nullable: false),
            //        CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
            //        CreatedDateTime = table.Column<DateTime>(type: "datetime", nullable: false),
            //        ModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        ModifiedDateTime = table.Column<DateTime>(type: "datetime", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ProcessParameter", x => x.Index);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "SeedControlTotal",
            //    columns: table => new
            //    {
            //        Index = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        RegionID = table.Column<int>(type: "int", nullable: false),
            //        CategoryID = table.Column<int>(type: "int", nullable: false),
            //        LastUpdate = table.Column<DateTime>(type: "datetime", nullable: true),
            //        DbCountFrom = table.Column<int>(type: "int", nullable: true),
            //        SeededData = table.Column<int>(type: "int", nullable: true),
            //        ExecutedOn = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_SeedControlTotal", x => x.Index);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "tblEvent",
            //    columns: table => new
            //    {
            //        EventId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        EventName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
            //        RegionId = table.Column<int>(type: "int", nullable: true),
            //        CategoryId = table.Column<int>(type: "int", nullable: true),
            //        DateStart = table.Column<DateTime>(type: "datetime", nullable: true),
            //        DateEnd = table.Column<DateTime>(type: "datetime", nullable: true),
            //        IsActive = table.Column<int>(type: "int", nullable: true),
            //        EncodedBy = table.Column<string>(type: "char(7)", unicode: false, fixedLength: true, maxLength: 7, nullable: true),
            //        DateTimeEncoded = table.Column<DateTime>(type: "datetime", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_tblEvent", x => x.EventId);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "tblJudges",
            //    columns: table => new
            //    {
            //        JudgeId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        JudgeName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_tblJudges", x => x.JudgeId);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "tblParameters",
            //    columns: table => new
            //    {
            //        ParamId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        ParamDescription = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
            //        ParamValue = table.Column<string>(type: "nchar(50)", fixedLength: true, maxLength: 50, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //    });

            //migrationBuilder.CreateTable(
            //    name: "tblRegion",
            //    columns: table => new
            //    {
            //        RegionId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        RegionName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_tblRegion", x => x.RegionId);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "tblStatus",
            //    columns: table => new
            //    {
            //        StatusId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        StatusDescription = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
            //        EncodedBy = table.Column<string>(type: "char(7)", unicode: false, fixedLength: true, maxLength: 7, nullable: true),
            //        DateTimeEncoded = table.Column<DateTime>(type: "datetime", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_tblStatus", x => x.StatusId);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "tblTallyTransaction",
            //    columns: table => new
            //    {
            //        TransId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        EventId = table.Column<int>(type: "int", nullable: true),
            //        JudgeId = table.Column<int>(type: "int", nullable: true),
            //        PhotoId = table.Column<int>(type: "int", nullable: true),
            //        StatusId = table.Column<int>(type: "int", nullable: true),
            //        Score = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
            //        RankNo = table.Column<int>(type: "int", nullable: true),
            //        EncodedBy = table.Column<string>(type: "char(7)", unicode: false, fixedLength: true, maxLength: 7, nullable: true),
            //        DateTimeEncoded = table.Column<DateTime>(type: "datetime", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_tblTallyTransaction", x => x.TransId);
            //    });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "Category");

            //migrationBuilder.DropTable(
            //    name: "DayNumber");

            //migrationBuilder.DropTable(
            //    name: "ImageBatch");

            //migrationBuilder.DropTable(
            //    name: "Judges");

            //migrationBuilder.DropTable(
            //    name: "PhotoLocations");

            //migrationBuilder.DropTable(
            //    name: "PhotoMetaData");

            //migrationBuilder.DropTable(
            //    name: "ProcessParameter");

            //migrationBuilder.DropTable(
            //    name: "SeedControlTotal");

            //migrationBuilder.DropTable(
            //    name: "tblEvent");

            //migrationBuilder.DropTable(
            //    name: "tblJudges");

            //migrationBuilder.DropTable(
            //    name: "tblParameters");

            //migrationBuilder.DropTable(
            //    name: "tblRegion");

            //migrationBuilder.DropTable(
            //    name: "tblStatus");

            //migrationBuilder.DropTable(
            //    name: "tblTallyTransaction");
        }
    }
}
