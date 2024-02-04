namespace MayazucMediaPlayer.Subtitles.OnlineAPIs.OpenSubtitles
{
    public sealed class OSDBSubtitleInfo : IOSDBSubtitleInfo
    {
        public string SubFileName
        {
            get;
            private set;
        }

        public string SubFormat
        {
            get;
            private set;
        }

        public string LanguageName
        {
            get;
            private set;
        }

        public string SubDownloadLink
        {
            get;
            private set;
        }

        public string Score
        {
            get;
            private set;
        }


        public int SimilarityScore
        {
            get;
            set;
        }

        public string FileId { get; private set; }

        public OSDBSubtitleInfo(string subFileName, string _SubFormat, string _LanguageName, string _SubDownloadLink, string _Score)
        {
            SubFileName = subFileName;
            SubFormat = _SubFormat;
            LanguageName = _LanguageName;
            SubDownloadLink = _SubDownloadLink;
            Score = _Score;
        }
    }

    public class OSDBSubtitleInfo2 : IOSDBSubtitleInfo, IOSDBSubtitleInfo2
    {
        public string LanguageName { get; private set; }

        public string Score { get; private set; }

        public int SimilarityScore { get; set; }

        public string SubDownloadLink { get; private set; }

        public string SubFileName { get; private set; }

        public string SubFormat { get; private set; }

        public int FileId { get; private set; }

        public OSDBSubtitleInfo2(string languageName, string subFileName, string subFormat, int fileId)
        {
            LanguageName = languageName;
            SubFileName = subFileName;
            SubFormat = subFormat;
            FileId = fileId;
        }
    }
}
