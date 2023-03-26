namespace DotNetAidLib.Core.AAA.Core
{
    public interface AuthenticationBlocker
    {
        void UnsuccessfullAuthentication(IIdentity id);
        bool IsValid(IIdentity id);
    }
}