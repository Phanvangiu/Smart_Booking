namespace SmartBooking.Domain.Enums;

public enum BookingStatus
{
  Pending = 1,   // Vừa tạo, chờ thanh toán
  Confirmed = 2,   // Đã thanh toán
  InProgress = 3,  // Đang thực hiện
  Completed = 4,   // Hoàn thành
  Cancelled = 5,   // Đã hủy
  NoShow = 6 // Khách không đến
}