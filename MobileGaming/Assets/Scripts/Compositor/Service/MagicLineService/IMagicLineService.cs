using UnityEngine;

public interface IMagicLineService : ISwitchableService
{
    public void SetData(MagicLinesData data);
    public void SetCamera(Camera camera);
}
