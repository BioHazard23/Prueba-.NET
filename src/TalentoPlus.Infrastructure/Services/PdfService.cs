using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TalentoPlus.Application.Services.Interfaces;
using TalentoPlus.Domain.Entities;

namespace TalentoPlus.Infrastructure.Services;

public class PdfService : IPdfService
{
    public PdfService()
    {
        // Configurar QuestPDF para uso comunitario
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerarHojaVida(Empleado empleado)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Element(c => ComposeHeader(c, empleado));
                page.Content().Element(c => ComposeContent(c, empleado));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, Empleado empleado)
    {
        container.Column(column =>
        {
            column.Item().Background(Colors.Blue.Darken3).Padding(20).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("HOJA DE VIDA").FontSize(24).Bold().FontColor(Colors.White);
                    col.Item().Text("TalentoPlus S.A.S.").FontSize(12).FontColor(Colors.White);
                });

                row.ConstantItem(100).AlignRight().Column(col =>
                {
                    col.Item().Height(60).Width(60)
                        .Background(Colors.White)
                        .AlignCenter()
                        .AlignMiddle()
                        .Text($"{empleado.Nombres[0]}{empleado.Apellidos[0]}")
                        .FontSize(24)
                        .Bold()
                        .FontColor(Colors.Blue.Darken3);
                });
            });

            column.Item().Height(10);
        });
    }

    private void ComposeContent(IContainer container, Empleado empleado)
    {
        container.PaddingVertical(20).Column(column =>
        {
            // Datos Personales
            column.Item().Element(c => ComposeSection(c, "DATOS PERSONALES", sec =>
            {
                sec.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text($"Nombre Completo: {empleado.NombreCompleto}").Bold();
                        col.Item().Text($"Documento: {empleado.Documento}");
                        col.Item().Text($"Fecha de Nacimiento: {empleado.FechaNacimiento:dd/MM/yyyy}");
                    });
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text($"Dirección: {empleado.Direccion}");
                        col.Item().Text($"Teléfono: {empleado.Telefono}");
                        col.Item().Text($"Email: {empleado.Email}");
                    });
                });
            }));

            column.Item().Height(15);

            // Información Laboral
            column.Item().Element(c => ComposeSection(c, "INFORMACIÓN LABORAL", sec =>
            {
                sec.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text($"Departamento: {empleado.Departamento?.Nombre ?? "N/A"}").Bold();
                        col.Item().Text($"Cargo: {empleado.Cargo?.Nombre ?? "N/A"}");
                        col.Item().Text($"Fecha de Ingreso: {empleado.FechaIngreso:dd/MM/yyyy}");
                    });
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text($"Salario: ${empleado.Salario:N0} COP");
                        col.Item().Text($"Estado: {empleado.Estado}");
                        col.Item().Text($"Antigüedad: {empleado.AntiguedadAnios} años");
                    });
                });
            }));

            column.Item().Height(15);

            // Nivel Educativo
            column.Item().Element(c => ComposeSection(c, "NIVEL EDUCATIVO", sec =>
            {
                sec.Item().Text($"Nivel alcanzado: {FormatNivelEducativo(empleado.NivelEducativo.ToString())}").Bold();
            }));

            column.Item().Height(15);

            // Perfil Profesional
            if (!string.IsNullOrEmpty(empleado.PerfilProfesional))
            {
                column.Item().Element(c => ComposeSection(c, "PERFIL PROFESIONAL", sec =>
                {
                    sec.Item().Text(empleado.PerfilProfesional).Justify();
                }));
            }

            column.Item().Height(30);

            // Firma
            column.Item().AlignCenter().Column(col =>
            {
                col.Item().Height(40);
                col.Item().Width(200).BorderTop(1).BorderColor(Colors.Grey.Medium);
                col.Item().Height(5);
                col.Item().Text(empleado.NombreCompleto).AlignCenter();
                col.Item().Text($"C.C. {empleado.Documento}").FontSize(9).AlignCenter();
            });
        });
    }

    private void ComposeSection(IContainer container, string title, Action<ColumnDescriptor> content)
    {
        container.Column(column =>
        {
            column.Item().BorderBottom(2).BorderColor(Colors.Blue.Darken3).PaddingBottom(5)
                .Text(title).FontSize(13).Bold().FontColor(Colors.Blue.Darken3);

            column.Item().PaddingTop(10).Column(content);
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Column(col =>
        {
            col.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten1).PaddingTop(10);
            col.Item().Row(row =>
            {
                row.RelativeItem().Text($"Documento generado el {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Medium);
                row.RelativeItem().AlignRight().Text("TalentoPlus S.A.S. - Sistema de Gestión de RRHH").FontSize(9).FontColor(Colors.Grey.Medium);
            });
        });
    }

    private static string FormatNivelEducativo(string nivel)
    {
        return nivel switch
        {
            "Tecnico" => "Técnico",
            "Tecnologo" => "Tecnólogo",
            "Especializacion" => "Especialización",
            "Maestria" => "Maestría",
            _ => nivel
        };
    }
}

