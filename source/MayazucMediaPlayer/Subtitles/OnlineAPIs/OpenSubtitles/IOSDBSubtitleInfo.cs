namespace MayazucMediaPlayer.Subtitles.OnlineAPIs.OpenSubtitles
{
    public interface IOSDBSubtitleInfo
    {
        string LanguageName { get; }
        int SimilarityScore { get; set; }
        string SubDownloadLink { get; }
        string SubFileName { get; }
        string SubFormat { get; }
    }

    public interface IOSDBSubtitleInfo2
    {
        int FileId { get; }
    }
}