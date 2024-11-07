using System.Collections.Generic;

public class HttpsUtils
{
    public const string DNS_URL = "http://121.5.33.171:8080";
    private static string token = "eyJhbGciOiJIUzUxMiJ9.eyJsb2dpbl91c2VyX2tleSI6IjhiNDZjZTIzLWM1MTItNGRhYi04ZDZjLTg3NjRlOGQwMWVkMyJ9.rMGfj8WKY1FsV5ubSdjxC8W2RADWsu0FO7wIBYaUlO1mY90uV13s1AXjD6N_V3ypnMogRH4lqv4FzAH-C4fqIg";
    private static Dictionary<string, string> HEADERS = new Dictionary<string, string>() {
        
    };

    private static Dictionary<string, string> PARAMZ = new Dictionary<string, string>()
    {

    };

    public static void SetToken(string resToken)
    {
        token = resToken;
    }

    public static void SetGeneralHeaders(Dictionary<string, string> headers)
    {
        if(!headers.ContainsKey("AppAuthorization"))
        {
            headers.Add("AppAuthorization", token);
        }          
        foreach (KeyValuePair<string, string> kv in HEADERS)
        {
            headers.Add(kv.Key, kv.Value);
        }
    }

    public static void SetGeneralParams(Dictionary<string, string> paramz)
    {
        foreach (KeyValuePair<string, string> kv in PARAMZ)
        {
            paramz.Add(kv.Key, kv.Value);
        }
    }


}
