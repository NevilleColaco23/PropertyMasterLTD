using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Common.Mapping;
using MyWarehouse.Application.Services;
using MyWarehouse.Domain.Common;

namespace MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Common;

//TODO : use mongodb pipeline across all function
/// <summary>
/// Generic base repository for MongoDB with implementations of basic operations.
/// Concrete derived repositories should extend it with custom querying requirements for the given document type.
/// </summary>
public abstract class RepositoryBaseMongo<TDocument, TId> : IRepository<TDocument, TId> where TDocument : class, IEntity<TId>
{
    protected IMongoCollection<TDocument> Collection;
    private readonly IMapper _mapper;
    private readonly ICounterService _counterService;
    protected IMongoCollection<BsonDocument> BsonCollection;

    protected RepositoryBaseMongo(IMongoDatabase database, IMapper mapper, string collectionName, ICounterService counterService)
    {
        Collection = database.GetCollection<TDocument>(collectionName);
        _mapper = mapper;
        BsonCollection = database.GetCollection<BsonDocument>(collectionName);
        _counterService = counterService;
    }

    // Implement other IRepository<TDocument> methods specifically for MongoDB...
    public Task<TDocument?> GetByIdAsync(TId id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TDocument>> GetFiltered(Expression<Func<TDocument, bool>> filter, bool readOnly = false)
    {
        throw new NotImplementedException();
    }

    public async void Add(TDocument entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        var nextId = await _counterService.GetNextSequenceValue(typeof(TDocument).Name);
        if (typeof(TId) == typeof(int))
        {
            entity.Id = (TId)(object)nextId;
        }
        else
        {
            throw new InvalidOperationException("Unsupported Id type");
        }

        try
        {
            await Collection.InsertOneAsync(entity);
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while adding the document to the collection.", ex);
        }
    }

    public void AddRange(IEnumerable<TDocument> entities)
    {
        throw new NotImplementedException();
    }

    public void Remove(TDocument entity)
    {
        throw new NotImplementedException();
    }

    public void RemoveRange(IEnumerable<TDocument> entities)
    {
        throw new NotImplementedException();
    }

    public void StartTracking(TDocument entity)
    {
        throw new NotImplementedException();
    }

    public Task<TDto?> GetProjectedAsync<TDto>(int id, bool readOnly = false) where TDto : IMapFrom<TDocument>
    {
        throw new NotImplementedException();
    }

    // Example MongoDB-specific implementation
    public virtual async Task<TDto?> GetProjectedAsync<TDto>(TId id, bool readOnly = false) where TDto : IMapFrom<TDocument>
    {
        var filter = Builders<TDocument>.Filter.Eq<TId>(doc => doc.Id, id);
        var document = await Collection.Find(filter).FirstOrDefaultAsync();

        return document != null ? _mapper.Map<TDto>(document) : default;
    }

    public async Task<IListResponseModel<TDto>> GetProjectedListAsync<TDto>(ListQueryModel<TDto> model, Expression<Func<TDocument, bool>>? additionalFilter = null,
        bool readOnly = false) where TDto : IMapFrom<TDocument>
    {
        // Build the filter
        var filter = additionalFilter ?? (doc => true);

        // Apply pagination and sorting from the model
        var query = Collection.Find(filter);

        if (!string.IsNullOrEmpty(model.OrderBy))
        {
            var sortDefinition = Builders<TDocument>.Sort.Ascending(model.OrderBy);
            query = query.Sort(sortDefinition);
        }

        var totalItems = await query.CountDocumentsAsync();

        var items = query
            .Skip((model.PageIndex - 1) * model.PageSize)
            .Limit(model.PageSize)
            .ToListAsync();

        // Project the results to the desired DTO type
        var projectedItems = _mapper.Map<List<TDto>>(await items);

        // Return the results in a IListResponseModel<TDto>
        return new ListResponseModel<TDto>(model, (int)totalItems, projectedItems);
    }

    public IList<T> GetListBy<T>(string tableName, INamedQuery filterQuery = null)
    {
        List<BsonDocument> stages;

        if (filterQuery?.BsonPipeline != null)
        {
            stages = filterQuery.BsonPipeline.Select(p => p.AsBsonDocument).ToList();
        }
        else
        {
            var json = filterQuery?.QueryStr ?? "[ { \"$match\": { } } ]";
            var bsonArray = BsonSerializer.Deserialize<BsonArray>(json);
            stages = bsonArray.Select(p => p.AsBsonDocument).ToList();
        }

        var pipeline = PipelineDefinition<BsonDocument, T>.Create(stages);

        var data = BsonCollection
            .Aggregate(pipeline, new AggregateOptions { AllowDiskUse = true })
            .ToList();

        return data;
    }

    public DataTable GetDataTablePaged(string tableName, INamedQuery filterQuery, DataTable template, int start, int numRecords,
        out int totalCount, string sortField1 = null, bool bAscending = true, string sortField2 = null,
        bool bAscending2 = true)
    {
        var stages = BsonSerializer.Deserialize<BsonArray>(filterQuery?.QueryStr ?? "[ {\"$match\": { }} ]").Select(p => p.AsBsonDocument).ToList();    // Add Empty Filter if no Query String is given

        if (!string.IsNullOrWhiteSpace(sortField1))
            stages.Add(BsonDocument.Parse($"{{ \"$sort\": {{ \"{sortField1}\": {(bAscending ? 1 : -1)}{(!string.IsNullOrWhiteSpace(sortField2) ? ", \"" + sortField2 + "\":" + (bAscending2 ? 1 : -1) : "")} }} }}"));

        stages.Add(BsonDocument.Parse($"{{ $facet: {{ pagedResults: [ {{ $skip: {start} }}, {{ $limit: {numRecords} }}] , Summary: [ {{ $count: \"RowCount\" }} ] }} }}"));
        stages.Add(BsonDocument.Parse("{ \"$unwind\" : { \"path\" : \"$Summary\" } }"));

        var data = BsonCollection
            .Aggregate(PipelineDefinition<BsonDocument, BsonDocument>.Create(stages), new AggregateOptions { AllowDiskUse = true })
            .ToList();

        totalCount = data.Count == 0 ? 0 : data[0].GetValue("Summary").AsBsonDocument.GetValue("RowCount").AsInt32;

        return MakeDataTable(template, data.Count == 0 ? new List<BsonDocument>() : data[0].GetValue("pagedResults").AsBsonArray.Select(p => p.AsBsonDocument).ToList());

    }

    //public T GetItemByIdDelete<T>(string tableName, INamedQuery filterQuery)
    //{
    //    var stages = BsonSerializer.Deserialize<BsonArray>(filterQuery?.QueryStr ?? "[ {\"$match\": { }} ]")
    //    .Select(p => p.AsBsonDocument)
    //    .ToList(); // Add Empty Filter if no Query String is given

    //    var data = BsonCollection
    //        .Aggregate(PipelineDefinition<BsonDocument, T>.Create(stages), new AggregateOptions { AllowDiskUse = true })
    //        .FirstOrDefault();

    //    return data;
    //}

    #region Private Methods
    private static DataTable MakeDataTable(DataTable templateTable, List<BsonDocument> data)
    {
        DataTable resultTable = templateTable ?? new DataTable();
        foreach (var item in data)
        {
            DataRow dr = resultTable.NewRow();
            foreach (BsonElement element in item.Elements)
            {
                var colExists = resultTable.Columns.Contains(element.Name);

                if (templateTable == null && !colExists)    // We created the DataTable and column doesn't exists -> Create column and set value
                {
                    resultTable.Columns.Add(new DataColumn(element.Name));
                    dr[element.Name] = element.Value == BsonNull.Value ? DBNull.Value : element.Value;
                }
                else if (colExists)     // We were given the DataTable, set the value if the column exists
                {
                    dr[element.Name] = element.Value == BsonNull.Value ? DBNull.Value : element.Value;
                }
            }

            resultTable.Rows.Add(dr);
        }

        return resultTable;
    }

    #endregion

}