namespace DotNetAidLib.Core.Context
{
    public interface IContextSupport
    {
        Context Context { get; }
        void DisposeContext();
    }
}