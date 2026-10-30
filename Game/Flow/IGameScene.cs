using Engine.Core.Time;

namespace GameProject.Game.Flow;

internal interface IGameScene : IDisposable
{
    void Initialize(
        GameSceneContext context,
        Action<IGameScene> requestScene);

    void Update(TimeSnapshot time);

    void Render(double interpolationAlpha);
}