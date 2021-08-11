using System.Threading.Tasks;

namespace NorthernCrown.HttpRestClient
{
    public interface IRestClient
    {
        Task<string> Get(string url, int trycount);
        Task<string> Post(string url, object body, string contentType);
        Task<string> Put(string url, object body, string contentType);
        Task<string> Patch(string url, object body, string contentType);
        Task<bool> Delete(string url);
    }
}
