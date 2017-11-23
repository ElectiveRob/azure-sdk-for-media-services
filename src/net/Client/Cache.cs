﻿//-----------------------------------------------------------------------
// <copyright file="Cache.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
// <license>
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </license>

using System;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Extends standard MemoryCache making it strongly typed and adding
    /// conditional element loading if it is not present.
    /// </summary>
    /// <typeparam name="T">Type of stored elements.</typeparam>
    public class Cache<T> : IDisposable
    {
        private object _refreshLock = new object();
        private MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        /// <summary>
        /// Gets cached element or caches new if it doesn't exist.
        /// </summary>
        /// <param name="key">The key of the element.</param>
        /// <param name="valueFactory">Function returning the element that must be cached.</param>
        /// <param name="expirationFactory">Function returning the expiration time in UTC of the cached element.</param>
        /// <returns>The element from cache or generated by valueFactory if it was not present.</returns>
        public T GetOrAdd(string key, Func<T> valueFactory, Func<DateTime> expirationFactory)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            lock (_refreshLock)
            {
                T result = (T)_cache.Get(key);

                if (result == null)
                {
                    if (valueFactory == null)
                    {
                        throw new ArgumentNullException("valueFactory");
                    }

                    if (expirationFactory == null)
                    {
                        throw new ArgumentNullException("expirationFactory");
                    }

                    result = valueFactory();
                    var cacheEntry = _cache.CreateEntry(key);
                    cacheEntry.Value = result;
                    cacheEntry.AbsoluteExpiration = new DateTimeOffset(expirationFactory(), TimeSpan.Zero);
                }

                return result;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_cache != null)
            {
                if (disposing)
                {
                    _cache.Dispose();
                }

                _cache = null;
            }
        }
        #endregion
    }
}
