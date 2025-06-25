using System.Data;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;

namespace MongoDBBackend
{
    // The main Mongo Repository with full functionality, use one repository per table / collection.
    class MongoRepository : IMongoRepository
    {
        private readonly string _tableName;
        private readonly MongoContext _mongoContext;

        internal MongoRepository(IPersistenceContext context, string tableName = null)
        {
            _tableName = tableName;
            _mongoContext = (MongoContext)context;
        }


        #region Get data by Specifications
        public T GetItemBy<T>(Expression<Func<T, bool>> specification)
        {
            return _mongoContext.GetCollection<T>(_tableName).Find(specification).FirstOrDefault();
        }

        public T GetItemBy<T>(Expression<Func<T, bool>> specification, Expression<Func<T, object>> sortSelector, bool bAscending = true, Expression<Func<T, object>> sortSelector2 = null, bool bAscending2 = true)
        {
            var result = _mongoContext.GetCollection<T>(_tableName).Find(specification);

            var sorts = bAscending ? Builders<T>.Sort.Ascending(sortSelector) : Builders<T>.Sort.Descending(sortSelector);

            if (sortSelector2 != null)
                sorts = bAscending2 ? sorts.Ascending(sortSelector2) : sorts.Descending(sortSelector2);

            return result.Sort(sorts).FirstOrDefault();
        }

        public T GetItemById<T, TId>(TId id)
        {
            return _mongoContext.GetCollection<T>(_tableName).Find(Builders<T>.Filter.Eq("_id", id)).FirstOrDefault();
        }

        public IList<T> GetItemsByIds<T, TId>(IEnumerable<TId> ids)
        {
            BsonDocument filter = new() { { "_id", new BsonDocument().Add("$in", new BsonArray(ids)) } };
            return _mongoContext.GetCollection<T>(_tableName).Find(filter).ToList();
        }

        public IList<T> GetListAll<T>(bool includeDeleted = false)
        {
            return _mongoContext.GetCollection<T>(_tableName).Find(Builders<T>.Filter.Empty).ToList();
        }

        public IList<T> GetListBy<T>(Expression<Func<T, bool>> specification)
        {
            return _mongoContext.GetCollection<T>(_tableName).AsQueryable().Where(specification.Compile()).ToList();
        }

        public IList<T> GetSortedListBy<T>(Expression<Func<T, bool>> specification, Expression<Func<T, object>> sortSelector, bool bAscending = true,
            Expression<Func<T, object>> sortSelector2 = null, bool bAscending2 = true)
        {
            var result = _mongoContext.GetCollection<T>(_tableName).Find(specification);
            var sorts = bAscending ? Builders<T>.Sort.Ascending(sortSelector) : Builders<T>.Sort.Descending(sortSelector);

            if (sortSelector2 != null)
                sorts = bAscending2 ? sorts.Ascending(sortSelector2) : sorts.Descending(sortSelector2);

            return result.Sort(sorts).ToList();
        }

        public IList<T> GetListPagedBy<T>(Expression<Func<T, bool>> specification, int start, int numRecords)
        {
            return _mongoContext.GetCollection<T>(_tableName).Find(specification).Skip(start).Limit(numRecords).ToList();
        }

        public IList<T> GetListPagedBy<T>(Expression<Func<T, bool>> specification, int start, int numRecords, out int rowCount)
        {
            var filter = _mongoContext.GetCollection<T>(_tableName).Find(specification);
            rowCount = (int)filter.CountDocuments();
            return filter.Skip(start).Limit(numRecords).ToList();
        }

        public IList<T> GetListPagedBy<T>(Expression<Func<T, bool>> specification, int start, int numRecords, out int rowCount, Expression<Func<T, object>> sortSelector, bool bAscending = true,
            Expression<Func<T, object>> sortSelector2 = null, bool bAscending2 = true)
        {
            var result = _mongoContext.GetCollection<T>(_tableName).Find(specification);
            rowCount = (int)result.CountDocuments();
            var sorts = bAscending ? Builders<T>.Sort.Ascending(sortSelector) : Builders<T>.Sort.Descending(sortSelector);

            if (sortSelector2 != null)
                sorts = bAscending2 ? sorts.Ascending(sortSelector2) : sorts.Descending(sortSelector2);

            return result.Sort(sorts).Skip(start).Limit(numRecords).ToList();
        }

