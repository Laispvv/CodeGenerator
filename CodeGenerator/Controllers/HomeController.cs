using CodeGenerator.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO.Compression;

namespace CodeGenerator.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Index(IFormFile file)
        {
            var tabelas = new List<Tabela>();
            if(file == null)
                return View();
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                {
                    var nome = reader.ReadLine();
                    if (nome != null)
                        tabelas.Add(new Tabela()
                        {
                            Nome = nome.ToString().Replace("\"", ""),
                            GeraArquivo = true
                        });
                }
            }
            TempData["Tabelas"] = JsonConvert.SerializeObject(tabelas.OrderBy(x => x.Nome).ToList());
            return RedirectToAction("NamespaceAndTableSelection");
        }        
        
        public IActionResult NamespaceAndTableSelection()
        {
            var tabelasSerializadas = TempData["Tabelas"];
            if (tabelasSerializadas != null)
            {
                var tabelas = JsonConvert.DeserializeObject<List<Tabela>>(tabelasSerializadas.ToString());
                var tabelaViewModel = new TabelaViewModel() { Tabelas = tabelas };
                return View(tabelaViewModel);
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult NamespaceAndTableSelection(TabelaViewModel tableViewModel)
        {
            var pathZip = @"C:\Users\Lais\source\repos\CodeGenerator\CodeGenerator\CreatedZipFile";
            var pathToDownload = @"C:\Users\Lais\source\repos\CodeGenerator\CodeGenerator\DownloadFiles";

            if (tableViewModel == null || tableViewModel.Tabelas == null)
                return RedirectToAction("Index");

            var filesNameToGenerate = Directory.GetFiles(@"C:\Users\Lais\source\repos\CodeGenerator\CodeGenerator\Example\").ToList();
            Helper.Helper.DeleteOlderFiles(filesNameToGenerate, pathToDownload);

            foreach (var table in tableViewModel.Tabelas)
                foreach (var templateFilePath in filesNameToGenerate)
                    if (table.GeraArquivo)
                        Helper.Helper.CreateFiles(templateFilePath, tableViewModel.Namespace, table.Nome);

            var directoryInfo = new DirectoryInfo(pathZip);
            foreach (FileInfo file in directoryInfo.GetFiles())
                file.Delete();

            var zipName = "Code" + tableViewModel.Namespace + ".zip";
            ZipFile.CreateFromDirectory(pathToDownload, pathZip + @"\" + zipName, CompressionLevel.Fastest, false);
            Helper.Helper.DeleteOlderFiles(filesNameToGenerate, pathToDownload);

            byte[] bytes = System.IO.File.ReadAllBytes(pathZip + @"\" + zipName);

            return File(bytes, "application/octet-stream", zipName);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}