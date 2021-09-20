using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using pdouelle.Entity;
using pdouelle.GenericMediatR.Models.Generics.Models.Commands.Create;
using pdouelle.GenericMediatR.Models.Generics.Models.Commands.Delete;
using pdouelle.GenericMediatR.Models.Generics.Models.Commands.Patch;
using pdouelle.GenericMediatR.Models.Generics.Models.Commands.Save;
using pdouelle.GenericMediatR.Models.Generics.Models.Commands.Update;
using pdouelle.GenericMediatR.Models.Generics.Models.Queries.IdQuery;
using pdouelle.GenericMediatR.Models.Generics.Models.Queries.ListQuery;
using pdouelle.MyControllerBaseWithBlob.Domain.BaseEntities;
using pdouelle.MyControllerBaseWithBlob.Domain.Blobs.Factory;
using pdouelle.MyControllerBaseWithBlob.Domain.BlobsStorage.Entities;
using pdouelle.MyControllerBaseWithBlob.Domain.BlobsStorage.Models.Commands.DeleteBlob;
using pdouelle.MyControllerBaseWithBlob.Domain.BlobsStorage.Models.Queries.GetBlobsList;

namespace pdouelle.MyControllerBaseWithBlob
{
    public class MyControllerBaseWithBlob<TEntity, TDto, TBlobEntity, TBlobDto, TQueryById> : ControllerBase
        where TEntity : IEntity, IEntityBlobs<TBlobEntity>
        where TDto : IEntity
        where TQueryById : IEntity, IIncludeBlobs, new()
        where TBlobDto : IEntityBlob
        where TBlobEntity : IEntityBlob
    {
        protected readonly ILogger<MyControllerBaseWithBlob<TEntity, TDto, TBlobEntity, TBlobDto, TQueryById>> Logger;
        protected readonly IMediator Mediator;
        protected readonly IMapper Mapper;
        protected readonly IBlobFactory Factory;
        protected string _containerName;

        public MyControllerBaseWithBlob(ILogger<MyControllerBaseWithBlob<TEntity, TDto, TBlobEntity, TBlobDto, TQueryById>> logger, IMediator mediator, IMapper mapper, IBlobFactory factory)
        {
            Logger = logger;
            Mediator = mediator;
            Mapper = mapper;
            Factory = factory;
        }

        /// <summary>
        /// Get all
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetList<TQueryList>([FromQuery] TQueryList request,
            CancellationToken cancellationToken) where TQueryList : IIncludeBlobs
        {
            IEnumerable<TEntity> response = await Mediator.Send(new ListQueryModel<TEntity, TQueryList>
            {
                Request = request
            }, cancellationToken);

            IEnumerable<BlobStorage> blobs = new List<BlobStorage>();

            if (request.IncludeBlobs)
                blobs = await Mediator.Send(new GetBlobsStorageListQueryModel
                {
                    ContainerName = _containerName,
                }, cancellationToken);

            IEnumerable<TDto> mappedResponse = Factory.MapBlob<TEntity, TDto, TBlobDto>(response, blobs);

            return Ok(mappedResponse);
        }

        /// <summary>
        /// Get by id
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetById([FromQuery] TQueryById request,
            CancellationToken cancellationToken)
        {
            TEntity entity = await Mediator.Send(new IdQueryModel<TEntity, TQueryById>
            {
                Request = request
            }, cancellationToken);

            if (entity == null)
            {
                Logger.LogInformation("Not found {Entity} / id: {Id}", entity.GetType(), request.Id);
                return NotFound();
            }

            IEnumerable<BlobStorage> blobs = null;

            if (request.IncludeBlobs)
                blobs = await Mediator.Send(new GetBlobsStorageListQueryModel
                {
                    ContainerName = _containerName,
                    Prefix = entity.Id.ToString("N")
                }, cancellationToken);

            TDto mappedResponse = Factory.MapBlob<TEntity, TDto, TBlobDto>(entity, blobs);

            return Ok(mappedResponse);
        }

        /// <summary>
        /// Create
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status201Created)]
        public virtual async Task<IActionResult> Create<TCreate>([FromBody] TCreate request,
            CancellationToken cancellationToken)
        {
            TEntity entity = await Mediator.Send(new CreateCommandModel<TEntity, TCreate>
            {
                Request = request
            }, cancellationToken);

            await Mediator.Send(new SaveCommandModel<TEntity>(), cancellationToken);

            var mappedResponse = Mapper.Map<TDto>(entity);

            return CreatedAtAction(nameof(GetById), new { id = mappedResponse.Id }, mappedResponse);
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> Update<TUpdate>([FromBody] TUpdate request,
            CancellationToken cancellationToken)
            where TUpdate : IEntity
        {
            TEntity entity = await Mediator.Send(new IdQueryModel<TEntity, TQueryById>
            {
                Request = new TQueryById { Id = request.Id }
            }, cancellationToken);

            if (entity == null)
            {
                Logger.LogInformation("Not found {Entity} / id: {Id}", entity.GetType(), request.Id);
                return NotFound();
            }

            await Mediator.Send(new UpdateCommandModel<TEntity, TUpdate>
            {
                Entity = entity,
                Request = request
            }, cancellationToken);

            await Mediator.Send(new SaveCommandModel<TEntity>(), cancellationToken);

            var mappedResponse = Mapper.Map<TDto>(entity);

            return Ok(mappedResponse);
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> Update<TUpdate>(Guid id, [FromBody] TUpdate request,
            CancellationToken cancellationToken)
            where TUpdate : IEntity
        {
            request.Id = id;

            TEntity entity = await Mediator.Send(new IdQueryModel<TEntity, TQueryById>
            {
                Request = new TQueryById { Id = request.Id }
            }, cancellationToken);

            if (entity == null)
            {
                Logger.LogInformation("Not found {Entity} / id: {Id}", entity.GetType(), request.Id);
                return NotFound();
            }

            await Mediator.Send(new UpdateCommandModel<TEntity, TUpdate>
            {
                Entity = entity,
                Request = request
            }, cancellationToken);

            await Mediator.Send(new SaveCommandModel<TEntity>(), cancellationToken);

            var mappedResponse = Mapper.Map<TDto>(entity);

            return Ok(mappedResponse);
        }

        /// <summary>
        /// Patch
        /// </summary>
        /// <param name="id"></param>
        /// <param name="patchDocument"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> Patch<TPatch>(Guid id,
            [FromBody] JsonPatchDocument<TPatch> patchDocument, CancellationToken cancellationToken)
            where TPatch : class, new()
        {
            var request = new TPatch();
            patchDocument.ApplyTo(request);

            TEntity entity = await Mediator.Send(new IdQueryModel<TEntity, TQueryById>
            {
                Request = new TQueryById { Id = id }
            }, cancellationToken);

            if (entity == null)
            {
                Logger.LogInformation("Not found {Entity} / id: {Id}", entity.GetType(), id);
                return NotFound();
            }

            TEntity response = await Mediator.Send(new PatchCommandModel<TEntity, TPatch>
            {
                Entity = entity,
                Request = request
            }, cancellationToken);

            await Mediator.Send(new SaveCommandModel<TEntity>(), cancellationToken);

            var mappedResponse = Mapper.Map<TDto>(response);

            return Ok(mappedResponse);
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete<TDelete>(Guid id, [FromQuery] TDelete request)
        {
            TEntity entity = await Mediator.Send(new IdQueryModel<TEntity, TQueryById>
            {
                Request = new TQueryById { Id = id, IncludeBlobs = true }
            });

            if (entity == null)
            {
                Logger.LogInformation("Not found {Entity} / id: {Id}", entity.GetType(), id);
                return NotFound();
            }

            await Mediator.Send(new DeleteCommandModel<TEntity, TDelete>
            {
                Entity = entity,
                Request = request
            });

            await Mediator.Send(new SaveCommandModel<TEntity>());

            foreach (TBlobEntity blob in entity.Blobs)
            {
                await Mediator.Send(new DeleteBlobCommandModel
                {
                    ContainerName = blob.ContainerName,
                    Name = blob.Name
                });
            }

            return NoContent();
        }
    }
}