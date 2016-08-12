﻿using System.Collections.Generic;
using MvvmCross.Platform;

namespace CodeHub.Core.Data
{
    public class Account
    {
        public string Id => Username + Domain;

        public string OAuth { get; set; }

        public string Password { get; set; }

        public string Domain { get; set; }

        public string WebDomain { get; set; }

        public bool IsEnterprise { get; set; }

        public bool ShowOrganizationsInEvents { get; set; }

        public bool ExpandOrganizations { get; set; }

        public bool ShowRepositoryDescriptionInList { get; set; }

        public bool? IsPushNotificationsEnabled { get; set; }

        public string Username { get; set; }

        public string AvatarUrl { get; set; }

        public string DefaultStartupView { get; set; }

        private List<PinnedRepository> _pinnedRepositories = new List<PinnedRepository>();
        public List<PinnedRepository> PinnedRepositories
        {
            get { return _pinnedRepositories ?? new List<PinnedRepository>(); }
            set { _pinnedRepositories = value ?? new List<PinnedRepository>(); }
        }

        private Dictionary<string, Filter> _filters = new Dictionary<string, Filter>();
        public Dictionary<string, Filter> Filters
        {
            get { return _filters ?? new Dictionary<string, Filter>(); }
            set { _filters = value ?? new Dictionary<string, Filter>(); }
        }
    }

    public static class AccountExtensions
    {
        public static T GetFilter<T>(this Account account, string key) where T : class, new()
        {
            Filter filter = null;
            if (account.Filters?.TryGetValue(key, out filter) == false)
                return default(T);
            return filter?.GetData<T>() ?? new T();
        }

        public static void SetFilter(this Account account, string key, object filter)
        {
            var f = new Filter();
            f.SetData(filter);
            if (account.Filters == null)
                account.Filters = new Dictionary<string, Filter>();
            account.Filters[key] = f;
        }
    }

    public class PinnedRepository
    {
        public string Owner { get; set; }

        public string Slug { get; set; }

        public string Name { get; set; }

        public string ImageUri { get; set; }
    }

    public class Filter
    {
        public string RawData { get; set; }
    }

    public static class FilterExtensions
    {
        public static T GetData<T>(this Filter filter) where T : new()
        {
            try
            {
                var serializer = Mvx.Resolve<Services.IJsonSerializationService>();
                return serializer.Deserialize<T>(filter.RawData);
            }
            catch
            {
                return default(T);
            }
        }

        public static void SetData(this Filter filter, object o)
        {
            var serializer = Mvx.Resolve<Services.IJsonSerializationService>();
            filter.RawData = serializer.Serialize(o);
        }
    }
}

