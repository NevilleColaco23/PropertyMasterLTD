using MyWarehouse.Domain.Common;
using Microsoft.EntityFrameworkCore;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Infrastructure.Persistence.Context;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Common.Mapping;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Common;

/// <summary>
/// Generic base repository with implementations of basic operations.
/// Concrete derived repositories should extend it with custom querying requirements for the given entity type.
/// </summary>
internal abstract class RepositoryBaseEf<TEntity, TId> : IRepository<TEntity, TId> where TEntity : class, IEntity<TId>
{
    protected ApplicationDbContext _context;
    protected DbSet<TEntity> _set;
    private readonly IMapper _mapper;

    /// <summary>
    /// Defines the base query for the given entity used by all operations.
    /// Concrete implementations should apply all necessary includes and pre-filters here.
    /// </summary>
    protected abstract IQueryable<TEntity> BaseQuery { get; }

    protected RepositoryBaseEf(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _set = context.Set<TEntity>();
        _mapper = mapper;
    }

    public virtual async Task<TEntity?> GetByIdAsync(TId id)
        => await BaseQuery.SingleOrDefaultAsync(e => Equals(e.Id, id));

    public async Task<IEnumerable<TEntity>> GetFiltered(Expression<Func<TEntity, bool>> filter, bool readOnly = false)
        => await (readOnly ? BaseQuery.AsNoTracking() : BaseQuery).Where(filter).ToListAsync();

    public virtual async Task<TDto?> GetProjectedAsync<TDto>(TId id, bool readOnly = false) where TDto : IMapFrom<TEntity>
        => await (readOnly ? BaseQuery.AsNoTracking() : BaseQuery)
            .Where(x => Equals(x.Id, id))
            .ProjectTo<TDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();

    public virtual void Add(TEntity entity)
        => _set.Add(entity);

    public virtual void AddRange(IEnumerable<TEntity> entities)
        => _set.AddRange(entities);

    public virtual void StartTracking(TEntity detachedEntity)
        => _set.Update(detachedEntity);

    /// <summary>
    /// Removes the entity. Concrete implementations should decide if this is a hard removal or a deletion flag.
    /// </summary>
    public abstract void Remove(TEntity entityToDelete);

    /// <summary>
    /// Removes the entities. Concrete implementations should decide if this is a hard removal or a deletion flag.
    /// </summary>
    public abstract void RemoveRange(IEnumerable<TEntity> entitiesToDelete);

    public virtual async Task<IListResponseModel<TDto>> GetProjectedListAsync<TDto>(ListQueryModel<TDto> model, Expression<Func<TEntity, bool>>? additionalFilter = null, bool readOnly = false) where TDto : IMapFrom<TEntity>
    {
        var query = readOnly ? BaseQuery.AsNoTracking() : BaseQuery;

        if (additionalFilter != null)
        {
            query = query.Where(additionalFilter);
        }

        IQueryable<TDto>? filteredDtoQuery = default;
        try
        {
            filteredDtoQuery = query
                .ProjectTo<TDto>(_mapper.ConfigurationProvider)
                .ApplyFilter(model.Filter);
        }
        catch (FormatException fe)
        {
            model.ThrowFilterIncorrectException(fe.InnerException);
        }

        var totalRowCount = await filteredDtoQuery!.CountAsync();

        IEnumerable<TDto>? resultPage = default;
        try
        {
            resultPage = await filteredDtoQuery!
                .ApplyOrder(model.OrderBy)
                .ApplyPaging(model.PageSize, model.PageIndex)
                .ToListAsync();
        }
        catch (FormatException fe)
        {
            model.ThrowOrderByIncorrectException(fe.InnerException);
        }

        return new ListResponseModel<TDto>(model, totalRowCount, resultPage!);
    }

    public IList<T> GetListBy<T>(string tableName, INamedQuery filterQuery = null)
    {
        throw new NotImplementedException();
    }

    public DataTable GetDataTablePaged(string tableName, INamedQuery filterQuery, DataTable template, int start, int numRecords,
        out int totalCount, string sortField1 = null, bool bAscending = true, string sortField2 = null,
        bool bAscending2 = true) //TODO: check if this can be removed
    {
        throw new NotImplementedException();
    }
}
