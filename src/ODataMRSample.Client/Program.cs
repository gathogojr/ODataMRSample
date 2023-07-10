using Microsoft.OData;
using Microsoft.OData.Client;
using ODataMRSample.Models;

namespace ODataMRSample.Models
{
    public partial class Asset
    {
        public string? Description { get; set; }
        public string? Color { get; set; }
    }
}

namespace ODataMRSample.Client
{
    internal class Program
    {
        static readonly Uri serviceRoot = new Uri("http://localhost:5000");

        static void Main(string[] args)
        {
            UpdateMLE();
        }

        static void CreateMLE()
        {
            var dataServiceContext = new Default.Container(serviceRoot);

            var asset = new Asset
            {
                Description = "Square",
                Color = "Black"
            };

            dataServiceContext.AddToAssets(asset);

            var fileName = "black.png";
            var path = $"..\\..\\..\\Resources\\{fileName}";

            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var requestArgs = new DataServiceRequestArgs();
                requestArgs.ContentType = "image/png";
                requestArgs.Headers.Add("Content-Disposition", $"inline;filename={fileName}");

                dataServiceContext.SetSaveStream(asset, fileStream, closeStream: false, args: requestArgs);
                dataServiceContext.BuildingRequest += DataServiceContext_BuildingRequest;

                // The response for the POST request contains the Asset MLE
                // Retrieve the Id property value and apply it to the backing property on the Asset object
                dataServiceContext.Configurations.ResponsePipeline.OnEntryStarted((args) =>
                {
                    if (args.Entry.TypeName == typeof(Asset).FullName)
                    {
                        var idProperty = args.Entry.Properties.Single(p => p.Name == "Id");
                        asset.Id = idProperty.Value.ToString();
                    }
                });

                dataServiceContext.SaveChanges();
            }
        }

        static void GetMLE()
        {
            var dataServiceContext = new Default.Container(serviceRoot);

            var asset = dataServiceContext.Assets.ByKey("47bc49a").GetValue();

            dataServiceContext.BuildingRequest += DataServiceContext_BuildingRequest;
            var dataServiceStreamResponse = dataServiceContext.GetReadStream(asset);

            var fileName = "black.png";
            var path = $"..\\..\\..\\Store\\{fileName}";

            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                dataServiceStreamResponse.Stream.CopyTo(fileStream);
            }
        }

        static void UpdateMLE()
        {
            var dataServiceContext = new Default.Container(serviceRoot);

            var asset = dataServiceContext.Assets.ByKey("47bc49a").GetValue();

            dataServiceContext.BuildingRequest += DataServiceContext_BuildingRequest;

            asset.Color = "Purple";
            dataServiceContext.UpdateObject(asset);

            var fileName = "purple.png";
            var path = $"..\\..\\..\\Resources\\{fileName}";

            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var requestArgs = new DataServiceRequestArgs();
                requestArgs.ContentType = "image/png";
                requestArgs.Headers.Add("Content-Disposition", $"inline;filename={fileName}");

                dataServiceContext.SetSaveStream(asset, fileStream, closeStream: false, args: requestArgs);

                dataServiceContext.SaveChanges();
            }
        }



        private static void DataServiceContext_BuildingRequest(object? sender, BuildingRequestEventArgs e)
        {
            
        }
    }
}