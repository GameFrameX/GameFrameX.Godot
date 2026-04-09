namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    public sealed class HttpResponse
    {
        [AssetSystemPreserve]
        public bool Success { get; set; }

        [AssetSystemPreserve]
        public long StatusCode { get; set; }

        [AssetSystemPreserve]
        public string Error { get; set; }

        [AssetSystemPreserve]
        public string Text { get; set; }

        [AssetSystemPreserve]
        public byte[] Data { get; set; }
    }
}
