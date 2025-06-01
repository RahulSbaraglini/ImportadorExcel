using DesafioConcilig1.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;

namespace DesafioConcilig1.Controllers
{
    public class ImportadorController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ImportadorController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var listaContratos = _context.Contratos
                                         .Include(c => c.Usuario)
                                         .OrderByDescending(c => c.DataImportacao)
                                         .ToList();
            return View(listaContratos);
        }

        [HttpGet]
        public IActionResult RelatorioClientes()
        {
            var nomesClientes = _context.Contratos
                                       .Select(c => c.Nome)
                                       .Distinct()
                                       .OrderBy(n => n)
                                       .ToList();

            return View(nomesClientes);
        }

        [HttpGet]
        public JsonResult GetResumoContratoCliente(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
            {
                return Json(new { success = false, message = "Nome de cliente inválido." });
            }

            var contratosDoCliente = _context.Contratos
                                             .Where(c => c.Nome == nome)
                                             .ToList();

            if (!contratosDoCliente.Any())
            {
                return Json(new
                {
                    success = true,
                    totalValor = 0m,
                    maiorAtrasoDias = 0
                });
            }

            var totalValor = contratosDoCliente.Sum(c => c.Valor);

            var hoje = DateTime.Now.Date;
            var atrasos = contratosDoCliente
                .Where(c => c.Vencimento.Date < hoje)
                .Select(c => (hoje - c.Vencimento.Date).Days);

            var maiorAtrasoDias = atrasos.Any() ? atrasos.Max() : 0;

            return Json(new
            {
                success = true,
                totalValor,
                maiorAtrasoDias
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UploadAjax(IFormFile arquivo)
        {
            try
            {
                if (arquivo == null || arquivo.Length == 0)
                    return Json(new { success = false, message = "Selecione um arquivo (.xlsx ou .csv) válido." });

                var uploadsFolder = Path.Combine(_env.WebRootPath ?? string.Empty, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var originalFileName = Path.GetFileName(arquivo.FileName);
                var uniqueFileName = $"contratos_{timestamp}_{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    await arquivo.CopyToAsync(fs);
                }

                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int usuarioLogadoId))
                {
                    return Json(new { success = false, message = "Não foi possível identificar o usuário autenticado." });
                }

                var contratosParaInserir = new List<Contratos>();
                var extensao = Path.GetExtension(uniqueFileName).ToLower();

                if (extensao == ".csv")
                {
                    using var reader = new StreamReader(filePath, System.Text.Encoding.GetEncoding("iso-8859-1"));
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
                        var nome = csv.GetField<string>("Nome")?.Trim();
                        var vencStr = csv.GetField<string>("Vencimento")?.Trim();
                        var valorStr = csv.GetField<string>("Valor")?.Trim();

                        if (string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(vencStr) || string.IsNullOrEmpty(valorStr))
                            continue;

                        if (!DateTime.TryParseExact(vencStr,
                                                    new[] { "dd/MM/yyyy", "d/M/yyyy" },
                                                    new CultureInfo("pt-BR"),
                                                    DateTimeStyles.None,
                                                    out DateTime vencimentoParsed))
                        {
                            continue;
                        }

                        var valorSanitized = valorStr.Replace(".", "").Replace(",", ".");
                        if (!decimal.TryParse(valorSanitized, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valorParsed))
                        {
                            continue;
                        }

                        contratosParaInserir.Add(new Contratos
                        {
                            Nome = nome,
                            Vencimento = vencimentoParsed,
                            Valor = valorParsed,
                            DataImportacao = DateTime.Now,
                            UsuarioId = usuarioLogadoId
                        });
                    }
                }
                else if (extensao == ".xlsx" || extensao == ".xls")
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using var package = new ExcelPackage(new FileInfo(filePath));
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                        return Json(new { success = false, message = "O Excel não contém nenhuma planilha." });

                    int rowCount = worksheet.Dimension.End.Row;
                    for (int row = 2; row <= rowCount; row++)
                    {
                        var nomeCell = worksheet.Cells[row, 1].Text?.Trim();   // Coluna 1 = Nome
                        var vencCell = worksheet.Cells[row, 5].Text?.Trim();   // Coluna 5 = Vencimento
                        var valorCell = worksheet.Cells[row, 6].Text?.Trim();   // Coluna 6 = Valor

                        if (string.IsNullOrEmpty(nomeCell) || string.IsNullOrEmpty(vencCell) || string.IsNullOrEmpty(valorCell))
                            continue;

                        if (!DateTime.TryParseExact(vencCell,
                                                    new[] { "dd/MM/yyyy", "d/M/yyyy" },
                                                    new CultureInfo("pt-BR"),
                                                    DateTimeStyles.None,
                                                    out DateTime vencimentoParsed))
                        {
                            continue;
                        }

                        var valorSanitized = valorCell.Replace(".", "").Replace(",", ".");
                        if (!decimal.TryParse(valorSanitized, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valorParsed))
                        {
                            continue;
                        }

                        contratosParaInserir.Add(new Contratos
                        {
                            Nome = nomeCell,
                            Vencimento = vencimentoParsed,
                            Valor = valorParsed,
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

                _context.Contratos.AddRange(contratosParaInserir);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"{contratosParaInserir.Count} contrato(s) importado(s) com sucesso." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro inesperado: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult GetTabelaContratos()
        {
            var listaContratos = _context.Contratos
                                         .Include(c => c.Usuario)
                                         .OrderByDescending(c => c.DataImportacao)
                                         .ToList();

            return PartialView("_TabelaContratos", listaContratos);
        }
    }
}
