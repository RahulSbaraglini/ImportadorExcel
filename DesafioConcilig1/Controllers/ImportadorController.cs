using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml; // Você precisa instalar o pacote EPPlus
using System.IO;
using CsvHelper;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Formats.Asn1;

namespace SeuProjeto.Controllers
{
    public class ImportarController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(IFormFile arquivo)
        {
            if (arquivo == null || arquivo.Length == 0)
            {
                ViewBag.Erro = "Selecione um arquivo válido.";
                return View();
            }

            var extensao = Path.GetExtension(arquivo.FileName).ToLower();
            DataTable tabela = new DataTable();

            using var stream = new MemoryStream();
            arquivo.CopyTo(stream);
            stream.Position = 0;

            if (extensao == ".xlsx")
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0];

                bool colunasDefinidas = false;
                for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
                {
                    var novaLinha = tabela.NewRow();
                    for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                    {
                        if (!colunasDefinidas)
                            tabela.Columns.Add(worksheet.Cells[1, col].Text);

                        if (row > 1)
                            novaLinha[col - 1] = worksheet.Cells[row, col].Text;
                    }
                    if (row > 1)
                        tabela.Rows.Add(novaLinha);
                    colunasDefinidas = true;
                }
            }
            else if (extensao == ".csv")
            {
                using var reader = new StreamReader(stream);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                using var dr = new CsvDataReader(csv);
                tabela.Load(dr);
            }
            else
            {
                ViewBag.Erro = "Formato de arquivo inválido. Use .xlsx ou .csv";
                return View();
            }

            ViewBag.Tabela = tabela;
            return View();
        }
    }
}