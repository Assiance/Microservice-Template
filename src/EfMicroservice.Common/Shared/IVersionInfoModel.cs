namespace EfMicroservice.Common.Shared
{
    public interface IVersionInfoModel
    {
        byte[] RowVersion { get; set; }
    }
}
