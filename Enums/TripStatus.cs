namespace DriveApp.Enums;

public enum TripStatus
{
    Requested = 1,      // درخواست شده
    Accepted = 2,       // پذیرفته شده
    DriverArrived = 3,  // راننده رسیده
    InProgress = 4,     // در حال انجام
    Completed = 5,      // تکمیل شده
    Cancelled = 6,      // لغو شده
    Failed = 7          // ناموفق
} 