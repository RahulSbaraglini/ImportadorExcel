using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DesafioConcilig1.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using CsvHelper;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using CsvHelper.Configuration;
using System.Security.Claims;
using System.Text;

namespace DesafioConcilig1.Controllers
{
    [Authorize] // só permite acessar se estiver autenticado
    public class ImportadorController : Controller
    {
        private readonly AppDbContext _context;

        public ImportadorController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var listaContratos = _context.Contratos
                .OrderByDescending(c => c.DataImportacao)
                .ToList();
            return View(listaContratos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UploadAjax(IFormFile arquivo)
        {
            try
            {
                if (arquivo == null || arquivo.Length == 0)
                    return Json(new { success = false, message = "Selecione um arquivo (.xlsx ou .csv) válido." });

                // 1) Obter o usuário logado
                //    Supondo que o ClaimTypes.NameIdentifier contenha o ID inteiro do usuário
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int usuarioLogadoId))
                {
                    return Json(new { success = false, message = "Não foi possível identificar o usuário autenticado." });
                }

                var extensao = Path.GetExtension(arquivo.FileName).ToLower();
                var contratosParaInserir = new List<Contratos>();

                if (extensao == ".csv")
                {
                    // Leitura de CSV com ; como delimitador
                    using var stream = arquivo.OpenReadStream();
                    using var reader = new StreamReader(stream, Encoding.GetEncoding("iso-8859-1"));
                    var config = new CsvConfiguration(new CultureInfo("pt-BR"))
                    {
                        Delimiter = ";",
                        HasHeaderRecord = true,
                        MissingFieldFound = null,
                        HeaderValidated = null
                    };
                    using var csv = new CsvReader(reader, config);

                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        // Exemplo: ler apenas "Nome" para gravar em Contratos
                        var nome = csv.GetField<string>("Nome")?.Trim();
                        if (string.IsNullOrEmpty(nome))
                            continue;

                        contratosParaInserir.Add(new Contratos
                        {
                            Nome = nome,
                            DataImportacao = DateTime.Now,
                            UsuarioId = usuarioLogadoId
                        });
                    }
                }
                else if (extensao == ".xlsx" || extensao == ".xls")
                {
                    // Leitura de Excel com EPPlus
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using var ms = new MemoryStream();
                    await arquivo.CopyToAsync(ms);
                    using var package = new ExcelPackage(ms);
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                        return Json(new { success = false, message = "O Excel não contém nenhuma planilha." });

                    int rowCount = worksheet.Dimension.End.Row;
                    for (int row = 2; row <= rowCount; row++)
                    {
                        var nomeCelula = worksheet.Cells[row, 1].Text?.Trim();
                        if (string.IsNullOrEmpty(nomeCelula))
                            continue;

                        contratosParaInserir.Add(new Contratos
                        {
                            Nome = nomeCelula,
                            DataImportacao = DateTime.Now,
                            UsuarioId = usuarioLogadoId
                        });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Formato não suportado. Use .xlsx ou .csv." });
                }

                if (!contratosParaInserir.Any())
                    return Json(new { success = false, message = "Nenhum registro válido encontrado no arquivo." });

                // Salvar no banco
                _context.Contratos.AddRange(contratosParaInserir);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = $"{contratosParaInserir.Count} registro(s) importado(s) com sucesso." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro inesperado: {ex.Message}" });
            }
        }
    }
}