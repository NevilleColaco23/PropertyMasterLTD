using MyWarehouse.Application.Common.Dependencies.DataAccess;
using System.Data;

namespace MyWarehouse.Application.Common.Menus.MenuQueries
{
    internal class GetMenuListQueryByUserId : INamedQuery
    {
        private readonly int _userId;

        public GetMenuListQueryByUserId(int userId)
        {
            _userId = userId;
        }

        public string QueryStr =>
        $"[{{ $match: {{ }} }}]";

        public CommandType CommandType => CommandType.Text;

        public IReadOnlyList<NamedQueryParameter> Parameters => null;
    }
}