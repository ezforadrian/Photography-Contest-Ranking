using Microsoft.Extensions.Options;
using photoCon.Dto;

namespace photoCon.Helper
{
    public class GetAppInfo
    {
        private readonly ConfigurationAppInfo _appInfo;

        public GetAppInfo(IOptions<ConfigurationAppInfo> appInfo)
        {
            _appInfo = appInfo.Value;
        }


    }
}
