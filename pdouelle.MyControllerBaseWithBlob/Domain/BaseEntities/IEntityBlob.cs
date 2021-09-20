namespace pdouelle.MyControllerBaseWithBlob.Domain.BaseEntities
{
    public interface IEntityBlob
    {
        public string ContainerName { get; set; }
        public string Name { get; set; }
    }
}