using Avalonia;
using Avalonia.Markup.Xaml;

namespace Runner;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
}