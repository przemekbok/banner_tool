using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UMT.IServices.Banner
{
    public interface IBannerService
    {
        void CreateCustomBanner(string siteUrl, string bannerJs);
        void CreateAutoRedirectNotification(string siteUrl, string redirectUrl = null, int? countdownSeconds = null, string message = null);
        void RemoveAllOptions(string siteUrl);
    }
}
