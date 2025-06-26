using System.Data;
using MyWarehouse.Application.Common.Dependencies.DataAccess;

namespace MyWarehouse.Application.Property.PropertyQueries;

public class GetPropertyQueryByUserId : INamedQuery
{
    private readonly int _userId;
    private readonly string _filterString;

    public GetPropertyQueryByUserId(int userId, string filterString)
    {
        _filterString = filterString;
        _userId = userId;
    }

    public string QueryStr => string.IsNullOrEmpty(_filterString) ?
    $"[{{ $match: {{ _id: 1 }}}}" +
        $",{{ $lookup: {{ from: \"Property\", let: {{ propertyIds: \"$PropertyAccessList.PropertyID\" }}, pipeline: [ {{ $match: {{ $expr: {{$and: [{{ $in: [\"$_id\", \"$$propertyIds\"] }},{{ $eq: [\"$Active\", true] }}] }} }} }}" +
        $",{{ $project: {{ Name: 1, Rooms: {{$filter: {{input: \"$Rooms\",as: \"room\",cond: {{ $eq: [\"$$room.Active\", true] }} }} }} }} }}" +
        $", {{ $project: {{ Name: 1, Rooms: 1 }} }} ], as: \"PropertyList\" }} }}" +
        $", {{ $project: {{ PropertyList: 1}} }}]"
    :
    $"[{{ \"$match\": {{ $expr: {_filterString} }} }}]";

    public CommandType CommandType => CommandType.Text;

    public IReadOnlyList<NamedQueryParameter> Parameters => null;
}