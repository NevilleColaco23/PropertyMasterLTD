namespace MongoDBBackend
{
    /// <summary>
    /// Root class for all our objects (Entities, Repositories, Factories, etc.) 
    /// </summary>
    public abstract class ObjectBase
    {
        protected bool CheckAllValuesEmpty(params string[] values)
        {
            return values == null || values.All(string.IsNullOrWhiteSpace);
        }
    }

    /// <summary>
    /// Base for all Model Entity classes. 
    /// </summary>
    public abstract class EntityBase : ObjectBase
    {
        protected string GenerateUniqueNumber()
        {
            return DateTime.Now.Ticks.ToString().Remove(0, 9);
        }
    }

    /// <summary>
    /// Base for all our Factory classes. 
    /// </summary>
    public abstract class FactoryBase : ObjectBase
    {

    }

    /// <summary>
    /// Base for all our Process classes. 
    /// </summary>
    public abstract class ProcessBase : ObjectBase
    {

    }

    /// <summary>
    /// Base class for all our Constraint classes
    /// </summary>
    public abstract class ConstraintBase : ObjectBase
    {
        public void CheckIsNotNull(object obj, string name)
        {
            if (obj == null)
                throw new AppException($"{name} is needed.");
        }

        public void CheckIsNotNull(string obj, string name)
        {
            if (string.IsNullOrWhiteSpace(obj))
                throw new AppException($"{name} is needed.");
        }
    }
}
