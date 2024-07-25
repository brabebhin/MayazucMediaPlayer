using System;
using System.Linq;
using System.Reflection;

namespace MayazucMediaPlayer.Settings
{
    public static class SettingsDefaultValues
    {
        public static void RestoreSettingsDefaults()
        {
            var properties = typeof(SettingsService).GetProperties();
            foreach (var v in properties)
            {
                var attribute = SettingDefaultValueAttribute.GetDefaultValue(v);
                if (attribute != null)
                    v.SetValue(SettingsService.Instance, attribute.DefaultValue);
            }
        }

        /// <summary>
        /// no-op if no debugger is attached. Tests only. Call after fresh install only
        /// </summary>
        internal static void TestDefaultAttributesCorrectness()
        {
            var properties = typeof(SettingsService).GetProperties();
            foreach (var v in properties)
            {
                var value = v.GetValue(null);
                var attribute = SettingDefaultValueAttribute.GetDefaultValue(v);
                if (attribute != null)
                {
                    if (!value.Equals(attribute.DefaultValue))
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                }
            }
        }
    }



    public class SettingDefaultValueAttribute : Attribute
    {


        public object DefaultValue
        {
            get;
            private set;
        }


        public SettingDefaultValueAttribute(object defaultValue)
        {
            DefaultValue = defaultValue;

        }

        public static SettingDefaultValueAttribute GetDefaultValue(PropertyInfo propertyData)
        {
            var attributes = propertyData.GetCustomAttributes(typeof(SettingDefaultValueAttribute));
            return attributes.FirstOrDefault() as SettingDefaultValueAttribute;
        }
    }
}

