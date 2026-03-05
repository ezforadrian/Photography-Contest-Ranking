using Microsoft.AspNetCore.Mvc;
using photoCon.Dto;
using photoCon.Interface;
using System.Security.Cryptography;
using System.Drawing;
using System.Text;
using Newtonsoft.Json;
using photoCon.Models;
using photoCon.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Metadata;


namespace photoCon.Controllers
{
    [Authorize(Roles = "SYSADMIN,ECDUSER,APPADMIN")]
    public class SeedDatabase : Controller
    {

        private readonly ILogger<SeedDatabase> _logger;
        private readonly IRegionRepository _regionRepository;
        private readonly ISeedDatabaseRepository _seedDatabaseRepository;
        private readonly ISeedControlTotalRepository _seedControlTotalRepository;
        private readonly ICategoryRepository _categoryRepository;
        LocalEncryption encryption = new LocalEncryption();

        private readonly RepositoryOperations _repositoryOperations;
        private readonly ConfigurationAppInfo _appInfo;

        const string key_ = "thequickbrownfox";

        public SeedDatabase(
                                    ILogger<SeedDatabase> logger,
                                    IRegionRepository regionRepository
                                    , ISeedDatabaseRepository seedDatabaseRepository
                                    , ISeedControlTotalRepository seedControlTotalRepository
                                    , ICategoryRepository categoryRepository
                                    , RepositoryOperations repositoryOperations
                                    , IOptions<ConfigurationAppInfo> appInfo
                                )
        {
            _logger = logger;
            _regionRepository = regionRepository;
            _seedDatabaseRepository = seedDatabaseRepository;
            _seedControlTotalRepository = seedControlTotalRepository;
            _categoryRepository = categoryRepository;
            _repositoryOperations = repositoryOperations;
            _appInfo = appInfo.Value;
        }

        public async Task<IActionResult> Index()
        {
            string csrfToken = encryption.GenerateCSRFToken(); // GenerateCSRFToken();
            HttpContext.Session.SetString("CSRFToken", csrfToken);

            var region = await _regionRepository.GetAllRegion();
            var category = await _categoryRepository.GetAllCategories();

            SeedInfo seedInfo = new SeedInfo();
            List<SeedInfo> seedInfoList = new List<SeedInfo>();

            int regionId = 0;
            string regionName = "";

            int categoryId = 0;
            string categoryName = "";


            foreach (var region_ in region)
            {
                regionId = region_.RegionId;
                regionName = region_.RegionName;

                foreach (var cat_ in category)
                {
                    categoryId = cat_.CategoryId;
                    categoryName = cat_.CategoryName;

                    SeedControlTotal lastRecord = await _seedControlTotalRepository.GetLastRecordPerRegionandPerCategory(regionId, categoryId);
                    int lastDbCount = 0;
                    int lastSeedCount = 0;
                    DateTime lastSeeded = DateTime.MinValue;
                    if (lastRecord != null)

                    {
                        lastDbCount = (int)lastRecord.DbCountFrom;
                        lastSeedCount = (int)lastRecord.SeededData;
                        lastSeeded = (DateTime)lastRecord.LastUpdate;
                    }





                    SeedInfo newSeedInfo = new SeedInfo
                    {
                        RegionId = regionId,
                        RegionIdHash = encryption.ANEncrypt(regionName, key_),
                        RegionName = regionName,
                        CategoryId = categoryId,
                        CategoryIdHash = encryption.ANEncrypt(categoryName, key_),
                        CategoryName = categoryName,
                        DBCountPerRegionandCategory = _seedControlTotalRepository.CountAllPhotoPerRegionAndCategory(regionId, categoryId),
                        FolderCountPerRegionandCategory = _repositoryOperations.FolderCount(regionName, categoryName),
                        LastSeededOn = lastSeeded,
                        LastDBCount = lastDbCount,
                        LastSeededCount = lastSeedCount,

                    };

                    // Add the newly created SeedInfo object to the list
                    seedInfoList.Add(newSeedInfo);

                }
            }






            return View(seedInfoList);
        }










