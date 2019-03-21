using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CanalIlhas.Controllers
{
    public class UploadController : Controller
    {
        private IHostingEnvironment _hostingEnvironment;
        public UploadController(IHostingEnvironment hostingEnvironment)
        {
            this._hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        [RequestSizeLimit(314572800)]// 300MB
        public async Task<IActionResult> UploadArquivo(IList<IFormFile> files)
        {
            if (files.Count > 0)
            {
                long totalBytes = files.Sum(f => f.Length);

                foreach (IFormFile source in files)
                {
                    string arquivoNome = ContentDispositionHeaderValue.Parse(source.ContentDisposition).FileName.ToString().Trim('"').ToLower();

                    arquivoNome = this.AsseguraNomeCorretoArquivo(arquivoNome);

                    byte[] buffer = new byte[16 * 1024];

                    using (FileStream output = System.IO.File.Create(this.ObterCaminhoNomeArquivo(arquivoNome)))
                    {
                        using (Stream input = source.OpenReadStream())
                        {
                            long totalLerBytes = 0;
                            int lerBytes;

                            while ((lerBytes = input.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                await output.WriteAsync(buffer, 0, lerBytes);
                                totalLerBytes += lerBytes;
                                // Startup.Progress = (int)((float)totalReadBytes / (float)totalBytes * 100.0);
                                await Task.Delay(10); // It is only to make the process slower
                            }
                        }
                    }
                }
                return Json(new { state = 1, message = "Enviado com sucesso!" });
            }
            //return this.Content("success");
            return Json(new { state = 0, message = string.Empty });
        }

        //[HttpPost]
        //public ActionResult Progress()
        //{
        //    return this.Content(Startup.Progress.ToString());
        //}

        private string AsseguraNomeCorretoArquivo(string arquivoNome)
        {
            if (arquivoNome.Contains("\\"))
                arquivoNome = arquivoNome.Substring(arquivoNome.LastIndexOf("\\") + 1);

            return arquivoNome;
        }

        private string ObterCaminhoNomeArquivo(string arquivoNome)
        {
            string caminho = this._hostingEnvironment.WebRootPath + "\\uploads\\";

            if (!Directory.Exists(caminho))
                Directory.CreateDirectory(caminho);

            return caminho + arquivoNome;
        }
    }
}