using CozyNestAPIHub.Models;
using MySql.Data.MySqlClient;
using System.Collections.Concurrent;
using System.Data;
using System.Xml.Linq;

namespace CozyNestAPIHub.Handlers
{
    public class ReservationHandler
    {
        private static string _connectionString;
        private static readonly SemaphoreSlim _reservationReadLock = new(1, 1);
        private static readonly SemaphoreSlim _reservationWriteLock = new(1, 1);
        private static readonly SemaphoreSlim _reservationServiceReadLock = new(1, 1);
        private static readonly SemaphoreSlim _reservationServiceWriteLock = new(1, 1);
        private static readonly SemaphoreSlim _serviceReadLock = new(1, 1);
        private static readonly SemaphoreSlim _serviceWriteLock = new(1, 1);
        private static readonly SemaphoreSlim _reservationStatusesReadLock = new(1, 1);

        public static void Initialize(string username, string password)
        {
            if (!string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("ReservationHandler is already initialized.");

            _connectionString = $"Server=localhost;Database=cozynest;User ID={username};Password={password};";
        }

        private static MySqlConnection CreateConnection() => new(_connectionString);

        public static async Task<List<Reservation>> GetReservations()
        {
            await _reservationReadLock.WaitAsync();
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                var reservations = new List<Reservation>();
                var query = "SELECT id, guest_id, room_id, check_in_date, check_out_date, status, notes, capacity FROM reservations";
                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var reservation = new Reservation
                    {
                        Id = reader.GetInt32("id"),
                        GuestId = reader.GetInt32("guest_id"),
                        RoomId = reader.GetInt32("room_id"),
                        CheckInDate = reader.GetDateTime("check_in_date"),
                        CheckOutDate = reader.GetDateTime("check_out_date"),
                        Status = reader.GetInt32("status"),
                        Notes = reader.GetString("notes"),
                        Capacity = reader.GetInt32("capacity")
                    };

                    reservations.Add(reservation);
                }

                return reservations;
            }
            finally
            {
                _reservationReadLock.Release();
            }
        }

        public static async Task<Reservation?> GetReservationById(int id)
        {

            await _reservationReadLock.WaitAsync();
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                var query = "SELECT guest_id, room_id, check_in_date, check_out_date, status, notes, capacity FROM reservations WHERE id = @id";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var reservation = new Reservation
                    {
                        Id = id,
                        GuestId = reader.GetInt32("guest_id"),
                        RoomId = reader.GetInt32("room_id"),
                        CheckInDate = reader.GetDateTime("check_in_date"),
                        CheckOutDate = reader.GetDateTime("check_out_date"),
                        Status = reader.GetInt32("status"),
                        Notes = reader.GetString("notes"),
                        Capacity = reader.GetInt32("capacity")
                    };
                    return reservation;
                }

