// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Threading.Tasks;

namespace DotnetServiceScaffold.Tests.Application.Services;

/// <summary>
/// Interface for the UserServiceTests class.
/// </summary>
public interface IUserServiceTests
{
    Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists();
    Task GetUserByIdAsync_ShouldReturnNull_WhenUserDoesNotExist();
    Task CreateUserAsync_ShouldReturnUser_WhenUserIsCreatedSuccessfully();
    Task CreateUserAsync_ShouldThrowException_WhenUsernameAlreadyExists();
    Task UpdateUserAsync_ShouldUpdateUser_WhenUserExists();
    Task UpdateUserAsync_ShouldThrowException_WhenUserDoesNotExist();
    Task DeleteUserAsync_ShouldDeleteUser_WhenUserExists();
}
