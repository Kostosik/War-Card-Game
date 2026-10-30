using Engine.Core.Assets;
using Engine.Graphics.Fonts;
using Engine.Graphics.Resources;
using Engine.Hosting.Desktop;
using Engine.Runtime;
using Engine.UI.Core;
using Engine.UI.Styling;

namespace GameProject.Game.Flow;

internal sealed class GameSceneContext : IDisposable
{
    private readonly FontHandle _font;
    private bool _disposed;

    public DesktopGameHostContext Host { get; }

    public EngineRuntime Runtime => Host.Runtime;

    public TextureResourceManager UiResources { get; }

    public UiTheme UiTheme { get; }

    public GameSceneContext(
        DesktopGameHostContext host)
    {
        ArgumentNullException.ThrowIfNull(host);

        Host = host;

        UiResources = new TextureResourceManager(
            host.Content,
            host.Graphics.Textures);

        var fontData = host.AssetSource.Load(
            new AssetPath("Fonts/GameFont.TTF"));

        _font = host.Graphics.Fonts.Create(
            fontData,
            new FontDescription(24));

        UiTheme = new UiTheme
        {
            DefaultFont = _font
        };
    }

    public UiSystem CreateUiSystem()
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this);

        

        return new UiSystem(
            Host.Graphics,
            Host.WindowWidth,
            Host.WindowHeight,
            Host.PointerInput,
            Host.TextInput,
            Host.Cursor,
            UiTheme);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        UiResources.Dispose();

        if (_font.IsValid &&
            Host.Graphics.Fonts.Exists(_font))
        {
            Host.Graphics.Fonts.Destroy(_font);
        }
    }
}