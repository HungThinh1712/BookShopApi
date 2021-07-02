using System.Threading.Tasks;

namespace BookShopApi
{
    public interface ITask
    {
        Task<bool> ExecutionAsync();
    }
}
