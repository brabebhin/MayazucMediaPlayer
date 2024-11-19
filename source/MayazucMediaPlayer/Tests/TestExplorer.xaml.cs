using CommunityToolkit.Mvvm.Input;
using FFmpegInteropX;
using MayazucMediaPlayer.Common;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.Playlists;
using Windows.Storage;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MayazucMediaPlayer.Tests
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TestExplorer : BasePage
    {
        public override string Title => "Experimental features";

        public TestExplorer()
        {
            InitializeComponent();
            lsvTests.ItemsSource = Tests;

            Tests.Add(new TestItem("Sync obs collection", new AsyncRelayCommand<object>(async (x) =>
            {

                ObservableCollection<string> list = new ObservableCollection<string>();
                list.Add("A");
                list.Add("B");
                list.Add("C");
                list.Add("D");

                SynchronizedObservableCollection<string, string> synced = new SynchronizedObservableCollection<string, string>(list, (x) => x);
                for (int i = 0; i < list.Count; i++)
                {
                    if (synced[i] != list[i])
                        System.Diagnostics.Debugger.Break();
                    if (synced[i].Count() != list.Count)
                        System.Diagnostics.Debugger.Break();
                }


                list.Add("E");
                for (int i = 0; i < list.Count; i++)
                {
                    if (synced[i] != list[i])
                        System.Diagnostics.Debugger.Break();
                    if (synced[i].Count() != list.Count)
                        System.Diagnostics.Debugger.Break();
                }

                list.Remove("B");
                for (int i = 0; i < list.Count; i++)
                {
                    if (synced[i] != list[i])
                        System.Diagnostics.Debugger.Break();
                    if (synced[i].Count() != list.Count)
                        System.Diagnostics.Debugger.Break();
                }

                list.Move(0, 2);
                for (int i = 0; i < list.Count; i++)
                {
                    if (synced[i] != list[i])
                        System.Diagnostics.Debugger.Break();
                    if (synced[i].Count() != list.Count)
                        System.Diagnostics.Debugger.Break();
                }

                list[0] = "X";
                for (int i = 0; i < list.Count; i++)
                {
                    if (synced[i] != list[i])
                        System.Diagnostics.Debugger.Break();
                    if (synced[i].Count() != list.Count)
                        System.Diagnostics.Debugger.Break();
                }


            })));

            Tests.Add(new TestItem("Now playing playbacklist", new AsyncRelayCommand<object>(async (x) =>
            {
                MediaPlaybackList list = new MediaPlaybackList();
                var nowPlaying = ServiceProvider.GetService<IPlaybackSequenceProvider>().GetPlaybackQueue();
                var builder = new FFmpegInteropItemBuilder(null);
                for (int i = 0; i < 100; i++)
                {
                    await nowPlaying.ForEachAsync(4, async (item) =>
                    {
                        Stopwatch watch = new Stopwatch();
                        watch.Start();
                        var mss = await builder.GetFFmpegInteropMssAsync(item, true, 0);
                        list.Items.Add(mss.PlaybackItem);
                        watch.Stop();
                        Debug.WriteLine($"playback item in {watch.ElapsedMilliseconds}");
                    });
                }
            })));



            Tests.Add(new TestItem("System playlists", new AsyncRelayCommand<object>(async (x) =>
            {

                var playlists = await KnownFolders.Playlists.GetFilesAsync();
                var first = playlists.FirstOrDefault();
                Playlist pls = await Playlist.LoadAsync(first);
                var files = pls.Files;
                foreach (var f in files)
                {
                    System.Diagnostics.Debug.WriteLine(f.Path);
                }

            })));


            Tests.Add(new TestItem("Youtube player", new AsyncRelayCommand<object>(async (x) =>
            {

                //YouTubeUri url = await YouTube.GetVideoUriAsync("xloasKalGGM", YouTubeQuality.Quality360P);
                //FFmpegInteropX.MediaSourceConfig config = new FFmpegInteropX.MediaSourceConfig();
                //config.PassthroughAudioMP3 = false;
                //config.PassthroughAudioAAC = false;
                //config.FFmpegOptions = new Windows.Foundation.Collections.PropertySet();
                //config.FFmpegOptions.Add("rw_timeout", 10000);
                //var urlS = url.Uri.ToString();
                //var interop = await FFmpegInteropX.FFmpegMediaSource.CreateFromUriAsync(urlS, config);


            })));

            Tests.Add(new TestItem("external subtitles ffmpeg", new AsyncRelayCommand<object>(async (x) =>
            {
                var subtitleFiles = new string[] { "subviewer.sub", "ttml.ttml", "VTT.vtt", "sami.sami", "subtitle.lrc", "subRIP.srt", "MicroDVD.sub" };

                foreach (var s in subtitleFiles)
                {
                    try
                    {
                        var constructString = $"ms-appx:///Tests/ExternalSubtitles/{s}";
                        var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(constructString));

                        var streamFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Tests/ExternalSubtitles/silence.flac"));
                        var stream = await streamFile.OpenReadAsync();
                        var interop = await FFmpegMediaSource.CreateFromStreamAsync(stream);
                        var subs = await interop.AddExternalSubtitleAsync(await file.OpenReadAsync(), file.Name);
                        subs.ToString();
                        if (interop.SubtitleStreams.Count == 0)
                        {
                            System.Diagnostics.Debugger.Break();
                        }
                        if (!interop.SubtitleStreams.FirstOrDefault().IsExternal)
                        {
                            System.Diagnostics.Debugger.Break();
                        }
                        if (interop.SubtitleStreams.FirstOrDefault().SubtitleTrack.Cues.Count != 3)
                        {
                            System.Diagnostics.Debug.WriteLine(interop.SubtitleStreams.FirstOrDefault().SubtitleTrack.Cues.Count);
                            foreach (var c in interop.SubtitleStreams.FirstOrDefault().SubtitleTrack.Cues)
                            {
                                System.Diagnostics.Debug.WriteLine(((TimedTextCue)c).Lines.Count);
                            }
                            System.Diagnostics.Debugger.Break();

                        }
                        interop.Dispose();

                    }
                    catch
                    {

                    }
                }
            })));




            Tests.Add(new TestItem("external subtitles ffmpeg (URI)", new AsyncRelayCommand<object>(async (x) =>
            {

                var subtitleFiles = new string[] { "sami.sami", "subtitle.lrc", "ass.ssa", "ttml.ttml", "VTT.vtt", "subRIP.srt", "MicroDVD.sub" };

                foreach (var s in subtitleFiles)
                {
                    var constructString = $"ms-appx:///Tests/ExternalSubtitles/{s}";
                    var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(constructString));

                    var streamFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Tests/ExternalSubtitles/silence.flac"));
                    //var stream = await streamFile.OpenReadAsync();
                    var interop = await FFmpegMediaSource.CreateFromUriAsync("http://stream2.srr.ro:8040");
                    await interop.AddExternalSubtitleAsync(await file.OpenReadAsync(), file.Name);

                    if (interop.SubtitleStreams.Count == 0)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                    if (!interop.SubtitleStreams.FirstOrDefault().IsExternal)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                    if (interop.SubtitleStreams.FirstOrDefault().SubtitleTrack.Cues.Count != 3)
                    {
                        System.Diagnostics.Debug.WriteLine(interop.SubtitleStreams.FirstOrDefault().SubtitleTrack.Cues.Count);
                        System.Diagnostics.Debugger.Break();

                    }

                    interop.Dispose();
                }


            })));
            //tests.Add(new TestItem("Custom playback surface", new AsyncRelayCommand<object>(async (x) =>
            //{
            //    this.Frame.NavigateSingle(typeof(CustomCompositionSurface));
            //})));
        }


        public ObservableCollection<TestItem> Tests { get; set; } = new ObservableCollection<TestItem>();

    }
}
