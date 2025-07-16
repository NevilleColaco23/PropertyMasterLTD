namespace MyWarehouse.Application.Common.Menus
{
    public class GetMenuPermissionMappingListDTO
    {
        public int Id { get; set; }
        public string label { get; set; }
        public int order { get; set; }
        public string accessLevel { get; set; }
        public bool hasDropdown { get; set; }
        public bool isVisible { get; set; }
        public int MenuID { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
