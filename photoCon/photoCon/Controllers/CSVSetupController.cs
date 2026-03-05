using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using photoCon.Interface;
using photoCon.Models;

using System.Globalization;


namespace photoCon.Controllers
{
    [Authorize(Roles = "SYSADMIN,APPADMIN,ECDUSER")]
    public class CSVSetupController : Controller
    {
        private readonly ILogger<CSVSetupController> _logger;
        private readonly ICSVDetailsRepository _cSVDetailsRepository;
        private static int uploadedCount = 0;
        public CSVSetupController(ILogger<CSVSetupController> logger, ICSVDetailsRepository cSVDetailsRepository)
        {
            _logger = logger;
            _cSVDetailsRepository = cSVDetailsRepository;
        }
        public IActionResult Index()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            if (Path.GetExtension(file.FileName).ToLower() != ".csv")
            {
                return BadRequest("File is not a CSV");
            }

            var expectedFileName = "ppc.csv";
            if (file.FileName.ToLower() != expectedFileName)
            {
                return BadRequest($"Invalid filename. Please upload a file with the name '{expectedFileName.ToUpper()}'");
            }

            //var expectedHeaders = _cSVDetailsRepository.GetDatabaseColumnNames(); // Fetch the expected headers from the database

            using (var stream = file.OpenReadStream())
            using (var reader = new StreamReader(stream))
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true, // Ensure this is set to true if your file has headers
                    MissingFieldFound = null, // Optionally, handle missing fields
                    BadDataFound = null // Optionally, handle bad data
                };

                using (var csv = new CsvReader(reader, config))
                {
                    try
                    {
                        var records = csv.GetRecords<CSV_Details>().ToList();
                        uploadedCount = 0;

                        foreach (var record in records)
                        {
                            var existingRecord = _cSVDetailsRepository.isRecordExist(record.photoname);

                            if (existingRecord == null)
                            {
                                uploadedCount++;
                                _cSVDetailsRepository.AddCSVPhotoDetails(record);
                            }
                            else
                            {
                                _logger.LogWarning($"Data with file name '{record.photoname}' already exists in the database.");
                            }
                        }
                        return Json(new { success = true, count = uploadedCount });
                    }
                    catch (Exception ex) 
                    { 
                        return BadRequest("Please check you CSV file.");
                    }
                }
            }
  
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCSV()
        {

            try
            {
                _cSVDetailsRepository.UpdateCSVDetails(@User.Identity.Name);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                return Json(new { success = false });
            }

            return Json(new { success = true, UploadedCount = uploadedCount });
        }
    }


}