        [HttpPost]
        public async Task<ActionResult> SeedDataPerRegionAndCategory([FromBody] RegionCategoryHash regionCategoryHash)
        {
            var jsonData = new ServerResponse
            {
                ServerResponseCode = 0,
                ServerResponseMessage = "",
            };

            // Variable declaration for future use in this function
            var regionName = "";
            var categoryName = "";
            var regionId_ = 0;
            var categoryId_ = 0;

            // Validation of the parameter --checking if null
            if (regionCategoryHash == null)
            {
                jsonData.ServerResponseCode = 400;
                jsonData.ServerResponseMessage = "Null values are not accepted";
                return Ok(JsonConvert.SerializeObject(jsonData));
            }

            // Validation of the parameter --checking if correct encryption
            try
            {
                regionName = encryption.ANDecrypt(regionCategoryHash.RegionHash, key_);
                categoryName = encryption.ANDecrypt(regionCategoryHash.CategoryHash, key_);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                jsonData.ServerResponseCode = 410;
                jsonData.ServerResponseMessage = "Incorrect Parameters";
                return Ok(JsonConvert.SerializeObject(jsonData));
            }

            // Validation of the parameter --check if regionName exist
            if (!_regionRepository.HasRegion(regionName))
            {
                jsonData.ServerResponseCode = 420;
                jsonData.ServerResponseMessage = "Region Not Found";
                return Ok(JsonConvert.SerializeObject(jsonData));
            }

            // Validation of the parameter --check if category exist
            if (!_categoryRepository.HasCategory(categoryName))
            {
                jsonData.ServerResponseCode = 430;
                jsonData.ServerResponseMessage = "Category Not Found";
                return Ok(JsonConvert.SerializeObject(jsonData));
            }

            // If parameter is OK
            try
            {
                var fullfolderpath = _appInfo.AppRepository + "1_Regional\\" + regionName + "\\" + categoryName + "\\";
                string[] directories = Directory.GetDirectories(fullfolderpath);

                regionId_ = _regionRepository.GetRegionId(regionName);
                categoryId_ = _categoryRepository.GetCategoryId(categoryName);

                var DBCountFrom = _seedControlTotalRepository.CountAllPhotoPerRegionAndCategory(regionId_, categoryId_);

                if (!Directory.Exists(fullfolderpath))
                {
                    jsonData.ServerResponseCode = 440;
                    jsonData.ServerResponseMessage = "Folder does not exist.";
                    return Ok(JsonConvert.SerializeObject(jsonData));
                }

                string[] jpgFiles = Directory.GetFiles(fullfolderpath, "*.jpg").OrderBy(f => f).ToArray();
                string[] jpegFiles = Directory.GetFiles(fullfolderpath, "*.jpeg").OrderBy(f => f).ToArray();
                string[] imageFiles = jpgFiles.Concat(jpegFiles).ToArray();

                var imageMetadatas = new List<photoCon.Dto.ImageMetadata>(); // Create a list to store metadata

                foreach (var filePath in imageFiles)
                {
                    try
                    {
                        using (Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(filePath))
                        {
                            var metadata = new photoCon.Dto.ImageMetadata
                            {
                                FileName = Path.GetFileName(filePath),
                                ImageNumber = 0,
                                ImageRegionId = regionId_,
                                ImageDirectory = fullfolderpath + Path.GetFileName(filePath),
                                ImageNumberHash = GenerateHash(fullfolderpath + Path.GetFileName(filePath)),
                                ImageBytes = null,
                                ImageURL = _appInfo.AppURLRepository + @"1_Regional/" + regionName + @"/" + categoryName + @"/" + Path.GetFileName(filePath),
                                Dimension = $"{image.Width}x{image.Height}",
                                Width = image.Width.ToString(),
                                Height = image.Height.ToString(),
                                HorizontalResolution = image.Metadata.HorizontalResolution.ToString(),
                                VerticalResolution = image.Metadata.VerticalResolution.ToString(),
                                BitDepth = image.PixelType.BitsPerPixel.ToString(),
                                ResolutionUnit = image.Metadata.ResolutionUnits.ToString(),
                            };

                            // Process EXIF metadata
                            var exifProfile = image.Metadata.ExifProfile;
                            if (exifProfile != null)
                            {
                                foreach (var value in exifProfile.Values)
                                {
                                    string propertyValue = value.GetValue().ToString();
                                    switch (value.Tag.ToString())
                                    {
                                        case "Make":
                                            metadata.CameraMaker = propertyValue;
                                            break;
                                        case "Model":
                                            metadata.CameraModel = propertyValue;
                                            break;
                                        case "FNumber":
                                            metadata.FStop = propertyValue;
                                            break;
                                        case "ExposureTime":
                                            metadata.ExposureTime = propertyValue;
                                            break;
                                        case "ISOSpeedRatings":
                                            metadata.ISOSpeed = propertyValue;
                                            break;
                                        case "FocalLength":
                                            metadata.FocalLength = propertyValue;
                                            break;
                                        case "MaxApertureValue":
                                            metadata.MaxAperture = propertyValue;
                                            break;
                                        case "MeteringMode":
                                            metadata.MeteringMode = propertyValue;
                                            break;
                                        case "Flash":
                                            metadata.FlashMode = propertyValue;
                                            break;
                                        case "FocalLengthIn35mmFilm":
                                            metadata.MmFocalLength = propertyValue;
                                            break;
                                        case "BitsPerSample":
                                            metadata.BitDepth = propertyValue;
                                            break;
                                    }
                                }
                            }

                            // Set default values for metadata if missing
                            metadata.CameraMaker ??= "-";
                            metadata.CameraModel ??= "-";
                            metadata.FStop ??= "-";
                            metadata.ExposureTime ??= "-";
                            metadata.ISOSpeed ??= "-";
                            metadata.FocalLength ??= "-";
                            metadata.MaxAperture ??= "-";
                            metadata.MeteringMode ??= "-";
                            metadata.FlashMode ??= "-";
                            metadata.MmFocalLength ??= "-";
                            metadata.BitDepth ??= "-";

                            imageMetadatas.Add(metadata); // Add the fetched metadata to the list
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing file {FilePath}. Skipping file.", filePath);
                        continue; // Skip this file and move to the next one
                    }
                }

                var seededCount = 0;
                foreach (var metadata in imageMetadatas)
                {
                    try
                    {
                        var photoLocationExist = _seedDatabaseRepository.IsPhotoLocationExist(metadata.ImageNumberHash, metadata.ImageDirectory);
                        var photoMetadataExist = _seedDatabaseRepository.IsPhotoMetaDataExist(metadata.ImageNumberHash, metadata.FileName, metadata.ImageURL);

                        if (!photoLocationExist && !photoMetadataExist)
                        {
                            var photoLocation_ = new PhotoLocation
                            {
                                HashPhotoId = metadata.ImageNumberHash,
                                RepositoryLocation = metadata.ImageDirectory,
                                RegionId = metadata.ImageRegionId,
                                CategoryId = categoryId_,
                                PhotoCode = "10",
                                EncodedBy = null,
                                DateTimeEncoded = DateTime.Now
                            };

                            var photoMetadata_ = new PhotoMetaDatum
                            {
                                HashPhotoId = metadata.ImageNumberHash,
                                FileName = metadata.FileName,
                                Dimension = metadata.Dimension,
                                Width = metadata.Width,
                                Height = metadata.Height,
                                HorizontalResolution = metadata.HorizontalResolution,
                                VerticalResolution = metadata.VerticalResolution,
                                BitDepth = metadata.BitDepth,
                                ResolutionUnit = metadata.ResolutionUnit,
                                ImageUrl = metadata.ImageURL,
                                CameraMaker = metadata.CameraMaker,
                                CameraModel = metadata.CameraModel,
                                Fstop = metadata.FStop,
                                ExposureTime = metadata.ExposureTime,
                                Isospeed = metadata.ISOSpeed,
                                FocalLength = metadata.FocalLength,
                                MaxAperture = metadata.MaxAperture,
                                MeteringMode = metadata.MeteringMode,
                                FlashMode = metadata.FlashMode,
                                MmFocalLength = metadata.MmFocalLength
                            };

                            try
                            {
                                _seedDatabaseRepository.CreatePhotoLocation(photoLocation_);
                                _seedDatabaseRepository.CreatePhotoMetadata(photoMetadata_);
                                seededCount += 1;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                                jsonData.ServerResponseCode = 500;
                                jsonData.ServerResponseMessage = $"An error occurred: Refresh application.";
                                return Ok(JsonConvert.SerializeObject(jsonData));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                        jsonData.ServerResponseCode = 500;
                        jsonData.ServerResponseMessage = $"An error occurred: Refresh application";
                        return Ok(JsonConvert.SerializeObject(jsonData));
                    }
                }

                try
                {
                    // Create SeedControlTotal
                    var seedControlTotal = new SeedControlTotal
                    {
                        RegionId = regionId_,
                        CategoryId = categoryId_,
                        LastUpdate = DateTime.Now,
                        DbCountFrom = DBCountFrom,
                        SeededData = seededCount,
                    };

                    _seedControlTotalRepository.CreateSeedControlTotal(seedControlTotal);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                    jsonData.ServerResponseCode = 510;
                    jsonData.ServerResponseMessage = $"An error occurred: Refresh application.";
                    return Ok(JsonConvert.SerializeObject(jsonData));
                }

                jsonData.ServerResponseCode = 200;
                jsonData.ServerResponseMessage = DateTime.Now.ToString();
                jsonData.DBCountTo = _seedControlTotalRepository.CountAllPhotoPerRegionAndCategory(regionId_, categoryId_);
                jsonData.FolderCount = _repositoryOperations.FolderCount(regionName, categoryName);
                jsonData.imageMetadata = imageMetadatas;

                return Ok(JsonConvert.SerializeObject(jsonData));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                jsonData.ServerResponseCode = 500;
                jsonData.ServerResponseMessage = $"An error occurred: Refresh application";
                return Ok(JsonConvert.SerializeObject(jsonData));
            }
        }

        


        private string DecodePropertyValue(byte[] value, short dataType)
        {
            switch (dataType)
            {
                case 1: // BYTE
                case 7: // UNDEFINED
                    return Encoding.ASCII.GetString(value).TrimEnd('\0');
                case 2: // ASCII
                    return Encoding.ASCII.GetString(value).TrimEnd('\0');
                case 3: // SHORT
                    if (value.Length >= 2)
                        return BitConverter.ToInt16(value, 0).ToString();
                    else
                        return "-";
                case 4: // LONG
                    if (value.Length >= 4)
                        return BitConverter.ToInt32(value, 0).ToString();
                    else
                        return "-";
                case 5: // RATIONAL
                    if (value.Length >= 8)
                    {
                        int numerator = BitConverter.ToInt32(value, 0);
                        int denominator = BitConverter.ToInt32(value, 4);
                        return $"{numerator}/{denominator}";
                    }
                    else
                        return "-";
                default: // Default to ASCII for other data types
                    return "-";
            }
        }




        private async Task<byte[]> GetImageBytesAsync(string imagePath)
        {
            try
            {
                // Read image bytes asynchronously
                byte[] imageBytes;
                using (FileStream fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
                {
                    MemoryStream memoryStream = new MemoryStream();
                    await fileStream.CopyToAsync(memoryStream);
                    imageBytes = memoryStream.ToArray();
                }
                return imageBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                // Handle exceptions appropriately (e.g., log, return null, throw)
                Console.WriteLine($"Error reading image bytes from {imagePath}: Error Encountered");
                return null;
            }
        }

        private string GenerateHash(string input)
        {
            using (var algorithm = MD5.Create())
            {
                byte[] hashBytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        //private string GenerateHash(string input)
        //{
        //    using (var algorithm = SHA256.Create())
        //    {
        //        byte[] hashBytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
        //        StringBuilder sb = new StringBuilder();
        //        foreach (byte b in hashBytes)
        //        {
        //            sb.Append(b.ToString("x2"));
        //        }
        //        return sb.ToString();
        //    }
        //}

        //private string GenerateHash(string input, int length = 10)
        //{
        //    using (var algorithm = SHA256.Create())
        //    {
        //        byte[] hashBytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
        //        StringBuilder sb = new StringBuilder();
        //        foreach (byte b in hashBytes)
        //        {
        //            sb.Append(b.ToString("x2"));
        //        }
        //        string fullHash = sb.ToString();
        //        return fullHash.Substring(0, Math.Min(length, fullHash.Length));
        //    }
        //}














    }
}


