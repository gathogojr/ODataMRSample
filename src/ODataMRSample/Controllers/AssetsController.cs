namespace ODataMRSample.Controllers
{
    using System.Net;
    using System.Net.Mime;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.OData.Deltas;
    using Microsoft.AspNetCore.OData.Routing.Controllers;
    using Microsoft.Extensions.Primitives;
    using ODataMRSample.Models;

    public class AssetsController : ODataController
    {
        private const string ContentTypeHeader = "Content-Type";
        private const string ContentDispositionHeader = "Content-Disposition";
        private static readonly List<Asset> assets = new List<Asset>();

        public async Task<ActionResult> Post()
        {
            var assetId = Guid.NewGuid().ToString("N").Substring(0, 7);
            var contentType = GetContentType();

            (string? contentDisposition, string? fileName) = GetContentDisposition();

            fileName = $"Media\\{fileName ?? assetId}";

            using (var fileStream = new FileStream(fileName, FileMode.Create))
            {
                await Request.Body.CopyToAsync(fileStream);
            }

            var asset = new Asset { Id = assetId, Path = fileName };

            asset.Properties.Add(ContentTypeHeader, contentType);
            asset.Properties.Add(ContentDispositionHeader, contentDisposition);

            assets.Add(asset);

            return Created(asset);
        }

        [HttpGet("Assets({key})/$value")]
        public async Task GetMediaResourceAsync(string key)
        {
            var asset = assets.SingleOrDefault(d => d.Id == key);

            if (asset == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            Response.StatusCode = (int)HttpStatusCode.OK;
            Response.Headers.Add(ContentTypeHeader, asset.Properties[ContentTypeHeader] as string);
            Response.Headers.Add(ContentDispositionHeader, asset.Properties[ContentDispositionHeader] as string);

            using (var fileStream = new FileStream(asset.Path, FileMode.Open))
            {
                await fileStream.CopyToAsync(Response.Body);
            }
        }

        public ActionResult Get(string key)
        {
            var asset = assets.SingleOrDefault(d => d.Id == key);

            if (asset == null)
            {
                return NotFound();
            }

            return Ok(asset);
        }

        public ActionResult Patch(string key, [FromBody] Delta<Asset> delta)
        {
            var asset = assets.SingleOrDefault(d => d.Id == key);

            if (asset == null)
            {
                return NotFound();
            }

            delta.Patch(asset);

            return Ok();
        }

        [HttpPut("Assets({key})/$value")]
        public async Task<ActionResult> SetMediaResourceAsync(string key)
        {
            var asset = assets.SingleOrDefault(d => d.Id == key);
            if (asset == null)
            {
                return NotFound();
            }

            var contentType = GetContentType();
            (string? contentDisposition, string? fileName) = GetContentDisposition();

            if (contentType == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return BadRequest();
            }

            fileName = $"Media\\{fileName ?? asset.Id}";

            using (var fileStream = new FileStream(fileName, FileMode.Create))
            {
                await Request.Body.CopyToAsync(fileStream);
            }

            asset.Path = fileName;
            asset.Properties[ContentTypeHeader] = contentType;
            asset.Properties[ContentDispositionHeader] = contentDisposition;        

            return Ok();
        }

        public string? GetContentType()
        {
            if (!string.IsNullOrEmpty(Request.ContentType))
            {
                return Request.ContentType;
            }

            if (Request.Headers.TryGetValue(ContentTypeHeader, out StringValues contentType))
            {
                return contentType.ToString();
            }

            return null;
        }

        public (string? ContentDisposition, string? FileName) GetContentDisposition()
        {
            if (Request.Headers.TryGetValue(ContentDispositionHeader, out StringValues contentDispositionHeader))
            {
                string contentDispositionHeaderValue = contentDispositionHeader.ToString();
                string? fileName = null;

                if (!string.IsNullOrEmpty(contentDispositionHeaderValue))
                {
                    ContentDisposition contentDisposition = new ContentDisposition(contentDispositionHeaderValue);
                    fileName = contentDisposition.FileName;
                }

                return (contentDispositionHeaderValue, fileName);
            }

            return (null, null);
        }
    }
}
