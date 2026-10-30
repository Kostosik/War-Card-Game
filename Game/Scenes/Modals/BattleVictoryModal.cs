using Engine.UI.Actions;
using Engine.UI.Core;
using GameProject.Combat;
using GameProject.Game.Flow;

namespace GameProject.Game.Scenes.Modals;

internal sealed class BattleVictoryModal : BattleResultModal
{
    public BattleVictoryModal(
        GameSceneContext context,
        UiSystem ui,
        UiActionRuntime uiActions,
        Action<CombatResult> onReturn)
        : base(
            context,
            ui,
            uiActions,
            "UI/BattleVictoryModal.ui",
            "ReturnFromVictory",
            CombatResult.PlayerVictory,
            onReturn)
    {
    }
}