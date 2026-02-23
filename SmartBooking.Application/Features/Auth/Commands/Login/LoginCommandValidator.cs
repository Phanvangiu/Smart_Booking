using FluentValidation;

namespace SmartBooking.Application.Features.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
  public LoginCommandValidator()
  {
    RuleFor(x => x.Email)
        .NotEmpty().WithMessage("Email không được để trống")
        .EmailAddress().WithMessage("Email không hợp lệ");

    RuleFor(x => x.Password)
        .NotEmpty().WithMessage("Mật khẩu không được để trống");
    // Login chỉ check không rỗng — không check format mạnh/yếu
    // Lý do: user đã đăng ký rồi, chỉ cần verify đúng password
  }
}