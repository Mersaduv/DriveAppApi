namespace DriveApp.Enums;

public enum PermissionType
{
    // User Management
    ViewUsers = 1,
    CreateUser = 2,
    EditUser = 3,
    DeleteUser = 4,
    
    // Driver Management
    ViewDrivers = 5,
    ApproveDriver = 6,
    RejectDriver = 7,
    SuspendDriver = 8,
    
    // Trip Management
    ViewTrips = 9,
    ManageTrips = 10,
    ViewTripReports = 11,
    
    // System Settings
    ManageRoles = 12,
    ManagePermissions = 13,
    SystemSettings = 14
} 