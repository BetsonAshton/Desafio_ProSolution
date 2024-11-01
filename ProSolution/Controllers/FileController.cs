using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Dapper;

public class FileController : ApiController
{
    private readonly string _connectionString = ConfigurationManager.ConnectionStrings["YourConnectionString"].ConnectionString;
    private readonly string _fileServerPath = @"C:\Desafio\"; // Caminho para o servidor de arquivos

    // Limitar tipos de arquivos não permitidos e tamanho máximo (10MB)
    private readonly string[] _blockedExtensions = { ".exe", ".bat" };
    private const int _maxFileSize = 10 * 1024 * 1024;

    // Endpoint para upload de arquivos
    [HttpPost]
    [Route("api/file/upload")]
    public async Task<IHttpActionResult> UploadFile()
    {
        if (!Request.Content.IsMimeMultipartContent())
            return StatusCode(HttpStatusCode.UnsupportedMediaType);

        var provider = new MultipartMemoryStreamProvider();
        await Request.Content.ReadAsMultipartAsync(provider);

        //Criando um provedor para armazenar os dados multipart e lê o conteúdo da solicitação.
        foreach (var file in provider.Contents)
        {
            var filename = file.Headers.ContentDisposition.FileName.Trim('\"');
            var buffer = await file.ReadAsByteArrayAsync();
            var extension = Path.GetExtension(filename).ToLower();

            // Verifica o tamanho do arquivo
            if (buffer.Length > _maxFileSize)
                return BadRequest("O arquivo excede o tamanho máximo permitido de 10MB.");

            // Verifica a extensão do arquivo
            if (_blockedExtensions.Contains(extension))
                return BadRequest("Tipo de arquivo não permitido.");

            // Definir caminho de destino (pode ser dinâmico via parâmetro)
            var destinationPath = Path.Combine(_fileServerPath, filename);
            File.WriteAllBytes(destinationPath, buffer);

          
        }

        return Ok("Arquivo(s) enviado(s) com sucesso.");
    }

    // Endpoint para download de arquivos
    [HttpGet]
    [Route("api/file/download")]
    public HttpResponseMessage DownloadFile(string fileName)
    {
        var filePath = Path.Combine(_fileServerPath, fileName);

        if (!File.Exists(filePath))
            return Request.CreateResponse(HttpStatusCode.NotFound, "Arquivo não encontrado.");

        var result = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(File.ReadAllBytes(filePath))
        };
        result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
        {
            FileName = fileName
        };
        result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

        return result;
    }
}
