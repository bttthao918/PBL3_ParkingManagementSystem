namespace ParkingManagement.BLL.DTOs
{
    public class PlateRecognitionOptions
    {
        public string Provider { get; set; } = "PlateRecognizer";
        public string ApiToken { get; set; } = string.Empty;
        public string Endpoint { get; set; } = "https://api.platerecognizer.com/v1/plate-reader/";
        public string Regions { get; set; } = "vn";
        public string Config { get; set; } = "{\"text_formats\":[\"[0-9][0-9][a-z][0-9]?[0-9][0-9][0-9][0-9][0-9]\"]}";
        public int TimeoutSeconds { get; set; } = 12;
    }

    public class PlateRecognitionRequestDto
    {
        public string ImageBase64 { get; set; } = string.Empty;
    }

    public class PlateRecognitionResponseDto
    {
        public bool Success { get; set; }
        public string? Plate { get; set; }
        public double? Score { get; set; }
        public string Provider { get; set; } = "PlateRecognizer";
        public string? Message { get; set; }
        public List<string> Candidates { get; set; } = new();
    }
}
