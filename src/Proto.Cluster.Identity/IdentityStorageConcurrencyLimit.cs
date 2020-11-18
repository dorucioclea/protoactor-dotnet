using System.Threading;
using System.Threading.Tasks;

namespace Proto.Cluster.Identity
{
    internal class IdentityStorageConcurrencyLimit : IIdentityStorage
    {
        private readonly IIdentityStorage _storage;
        private readonly SemaphoreSlim _concurrencyLimit;

        public IdentityStorageConcurrencyLimit(IIdentityStorage storage, int maxConcurrentCalls)
        {
            _storage = storage;
            _concurrencyLimit = new SemaphoreSlim(maxConcurrentCalls, maxConcurrentCalls);
        }

        public async Task<StoredActivation?> TryGetExistingActivationAsync(ClusterIdentity clusterIdentity,
            CancellationToken ct)
        {
            try
            {
                await _concurrencyLimit.WaitAsync(ct);
                return await _storage.TryGetExistingActivationAsync(clusterIdentity, ct);
            }
            finally
            {
                _concurrencyLimit.Release();
            }
        }

        public async Task<SpawnLock?> TryAcquireLockAsync(ClusterIdentity clusterIdentity, CancellationToken ct)
        {
            try
            {
                await _concurrencyLimit.WaitAsync(ct);
                return await _storage.TryAcquireLockAsync(clusterIdentity, ct);
            }
            finally
            {
                _concurrencyLimit.Release();
            }
            
        }

        public Task<StoredActivation?> WaitForActivationAsync(ClusterIdentity clusterIdentity, CancellationToken ct)
        {
            //Since this does not make progress on its own, it might potentially deadlock if we use the semaphore..
            return _storage.WaitForActivationAsync(clusterIdentity, ct);
        }

        public async Task RemoveLock(SpawnLock spawnLock, CancellationToken ct)
        {
            try
            {
                await _concurrencyLimit.WaitAsync(ct);
                await _storage.RemoveLock(spawnLock, ct);
            }
            finally
            {
                _concurrencyLimit.Release();
            }
            
        }

        public async Task StoreActivation(string memberId, SpawnLock spawnLock, PID pid, CancellationToken ct)
        {
            try
            {
                await _concurrencyLimit.WaitAsync(ct);
                await _storage.StoreActivation(memberId, spawnLock, pid, ct);
            }
            finally
            {
                _concurrencyLimit.Release();
            }
        }

        public async Task RemoveActivation(PID pid, CancellationToken ct)
        {
            try
            {
                await _concurrencyLimit.WaitAsync(ct);
                await _storage.RemoveActivation(pid, ct);
            }
            finally
            {
                _concurrencyLimit.Release();
            }
            
        }

        public async Task RemoveMemberIdAsync(string memberId, CancellationToken ct)
        {
            try
            {
                await _concurrencyLimit.WaitAsync(ct);
                await _storage.RemoveMemberIdAsync(memberId, ct);
            }
            finally
            {
                _concurrencyLimit.Release();
            }
            
        }
        
        public void Dispose()
        {
            _storage.Dispose();
        }
    }
}