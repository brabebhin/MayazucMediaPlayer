using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Services.MediaSources;
using MayazucMediaPlayer.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.MediaPlayback.MediaSequencer
{
    public class MediaPlaybackQueueProvider
    {
        public bool Shuffle { get; set; }

        public string AutoRepeatMode { get; set; }

        PlaybackSequence InternalList
        {
            get;
            set;
        }

        readonly Dictionary<NextMediaStrategyKey, Func<int, Task<int>>> nextIndexStrategies = new Dictionary<NextMediaStrategyKey, Func<int, Task<int>>>();
        public Stack<int> BackStack = new Stack<int>();

        readonly RandomQueueNavigator rngIndexSource = new RandomQueueNavigator();

        public async Task<IMediaPlayerItemSource> GetNextMediaData(int currentIndx, bool userAction, bool incrementIndex)
        {
            var newIndex = currentIndx;
            if (incrementIndex)
            {
                AutoRepeatMode = SettingsService.Instance.RepeatMode;
                var repeatMode = GetRepeatMode(AutoRepeatMode);
                var strategy = new NextMediaStrategyKey(userAction, Shuffle, repeatMode);
                newIndex = await nextIndexStrategies[strategy].Invoke(currentIndx);
            }

            if (newIndex == -1) return null;
            BackStack.Push(newIndex);
            InternalList[newIndex].MediaData.ExpectedPlaybackIndex = newIndex;
            return InternalList[newIndex].MediaData;
        }

        public Task<IMediaPlayerItemSource> GetStartingItem(int startIndex)
        {

            if (InternalList.Count < startIndex) return Task.FromResult<IMediaPlayerItemSource>(null);
            else
            {
                rngIndexSource.Seek(startIndex);
                InternalList[startIndex].MediaData.ExpectedPlaybackIndex = startIndex;
                return Task.FromResult(InternalList[startIndex].MediaData);
            }
        }

        RepeatMode GetRepeatMode(string mode)
        {
            switch (mode)
            {
                case Constants.RepeatNone: return RepeatMode.StopEndOfList;
                case Constants.RepeatOne: return RepeatMode.One;
                case Constants.RepeatAll: return RepeatMode.List;
            }
            throw new NotSupportedException();
        }

        public MediaPlaybackQueueProvider(PlaybackSequence _internalList)
        {
            InternalList = _internalList;

            nextIndexStrategies.Add(new NextMediaStrategyKey(userAction: false, shuffleMode: false, repeatMode: RepeatMode.One), (CurrentIndex) => { return Task.FromResult(CurrentIndex); });
            nextIndexStrategies.Add(new NextMediaStrategyKey(userAction: false, shuffleMode: false, repeatMode: RepeatMode.StopEndOfList), (CurrentIndex) => { return Task.FromResult(CurrentIndex == InternalList.Count - 1 ? -1 : CurrentIndex + 1); });
            nextIndexStrategies.Add(new NextMediaStrategyKey(userAction: false, shuffleMode: false, repeatMode: RepeatMode.List), (CurrentIndex) => { return IncrementLoopBackIndex(CurrentIndex); });

            nextIndexStrategies.Add(new NextMediaStrategyKey(userAction: false, shuffleMode: true, repeatMode: RepeatMode.One), (CurrentIndex) => { return Task.FromResult(CurrentIndex); });
            nextIndexStrategies.Add(new NextMediaStrategyKey(userAction: false, shuffleMode: true, repeatMode: RepeatMode.StopEndOfList), (CurrentIndex) => { return Task.FromResult(GetRandomIndex()); });
            nextIndexStrategies.Add(new NextMediaStrategyKey(userAction: false, shuffleMode: true, repeatMode: RepeatMode.List), (CurrentIndex) => { return Task.FromResult(GetRandomIndex()); });

            nextIndexStrategies.Add(new NextMediaStrategyKey(userAction: true, shuffleMode: false, repeatMode: RepeatMode.One), (CurrentIndex) => { return IncrementLoopBackIndex(CurrentIndex); });
            nextIndexStrategies.Add(new NextMediaStrategyKey(userAction: true, shuffleMode: false, repeatMode: RepeatMode.StopEndOfList), (CurrentIndex) => { return IncrementLoopBackIndex(CurrentIndex); });
            nextIndexStrategies.Add(new NextMediaStrategyKey(userAction: true, shuffleMode: false, repeatMode: RepeatMode.List), (CurrentIndex) => { return IncrementLoopBackIndex(CurrentIndex); });

            nextIndexStrategies.Add(new NextMediaStrategyKey(userAction: true, shuffleMode: true, repeatMode: RepeatMode.One), (CurrentIndex) => { return Task.FromResult(GetRandomIndex()); });
            nextIndexStrategies.Add(new NextMediaStrategyKey(userAction: true, shuffleMode: true, repeatMode: RepeatMode.StopEndOfList), (CurrentIndex) => { return Task.FromResult(GetRandomIndex()); });
            nextIndexStrategies.Add(new NextMediaStrategyKey(userAction: true, shuffleMode: true, repeatMode: RepeatMode.List), (CurrentIndex) => { return Task.FromResult(GetRandomIndex()); });
        }

        public void SkipToIndex(int newIndex)
        {
            rngIndexSource.Seek(newIndex);
        }

        public void Refresh(int CurrentIndex)
        {
            rngIndexSource.RegenerateRandomSequence(InternalList.Count, CurrentIndex);
        }

        /// <summary>
        /// increments index or returns 0 at end of list
        /// </summary>
        /// <returns></returns>
        private async Task<int> IncrementLoopBackIndex(int CurrentIndex)
        {
            var possibleNextIndex = CurrentIndex + 1;
            var indexHandledResult = await InternalList.GetMediaDataItemAtIndex(possibleNextIndex);
            if (indexHandledResult.IsFailed)
                return 0;
            return possibleNextIndex;
        }

        private int GetRandomIndex()
        {
            return rngIndexSource.GetNextIndex();
        }

        private enum RepeatMode
        {
            List = 0,
            One = 1,
            StopEndOfList = 2
        }

        private class RandomQueueNavigator
        {
            List<int> randomIndices = new List<int>();
            int currentIndex = 0;
            public int GetNextIndex()
            {
                //if this is the last one, reshuffle again. 
                if (currentIndex == randomIndices.Count - 1)
                {
                    var actualIndex = randomIndices[currentIndex];
                    //the first item in the new order, which would be the next we return, should not be the same
                    randomIndices.Randomize();
                    if (randomIndices[0] == actualIndex)
                    {
                        randomIndices.RemoveAt(0);
                        randomIndices.Add(actualIndex);
                    }
                }
                currentIndex = currentIndex + 1 % randomIndices.Count;
                return randomIndices[currentIndex];
            }

            public void RegenerateRandomSequence(int count, int currentIndex)
            {
                randomIndices = Enumerable.Range(0, count).ToList();
                randomIndices.Randomize();
                if (currentIndex < 0 || currentIndex >= count)
                    currentIndex = 0;
                Seek(currentIndex);
            }

            public void Seek(int newIndex)
            {
                for (int i = 0; i < randomIndices.Count; i++)
                {
                    if (randomIndices[i] == newIndex)
                        currentIndex = i;
                }
            }
        }

        private class NextMediaStrategyKey
        {
            public bool UserAction { get; private set; }
            public bool ShuffleMode { get; private set; }

            public RepeatMode RepeatMode { get; private set; }

            public NextMediaStrategyKey(bool userAction, bool shuffleMode, RepeatMode repeatMode)
            {
                ShuffleMode = shuffleMode;
                RepeatMode = repeatMode;
                UserAction = userAction;
            }

            public override bool Equals(object obj)
            {
                return obj is NextMediaStrategyKey key &&
                       ShuffleMode == key.ShuffleMode &&
                       RepeatMode == key.RepeatMode &&
                       UserAction == key.UserAction;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(ShuffleMode, RepeatMode, UserAction);
            }
        }
    }
}
