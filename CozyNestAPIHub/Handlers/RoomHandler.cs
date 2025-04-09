using CozyNestAPIHub.Models;
using MySql.Data.MySqlClient;
using System.Collections.Concurrent;
using System.Data;

namespace CozyNestAPIHub.Handlers
{
    public class RoomHandler
    {
        private static string _connectionString;
        private static readonly SemaphoreSlim _roomReadLock = new(1, 1);
        private static readonly SemaphoreSlim _roomWriteLock = new(1, 1);
        private static readonly SemaphoreSlim _roomTypeRead = new(1, 1);
        private static readonly SemaphoreSlim _roomStatusRead = new(1, 1);

        public static void Initialize(string username, string password)
        {
            if (!string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("RoomHandler is already initialized.");

            _connectionString = $"Server=localhost;Database=cozynest;User ID={username};Password={password};";
        }

        private static MySqlConnection CreateConnection() => new(_connectionString);

        public static async Task<List<Room>> GetRooms(bool includeDeleted = false)
        {
            var rooms = new List<Room>();
            await _roomReadLock.WaitAsync();
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                string query = "SELECT id, room_number, type, capacity, price_per_night, status, description, deleted FROM room";
                if (!includeDeleted)
                {
                    query += " WHERE deleted = 0";
                }

                using var command = new MySqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    Room room = new Room
                    {
                        Id = reader.GetInt32("id"),
                        RoomNumber = reader.GetString("room_number"),
                        Type = reader.GetInt32("type"),
                        PricePerNight = reader.GetDecimal("price_per_night"),
                        Status = reader.GetInt32("status"),
                        Description = reader.GetString("description"),
                        Deleted = reader.GetBoolean("deleted"),
                        Capacity = reader.GetInt32("capacity")
                    };
                    rooms.Add(room);
                }
            }
            finally
            {
                _roomReadLock.Release();
            }
            return rooms;
        }

