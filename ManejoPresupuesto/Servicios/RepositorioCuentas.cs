using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioCuentas
    {
        Task Actualizar(CuentaCreacionViewModel cuenta);
        Task Borrar(int id);
        Task<IEnumerable<Cuenta>> Buscar(int usuarioId);
        Task Crear(Cuenta cuenta);
        Task<Cuenta> ObtenerPorId(int id, int usuarioId);
    }


    public class RepositorioCuentas : IRepositorioCuentas
    {
        private readonly string connectionString;

        public RepositorioCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Cuenta cuenta)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(
                @"INSERT INTO Cuentas1 (Nombre, TipoCuentaId, Descripcion, Balance)
                    VALUES (@Nombre, @TipoCuentaId, @Descripcion, @Balance);
                    SELECT SCOPE_IDENTITY();", cuenta);

            cuenta.Id = id;
        }
        public async Task<IEnumerable<Cuenta>> Buscar(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Cuenta>
                (@"SELECT Cuentas1.Id, Cuentas1.Nombre, Balance, tc.Nombre AS TipoCuenta
                 FROM Cuentas1
                 INNER JOIN TiposCuentas1 tc
                 ON tc.Id = Cuentas1.TipoCuentaId
                 WHERE tc.UsuarioId = @UsuarioId
                 ORDER BY tc.Orden
                 ", new {usuarioId});
        }

        public async Task<Cuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Cuenta>
                (@"SELECT Cuentas1.Id, Cuentas1.Nombre, Balance, Descripcion, TipoCuentaId
                 FROM Cuentas1
                 INNER JOIN TiposCuentas1 tc
                 ON tc.Id = Cuentas1.TipoCuentaId
                 WHERE tc.UsuarioId = @UsuarioId AND Cuentas1.Id = @Id", new {id, usuarioId});
        }
        public async Task Actualizar(CuentaCreacionViewModel cuenta)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE Cuentas1
                                    SET Nombre = @Nombre, Balance = @Balance, Descripcion = @Descripcion,
                                    TipoCuentaId = @TipoCuentaId
                                    WHERE Id = @Id;", cuenta);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("DELETE Cuentas1 WHERE Id = @Id", new { id });
        }

    }
}
