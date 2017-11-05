namespace Chame.ContentLoaders
{
    //
    // TODO: T‰st‰ julkinen luokka --> Ja tehd‰‰ siit‰ Content-luokan kantaluokka (ja sille annetaan uusi nimi tyyliin ContentLoaderResponse)
    //
    public class FileContent
    {
        /// <summary>
        /// Content bytes
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// HTTP ETag for content
        /// </summary>
        public string ETag { get; set; }


        ////
        //// TODO: Siir‰ t‰m‰ FileContent-luokkaan, koska k‰ytet‰‰n kahdesta palvelusta
        ////
        //private Content GetContent(ContentLoadingContext context, bool supportETag)
        //{
        //    Content content;

        //    FileContent fileContent = this;

        //    if (fileContent != null)
        //    {
        //        if (_options1.SupportETag && context.ETag != null && fileContent.ETag != null && context.ETag == fileContent.ETag)
        //        {
        //            content = Content.NotModified();
        //        }
        //        else
        //        {
        //            if (fileContent.ETag != null)
        //            {
        //                content = Content.Ok(fileContent.Data, fileContent.ETag);
        //            }
        //            else
        //            {
        //                content = Content.Ok(fileContent.Data);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        content = Content.NotFound();
        //    }

        //    return content;
        //}

    }
}