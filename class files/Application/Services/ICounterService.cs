namespace MyWarehouse.Application.Services;

public interface ICounterService
{
    public Task<int> GetNextSequenceValue(string entityName);
}