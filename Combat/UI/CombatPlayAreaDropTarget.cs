using Engine.UI.Controls;
using Engine.UI.Input;
using GameProject.Combat.Cards;

namespace GameProject.Combat.UI;

internal sealed class CombatPlayAreaDropTarget :
    UiPanel,
    IUiDropTarget
{
    private readonly Func<CardDefinition, bool> _tryPlayCard;

    public CombatPlayAreaDropTarget(
        Func<CardDefinition, bool> tryPlayCard)
    {
        ArgumentNullException.ThrowIfNull(tryPlayCard);

        _tryPlayCard = tryPlayCard;
    }

    public bool CanAcceptDrop(
        UiDragPayload payload)
    {
        return payload is UiDragPayload<CardDefinition>;
    }

    public bool OnDrop(
        UiDragContext context)
    {
        if (context.Payload is not UiDragPayload<CardDefinition> payload)
        {
            return false;
        }

        return _tryPlayCard(payload.Value);
    }
}