using CodeHub.Core.Services;
using CodeFramework.Core.Services;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Foundation;

namespace CodeHub.iOS.Services
{
    public class FeaturesService : IFeaturesService
    {
        private readonly IDefaultValueService _defaultValueService;
        private readonly IHttpClientService _httpClientService;
        private readonly IJsonSerializationService _jsonSerializationService;


        public FeaturesService(IDefaultValueService defaultValueService, IHttpClientService httpClientService, IJsonSerializationService jsonSerializationService)
        {
            _defaultValueService = defaultValueService;
            _httpClientService = httpClientService;
            _jsonSerializationService = jsonSerializationService;
        }

        public bool IsPushNotificationsActivated
        {
            get
            {
                return IsActivated(FeatureIds.PushNotifications);
            }
            set
            {
                _defaultValueService.Set(FeatureIds.PushNotifications, value);
            }
        }

        public bool IsEnterpriseSupportActivated
        {
            get
            {
                return IsActivated(FeatureIds.EnterpriseSupport);
            }
            set
            {
                _defaultValueService.Set(FeatureIds.EnterpriseSupport, value);
            }
        }

        public void Activate(string id)
        {
            InAppPurchases.Instance.PurchaseProduct(id);
        }

        public bool IsActivated(string id)
        {
            bool value;
            return _defaultValueService.TryGet<bool>(id, out value) && value;
        }

        public async Task<IEnumerable<string>> GetAvailableFeatureIds()
        {
            var ids = new List<string>();
            ids.Add(FeatureIds.EnterpriseSupport);
            var client = _httpClientService.Create();
            client.Timeout = new TimeSpan(0, 0, 15);
			var response = await client.GetAsync("http://push.codehub-app.com/in-app?version=" + NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"].ToString());
            var data = await response.Content.ReadAsStringAsync();
            ids.AddRange(_jsonSerializationService.Deserialize<List<string>>(data));
            return ids;
        }
    }
}

