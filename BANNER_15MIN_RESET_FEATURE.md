# Banner 15-Minute Reset Mechanism

## Overview

This branch implements a new timing mechanism for banner suppression that automatically resets after 15 minutes instead of the previous 24-hour suppression period. This provides a more balanced user experience between persistent notification and user control.

## Problem Statement

Previously, when users declined the migration redirect:
- Banner was suppressed for 24 hours
- This was too long for important migration notices
- Users might miss critical updates during this extended period

## Solution

### New 15-Minute Reset Logic

The banner now implements an intelligent suppression system:

1. **User declines redirect** → Banner popup suppressed for 15 minutes
2. **After 15 minutes pass** → State automatically resets
3. **Next page load** → Banner and redirect popup appear again
4. **Banner persistence** → Top banner remains visible during suppression period

## Technical Implementation

### Key Changes

#### 1. Modified Time Check
```javascript
// OLD: 24-hour suppression
var hoursSinceDeclined = (Date.now() - declinedTime) / (1000 * 60 * 60);
hasRecentlyDeclined = hoursSinceDeclined < 24;

// NEW: 15-minute suppression with auto-reset
var minutesSinceDeclined = (Date.now() - declinedTime) / (1000 * 60);

if (minutesSinceDeclined >= 15) {
    // 15 minutes have passed - reset the state
    localStorage.removeItem(lastDeclinedKey);
    console.log('15 minutes elapsed since last decline - resetting banner state');
    stateReset = true;
} else {
    hasRecentlyDeclined = true;
    var remainingMinutes = Math.ceil(15 - minutesSinceDeclined);
    console.log('Banner suppressed - user declined recently. Will reset in ' + remainingMinutes + ' minutes');
}
```

#### 2. Automatic State Reset
- When 15 minutes pass, the `lastDeclinedKey` is automatically removed from localStorage
- This ensures the banner will show again on the next page load
- No manual intervention required

#### 3. Enhanced User Feedback
- Button text updated to "Stay on Current Site (15 min)" to indicate duration
- Console logging shows remaining minutes until reset
- Help text updated to reflect new timing

#### 4. Improved State Management
```javascript
if (stateReset) {
    console.log('Banner state reset - showing banner again after 15-minute cooldown');
}
```

## User Experience Flow

### Scenario 1: First Visit
1. User visits SharePoint site
2. Banner appears immediately
3. After 1 second, redirect modal appears
4. User can choose to redirect or stay

### Scenario 2: User Declines Redirect
1. User clicks "Stay on Current Site (15 min)"
2. Modal disappears, banner remains visible
3. Redirect popup suppressed for 15 minutes
4. Banner stays visible throughout suppression period

### Scenario 3: After 15 Minutes
1. 15 minutes pass since user declined
2. State automatically resets
3. Next page load shows both banner and modal again
4. User gets fresh opportunity to redirect

### Scenario 4: Permanent Disable (Still Available)
1. User can still check "Don't show this message again"
2. This creates permanent suppression (until manually cleared)
3. Overrides the 15-minute reset mechanism

## Benefits

### 1. **Balanced Persistence**
- 15 minutes provides breathing room for users
- Not too short (annoying) or too long (ineffective)
- Ensures important migration notices remain visible

### 2. **Automatic Recovery**
- No need for admins to manually reset user preferences
- System automatically re-engages users after reasonable time
- Reduces support burden

### 3. **Better Compliance**
- Ensures critical migration information reaches users
- Reduces risk of users missing important deadlines
- Maintains visibility of persistent banner

### 4. **User-Friendly**
- Clear indication of suppression duration (15 min)
- Predictable behavior
- Banner remains visible during suppression

## Configuration

### Current Settings
- **Suppression Duration**: 15 minutes
- **Banner Persistence**: Yes (banner stays visible)
- **Reset Method**: Automatic (on timer expiration)

### Customization Options
To modify the timing, change this line in `GenerateCombinedBannerAndRedirectJs()`:
```javascript
if (minutesSinceDeclined >= 15) {  // Change 15 to desired minutes
```

## Testing

### Test Cases

1. **Basic Functionality**
   - Apply banner with redirect
   - Decline redirect
   - Verify 15-minute suppression
   - Verify automatic reset after 15 minutes

2. **Edge Cases**
   - Multiple page loads during suppression
   - Browser refresh during countdown
   - Multiple tabs/windows

3. **State Management**
   - Verify localStorage cleanup after reset
   - Check console logging accuracy
   - Confirm banner persistence during suppression

### Validation Script
```javascript
// Run in browser console to check current state
var siteKey = 'migrationRedirect_' + window.location.hostname + window.location.pathname.replace(/\//g, '_');
var lastDeclined = localStorage.getItem(siteKey + '_lastDeclined');

if (lastDeclined) {
    var minutesSince = (Date.now() - parseInt(lastDeclined)) / (1000 * 60);
    console.log('Minutes since last decline:', minutesSince.toFixed(1));
    console.log('Will reset in:', Math.ceil(15 - minutesSince), 'minutes');
} else {
    console.log('No recent decline recorded');
}
```

## Deployment Notes

### Backward Compatibility
- Existing localStorage entries from 24-hour system will work correctly
- Users with existing suppressions will get new 15-minute behavior
- No migration needed

### Monitoring
The system provides detailed console logging:
- Banner execution status
- Suppression reasons
- Reset notifications
- Remaining time indicators

## Future Enhancements

### Potential Improvements
1. **Configurable Timing**: Admin setting for suppression duration
2. **Progressive Timing**: Increasing suppression (15 min → 1 hour → 4 hours)
3. **Usage Analytics**: Track suppression/acceptance rates
4. **Time-based Urgency**: Shorter suppression as deadline approaches

### Admin Controls
Consider adding admin options for:
- Default suppression duration
- Maximum suppressions per day
- Emergency override (force show regardless of user preference)

## Related Files

- `UMT.UI/ViewModel/MainViewModel.cs` - Main implementation
- `BANNER_PERSISTENCE_FEATURE.md` - Previous persistence enhancement
- `DUPLICATE_BANNER_FIX.md` - Duplicate prevention system

## Support

### Common Issues
1. **Banner not resetting**: Check browser console for localStorage entries
2. **Timing seems off**: Browser clock issues or timezone differences
3. **Permanent suppression**: User checked "Don't show again" - clear localStorage

### Debug Commands
```javascript
// Clear all banner preferences for current site
var siteKey = 'migrationRedirect_' + window.location.hostname + window.location.pathname.replace(/\//g, '_');
localStorage.removeItem(siteKey + '_disabled');
localStorage.removeItem(siteKey + '_lastDeclined');
console.log('Banner preferences cleared');
```

This enhancement maintains the balance between user autonomy and organizational communication needs, ensuring important migration information remains accessible while respecting user preferences.
