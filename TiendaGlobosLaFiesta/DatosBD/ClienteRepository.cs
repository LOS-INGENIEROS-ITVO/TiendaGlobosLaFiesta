using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Modelos;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Data
{
    public class ClienteRepository
    {
        public bool AgregarCliente(Cliente cliente)
        {
            string query = @"INSERT INTO Cliente (clienteId, primerNombre, segundoNombre, apellidoP, apellidoM, telefono)
                             VALUES (@id, @nombre1, @nombre2, @apellidoP, @apellidoM, @telefono)";

            var parametros = new[]
            {
                new SqlParameter("@id", cliente.ClienteId),
                new SqlParameter("@nombre1", cliente.PrimerNombre),
                new SqlParameter("@nombre2", (object?)cliente.SegundoNombre ?? DBNull.Value),
                new SqlParameter("@apellidoP", cliente.ApellidoP),
                new SqlParameter("@apellidoM", cliente.ApellidoM),
                new SqlParameter("@telefono", (object?)cliente.Telefono ?? DBNull.Value)
            };

            return DbHelper.ExecuteNonQuery(query, parametros) > 0;
        }

        public bool ActualizarCliente(Cliente cliente)
        {
            string query = @"UPDATE Cliente 
                             SET primerNombre=@nombre1, segundoNombre=@nombre2, 
                                 apellidoP=@apellidoP, apellidoM=@apellidoM, telefono=@telefono
                             WHERE clienteId=@id";

            var parametros = new[]
            {
                new SqlParameter("@id", cliente.ClienteId),
                new SqlParameter("@nombre1", cliente.PrimerNombre),
                new SqlParameter("@nombre2", (object?)cliente.SegundoNombre ?? DBNull.Value),
                new SqlParameter("@apellidoP", cliente.ApellidoP),
                new SqlParameter("@apellidoM", cliente.ApellidoM),
                new SqlParameter("@telefono", (object?)cliente.Telefono ?? DBNull.Value)
            };

            return DbHelper.ExecuteNonQuery(query, parametros) > 0;
        }

        public bool EliminarCliente(string clienteId)
        {
            string query = "DELETE FROM Cliente WHERE clienteId=@id";
            var parametros = new[] { new SqlParameter("@id", clienteId) };
            return DbHelper.ExecuteNonQuery(query, parametros) > 0;
        }


        public List<Cliente> ObtenerClientes()
        {
            string query = "SELECT clienteId, primerNombre, segundoNombre, apellidoP, apellidoM, telefono FROM Cliente";
            DataTable dt = DbHelper.ExecuteQuery(query);

            var lista = new List<Cliente>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new Cliente
                {
                    ClienteId = row["clienteId"].ToString(),
                    PrimerNombre = row["primerNombre"].ToString(),
                    SegundoNombre = row["segundoNombre"] == DBNull.Value ? null : row["segundoNombre"].ToString(),
                    ApellidoP = row["apellidoP"].ToString(),
                    ApellidoM = row["apellidoM"].ToString(),
                    Telefono = row["telefono"] == DBNull.Value ? (long?)null : Convert.ToInt64(row["telefono"])
                });
            }
            return lista;
        }

        public Cliente? ObtenerClientePorId(string clienteId)
        {
            string query = @"SELECT clienteId, primerNombre, segundoNombre, apellidoP, apellidoM, telefono
                             FROM Cliente WHERE clienteId=@id";

            var parametros = new[] { new SqlParameter("@id", clienteId) };
            DataTable dt = DbHelper.ExecuteQuery(query, parametros);

            if (dt.Rows.Count == 0) return null;

            DataRow row = dt.Rows[0];
            return new Cliente
            {
                ClienteId = row["clienteId"].ToString(),
                PrimerNombre = row["primerNombre"].ToString(),
                SegundoNombre = row["segundoNombre"] == DBNull.Value ? null : row["segundoNombre"].ToString(),
                ApellidoP = row["apellidoP"].ToString(),
                ApellidoM = row["apellidoM"].ToString(),
                Telefono = row["telefono"] == DBNull.Value ? (long?)null : Convert.ToInt64(row["telefono"])
            };
        }
    }
}