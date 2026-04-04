namespace YooAsset
{
    [UnityEngine.Scripting.Preserve]
    public sealed class HttpResponse
    {
        [UnityEngine.Scripting.Preserve]
        public bool Success { get; set; }

        [UnityEngine.Scripting.Preserve]
        public long StatusCode { get; set; }

        [UnityEngine.Scripting.Preserve]
        public string Error { get; set; }

        [UnityEngine.Scripting.Preserve]
        public string Text { get; set; }

        [UnityEngine.Scripting.Preserve]
        public byte[] Data { get; set; }
    }
}
