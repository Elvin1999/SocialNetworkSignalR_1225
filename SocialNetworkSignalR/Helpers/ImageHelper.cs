namespace SocialNetworkSignalR.Helpers
{
    public class ImageHelper
    {
        private readonly IWebHostEnvironment _webHost;

        public ImageHelper(IWebHostEnvironment webHost)
        {
            _webHost = webHost;
        }

        public async Task<string> SaveFile(IFormFile file)
        {
            var saveImg = Path.Combine(_webHost.WebRootPath, "images", file.FileName);
            using (var img=new FileStream(saveImg,FileMode.OpenOrCreate))
            {
                await file.CopyToAsync(img);
            }
            return file.FileName.ToString();
        }
    }
}
