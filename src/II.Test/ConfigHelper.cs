﻿// Copyright 2018 by PeopleWare n.v..
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Configuration;
using System.Globalization;

namespace PPWCode.Vernacular.NHibernate.II.Test
{
    /// <summary>
    ///     Helper class for configuration.
    /// </summary>
    internal static class ConfigHelper
    {
        /// <summary>
        ///     Gets the value for the given key out of the configuration.
        /// </summary>
        /// <typeparam name="T">The type used.</typeparam>
        /// <param name="key">The key as string.</param>
        /// <param name="defaultValue">The default of the type.</param>
        /// <returns>The given type.</returns>
        public static T GetAppSetting<T>(string key, T defaultValue = default(T))
            where T : IConvertible
        {
            T result;
            try
            {
                result =
                    ConfigurationManager.AppSettings.Get(key) != null
                        ? (T)Convert.ChangeType(ConfigurationManager.AppSettings[key], typeof(T), CultureInfo.InvariantCulture)
                        : defaultValue;
            }
            catch
            {
                result = defaultValue;
            }

            return result;
        }

        /// <summary>
        ///     Gets the value for the given key out of the given configuration.
        /// </summary>
        /// <typeparam name="T">The type used.</typeparam>
        /// <param name="configuration">The configuration.</param>
        /// <param name="key">The given key.</param>
        /// <param name="defaultValue">The default value of T.</param>
        /// <returns>The given type.</returns>
        public static T GetAppSetting<T>(Configuration configuration, string key, T defaultValue = default(T))
            where T : IConvertible
        {
            T result;
            try
            {
                result =
                    configuration.AppSettings.Settings[key] != null
                        ? (T)Convert.ChangeType(configuration.AppSettings.Settings[key].Value, typeof(T), CultureInfo.InvariantCulture)
                        : defaultValue;
            }
            catch
            {
                result = defaultValue;
            }

            return result;
        }

        /// <summary>
        ///     Gets the connection string for the given key.
        /// </summary>
        /// <param name="key">The given key.</param>
        /// <returns>The connection string.</returns>
        public static string GetConnectionString(string key)
            => ConfigurationManager.ConnectionStrings[key]?.ConnectionString;
    }
}
