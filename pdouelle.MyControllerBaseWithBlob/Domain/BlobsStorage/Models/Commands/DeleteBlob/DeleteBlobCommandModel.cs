using MediatR;

namespace pdouelle.MyControllerBaseWithBlob.Domain.BlobsStorage.Models.Commands.DeleteBlob
{
    public class DeleteBlobCommandModel : IRequest<bool>
    {
        public string ContainerName { get; set; }
        public string Name { get; set; }
    }
}