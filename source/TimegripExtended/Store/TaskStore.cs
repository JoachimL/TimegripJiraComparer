using System.Data.Entity;
using TimegripExtended.Business.Domain;

namespace TimegripExtended.Store
{
    public class TaskStoreContext : DbContext
    {
        public DbSet<ExcludedTask> ExcludedTasks { get; set; }
    }
}