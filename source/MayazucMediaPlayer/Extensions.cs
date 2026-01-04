using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Storage;
using Windows.UI;

namespace MayazucMediaPlayer
{
    public static class Extensions
    {
        public static void RemoveWithLock<T>(this Collection<T> list, T item, object lockObject)
        {
            lock (lockObject)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].Equals(item))
                    {
                        list.RemoveAt(i);
                    }
                }
            }
        }


        public static void RemoveAll<T>(this Collection<T> list, IEnumerable<T> items, object lockObject)
        {
            lock (lockObject)
            {
                foreach (var item in items)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i].Equals(item))
                        {
                            list.RemoveAt(i);
                        }
                    }
                }
            }
        }

        public static void AddRangeLast<T>(this LinkedList<T> list, IEnumerable<T> items)
        {
            foreach (var i in items)
                list.AddLast(i);
        }

        public static IEnumerable<ItemIndexRange> GetItemRanges<T>(this IEnumerable<T> items, IEnumerable<T> selector)
        {
            return GetItemRanges(items.ToList(), selector);
        }


        public static IEnumerable<ItemIndexRange> GetItemRanges<T>(this IList<T> items, IEnumerable<T> selector)
        {
            List<ItemIndexRange> ranges = new List<ItemIndexRange>();

            int lastIndex = -1;
            int startIndex = 0;
            uint count = 0;
            var map = items.ObjectToIndexMap();
            foreach (var current in selector)
            {

                var newIndex = map[current];
                if (lastIndex == -1)
                {
                    startIndex = newIndex;
                    count++;
                }
                else
                if (newIndex == lastIndex + 1)
                {
                    count++;
                }
                else
                {
                    ranges.Add(new ItemIndexRange(startIndex, count));
                    startIndex = newIndex;
                    count = 1;
                }

                lastIndex = newIndex;

            }
            if (count > 0)
            {
                ranges.Add(new ItemIndexRange(startIndex, count));
            }

            return ranges.AsEnumerable();
        }

        public static Dictionary<object, int> ObjectToIndexMap<T>(this IList<T> items)
        {
            Dictionary<object, int> retValue = new Dictionary<object, int>();
            for (int i = 0; i < items.Count; i++)
            {
                retValue.Add(items[i], i);
            }

            return retValue;
        }

        public static bool IsElementChildOf(DependencyObject target, DependencyObject parent)
        {
            DependencyObject currentParent = null;
            DependencyObject currenttarget = target;
            do
            {
                currentParent = VisualTreeHelper.GetParent(currenttarget);
                if (currentParent == null)
                {
                    return false;
                }
                else
                {
                    if (currentParent == parent)
                    {
                        return true;
                    }
                }
                currenttarget = currentParent;
            } while (currentParent != null);

            return false;
        }


        public static Color ConvertColorFromHexString(string colorStr)
        {
            //Target hex string

            colorStr = colorStr.Replace("#", string.Empty);
            // from #RRGGBB string
            var r = Convert.ToByte(colorStr.Substring(0, 2), 16);
            var g = Convert.ToByte(colorStr.Substring(2, 2), 16);
            var b = Convert.ToByte(colorStr.Substring(4, 2), 16);
            //get the color
            Color color = Color.FromArgb(255, r, g, b);
            // create the solidColorbrush
            return color;
        }


        public static string ToHexString(this IEnumerable<byte> bytes)
        {
            var returnValue = new StringBuilder();
            foreach (var b in bytes)
            {
                returnValue.Append(b.ToString("x2"));
            }
            return returnValue.ToString();
        }
        /// <summary>
        /// generates a random list of int between 0 and exclusive max
        /// </summary>
        /// <param name="r"></param>
        /// <param name="exclusiveMax"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IList<int> GetUniqueSequence(this Random r, int exclusiveMax, int count, int maxInter = 10)
        {
            List<int> retValue = new List<int>();
            if (exclusiveMax > 0 && count > 0)
            {
                if (count < exclusiveMax)
                {
                    while (retValue.Count < count)
                    {
                        int retryCount = 0;
                        int result = r.Next(0, exclusiveMax);
                        while (retValue.Contains(result) && retryCount < maxInter)
                        {
                            result = r.Next(0, exclusiveMax);
                        }

                        retValue.Add(result);
                    }
                }
                else
                {
                    int c = 0;
                    while (c < count)
                    {
                        retValue.Add(c % exclusiveMax);
                        c++;
                    }
                }
            }

            return retValue;
        }


        public static T GetDataContextObject<T>(this FrameworkElement sender) where T : class
        {
            T result = sender.DataContext as T;
            if (result == null)
            {
                throw new ArgumentException($"Cannot convert {sender.DataContext.GetType()} to {typeof(T)}");
            }

            return result;
        }

        public static bool TryGetDataContextObject<T>(this FrameworkElement sender, out T dataContextObject) where T : class
        {
            dataContextObject = sender.DataContext as T;
            return dataContextObject != null;
        }


        public static T GetDataContextObject<T>(this object? sender) where T : class
        {
            T result = null;
            if (sender is FrameworkElement)
            {
                result = (sender as FrameworkElement).DataContext as T;

                if (result == null)
                {
                    throw new ArgumentException($"Cannot convert {(sender as FrameworkElement).DataContext?.GetType()} to {typeof(T)}");
                }
            }
            else
            {
                throw new ArgumentException("Sender is not framework element");
            }

            return result;

        }

        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static IEnumerable<T> FindVisualChildrenDeep<T>([NotNull] this DependencyObject parent) where T : DependencyObject
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            var queue = new Queue<DependencyObject>(new[] { parent });

            while (queue.Any())
            {
                var reference = queue.Dequeue();
                var count = VisualTreeHelper.GetChildrenCount(reference);

                for (var i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(reference, i);
                    if (child is T children)
                        yield return children;

                    queue.Enqueue(child);
                }
            }
        }


        public static int IndexOf(this IEnumerable<TimedMetadataTrack> tracks, TimedMetadataTrack track)
        {
            int i = 0;
            foreach (var t in tracks)
            {
                if (t == track)
                {
                    return i;
                }
                i++;
            }

            return -1;
        }

        public static Task ForEachAsync<T>(this IEnumerable<T> source, int dop, Func<T, Task> body)
        {
            return Task.WhenAll(
                from partition in Partitioner.Create(source.ToList()).GetPartitions(dop)
                select Task.Run(async delegate
                {
                    using (partition)
                    {
                        while (partition.MoveNext())
                        {
                            await body(partition.Current);
                        }
                    }
                }));
        }

        public static bool IsVideoFile(this StorageFile file)
        {
            return SupportedFileFormats.IsVideoFile(file.Path);
        }

        public static bool IsAudioFile(this StorageFile file)
        {
            return SupportedFileFormats.IsAudioFile(file.Path);
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }


        public static T[] SubArray<T>(this T[] data, int index)
        {
            T[] result = new T[data.Length - index];
            Array.Copy(data, index, result, 0, result.Length);
            return result;
        }


        public static SolidColorBrush GetColorFromHex(string hexaColor)
        {
            return new SolidColorBrush(
                Color.FromArgb(
                  Convert.ToByte(hexaColor.Substring(1, 2), 16),
                    Convert.ToByte(hexaColor.Substring(3, 2), 16),
                    Convert.ToByte(hexaColor.Substring(5, 2), 16),
                    Convert.ToByte(hexaColor.Substring(7, 2), 16)
                )
            );
        }
    }

    public static class DispatcherTaskExtensions
    {

        // There is no TaskCompletionSource<void> so we use a bool that we throw away.
        public static IList<T> Randomize<T>(this IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }

        public static int Randomize<T>(this IList<T> list, int observableIndex)
        {
            Random rng = new Random();
            var returnValue = observableIndex;
            int n = list.Count;
            List<Tuple<int, T>> reorderList = new List<Tuple<int, T>>();
            for (int i = 0; i < list.Count; i++)
            {
                reorderList.Add(new Tuple<int, T>(i, list[i]));
            }

            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                var value = reorderList[k];
                reorderList[k] = reorderList[n];
                reorderList[n] = value;
            }

            for (int i = 0; i < list.Count; i++)
            {
                list[i] = reorderList[i].Item2;
                if (reorderList[i].Item1 == observableIndex)
                {
                    returnValue = i;
                }
            }

            return returnValue;
        }

        public static void NumberMediaData(this ObservableCollection<MediaPlayerItemSourceUIWrapper> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].ExpectedPlaybackIndex = (i);
            }
        }

        public static int MoveItemUp<T>(this IList<T> list, T item, bool clip)
        {
            var index = list.IndexOf(item);
            if (index == -1)
            {
                throw new ArgumentException("item does not belong in list");
            }
            var newIndex = index - 1;
            if (newIndex <= 0)
            {
                if (clip)
                {
                    newIndex = list.Count - 1;
                }
                else
                {
                    newIndex = 0;
                }
            }

            list.Switch(index, newIndex);
            return newIndex;
        }

        public static int MoveItemDown<T>(this IList<T> list, T item, bool clip)
        {
            var index = list.IndexOf(item);
            if (index == -1)
            {
                throw new ArgumentException("item does not belong in list");
            }
            var newIndex = index + 1;
            if (newIndex >= list.Count)
            {
                if (clip)
                {
                    newIndex = 0;
                }
                else
                {
                    newIndex = list.Count - 1;
                }
            }

            list.Switch(index, newIndex);
            return newIndex;
        }

        public static void Switch<T>(this IList<T> list, int source, int destination)
        {
            var temp = list[destination];
            list[destination] = list[source];
            list[source] = temp;
        }

        public static string GetString(this SolidColorBrush brush)
        {
            return brush.Color.A.ToString() + ">" + brush.Color.B.ToString() + ">" + brush.Color.G.ToString() + ">" + brush.Color.R.ToString();
        }

        public static SolidColorBrush GetColorBurhsFromString(this string str)
        {
            var splits = str.Split('>');
            if (splits.Length != 4)
            {
                throw new ArgumentException("The string is not in the correct format");
            }
            else
            {
                return new SolidColorBrush(Color.FromArgb(byte.Parse(splits[0]), byte.Parse(splits[3]), byte.Parse(splits[2]), byte.Parse(splits[1])));
            }
        }

        public static void AddRange<T>(this ObservableCollection<T> target, IEnumerable<T> itemsToAdd)
        {
            foreach (var t in itemsToAdd)
            {
                target.Add(t);
            }
        }

        public static void InsertRange<T>(this ObservableCollection<T> target, IEnumerable<T> itemsToAdd, int index)
        {
            foreach (var t in itemsToAdd.Reverse())
            {
                target.Insert(index, t);
            }
        }

        /// <summary>
        /// Creates the given folder hierarchy in the given folder
        /// </summary>
        /// <param name="baseFolder">base folder</param>
        /// <param name="Path">The hierarchy</param>
        /// <param name="PathSeparator">Cutoff path: Pictures, Music, Videos</param>
        /// <returns></returns>
        public static async Task<StorageFolder> CreateFolderHierarchyAsync(this StorageFolder baseFolder, string Path)
        {
            var indexOfMusic = Path.IndexOf(baseFolder.DisplayName);
            Path = Path.Remove(0, indexOfMusic + baseFolder.DisplayName.Length);
            var splits = Path.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
            var currentFolder = baseFolder;
            foreach (var s in splits)
            {
                try
                {
                    currentFolder = await currentFolder.CreateFolderAsync(s, CreationCollisionOption.ReplaceExisting);
                }
                catch { }
            }

            return currentFolder;
        }


        public static string AddSpacesToSentence(this string text, bool preserveAcronyms)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                {
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                        (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                         i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                    {
                        newText.Append(' ');
                    }
                }

                newText.Append(text[i]);
            }
            return newText.ToString();
        }

        public static string[] SplitByNonAlphaNumeric(this string value)
        {
            List<string> returnValue = new List<string>();
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (char.IsLetterOrDigit(c))
                {
                    builder.Append(c);
                }
                else
                {
                    returnValue.Add(builder.ToString());
                    builder = new StringBuilder();
                }
            }
            return returnValue.ToArray();
        }

        /// <summary>
        /// computes a similarity score by counting how many substrings of string1 are contained in string2
        /// </summary>
        /// <param name="string1"></param>
        /// <param name="string2"></param>
        /// <returns></returns>
        public static int CalculateStringSimilarityScores(this string string1, string string2)
        {
            int returnValue = 0;
            var lower2 = string2.ToLowerInvariant();
            var string1Splits = string1.ToLowerInvariant().SplitByNonAlphaNumeric();

            foreach (var s in string1Splits)
            {
                if (lower2.Contains(s))
                {
                    returnValue++;
                }
            }

            return returnValue;
        }



        /// <summary>
        /// computes a similarity score by counting how many substrings of string1 are contained in string2
        /// </summary>
        /// <param name="string1"></param>
        /// <param name="string2"></param>
        /// <returns></returns>
        public static int CalculateStringSimilarityScores(this string string1, string[] string2)
        {
            int returnValue = 0;
            var string1Splits = string1.ToLowerInvariant().SplitByNonAlphaNumeric();

            foreach (var s in string1Splits)
            {
                if (string2.Contains(s))
                {
                    returnValue++;
                }
            }

            return returnValue;
        }

        public static string GetFFmpegFilterJoinedFilterDef(this IEnumerable<AvEffectDefinition> filters)
        {
            return string.Join(',', filters.Select(x => $"{x.FilterName}={x.Configuration}"));
        }

        public static T GetService<T>(this IServiceProvider provider)
        {
            return (T)provider.GetService(typeof(T));
        }
    }

    public static class TimedMetadataTrackExtensions
    {
        public static bool IsSubtitle(this TimedMetadataTrack track)
        {
            return track.TimedMetadataKind == TimedMetadataKind.Subtitle || track.TimedMetadataKind == TimedMetadataKind.ImageSubtitle;
        }
    }

    public static class RsaExtensions
    {
        public static string ExportRsaPrivateKey(this RSACng cng)
        {
            StringBuilder builder = new StringBuilder();
            var parameters = cng.ExportParameters(true);
            builder.Append(Convert.ToBase64String(parameters.Modulus));
            builder.Append(".");
            builder.Append(Convert.ToBase64String(parameters.Exponent));
            builder.Append(".");
            builder.Append(Convert.ToBase64String(parameters.P));
            builder.Append(".");
            builder.Append(Convert.ToBase64String(parameters.D));
            builder.Append(".");
            builder.Append(Convert.ToBase64String(parameters.DP));
            builder.Append(".");
            builder.Append(Convert.ToBase64String(parameters.DQ));
            builder.Append(".");
            builder.Append(Convert.ToBase64String(parameters.InverseQ));
            builder.Append(".");
            builder.Append(Convert.ToBase64String(parameters.Q));
            return builder.ToString();
        }

        public static string ExportPublicKey(this RSACng cng)
        {
            StringBuilder builder = new StringBuilder();
            var parameters = cng.ExportParameters(true);
            builder.Append(Convert.ToBase64String(parameters.Modulus));
            builder.Append(".");
            builder.Append(Convert.ToBase64String(parameters.Exponent));
            return builder.ToString();
        }

        public static RSACng ImportKey(this string builder)
        {
            var splits = builder.Split(".");
            RSAParameters parameters = new RSAParameters();
            parameters.Modulus = Convert.FromBase64String(splits[0]);
            parameters.Exponent = Convert.FromBase64String(splits[1]);
            parameters.P = Convert.FromBase64String(splits[2]);
            parameters.D = Convert.FromBase64String(splits[3]);
            parameters.DP = Convert.FromBase64String(splits[4]);
            parameters.DQ = Convert.FromBase64String(splits[5]);
            parameters.InverseQ = Convert.FromBase64String(splits[6]);
            parameters.Q = Convert.FromBase64String(splits[7]);
            var cng = new RSACng();
            cng.ImportParameters(parameters);
            return cng;
        }


    }

    public static class IEnumerableExtensions
    {
        public static int IndexOf<T>(this IEnumerable<T> enumerable, T value, IEqualityComparer<T> comparer)
        {
            int currentIterationIndex = -1;
            int actualIndex = -1;

            foreach (var item in enumerable)
            {
                currentIterationIndex++;
                if (comparer.Equals(item, value))
                {
                    actualIndex = currentIterationIndex;
                    break;
                }
            }

            return actualIndex;
        }
    }

    public static class AsyncLockExtensions
    {
        public static IDisposable LockWithTimeout(this AsyncLock asyncLock)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(30000);
            return asyncLock.Lock(cancellationTokenSource.Token);
        }

        public static Task<IDisposable> LockWithTimeoutAsync(this AsyncLock asyncLock)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(30000);
            return asyncLock.LockAsync(cancellationTokenSource.Token);
        }
    }
}
