using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Chame.ContentLoaders
{
    public class Content
    {
        /// <summary>
        /// Content bytes
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// HTTP ETag for content (optional)
        /// </summary>
        public string ETag { get; set; }

        /// <summary>
        /// Response status
        /// </summary>
        public ResponseStatus Status { get; set; }

        public static Content NotFound()
        {
            return new Content {Status = ResponseStatus.NotFound};
        }

        public static Content NotModified()
        {
            return new Content {Status = ResponseStatus.NotModified};
        }

        public static Content Ok(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return new Content {Status = ResponseStatus.Ok, Data = data};
        }

        public static Content Ok(byte[] data, string eTag)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (eTag == null)
            {
                throw new ArgumentNullException(nameof(eTag));
            }

            return new Content {Status = ResponseStatus.Ok, Data = data, ETag = eTag};
        }

        /// <summary>
        /// Combines multiple objects.
        /// </summary>
        public static Content Combine(IEnumerable<Content> elements)
        {
            if (elements == null)
            {
                throw new ArgumentNullException(nameof(elements));
            }

            List<byte> data = new List<byte>();

            foreach (Content element in elements)
            {
                if (element.Data != null)
                {
                    data.AddRange(element.Data);
                }
            }

            return Ok(data.ToArray());
        }

    }
}
