namespace SmartBooking.Domain.Constants;

/// <summary>
/// Các permission code dùng trong toàn bộ hệ thống.
/// Dùng constant thay vì string thô để tránh typo.
/// 
/// Ví dụ:
///   if (await _permissionService.HasPermissionAsync(userId, PermissionCodes.Booking.Create))
/// </summary>
public static class PermissionCodes
{
  public static class Booking
  {
    public const string ViewAll = "booking.view.all";   // xem tất cả booking
    public const string ViewOwn = "booking.view.own";   // chỉ xem booking của mình
    public const string Create = "booking.create";     // tạo booking cho customer
    public const string Cancel = "booking.cancel";     // hủy booking
    public const string UpdateStatus = "booking.update.status"; // cập nhật trạng thái
  }

  public static class Service
  {
    public const string View = "service.view";
    public const string Create = "service.create";
    public const string Edit = "service.edit";
    public const string Delete = "service.delete";
  }

  public static class Staff
  {
    public const string Invite = "staff.invite";       // mời nhân viên
    public const string Remove = "staff.remove";       // xóa nhân viên
    public const string RoleAssign = "staff.role.assign";  // gán role cho staff
  }

  public static class Role
  {
    public const string Create = "role.create";
    public const string Edit = "role.edit";
    public const string Delete = "role.delete";
    public const string PermissionAssign = "role.permission.assign"; // gán permission cho role
  }

  public static class Report
  {
    public const string View = "report.view";
    public const string Export = "report.export";
  }

  public static class BusinessSettings
  {
    public const string Manage = "business.settings"; // cấu hình thông tin business
    public const string Hours = "business.hours";    // cấu hình giờ làm việc
  }
}