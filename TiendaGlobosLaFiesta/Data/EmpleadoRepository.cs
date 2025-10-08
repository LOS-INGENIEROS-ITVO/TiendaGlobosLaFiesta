using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Models.Empleados;

namespace TiendaGlobosLaFiesta.Data
{
    public class EmpleadoRepository
    {
        public List<Empleado> ObtenerEmpleados(bool soloActivos = true)
        {
            string query = "SELECT empleadoId, primerNombre, segundoNombre, apellidoP, apellidoM, Activo FROM Empleado";
            if (soloActivos) query += " WHERE Activo = 1";

            DataTable dt = DbHelper.ExecuteQuery(query);
            var lista = new List<Empleado>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new Empleado
                {
                    EmpleadoId = Convert.ToInt32(row["empleadoId"]),
                    PrimerNombre = row["primerNombre"].ToString(),
                    SegundoNombre = row["segundoNombre"].ToString(),
                    ApellidoP = row["apellidoP"].ToString(),
                    ApellidoM = row["apellidoM"].ToString(),
                    Activo = Convert.ToBoolean(row["Activo"])
                });
            }
            return lista;
        }

        public Empleado? ObtenerEmpleadoPorId(int empleadoId)
        {
            string query = "SELECT empleadoId, primerNombre, segundoNombre, apellidoP, apellidoM, Activo FROM Empleado WHERE empleadoId=@id";
            var parametros = new[] { new SqlParameter("@id", empleadoId) };
            DataTable dt = DbHelper.ExecuteQuery(query, parametros);
            if (dt.Rows.Count == 0) return null;

            var row = dt.Rows[0];
            return new Empleado
            {
                EmpleadoId = Convert.ToInt32(row["empleadoId"]),
                PrimerNombre = row["primerNombre"].ToString(),
                SegundoNombre = row["segundoNombre"].ToString(),
                ApellidoP = row["apellidoP"].ToString(),
                ApellidoM = row["apellidoM"].ToString(),
                Activo = Convert.ToBoolean(row["Activo"])
            };
        }
    }
}