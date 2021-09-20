using System.Collections.Generic;
using pdouelle.MyControllerBaseWithBlob.Domain.BaseEntities;
using pdouelle.MyControllerBaseWithBlob.Domain.BlobsStorage.Entities;

namespace pdouelle.MyControllerBaseWithBlob.Domain.Blobs.Factory
{
    public interface IBlobFactory
    {
        IEnumerable<TEntityDto> MapBlob<TEntity, TEntityDto, TBlobDto>
            (IEnumerable<TEntity> itemsToMap, IEnumerable<BlobStorage> blobs)
            where TBlobDto : IEntityBlob;

        TEntityDto MapBlob<TEntity, TEntityDto, TBlobDto>(TEntity itemToMap, IEnumerable<BlobStorage> blobs)
            where TBlobDto : IEntityBlob;
    }
}