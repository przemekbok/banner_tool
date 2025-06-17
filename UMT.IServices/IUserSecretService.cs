namespace UMT.IServices
{
    public interface IUserSecretService
    {
        void WriteSecret(string fileName, string data);
        string ReadSecret(string fileName);
        void ClearSecret(string fileName);
    }
}
