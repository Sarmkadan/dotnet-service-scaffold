namespace DotnetServiceScaffold.Tests
{
    public interface ILogContextServiceTests
    {
        void SetAndGetProperties();
        void OverwriteProperties();
        void MissingKeyBehavior();
        void IsolationBetweenScopes();
    }
}