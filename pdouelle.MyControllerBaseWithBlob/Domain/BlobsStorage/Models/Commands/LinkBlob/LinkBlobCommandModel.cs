using System.ComponentModel.DataAnnotations;
using pdouelle.MyControllerBaseWithBlob.Domain.BlobsStorage.Enums;

namespace pdouelle.MyControllerBaseWithBlob.Domain.BlobsStorage.Models.Commands.LinkBlob
{
    public class LinkBlobCommandModel
    {
        [Required] public MyBlobType? Type { get; set; }  
    }
}