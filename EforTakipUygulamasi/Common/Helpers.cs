namespace EforTakipUygulamasi.Common
{
    public static class EffortHelper
    {
        public static string GetTShirtColor(TShirtSizeEnum size)
        {
            return size switch
            {
                TShirtSizeEnum.FastTrack => Constants.Colors.Green,
                TShirtSizeEnum.XS => Constants.Colors.Green,
                TShirtSizeEnum.S => Constants.Colors.Yellow,
                TShirtSizeEnum.M => Constants.Colors.Orange,
                TShirtSizeEnum.L => Constants.Colors.Red,
                TShirtSizeEnum.XL => Constants.Colors.Purple,
                _ => Constants.Colors.Red
            };
        }

        public static bool IsOverdue(DateTime? deadline)
        {
            return deadline.HasValue && deadline.Value < DateTime.Now;
        }

        public static string StatusToString(RequestStatusEnum status)
        {
            return status switch
            {
                RequestStatusEnum.New => Constants.Status.New,
                RequestStatusEnum.InProgress => Constants.Status.InProgress,
                RequestStatusEnum.Testing => "Test",
                RequestStatusEnum.OnHold => Constants.Status.OnHold,
                RequestStatusEnum.Completed => Constants.Status.Completed,
                RequestStatusEnum.Cancelled => Constants.Status.Cancelled,
                _ => Constants.Status.New
            };
        }
    }
}