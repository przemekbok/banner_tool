# Summary: 15-Minute Banner Reset Implementation

## Branch: `banner-15min-reset`

This branch implements a new timing mechanism for banner suppression that provides better balance between user control and persistent messaging for critical migration notices.

## Key Changes

### 1. **Timing Modification**
- **Previous**: 24-hour suppression after user declines redirect
- **New**: 15-minute suppression with automatic reset

### 2. **Automatic State Reset**
```javascript
// When 15 minutes pass, state automatically resets
if (minutesSinceDeclined >= 15) {
    localStorage.removeItem(lastDeclinedKey);
    console.log('15 minutes elapsed since last decline - resetting banner state');
    stateReset = true;
}
```

### 3. **Enhanced User Interface**
- Button text updated to "Stay on Current Site (15 min)" 
- Help text indicates 15-minute suppression period
- Clear logging of remaining time

### 4. **Improved User Experience**
- Shorter suppression period reduces risk of missing important updates
- Banner remains visible during suppression period
- Automatic re-engagement after reasonable cooldown

## Behavior Matrix

| User Action | Modal State | Banner State | Next Occurrence |
|-------------|-------------|--------------|-----------------|
| Go to New Site | Removed | Removed | Normal (new page) |
| Stay on Current Site | Removed | **Remains Visible** | 15 minutes later |
| Escape Key | Removed | **Remains Visible** | 15 minutes later |
| Check "Don't show again" | Removed | **Remains Visible** | Never (permanent) |
| 15 minutes pass | N/A | N/A | Banner + Modal shown |

## Benefits

### For Users
- **Reasonable cooldown**: 15 minutes provides breathing room without being excessive
- **Predictable behavior**: Clear indication of when banner will return
- **Persistent visibility**: Important information remains accessible via banner

### For Organizations
- **Better compliance**: Shorter suppression ensures critical information reaches users
- **Reduced support burden**: Automatic reset eliminates need for manual intervention
- **Effective communication**: Balances user autonomy with organizational needs

## Testing

### Quick Test Procedure
1. Apply "Default Banner and Redirect" to a SharePoint site
2. Navigate to site and wait for modal
3. Click "Stay on Current Site (15 min)"
4. Verify modal disappears but banner remains
5. Wait 15+ minutes
6. Refresh page - banner and modal should appear again

### Validation in Browser Console
```javascript
// Check current suppression status
var siteKey = 'migrationRedirect_' + window.location.hostname + window.location.pathname.replace(/\//g, '_');
var lastDeclined = localStorage.getItem(siteKey + '_lastDeclined');

if (lastDeclined) {
    var minutesSince = (Date.now() - parseInt(lastDeclined)) / (1000 * 60);
    console.log('Suppressed for:', minutesSince.toFixed(1), 'minutes');
    console.log('Will reset in:', Math.ceil(15 - minutesSince), 'minutes');
} else {
    console.log('No active suppression');
}
```

## Configuration

### Current Default
- **Suppression Duration**: 15 minutes
- **Reset Method**: Automatic (removes localStorage entry)
- **Banner Persistence**: Yes (stays visible during suppression)

### Customization
To modify the timing, edit this line in `MainViewModel.cs`:
```javascript
if (minutesSinceDeclined >= 15) {  // Change 15 to desired minutes
```

## Deployment Notes

- **Backward Compatible**: Works with existing localStorage entries
- **No Migration Needed**: Existing suppressions get new 15-minute behavior
- **Safe Rollback**: Can revert to previous branch without data loss

## File Changes

- `UMT.UI/ViewModel/MainViewModel.cs` - Main implementation
- `BANNER_15MIN_RESET_FEATURE.md` - Comprehensive documentation

## Support Commands

```javascript
// Clear all banner preferences (admin/debug)
var siteKey = 'migrationRedirect_' + window.location.hostname + window.location.pathname.replace(/\//g, '_');
localStorage.removeItem(siteKey + '_disabled');
localStorage.removeItem(siteKey + '_lastDeclined');
console.log('All banner preferences cleared for this site');

// Force banner to show immediately (admin/debug)
window['migrationBannerExecuted_' + window.location.href] = false;
location.reload();
```

This implementation provides an optimal balance between user experience and organizational communication requirements, ensuring critical migration information remains accessible while respecting user preferences for a reasonable cooldown period.
