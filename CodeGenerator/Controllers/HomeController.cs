using CodeGenerator.Models;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

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
                    tabelas.Add(new Tabela()
                    {
                        Nome = reader.ReadLine().ToString().Replace("\"", ""),
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
            DeleteOlderFiles(filesNameToGenerate, pathToDownload);

            foreach (var table in tableViewModel.Tabelas)
                foreach (var templateFilePath in filesNameToGenerate)
                    if (table.GeraArquivo)
                        SearchAndReplace(templateFilePath, tableViewModel.Namespace, table.Nome);

            var directoryInfo = new DirectoryInfo(pathZip);
            foreach (FileInfo file in directoryInfo.GetFiles())
                file.Delete();

            ZipFile.CreateFromDirectory(pathToDownload, pathZip + @"\Code.zip", CompressionLevel.Fastest, false);

            byte[] bytes = System.IO.File.ReadAllBytes(pathZip + @"\Code.zip");

            return File(bytes, "application/octet-stream", "Code.zip");
        }

        private static void DeleteOlderFiles(List<string> filesNameToGenerate, string downloadFilePath)
        {
            foreach (var templateFilePath in filesNameToGenerate)
            {
                var pathFileToDelete =  downloadFilePath +
                                         @"\" + Path.GetFileNameWithoutExtension(templateFilePath).Replace(".", @"\");
                var directoryInfo = new DirectoryInfo(pathFileToDelete);
                foreach (FileInfo file in directoryInfo.GetFiles())
                    file.Delete();
            }
        }

        public static void SearchAndReplace(string templateFilePath, string nameSpace, string className)
        {
            StreamReader rd = new StreamReader(templateFilePath);
            string strContents = rd.ReadToEnd();
            rd.Close();

            Regex regexText = new Regex("#NAMESPACE#");
            strContents = regexText.Replace(strContents, nameSpace);
            regexText = new Regex("#CLASSNAME#");
            strContents = regexText.Replace(strContents, className.ToUpper());
            regexText = new Regex("#VARCLASSNAME#");
            strContents = regexText.Replace(strContents, className);

            string pathToSave = @"C:\Users\Lais\source\repos\CodeGenerator\CodeGenerator\DownloadFiles\" + 
                                Path.GetFileNameWithoutExtension(templateFilePath).Replace(".", @"\");

            string fileName = className + "." + "cs";

            var path = Path.Combine(pathToSave, fileName);

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(strContents);
                    bw.Close();
                }
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}