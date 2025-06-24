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
                    RemoveAllExistingBanners(context, methodName);

                    UserCustomAction customBanner = context.Site.UserCustomActions.Add();
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
                    RemoveAllExistingBanners(context, methodName);

                    UserCustomAction autoRedirect = context.Site.UserCustomActions.Add();
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

        private void RemoveAllExistingBanners(ClientContext context, string callingMethod)
        {
            try
            {
                _logService.LogInfo("Checking for existing banner actions to remove", null, callingMethod);

                // Load ALL custom actions (not just web level)
                context.Load(context.Web, w => w.UserCustomActions);
                context.Load(context.Site, s => s.UserCustomActions);
                context.ExecuteQuery();

                var allActionsToRemove = new List<UserCustomAction>();

                // Check web-level custom actions
                var webActionsToRemove = context.Web.UserCustomActions
                    .Where(a => IsBannerAction(a))
                    .ToList();
                allActionsToRemove.AddRange(webActionsToRemove);

                // Check site-level custom actions
                var siteActionsToRemove = context.Site.UserCustomActions
                    .Where(a => IsBannerAction(a))
                    .ToList();
                allActionsToRemove.AddRange(siteActionsToRemove);

                if (allActionsToRemove.Any())
                {
                    _logService.LogInfo($"Found {allActionsToRemove.Count} existing banner actions to remove", 
                        $"Web actions: {webActionsToRemove.Count}, Site actions: {siteActionsToRemove.Count}", callingMethod);

                    foreach (var action in allActionsToRemove)
                    {
                        try
                        {
                            action.DeleteObject();
                            _logService.LogInfo($"Queued existing banner action for removal", 
                                $"Name: {action.Name}, Title: {action.Title}, Location: {action.Location}, Sequence: {action.Sequence}", callingMethod);
                        }
                        catch (Exception ex)
                        {
                            _logService.LogWarning($"Failed to queue action for removal", $"Action: {action.Name}, Error: {ex.Message}", callingMethod);
                        }
                    }

                    context.ExecuteQuery();
                    _logService.LogInfo($"Successfully removed {allActionsToRemove.Count} existing banner actions", null, callingMethod);
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

        private bool IsBannerAction(UserCustomAction action)
        {
            if (action == null) return false;

            // Check location
            if (action.Location != "ScriptLink") return false;

            // Check by name (known banner action names)
            var knownBannerNames = new[]
            {
                Modes.DefaultBanner,
                Modes.DefaultBannerWithRedirect,
                Modes.CustomBanner,
                Modes.MigrationBanner,
                "Migration Banner" // Title-based check
            };

            if (knownBannerNames.Any(name => 
                string.Equals(action.Name, name, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(action.Title, name, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            // Check by sequence (banner sequences)
            if (action.Sequence == 50 || action.Sequence == 100)
            {
                return true;
            }

            // Check script content for migration-related keywords
            if (!string.IsNullOrEmpty(action.ScriptBlock))
            {
                var scriptLower = action.ScriptBlock.ToLower();
                var migrationKeywords = new[]
                {
                    "migration",
                    "migrationredirect",
                    "data-migration-banner",
                    "data-migration-modal",
                    "ff6b35", // Orange banner color
                    "this site is being migrated"
                };

                if (migrationKeywords.Any(keyword => scriptLower.Contains(keyword)))
                {
                    return true;
                }
            }

            return false;
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
                    RemoveAllExistingBanners(context, methodName);
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
                    RemoveAllExistingBanners(context, methodName);
                    _logService.LogSuccess($"Successfully removed banner actions", $"Site: {siteUrl}", methodName);
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
