using IdentityMicroservice.DataAccess;

namespace IdentityMicroservice.Repositories
{
    public abstract class BaseRepository
    {
        protected BaseRepository(DataContext context)
        {
            DataContext = context;
        }

        public DataContext DataContext { get; private set; }
    }
}
