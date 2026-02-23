using SmartBooking.Domain.Common;

namespace SmartBooking.Domain.Entities
{
  public class Role : BaseEntity
  {
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid RoleId { get; set; }
    public ICollection<User> Users { get; set; } = new List<User>();


  }

}