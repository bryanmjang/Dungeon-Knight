using UnityEngine;

public class EnemyPersistence : BasePersistence
{
    protected override string Prefix => "Enemy";
    protected override string StateSuffix => "IsDead";

    [ContextMenu("Set As Dead")]
    public void MarkAsDead() => SetState(true);

    [ContextMenu("Reset Death State")]
    public void ResetState() => SetState(false);
}