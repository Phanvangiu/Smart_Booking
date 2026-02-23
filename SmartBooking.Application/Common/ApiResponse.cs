namespace SmartBooking.Application.Common;
// Trả về T tại vì mỗi feature sẽ có kiểu dữ liệu khác nhau, có thể là UserDto, RoleDto, BookingDto, v.v.
public class ApiResponse<T>
{
  public bool IsSuccess { get; set; }
  public string Message { get; set; } = string.Empty;
  public T? Data { get; set; }
  public List<string> Errors { get; set; } = new();

  // Factory methods — tạo response nhanh, không cần new() mỗi lần
  public static ApiResponse<T> Success(T data, string message = "Success")
      => new() { IsSuccess = true, Message = message, Data = data };

  public static ApiResponse<T> Fail(string error)
      => new() { IsSuccess = false, Errors = new List<string> { error } };

  public static ApiResponse<T> Fail(List<string> errors)
      => new() { IsSuccess = false, Errors = errors };
}