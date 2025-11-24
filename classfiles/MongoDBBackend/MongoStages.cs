namespace MongoDBBackend
{
    public static class MongoStages
    {
        public const string MATCH = "$match";
        public const string PROJECT = "$project";
        public const string UNWIND = "$unwind";
        public const string LOOKUP = "$lookup";
        public const string FILTER = "$filter";
        public const string LET = "let";
        public const string PIPELINE = "pipeline";
        public const string FROM = "from";
        public const string AS = "as";
        public const string EXPR = "$expr";
        public const string AND = "$and";
        public const string IN = "$in";
        public const string EQ = "$eq";
        public const string SORT = "$sort";
        public const string REPLACEROOT = "$replaceRoot";
        public const string MERGEOBJECTS = "$mergeObjects";
        public const string ADDFIELDS = "$addFields";
    }

}
