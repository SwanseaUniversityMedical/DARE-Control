
namespace BL.Services
{
    public interface IEncDecHelper
    {
        string Decrypt(string encryptedText);
        string Encrypt(string text);
    }
}
