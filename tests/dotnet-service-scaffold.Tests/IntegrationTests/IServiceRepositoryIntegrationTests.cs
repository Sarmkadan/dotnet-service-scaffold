using System.Threading.Tasks;

public interface IServiceRepositoryIntegrationTests
{
    Task AddServiceRegistrationAsync_ShouldAddServiceToDatabase();
    Task GetServiceRegistrationByIdAsync_ShouldReturnService_WhenFound();
    Task GetServiceRegistrationByIdAsync_ShouldReturnNull_WhenNotFound();
    Task UpdateServiceRegistrationAsync_ShouldUpdateServiceInDatabase();
    Task DeleteServiceRegistrationAsync_ShouldRemoveServiceFromDatabase();
    Task GetAllServiceRegistrationsAsync_ShouldReturnAllServices();
    Task GetAllServiceRegistrationsAsync_ShouldReturnEmpty_WhenNoServices();
}