using System.Threading.Tasks;

namespace BookShopApi
{
    public abstract class TaskBase
    {
        public abstract Task<bool> ExecutionAsync();
    }
}
