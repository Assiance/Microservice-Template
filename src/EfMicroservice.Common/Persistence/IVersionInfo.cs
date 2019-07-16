namespace EfMicroservice.Common.Persistence
{
    public interface IVersionInfo
    {
        byte[] RowVersion { get; set; }
    }
}