        public IList<T> GetListPagedBy<T>(Expression<Func<T, bool>> specification, int start, int numRecords, Expression<Func<T, object>> sortSelector, bool bAscending = true,
            Expression<Func<T, object>> sortSelector2 = null, bool bAscending2 = true)
        {
            var result = _mongoContext.GetCollection<T>(_tableName).Find(specification);
            var sorts = bAscending ? Builders<T>.Sort.Ascending(sortSelector) : Builders<T>.Sort.Descending(sortSelector);

            if (sortSelector2 != null)
                sorts = bAscending2 ? sorts.Ascending(sortSelector2) : sorts.Descending(sortSelector2);

            return result.Sort(sorts).Skip(start).Limit(numRecords).ToList();
        }
        #endregion

        #region  Get data by INamedQuery

        public IList<T> GetListBy<T>(string tableName, INamedQueryMongo filterQuery = null)
        {
            var stages = BsonSerializer.Deserialize<BsonArray>(filterQuery?.QueryStr ?? "[ {\"$match\": { }} ]").Select(p => p.AsBsonDocument).ToList();    // Add Empty Filter if no Query String is given

            var data = _mongoContext.GetCollection<BsonDocument>(tableName)
                .Aggregate(PipelineDefinition<BsonDocument, T>.Create(stages), new AggregateOptions { AllowDiskUse = true })
                .ToList();

            return data.ToList();
        }

