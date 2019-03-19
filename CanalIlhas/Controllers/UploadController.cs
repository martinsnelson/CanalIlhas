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
                    string filename = ContentDispositionHeaderValue.Parse(source.ContentDisposition).FileName.ToString().Trim('"');

                    filename = this.EnsureCorrectFilename(filename);

                    byte[] buffer = new byte[16 * 1024];

                    using (FileStream output = System.IO.File.Create(this.GetPathAndFilename(filename)))
                    {
                        using (Stream input = source.OpenReadStream())
                        {
                            long totalReadBytes = 0;
                            int readBytes;

                            while ((readBytes = input.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                await output.WriteAsync(buffer, 0, readBytes);
                                totalReadBytes += readBytes;
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

        private string EnsureCorrectFilename(string filename)
        {
            if (filename.Contains("\\"))
                filename = filename.Substring(filename.LastIndexOf("\\") + 1);

            return filename;
        }

        private string GetPathAndFilename(string filename)
        {
            string path = this._hostingEnvironment.WebRootPath + "\\uploads\\";

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path + filename;
        }
    }
}