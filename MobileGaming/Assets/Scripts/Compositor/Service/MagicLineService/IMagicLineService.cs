public interface IMagicLineService : ISwitchableService
{
    public void SetData(MagicLinesData data);
    public void CreateLink(ILinkable startLinkable, ILinkable endLinkable);
    public void CanDestroyLinks(bool value);
}
