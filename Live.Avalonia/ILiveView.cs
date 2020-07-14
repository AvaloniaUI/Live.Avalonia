using Avalonia.Controls;

namespace Live.Avalonia
{
    public interface ILiveView
    {
        object CreateView(Window window);
    }
}