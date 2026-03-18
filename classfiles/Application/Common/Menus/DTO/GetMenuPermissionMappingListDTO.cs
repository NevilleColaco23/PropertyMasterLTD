namespace MyWarehouse.Application.Common.Menus.DTO
{
    public class GetMenuPermissionMappingListDTO
    {
        public int Id { get; set; }
        public string label { get; set; }
        public string path { get; set; }
        public int order { get; set; }
        public string accessLevel { get; set; }
        public bool hasDropdown { get; set; }
        public List<SubMenuItemDTO> subItems { get; set; }
        public bool isVisible { get; set; }
        public int MenuID { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public MyWarehouse.Domain.Property.Property property { get; set; }
    }

    public class SubMenuItemDTO
    {
        public string subLabel { get; set; }
        public string subPath { get; set; }
        public int subOrder { get; set; } = 0; // Priority for sorting sub-items
    }
}
