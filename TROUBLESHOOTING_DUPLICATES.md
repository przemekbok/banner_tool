# Troubleshooting Duplicate Banner Issues

## Quick Diagnosis

If you're still seeing duplicate banners, follow these steps to diagnose the issue:

### 1. Check Browser Developer Console

Open your browser's Developer Tools (F12) and look for these console messages:

**✅ Good Messages (banner working correctly):**
```
Migration banner already executed for this page, skipping
Migration banner setup complete
Migration modal displayed
```

**❌ Problem Messages (indicating duplicates):**
```
Multiple console messages with same execution key
Multiple "Migration banner setup complete" messages
Errors about elements already existing
```

### 2. Check SharePoint Custom Actions

Use PowerShell to check what custom actions exist on your site:

```powershell
# Connect to your SharePoint site
Connect-PnPOnline -Url "YOUR_SITE_URL" -Interactive

# List all custom actions
Get-PnPCustomAction -Scope Web | Format-Table Name, Title, Location, Sequence
Get-PnPCustomAction -Scope Site | Format-Table Name, Title, Location, Sequence

# Look for actions with:
# - Location: ScriptLink  
# - Sequence: 50 or 100
# - Names containing: Migration, Banner, DefaultBanner, etc.
```

### 3. Check DOM Elements

In the browser console, run this to check for duplicate elements:

```javascript
// Check for duplicate banners
var banners = document.querySelectorAll('[data-migration-banner="true"]');
console.log('Banner elements found:', banners.length);

// Check for duplicate modals  
var modals = document.querySelectorAll('[data-migration-modal="true"]');
console.log('Modal elements found:', modals.length);

// List all custom actions in page (if any are embedded)
var scripts = document.querySelectorAll('script');
var migrationScripts = Array.from(scripts).filter(s => 
    s.textContent.includes('migration') || 
    s.textContent.includes('data-migration-banner')
);
console.log('Migration scripts found:', migrationScripts.length);
```

## Common Issues and Solutions

### Issue 1: Multiple Custom Actions Exist

**Symptoms:** Multiple banners appear immediately on page load

**Diagnosis:** Check PowerShell output for multiple ScriptLink actions

**Solution:** Remove all existing banner actions manually:

```powershell
# Remove all banner-related custom actions
$actions = Get-PnPCustomAction -Scope Web | Where-Object {
    $_.Location -eq "ScriptLink" -and (
        $_.Name -like "*Banner*" -or 
        $_.Name -like "*Migration*" -or
        $_.Sequence -eq 50 -or 
        $_.Sequence -eq 100
    )
}

foreach($action in $actions) {
    Write-Host "Removing: $($action.Name) - $($action.Title)"
    Remove-PnPCustomAction -Identity $action.Id -Scope Web -Force
}

# Also check site-level actions
$siteActions = Get-PnPCustomAction -Scope Site | Where-Object {
    $_.Location -eq "ScriptLink" -and (
        $_.Name -like "*Banner*" -or 
        $_.Name -like "*Migration*" -or
        $_.Sequence -eq 50 -or 
        $_.Sequence -eq 100
    )
}

foreach($action in $siteActions) {
    Write-Host "Removing site action: $($action.Name) - $($action.Title)"
    Remove-PnPCustomAction -Identity $action.Id -Scope Site -Force
}
```

### Issue 2: JavaScript Execution Multiple Times

**Symptoms:** Console shows multiple "Migration banner setup complete" messages

**Diagnosis:** SharePoint is executing the same script multiple times during navigation

**Solution:** The updated code includes execution prevention, but you can also add this debug code:

```javascript
// Add to browser console to see execution tracking
window.migrationDebug = true;
localStorage.setItem('migrationDebug', 'true');

// This will show detailed logging in the updated script
```

### Issue 3: Old Custom Actions Not Removed

**Symptoms:** Banner tool says "applied successfully" but duplicates still appear

**Diagnosis:** Old custom actions exist that aren't being detected by the removal logic

**Solution:** Use the nuclear option to remove ALL ScriptLink actions:

```powershell
# ⚠️ WARNING: This removes ALL custom script actions
# Only use if you're sure no other custom scripts are needed

Get-PnPCustomAction -Scope Web | Where-Object {$_.Location -eq "ScriptLink"} | ForEach-Object {
    Write-Host "Removing: $($_.Name) - $($_.Title)"
    Remove-PnPCustomAction -Identity $_.Id -Scope Web -Force
}

Get-PnPCustomAction -Scope Site | Where-Object {$_.Location -eq "ScriptLink"} | ForEach-Object {
    Write-Host "Removing site action: $($_.Name) - $($_.Title)"
    Remove-PnPCustomAction -Identity $_.Id -Scope Site -Force
}
```

