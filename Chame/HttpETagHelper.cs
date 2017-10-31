using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Chame
{
    public static class HttpETagHelper
    {
        /// <summary>
        /// Tries to parse HTTP ETag from the request.
        /// </summary>
        /// <param name="request">request</param>
        /// <param name="value">parse HTTP ETag</param>
        /// <returns>true if succeed</returns>
        public static bool TryParse(HttpRequest request, out string value)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            value = null;
            if (request.Headers.ContainsKey("If-None-Match"))
            {
                value = request.Headers["If-None-Match"].FirstOrDefault();
            }
            return !string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Uses specified HTTP ETag with the response.
        /// </summary>
        /// <param name="response">response</param>
        /// <param name="value">HTTP ETag</param>
        public static void Use(HttpResponse response, string value)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("", nameof(value));
            }

            response.Headers.Add("ETag", new StringValues(value));
        }

        /// <summary>
        /// Calculates a HTTP ETag.
        /// </summary>
        /// <param name="data">data</param>
        /// <returns>HTTP ETag</returns>
        public static string Calculate(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(data);
                var buffer = new StringBuilder();
                foreach (var b in hash)
                {
                    buffer.AppendFormat("{0:X2}", b);
                }
                return buffer.ToString();
            }
        }
    }
}
