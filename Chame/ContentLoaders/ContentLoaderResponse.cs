using System;
using System.Collections.Generic;

namespace Chame.ContentLoaders
{
    public class ContentLoaderResponse : FileContent
    {
        /// <summary>
        /// Response status
        /// </summary>
        public virtual ResponseStatus Status { get; set; }

        public static ContentLoaderResponse NotFound()
        {
            return new ContentLoaderResponse { Status = ResponseStatus.NotFound };
        }

        public static ContentLoaderResponse NotModified()
        {
            return new ContentLoaderResponse { Status = ResponseStatus.NotModified };
        }

        public static ContentLoaderResponse Ok(FileContent content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            return new ContentLoaderResponse { Status = ResponseStatus.Ok, Data = content.Data, ETag = content.ETag };
        }

        /// <summary>
        /// Makes a bundle
        /// </summary>
        public static ContentLoaderResponse Bundle(IEnumerable<ContentLoaderResponse> responses)
        {
            if (responses == null)
            {
                throw new ArgumentNullException(nameof(responses));
            }

            var data = new List<byte>();

            foreach (var response in responses)
            {
                if (response.Data != null)
                {
                    data.AddRange(response.Data);
                }
            }

            var content = new FileContent { Data = data.ToArray() };

            return Ok(content);
        }

        public static ContentLoaderResponse CreateResponse(FileContent content, ContentLoadingContext context, ContentLoaderOptions options)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (content == null)
            {
                return NotFound();
            }

            if (options.SupportETag && context.ETag != null && content.ETag != null && context.ETag == content.ETag)
            {
                return NotModified();
            }

            return Ok(content);
        }

    }
}
