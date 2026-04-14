namespace MandarinBid.Models
{
    public class ErrorViewModel
    {
        // id запроса (используется для трекинга ошибок)
        public string? RequestId { get; set; }

        // показывать ли request id в ui
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}