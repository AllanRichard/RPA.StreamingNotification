using RestSharp;
using Newtonsoft.Json;
using RPA.StreamingNotification.Models;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System.Net;

namespace RPA.StreamingNotification.Handler
{
    public class RequestHandler
    {
        private RestClient _client { get; set; }
        private ILogger<Worker> _logger { get; set; }
        private IWebDriver _driver;
        private readonly string _urlToken;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _grantType;
        private readonly int _timeOut;

        public RequestHandler(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _urlToken = configuration.GetValue<string>("url_token");
            _clientId = configuration.GetValue<string>("client_id");
            _clientSecret = configuration.GetValue<string>("client_secret");
            _grantType = configuration.GetValue<string>("grant_type");

            _client = new RestClient()
            {
                CookieContainer = new CookieContainer(),
                Timeout = configuration.GetValue<int>("TimeOut"),
                ThrowOnAnyError = true,
            };
        }

        public IRestResponse Get(string url, Dictionary<string, string>? headers, Dictionary<string, string>? parameters)
        {
            var request = new RestRequest(url);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.AddHeader(header.Key, header.Value);
                }
            }

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    request.AddParameter(parameter.Key, parameter.Value);
                }
            }

            var response = _client.Get(request);
            return response;
        }

        public IRestResponse Post(string url, Dictionary<string, string> parameters)
        {
            var request = new RestRequest(url);

            foreach (var parameter in parameters)
            {
                request.AddParameter(parameter.Key, parameter.Value);
            }

            var response = _client.Post(request);
            return response;
        }

        private bool InitChromeDriver()
        {
            try
            {
                var options = new ChromeOptions();

                options.PageLoadStrategy = PageLoadStrategy.Normal;
                options.AddArgument("no-sandbox");
                options.AddArgument("--profile-directory=Default");
                options.AddArguments("--disable-web-security");
                options.AddArguments("--disable-gpu");
                options.AddArguments("--start-maximized");
                options.AddArguments("--ignore-certificate-errors");

                //options.AddArguments("headless");
                options.AddExcludedArgument("enable-logging");

                _driver = new ChromeDriver(options);
                _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);

                return true;
            }
            catch (Exception ex)
            {
                CloseChromeDriver();
                _logger.LogError($"Erro ao iniciar o chrome driver. Ex: {ex.StackTrace}");

                return false;
            }
        }

        private void CloseChromeDriver()
        {
            try
            {
                //Console.Clear();
                _driver.Quit();
            }
            catch { }
        }

        private WebDriverWait Wait(int timeOut = 10)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeOut));
            wait.PollingInterval = TimeSpan.FromMilliseconds(250);
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException));

            return wait;
        }

        private IWebElement? GetElement(By by, int timeOut = 10)
        {
            try
            {
                return Wait(timeOut).Until(e => e.FindElement(by));
            }
            catch 
            {
                return null;
            }
        }

        private IReadOnlyCollection<IWebElement>? GetElements(By by, int timeOut = 10)
        {
            try
            {
                return Wait(timeOut).Until(e => e.FindElements(by));
            }
            catch 
            {
                return null;
            }
        }

        private void ClickJs(By by, int timeOut = 10)
        {
            var element = Wait(timeOut).Until(e => e.FindElement(by));

            if (element.Displayed)
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
                js.ExecuteScript("arguments[0].click();", element);
                Thread.Sleep(250);
            }
        }

        public Token? Autentication()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                {"client_id", _clientId},
                {"client_secret" , _clientSecret},
                {"grant_type", _grantType}
            };

            var response = Post(_urlToken, parameters);

            if (response.IsSuccessful)
            {
                return JsonConvert.DeserializeObject<Token>(response.Content);
            }
            else
            {
                _logger.LogError($"Erro na autenticação: {response.Content}");
                return null;
            }
        }

        public void CheckStatusStreaming(string channel)
        {
            try
            {
                var token = Autentication();

                if (token != null)
                {
                    Dictionary<string, string> headers = new Dictionary<string, string>()
                    {
                        {"Authorization", string.Format("Bearer {0}", token.TokenId) },
                        {"Client-Id", _clientId }
                    };

                    Dictionary<string, string> parameters = new Dictionary<string, string>()
                    {
                        {"first", "1" }
                    };

                    var response = Get($"https://api.twitch.tv/helix/search/channels?query={channel}", headers, parameters);



                }
            }
            catch 
            {
                
            }
        }

        public async Task RequestWhatsAppWeb(string descriptionStatus)
        {
            try
            {
                var options = new ChromeOptions();
                options.AddArguments("--test-type", "--start-maximized");
                options.AddArguments("--test-type", "--ignore-certificate-errors");
                options.AddArguments("user-data-dir=C:/Users/Allan/AppData/Local/Google/Chrome/User Data/Default");
                var driver = new ChromeDriver(options);

                driver.Navigate().GoToUrl("https://web.whatsapp.com/");
                Thread.Sleep(8000);

                string titleOld = driver.FindElement(By.XPath("//span[@title='TESTE']")).Text;



                driver.FindElement(By.XPath("//span[@title='TESTE']")).Click();
                Thread.Sleep(5000);

                driver.FindElement(By.XPath("//*[@id='main']/header/div[1]/div/div/span")).Click();
                Thread.Sleep(5000);

                new Actions(driver).MoveToElement(driver.FindElement(By.XPath("//span[@data-testid='pencil']"))).Click().Perform();
                Thread.Sleep(5000);

                var textOld = driver.FindElement(By.XPath("//*[@id='app']/div/div/div[2]/div[3]/span/div/span/div/div/section/div[1]/div/div[2]/div/div[1]/div/div/div[2]")).Text;
                Thread.Sleep(1000);
                driver.FindElement(By.XPath("//*[@id='app']/div/div/div[2]/div[3]/span/div/span/div/div/section/div[1]/div/div[2]/div/div[1]/div/div/div[2]")).Clear();
                Thread.Sleep(1000);
                driver.FindElement(By.XPath("//*[@id='app']/div/div/div[2]/div[3]/span/div/span/div/div/section/div[1]/div/div[2]/div/div[1]/div/div/div[2]")).SendKeys(textOld + " Online");
                Thread.Sleep(1000);

                new Actions(driver).MoveToElement(driver.FindElement(By.
                    XPath("//*[@id='app']/div/div/div[2]/div[3]/span/div/span/div/div/section/div[1]/div/div[2]/div/div[1]/span[2]/button/span")))
                    .Click().Perform();
                Thread.Sleep(5000);
                driver.Quit();
                driver.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar Chrome Driver.");
            }
        }
    }
}
