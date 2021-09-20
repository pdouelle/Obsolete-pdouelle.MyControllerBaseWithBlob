using System.Collections.Generic;

namespace pdouelle.MyControllerBaseWithBlob.Domain.BaseEntities
{
    public interface IEntityBlobs<TBlob>
    {
        public ICollection<TBlob> Blobs { get; set; }
    }
}