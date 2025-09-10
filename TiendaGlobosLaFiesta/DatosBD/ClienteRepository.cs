using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Modelos;

namespace TiendaGlobosLaFiesta.Data
{
    public class ClienteRepository
    {
        // Crear cliente
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


        public class TopCliente
        {
            public string Nombre { get; set; }
            public decimal Total { get; set; }
        }

        public TopCliente ObtenerTopClienteMes()
        {
            string query = @"
                SELECT TOP 1 c.primerNombre + ' ' + c.apellidoP AS Nombre, SUM(v.total) AS Total
                FROM Cliente c
                JOIN Venta v ON c.clienteId = v.clienteId
                WHERE MONTH(v.fecha) = MONTH(GETDATE()) AND YEAR(v.fecha) = YEAR(GETDATE())
                GROUP BY c.primerNombre, c.apellidoP
                ORDER BY SUM(v.total) DESC";

            DataTable dt = DbHelper.ExecuteQuery(query);
            if (dt.Rows.Count == 0) return null;

            return new TopCliente
            {
                Nombre = dt.Rows[0]["Nombre"].ToString(),
                Total = Convert.ToDecimal(dt.Rows[0]["Total"])
            };
        }


        public class ClienteFrecuente
        {
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public string NombreCompleto() => $"{Nombre} {Apellido}";
        }

        // Clientes frecuentes (ejemplo: que hayan comprado >2 veces)
        public List<ClienteFrecuente> ObtenerClientesFrecuentesDetalle()
        {
            string query = @"
                SELECT TOP 10 c.primerNombre, c.apellidoP, COUNT(v.ventaId) AS Compras
                FROM Cliente c
                JOIN Venta v ON c.clienteId = v.clienteId
                GROUP BY c.primerNombre, c.apellidoP
                HAVING COUNT(v.ventaId) > 2
                ORDER BY COUNT(v.ventaId) DESC";

            DataTable dt = DbHelper.ExecuteQuery(query);

            var lista = new List<ClienteFrecuente>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new ClienteFrecuente
                {
                    Nombre = row["primerNombre"].ToString(),
                    Apellido = row["apellidoP"].ToString()
                });
            }

            return lista;
        }



        // Actualizar cliente
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

        // Eliminar cliente
        public bool EliminarCliente(string clienteId)
        {
            string query = "DELETE FROM Cliente WHERE clienteId=@id";
            var parametros = new[] { new SqlParameter("@id", clienteId) };

            return DbHelper.ExecuteNonQuery(query, parametros) > 0;
        }

        // Obtener todos los clientes
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

        // Buscar cliente por ID
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