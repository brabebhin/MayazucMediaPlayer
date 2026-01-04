namespace MayazucMediaPlayer.Common
{
    public interface BindingCommandDecorator<T> where T : class
    {
        T Commands { get; }
    }
}
