using Microsoft.Extensions.Options;
using photoCon.Dto;

namespace photoCon.Helper
{
    public class RepositoryOperations 
    {
        private readonly ConfigurationAppInfo _appInfo;

        public RepositoryOperations(IOptions<ConfigurationAppInfo> appInfo)
        {
            _appInfo = appInfo.Value;
        }

        public int FolderCount(string RegionName, string CategoryName)
        {
            string folderPath = _appInfo.AppRepository + "1_Regional\\";  
            RegionName = RegionName + "\\";
            CategoryName = CategoryName + "\\";

            var fileCount = (from file in Directory.EnumerateFiles(folderPath + RegionName + CategoryName, "*.JPG", SearchOption.TopDirectoryOnly)
                             select file).Count();
            return fileCount;
        }
    }
}
