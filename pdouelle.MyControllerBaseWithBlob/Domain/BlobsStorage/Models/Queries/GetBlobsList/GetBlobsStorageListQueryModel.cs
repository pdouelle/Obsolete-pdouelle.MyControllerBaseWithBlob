using System.Collections.Generic;
using MediatR;

namespace pdouelle.MyControllerBaseWithBlob.Domain.BlobsStorage.Models.Queries.GetBlobsList
{
    public class GetBlobsStorageListQueryModel : IRequest<IEnumerable<Entities.BlobStorage>>
    {
        public string ContainerName { get; set; }
        public string Prefix { get; set; }
    }
}