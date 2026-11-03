using Engine.Core.Math;
using Engine.Core.Time;
using Engine.Graphics.Commands;
using Engine.UI.Controls;
using Engine.UI.Core;
using Engine.UI.Layout;
using GameProject.Game.Campaign;
using GameProject.Game.Flow;
using GameProject.Game.Operations;

namespace GameProject.Game.Scenes;

internal sealed class OperationReportScene : IGameScene
{
    private readonly CampaignState _campaign;
    private readonly OperationState _operation;

    private GameSceneContext? _context;
    private Action<IGameScene>? _requestScene;

    private UiSystem? _ui;
    private UiCanvas? _canvas;
    private UiPanel? _backgroundPanel;

    public OperationReportScene(
        CampaignState campaign,
        OperationState operation)
    {
        ArgumentNullException.ThrowIfNull(campaign);
        ArgumentNullException.ThrowIfNull(operation);

        if (!operation.IsCompleted)
        {
            throw new ArgumentException(
                "Operation must be completed before showing its report.",
                nameof(operation));
        }

        _campaign = campaign;
        _operation = operation;
    }

    public void Initialize(
        GameSceneContext context,
        Action<IGameScene> requestScene)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(requestScene);

        _context = context;
        _requestScene = requestScene;
        CampaignSaveStore.Save(_campaign);
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
            _campaign.Stage == CampaignStage.Completed
                ? "ИТОГ КАМПАНИИ"
                : "ОТЧЁТ ОПЕРАЦИИ",
            new Vector2(0, 55),
            700,
            50,
            30);

        AddLabel(
            _operation.Definition.Name,
            new Vector2(0, 115),
            900,
            40,
            23);

        AddLabel(
            GetOutcomeText(),
            new Vector2(0, 175),
            800,
            45,
            25);

        AddLabel(
            $"ОСНОВНАЯ ЗАДАЧА: " +
            $"{_operation.Definition.MainObjective}\n" +
            $"РЕЗУЛЬТАТ: {GetMainObjectiveResult()}",
            new Vector2(0, 250),
            950,
            75,
            17);

        AddLabel(
            $"ДОПОЛНИТЕЛЬНОЕ УСЛОВИЕ: " +
            $"{_operation.Definition.AdditionalObjective}\n" +
            $"РЕЗУЛЬТАТ: {GetAdditionalObjectiveResult()}",
            new Vector2(0, 345),
            950,
            75,
            17);

        var campaignCompleted =
            _campaign.Stage == CampaignStage.Completed;

        AddLabel(
            campaignCompleted
                ? BuildBattleResults() + "\n\n" + BuildCampaignSummary()
                : BuildBattleResults() + "\n\n" + BuildOperationRewardSummary(),
            new Vector2(0, 445),
            950,
            campaignCompleted ? 180 : 165,
            campaignCompleted ? 14 : 17);

        var hasPendingReward =
            _campaign.Progression.HasPendingReward;

        var backButton = new UiButton(
            campaignCompleted
                ? "В ГЛАВНОЕ МЕНЮ"
                : hasPendingReward
                    ? "ВЫБРАТЬ НАГРАДУ"
                    : "ВЕРНУТЬСЯ В ШТАБ")
        {
            Width = 260,
            Height = 50,
            FontSize = 18,
            LayoutMode = UiLayoutMode.Anchor,
            Anchor = UiAnchor.BottomCenter,
            Offset = new Vector2(0, -45)
        };

        backButton.Clicked += () =>
        {
            if (campaignCompleted)
            {
                _requestScene?.Invoke(new MainMenuScene());
            }
            else if (hasPendingReward)
            {
                _requestScene?.Invoke(
                    new CampaignRewardScene(_campaign));
            }
            else
            {
                _requestScene?.Invoke(
                    new HeadquartersScene(_campaign));
            }
        };

        _canvas.AddChild(backButton);

        context.Host.Resized += OnHostResized;
    }

    private string BuildOperationRewardSummary()
    {
        var reward = CampaignRewardRules.GetResources(_operation);

        return
            $"ПОЛУЧЕНО РЕСУРСОВ: +{reward}\n" +
            $"ЗАПАС ШТАБА: {_campaign.Progression.Resources}";
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
            HorizontalAlignment = UiHorizontalAlignment.Left,
            VerticalAlignment = UiVerticalAlignment.Top
        };

        _canvas!.AddChild(label);
    }

    private string GetOutcomeText()
    {
        if (_campaign.Stage == CampaignStage.Completed)
        {
            return _operation.Outcome switch
            {
                OperationOutcome.Victory => "ПОБЕДА В ВОЙНЕ",
                OperationOutcome.Defeat => "ПОРАЖЕНИЕ В ВОЙНЕ",
                _ => "НЕИЗВЕСТНЫЙ РЕЗУЛЬТАТ"
            };
        }
        return _operation.Outcome switch
        {
            OperationOutcome.Victory => "ОПЕРАЦИЯ УСПЕШНО ЗАВЕРШЕНА",
            OperationOutcome.Defeat => "ОПЕРАЦИЯ ПРОВАЛЕНА",

            _ => throw new InvalidOperationException(
                "Operation has no final outcome.")
        };
    }

    private string BuildCampaignSummary()
    {
        var successfulOperations = _campaign.OperationHistory.Count(
            operation => operation.Outcome == OperationOutcome.Victory);

        var completedAdditionalObjectives =
            _campaign.OperationHistory.Count(
                operation => !operation.HasFailedAdditionalObjective);

        if (_campaign.FinalOperationResult is
            { HasFailedAdditionalObjective: false })
        {
            completedAdditionalObjectives++;
        }

        return
            $"ПОЛОЖЕНИЕ НА ВОЙНЕ: {_campaign.WarPositionDescription}\n" +
            $"УСПЕШНЫХ ОБЫЧНЫХ ОПЕРАЦИЙ: " +
            $"{successfulOperations}/{_campaign.RegularOperationCount}\n" +
            $"ВЫПОЛНЕННЫХ ДОПОЛНИТЕЛЬНЫХ УСЛОВИЙ: " +
            $"{completedAdditionalObjectives}/" +
            $"{_campaign.RegularOperationCount + 1}" + $"\nРЕСУРСЫ ШТАБА: {_campaign.Progression.Resources}"; 

    }

    private string GetMainObjectiveResult()
    {
        return _operation.Outcome == OperationOutcome.Victory
            ? "ВЫПОЛНЕНО"
            : "НЕ ВЫПОЛНЕНО";
    }

    private string GetAdditionalObjectiveResult()
    {
        return _operation.HasFailedAdditionalObjective
            ? "ПРОВАЛЕНО"
            : "ВЫПОЛНЕНО";
    }

    private string BuildBattleResults()
    {
        var lines = new List<string>
        {
            "РЕЗУЛЬТАТЫ СРАЖЕНИЙ:"
        };

        for (var i = 0; i < _operation.TotalBattleCount; i++)
        {
            var result = i < _operation.BattleResults.Count
                ? _operation.BattleResults[i] switch
                {
                    OperationBattleResult.Victory => "ПОБЕДА",
                    OperationBattleResult.Defeat => "ПОРАЖЕНИЕ",
                    _ => "НЕИЗВЕСТНО"
                }
                : "НЕ ПРОВОДИЛОСЬ";

            lines.Add($"Сражение {i + 1}: {result}");
        }

        return string.Join("\n", lines);
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