using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDBBackend;

public class CounterService
{
    // Represents the collection in the MongoDB database
    private readonly IMongoCollection<BsonDocument> countersCollection;

    // Constructor that initializes the CounterService with the provided database
    public CounterService(IMongoDatabase database)
    {
        // Getting the "counters" collection from the specified database
        countersCollection = database.GetCollection<BsonDocument>("counters");
    }

    // Method to get the next sequence value for a specified collection
    public async Task<int> GetNextSequenceValue(string collectionName)
    {
        // Starting a session to group multiple operations as a transaction
        var session = await countersCollection.Database.Client.StartSessionAsync();
        try
        {
            // Starting a transaction for the session
            session.StartTransaction();

            // Creating a filter to find the document with the specified collection name
            var filter = Builders<BsonDocument>.Filter.Eq("_id", collectionName);

            // Creating an update definition to increment the "seq" field by 1
            var update = Builders<BsonDocument>.Update.Inc("seq", 1);

            // Setting options for the FindOneAndUpdate operation
            var options = new FindOneAndUpdateOptions<BsonDocument>
            {
                IsUpsert = true, // If no document matches the filter, a new one will be created
                ReturnDocument = ReturnDocument.After // Returns the updated document
            };

            // Finding and updating the document with the provided filter and update
            var result = await countersCollection.FindOneAndUpdateAsync(session, filter, update, options);

            // Committing the transaction if all the operations are successful
            await session.CommitTransactionAsync();

            // Returning the value of the "seq" field from the updated document
            return result["seq"].AsInt32;
        }
        catch (Exception ex)
        {
            // Aborting the transaction if an error occurs
            await session.AbortTransactionAsync();

            // Rethrowing the exception to be handled by the calling code
            throw ex;
        }
        finally
        {
            // Disposing of the session to release associated resources
            session.Dispose();
        }
    }
}