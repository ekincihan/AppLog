using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace AppLog.Core.Model.Helper
{
    public static class ApplicationHelper
    {
        public static object GetEnumValue(Type convertionType, string value)
        {
            return Enum.Parse(convertionType, value);
        }
        public static T GetEnumValue<T>(string value)
        {
            return (T)GetEnumValue(typeof(T), value);
        }
        public static T GetAppConfigValue<T>(string key)
        {
            return GetAppConfigValue<T>(key, default, false);
        }

        public static T GetAppConfigValue<T>(string key, bool throwErrorOnMissingKey)
        {
            return GetAppConfigValue<T>(key, default, throwErrorOnMissingKey);
        }

        public static T GetAppConfigValue<T>(string key, T defaultValue, bool throwErrorOnMissingKey)
        {
            if (string.IsNullOrEmpty(key))
            {
                Exception exception = new Exception("A key can not be null or empty !");
                throw exception;
            }

            string configValue = AppConfiguration.Instance.Configuration.GetSection(key).Value;
            if (configValue == null)
            {
                if (throwErrorOnMissingKey)
                    throw new ArgumentOutOfRangeException();
                else
                    return defaultValue;
            }
            return GetConvertedValue<T>(configValue);
        }
        
        public static T GetConvertedValue<T>(object value)
        {
            return GetConvertedValueForCulture<T>(value, CultureInfo.CurrentUICulture);
        }
        public static T GetConvertedValueForCulture<T>(object value, CultureInfo culture)
        {
            try
            {
                if (typeof(T).IsGenericType)
                {
                    if (typeof(T).GetGenericArguments()[0] == typeof(Guid)
                        ||
                        (typeof(T) == typeof(Guid)))
                    {
                        return (T)((object)new Guid(value.ToString()));
                    }
                    else
                    {
                        if (value == null || value == DBNull.Value)
                            return default;
                        return GetConvertedCultureValue<T>(value, culture);
                    }
                }
                else if (typeof(T).BaseType == typeof(Enum))
                {
                    return GetEnumValue<T>(value.ToString());
                }
                else if (typeof(T) == typeof(bool))
                {
                    object returnValue = value != null && (
                                             value.ToString().ToLowerInvariant().Equals("on") ||
                                             value.ToString().ToLowerInvariant().Equals("yes") ||
                                             value.ToString().ToLowerInvariant().Equals(bool.TrueString.ToLowerInvariant()) ||
                                             value.ToString().Equals("1"));
                    return (T)returnValue;
                }
                else
                {
                    return GetConvertedCultureValue<T>(value, culture);
                }
            }
            catch (Exception e)
            {
                Exception exception = new Exception(string.Format("Can not convert {0} to enum type {1}, Err: {2}", value, typeof(T).ToString(), e.Message));
                throw exception;
            }
        }
        private static T GetConvertedCultureValue<T>(object value, CultureInfo culture)
        {
            if (culture == null) throw new ArgumentNullException("culture");
            Type destinationType = typeof(T);
            if (destinationType.IsGenericType && destinationType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                    return default;

                var nullableConverter = new NullableConverter(destinationType);
                destinationType = nullableConverter.UnderlyingType;
            }
            if (value is IConvertible)
            {
                value = (T)Convert.ChangeType(value, destinationType, culture);
            }
            return (T)value;
        }
        public static string GetMacAddress()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            String macAddress = String.Empty;
            foreach (NetworkInterface adapter in nics)
            {
                if (macAddress == String.Empty)
                {
                    macAddress = adapter.GetPhysicalAddress().ToString();
                }
            }
            return macAddress;
        }
        public static string[] GetLocalIpAddresses()
        {
            String strHostName = Dns.GetHostName();
            return Dns.GetHostEntry(strHostName).AddressList.Select(x => x.ToString()).ToArray();
        }
        public static string GetHostname()
        {
            return Dns.GetHostName();
        }
        public static string GetApplicationName()
        {
            return System.AppDomain.CurrentDomain.FriendlyName;
        }
        public static string GetExecutingPath()
        {
            return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

    }
}