        public static async Task<Room?> GetRoomById(int id)
        {

            await _roomReadLock.WaitAsync();
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                string query = "SELECT id, room_number, type, price_per_night, status, description, deleted, capacity FROM room WHERE id = @id";

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    Room room = new Room
                    {
                        Id = id,
                        RoomNumber = reader.GetString("room_number"),
                        Type = reader.GetInt32("type"),
                        PricePerNight = reader.GetDecimal("price_per_night"),
                        Status = reader.GetInt32("status"),
                        Description = reader.GetString("description"),
                        Deleted = reader.GetBoolean("deleted"),
                        Capacity = reader.GetInt32("capacity")
                    };

                    return room;
                }
            }
            finally
            {
                _roomReadLock.Release();
            }
            return null;
        }

        public static async Task<Room?> CreateRoom(Room room)
        {
            await _roomWriteLock.WaitAsync();
            try
            {
                string insertQuery = @"INSERT INTO room (room_number, type, price_per_night, status, description, deleted, capacity) 
                                       VALUES (@roomNumber, @type, @pricePerNight, @status, @description, 0, @capacity);";

                using var connection = CreateConnection();
                await connection.OpenAsync();

                using var command = new MySqlCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@roomNumber", room.RoomNumber);
                command.Parameters.AddWithValue("@type", room.Type);
                command.Parameters.AddWithValue("@pricePerNight", room.PricePerNight);
                command.Parameters.AddWithValue("@status", room.Status);
                command.Parameters.AddWithValue("@description", room.Description);
                command.Parameters.AddWithValue("@capacity", room.Capacity);

                int rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected > 0)
                {
                    room.Id = (int)command.LastInsertedId;
                    room.Deleted = false;
                    return room;
                }
            }
            finally
            {
                _roomWriteLock.Release();
            }
            return null;
        }

        public static async Task<Room?> ModifyRoom(Room room)
        {
            await _roomWriteLock.WaitAsync();
            try
            {
                string updateQuery = @"UPDATE room SET 
                                        room_number = @roomNumber, 
                                        type = @type, 
                                        price_per_night = @pricePerNight, 
                                        status = @status, 
                                        description = @description,
                                        deleted = @deleted,
                                        capacity = @capacity
                                       WHERE id = @id;";

                using var connection = CreateConnection();
                await connection.OpenAsync();

                using var command = new MySqlCommand(updateQuery, connection);
                command.Parameters.AddWithValue("@id", room.Id);
                command.Parameters.AddWithValue("@roomNumber", room.RoomNumber);
                command.Parameters.AddWithValue("@type", room.Type);
                command.Parameters.AddWithValue("@pricePerNight", room.PricePerNight);
                command.Parameters.AddWithValue("@status", room.Status);
                command.Parameters.AddWithValue("@description", room.Description);
                command.Parameters.AddWithValue("@deleted", room.Deleted);
                command.Parameters.AddWithValue("@capacity", room.Capacity);

                int rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected > 0)
                {
                    return room;
                }
            }
            finally
            {
                _roomWriteLock.Release();
            }
            return null;
        }
        public static async Task<List<RoomType>> GetRoomTypes()
        {
            var roomTypes = new List<RoomType>();
            await _roomTypeRead.WaitAsync();
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                string query = "SELECT id, description FROM roomtype";
                using var command = new MySqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    roomTypes.Add(new RoomType
                    {
                        Id = reader.GetInt32("id"),
                        Description = reader.GetString("description")
                    });
                }
            }
            finally
            {
                _roomTypeRead.Release();
            }
            return roomTypes;
        }

        public static async Task<List<RoomStatus>> GetRoomStatuses()
        {
            var roomStatuses = new List<RoomStatus>();
            await _roomStatusRead.WaitAsync();
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                string query = "SELECT id, description FROM roomstatus";
                using var command = new MySqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    roomStatuses.Add(new RoomStatus
                    {
                        Id = reader.GetInt32("id"),
                        Description = reader.GetString("description")
                    });
                }
            }
            finally
            {
                _roomStatusRead.Release();
            }
            return roomStatuses;
        }

        public static async Task<RoomType?> GetRoomTypeById(int id)
        {
            await _roomTypeRead.WaitAsync();
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                string query = "SELECT description FROM roomtype WHERE id = @id";
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new RoomType
                    {
                        Id = id,
                        Description = reader.GetString("description")
                    };
                }
                return null;
            }
            finally
            {
                _roomTypeRead.Release();
            }
        }

        public static async Task<RoomStatus?> GetRoomStatusById(int id)
        {
            await _roomStatusRead.WaitAsync();
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                string query = "SELECT id, description FROM roomstatus WHERE id = @id";
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new RoomStatus
                    {
                        Id = reader.GetInt32("id"),
                        Description = reader.GetString("description")
                    };
                }
                return null;
            }
            finally
            {
                _roomStatusRead.Release();
            }
        }

        public static async Task<RoomType?> GetRoomTypeByDescription(string description)
        {
            await _roomTypeRead.WaitAsync();
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                string query = "SELECT id, description FROM roomtype WHERE description = @description";
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@description", description);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new RoomType
                    {
                        Id = reader.GetInt32("id"),
                        Description = reader.GetString("description")
                    };
                }
                return null;
            }
            finally
            {
                _roomTypeRead.Release();
            }
        }

        public static async Task<RoomStatus?> GetRoomStatusByDescription(string description)
        {
            await _roomStatusRead.WaitAsync();
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                string query = "SELECT id, description FROM roomstatus WHERE description = @description";
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@description", description);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new RoomStatus
                    {
                        Id = reader.GetInt32("id"),
                        Description = reader.GetString("description")
                    };
                }
                return null;
            }
            finally
            {
                _roomStatusRead.Release();
            }
        }
        public static async Task<Room?> GetRoomByRoomNumber(string roomNumber) 
        { 
            await _roomReadLock.WaitAsync();
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                string query = "SELECT id, room_number, type, price_per_night, status, description FROM room WHERE room_number = @roomNumber and deleted = @deleted";

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@roomNumber", roomNumber);
                command.Parameters.AddWithValue("@deleted", false);
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    Room room = new Room
                    {
                        Id = reader.GetInt32("id"),
                        RoomNumber = roomNumber,
                        Type = reader.GetInt32("type"),
                        PricePerNight = reader.GetDecimal("price_per_night"),
                        Status = reader.GetInt32("status"),
                        Description = reader.GetString("description"),
                        Deleted = false
                    };

                    return room;
                }
            }
            finally
            {
                _roomReadLock.Release();
            }
            return null;
        }
    }
}
