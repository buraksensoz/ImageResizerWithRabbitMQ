using ImageResizerWithRabbitMQ.App.Models;
using ImageResizerWithRabbitMQ.App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageResizerWithRabbitMQ.App.Controllers
{
    public class HomeController : Controller
    {

        private readonly RabbitResizeImagePublisher _rabbitResizeImagePublisher;

        public HomeController(RabbitResizeImagePublisher rabbitResizeImagePublisher)
        {
            _rabbitResizeImagePublisher = rabbitResizeImagePublisher;


        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file) {

            if (file is not {Length:>0 })
            {
                return RedirectToAction("Index");
            }

            var randomImageName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var path=Path.Combine(Directory.GetCurrentDirectory(),"wwwroot/images", randomImageName);
            await using FileStream stream = new(path, FileMode.Create);
            await file.CopyToAsync(stream);


           var published= _rabbitResizeImagePublisher.Publish(new()
            {
                ImageFileName=randomImageName
            });

            if (!published) TempData["Error"]= "RabbitMQ Server is Down";

            return RedirectToAction("Index");
        }
    }
}
