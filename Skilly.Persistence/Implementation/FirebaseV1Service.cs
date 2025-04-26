//using Google.Apis.Auth.OAuth2;
//using Newtonsoft.Json;
//using System.Net.Http;
//using System.Text;

//public class FirebaseV1Service
//{
//    private readonly string _firebaseProjectId = "grad-a1dc0"; 
//    private readonly string _serviceAccountFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "firebase", "firebase.json");


//    public async Task SendNotificationAsync(string targetFcmToken, string title, string body)
//    {
//        var credential = GoogleCredential.FromFile(_serviceAccountFile)
//            .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");

//        var accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

//        var message = new
//        {
//            message = new
//            {
//                token = targetFcmToken,
//                notification = new
//                {
//                    title = title,
//                    body = body
//                }
//            }
//        };

//        var jsonMessage = JsonConvert.SerializeObject(message);

//        using var httpClient = new HttpClient();
//        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

//        var url = $"https://fcm.googleapis.com/v1/projects/{_firebaseProjectId}/messages:send";

//        var content = new StringContent(jsonMessage, Encoding.UTF8, "application/json");

//        var response = await httpClient.PostAsync(url, content);
//        var result = await response.Content.ReadAsStringAsync();

//        if (!response.IsSuccessStatusCode)
//            throw new Exception($"Firebase error: {result}");
//    }
//}
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

public class FirebaseV1Service
{
    //private readonly string _firebaseProjectId = "skilly-58194";
    //private readonly string _serviceAccountFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "firebase", "firebase.json");

    //public async Task SendNotificationAsync(string targetFcmToken, string title, string body)
    //{
    //    // تحميل بيانات الاعتماد من الملف بدون تحديد نطاق إذا كان ذلك غير ضروري
    //    var credential = GoogleCredential.FromFile(_serviceAccountFile)
    //        .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");

    //    var accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

    //    // إعداد الرسالة ككائن مع البيانات المطلوبة
    //    var message = new
    //    {
    //        token = targetFcmToken,
    //        notification = new
    //        {
    //            title = title,
    //            body = body
    //        }
    //    };

    //    var jsonMessage = JsonConvert.SerializeObject(new { message });

    //    using var httpClient = new HttpClient();
    //    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

    //    // تحديد الرابط الصحيح لإرسال الرسالة
    //    var url = $"https://fcm.googleapis.com/v1/projects/{_firebaseProjectId}/messages:send";

    //    // استخدام StringContent لإرسال البيانات بتنسيق JSON
    //    var content = new StringContent(jsonMessage, Encoding.UTF8, "application/json");

    //    // إرسال الطلب إلى Firebase
    //    var response = await httpClient.PostAsync(url, content);
    //    var result = await response.Content.ReadAsStringAsync();

    //    // التحقق من الاستجابة
    //    if (!response.IsSuccessStatusCode)
    //        throw new Exception($"Firebase error: {result}");
    //}
    public async Task SendNotificationAsync(string deviceToken, string title, string body)
    {
        var message = new Message()
        {
            Token = deviceToken,
            Notification = new Notification()
            {
                Title = title,
                Body = body
            }
        };

        string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        Console.WriteLine($"Successfully sent message: {response}");
    }
}
