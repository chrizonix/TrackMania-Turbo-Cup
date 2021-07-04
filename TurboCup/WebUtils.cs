using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TurboCup {
public class WebUtils {

    public static async Task SendRequestAsync(String url, String username, String password) {
        // Start Async Task for Web Request
        var task = Task.Run(() => SendRequest(url, username, password));

        // Run Communication Task for max 2 Seconds
        if (await Task.WhenAny(task, Task.Delay(2 * 1000)) == task) {
            // Peaceful Disconnect
        } else {
            // Client Timeout
        }
    }

    public static HttpStatusCode SendRequest(String url, String username, String password) {
        // Default Http Status Code
        HttpStatusCode status = HttpStatusCode.OK;

        // Create a Request for the URL
        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);

        // Basic Apache Authentication
        String authInfo = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));

        // Set Credentials
        request.Headers["Authorization"] = "Basic " + authInfo;
        request.Credentials = GetCredential(url, username, password);
        request.PreAuthenticate = true;

        try {
            // Get the Response
            WebResponse response = request.GetResponse();

            // Get Http Status Code
            status = ((HttpWebResponse)response).StatusCode;

            // Get the stream containing content returned by the server.
            // The using block ensures the stream is automatically closed.
            using (Stream dataStream = response.GetResponseStream()) {
                // Open the Stream using a StreamReader for Easy Access
                StreamReader reader = new StreamReader(dataStream);
                // Read Single Line
                string responseFromServer = reader.ReadLine();
                // Display the Content
                Console.WriteLine(responseFromServer);
            }

            // Close the Response
            response.Close();
        } catch (WebException e) {
            // Get Http Status Code
            status = ((HttpWebResponse)e.Response).StatusCode;
        }

        // Http Status Code
        return status;
    }

    private static CredentialCache GetCredential(String url, String username, String password) {
        // Create New Credential Cache
        CredentialCache credentialCache = new CredentialCache();
        credentialCache.Add(new System.Uri(url), "Basic", new NetworkCredential(username, password));

        // Return Credential Cache
        return credentialCache;
    }
}
}
