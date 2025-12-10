using ClosedXML.Excel;
using TalentoPlus.Application.Services.Interfaces;
using TalentoPlus.Domain.Entities;
using TalentoPlus.Domain.Enums;
using TalentoPlus.Domain.Interfaces;

namespace TalentoPlus.Infrastructure.Services;

public class ExcelService : IExcelService
{
    private readonly IUnitOfWork _unitOfWork;

    public ExcelService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ExcelImportResultDto> ImportarEmpleadosAsync(Stream excelStream)
    {
        var result = new ExcelImportResultDto();

        try
        {
            using var workbook = new XLWorkbook(excelStream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1); // Saltar encabezado

            // Obtener catálogos
            var departamentos = (await _unitOfWork.Departamentos.GetAllAsync()).ToList();
            var cargos = (await _unitOfWork.Cargos.GetAllAsync()).ToList();

            foreach (var row in rows)
            {
                result.TotalFilas++;

                try
                {
                    var documento = row.Cell(1).GetString().Trim();
                    if (string.IsNullOrEmpty(documento)) continue;

                    var nombres = row.Cell(2).GetString().Trim();
                    var apellidos = row.Cell(3).GetString().Trim();
                    var fechaNacimientoStr = row.Cell(4).GetString().Trim();
                    var direccion = row.Cell(5).GetString().Trim();
                    var telefono = row.Cell(6).GetString().Trim();
                    var email = row.Cell(7).GetString().Trim();
                    var cargoStr = row.Cell(8).GetString().Trim();
                    var salarioStr = row.Cell(9).GetString().Trim();
                    var fechaIngresoStr = row.Cell(10).GetString().Trim();
                    var estadoStr = row.Cell(11).GetString().Trim();
                    var nivelEducativoStr = row.Cell(12).GetString().Trim();
                    var perfilProfesional = row.Cell(13).GetString().Trim();
                    var departamentoStr = row.Cell(14).GetString().Trim();

                    // Parsear fechas
                    if (!DateTime.TryParse(fechaNacimientoStr, out var fechaNacimiento))
                    {
                        result.Errores++;
                        result.Mensajes.Add($"Fila {result.TotalFilas + 1}: Fecha de nacimiento inválida para documento {documento}");
                        continue;
                    }

                    if (!DateTime.TryParse(fechaIngresoStr, out var fechaIngreso))
                    {
                        result.Errores++;
                        result.Mensajes.Add($"Fila {result.TotalFilas + 1}: Fecha de ingreso inválida para documento {documento}");
                        continue;
                    }

                    // Parsear salario
                    if (!decimal.TryParse(salarioStr, out var salario))
                    {
                        salario = 0;
                    }

                    // Buscar departamento
                    var departamento = departamentos.FirstOrDefault(d => 
                        d.Nombre.Equals(departamentoStr, StringComparison.OrdinalIgnoreCase));
                    if (departamento == null)
                    {
                        result.Errores++;
                        result.Mensajes.Add($"Fila {result.TotalFilas + 1}: Departamento '{departamentoStr}' no encontrado para documento {documento}");
                        continue;
                    }

                    // Buscar cargo
                    var cargo = cargos.FirstOrDefault(c => 
                        c.Nombre.Equals(cargoStr, StringComparison.OrdinalIgnoreCase));
                    if (cargo == null)
                    {
                        result.Errores++;
                        result.Mensajes.Add($"Fila {result.TotalFilas + 1}: Cargo '{cargoStr}' no encontrado para documento {documento}");
                        continue;
                    }

                    // Parsear estado
                    var estado = ParseEstado(estadoStr);

                    // Parsear nivel educativo
                    var nivelEducativo = ParseNivelEducativo(nivelEducativoStr);

                    // Verificar si el empleado ya existe
                    var empleadoExistente = await _unitOfWork.Empleados.GetByDocumentoAsync(documento);

                    if (empleadoExistente != null)
                    {
                        // Actualizar
                        empleadoExistente.Nombres = nombres;
                        empleadoExistente.Apellidos = apellidos;
                        empleadoExistente.FechaNacimiento = DateTime.SpecifyKind(fechaNacimiento, DateTimeKind.Utc);
                        empleadoExistente.Direccion = direccion;
                        empleadoExistente.Telefono = telefono;
                        empleadoExistente.Email = email;
                        empleadoExistente.Salario = salario;
                        empleadoExistente.FechaIngreso = DateTime.SpecifyKind(fechaIngreso, DateTimeKind.Utc);
                        empleadoExistente.Estado = estado;
                        empleadoExistente.NivelEducativo = nivelEducativo;
                        empleadoExistente.PerfilProfesional = perfilProfesional;
                        empleadoExistente.DepartamentoId = departamento.Id;
                        empleadoExistente.CargoId = cargo.Id;

                        await _unitOfWork.Empleados.UpdateAsync(empleadoExistente);
                        result.Actualizados++;
                    }
                    else
                    {
                        // Insertar
                        var nuevoEmpleado = new Empleado
                        {
                            Documento = documento,
                            Nombres = nombres,
                            Apellidos = apellidos,
                            FechaNacimiento = DateTime.SpecifyKind(fechaNacimiento, DateTimeKind.Utc),
                            Direccion = direccion,
                            Telefono = telefono,
                            Email = email,
                            Salario = salario,
                            FechaIngreso = DateTime.SpecifyKind(fechaIngreso, DateTimeKind.Utc),
                            Estado = estado,
                            NivelEducativo = nivelEducativo,
                            PerfilProfesional = perfilProfesional,
                            DepartamentoId = departamento.Id,
                            CargoId = cargo.Id
                        };

                        await _unitOfWork.Empleados.AddAsync(nuevoEmpleado);
                        result.Insertados++;
                    }
                }
                catch (Exception ex)
                {
                    result.Errores++;
                    result.Mensajes.Add($"Fila {result.TotalFilas + 1}: Error - {ex.Message}");
                }
            }

            await _unitOfWork.SaveChangesAsync();
            result.Mensajes.Insert(0, $"Importación completada: {result.Insertados} insertados, {result.Actualizados} actualizados, {result.Errores} errores");
        }
        catch (Exception ex)
        {
            result.Errores++;
            result.Mensajes.Add($"Error general al procesar el archivo: {ex.Message}");
        }

        return result;
    }

    private static EstadoEmpleado ParseEstado(string estado)
    {
        return estado.ToLower() switch
        {
            "activo" => EstadoEmpleado.Activo,
            "inactivo" => EstadoEmpleado.Inactivo,
            "vacaciones" => EstadoEmpleado.Vacaciones,
            _ => EstadoEmpleado.Activo
        };
    }

    private static NivelEducativo ParseNivelEducativo(string nivel)
    {
        return nivel.ToLower() switch
        {
            "técnico" or "tecnico" => NivelEducativo.Tecnico,
            "tecnólogo" or "tecnologo" => NivelEducativo.Tecnologo,
            "profesional" => NivelEducativo.Profesional,
            "especialización" or "especializacion" => NivelEducativo.Especializacion,
            "maestría" or "maestria" => NivelEducativo.Maestria,
            _ => NivelEducativo.Tecnico
        };
    }
}

