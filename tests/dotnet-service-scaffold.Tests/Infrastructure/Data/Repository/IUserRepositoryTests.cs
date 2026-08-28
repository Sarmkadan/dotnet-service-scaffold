#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Threading.Tasks;

/// <summary>
/// Contract for tests of the UserRepository class.
/// </summary>
public interface IUserRepositoryTests
{
    Task AddUserAsync_ShouldAddUserToDatabase();

    Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists();

    Task GetUserByIdAsync_ShouldReturnNull_WhenUserDoesNotExist();

    Task GetUserByUsernameAsync_ShouldReturnUser_WhenUserExists();

    Task GetUserByUsernameAsync_ShouldReturnNull_WhenUserDoesNotExist();

    Task UpdateUserAsync_ShouldUpdateUserInDatabase();

    Task DeleteUserAsync_ShouldRemoveUserFromDatabase();

    void Dispose();
}