                return null;
            }
            finally
            {
                _reservationReadLock.Release();
            }
        }

        public static async Task<Reservation?> CreateReservation(Reservation reservation)
        {
            await _reservationWriteLock.WaitAsync();
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                var query = @"INSERT INTO reservations (guest_id, room_id, check_in_date, check_out_date, status, notes, capacity) 
                              VALUES (@guestId, @roomId, @checkInDate, @checkOutDate, @status, @notes, @capacity); 
                              SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@guestId", reservation.GuestId);
                cmd.Parameters.AddWithValue("@roomId", reservation.RoomId);
                cmd.Parameters.AddWithValue("@checkInDate", reservation.CheckInDate);
                cmd.Parameters.AddWithValue("@checkOutDate", reservation.CheckOutDate);
                cmd.Parameters.AddWithValue("@status", reservation.Status);
                cmd.Parameters.AddWithValue("@notes", reservation.Notes);
                cmd.Parameters.AddWithValue("@capacity", reservation.Capacity);
                var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                reservation.Id = newId;

                return await GetReservationById(reservation.Id);
            }
            finally
            {
                _reservationWriteLock.Release();
            }
        }

        public static async Task<Reservation?> ModifyReservation(Reservation reservation)
        {
            await _reservationWriteLock.WaitAsync();
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                Console.WriteLine("Connection opened.");

                var query = @"UPDATE reservations 
                      SET guest_id = @guestId, room_id = @roomId, check_in_date = @checkInDate, 
                          check_out_date = @checkOutDate, status = @status, notes = @notes, capacity = @capacity 
                      WHERE id = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", reservation.Id);
                cmd.Parameters.AddWithValue("@guestId", reservation.GuestId);
                cmd.Parameters.AddWithValue("@roomId", reservation.RoomId);
                cmd.Parameters.AddWithValue("@checkInDate", reservation.CheckInDate);
                cmd.Parameters.AddWithValue("@checkOutDate", reservation.CheckOutDate);
                cmd.Parameters.AddWithValue("@status", reservation.Status);
                cmd.Parameters.AddWithValue("@notes", reservation.Notes ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@capacity", reservation.Capacity);

                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                Console.WriteLine($"Rows Affected: {rowsAffected}");

                if (rowsAffected > 0)
                {
                    Console.WriteLine("Rows were updated.");
                    return await GetReservationById(reservation.Id);
                }

                Console.WriteLine("No rows were updated.");
                return null;
            }
            finally
            {
                _reservationWriteLock.Release();
            }
        }

        public static async Task<List<Reservation>> GetUserReservations(User user)
        {
            await _reservationReadLock.WaitAsync();
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();
                var reservations = new List<Reservation>();
                var query = "SELECT id, room_id, check_in_date, check_out_date, status, notes, capacity FROM reservations WHERE guest_id = @guestId";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@guestId", user.Id);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var reservation = new Reservation
                    {
                        Id = reader.GetInt32("id"),
                        GuestId = user.Id,
                        RoomId = reader.GetInt32("room_id"),
                        CheckInDate = reader.GetDateTime("check_in_date"),
                        CheckOutDate = reader.GetDateTime("check_out_date"),
                        Status = reader.GetInt32("status"),
                        Notes = reader.GetString("notes"),
                        Capacity = reader.GetInt32("capacity")
                    };
                    reservations.Add(reservation);
                }
                return reservations;
            }
            finally
            {
                _reservationReadLock.Release();
            }
        }
        public static async Task<List<ReservationService>> GetReservationServices(Reservation reservation)
        {
            await _reservationServiceReadLock.WaitAsync();
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();
                var reservationServices = new List<ReservationService>();
                var query = "SELECT id, reservation_id, service_id, quantity FROM reservationservices WHERE reservation_id = @reservationId";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@reservationId", reservation.Id);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var reservationService = new ReservationService
                    {
                        Id = reader.GetInt32("id"),
                        ReservationId = reader.GetInt32("reservation_id"),
                        ServiceId = reader.GetInt32("service_id"),
                        Quantity = reader.GetInt32("quantity")
                    };
                    reservationServices.Add(reservationService);
                }
                return reservationServices;
            }
            finally
            {
                _reservationServiceReadLock.Release();
            }
        }
        public static async Task<ReservationService?> GetReservationServiceById(int id)
        {
            await _reservationServiceReadLock.WaitAsync();
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();
                var query = "SELECT reservation_id, service_id, quantity FROM reservationservices WHERE id = @id";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var reservationService = new ReservationService
                    {
                        Id = id,
                        ReservationId = reader.GetInt32("reservation_id"),
                        ServiceId = reader.GetInt32("service_id"),
                        Quantity = reader.GetInt32("quantity")
                    };
                    return reservationService;
                }
                return null;
            }
            finally
            {
                _reservationServiceReadLock.Release();
            }
        }

        public static async Task<ReservationService?> CreateReservationService(ReservationService reservationService)
        {
            await _reservationServiceWriteLock.WaitAsync();
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();
                var query = @"INSERT INTO reservationservices (reservation_id, service_id, quantity) VALUES (@reservationId, @serviceId, @quantity); SELECT LAST_INSERT_ID();";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@reservationId", reservationService.ReservationId);
                cmd.Parameters.AddWithValue("@serviceId", reservationService.ServiceId);
                cmd.Parameters.AddWithValue("@quantity", reservationService.Quantity);
                var lastId = await cmd.ExecuteScalarAsync();
                return await GetReservationServiceById(Convert.ToInt32(lastId));
            }
            finally
            {
                _reservationServiceWriteLock.Release();
            }
        }

        public static async Task<ReservationService?> ModifyReservationService(ReservationService reservationService)
        {
            await _reservationServiceWriteLock.WaitAsync();
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();
                var query = @"UPDATE reservationservices SET reservation_id = @reservationId, service_id = @serviceId, quantity = @quantity WHERE id = @id";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", reservationService.Id);
                cmd.Parameters.AddWithValue("@reservationId", reservationService.ReservationId);
                cmd.Parameters.AddWithValue("@serviceId", reservationService.ServiceId);
                cmd.Parameters.AddWithValue("@quantity", reservationService.Quantity);
                await cmd.ExecuteNonQueryAsync();
                return await GetReservationServiceById(reservationService.Id);
            }
            finally
            {
                _reservationServiceWriteLock.Release();
            }
        }

        public static async Task<bool> ServiceExists(string name)
        {
            await _serviceReadLock.WaitAsync();
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();
                var query = "SELECT COUNT(*) FROM services WHERE name = @name";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@name", name);
                var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return count > 0;
            }
            finally
            {
                _serviceReadLock.Release();
            }
        }

        public static async Task<List<Service>> GetServices()
        {
            await _serviceReadLock.WaitAsync();
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();
                var services = new List<Service>();
                var query = "SELECT id, name, description, price, is_active FROM services";
                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var service = new Service
                    {
                        Id = reader.GetInt32("id"),
                        Name = reader.GetString("name"),
                        Description = reader.GetString("description"),
                        Price = reader.GetDouble("price"),
                        IsActive = reader.GetBoolean("is_active")
                    };
                    services.Add(service);
                }
                return services;
            }
            finally
            {
                _serviceReadLock.Release();
            }
        }
        public static async Task<Service?> GetServiceById(int id)
        {
            await _serviceReadLock.WaitAsync();
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();
                var query = "SELECT name, description, price, is_active FROM services WHERE id = @id";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var service = new Service
                    {
                        Id = id,
                        Name = reader.GetString("name"),
                        Description = reader.GetString("description"),
                        Price = reader.GetDouble("price"),
                        IsActive = reader.GetBoolean("is_active")
                    };
                    return service;
                }
                return null;
            }
            finally
            {
                _serviceReadLock.Release();
            }
        }
        public static async Task<Service?> GetServiceByName(string name)
        {
            await _serviceReadLock.WaitAsync();
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();
                var query = "SELECT id, description, price, is_active FROM services WHERE name = @name";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@name", name);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var service = new Service
                    {
                        Id = reader.GetInt32("id"),
                        Name = name,
                        Description = reader.GetString("description"),
                        Price = reader.GetDouble("price"),
                        IsActive = reader.GetBoolean("is_active")
                    };
                    return service;
                }
                return null;
            }
            finally
            {
                _serviceReadLock.Release();
            }
        }
        public static async Task<Service?> CreateService(Service service)
        {
            if (await ServiceExists(service.Name))
            {
                return null;
            }
            await _serviceWriteLock.WaitAsync();
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();
                var query = @"INSERT INTO services (name, description, price, is_active) VALUES (@name, @description, @price, @is_active);";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@name", service.Name);
                cmd.Parameters.AddWithValue("@description", service.Description);
                cmd.Parameters.AddWithValue("@price", service.Price);
                cmd.Parameters.AddWithValue("@is_active", true);
                await cmd.ExecuteScalarAsync();
                return await GetServiceByName(service.Name);
            }
            finally
            {
                _serviceWriteLock.Release();
            }
        }
        public static async Task<Service?> ModifyService(Service service)
        {
            await _serviceWriteLock.WaitAsync();
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();
                var query = @"UPDATE services SET name = @name, description = @description, price = @price, is_active = @isActive WHERE id = @id";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", service.Id);
                cmd.Parameters.AddWithValue("@name", service.Name);
                cmd.Parameters.AddWithValue("@description", service.Description);
                cmd.Parameters.AddWithValue("@price", service.Price);
                cmd.Parameters.AddWithValue("@isActive", service.IsActive);
                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected > 0)
                {
                    return await GetServiceById(service.Id);
                }
                return null;
            }
            finally
            {
                _serviceWriteLock.Release();
            }
        }
        public static async Task<List<ReservationStatus>> GetReservationStatuses()
        {
            await _reservationStatusesReadLock.WaitAsync();
            try
            {
                List<ReservationStatus> reservationStatuses = new List<ReservationStatus>();
                using var connection = CreateConnection();
                await connection.OpenAsync();
                var query = @"SELECT id, description FROM reservationstatuses";
                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    reservationStatuses.Add(new ReservationStatus
                    {
                        Id = reader.GetInt32("id"),
                        Description = reader.GetString("description"),
                    });
                }
                return reservationStatuses;
            }
            finally
            {
                _reservationStatusesReadLock.Release();
            }
        }
        public static async Task<ReservationStatus?> GetReservationStatusById(int id)
        {
            await _reservationStatusesReadLock.WaitAsync();
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();
                var query = @"SELECT description FROM reservationstatuses WHERE id = @id";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new ReservationStatus
                    {
                        Id = id,
                        Description = reader.GetString("description"),
                    };
                }
                return null;
            }
            finally
            {
                _reservationStatusesReadLock.Release();
            }
        }
        public static async Task<ReservationStatus?> GetReservationStatusByDescription(string description)
        {
            await _reservationStatusesReadLock.WaitAsync();
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();
                var query = @"SELECT id FROM reservationstatuses WHERE description = @description";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@description", description);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new ReservationStatus
                    {
                        Id = reader.GetInt32("id"),
                        Description = description,
                    };
                }
                return null;

            }
            finally
            {
                _reservationStatusesReadLock.Release();
            }
        }
        public static async Task<bool> IsReservationTimeValid(Reservation newReservation)
        {
            if (newReservation.CheckInDate <= DateTime.Now.AddDays(7))
            {
                return false;
            }

            using var connection = CreateConnection();
            await connection.OpenAsync();

            var query = @"
                SELECT COUNT(*) 
                FROM reservations 
                WHERE room_id = @roomId 
                AND (
                    (@checkInDate < check_out_date AND @checkOutDate > check_in_date)
                )
                AND @checkInDate >= DATE_ADD(NOW(), INTERVAL 7 DAY)";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@roomId", newReservation.RoomId);
            cmd.Parameters.AddWithValue("@checkInDate", newReservation.CheckInDate);
            cmd.Parameters.AddWithValue("@checkOutDate", newReservation.CheckOutDate);

            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());

            return count == 0;
        }
        public static async Task<List<Room>> GetAvailableRoomsBetweenTimes(DateTime start, DateTime end)
        {
            if (start < DateTime.UtcNow.AddDays(7))
            {
                return new List<Room>();
            }

            await _reservationReadLock.WaitAsync();
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();
                var roomIds = new List<int>();
                var rooms = await RoomHandler.GetRooms();

                var query = @"
        SELECT room_id 
        FROM reservations 
        WHERE (check_in_date < @end AND check_out_date > @start)
           OR (check_out_date > DATE_SUB(@start, INTERVAL 7 DAY))";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@start", start);
                cmd.Parameters.AddWithValue("@end", end);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    roomIds.Add(reader.GetInt32("room_id"));
                }

                return rooms.Where(x => roomIds.Contains(x.Id)).ToList();
            }
            finally
            {
                _reservationReadLock.Release();
            }
        }
        
    }
}
