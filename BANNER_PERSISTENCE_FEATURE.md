# Banner Persistence Feature - Keep Banner Displayed When Redirect is Declined

## Overview

This branch implements a feature enhancement for the "Default Banner and Redirect" functionality. The banner now remains visible when a user declines the redirect, ensuring continuous visibility of important notices.

## Problem Statement

Previously, when using the "Default Banner and Redirect" option:
1. A banner would display at the top of the page
2. After 1 second, a modal would appear offering redirect options
3. If the user clicked "Stay on Current Site", **both the banner and modal were removed**
4. This meant the important notice was no longer visible to the user

## Solution

The JavaScript has been modified to implement separate cleanup behaviors:

### New Behavior
- **When redirect is accepted**: Both banner and modal are removed (user is redirected)
- **When redirect is declined**: Only the modal is removed, **banner remains visible**
- **On page navigation**: Both banner and modal are removed (normal cleanup)

## Technical Changes

### 1. Separate Cleanup Functions

**Before (Single cleanup function):**
```javascript
function cleanupAll() {
    // Removed both modal and banner in all cases
    clearInterval(timer);
    removeModal();
    removeBanner();
    resetGlobalFlag();
}
```

**After (Two separate functions):**
```javascript
// Only removes the modal (used when user declines redirect)
function cleanupModal() {
    clearInterval(timer);
    removeModal();
    // Banner remains visible
}

// Removes everything (used for redirect or navigation)
function cleanupAll() {
    clearInterval(timer);
    removeModal();
    removeBanner();
    resetGlobalFlag();
}
```

### 2. Updated Button Handlers

**"Stay on Current Site" Button:**
- Now calls `cleanupModal()` instead of `cleanupAll()`
- Banner remains visible for continued user awareness

**"Go to New Site Now" Button:**
- Still calls `cleanupAll()` before redirect
- Complete cleanup as user is leaving the site

**Automatic Redirect (countdown expires):**
- Still calls `cleanupAll()` before redirect
- Complete cleanup as user is leaving the site

### 3. Enhanced User Experience

**Escape Key:**
- Now only closes the modal, keeps banner visible
- Previously removed everything

**User Preference Note:**
- Updated text to clarify: "This preference is saved for this specific site only. The banner will remain visible."
- Users understand that declining only affects the redirect modal, not the banner

## Use Cases

### Scenario 1: Maintenance Notice
- Banner: "Scheduled maintenance tonight 8-10 PM"
- User declines redirect to maintenance page
- **Result**: Banner stays visible as ongoing reminder

### Scenario 2: Site Migration
- Banner: "This site is moving to a new location"
- User declines immediate redirect
- **Result**: Banner continues to inform about migration

### Scenario 3: Important Announcement
- Banner: "New security policy in effect"
- User declines redirect to policy page
- **Result**: Banner remains as persistent notice

## Benefits

1. **Continuous Visibility**: Important information remains accessible
2. **User Choice**: Users can decline redirect while staying informed
3. **Compliance**: Ensures persistent display of critical notices
4. **Better UX**: Users aren't left without important context

## Testing

To test the new behavior:

1. Apply "Default Banner and Redirect" to a SharePoint site
2. Navigate to the site - banner should appear
3. Wait for modal to appear (1 second delay)
4. Click "Stay on Current Site"
5. **Verify**: Modal disappears but banner remains visible
6. Navigate to another page and back
7. **Verify**: Banner reappears (normal behavior)

### Expected Behavior

| Action | Modal State | Banner State | 
|--------|------------|--------------|
| Page loads | Appears after 1s | Visible |
| "Go to New Site" clicked | Removed | Removed |
| "Stay on Current Site" clicked | Removed | **Remains Visible** |
| Countdown expires | Removed | Removed |
| Escape key pressed | Removed | **Remains Visible** |
| Page navigation | N/A | Removed |

## Files Modified

- `UMT.UI/ViewModel/MainViewModel.cs`: Updated `GenerateCombinedBannerAndRedirectJs()` method

## Deployment

This change is backward compatible. No database changes or configuration updates required.

## Future Enhancements

Potential future improvements:
- Banner dismiss button (separate from redirect decline)
- Banner persistence across browser sessions
- Different banner styles for different notice types
- Admin setting to control banner persistence behavior
