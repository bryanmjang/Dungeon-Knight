using UnityEngine;

public class DoorPersistence : BasePersistence
{
    protected override string Prefix => "Door";
    protected override string StateSuffix => "Opened";


    [ContextMenu("Mark As Opened")]
    public void MarkAsOpen() => SetState(true);

    [ContextMenu("Reset Save State")]
    public void ResetState() => SetState(false);
}
