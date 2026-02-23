namespace SmartBooking.Application.Common;

public class PagedResult<T>
{
  public List<T> Items { get; set; } = new();
  public int TotalCount { get; set; }    // tổng số records
  public int PageNumber { get; set; }    // đang ở trang mấy
  public int PageSize { get; set; }      // mỗi trang bao nhiêu records

  // Tính toán tự động — không cần client tính
  public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
  public bool HasPreviousPage => PageNumber > 1;
  public bool HasNextPage => PageNumber < TotalPages;
}