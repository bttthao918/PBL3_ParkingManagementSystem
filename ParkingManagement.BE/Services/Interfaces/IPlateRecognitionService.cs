using ParkingManagement.BLL.DTOs;

namespace ParkingManagement.BLL.Services.Interfaces
{
    public interface IPlateRecognitionService
    {
        Task<PlateRecognitionResponseDto> RecognizeAsync(PlateRecognitionRequestDto request, CancellationToken cancellationToken = default);
    }
}
