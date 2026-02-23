using FluentAssertions;
using SmartBooking.Application.Features.Auth.Commands.Register;

namespace SmartBooking.Application.Tests.Features.Auth;

/// <summary>
/// Test Validator độc lập — không cần mock gì cả.
/// Validator là pure logic: nhận input → trả lỗi hay không.
/// </summary>
public class RegisterCommandValidatorTests
{
  // Tạo 1 lần dùng cho tất cả test trong class
  private readonly RegisterCommandValidator _validator = new();

  // ─── Happy Path ──────────────────────────────────────────────────

  [Fact]
  public void Validate_ValidCommand_ShouldPassWithNoErrors()
  {
    // Arrange
    var command = new RegisterCommand
    {
      FullName = "Nguyen Van A",
      Email = "vana@gmail.com",
      Password = "Password123",
      PhoneNumber = "0901234567"
    };

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeTrue();
    result.Errors.Should().BeEmpty();
  }

  [Fact]
  public void Validate_NullPhoneNumber_ShouldPass()
  {
    // PhoneNumber nullable — không điền vẫn hợp lệ
    var command = new RegisterCommand
    {
      FullName = "Nguyen Van A",
      Email = "vana@gmail.com",
      Password = "Password123",
      PhoneNumber = null
    };

    var result = _validator.Validate(command);
    result.IsValid.Should().BeTrue();
  }

  // ─── FullName ────────────────────────────────────────────────────

  [Fact]
  public void Validate_EmptyFullName_ShouldHaveError()
  {
    var command = ValidCommand() with { FullName = "" };

    var result = _validator.Validate(command);

    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e => e.ErrorMessage == "Họ tên không được để trống");
  }

  [Fact]
  public void Validate_FullNameExceeds100Chars_ShouldHaveError()
  {
    var command = ValidCommand() with { FullName = new string('A', 101) };

    var result = _validator.Validate(command);

    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e => e.ErrorMessage == "Họ tên không quá 100 ký tự");
  }

  // ─── Email ───────────────────────────────────────────────────────

  [Fact]
  public void Validate_EmptyEmail_ShouldHaveError()
  {
    var command = ValidCommand() with { Email = "" };

    var result = _validator.Validate(command);

    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e => e.ErrorMessage == "Email không được để trống");
  }

  [Theory]
  [InlineData("khonghople")]       // thiếu @
  [InlineData("khong@")]           // thiếu domain
  [InlineData("@gmail.com")]       // thiếu local part
  [InlineData("khong hop le")]     // có khoảng trắng
  public void Validate_InvalidEmailFormat_ShouldHaveError(string invalidEmail)
  {
    var command = ValidCommand() with { Email = invalidEmail };

    var result = _validator.Validate(command);

    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e => e.ErrorMessage == "Email không hợp lệ");
  }

  // ─── Password ────────────────────────────────────────────────────

  [Fact]
  public void Validate_EmptyPassword_ShouldHaveError()
  {
    var command = ValidCommand() with { Password = "" };

    var result = _validator.Validate(command);

    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e => e.ErrorMessage == "Mật khẩu không được để trống");
  }

  [Fact]
  public void Validate_PasswordTooShort_ShouldHaveError()
  {
    var command = ValidCommand() with { Password = "Pass1" }; // 5 ký tự

    var result = _validator.Validate(command);

    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e => e.ErrorMessage == "Mật khẩu tối thiểu 8 ký tự");
  }

  [Fact]
  public void Validate_PasswordNoUppercase_ShouldHaveError()
  {
    var command = ValidCommand() with { Password = "password123" }; // không có chữ hoa

    var result = _validator.Validate(command);

    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e => e.ErrorMessage == "Mật khẩu phải có ít nhất 1 chữ hoa");
  }

  [Fact]
  public void Validate_PasswordNoNumber_ShouldHaveError()
  {
    var command = ValidCommand() with { Password = "PasswordOnly" }; // không có số

    var result = _validator.Validate(command);

    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e => e.ErrorMessage == "Mật khẩu phải có ít nhất 1 số");
  }

  // ─── PhoneNumber ─────────────────────────────────────────────────

  [Theory]
  [InlineData("090123")]         // quá ngắn
  [InlineData("090123456789")]   // quá dài
  [InlineData("090abcd1234")]    // có chữ
  [InlineData("090 123 4567")]   // có khoảng trắng
  public void Validate_InvalidPhoneNumber_ShouldHaveError(string invalidPhone)
  {
    var command = ValidCommand() with { PhoneNumber = invalidPhone };

    var result = _validator.Validate(command);

    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e => e.ErrorMessage == "Số điện thoại không hợp lệ");
  }

  // ─── Helper ──────────────────────────────────────────────────────

  /// <summary>
  /// Tạo command hợp lệ làm baseline.
  /// Dùng "with" để override từng field cần test.
  /// Tránh lặp code setup ở mỗi test.
  /// </summary>
  private static RegisterCommand ValidCommand() => new()
  {
    FullName = "Nguyen Van A",
    Email = "vana@gmail.com",
    Password = "Password123"
  };
}