### Issue 4: Browser Cache

**Symptoms:** Changes don't take effect immediately

**Solution:**
1. Clear browser cache completely
2. Use incognito/private browsing mode
3. Hard refresh (Ctrl+Shift+R)

### Issue 5: SharePoint Cache

**Symptoms:** Old banners keep appearing even after removal

**Solution:**
1. Wait 15-30 minutes for SharePoint cache to clear
2. Or use PowerShell to clear cache:

```powershell
# Clear SharePoint cache (if you have admin access)
Clear-PnPTenantRecycleBinItem -Force
```

## Advanced Debugging

### Enable Verbose Logging

Add this to your banner JavaScript for detailed debugging:

```javascript
// Add at the beginning of any custom banner script
window.migrationDebugLevel = 'verbose';

// This will enable detailed console logging
console.log('Migration script starting with verbose logging');
```

### Monitor Network Traffic

1. Open Developer Tools → Network tab
2. Filter by "XHR" or "Fetch"  
3. Look for SharePoint API calls that might be loading custom actions
4. Check if multiple identical requests are being made

### Check SharePoint Logs

If you have access to SharePoint admin center:

1. Go to SharePoint Admin Center
2. Check "Health" section for any errors
3. Look for custom action related errors

## Prevention

To prevent future duplicate issues:

### 1. Always Remove Before Adding
Make sure your tool always calls "Remove All Banners" before applying new ones.

### 2. Test in Stages
1. First remove all banners
2. Wait 5 minutes
3. Then apply new banner
4. Test thoroughly before moving to production

### 3. Use Unique Identifiers
The updated code uses unique execution keys and DOM attributes to prevent conflicts.

### 4. Monitor Regularly
Set up regular checks to ensure only one banner action exists:

```powershell
# Save this as a scheduled script
$bannerCount = (Get-PnPCustomAction -Scope Web | Where-Object {$_.Location -eq "ScriptLink"}).Count
if($bannerCount -gt 1) {
    Write-Warning "Multiple banner actions detected: $bannerCount"
    # Send alert or take corrective action
}
```

## Emergency Recovery

If banners completely break the site:

### Option 1: PowerShell Emergency Cleanup
```powershell
# Remove ALL custom actions (nuclear option)
Get-PnPCustomAction -Scope Web | Remove-PnPCustomAction -Force
Get-PnPCustomAction -Scope Site | Remove-PnPCustomAction -Force
```

### Option 2: SharePoint Designer
1. Open site in SharePoint Designer
2. Go to "All Files" → "_catalogs" → "masterpage"
3. Check for any custom master pages with embedded scripts

### Option 3: Contact SharePoint Admin
If you don't have the necessary permissions, contact your SharePoint administrator with this troubleshooting information.

## Validation Script

Use this PowerShell script to validate your site after applying fixes:

```powershell
# Validation script
Connect-PnPOnline -Url "YOUR_SITE_URL" -Interactive

Write-Host "=== Banner Action Validation ===" -ForegroundColor Yellow

# Check web-level custom actions
$webActions = Get-PnPCustomAction -Scope Web
$bannerActions = $webActions | Where-Object {$_.Location -eq "ScriptLink"}

Write-Host "Web-level ScriptLink actions: $($bannerActions.Count)" -ForegroundColor Cyan
foreach($action in $bannerActions) {
    Write-Host "  - Name: $($action.Name), Title: $($action.Title), Sequence: $($action.Sequence)" -ForegroundColor White
}

# Check site-level custom actions  
$siteActions = Get-PnPCustomAction -Scope Site
$siteBannerActions = $siteActions | Where-Object {$_.Location -eq "ScriptLink"}

Write-Host "Site-level ScriptLink actions: $($siteBannerActions.Count)" -ForegroundColor Cyan
foreach($action in $siteBannerActions) {
    Write-Host "  - Name: $($action.Name), Title: $($action.Title), Sequence: $($action.Sequence)" -ForegroundColor White
}

# Summary
$totalBannerActions = $bannerActions.Count + $siteBannerActions.Count
if($totalBannerActions -eq 0) {
    Write-Host "✅ No banner actions found - site is clean" -ForegroundColor Green
} elseif($totalBannerActions -eq 1) {
    Write-Host "✅ Exactly 1 banner action found - this is correct" -ForegroundColor Green
} else {
    Write-Host "❌ Multiple banner actions found ($totalBannerActions) - duplicates may occur" -ForegroundColor Red
}
```

This troubleshooting guide should help you identify and resolve any remaining duplicate banner issues.
