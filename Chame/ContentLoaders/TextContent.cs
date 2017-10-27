using System;
using System.Collections.Generic;
using System.Text;

namespace Chame.ContentLoaders
{
    public class TextContent
    {
        private static readonly Encoding DefaultEncoding = Encoding.UTF8;

        /// <summary>
        /// Text content
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Encoding for content
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// HTTP ETag for content (optional)
        /// </summary>
        public string ETag { get; set; }

        /// <summary>
        /// Response status
        /// </summary>
        public ResponseStatus Status { get; set; }

        /// <summary>
        /// Creates a new 'NotFound' TextContent object.
        /// </summary>
        /// <returns></returns>
        public static TextContent NotFound()
        {
            return new TextContent { Status = ResponseStatus.NotFound };
        }

        /// <summary>
        /// Creates a new 'NotModified' TextContent object.
        /// </summary>
        public static TextContent NotModified()
        {
            return new TextContent { Status = ResponseStatus.NotModified };
        }

        /// <summary>
        /// Creates a new 'Ok' TextContent object.
        /// </summary>
        public static TextContent Ok(string content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            return new TextContent { Status = ResponseStatus.Ok, Value = content, Encoding = DefaultEncoding };
        }

        /// <summary>
        /// Creates a new 'Ok' TextContent object.
        /// </summary>
        public static TextContent Ok(string content, Encoding encoding)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            return new TextContent { Status = ResponseStatus.Ok, Value = content, Encoding = encoding };
        }

        /// <summary>
        /// Creates a new 'Ok' TextContent object.
        /// </summary>
        public static TextContent Ok(string content, Encoding encoding, string eTag)
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

            return new TextContent { Status = ResponseStatus.Ok, Value = content, Encoding = encoding, ETag = eTag };
        }

        /// <summary>
        /// Creates a new 'Ok' TextContent object.
        /// </summary>
        public static TextContent Ok(string content, string eTag)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (eTag == null)
            {
                throw new ArgumentNullException(nameof(eTag));
            }

            return new TextContent { Status = ResponseStatus.Ok, Value = content, ETag = eTag };
        }

        /// <summary>
        /// Combines multiple TextContent objects.
        /// </summary>
        public static TextContent Combine(IEnumerable<TextContent> elements)
        {
            if (elements == null)
            {
                throw new ArgumentNullException(nameof(elements));
            }

            var content = new TextContent { Encoding = null, ETag = null, Status = ResponseStatus.Ok };
            var buf = new StringBuilder();

            foreach (TextContent element in elements)
            {
                if (content.Encoding != null && content.Encoding.EncodingName != element.Encoding.EncodingName)
                {
                    throw new InvalidOperationException(string.Format("Failed to combine ResponseContent object because multiple encondings were used ({0} and {1}).", content.Encoding.EncodingName, element.Encoding.EncodingName));
                }

                buf.Append(element.Value);
                content.Encoding = element.Encoding;
            }

            content.Value = buf.ToString();
            return content;
        }

    }
}
