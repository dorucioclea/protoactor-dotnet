namespace Proto.Cluster.Identity
{
    public class IdentityStorageExtensions
    {
        public static IIdentityStorage WithConcurrencyLimit(IIdentityStorage storage, int concurrencyLimit)
        {
            return new IdentityStorageConcurrencyLimit(storage, concurrencyLimit);
        }
    }
}