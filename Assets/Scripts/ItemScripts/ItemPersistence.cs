using UnityEngine;

public class ItemPersistence : BasePersistence
{
    protected override string Prefix => "Item";
    protected override string StateSuffix => "IsCollected";

    [ContextMenu("Set Collected")]
    public void MarkAsCollected() => SetState(true);

    [ContextMenu("Set Uncollected")]
    public void ResetState() => SetState(false);
}