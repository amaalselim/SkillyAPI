using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Skilly.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Vonage.Conversations;

namespace Skilly.Infrastructure.Implementation
{
    public class PaymobService
    {
        private readonly PaymobSettings _settings;
        private readonly HttpClient _httpClient;
        private string _authToken;
        public PaymobService(IOptions<PaymobSettings> options)
        {
            _settings = options.Value;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://accept.paymob.com/api/")
            };
        }

        public async Task<string> GetAuthTokenAsync()
        {
            if (!string.IsNullOrWhiteSpace(_authToken))
                return _authToken;

            var payload = new
            {
                api_key = _settings.ApiKey
            };

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("auth/tokens", content);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            _authToken = result.GetProperty("token").GetString();
            return _authToken;
        }

        public async Task<int> CreateOrderAsync(string authToken, decimal amountCents)
        {
            var payload = new
            {
                auth_token = authToken,
                delivery_needed = false,
                amount_cents =(amountCents),
                currency = "EGP",
                items = new object[] { }
            };
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("ecommerce/orders", content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            var orderId = result.GetProperty("id").GetInt32();
            return orderId;
        }
        public async Task<string> CreatePaymentKeyAsync(string authToken, int orderId, decimal amountCents,UserProfile userProfile, string redirectUrl)
        {

            var payload = new
            {
                auth_token = authToken,
                amount_cents = (int)(amountCents * 100),
                currency = "EGP",
                integration_id = _settings.CardIntegrationId,
                order_id = orderId,

                billing_data = new
                {
                    apartment = "NA",
                    email = userProfile.Email,
                    floor = "NA",
                    first_name = userProfile.FirstName,
                    street = "NA",
                    building = "NA",
                    phone_number = userProfile.PhoneNumber,
                    shipping_method = "PKG",
                    postal_code = "NA",
                    city = userProfile.City ?? "Cairo",
                    country = userProfile.Governorate,
                    last_name = userProfile.LastName,
                    state = userProfile.StreetName ?? "Cairo"
                },
                redirect_url = redirectUrl
            };

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("acceptance/payment_keys", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error: {response.StatusCode}, Body: {responseBody}");
            }

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            var paymentKey = result.GetProperty("token").GetString();
            return paymentKey;
        }

        public async Task<string> CreatePaymentKeyAsync2(string authToken, int orderId, decimal amountCents, UserProfile userProfile)
        {

            var payload = new
            {
                auth_token = authToken,
                amount_cents = (int)(amountCents * 100),
                currency = "EGP",
                integration_id = _settings.CardIntegrationId,
                order_id = orderId,

                billing_data = new
                {
                    apartment = "NA",
                    email = userProfile.Email,
                    floor = "NA",
                    first_name = userProfile.FirstName,
                    street = "NA",
                    building = "NA",
                    phone_number = userProfile.PhoneNumber,
                    shipping_method = "PKG",
                    postal_code = "NA",
                    city = userProfile.City ?? "Cairo",
                    country = userProfile.Governorate,
                    last_name = userProfile.LastName,
                    state = userProfile.StreetName ?? "Cairo"
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("acceptance/payment_keys", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error: {response.StatusCode}, Body: {responseBody}");
            }

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            var paymentKey = result.GetProperty("token").GetString();
            return paymentKey;
        }


        public string IframeId => _settings.IframeId;
        public bool ValidateHmac(string rawBody, string receivedHmac)
        {
            var secret = _settings.HmacSecret; // خديه من إعداداتك

            // استخدمي HMACSHA512 لو باي موب بيستخدموه (ممكن تجربي HMACSHA256 لو مش متأكدة)
            using var hmac = new System.Security.Cryptography.HMACSHA512(Encoding.UTF8.GetBytes(secret));

            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawBody));
            var generatedHmac = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            Console.WriteLine("Generated HMAC: " + generatedHmac);

            return generatedHmac == receivedHmac?.ToLower();
        }

    }
}
