using MongoDB.Driver;
using MyWarehouse.Application.Common.Mapping;
using MyWarehouse.Domain.Common;
using System.Data;
using System.Linq.Expressions;

namespace MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;

public interface IRepository<TEntity, in TId> where TEntity : IEntity<TId>
{
    /// <summary>
    /// Returns the entity corresponding to the given ID, or default if not found.
    /// </summary>
    Task<TEntity?> GetByIdAsync(TId id);

    Task<IEnumerable<TEntity>> GetFiltered(Expression<Func<TEntity, bool>> filter, bool readOnly = false);

    void Add(TEntity entity);

    void Remove(TEntity entity);

    /// <summary>
    /// Finds the entity of the given Id, and returns it mapped to the specified mappable type, or returns default if not found.
    /// </summary>
    Task<TDto?> GetProjectedAsync<TDto>(TId id, bool readOnly = false) where TDto : IMapFrom<TEntity>;

    /// <summary>
    /// Finds the list of entities corresponding to the provided query, and returns them mapped to the specified mappable type.
    /// </summary>
    Task<IListResponseModel<TDto>> GetProjectedListAsync<TDto>(ListQueryModel<TDto> model, Expression<Func<TEntity, bool>>? additionalFilter = null, bool readOnly = false) where TDto : IMapFrom<TEntity>;



    #region  Get data by INamedQuery

    DataTable GetDataTablePaged(string tableName, INamedQuery filterQuery, DataTable template, int start,
        int numRecords, out int totalCount,
        string sortField1 = null, bool bAscending = true, string sortField2 = null, bool bAscending2 = true);

    IList<T> GetListBy<T>(string tableName, INamedQuery filterQuery = null);

    #endregion
}
