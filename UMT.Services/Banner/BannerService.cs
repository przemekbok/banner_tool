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
            public const string LegacyMigrationBanner = "Migration Banner"; // Legacy name with space
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
                    // AGGRESSIVE cleanup - remove ALL possible banner actions
                    RemoveAllPossibleBanners(context, methodName);

                    UserCustomAction customBanner = context.Web.UserCustomActions.Add();
                    customBanner.Name = Modes.MigrationBanner;
                    customBanner.Title = "Migration Banner";
                    customBanner.Location = "ScriptLink";
                    customBanner.Sequence = 1; // Use sequence 1 to ensure priority
                    customBanner.Description = "Auto-generated migration banner - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

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
                    // AGGRESSIVE cleanup - remove ALL possible banner actions
                    RemoveAllPossibleBanners(context, methodName);

                    UserCustomAction autoRedirect = context.Web.UserCustomActions.Add();
                    autoRedirect.Name = redirectUrl == null ? Modes.DefaultBanner : Modes.DefaultBannerWithRedirect;
                    autoRedirect.Title = redirectUrl == null ? Modes.DefaultBanner : Modes.DefaultBannerWithRedirect;
                    autoRedirect.Location = "ScriptLink";
                    autoRedirect.Sequence = 1; // Use sequence 1 to ensure priority
                    autoRedirect.Description = "Auto-generated banner - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

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

        private void RemoveAllPossibleBanners(ClientContext context, string callingMethod)
        {
            try
            {
                _logService.LogInfo("Starting aggressive cleanup of ALL possible banner actions", null, callingMethod);

                // Load web custom actions
                context.Load(context.Web, w => w.UserCustomActions);
                context.ExecuteQuery();

                var allActions = context.Web.UserCustomActions.ToList();
                _logService.LogInfo($"Found {allActions.Count} total custom actions on site", null, callingMethod);

                // Remove by known names
                var actionsToRemoveByName = allActions
                    .Where(a => a.Location == "ScriptLink" && (
                        a.Name.Equals(Modes.DefaultBanner, StringComparison.OrdinalIgnoreCase) || 
                        a.Name.Equals(Modes.DefaultBannerWithRedirect, StringComparison.OrdinalIgnoreCase) || 
                        a.Name.Equals(Modes.CustomBanner, StringComparison.OrdinalIgnoreCase) ||
                        a.Name.Equals(Modes.MigrationBanner, StringComparison.OrdinalIgnoreCase) ||
                        a.Name.Equals(Modes.LegacyMigrationBanner, StringComparison.OrdinalIgnoreCase) ||
                        a.Title.Equals("Migration Banner", StringComparison.OrdinalIgnoreCase) ||
                        a.Name.Contains("Migration", StringComparison.OrdinalIgnoreCase) ||
                        a.Name.Contains("Banner", StringComparison.OrdinalIgnoreCase)
                    ))
                    .ToList();

                // Remove by sequence numbers used by our app
                var actionsToRemoveBySequence = allActions
                    .Where(a => a.Location == "ScriptLink" && (
                        a.Sequence == 1 || 
                        a.Sequence == 50 || 
                        a.Sequence == 100
                    ))
                    .ToList();

                // Remove by script content patterns (check for our specific script patterns)
                var actionsToRemoveByContent = allActions
                    .Where(a => a.Location == "ScriptLink" && !string.IsNullOrEmpty(a.ScriptBlock) && (
                        a.ScriptBlock.Contains("migrationRedirectRunning") ||
                        a.ScriptBlock.Contains("migration-modal") ||
                        a.ScriptBlock.Contains("data-migration-modal") ||
                        a.ScriptBlock.Contains("Migration redirect") ||
                        a.ScriptBlock.Contains("redirectUrl") ||
                        a.ScriptBlock.Contains("This site is being migrated")
                    ))
                    .ToList();

                // Combine all lists and remove duplicates
                var allActionsToRemove = actionsToRemoveByName
                    .Union(actionsToRemoveBySequence)
                    .Union(actionsToRemoveByContent)
                    .Distinct()
                    .ToList();

                if (allActionsToRemove.Any())
                {
                    _logService.LogInfo($"Found {allActionsToRemove.Count} banner actions to remove", null, callingMethod);

                    foreach (var action in allActionsToRemove)
                    {
                        action.DeleteObject();
                        _logService.LogInfo($"Queued banner action for removal", 
                            $"Name: {action.Name}, Title: {action.Title}, Sequence: {action.Sequence}, Description: {action.Description}", 
                            callingMethod);
                    }

                    context.ExecuteQuery();
                    _logService.LogInfo($"Successfully removed {allActionsToRemove.Count} banner actions", null, callingMethod);
                }
                else
                {
                    _logService.LogInfo("No banner actions found to remove", null, callingMethod);
                }

                // Double-check by loading again and logging remaining actions
                context.Load(context.Web, w => w.UserCustomActions);
                context.ExecuteQuery();
                
                var remainingActions = context.Web.UserCustomActions
                    .Where(a => a.Location == "ScriptLink")
                    .ToList();
                
                _logService.LogInfo($"After cleanup: {remainingActions.Count} ScriptLink actions remain", null, callingMethod);
                foreach (var action in remainingActions)
                {
                    _logService.LogInfo($"Remaining action: {action.Name} (Seq: {action.Sequence})", null, callingMethod);
                }
            }
            catch (Exception ex)
            {
                _logService.LogError($"Failed during aggressive banner cleanup", $"Error: {ex.Message}", callingMethod);
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
                    RemoveAllPossibleBanners(context, methodName);
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
                    RemoveAllPossibleBanners(context, methodName);
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
