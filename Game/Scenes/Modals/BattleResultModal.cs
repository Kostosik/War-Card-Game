using Engine.Core.Assets;
using Engine.Core.Events;
using Engine.UI.Actions;
using Engine.UI.Assets;
using Engine.UI.Controls;
using Engine.UI.Core;
using GameProject.Combat;
using GameProject.Game.Flow;

namespace GameProject.Game.Scenes.Modals;

internal abstract class BattleResultModal : IDisposable
{
    private sealed record ReturnRequestedEvent(
        CombatResult Result) : IEvent;

    private readonly UiModal _modal;
    private readonly EventSubscription _returnSubscription;
    private readonly Action<CombatResult> _onReturn;
    private readonly CombatResult _result;

    private bool _returnRequested;

    protected BattleResultModal(
        GameSceneContext context,
        UiSystem ui,
        UiActionRuntime uiActions,
        string assetPath,
        string actionId,
        CombatResult result,
        Action<CombatResult> onReturn)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(ui);
        ArgumentNullException.ThrowIfNull(uiActions);
        ArgumentException.ThrowIfNullOrWhiteSpace(assetPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(actionId);
        ArgumentNullException.ThrowIfNull(onReturn);

        _result = result;
        _onReturn = onReturn;

        uiActions.Register(
            actionId,
            () => new ReturnRequestedEvent(result));

        _returnSubscription =
            context.Runtime.Events.Subscribe<ReturnRequestedEvent>(
                HandleReturnRequested);

        var asset = context.Host.Content.Load<UiAsset>(
            new AssetPath(assetPath));

        var instance = uiActions.Load(asset);

        _modal = new UiModal(
            ui.Overlays,
            ui.Focus)
        {
            CloseOnEscape = false
        };

        _modal.SetSize(
            asset.CanvasSize.X + 40.0f,
            asset.CanvasSize.Y + 40.0f);

        _modal.SetContent(instance.Root);
    }

    public void Show()
    {
        if (_returnRequested)
        {
            return;
        }

        _modal.Show();
    }

    private void HandleReturnRequested(
        ReturnRequestedEvent request)
    {
        if (_returnRequested ||
            request.Result != _result)
        {
            return;
        }

        _returnRequested = true;
        _onReturn(_result);
    }

    public void Dispose()
    {
        _returnSubscription.Dispose();
        _modal.Close();
    }
}