namespace ParkingManagement.FE.Models
{
    public class NotificationCenterViewModel
    {
        public List<NotificationItemViewModel> Items { get; set; } = new();
        public int Count => Items.Count;
        public bool HasItems => Items.Count > 0;
    }

    public class NotificationItemViewModel
    {
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string? Url { get; set; }
        public string Icon { get; set; } = "fa-solid fa-circle-info";
        public string Type { get; set; } = "info";
        public string TimeText { get; set; } = "Vừa cập nhật";
    }
}
