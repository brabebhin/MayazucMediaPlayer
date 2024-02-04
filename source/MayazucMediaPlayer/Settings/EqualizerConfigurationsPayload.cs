using MayazucMediaPlayer.AudioEffects;
using Nito.AsyncEx;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MayazucMediaPlayer.Settings
{
    public class EqualizerConfigurationsPayload
    {
        static readonly AsyncLock _syncLock = new AsyncLock();
        private readonly AsyncLock SaveOpenLock = new AsyncLock();
        public EqualizerConfigurationsPayload()
        {
            EQConfigurations = new List<EqualizerConfiguration>();
        }

        public List<EqualizerConfiguration> EQConfigurations
        {
            get; set;
        }

        public void SaveConfigurationAsync2()
        {
            using (SaveOpenLock.Lock())
            {
                foreach (var config in EQConfigurations)
                    config.SaveToFileAsync();
            }
        }


        /// <summary>
        /// returns the default current settings 
        /// </summary>
        /// <returns></returns>
        public static EqualizerConfigurationsPayload GetDefaultConfiguration()
        {
            using (_syncLock.Lock())
            {
                try
                {
                    EqualizerConfigurationsPayload returnValue = new EqualizerConfigurationsPayload();
                    var equalizerConfigurationDirectory = Directory.CreateDirectory(LocalCache.LocalFolders.EqualizerConfigurationsPayloadFolderPath());
                    var filesEnumeration = equalizerConfigurationDirectory.EnumerateFiles();

                    filesEnumeration = equalizerConfigurationDirectory.EnumerateFiles();
                    if (!filesEnumeration.Any())
                    {
                        return GetAndSaveDefaultPayload();
                    }
                    else
                    {
                        foreach (var file in filesEnumeration)
                        {
                            if (file.Length > 0)
                            {
                                var configuration = EqualizerConfiguration.LoadFromFile(file);
                                if (configuration.IsSuccess)
                                    returnValue.EQConfigurations.Add(configuration.Value);
                                else return GetAndSaveDefaultPayload();
                            }
                            else
                            {

                            }
                        }
                    }

                    return returnValue;
                }
                catch
                {
                    return GetAndSaveDefaultPayload();
                }
            }
        }

        private static EqualizerConfigurationsPayload GetAndSaveDefaultPayload()
        {
            var rollbackPayload = GetDefaultPayload();
            rollbackPayload.SaveConfigurationAsync2();
            return rollbackPayload;
        }


        /// <summary>
        /// returns a clean state of configuration, with no user specific changes
        /// </summary>
        /// <returns></returns>
        public static EqualizerConfigurationsPayload GetDefaultPayload()
        {
            EqualizerConfigurationsPayload defaultp = new EqualizerConfigurationsPayload();
            defaultp.EQConfigurations.Add(EqualizerConfiguration.GetDefault());

            return defaultp;
        }
    }
}
