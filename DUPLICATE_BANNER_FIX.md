# Fix for Duplicate Banner Display Issue

## Problem Description

The SharePoint Banner Manager was experiencing an issue where banners would be displayed multiple times when users navigated through SharePoint, particularly when clicking the cog icon and selecting "Site Contents" from the dropdown menu. 

## Root Cause Analysis

After investigation, the main issue was identified:

### **PRIMARY ISSUE: Multiple Custom Actions Created**

In the `DefaultBannerRedirect` mode, the application was creating **TWO separate SharePoint custom actions**:

```csharp
// This created FIRST custom action
_bannerService.CreateAutoRedirectNotification(SiteUrl, null, null, BannerMessage);

// This created SECOND custom action  
_bannerService.CreateCustomBanner(SiteUrl, GenerateCustomJsBanner(...));
```

**Result:** Two custom actions = Two independent script executions = Duplicate banners on every page load.

### Secondary Issues
1. **Inadequate Custom Action Cleanup**: Removal wasn't thorough enough to catch all banner types
2. **SharePoint Navigation**: SPA-like navigation in SharePoint caused additional script executions
3. **Insufficient Detection Logic**: Wasn't detecting all possible banner custom actions

## Solution Implementation

### 1. **Fixed Multiple Custom Action Creation**

**Before (BROKEN):**
```csharp
case AppMode.DefaultBannerRedirect:
    // Created 2 separate custom actions!
    _bannerService.CreateAutoRedirectNotification(SiteUrl, null, null, BannerMessage);
    _bannerService.CreateCustomBanner(SiteUrl, GenerateCustomJsBanner(...));
    break;
```

**After (FIXED):**
```csharp
case AppMode.DefaultBannerRedirect:
    // Creates ONLY 1 custom action that handles both banner and redirect
    string combinedJs = GenerateCombinedBannerAndRedirectJs(BannerMessage, RedirectionUrl, CountdownSeconds.ToString(), PopupMessage);
    _bannerService.CreateCustomBanner(SiteUrl, combinedJs);
    break;
```

### 2. **Enhanced JavaScript with Robust Duplicate Prevention**

#### Multi-Layer Execution Prevention
```javascript
// Global execution flag using unique page URL
var globalKey = 'migrationBannerExecuted_' + window.location.href;
if (window[globalKey]) {
    console.log('Migration banner already executed for this page, skipping');
    return;
}
window[globalKey] = true;

// DOM-based duplicate detection
var existingBanner = document.querySelector('[data-migration-banner="true"]');
var existingModal = document.querySelector('[data-migration-modal="true"]');
if (existingBanner || existingModal) {
    console.log('Migration elements already present, skipping');
    return;
}
```

#### Unique Element Marking
```javascript
// All banner elements marked with unique attributes
banner.setAttribute('data-migration-banner', 'true');
modal.setAttribute('data-migration-modal', 'true');
banner.setAttribute('data-execution-key', executionKey);
```

#### Combined Banner and Modal Logic
- Single JavaScript creates both the top banner AND the redirect modal
- Proper timing (1 second delay before modal)
- Unified cleanup system
- SharePoint navigation detection

### 3. **Enhanced Server-Side Cleanup**

#### Comprehensive Custom Action Detection
```csharp
private bool IsBannerAction(UserCustomAction action)
{
    // Check by known names
    var knownBannerNames = new[] { "DefaultBanner", "MigrationBanner", "CustomBanner" };
    
    // Check by sequence numbers
    if (action.Sequence == 50 || action.Sequence == 100) return true;
    
    // Check script content for migration keywords
    if (action.ScriptBlock?.ToLower().Contains("migration") == true) return true;
    
    // Check for specific styling (orange banner color)
    if (action.ScriptBlock?.Contains("#ff6b35") == true) return true;
}
```

#### Multi-Level Action Removal
```csharp
// Check BOTH web and site level custom actions
context.Load(context.Web, w => w.UserCustomActions);
context.Load(context.Site, s => s.UserCustomActions);

// Remove from both levels
var webActions = context.Web.UserCustomActions.Where(a => IsBannerAction(a));
var siteActions = context.Site.UserCustomActions.Where(a => IsBannerAction(a));
```

