using FluentAssertions;
using Moq;
using SmartBooking.Application.Features.Auth.Commands.Login;
using SmartBooking.Application.Interfaces;
using SmartBooking.Domain.Entities;
using System.Linq.Expressions;

namespace SmartBooking.Application.Tests.Features.Auth;

public class LoginCommandHandlerTests
{
  private readonly Mock<IUnitOfWork> _uowMock = new();
  private readonly Mock<ITokenService> _tokenServiceMock = new();
  private readonly Mock<IPasswordService> _passwordServiceMock = new();
  private readonly Mock<IRepository<User>> _userRepoMock = new();
  private readonly Mock<IRepository<Role>> _roleRepoMock = new();
  private readonly LoginCommandHandler _handler;

  public LoginCommandHandlerTests()
  {
    _uowMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
    _uowMock.Setup(u => u.Roles).Returns(_roleRepoMock.Object);

    _handler = new LoginCommandHandler(
        _uowMock.Object,
        _tokenServiceMock.Object,
        _passwordServiceMock.Object);
  }

  // ─── CASE 1: Email không tồn tại ─────────────────────────────────

  [Fact]
  public async Task Handle_EmailNotFound_ShouldReturnFail()
  {
    // Arrange
    _userRepoMock
        .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), default))
        .ReturnsAsync((User?)null); // Không tìm thấy user

    // Act
    var result = await _handler.Handle(new LoginCommand
    {
      Email = "notexist@gmail.com",
      Password = "Password123"
    }, default);

    // Assert
    result.IsSuccess.Should().BeFalse();

    // Quan trọng: cùng message với sai password
    // Tránh để attacker biết email có tồn tại hay không
    result.Errors.Should().Contain("Email hoặc mật khẩu không đúng");
  }

  // ─── CASE 2: Tài khoản bị khoá ───────────────────────────────────

  [Fact]
  public async Task Handle_InactiveUser_ShouldReturnFail()
  {
    // Arrange — user tồn tại nhưng bị khoá
    var inactiveUser = new User
    {
      Email = "vana@gmail.com",
      Password = "hashed",
      IsActive = false  // ← bị khoá
    };

    _userRepoMock
        .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), default))
        .ReturnsAsync(inactiveUser);

    // Act
    var result = await _handler.Handle(ValidCommand(), default);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors.Should().Contain("Tài khoản đã bị khoá");
  }

  // ─── CASE 3: User đăng nhập Google, không có password ────────────

  [Fact]
  public async Task Handle_GoogleUser_ShouldReturnFail()
  {
    // Arrange — user đăng nhập Google, không có PasswordHash
    var googleUser = new User
    {
      Email = "vana@gmail.com",
      Password = null,  // ← không có password
      GoogleId = "google_id_123",
      IsActive = true
    };

    _userRepoMock
        .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), default))
        .ReturnsAsync(googleUser);

    // Act
    var result = await _handler.Handle(ValidCommand(), default);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors.Should().Contain("Tài khoản này đăng nhập bằng Google");
  }

  // ─── CASE 4: Sai password ─────────────────────────────────────────

  [Fact]
  public async Task Handle_WrongPassword_ShouldReturnFail()
  {
    // Arrange
    var user = ActiveUser();

    _userRepoMock
        .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), default))
        .ReturnsAsync(user);

    // Mock: password không khớp
    _passwordServiceMock
        .Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
        .Returns(false);

    // Act
    var result = await _handler.Handle(ValidCommand(), default);

    // Assert — cùng message với email không tồn tại
    result.IsSuccess.Should().BeFalse();
    result.Errors.Should().Contain("Email hoặc mật khẩu không đúng");
  }

  // ─── CASE 5: Đăng nhập thành công ────────────────────────────────

  [Fact]
  public async Task Handle_ValidCredentials_ShouldReturnSuccessWithToken()
  {
    // Arrange
    SetupHappyPath();

    // Act
    var result = await _handler.Handle(ValidCommand(), default);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Data.Should().NotBeNull();
    result.Data!.AccessToken.Should().Be("fake_access_token");
    result.Data.RefreshToken.Should().Be("fake_refresh_token");
    result.Data.Role.Should().Be("Customer");
  }

  [Fact]
  public async Task Handle_ValidCredentials_ShouldRotateRefreshToken()
  {
    // Arrange
    var user = ActiveUser();
    var oldRefreshToken = user.RefreshToken;

    _userRepoMock
        .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), default))
        .ReturnsAsync(user);
    _passwordServiceMock
        .Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
        .Returns(true);
    _roleRepoMock
        .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
        .ReturnsAsync(new Role { Name = "Customer" });
    _tokenServiceMock
        .Setup(t => t.GenerateRefreshToken())
        .Returns("new_refresh_token"); // ← token mới
    _tokenServiceMock
        .Setup(t => t.GenerateAccessToken(It.IsAny<User>()))
        .Returns("fake_access_token");
    _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

    // Act
    await _handler.Handle(ValidCommand(), default);

    // Assert — RefreshToken phải được thay bằng token mới (rotation)
    user.RefreshToken.Should().Be("new_refresh_token");
    user.RefreshToken.Should().NotBe(oldRefreshToken);

    // Update phải được gọi để lưu token mới vào DB
    _userRepoMock.Verify(r => r.Update(user), Times.Once);
    _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
  }

  // ─── Helpers ─────────────────────────────────────────────────────

  private static LoginCommand ValidCommand() => new()
  {
    Email = "vana@gmail.com",
    Password = "Password123"
  };

  private static User ActiveUser() => new()
  {
    Id = Guid.NewGuid(),
    Email = "vana@gmail.com",
    Password = "hashed_password",
    IsActive = true,
    RoleId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
    RefreshToken = "old_refresh_token"
  };

  private void SetupHappyPath()
  {
    var user = ActiveUser();
    var role = new Role { Id = user.RoleId, Name = "Customer" };

    _userRepoMock
        .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), default))
        .ReturnsAsync(user);
    _passwordServiceMock
        .Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
        .Returns(true);
    _roleRepoMock
        .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
        .ReturnsAsync(role);
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