using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace YourNamespace.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFileRepository _fileRepository;
        private readonly string _fileServerPath = @"C:\Desafio\"; // Caminho para o servidor de arquivos

        public HomeController()
        {
            // Usar a string de conexão apropriada
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("A string de conexão não está configurada.");
            }

            _fileRepository = new FileRepository(connectionString);
        }


        public ActionResult Index()
        {
            // Listar arquivos disponíveis para download
            var files = Directory.GetFiles(_fileServerPath)
                .Select(Path.GetFileName) // Extrai apenas o nome do arquivo
                .ToList();

            ViewBag.Files = files; // Passa a lista de arquivos para a View

            return View();
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                try
                {
                    if (file.ContentLength > 10 * 1024 * 1024) // 10 MB
                    {
                        TempData["Message"] = "O arquivo deve ter no máximo 10 MB.";
                        TempData["MessageType"] = "error";
                        return RedirectToAction("Index");
                    }

                    var extension = Path.GetExtension(file.FileName).ToLower();
                    if (extension == ".exe" || extension == ".bat")
                    {
                        TempData["Message"] = "Tipo de arquivo não permitido.";
                        TempData["MessageType"] = "error";
                        return RedirectToAction("Index");
                    }

                    var path = Path.Combine(_fileServerPath, Path.GetFileName(file.FileName));
                    file.SaveAs(path);

                    // Chama o repositório para salvar os dados no banco
                    _fileRepository.AddFile(file.FileName, path);

                    TempData["Message"] = "Arquivo enviado com sucesso!";
                    TempData["MessageType"] = "success";
                }
                catch (Exception ex)
                {
                    TempData["Message"] = $"Erro ao enviar o arquivo: {ex.Message}";
                    TempData["MessageType"] = "error";
                }
            }
            else
            {
                TempData["Message"] = "Nenhum arquivo selecionado.";
                TempData["MessageType"] = "error";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Download(string fileName)
        {
            var filePath = Path.Combine(_fileServerPath, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                TempData["Message"] = "Arquivo não encontrado.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
    }
}
