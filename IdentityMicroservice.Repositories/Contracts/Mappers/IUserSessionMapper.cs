namespace IdentityMicroservice.Repositories.Contracts.Mappers
{
    public interface IUserSessionMapper
    {
        DataAccess.UserSession FillFromDomain(Domain.Entities.UserSession userSession);
    }
}