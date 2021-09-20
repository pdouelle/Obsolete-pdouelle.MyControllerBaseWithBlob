using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using pdouelle.MyControllerBaseWithBlob.Domain.BaseEntities;
using pdouelle.MyControllerBaseWithBlob.Domain.BlobsStorage.Entities;

namespace pdouelle.MyControllerBaseWithBlob.Domain.Blobs.Factory
{
    public class BlobFactory : IBlobFactory
    {
        private readonly IMapper _mapper;

        public BlobFactory(IMapper mapper)
        {
            _mapper = mapper;
        }

        public IEnumerable<TEntityDto> MapBlob<TEntity, TEntityDto, TBlobDto>
            (IEnumerable<TEntity> itemsToMap, IEnumerable<BlobStorage> blobs)
            where TBlobDto : IEntityBlob
        {
            var mappedItems = _mapper.Map<List<TEntityDto>>(itemsToMap);
            
            if (blobs == null) return mappedItems;

            PropertyInfo[] props = GetBlobProperties<TEntityDto, TBlobDto>();

            IEnumerable<TBlobDto> mappedBlobs = mappedItems
                .SelectMany(x => props.Select(pi => (TBlobDto) pi.GetValue(x)))
                .Where(x => x != null);

            List<TBlobDto> updateRef = mappedBlobs
                .Join(blobs, b => b.Name, bs => bs.Name,
                    (b, bs) =>
                        _mapper.Map(bs, b))
                .ToList();

            return mappedItems;
        }

        public TEntityDto MapBlob<TEntity, TEntityDto, TBlobDto>
            (TEntity itemToMap, IEnumerable<BlobStorage> blobs)
            where TBlobDto : IEntityBlob
        {
            var mappedItem = _mapper.Map<TEntityDto>(itemToMap);

            if (blobs == null) return mappedItem;

            PropertyInfo[] props = GetBlobProperties<TEntityDto, TBlobDto>();

            IEnumerable<TBlobDto> mappedBlobs = props
                .Select(pi => (TBlobDto) pi.GetValue(mappedItem))
                .Where(x => x != null);

            List<TBlobDto> updateRef = mappedBlobs
                .Join(blobs, b => b.Name, bs => bs.Name,
                    (b, bs) =>
                        _mapper.Map(bs, b))
                .ToList();

            return mappedItem;
        }

        private static PropertyInfo[] GetBlobProperties<TEntityDto, TBlobDto>() => typeof(TEntityDto).GetProperties()
            .Where(x => x.PropertyType == typeof(TBlobDto))
            .ToArray();
    }
}