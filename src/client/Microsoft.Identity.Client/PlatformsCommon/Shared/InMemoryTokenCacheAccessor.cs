﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Microsoft.Identity.Client.Cache;
using Microsoft.Identity.Client.Cache.Items;
using Microsoft.Identity.Client.Cache.Keys;
using Microsoft.Identity.Client.Core;

namespace Microsoft.Identity.Client.PlatformsCommon.Shared
{
    /// <summary>
    /// Keeps the token cache dictionaries in memory. Token Cache extensions
    /// are responsible for persistance.
    /// </summary>
    /// <remarks>See this post for efficient use of Concurrent Dictionary http://geekswithblogs.net/simonc/archive/2012/02/22/inside-the-concurrent-collections-concurrentdictionary.aspx</remarks>
    internal class InMemoryTokenCacheAccessor : ITokenCacheAccessor
    {
        private readonly ConcurrentDictionary<string, MsalAccessTokenCacheItem> _accessTokenCacheDictionary =
            new ConcurrentDictionary<string, MsalAccessTokenCacheItem>();

        private readonly ConcurrentDictionary<string, MsalRefreshTokenCacheItem> _refreshTokenCacheDictionary =
            new ConcurrentDictionary<string, MsalRefreshTokenCacheItem>();

        private readonly ConcurrentDictionary<string, MsalIdTokenCacheItem> _idTokenCacheDictionary =
            new ConcurrentDictionary<string, MsalIdTokenCacheItem>();

        private readonly ConcurrentDictionary<string, MsalAccountCacheItem> _accountCacheDictionary =
            new ConcurrentDictionary<string, MsalAccountCacheItem>();

        private readonly ConcurrentDictionary<string, MsalAppMetadataCacheItem> _appMetadataDictionary =
           new ConcurrentDictionary<string, MsalAppMetadataCacheItem>();
        private readonly ICoreLogger _logger;

        public InMemoryTokenCacheAccessor(ICoreLogger logger)
        {
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        #region Add
        public void SaveAccessToken(MsalAccessTokenCacheItem item)
        {
            string key = item.GetKey().ToString();

            // if a conflict occurs, pick the latest value
            _accessTokenCacheDictionary.AddOrUpdate(key, item, (k, oldValue) => item);
        }

        public void SaveRefreshToken(MsalRefreshTokenCacheItem item)
        {
            string key = item.GetKey().ToString();
            _refreshTokenCacheDictionary.AddOrUpdate(key, item, (k, oldValue) => item);
        }

        public void SaveIdToken(MsalIdTokenCacheItem item)
        {
            string key = item.GetKey().ToString();
            _idTokenCacheDictionary.AddOrUpdate(key, item, (k, oldValue) => item);
        }

        public void SaveAccount(MsalAccountCacheItem item)
        {
            string key = item.GetKey().ToString();
            _accountCacheDictionary.AddOrUpdate(key, item, (k, oldValue) => item);
        }

        public void SaveAppMetadata(MsalAppMetadataCacheItem item)
        {
            string key = item.GetKey().ToString();
            _appMetadataDictionary.AddOrUpdate(key, item, (k, oldValue) => item);
        }
        #endregion

        #region Get
        public MsalAccessTokenCacheItem GetAccessToken(MsalAccessTokenCacheKey accessTokenKey)
        {
            _accessTokenCacheDictionary.TryGetValue(accessTokenKey.ToString(), out MsalAccessTokenCacheItem cacheItem);
            return cacheItem;
        }

        public MsalRefreshTokenCacheItem GetRefreshToken(MsalRefreshTokenCacheKey refreshTokenKey)
        {
            _refreshTokenCacheDictionary.TryGetValue(refreshTokenKey.ToString(), out var cacheItem);
            return cacheItem;
        }

        public MsalIdTokenCacheItem GetIdToken(MsalIdTokenCacheKey idTokenKey)
        {
            _idTokenCacheDictionary.TryGetValue(idTokenKey.ToString(), out var cacheItem);
            return cacheItem;
        }

        public MsalAccountCacheItem GetAccount(MsalAccountCacheKey accountKey)
        {
            _accountCacheDictionary.TryGetValue(accountKey.ToString(), out var cacheItem);
            return cacheItem;
        }

        public MsalAppMetadataCacheItem GetAppMetadata(MsalAppMetadataCacheKey appMetadataKey)
        {
            _appMetadataDictionary.TryGetValue(appMetadataKey.ToString(), out var cacheItem);
            return cacheItem;
        }
        #endregion

        #region Delete
        public void DeleteAccessToken(MsalAccessTokenCacheKey cacheKey)
        {
            if (!_accessTokenCacheDictionary.TryRemove(cacheKey.ToString(), out _))
            {
                _logger.InfoPii(
                    $"Cannot delete an access token because it was already deleted. Key {cacheKey}",
                    "Cannot delete an access token because it was already deleted");
            }
        }

        public void DeleteRefreshToken(MsalRefreshTokenCacheKey cacheKey)
        {
            if (!_refreshTokenCacheDictionary.TryRemove(cacheKey.ToString(), out _))
            {
                _logger.InfoPii(
                    $"Cannot delete an refresh token because it was already deleted. Key {cacheKey}",
                    "Cannot delete an refresh token because it was already deleted");
            }
        }

        public void DeleteIdToken(MsalIdTokenCacheKey cacheKey)
        {
            if (!_idTokenCacheDictionary.TryRemove(cacheKey.ToString(), out _))
            {
                _logger.InfoPii(
                    $"Cannot delete an id token because it was already deleted. Key {cacheKey}",
                    "Cannot delete an id token because it was already deleted");
            }
        }

        public void DeleteAccount(MsalAccountCacheKey cacheKey)
        {
            if (!_accountCacheDictionary.TryRemove(cacheKey.ToString(), out _))
            {
                _logger.InfoPii(
                    $"Cannot delete an account because it was already deleted. Key {cacheKey}",
                    "Cannot delete an account because it was already deleted");
            }
        }

        #endregion

        #region Get All Values
        public IEnumerable<MsalAccessTokenCacheItem> GetAllAccessTokens()
        {
            return new ReadOnlyCollection<MsalAccessTokenCacheItem>(
                _accessTokenCacheDictionary.Values.ToList());
        }

        public IEnumerable<MsalRefreshTokenCacheItem> GetAllRefreshTokens()
        {
            return new ReadOnlyCollection<MsalRefreshTokenCacheItem>(
                _refreshTokenCacheDictionary.Values.ToList());
        }

        public IEnumerable<MsalIdTokenCacheItem> GetAllIdTokens()
        {
            return new ReadOnlyCollection<MsalIdTokenCacheItem>(
                _idTokenCacheDictionary.Values.ToList());
        }

        public IEnumerable<MsalAccountCacheItem> GetAllAccounts()
        {
            return new ReadOnlyCollection<MsalAccountCacheItem>(
                _accountCacheDictionary.Values.ToList());
        }

        public IEnumerable<MsalAppMetadataCacheItem> GetAllAppMetadata()
        {
            return new ReadOnlyCollection<MsalAppMetadataCacheItem>(
               _appMetadataDictionary.Values.ToList());
        }
        #endregion

        public void SetiOSKeychainSecurityGroup(string keychainSecurityGroup)
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            _accessTokenCacheDictionary.Clear();
            _refreshTokenCacheDictionary.Clear();
            _idTokenCacheDictionary.Clear();
            _accountCacheDictionary.Clear();
            // app metadata isn't removable
        }


    }
}
