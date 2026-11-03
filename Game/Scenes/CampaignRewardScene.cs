using Engine.Core.Math;
using Engine.Core.Time;
using Engine.Graphics.Commands;
using Engine.UI.Controls;
using Engine.UI.Core;
using Engine.UI.Layout;
using GameProject.Game.Campaign;
using GameProject.Game.Flow;

namespace GameProject.Game.Scenes;

internal sealed class CampaignRewardScene : IGameScene
{
    private readonly CampaignState _campaign;

    private GameSceneContext? _context;
    private Action<IGameScene>? _requestScene;

    private UiSystem? _ui;
    private UiCanvas? _canvas;
    private UiPanel? _backgroundPanel;

    public CampaignRewardScene(CampaignState campaign)
    {
        ArgumentNullException.ThrowIfNull(campaign);

        if (!campaign.Progression.HasPendingReward)
        {
            throw new ArgumentException(
                "There are no pending campaign rewards.",
                nameof(campaign));
        }

        _campaign = campaign;
    }

    public void Initialize(
        GameSceneContext context,
        Action<IGameScene> requestScene)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(requestScene);

        _context = context;
        _requestScene = requestScene;

        _ui = context.CreateUiSystem();
        _canvas = new UiCanvas();

        _ui.Root.AddChild(_canvas);

        _backgroundPanel = new UiPanel
        {
            Width = context.Host.WindowWidth,
            Height = context.Host.WindowHeight,
            Background = new UiColor(18, 22, 30),
            LayoutMode = UiLayoutMode.Anchor,
            Anchor = UiAnchor.TopLeft,
            ZIndex = -100
        };

        _canvas.AddChild(_backgroundPanel);

        AddLabel(
            "НАГРАДА ЗА ОПЕРАЦИЮ",
            new Vector2(0, 55),
            800,
            50,
            30);

        AddLabel(
            $"РЕСУРСЫ ШТАБА: {_campaign.Progression.Resources}\n" +
            "Выберите одну из наград.",
            new Vector2(0, 115),
            800,
            65,
            18);

        var rewardIds = _campaign.Progression.PendingRewardIds;

        for (var i = 0; i < rewardIds.Count; i++)
        {
            var reward = CampaignRewardCatalog.FindById(rewardIds[i])
                ?? throw new InvalidDataException(
                    $"Unknown campaign reward: {rewardIds[i]}");

            var offsetX = (i - (rewardIds.Count - 1) / 2f) * 380;

            AddLabel(
                $"{reward.Title}\n\n{reward.Description}",
                new Vector2(offsetX, 220),
                330,
                150,
                18);

            var button = new UiButton("ВЫБРАТЬ")
            {
                Width = 220,
                Height = 50,
                FontSize = 18
            };

            ConfigureAnchor(
                button,
                UiAnchor.TopCenter,
                new Vector2(offsetX, 395));

            button.Clicked += () => ClaimReward(reward.Id);

            _canvas.AddChild(button);
        }

        context.Host.Resized += OnHostResized;
    }

    public void Update(TimeSnapshot time)
    {
        _ui?.Update(time.Delta.TotalSeconds);
    }

    public void Render(double interpolationAlpha)
    {
        _ui?.Render();
    }

    public void Dispose()
    {
        if (_context is not null)
        {
            _context.Host.Resized -= OnHostResized;
        }

        if (_ui is not null && _canvas is not null)
        {
            _ui.Root.RemoveChild(_canvas);
        }

        _backgroundPanel = null;
        _canvas = null;
        _ui = null;
        _context = null;
        _requestScene = null;
    }

    private void ClaimReward(string rewardId)
    {
        if (!_campaign.Progression.HasPendingReward)
            return;

        _campaign.Progression.ClaimReward(rewardId);

        CampaignSaveStore.Save(_campaign);

        _requestScene?.Invoke(
            new HeadquartersScene(_campaign));
    }

    private void AddLabel(
        string text,
        Vector2 offset,
        float width,
        float height,
        float fontSize)
    {
        var label = new UiLabel(text)
        {
            Width = width,
            Height = height,
            FontSize = fontSize,
            LayoutMode = UiLayoutMode.Anchor,
            Anchor = UiAnchor.TopCenter,
            Offset = offset,
            HorizontalAlignment = UiHorizontalAlignment.Center,
            VerticalAlignment = UiVerticalAlignment.Center
        };

        _canvas!.AddChild(label);
    }

    private static void ConfigureAnchor(
        UiWidget widget,
        UiAnchor anchor,
        Vector2 offset)
    {
        widget.LayoutMode = UiLayoutMode.Anchor;
        widget.Anchor = anchor;
        widget.Offset = offset;
    }

    private void OnHostResized(int width, int height)
    {
        _ui?.Resize(width, height);

        if (_backgroundPanel is not null)
        {
            _backgroundPanel.Width = width;
            _backgroundPanel.Height = height;
        }
    }
}