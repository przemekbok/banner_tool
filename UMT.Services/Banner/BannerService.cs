extern alias SharePoint2016;

using SharePoint2016::Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using UMT.IServices;
using UMT.IServices.Banner;

namespace UMT.Services.Banner
{
    public class BannerService : IBannerService
    {
        private readonly ILogService _logService;

        public static class Modes
        {
            public const string DefaultBanner = "DefaultBanner";
            public const string DefaultBannerWithRedirect = "DefaultBannerWithRedirect";
            public const string CustomBanner = "CustomBanner";
            public const string MigrationBanner = "MigrationBanner";
        }

        public BannerService(ILogService logService)
        {
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public void CreateCustomBanner(string siteUrl, string bannerJs)
        {
            const string methodName = nameof(CreateCustomBanner);
            
            try
            {
                _logService.LogInfo($"Starting custom banner creation", $"Site: {siteUrl}", methodName);

                using (var context = new ClientContext(siteUrl))
                {
                    // First, remove any existing banner actions to prevent duplicates
                    RemoveExistingBanners(context, methodName);

                    UserCustomAction customBanner = context.Web.UserCustomActions.Add();
                    customBanner.Name = Modes.MigrationBanner;
                    customBanner.Title = "Migration Banner";
                    customBanner.Location = "ScriptLink";
                    customBanner.Sequence = 50;

                    customBanner.ScriptBlock = bannerJs;

                    customBanner.Update();
                    context.ExecuteQuery();

                    _logService.LogSuccess($"Custom banner created successfully", $"Site: {siteUrl}, Action Name: {Modes.MigrationBanner}", methodName);
                }
            }
            catch (Exception ex)
            {
                _logService.LogError($"Failed to create custom banner", $"Site: {siteUrl}, Error: {ex.Message}", methodName);
                throw;
            }
        }

        public void CreateAutoRedirectNotification(string siteUrl, string redirectUrl = null, int? countdownSeconds = null, string message = null)
        {
            const string methodName = nameof(CreateAutoRedirectNotification);
            
            try
            {
                var bannerType = redirectUrl == null ? "Default Banner" : "Banner with Redirect";
                _logService.LogInfo($"Starting {bannerType} creation", $"Site: {siteUrl}, Redirect: {redirectUrl ?? "None"}, Countdown: {countdownSeconds?.ToString() ?? "N/A"}", methodName);

                using (var context = new ClientContext(siteUrl))
                {
                    // First, remove any existing banner actions to prevent duplicates
                    RemoveExistingBanners(context, methodName);

                    UserCustomAction autoRedirect = context.Web.UserCustomActions.Add();
                    autoRedirect.Name = redirectUrl == null ? Modes.DefaultBanner : Modes.DefaultBannerWithRedirect;
                    autoRedirect.Title = redirectUrl == null ? Modes.DefaultBanner : Modes.DefaultBannerWithRedirect;
                    autoRedirect.Location = "ScriptLink";
                    autoRedirect.Sequence = 50;

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

                    _logService.LogSuccess($"{bannerType} created successfully", $"Site: {siteUrl}, Action Name: {autoRedirect.Name}", methodName);
                }
            }
            catch (Exception ex)
            {
                _logService.LogError($"Failed to create auto-redirect notification", $"Site: {siteUrl}, Error: {ex.Message}", methodName);
                throw;
            }
        }

        private void RemoveExistingBanners(ClientContext context, string callingMethod)
        {
            try
            {
                _logService.LogInfo("Checking for existing banner actions to remove", null, callingMethod);

                // Load web custom actions
                context.Load(context.Web, w => w.UserCustomActions);
                context.ExecuteQuery();

                var actionsToRemove = context.Web.UserCustomActions
                    .Where(a => a.Location == "ScriptLink" &&
                               (a.Name.Equals(Modes.DefaultBanner) || 
                                a.Name.Equals(Modes.DefaultBannerWithRedirect) || 
                                a.Name.Equals(Modes.CustomBanner) ||
                                a.Name.Equals(Modes.MigrationBanner) ||
                                a.Sequence == 100 || 
                                a.Sequence == 50))
                    .ToList();

                if (actionsToRemove.Any())
                {
                    _logService.LogInfo($"Found {actionsToRemove.Count} existing banner actions to remove", null, callingMethod);

                    foreach (var action in actionsToRemove)
                    {
                        action.DeleteObject();
                        _logService.LogInfo($"Queued existing banner action for removal", $"Action: {action.Name}, Sequence: {action.Sequence}", callingMethod);
                    }

                    context.ExecuteQuery();
                    _logService.LogInfo($"Successfully removed {actionsToRemove.Count} existing banner actions", null, callingMethod);
                }
                else
                {
                    _logService.LogInfo("No existing banner actions found to remove", null, callingMethod);
                }
            }
            catch (Exception ex)
            {
                _logService.LogWarning($"Failed to remove existing banners", $"Error: {ex.Message}", callingMethod);
                // Don't throw here as this is a cleanup operation - we can continue with creating the new banner
            }
        }

        public void RemoveDefaultBanner(string siteUrl) 
        {
            const string methodName = nameof(RemoveDefaultBanner);
            _logService.LogWarning("Method not implemented", $"Site: {siteUrl}", methodName);
        }

        public void RemoveDefaultBannerWithRedirect(string siteUrl) 
        {
            const string methodName = nameof(RemoveDefaultBannerWithRedirect);
            _logService.LogWarning("Method not implemented", $"Site: {siteUrl}", methodName);
        }

        public void RemoveCustomBanner(string siteUrl)
        { 
            const string methodName = nameof(RemoveCustomBanner);
            _logService.LogWarning("Method not implemented", $"Site: {siteUrl}", methodName);
        }

        public void RemoveAllOptions(string siteUrl)
        {
            const string methodName = nameof(RemoveAllOptions);
            
            try
            {
                _logService.LogInfo($"Starting removal of all banner options", $"Site: {siteUrl}", methodName);

                using (var context = new ClientContext(siteUrl))
                {
                    RemoveExistingBanners(context, methodName);
                    _logService.LogSuccess($"Successfully removed all banner actions", $"Site: {siteUrl}", methodName);
                }
            }
            catch (Exception ex)
            {
                _logService.LogError($"Failed to remove banner options", $"Site: {siteUrl}, Error: {ex.Message}", methodName);
                throw;
            }
        }

        public void RemoveOption(string siteUrl, string customActionName)
        {
            const string methodName = nameof(RemoveOption);
            
            try
            {
                _logService.LogInfo($"Starting removal of specific banner option", $"Site: {siteUrl}, Action: {customActionName}", methodName);

                using (var context = new ClientContext(siteUrl))
                {
                    // Load web custom actions
                    context.Load(context.Web, w => w.UserCustomActions);
                    context.ExecuteQuery();

                    var actionsToRemove = context.Web.UserCustomActions
                        .Where(a => a.Location == "ScriptLink" &&
                                   (a.Name.Equals(Modes.DefaultBanner) || a.Name.Equals(Modes.DefaultBannerWithRedirect) || a.Name.Equals(Modes.CustomBanner)))
                        .ToList();

                    if (actionsToRemove.Any())
                    {
                        _logService.LogInfo($"Found {actionsToRemove.Count} actions to remove", $"Site: {siteUrl}", methodName);

                        foreach (var action in actionsToRemove)
                        {
                            action.DeleteObject();
                            _logService.LogInfo($"Queued action for removal", $"Action: {action.Name}", methodName);
                        }

                        context.ExecuteQuery();
                        _logService.LogSuccess($"Successfully removed {actionsToRemove.Count} banner actions", $"Site: {siteUrl}", methodName);
                    }
                    else
                    {
                        _logService.LogInfo("No banner actions found to remove", $"Site: {siteUrl}", methodName);
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.LogError($"Failed to remove specific banner option", $"Site: {siteUrl}, Action: {customActionName}, Error: {ex.Message}", methodName);
                throw;
            }
        }
    }
}
