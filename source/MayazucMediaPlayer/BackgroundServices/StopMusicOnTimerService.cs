using Quartz;
using Quartz.Impl;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.BackgroundServices
{
    internal class StopMusicOnTimerService
    {
        private readonly CancellationTokenSource cancelToken = new CancellationTokenSource();
        public async Task StartService(TimeSpan time)
        {
            ISchedulerFactory schedFact = new StdSchedulerFactory();

            // get a scheduler
            IScheduler sched = await schedFact.GetScheduler(cancelToken.Token);
            await sched.Start(cancelToken.Token);

            IJobDetail job = JobBuilder.Create<StopMusicJob>()
                .WithIdentity($"MCMediaCenter.{Guid.NewGuid().ToString()}", $"StopMusicOnTimer.{Guid.NewGuid().ToString()}")
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
               .WithDailyTimeIntervalSchedule
                 (s =>
                    s.WithIntervalInHours(24)
                   .OnEveryDay()
                   .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(time.Hours, time.Minutes))
                 )
               .Build();

            await sched.ScheduleJob(job, trigger, cancelToken.Token);
        }

        public void StopService()
        {
            cancelToken.Cancel();
        }

        private class StopMusicJob : IJob
        {
            public async Task Execute(IJobExecutionContext context)
            {
                var mediaPlayer = AppState.Current.MediaServiceConnector.CurrentPlayer;
                mediaPlayer.Pause();
            }
        }
    }
}
