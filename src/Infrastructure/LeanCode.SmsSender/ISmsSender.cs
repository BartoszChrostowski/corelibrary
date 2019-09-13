using System.Threading.Tasks;

namespace LeanCode.SmsSender
{
    public interface ISmsSender
    {
        Task SendAsync(string message, string phoneNumber);
    }
}
