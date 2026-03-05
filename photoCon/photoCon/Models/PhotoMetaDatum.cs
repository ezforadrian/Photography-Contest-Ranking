using System;
using System.Collections.Generic;

namespace photoCon.Models
{
    public partial class PhotoMetaDatum
    {
        public int Index { get; set; }
        public string HashPhotoId { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string Dimension { get; set; } = null!;
        public string Width { get; set; } = null!;
        public string Height { get; set; } = null!;
        public string HorizontalResolution { get; set; } = null!;
        public string VerticalResolution { get; set; } = null!;
        public string BitDepth { get; set; } = null!;
        public string ResolutionUnit { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public string CameraMaker { get; set; } = null!;
        public string CameraModel { get; set; } = null!;
        public string Fstop { get; set; } = null!;
        public string ExposureTime { get; set; } = null!;
        public string Isospeed { get; set; } = null!;
        public string FocalLength { get; set; } = null!;
        public string MaxAperture { get; set; } = null!;
        public string MeteringMode { get; set; } = null!;
        public string FlashMode { get; set; } = null!;
        public string MmFocalLength { get; set; } = null!;
    }
}
