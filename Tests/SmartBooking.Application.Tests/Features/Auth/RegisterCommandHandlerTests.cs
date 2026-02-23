using FluentAssertions;
using Moq;
using SmartBooking.Application.Features.Auth.Commands.Register;
using SmartBooking.Application.Interfaces;
using SmartBooking.Domain.Entities;
using System.Linq.Expressions;

namespace SmartBooking.Application.Tests.Features.Auth;

/// <summary>
/// Test Handler — mock tất cả dependency, không cần DB thật.
///
/// Mỗi test kiểm tra 1 case duy nhất:
/// - Email đã tồn tại → fail
/// - Role không tồn tại → fail
/// - Hợp lệ → success, gọi đúng method
/// </summary>
public class RegisterCommandHandlerTests
{
  // ─── Mock tất cả dependency ──────────────────────────────────────
  private readonly Mock<IUnitOfWork> _uowMock = new();
  private readonly Mock<ITokenService> _tokenServiceMock = new();
  private readonly Mock<IPasswordService> _passwordServiceMock = new();
  private readonly Mock<IRepository<User>> _userRepoMock = new();
  private readonly Mock<IRepository<Role>> _roleRepoMock = new();
  private readonly RegisterCommandHandler _handler;

  public RegisterCommandHandlerTests()
  {
    // UnitOfWork expose các Repository qua property
    // Mock: khi gọi _uow.Users → trả về mock repo
    _uowMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
    _uowMock.Setup(u => u.Roles).Returns(_roleRepoMock.Object);

    _handler = new RegisterCommandHandler(
        _uowMock.Object,
        _tokenServiceMock.Object,
        _passwordServiceMock.Object);
  }

  // ─── CASE 1: Email đã tồn tại ────────────────────────────────────

  [Fact]
  public async Task Handle_EmailAlreadyExists_ShouldReturnFail()
  {
    // Arrange
    var command = ValidCommand();

    // Mock: DB báo email đã tồn tại
    _userRepoMock
        .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<User, bool>>>(), default))
        .ReturnsAsync(true);

    // Act
    var result = await _handler.Handle(command, default);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors.Should().Contain("Email đã được sử dụng");

    // Quan trọng: không được tạo user mới khi email đã tồn tại
    _userRepoMock.Verify(
        r => r.AddAsync(It.IsAny<User>(), default),
        Times.Never);
  }

  // ─── CASE 2: Không tìm thấy Role Customer ────────────────────────

  [Fact]
  public async Task Handle_CustomerRoleNotFound_ShouldReturnFail()
  {
    // Arrange
    var command = ValidCommand();

    // Email chưa tồn tại
    _userRepoMock
        .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<User, bool>>>(), default))
        .ReturnsAsync(false);

    // Role Customer không tồn tại trong DB — seed data bị thiếu
    _roleRepoMock
        .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Role, bool>>>(), default))
        .ReturnsAsync((Role?)null);

    // Act
    var result = await _handler.Handle(command, default);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors.Should().Contain("Lỗi cấu hình hệ thống: không tìm thấy Role");
  }

  // ─── CASE 3: Register thành công ─────────────────────────────────

  [Fact]
  public async Task Handle_ValidCommand_ShouldReturnSuccessWithToken()
  {
    // Arrange
    var command = ValidCommand();
    var customerRole = new Role
    {
      Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
      Name = "Customer"
    };

    SetupHappyPath(customerRole);

    // Act
    var result = await _handler.Handle(command, default);

    // Assert — kết quả
    result.IsSuccess.Should().BeTrue();
    result.Data.Should().NotBeNull();
    result.Data!.Email.Should().Be(command.Email.ToLower().Trim());
    result.Data.Role.Should().Be("Customer");
    result.Data.AccessToken.Should().Be("fake_access_token");
    result.Data.RefreshToken.Should().Be("fake_refresh_token");
  }

  [Fact]
  public async Task Handle_ValidCommand_ShouldCallAddAsyncOnce()
  {
    // Arrange
    var command = ValidCommand();
    SetupHappyPath(new Role { Id = Guid.NewGuid(), Name = "Customer" });

    // Act
    await _handler.Handle(command, default);

    // Assert — verify interaction
    // AddAsync phải được gọi đúng 1 lần với bất kỳ User nào
    _userRepoMock.Verify(
        r => r.AddAsync(It.IsAny<User>(), default),
        Times.Once);
  }

  [Fact]
  public async Task Handle_ValidCommand_ShouldCallSaveChangesOnce()
  {
    // Arrange
    var command = ValidCommand();
    SetupHappyPath(new Role { Id = Guid.NewGuid(), Name = "Customer" });

    // Act
    await _handler.Handle(command, default);

    // Assert — SaveChanges phải được gọi đúng 1 lần
    // Đảm bảo không bị gọi nhiều lần (performance issue)
    _uowMock.Verify(
        u => u.SaveChangesAsync(default),
        Times.Once);
  }

  [Fact]
  public async Task Handle_ValidCommand_ShouldHashPassword()
  {
    // Arrange
    var command = ValidCommand();
    SetupHappyPath(new Role { Id = Guid.NewGuid(), Name = "Customer" });

    // Act
    await _handler.Handle(command, default);

    // Assert — password phải được hash, không lưu plain text
    _passwordServiceMock.Verify(
        p => p.HashPassword(command.Password),
        Times.Once);
  }

  [Fact]
  public async Task Handle_ValidCommand_UserEmailShouldBeLowercase()
  {
    // Arrange — email có chữ hoa
    var command = ValidCommand() with { Email = "VanA@Gmail.COM" };
    var capturedUser = (User?)null;

    SetupHappyPath(new Role { Id = Guid.NewGuid(), Name = "Customer" });

    // Capture user được tạo ra
    _userRepoMock
        .Setup(r => r.AddAsync(It.IsAny<User>(), default))
        .Callback<User, CancellationToken>((user, _) => capturedUser = user);

    // Act
    await _handler.Handle(command, default);

    // Assert — email phải được lowercase trước khi lưu
    capturedUser.Should().NotBeNull();
    capturedUser!.Email.Should().Be("vana@gmail.com");
  }

  // ─── Helpers ─────────────────────────────────────────────────────

  private static RegisterCommand ValidCommand() => new()
  {
    FullName = "Nguyen Van A",
    Email = "vana@gmail.com",
    Password = "Password123"
  };

  /// <summary>
  /// Setup tất cả mock cho happy path (mọi thứ thành công).
  /// Dùng lại ở nhiều test thay vì lặp code.
  /// </summary>
  private void SetupHappyPath(Role customerRole)
  {
    _userRepoMock
        .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<User, bool>>>(), default))
        .ReturnsAsync(false);

    _roleRepoMock
        .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Role, bool>>>(), default))
        .ReturnsAsync(customerRole);

    _passwordServiceMock
        .Setup(p => p.HashPassword(It.IsAny<string>()))
        .Returns("hashed_password");

    _tokenServiceMock
        .Setup(t => t.GenerateRefreshToken())
        .Returns("fake_refresh_token");

    _tokenServiceMock
        .Setup(t => t.GenerateAccessToken(It.IsAny<User>()))
        .Returns("fake_access_token");

    _uowMock
        .Setup(u => u.SaveChangesAsync(default))
        .ReturnsAsync(1);
  }
}