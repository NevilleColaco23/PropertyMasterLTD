namespace MyWarehouse.Application.Common.Menus
{
    public class GetMenuListDTO
    {
        public int Id { get; set; }
        public string MenuName { get; set; }
        public int ParentMenuId { get; set; }
        public bool isActive { get; set; }
    }
}
