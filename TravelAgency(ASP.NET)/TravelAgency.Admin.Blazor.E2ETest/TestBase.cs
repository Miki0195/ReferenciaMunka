using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace ELTE.TravelAgency.Admin.Blazor.E2ETest
{
    public abstract class TestBase : PageTest
    {
        private const string BaseUrl = "https://localhost:7087";

        public override BrowserNewContextOptions ContextOptions()
        {
            return new BrowserNewContextOptions()
            {
                ViewportSize = new()
                {
                    Width = 1920,
                    Height = 1080
                },
                BaseURL = BaseUrl,
                RecordVideoDir = "videos",
                RecordVideoSize = new RecordVideoSize { Width = 1024, Height = 768 }
            };
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
        }
    }

}