using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace CanalIlhas.Controllers
{
    public class JsonContent : StringContent
    {
        public JsonContent(object obj, object ob) :
            base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
        { }
    }

    public class Test
    {
        public string Aplicacao { get; set; }
        public string Login { get; set; }
    }

    public class CanalIlhasController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CanalIlhasController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult UploadArquivo()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        [RequestSizeLimit(314572800)]// 300MB
        public JsonResult UploadArquivo(IList<IFormFile> files)
        {
            if (files.Count > 0)
            {
                return Json(new { state = 1, message = "Enviado com sucesso!" });
            }
            return Json(new { state = 0, message = string.Empty });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Upload()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(314572800)]// 300MB
        public async Task<IActionResult> Upload(List<IFormFile> files)
        {
            long size = files.Sum(f => f.Length);
            // caminho completo para o arquivo no local temporário
            /*
            var t =  Path.GetFileName();
            var t1 = Path.GetFileNameWithoutExtension();
            var t2 = Path.GetFileNameWithoutExtension();
            var t3 = Path.GetFullPath();
            var t4 = Path.GetFullPath();
            */
            var filePath = Path.GetTempFileName();

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }

            return Ok(new { count = files.Count, size, filePath });
        }

        //var teste = new Test { Aplicacao = "", Login = "" };

        //var test = new Test
        //{
        //     Aplicacao = "";
        //     Login = "";
        //};

        //[HttpPost]
        //public async Task<ActionResult> Get()
        //{
        //    var client = _httpClientFactory.CreateClient();
        //    client.BaseAddress = new Uri("HTTPS://WASEGURANCA.BRASILCENTER.COM.BR/WASEGURANCA"); ///api/Usuario/ObterUsuarioPorLogin
        //    // client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
        //    // client.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");
        //    string result = await client.PostAsJsonAsync("", Test);
        //    return Ok(result);
        //}
        //// 1. Disable the form value model binding here to take control of handling 
        ////    potentially large files.
        //// 2. Typically antiforgery tokens are sent in request body, but since we 
        ////    do not want to read the request body early, the tokens are made to be 
        ////    sent via headers. The antiforgery token filter first looks for tokens
        ////    in the request header and then falls back to reading the body.
        //[HttpPost]
        //[DisableFormValueModelBinding]
        //[ValidateAntiForgeryToken]
        //[RequestSizeLimit(314572800)]// 300MB        
        //public async Task<IActionResult> Upload()
        //{
        //    if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
        //    {
        //        return BadRequest($"Expected a multipart request, but got {Request.ContentType}");
        //    }

        //    // Used to accumulate all the form url encoded key value pairs in the 
        //    // request.
        //    var formAccumulator = new KeyValueAccumulator();
        //    string targetFilePath = null;

        //    var boundary = MultipartRequestHelper.GetBoundary(
        //        MediaTypeHeaderValue.Parse(Request.ContentType),
        //        _defaultFormOptions.MultipartBoundaryLengthLimit);
        //    var reader = new MultipartReader(boundary, HttpContext.Request.Body);

        //    var section = await reader.ReadNextSectionAsync();
        //    while (section != null)
        //    {
        //        ContentDispositionHeaderValue contentDisposition;
        //        var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);

        //        if (hasContentDispositionHeader)
        //        {
        //            if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
        //            {
        //                targetFilePath = Path.GetTempFileName();
        //                using (var targetStream = System.IO.File.Create(targetFilePath))
        //                {
        //                    await section.Body.CopyToAsync(targetStream);

        //                    _logger.LogInformation($"Copied the uploaded file '{targetFilePath}'");
        //                }
        //            }
        //            else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
        //            {
        //                // Content-Disposition: form-data; name="key"
        //                //
        //                // value

        //                // Do not limit the key name length here because the 
        //                // multipart headers length limit is already in effect.
        //                var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name);
        //                var encoding = GetEncoding(section);
        //                using (var streamReader = new StreamReader(
        //                    section.Body,
        //                    encoding,
        //                    detectEncodingFromByteOrderMarks: true,
        //                    bufferSize: 1024,
        //                    leaveOpen: true))
        //                {
        //                    // The value length limit is enforced by MultipartBodyLengthLimit
        //                    var value = await streamReader.ReadToEndAsync();
        //                    if (String.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
        //                    {
        //                        value = String.Empty;
        //                    }
        //                    formAccumulator.Append(key, value);

        //                    if (formAccumulator.ValueCount > _defaultFormOptions.ValueCountLimit)
        //                    {
        //                        throw new InvalidDataException($"Form key count limit {_defaultFormOptions.ValueCountLimit} exceeded.");
        //                    }
        //                }
        //            }
        //        }

        //        // Drains any remaining section body that has not been consumed and
        //        // reads the headers for the next section.
        //        section = await reader.ReadNextSectionAsync();
        //    }

        //    // Bind form data to a model
        //    var user = new User();
        //    var formValueProvider = new FormValueProvider(
        //        BindingSource.Form,
        //        new FormCollection(formAccumulator.GetResults()),
        //        CultureInfo.CurrentCulture);

        //    var bindingSuccessful = await TryUpdateModelAsync(user, prefix: "",
        //        valueProvider: formValueProvider);
        //    if (!bindingSuccessful)
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest(ModelState);
        //        }
        //    }

        //    var uploadedData = new UploadedData()
        //    {
        //        Name = user.Name,
        //        Age = user.Age,
        //        Zipcode = user.Zipcode,
        //        FilePath = targetFilePath
        //    };
        //    return Json(uploadedData);
        //}
    }


    //public class GenerateAntiforgeryTokenCookieForAjaxAttribute : ActionFilterAttribute
    //{
    //    public override void OnActionExecuted(ActionExecutedContext context)
    //    {
    //        var antiforgery = context.HttpContext.RequestServices.GetService<IAntiforgery>();

    //        // We can send the request token as a JavaScript-readable cookie, 
    //        // and Angular will use it by default.
    //        var tokens = antiforgery.GetAndStoreTokens(context.HttpContext);
    //        context.HttpContext.Response.Cookies.Append(
    //            "XSRF-TOKEN",
    //            tokens.RequestToken,
    //            new CookieOptions() { HttpOnly = false });
    //    }
    //}

    //[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    //public class DisableFormValueModelBindingAttribute : Attribute, IResourceFilter
    //{
    //    public void OnResourceExecuting(ResourceExecutingContext context)
    //    {
    //        var factories = context.ValueProviderFactories;
    //        factories.RemoveType<FormValueProviderFactory>();
    //        factories.RemoveType<JQueryFormValueProviderFactory>();
    //    }

    //    public void OnResourceExecuted(ResourceExecutedContext context)
    //    {
    //    }
        
    //}

}