namespace OrderService.DTOs
{
    public class OrderWithUserInforDto : OrderDto
    {
        public UserInforDto? UserInfor { get; set; }
    }
}
