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

        public void CreateCustomBanner(string siteUrl, string bannerJs, bool applyToSubsites = false)
        {
            const string methodName = nameof(CreateCustomBanner);
            
            try
            {
                _logService.LogInfo($"Starting custom banner creation", $"Site: {siteUrl}, Apply to subsites: {applyToSubsites}", methodName);

                using (var context = new ClientContext(siteUrl))
                {
                    // Apply to main site
                    ApplyBannerToSite(context, bannerJs, Modes.MigrationBanner, "Migration Banner", methodName);

                    // Apply to subsites if requested
                    if (applyToSubsites)
                    {
                        var subsites = GetAllSubsites(context);
                        _logService.LogInfo($"Found {subsites.Count} subsites to process", null, methodName);

                        foreach (var subsite in subsites)
                        {
                            try
                            {
                                using (var subsiteContext = new ClientContext(subsite))
                                {
                                    ApplyBannerToSite(subsiteContext, bannerJs, Modes.MigrationBanner, "Migration Banner", methodName, subsite);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logService.LogWarning($"Failed to apply banner to subsite", $"Subsite: {subsite}, Error: {ex.Message}", methodName);
                            }
                        }
                    }

                    _logService.LogSuccess($"Custom banner creation completed", $"Site: {siteUrl}, Subsites processed: {applyToSubsites}", methodName);
                }
            }
            catch (Exception ex)
            {
                _logService.LogError($"Failed to create custom banner", $"Site: {siteUrl}, Error: {ex.Message}", methodName);
                throw;
            }
        }

        public void CreateAutoRedirectNotification(string siteUrl, string redirectUrl = null, int? countdownSeconds = null, string message = null, bool applyToSubsites = false)
        {
            const string methodName = nameof(CreateAutoRedirectNotification);
            
            try
            {
                var bannerType = redirectUrl == null ? "Default Banner" : "Banner with Redirect";
                _logService.LogInfo($"Starting {bannerType} creation", $"Site: {siteUrl}, Redirect: {redirectUrl ?? "None"}, Countdown: {countdownSeconds?.ToString() ?? "N/A"}, Apply to subsites: {applyToSubsites}", methodName);

                using (var context = new ClientContext(siteUrl))
                {
                    var actionName = redirectUrl == null ? Modes.DefaultBanner : Modes.DefaultBannerWithRedirect;
                    var actionTitle = redirectUrl == null ? Modes.DefaultBanner : Modes.DefaultBannerWithRedirect;

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

                    string scriptBlock = $@"(function() {{
                        {banner}
                        {redirect}
                }})();";

                    // Apply to main site
                    ApplyBannerToSite(context, scriptBlock, actionName, actionTitle, methodName);

                    // Apply to subsites if requested
                    if (applyToSubsites)
                    {
                        var subsites = GetAllSubsites(context);
                        _logService.LogInfo($"Found {subsites.Count} subsites to process", null, methodName);

                        foreach (var subsite in subsites)
                        {
                            try
                            {
                                using (var subsiteContext = new ClientContext(subsite))
                                {
                                    ApplyBannerToSite(subsiteContext, scriptBlock, actionName, actionTitle, methodName, subsite);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logService.LogWarning($"Failed to apply banner to subsite", $"Subsite: {subsite}, Error: {ex.Message}", methodName);
                            }
                        }
                    }

                    _logService.LogSuccess($"{bannerType} creation completed", $"Site: {siteUrl}, Subsites processed: {applyToSubsites}", methodName);
                }
            }
            catch (Exception ex)
            {
                _logService.LogError($"Failed to create auto-redirect notification", $"Site: {siteUrl}, Error: {ex.Message}", methodName);
                throw;
            }
        }

        private void ApplyBannerToSite(ClientContext context, string scriptBlock, string actionName, string actionTitle, string callingMethod, string siteUrl = null)
        {
            try
            {
                var siteName = siteUrl ?? context.Url;
                _logService.LogInfo($"Applying banner to site", $"Site: {siteName}", callingMethod);

                // First, remove any existing banner actions to prevent duplicates
                RemoveAllExistingBanners(context, callingMethod);

                UserCustomAction banner = context.Web.UserCustomActions.Add();
                banner.Name = actionName;
                banner.Title = actionTitle;
                banner.Location = "ScriptLink";
                banner.Sequence = 50;
                banner.ScriptBlock = scriptBlock;

                banner.Update();
                context.ExecuteQuery();

                _logService.LogInfo($"Banner applied successfully", $"Site: {siteName}, Action Name: {actionName}", callingMethod);
            }
            catch (Exception ex)
            {
                _logService.LogError($"Failed to apply banner to site", $"Site: {siteUrl ?? context.Url}, Error: {ex.Message}", callingMethod);
                throw;
            }
        }

        private List<string> GetAllSubsites(ClientContext context)
        {
            var subsites = new List<string>();
            
            try
            {
                context.Load(context.Web, w => w.Webs);
                context.ExecuteQuery();

                foreach (var web in context.Web.Webs)
                {
                    context.Load(web, w => w.Url, w => w.Webs);
                    context.ExecuteQuery();
                    
                    subsites.Add(web.Url);

                    // Recursively get subsites of subsites
                    using (var subsiteContext = new ClientContext(web.Url))
                    {
                        subsites.AddRange(GetAllSubsites(subsiteContext));
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.LogWarning($"Failed to retrieve subsites", $"Error: {ex.Message}", "GetAllSubsites");
            }

            return subsites;
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

        public void RemoveAllOptions(string siteUrl, bool removeFromSubsites = false)
        {
            const string methodName = nameof(RemoveAllOptions);
            
            try
            {
                _logService.LogInfo($"Starting removal of all banner options", $"Site: {siteUrl}, Remove from subsites: {removeFromSubsites}", methodName);

                using (var context = new ClientContext(siteUrl))
                {
                    // Remove from main site
                    RemoveAllExistingBanners(context, methodName);

                    // Remove from subsites if requested
                    if (removeFromSubsites)
                    {
                        var subsites = GetAllSubsites(context);
                        _logService.LogInfo($"Found {subsites.Count} subsites to process for removal", null, methodName);

                        foreach (var subsite in subsites)
                        {
                            try
                            {
                                using (var subsiteContext = new ClientContext(subsite))
                                {
                                    _logService.LogInfo($"Removing banners from subsite", $"Subsite: {subsite}", methodName);
                                    RemoveAllExistingBanners(subsiteContext, methodName);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logService.LogWarning($"Failed to remove banners from subsite", $"Subsite: {subsite}, Error: {ex.Message}", methodName);
                            }
                        }
                    }

                    _logService.LogSuccess($"Successfully removed all banner actions", $"Site: {siteUrl}, Subsites processed: {removeFromSubsites}", methodName);
                }
            }
            catch (Exception ex)
            {
                _logService.LogError($"Failed to remove banner options", $"Site: {siteUrl}, Error: {ex.Message}", methodName);
                throw;
            }
        }
    }
}