extern alias SharePoint2016;

using SharePoint2016::Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using UMT.IServices.Banner;

namespace UMT.Services.Banner
{
    public class BannerService : IBannerService
    {
        public static class Modes
        {
            public const string DefaultBanner = "DefaultBanner";
            public const string DefaultBannerWithRedirect = "DefaultBannerWithRedirect";
            public const string CustomBanner = "CustomBanner";
        }

        public void CreateCustomBanner(string siteUrl, string bannerJs)
        {
            using (var context = new ClientContext(siteUrl))
            {
                UserCustomAction autoRedirect = context.Web.UserCustomActions.Add();
                autoRedirect.Name = "MigrationBanner";
                autoRedirect.Title = "Migration Banner";
                autoRedirect.Location = "ScriptLink";
                autoRedirect.Sequence = 50;

                autoRedirect.ScriptBlock = bannerJs;

                autoRedirect.Update();
                context.ExecuteQuery();

                Console.WriteLine("Auto-redirect notification created successfully!");
            }
        }

        public void CreateAutoRedirectNotification(string siteUrl, string redirectUrl = null, int? countdownSeconds = null, string message = null)
        {
            using (var context = new ClientContext(siteUrl))
            {
                UserCustomAction autoRedirect = context.Web.UserCustomActions.Add();
                autoRedirect.Name = redirectUrl == null ? Modes.DefaultBanner : Modes.DefaultBannerWithRedirect;
                autoRedirect.Title = redirectUrl == null ? Modes.DefaultBanner : Modes.DefaultBannerWithRedirect;
                autoRedirect.Location = "ScriptLink";
                autoRedirect.Sequence = 50;

                //string defaultMessage = message ?? "This site is being migrated. You will be redirected to the new location in";

                string banner = $@"
                        var banner = document.createElement('div');
                        banner.style.cssText = 'width:100%;background:#ff6b35;color:white;padding:10px;text-align:center;';
                        banner.innerHTML = '{message.Replace("\r\n", "<br>").Replace("\n", "<br>").Replace("\r", "<br>")}';
                        document.body.insertBefore(banner, document.body.firstChild);";

                string redirect = redirectUrl == null ? "" : $@"var countdown = {countdownSeconds};
                        var redirectUrl = '{redirectUrl}';
                                                
                        var timer = setInterval(function() {{
                            countdown--;

                            if (countdown <= 0) {{
                                clearInterval(timer);
                                window.location.href = redirectUrl;
                            }}
                        }}, 1000);";

                autoRedirect.ScriptBlock = $@"(function() {{
                        {banner}
                        {redirect}
                }})();";

                autoRedirect.Update();
                context.ExecuteQuery();

                Console.WriteLine("Auto-redirect notification created successfully!");
            }
        }

        public void RemoveDefaultBanner(string siteUrl) 
        {
        }

        public void RemoveDefaultBannerWithRedirect(string siteUrl) 
        {
        }

        public void RemoveCustomBanner(string siteUrl)
        { 
        }

        public void RemoveAllOptions(string siteUrl)
        {
            using (var context = new ClientContext(siteUrl))
            {
                // Load web custom actions
                context.Load(context.Web, w => w.UserCustomActions);
                context.ExecuteQuery();

                var actionsToRemove = context.Web.UserCustomActions
                    .Where(a => a.Location == "ScriptLink" &&
                               (a.Name.Equals(Modes.DefaultBanner) || a.Name.Equals(Modes.DefaultBannerWithRedirect) || a.Name.Equals(Modes.CustomBanner) || a.Sequence == 100 || a.Sequence == 50))
                    .ToList();

                foreach (var action in actionsToRemove)
                {
                    action.DeleteObject();
                    Console.WriteLine($"Removing action: {action.Name}");
                }

                if (actionsToRemove.Any())
                {
                    context.ExecuteQuery();
                    Console.WriteLine($"Removed {actionsToRemove.Count} redirect actions.");
                }
                else
                {
                    Console.WriteLine("No redirect actions found to remove.");
                }
            }
        }

        public void RemoveOption(string siteUrl, string customActionName)
        {
            using (var context = new ClientContext(siteUrl))
            {
                // Load web custom actions
                context.Load(context.Web, w => w.UserCustomActions);
                context.ExecuteQuery();

                var actionsToRemove = context.Web.UserCustomActions
                    .Where(a => a.Location == "ScriptLink" &&
                               (a.Name.Equals(Modes.DefaultBanner) || a.Name.Equals(Modes.DefaultBannerWithRedirect) || a.Name.Equals(Modes.CustomBanner)))
                    .ToList();

                foreach (var action in actionsToRemove)
                {
                    action.DeleteObject();
                    Console.WriteLine($"Removing action: {action.Name}");
                }

                if (actionsToRemove.Any())
                {
                    context.ExecuteQuery();
                    Console.WriteLine($"Removed {actionsToRemove.Count} redirect actions.");
                }
                else
                {
                    Console.WriteLine("No redirect actions found to remove.");
                }
            }
        }
    }
}
