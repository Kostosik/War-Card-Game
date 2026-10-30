using Engine.UI.Actions;
using Engine.UI.Core;
using GameProject.Combat;
using GameProject.Game.Flow;

namespace GameProject.Game.Scenes.Modals;

internal sealed class BattleDefeatModal : BattleResultModal
{
    public BattleDefeatModal(
        GameSceneContext context,
        UiSystem ui,
        UiActionRuntime uiActions,
        Action<CombatResult> onReturn)
        : base(
            context,
            ui,
            uiActions,
            "UI/BattleDefeatModal.ui",
            "ReturnFromDefeat",
            CombatResult.PlayerDefeat,
            onReturn)
    {
    }
}