namespace EfMicroservice.Common.Persistence
{
    public abstract class BaseEntity<TKey>
    {
        public TKey Id { get; set; }
    }
}
