namespace MayazucMediaPlayer
{
    internal class FolderNames
    {
        internal static readonly string EqualizerConfigurations = "EqualizerConfigurations";
    }

    internal class ContainerNames
    {
        internal static readonly string AudioDSP = "audioDSP";
        internal static readonly string PlayerState = "playerstate";
        internal static readonly string MetadataOptions = "metadataoptions";
        internal static readonly string Customizations = "customizations";
        internal static readonly string Networking = "NetworkSettings";
    }

    internal class ContainerKeys
    {
        internal static readonly string BlockConfirmationMessages = "BlockConfirmationMessages";
        internal static readonly string AutoPlayVideo = "AutoPlayVideo";
        internal static readonly string AutoPlayMusic = "AutoPlayMusic";

        internal static readonly string equalizerEnabled = "equalizerEnabled";

        /// <summary>
        /// 0 => frequency
        /// 1 => octaves
        /// </summary>
        internal static readonly string EqualizerConfigurationStringFormat = "f={0}:width_type=o:width={1}:g={2}";

        internal static readonly string playerstateStopMusicEnabled = "StopMusicEnabled";
        internal static readonly string playerstateStopMusicTimePosition = "StopMusicTimePosition";

        internal static readonly string metadataoptionsGenreIndex = "genreindex";
        internal static readonly string metadataoptionsArtistIndex = "artistindex";
        internal static readonly string metadataoptionsAlbumIndex = "albumindex";
        internal static readonly string metadataOptionsUseDefault = "usedefault";

        internal static readonly string playerstateIndex = "index";

        internal static readonly string AudioBalance = "audiobalance";

        internal static readonly string ResumePosition = "LastTrackResumePosition";

        internal static readonly string NetworkRole = "Role";

        internal static readonly string AutoClearFilePicker = "AutoClearFilePicker";

        internal static readonly string subtitleAutoDetect = "subtitleAutoDetect";
        internal static readonly string subtitleLanguage = "subtitleLanguage";
        internal static readonly string VideoOutputAllowIyuv = "VideoOutputAllowIyuv";
        internal static readonly string VideoOutputAllow10bit = "VideoOutputAllow10bit";
        internal static readonly string VideoOutputAllowBgra8 = "VideoOutputAllowBgra8";
        internal static readonly string StartPlaybackAfterSeek = "StartPlaybackAfterSeek";
        internal static readonly string PlaybackTapGestureMode = "PlaybackTapGestureMode";
        internal static readonly string DefaultUITheme = "DefaultUITheme";
        internal static readonly string CurrentPlaylist = "CurrentPlaylist";

        internal static readonly string AutoloadInternalSubtitle = "AutoloadInternalSubtitle";

        internal static readonly string FFmpegCharacterEncodingIndex = "FFmpegCharacterEncodingIndex";
        internal static readonly string KeepPlaybackRateBetweenTracks = "KeepPlaybackRateBetweenTracks";
        internal static readonly string AutoloadForcedSubtitles = "AutoloadForcedSubtitles";
        internal static readonly string DiscoveryPort = "DiscoveryPort";
        internal static readonly string VideoDecoderMode = "VideoDecoderMode2";
        internal static readonly string PlayerResumePath = "PlayerResumePath";
        internal static readonly string MinimumSubtitleDuration = "MinimumSubtitleDuration";
        internal static readonly string PreventSubtitleOverlaps = "PreventSubtitleOverlaps";
        internal static readonly string PlayToEnabled = "PlayToEnabled";
        internal static readonly string PlaybackIndexDlnaIntrerupt = "PlaybackIndexDlnaIntrerupt";
        internal static readonly string ResumePositionDlnaIntrerupt = "ResumePositionDlnaIntrerupt";
        internal static readonly string StereoDownMix = "StereoDownMix";
        internal static readonly string EqualizerConfiguration = "EqualizerConfiguration";
        internal static readonly string OnlyUseCacheInFilePicker = "OnlyUseCacheInFilePicker";
    }

    internal class FontIconPaths
    {
        internal static readonly string ActionsImagePath = "\uE123";
        internal static readonly string PlaybackControlImagePath = "\uEC58";
        internal static readonly string NowPlayingImagePath = "\uE29B";

        internal static readonly string CollectionView = "\uF57E";
        internal static readonly string ExclamationMark = "\uE814";


        internal static readonly string metadataGroupImage = "\uE142";

        internal static readonly string metadataFolderCovers = "\uEC50";
        internal static readonly string metadataFileCovers = "\uE838";
        internal static readonly string saveButton = "\uE142";

        internal static readonly string albumImage = "\uE17B";

        internal static readonly string EncodingsGroupImage = "\uE164";
        internal static readonly string SubtitlesEncodings = "\uE190";
        internal static readonly string VideoSettings = "\uE116";

        internal static readonly string PlaceholderAlbumArt = "ms-appx:///Assets/placeholderAlbumArt.png";
        internal static readonly string AutoPlaySettings = "\uEB9D";
        internal static readonly string UIThemesGroup = "\uE113";
    }

    internal class AssetsPaths
    {
        internal static readonly string PlaceholderAlbumArt = "ms-appx:///Assets/placeholderAlbumArt.png";
        internal static readonly string PlaylistCoverPlaceholder = @"ms-appx:///Assets/playlistPlaceholder.png";
    }
}
