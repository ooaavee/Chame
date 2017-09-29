using System;
using System.Text;
using System.Threading.Tasks;

namespace Chame
{
    public class ResponseContent
    {
        public static readonly Encoding DefaultEncoding = Encoding.UTF8;

        /// <summary>
        /// Response content
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Encoding for response content
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// HTTP ETag for response content (optional)
        /// </summary>
        public string ETag { get; set; }

        /// <summary>
        /// Response status
        /// </summary>
        public ResponseContentStatus Status { get; set; }

        /// <summary>
        /// Creates a new 'NotFound' response content.
        /// </summary>
        /// <returns></returns>
        public static ResponseContent NotFound()
        {
            return new ResponseContent { Status = ResponseContentStatus.NotFound };
        }

        /// <summary>
        /// Creates a new 'NotModified' response content.
        /// </summary>
        public static ResponseContent NotModified()
        {
            return new ResponseContent { Status = ResponseContentStatus.NotModified };
        }

        /// <summary>
        /// Creates a new 'Ok' response content.
        /// </summary>
        public static ResponseContent Ok(string content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            return new ResponseContent { Status = ResponseContentStatus.OK, Content = content, Encoding = DefaultEncoding };
        }

        /// <summary>
        /// Creates a new 'Ok' response content.
        /// </summary>
        public static ResponseContent Ok(string content, Encoding encoding)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            return new ResponseContent { Status = ResponseContentStatus.OK, Content = content, Encoding = encoding };
        }

        /// <summary>
        /// Creates a new 'Ok' response content.
        /// </summary>
        public static ResponseContent Ok(string content, Encoding encoding, string eTag)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            if (eTag == null)
            {
                throw new ArgumentNullException(nameof(eTag));
            }

            return new ResponseContent { Status = ResponseContentStatus.OK, Content = content, Encoding = encoding, ETag = eTag };
        }

        /// <summary>
        /// Creates a new 'Ok' response content.
        /// </summary>
        public static ResponseContent Ok(string content, string eTag)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (eTag == null)
            {
                throw new ArgumentNullException(nameof(eTag));
            }

            return new ResponseContent { Status = ResponseContentStatus.OK, Content = content, ETag = eTag };
        }

        public Task<ResponseContent> AsTask()
        {
            return Task.FromResult(this);
        }
    }
}
