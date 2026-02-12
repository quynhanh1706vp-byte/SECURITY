namespace DeMasterProCloud.DataModel.Header
{
    public interface IPageInfo
    {
        string[] GetHeaderNameList();

        string GetPageName();

        string GetPageType();
    }
}
