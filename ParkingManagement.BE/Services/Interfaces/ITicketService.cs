using ParkingManagement.BLL.DTOs;

namespace ParkingManagement.BLL.Services.Interfaces
{
    public interface ITicketService
    {
        // ── General Ticket Management (Manager & Employee) ──
        Task<ListTicketDto> GetTicketsAsync(TicketFilterDto filter);
        Task<TicketDetailDto> GetTicketDetailAsync(string ticketId);
        Task<ListEmployeeTicketDto> SearchTicketsAsync(EmployeeTicketSearchDto search);

        // ── Customer Specific ──
        Task<ListCustomerTicketDto> GetMyTicketsAsync(string customerId, CustomerTicketFilterDto filter);
        Task<CustomerTicketDetailDto> GetCustomerTicketDetailAsync(string customerId, string ticketId);
        Task<ListCustomerPaymentDto> GetPaymentHistoryAsync(string customerId, CustomerPaymentFilterDto filter);

        // ── Check-in ──
        Task<CheckInValidationDto> ValidateAndPrepareCheckInAsync(CheckInInputDto input);
        Task<CheckInResultDto> ConfirmCheckInAsync(ConfirmCheckInDto input);

        // ── Check-out ──
        Task<CheckOutValidationDto> ValidateAndPrepareCheckOutAsync(CheckOutInputDto input);
        Task<CheckOutResultDto> ConfirmCheckOutAsync(ConfirmCheckOutDto input);
    }
}
