namespace Zubs.Application.Interfaces.Helpers;
public interface ICurrentUserService
{
    Guid? UserId { get; }
}
