namespace IdentityMicroservice.Repositories.Contracts.Mappers
{
    public interface IUserMapper
    {
        Domain.Entities.User FillFromDataAccess(DataAccess.User user);
        DataAccess.User FillFromDomain(Domain.Entities.User user);
    }
}