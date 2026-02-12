namespace DeMasterProCloud.Common.Infrastructure
{
    public interface ISerializer
    {
        byte[] Serialize(object obj);
        T DeSerialize<T>(byte[] arrBytes);
    }
}