## Key Improvements

### ✅ **Single Custom Action Creation**
- **Before**: DefaultBannerRedirect created 2 custom actions
- **After**: All modes create exactly 1 custom action
- **Result**: Eliminates the primary cause of duplicates

### ✅ **Proactive Cleanup**
- **Before**: Limited removal based on name/sequence only
- **After**: Multi-criteria detection (name, sequence, content, styling)
- **Result**: Catches and removes ALL banner-related custom actions

### ✅ **Robust JavaScript Execution Control**
- **Before**: Basic localStorage checks
- **After**: Multi-layer prevention (global flags, DOM checks, execution keys)
- **Result**: Prevents any possibility of duplicate execution

### ✅ **Site and Web Level Coverage**
- **Before**: Only checked web-level custom actions
- **After**: Checks both site and web level custom actions
- **Result**: Complete cleanup regardless of where actions were created

### ✅ **Enhanced Logging and Debugging**
- **Before**: Basic success/error messages
- **After**: Detailed execution tracking and comprehensive troubleshooting tools
- **Result**: Easy diagnosis of any remaining issues

## Testing Scenarios Verified

### ✅ **Primary Issue Fixed**
- **Cog Menu → Site Contents**: No more duplicate banners
- **Multiple Page Loads**: Only one banner appears total
- **Rapid Navigation**: No conflicts or multiple modals

### ✅ **Edge Cases Handled**
- **Browser Refresh**: Clean state, proper banner display
- **SharePoint Navigation**: Automatic cleanup during navigation
- **Multiple Tabs**: Each tab maintains independent state
- **User Preferences**: All localStorage preferences preserved

### ✅ **All Banner Modes Working**
- **Default Banner**: Single banner, no duplicates
- **Banner with Redirect**: Single banner + single modal
- **Custom Banner**: Single custom script execution
- **Remove All**: Complete cleanup of all banner types

## Files Modified

1. **`UMT.UI/ViewModel/MainViewModel.cs`**
   - Fixed DefaultBannerRedirect to create single custom action
   - Enhanced JavaScript with comprehensive duplicate prevention
   - Added execution tracking and cleanup mechanisms

2. **`UMT.Services/Banner/BannerService.cs`**
   - Enhanced custom action detection logic
   - Added multi-level cleanup (site + web)
   - Improved content-based banner detection

3. **`TROUBLESHOOTING_DUPLICATES.md`**
   - Comprehensive diagnostic tools
   - PowerShell scripts for validation
   - Step-by-step problem resolution

## Validation

To verify the fix is working:

### Quick Check
```javascript
// Run in browser console on SharePoint site
console.log('Banners:', document.querySelectorAll('[data-migration-banner="true"]').length);
console.log('Modals:', document.querySelectorAll('[data-migration-modal="true"]').length);
// Should show: Banners: 1, Modals: 0 (or 1 if modal is displayed)
```

### PowerShell Check
```powershell
# Check custom actions count
$actions = Get-PnPCustomAction -Scope Web | Where-Object {$_.Location -eq "ScriptLink"}
Write-Host "ScriptLink actions found: $($actions.Count)"
# Should show: 1 (or 0 if no banners applied)
```

## Deployment Instructions

1. **Deploy the updated code**
2. **For existing sites with duplicate issues:**
   - Use "Remove All Banners" first
   - Wait 5 minutes for SharePoint cache
   - Apply new banner
3. **Verify using validation scripts above**

## Emergency Recovery

If issues persist, use the PowerShell cleanup from `TROUBLESHOOTING_DUPLICATES.md`:

```powershell
# Nuclear option - removes ALL ScriptLink custom actions
Get-PnPCustomAction -Scope Web | Where-Object {$_.Location -eq "ScriptLink"} | Remove-PnPCustomAction -Force
Get-PnPCustomAction -Scope Site | Where-Object {$_.Location -eq "ScriptLink"} | Remove-PnPCustomAction -Force
```

This fix addresses the root cause of duplicate banners and provides comprehensive prevention mechanisms and diagnostic tools for any edge cases.
