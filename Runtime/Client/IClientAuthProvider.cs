namespace Idem.Client
{
    public interface IClientAuthProvider
    {
        public const string SkipAuthorization = "Demo";

        string GetPlayerId();

        string GetAuthString()
        {
            return SkipAuthorization;
        }
    }
}