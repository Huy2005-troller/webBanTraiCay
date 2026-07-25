using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Fruitables.Helpers;

public class VnPayLibrary
{
    private readonly SortedList<string, string> _requestData = new(new VnPayCompare());
    private readonly SortedList<string, string> _responseData = new(new VnPayCompare());

    public void AddRequestData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _requestData.Add(key, value);
        }
    }

    public void AddResponseData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _responseData.Add(key, value);
        }
    }

    public string GetResponseData(string key)
    {
        return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
    }

    #region Request

    public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
    {
        // VNPAY ký chính chuỗi query đã URL-encode, sau khi sắp xếp tên tham số.
        var data = new StringBuilder();
        foreach (var kv in _requestData)
        {
            if (!string.IsNullOrEmpty(kv.Value))
            {
                data.Append(WebUtility.UrlEncode(kv.Key));
                data.Append('=');
                data.Append(WebUtility.UrlEncode(kv.Value));
                data.Append('&');
            }
        }

        var queryString = data.ToString().TrimEnd('&');
        var signData = queryString;

        var vnpSecureHash = HmacSHA512(vnpHashSecret, signData);

        return baseUrl + "?" + queryString + "&vnp_SecureHash=" + vnpSecureHash;
    }

    #endregion

    #region Response

    public bool ValidateSignature(string inputHash, string secretKey)
    {
        string rspRaw = GetResponseRawData();
        string myChecksum = HmacSHA512(secretKey, rspRaw);
        return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
    }

    private string GetResponseRawData()
    {
        var data = new StringBuilder();
        // Loại bỏ các field hash khỏi dữ liệu verify
        if (_responseData.ContainsKey("vnp_SecureHashType"))
            _responseData.Remove("vnp_SecureHashType");
        if (_responseData.ContainsKey("vnp_SecureHash"))
            _responseData.Remove("vnp_SecureHash");

        foreach (var kv in _responseData)
        {
            if (!string.IsNullOrEmpty(kv.Value))
            {
                data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
            }
        }
        if (data.Length > 0)
            data.Length--;

        return data.ToString();
    }

    #endregion

    private static string HmacSHA512(string key, string inputData)
    {
        var hash = new StringBuilder();
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
        using (var hmac = new HMACSHA512(keyBytes))
        {
            byte[] hashValue = hmac.ComputeHash(inputBytes);
            foreach (var theByte in hashValue)
            {
                hash.Append(theByte.ToString("x2"));
            }
        }
        return hash.ToString();
    }

    public static string GetIpAddress(HttpContext context)
    {
        try
        {
            var remoteIpAddress = context.Connection.RemoteIpAddress;
            if (remoteIpAddress == null)
                return "127.0.0.1";

            // Localhost / IPv6 (::1) → dùng IPv4 cố định, tránh lỗi ký với %3A%3A1
            if (IPAddress.IsLoopback(remoteIpAddress))
                return "127.0.0.1";

            if (remoteIpAddress.IsIPv4MappedToIPv6)
                remoteIpAddress = remoteIpAddress.MapToIPv4();

            if (remoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                remoteIpAddress = Dns.GetHostEntry(remoteIpAddress).AddressList
                    .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
            }

            if (remoteIpAddress != null && remoteIpAddress.AddressFamily == AddressFamily.InterNetwork)
                return remoteIpAddress.ToString();
        }
        catch
        {
            // ignored
        }
        return "127.0.0.1";
    }
}

public class VnPayCompare : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        if (x == y) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        var vnpCompare = CompareInfo.GetCompareInfo("en-US");
        return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
    }
}