        public IList<T> GetListByFacet<T>(string tableName, INamedQueryMongo filterQuery, INamedQueryMongo summaryQuery, int start, int numRecords, out Dictionary<string, object> summary,
            string sortField1 = null, bool bAscending = true, string sortField2 = null, bool bAscending2 = true)
        {
            var stages = BsonSerializer.Deserialize<BsonArray>(filterQuery?.QueryStr ?? "[ {\"$match\": { }} ]").Select(p => p.AsBsonDocument).ToList();    // Add Empty Filter if no Query String is given

            if (!string.IsNullOrWhiteSpace(sortField1))
                stages.Add(BsonDocument.Parse($"{{ \"$sort\": {{ \"{sortField1}\": {(bAscending ? 1 : -1)}{(!string.IsNullOrWhiteSpace(sortField2) ? ", \"" + sortField2 + "\":" + (bAscending2 ? 1 : -1) : "")} }} }}"));

            var summaryQueries = summaryQuery?.QueryStr ?? "[ { $count: \"RowCount\" } ]";    // Default Summary is RowCount

            stages.Add(BsonDocument.Parse($"{{ $facet: {{ Data: [ {{ $skip: {start} }}, {{ $limit: {numRecords} }}] , Summary: {summaryQueries} }} }}"));
            stages.Add(BsonDocument.Parse("{ \"$unwind\" : { \"path\" : \"$Summary\" } }"));

            var results = _mongoContext.GetCollection<BsonDocument>(tableName)
                .Aggregate(PipelineDefinition<BsonDocument, FacetResult<T>>.Create(stages), new AggregateOptions { AllowDiskUse = true })
                .FirstOrDefault();

            summary = results?.Summary.ToDictionary() ?? new Dictionary<string, object>();
            return results?.Data ?? new List<T>();
        }
        #endregion

        #region Get data as IQueryable
        public IQueryable<T> GetQueryable<T>()
        {
            return _mongoContext.GetCollection<T>(_tableName).AsQueryable();
        }

        public IQueryable<T> GetQueryable<T>(Expression<Func<T, bool>> specification)
        {
            return _mongoContext.GetCollection<T>(_tableName).AsQueryable().Where(specification);
        }
        #endregion 

        #region Get data as DataTable / DataSet

        public DataTable GetDataTableUnion(string tableName, List<INamedQueryMongo> queries)
        {
            List<BsonDocument> stages = BsonSerializer.Deserialize<BsonArray>(queries[0].QueryStr).Select(p => p.AsBsonDocument).ToList();

            for (int i = 1; i < queries.Count; i++)
            {
                var unionStage = $"{{ $unionWith: {{ coll: \"{tableName}\", pipeline:  {queries[i].QueryStr} }} }}";
                stages.Add(BsonDocument.Parse(unionStage));
            }

            var data = _mongoContext.GetCollection<BsonDocument>(tableName)
                .Aggregate(PipelineDefinition<BsonDocument, BsonDocument>.Create(stages), new AggregateOptions { AllowDiskUse = true })
                .ToList();

            return MakeDataTable(null, data);
        }

        public DataSet GetDataSetByFacet(string tableName, List<INamedQueryMongo> queries)
        {
            List<BsonDocument> stages = BsonSerializer.Deserialize<BsonArray>(queries[0].QueryStr).Select(p => p.AsBsonDocument).ToList();

            for (int i = 1; i < queries.Count; i++)
            {
                var unionStage = $"{{ $unionWith: {{ coll: \"{tableName}\", pipeline:  {queries[i].QueryStr} }} }}";
                stages.Add(BsonDocument.Parse(unionStage));
            }

            var data = _mongoContext.GetCollection<BsonDocument>(tableName)
                .Aggregate(PipelineDefinition<BsonDocument, BsonDocument>.Create(stages), new AggregateOptions { AllowDiskUse = true })
                .ToList();

            return MakeDataSet(null, data);
        }

        // Caution: Passing null query will load the whole table
        public DataTable GetDataTable(string tableName, INamedQueryMongo filterQuery, DataTable template = null, string sortField1 = null,
            bool bAscending = true, string sortField2 = null, bool bAscending2 = true)
        {
            var stages = BsonSerializer.Deserialize<BsonArray>(filterQuery?.QueryStr ?? "[ {\"$match\": { }} ]").Select(p => p.AsBsonDocument).ToList();    // Add Empty Filter if no Query String is given

            if (!string.IsNullOrWhiteSpace(sortField1))
                stages.Add(BsonDocument.Parse($"{{ \"$sort\": {{ \"{sortField1}\": {(bAscending ? 1 : -1)}{(!string.IsNullOrWhiteSpace(sortField2) ? ", \"" + sortField2 + "\":" + (bAscending2 ? 1 : -1) : "")} }} }}"));

            var data = _mongoContext.GetCollection<BsonDocument>(tableName)
                .Aggregate(PipelineDefinition<BsonDocument, BsonDocument>.Create(stages), new AggregateOptions { AllowDiskUse = true })
                .ToList();

            return MakeDataTable(template, data);
        }

        // Caution: Passing null query will load the whole table
        public DataSet GetDataSet(string tableName, INamedQueryMongo filterQuery, DataTable template = null)
        {
            var stages = BsonSerializer.Deserialize<BsonArray>(filterQuery?.QueryStr ?? "[ {\"$match\": { }} ]").Select(p => p.AsBsonDocument).ToList();    // Add Empty Filter if no Query String is given

            var data = _mongoContext.GetCollection<BsonDocument>(tableName)
                .Aggregate(PipelineDefinition<BsonDocument, BsonDocument>.Create(stages), new AggregateOptions { AllowDiskUse = true })
                .ToList();

            return MakeDataSet(template, data);
        }

        public DataTable GetDataTablePaged(string tableName, INamedQueryMongo filterQuery, DataTable template, int start, int numRecords, out int totalCount,
            string sortField1 = null, bool bAscending = true, string sortField2 = null, bool bAscending2 = true)
        {
            var stages = BsonSerializer.Deserialize<BsonArray>(filterQuery?.QueryStr ?? "[ {\"$match\": { }} ]").Select(p => p.AsBsonDocument).ToList();    // Add Empty Filter if no Query String is given

            if (!string.IsNullOrWhiteSpace(sortField1))
                stages.Add(BsonDocument.Parse($"{{ \"$sort\": {{ \"{sortField1}\": {(bAscending ? 1 : -1)}{(!string.IsNullOrWhiteSpace(sortField2) ? ", \"" + sortField2 + "\":" + (bAscending2 ? 1 : -1) : "")} }} }}"));

            stages.Add(BsonDocument.Parse($"{{ $facet: {{ pagedResults: [ {{ $skip: {start} }}, {{ $limit: {numRecords} }}] , Summary: [ {{ $count: \"RowCount\" }} ] }} }}"));
            stages.Add(BsonDocument.Parse("{ \"$unwind\" : { \"path\" : \"$Summary\" } }"));

            var data = _mongoContext.GetCollection<BsonDocument>(tableName)
                            .Aggregate(PipelineDefinition<BsonDocument, BsonDocument>.Create(stages), new AggregateOptions { AllowDiskUse = true })
                            .ToList();

            totalCount = data.Count == 0 ? 0 : data[0].GetValue("Summary").AsBsonDocument.GetValue("RowCount").AsInt32;

            return MakeDataTable(template, data.Count == 0 ? new List<BsonDocument>() : data[0].GetValue("pagedResults").AsBsonArray.Select(p => p.AsBsonDocument).ToList());
        }
        #endregion

        #region Get data as Dictionary

        public List<Dictionary<string, object>> GetDictionary(string tableName, INamedQueryMongo filterQuery = null,
            string sortField1 = null, bool bAscending = true, string sortField2 = null, bool bAscending2 = true)    // Sorting is optional
        {
            var collection = _mongoContext.GetCollection<BsonDocument>(tableName);

            var data = filterQuery == null
                ? collection.Find(Builders<BsonDocument>.Filter.Empty)
                : collection.Find(filterQuery.QueryStr);

            if (!string.IsNullOrWhiteSpace(sortField1))
                data = data.Sort("{ \"" + sortField1 + "\" : " + (bAscending ? 1 : -1) + (!string.IsNullOrWhiteSpace(sortField2) ? ", \"" + sortField2 + "\" : " + (bAscending2 ? 1 : -1) : "") + " }");


            return data.ToList().Select(x => x.Elements.ToDictionary(y => y.Name, y => y.Value == BsonNull.Value ? (object)null : y.Value)).ToList();
        }

        public List<Dictionary<string, object>> GetDictionaryByAggregate(string tableName, INamedQueryMongo filterQuery = null)
        {
            var stages = BsonSerializer.Deserialize<BsonArray>(filterQuery?.QueryStr ?? "[ {\"$match\": { }} ]").Select(p => p.AsBsonDocument).ToList();    // Add Empty Filter if no Query String is given

            var data = _mongoContext.GetCollection<BsonDocument>(tableName)
                .Aggregate(PipelineDefinition<BsonDocument, BsonDocument>.Create(stages), new AggregateOptions { AllowDiskUse = true })
                .ToList();

            data = ConvertDateFieldsInBsonDocumentForRead(data);

            return data.ToList().Select(x => x.Elements.ToDictionary(y => y.Name, y => y.Value == BsonNull.Value ? (object)null : y.Value)).ToList();
        }

        public List<Dictionary<string, object>> GetDictionaryPaged(string tableName, INamedQueryMongo filterQuery, int start, int numRecords, out int totalRecords,
            string sortField1 = null, bool bAscending = true, string sortField2 = null, bool bAscending2 = true)    // Sorting is optional
        {
            var collection = _mongoContext.GetCollection<BsonDocument>(tableName);

            var data = filterQuery == null
                ? collection.Find(Builders<BsonDocument>.Filter.Empty)
                : collection.Find(filterQuery.QueryStr);

            if (!string.IsNullOrWhiteSpace(sortField1))
                data = data.Sort("{ \"" + sortField1 + "\" : " + (bAscending ? 1 : -1) + (!string.IsNullOrWhiteSpace(sortField2) ? ", \"" + sortField2 + "\" : " + (bAscending2 ? 1 : -1) : "") + " }");

            totalRecords = 0;

            return data.Skip(start).Limit(numRecords).ToList()
                .Select(x => x.Elements.ToDictionary(y => y.Name, y => y.Value == BsonNull.Value ? (object)null : y.Value)).ToList();
        }
        #endregion

        #region Utility methods
        public List<TF> GetDistinct<TF>(string tableName, INamedQueryMongo filterQuery, string fieldName)
        {
            var collection = _mongoContext.GetCollection<BsonDocument>(tableName);
            var filter = filterQuery != null ? new JsonFilterDefinition<BsonDocument>(filterQuery.QueryStr) : new JsonFilterDefinition<BsonDocument>("{}");
            var result = collection.Distinct<TF>(fieldName, filter).ToList();
            return result;
        }

        public int GetCount<T>(bool includeDeleted = false)
        {
            return (int)_mongoContext.GetCollection<T>(_tableName).CountDocuments(FilterDefinition<T>.Empty);
        }

        public int GetCountBy<T>(Expression<Func<T, bool>> specification)
        {
            return (int)_mongoContext.GetCollection<T>(_tableName).CountDocuments(specification);
        }

        // Use this to just execute queries when return data is not expected
        public void ExecuteQuery(string tableName, INamedQueryMongo aggregateQuery)
        {
            var stages = BsonSerializer.Deserialize<BsonArray>(aggregateQuery?.QueryStr ?? "[ {\"$match\": { }} ]").Select(p => p.AsBsonDocument).ToList();    // Add Empty Filter if no Query String is given

            _mongoContext.GetCollection<BsonDocument>(tableName)
                .Aggregate(PipelineDefinition<BsonDocument, BsonDocument>.Create(stages), new AggregateOptions { AllowDiskUse = true });
        }

        public void DropCollection(string tableName)
        {
            _mongoContext.GetDatabase().DropCollection(tableName);
        }

        #endregion

        // Methods for Data Save / Update / Delete
        #region Data Save / Update / Delete

        // Caution: Do not use this method to save data table. T should have a property 'Id' of type string.
        public string Save<T>(T item)
        {
            dynamic dynamicItem = item;
            if (dynamicItem.Id == null)
                dynamicItem.Id = ObjectId.GenerateNewId().ToString();

            var result = _mongoContext.GetCollection<T>(_tableName).ReplaceOne(new BsonDocument("_id", dynamicItem.Id), item, new ReplaceOptions { IsUpsert = true });

            return result.IsAcknowledged ? result.UpsertedId?.ToString() : null;
        }

        public void Save<T>(string tableName, IEnumerable<T> data, bool merge)
        {
            if (merge)  // Keeps existing fields
            {
                var json = JsonConvert.SerializeObject(data, new JsonSerializerSettings { DateFormatHandling = DateFormatHandling.MicrosoftDateFormat })
                                .Replace("\"Id\":", "\"_id\":");

                var docList = BsonSerializer.Deserialize<List<BsonDocument>>(json).Select(ConvertDateFieldsInBsonDocument);

                SaveInternal(tableName, docList, true);
            }
            else    // Replace the documents with the new ones
            {
                var collection = _mongoContext.GetCollection<T>(tableName);
                var bulkOps = new List<WriteModel<T>>();

                foreach (var record in data)
                {
                    dynamic dynamicItem = record;
                    var upsertOne = new ReplaceOneModel<T>(Builders<T>.Filter.Eq("_id", dynamicItem.Id), record) { IsUpsert = true };

                    bulkOps.Add(upsertOne);
                }

                collection.BulkWrite(bulkOps);
            }
        }

        // Saves multiple records, inserts if not found, will merge or replace as per the merge flag
        public void Save(DataTable dt, bool merge, bool insertNullFields = false)
        {
            var columnNames = dt.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList();

            var bsonList = from DataRow row in dt.Rows
                           select insertNullFields
                               ? columnNames.ToDictionary(x => x, x => row[x] == DBNull.Value ? null : row[x])
                               : columnNames.Where(x => row[x] != DBNull.Value).ToDictionary(x => x, x => row[x])
                into dictionary
                           select new BsonDocument(dictionary);

            SaveInternal(dt.TableName, bsonList, merge);
        }

        // Saves multiple documents, inserts if not found, will merge or replace as per the merge flag
        public void Save(string tableName, List<Dictionary<string, object>> data, bool merge)
        {
            var bsonList = data.Select(item => item.Keys.ToDictionary(key => key, key => item[key] == DBNull.Value ? BsonNull.Value : item[key]))
                .Select(dictItem => new BsonDocument(dictItem));

            SaveInternal(tableName, bsonList, merge);
        }

        // Saves one record, specified by recordId, inserts if not found, will merge or replace as per the merge flag
        public void Save(string tableName, string recordId, Dictionary<string, object> data, bool merge)
        {
            var collection = _mongoContext.GetCollection<BsonDocument>(tableName);

            var json = JsonConvert.SerializeObject(data);
            var bsonDocument = BsonSerializer.Deserialize<BsonDocument>(json);

            bsonDocument = ConvertDateFieldsInBsonDocument(bsonDocument);

            if (merge)
            {
                var updateDefinition = bsonDocument.Select(item => Builders<BsonDocument>.Update.Set(item.Name, item.Value));

                var combinedUpdate = Builders<BsonDocument>.Update.Combine(updateDefinition);
                collection.UpdateOne(Builders<BsonDocument>.Filter.Eq("_id", recordId), combinedUpdate, new UpdateOptions { IsUpsert = true });
            }
            else
            {
                collection.ReplaceOne(Builders<BsonDocument>.Filter.Eq("_id", recordId), bsonDocument, new ReplaceOptions { IsUpsert = true });
            }
        }

        // Bulk Inserts new records, ensure the records have unique Ids
        public void BulkInsert(DataTable dt, bool insertNullFields = false)
        {
            var columnNames = dt.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList();

            var bsonList = from DataRow row in dt.Rows
                           select insertNullFields
                               ? columnNames.ToDictionary(x => x, x => row[x] == DBNull.Value ? null : row[x])
                               : columnNames.Where(x => row[x] != DBNull.Value).ToDictionary(x => x, x => row[x])
                into dictionary
                           select new BsonDocument(dictionary);

            var collection = _mongoContext.GetCollection<BsonDocument>(dt.TableName);
            collection.InsertMany(bsonList);
        }

        public void Update(string tableName, INamedQueryMongo filterQuery)
        {
            var collection = _mongoContext.GetCollection<BsonDocument>(tableName);
            var splitQuery = filterQuery.QueryStr.Split(';');
            collection.UpdateMany(splitQuery[0], splitQuery[1]);
        }

        public void Delete<T>(T item)
        {
            ObjectFilterDefinition<T> filter = new ObjectFilterDefinition<T>(item);
            _mongoContext.GetCollection<T>(_tableName).DeleteOne(filter);
        }

        public int DeleteBy<T>(Expression<Func<T, bool>> specification)
        {
            var result = _mongoContext.GetCollection<T>(_tableName).DeleteMany(specification);
            return (int)result.DeletedCount;
        }

        public void DeleteById<T, TId>(TId id)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            _mongoContext.GetCollection<T>(_tableName).DeleteOne(filter);
        }

        public long DeleteMany(string tableName, INamedQueryMongo namedQuery)
        {
            return _mongoContext.GetCollection<BsonDocument>(tableName).DeleteMany(namedQuery.QueryStr).DeletedCount;
        }

        #endregion

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

        private static DataSet MakeDataSet(DataTable templateTable, IEnumerable<BsonDocument> data)
        {
            DataSet ds = new();
            DataTable resultTable = templateTable ?? new DataTable();
            resultTable.TableName = "Base";

            foreach (var item in data)
            {
                DataRow dr = resultTable.NewRow();
                foreach (BsonElement element in item.Elements)
                {
                    var colExists = resultTable.Columns.Contains(element.Name);

                    if (element.Value.IsBsonArray)
                    {
                        var childTableExists = ds.Tables.Contains(element.Name);
                        var childTable = MakeDataTable(childTableExists ? ds.Tables[element.Name] : null,
                            element.Value.AsBsonArray.Select(x => x as BsonDocument).ToList());

                        if (!childTableExists)
                        {
                            childTable.TableName = element.Name;
                            ds.Tables.Add(childTable);
                        }
                    }

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

            ds.Tables.Add(resultTable);

            return ds;
        }

        private void SaveInternal(string tableName, IEnumerable<BsonDocument> data, bool merge)
        {
            var collection = _mongoContext.GetCollection<BsonDocument>(tableName);
            var bulkOps = new List<WriteModel<BsonDocument>>();

            if (merge)  // Keeps existing fields
            {
                foreach (var doc in data)
                {
                    var updateDefinitions = doc.Elements.Select(x => Builders<BsonDocument>.Update.Set(x.Name, x.Value));
                    var combinedUpdate = Builders<BsonDocument>.Update.Combine(updateDefinitions);
                    bulkOps.Add(new UpdateOneModel<BsonDocument>(Builders<BsonDocument>.Filter.Eq("_id", doc["_id"]), combinedUpdate) { IsUpsert = true });
                }
            }
            else    // Replace the documents with the new ones
            {
                bulkOps.AddRange(data.Select(doc => new ReplaceOneModel<BsonDocument>(Builders<BsonDocument>.Filter.Eq("_id", doc["_id"]), doc) { IsUpsert = true }));
            }

            collection.BulkWrite(bulkOps);
        }

        private static BsonDocument ConvertDateFieldsInBsonDocument(BsonDocument bsonDocument)
        {
            var rootDocument = new BsonDocument(bsonDocument);
            return HandleBsonDocument(rootDocument);
        }

        private static BsonDocument HandleBsonDocument(BsonDocument bsonDocument)
        {
            var copyDocument = new BsonDocument(bsonDocument);
            foreach (var element in bsonDocument.Elements)
            {
                if (element.Value == null)
                    continue;

                if (element.Value.IsBsonArray)
                {
                    var subDocumentArray = new BsonArray();
                    foreach (var subElement in element.Value.AsBsonArray)
                    {
                        subDocumentArray.Add(HandleBsonDocument(subElement.ToBsonDocument()));
                    }

                    copyDocument[element.Name] = subDocumentArray;
                }
                else if (element.Value.IsBsonDocument)
                {
                    var document = HandleBsonDocument(element.Value.ToBsonDocument());
                    copyDocument[element.Name] = document;
                }
                else if (element.Value.ToString()?.StartsWith("/Date(") == true)
                {
                    copyDocument[element.Name] = JsonConvert.DeserializeObject<DateTime>(JsonConvert.SerializeObject(element.Value));
                }
            }

            return copyDocument;
        }


        private static List<BsonDocument> ConvertDateFieldsInBsonDocumentForRead(List<BsonDocument> bsonDocument)
        {
            List<BsonDocument> cleanedDocuments = bsonDocument.Select(HandleBsonDocumentForIsoDate).ToList();

            return cleanedDocuments;
        }

        private static BsonDocument HandleBsonDocumentForIsoDate(BsonDocument bsonDocument)
        {
            var copyDocument = new BsonDocument(bsonDocument);
            foreach (var element in bsonDocument.Elements)
            {
                if (element.Value == null)
                    continue;

                if (element.Value.IsBsonArray)
                {
                    var subDocumentArray = new BsonArray();
                    foreach (var subElement in element.Value.AsBsonArray)
                    {
                        subDocumentArray.Add(HandleBsonDocumentForIsoDate(subElement.ToBsonDocument()));
                    }

                    copyDocument[element.Name] = subDocumentArray;
                }
                else if (element.Value.IsBsonDocument)
                {
                    var document = HandleBsonDocumentForIsoDate(element.Value.ToBsonDocument());
                    copyDocument[element.Name] = document;
                }
                else if (element.Value.IsBsonDateTime)
                {
                    copyDocument[element.Name] = element.Value.ToString();
                }
            }

            return copyDocument;
        }
        #endregion
    }


    internal class FacetResult<T>
    {
        public IList<T> Data { get; set; }

        public BsonDocument Summary { get; set; }
    }

}
