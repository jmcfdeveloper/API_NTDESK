
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PdfGeneratorApi.Models;
using PuppeteerSharp;
using Swashbuckle.AspNetCore.Annotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Net;
using System.Text.RegularExpressions;




namespace PdfGeneratorApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PdfController : ControllerBase
    {
        private static readonly string ChromePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe"; // Ruta al ejecutable de Chrome

        [HttpGet]       
        [Authorize] // Requiere autenticación
        [Route("GeneratePdf")]
        [SwaggerOperation(Summary = "Genera un PDF a partir de una URL")]
        [SwaggerResponse(200, "PDF generado correctamente")]
        [SwaggerResponse(400, "URL no válida")]
        [SwaggerResponse(500, "Error interno del servidor")]
        public async Task<IActionResult> GeneratePdf(string url)
         
        {

            try
            {
                if (string.IsNullOrWhiteSpace(url))
                    return BadRequest("URL no válida");

                var launchOptions = new LaunchOptions
                {
                    Headless = true,
                    ExecutablePath = ChromePath // Ruta al ejecutable de Chrome
                };

                await using var browser = await Puppeteer.LaunchAsync(launchOptions);
                await using var page = await browser.NewPageAsync();

                await page.GoToAsync(url);
                var pdfStream = await page.PdfStreamAsync();

                var pdfBytes = ((MemoryStream)pdfStream).ToArray();

                return File(pdfBytes, "application/pdf", "documento.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar el PDF: {ex.Message}");
            }
        }


        //rest of endpoints


        //end of endpoints

        // Informe Tickets de desarrollo 
        [HttpPost]
        [Authorize] // Requiere autenticación
        [Route("InformeDesarrollo")]
        [SwaggerOperation(Summary = "Genera un informe PDF con los datos de desarrollo")]
        [SwaggerResponse(200, "Informe PDF generado correctamente", typeof(FileResult))]
        [SwaggerResponse(400, "Solicitud no válida")]
        [SwaggerResponse(500, "Error interno del servidor")]
        public IActionResult InformeDesarrollo([FromBody] List<TicketData> tickets)
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    try
                    {
                        // Inicializar el documento
                        var document = new Document(PageSize.LEGAL.Rotate(), 28.34f, 28.34f, 28.34f, 28.34f); // Márgenes de 1 cm (28.34 puntos)
                        var writer = PdfWriter.GetInstance(document, stream);
                        document.Open();

                        // Crear tabla con 6 columnas
                        var table = new PdfPTable(6)
                        {
                            WidthPercentage = 100
                        };

                        // Añadir encabezado de la tabla
                        AddTableHeader(table);

                        // Añadir filas con datos
                        foreach (var ticket in tickets)
                        {
                            AddTableRow(table, ticket);
                        }

                        // Asegurarse de que la tabla use el espacio disponible y continúe en nuevas páginas
                        table.SplitLate = false;
                        table.SplitRows = true;

                        document.Add(table);
                        document.Close();

                        var content = stream.ToArray();
                        var fileContentResult = new FileContentResult(content, "application/pdf")
                        {
                            FileDownloadName = "InformeDesarrollo.pdf"
                        };

                        return fileContentResult;
                    }
                    catch (DocumentException docEx)
                    {
                        return StatusCode(500, $"Error al generar el contenido del PDF: {docEx.Message}\n{docEx.StackTrace}");
                    }
                    catch (IOException ioEx)
                    {
                        return StatusCode(500, $"Error de entrada/salida al generar el PDF: {ioEx.Message}\n{ioEx.StackTrace}");
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, $"Error desconocido al generar el PDF: {ex.Message}\n{ex.StackTrace}");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void AddTableHeader(PdfPTable table)
        {
            PdfPCell cell = new PdfPCell(new Phrase("Número del ticket"));
            //cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase("Fecha de creación del ticket"));
            //cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase("Fecha de asignación del ticket"));
            //cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase("Descripción del ticket"));
            //cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase("Fecha de respuesta del ticket"));
            //cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase("Descripción de la respuesta"));
            //cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
        }

        private void AddTableRow(PdfPTable table, TicketData ticket)
        {
            table.AddCell(LimpiarTexto(ticket.NumeroTicket));
            table.AddCell(LimpiarTexto(ticket.FechaCreacion));
            table.AddCell(LimpiarTexto(ticket.FechaAsignacion));
            table.AddCell(LimpiarTexto(ticket.DescripcionDelTicket));
            table.AddCell(LimpiarTexto(ticket.FechaRespuesta));
            table.AddCell(LimpiarTexto(ticket.DescripcionRespuesta));
        }

        private string LimpiarTexto(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            // Decodificar entidades HTML
            string decoded = WebUtility.HtmlDecode(input);

            // Eliminar etiquetas HTML
            string limpio = Regex.Replace(decoded, "<.*?>", string.Empty);

            return limpio;
        }

        // Fin Informe Desarrollo



    }


    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly string _secretKey = "8e5291daf33b566ec84fddab418d5e81"; // Reemplazar esto con clave secreta
        private readonly string _issuer = "PdfGeneratorApi"; // Reemplaza esto con el emisor deseado

        [HttpPost]
        [Route("GenerateToken")]
        public IActionResult GenerateToken([FromBody] string secret)
        {
            // Verifica si la clave secreta proporcionada es correcta
            if (secret != _secretKey)
                return Unauthorized("Clave secreta incorrecta");

            // Crea las credenciales del token
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "PdfGeneratorApi"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Genera la clave privada utilizando la clave secreta
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));

            // Genera el token
            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _issuer,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1), // Token válido por 1 hora
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            // Devuelve el token como una cadena JWT
            return Ok(new JwtSecurityTokenHandler().WriteToken(token));
        }
    }
}
