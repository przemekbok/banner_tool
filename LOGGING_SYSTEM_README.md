# SharePoint Banner Manager - Logging System

## Overview
This branch adds a comprehensive logging system to the SharePoint Banner Manager application. The logging system provides detailed tracking of all application activities, errors, and user actions with both in-memory and file-based persistence.

## New Features

### 1. Comprehensive Logging Service
- **ILogService Interface**: Defines logging contracts with support for multiple log levels
- **LogService Implementation**: Thread-safe logging with file persistence and memory management
- **Log Levels**: Info, Warning, Error, Success
- **Structured Logging**: Each log entry includes timestamp, level, message, details, and source

### 2. Logs Viewer Window
- **Real-time Log Display**: Live updates as new logs are generated
- **Advanced Filtering**: Filter by log level and search text
- **Export Functionality**: Export logs to text files
- **Professional UI**: Clean, modern interface with color-coded log levels

### 3. Enhanced User Experience
- **Status Messages**: Comprehensive success/error messages for all operations
- **Input Validation**: Proper validation with user-friendly error messages
- **Logs Button**: Easy access to logs from the main window header
- **Real-time Feedback**: All operations now provide immediate feedback to users

### 4. File-based Persistence
- **Daily Log Files**: Automatic daily rotation with timestamped filenames
- **Secure Storage**: Logs saved in user's AppData folder
- **Memory Management**: Intelligent memory limits to prevent performance issues

## Technical Implementation

### Architecture Changes

#### New Services
```csharp
ILogService - Logging interface
LogService - Thread-safe logging implementation with file persistence
```

#### New UI Components
```csharp
LogsViewModel - Manages log display and operations
LogsWindow - WPF window for viewing and managing logs
RelayCommand - Additional command implementation for synchronous operations
```

#### Updated Components
```csharp
MainViewModel - Enhanced with logging and comprehensive status messages
BannerService - All operations now include detailed logging
UnityConfig - Registers new LogService as singleton
```

### Logging Features

#### Log Levels
- **Info**: General application activities
- **Warning**: Non-critical issues
- **Error**: Failures and exceptions
- **Success**: Successful operations

#### Log Structure
```csharp
LogEntry {
    DateTime Timestamp
    LogLevel Level
    string Message
    string Details (optional)
    string Source (optional)
}
```

#### File Storage
- Location: `%AppData%\UltimateMigrationTool\logs_YYYYMMDD.txt`
- Format: `[YYYY-MM-DD HH:mm:ss] LEVEL [Source]: Message - Details`
- Rotation: Daily files with date in filename

### UI Enhancements

#### Main Window
- Added "View Logs" button in header with icon
- Improved input validation with detailed error messages
- Success/error notifications for all operations
- Better field descriptions and help text

#### Logs Window
- Real-time log streaming
- Filter by log level dropdown
- Search functionality across all log fields
- Export to file with timestamp
- Clear all logs functionality
- Color-coded log entries by level
- Professional layout with proper sizing

## Usage

### Accessing Logs
1. Click the "📋 View Logs" button in the main window header
2. The logs window will open showing all application activities

### Filtering Logs
1. Use the "Filter by Level" dropdown to show only specific log levels
2. Use the search box to find logs containing specific text
3. Click "Refresh" to reload logs from file

### Exporting Logs
1. Click the "Export" button in the logs window
2. Choose a location and filename
3. All current logs will be saved to the selected file

### Log File Location
Logs are automatically saved to:
```
C:\Users\[Username]\AppData\Roaming\UltimateMigrationTool\logs_[date].txt
```

## Benefits

### For Developers
- **Debugging**: Detailed error information with stack traces
- **Monitoring**: Track application behavior and performance
- **Audit Trail**: Complete record of all user actions

### For Users
- **Transparency**: Clear feedback on all operations
- **Troubleshooting**: Easy access to error details
- **Support**: Exportable logs for technical support

### For Operations
- **Issue Resolution**: Detailed logs help quickly identify problems
- **Usage Analytics**: Track application usage patterns
- **Quality Assurance**: Verify all operations complete successfully

## Configuration

### Memory Management
- Maximum 1000 log entries in memory
- Automatic cleanup of old entries
- Efficient memory usage with concurrent collections

### File Management
- Daily log rotation
- Automatic directory creation
- Graceful handling of file access issues

### UI Settings
- Maximum 500 entries displayed in UI for performance
- Real-time updates via event subscription
- Thread-safe UI updates

## Error Handling

### Logging Failures
- Silent failure for logging operations to prevent application crashes
- Fallback to Debug.WriteLine for critical logging issues
- Robust file access with proper exception handling

### UI Error Handling
- Comprehensive try-catch blocks around all operations
- User-friendly error messages
- Detailed logging of all exceptions

## Future Enhancements

Potential improvements for future versions:
- Log rotation based on file size
- Remote logging capabilities
- Advanced filtering options (date ranges, custom queries)
- Log analysis and reporting features
- Integration with external monitoring systems

## Testing

The logging system has been designed with the following test scenarios in mind:
- High-volume logging scenarios
- Concurrent access from multiple threads
- File system permission issues
- Memory constraints
- UI responsiveness during heavy logging

## Compatibility

This logging system is compatible with:
- .NET Framework 4.7.2
- Windows 7 and later
- All existing SharePoint Banner Manager functionality
- Existing configuration and user settings
