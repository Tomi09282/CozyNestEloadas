using CozyNestAPIHub.Attributes;
using CozyNestAPIHub.Handlers;
using CozyNestAPIHub.Models;
using CozyNestAPIHub.RequestTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CozyNestAPIHub.Controllers
{
    /// <summary>
    /// API végpont gyűjtő a szobákkal kapcsolatos funkciókhoz.
    /// </summary>
    [Route("api/room")]
    [ApiController]
    [RequireAccessToken]
    [Role("Manager")]
    public class RoomController : ControllerBase
    {
        /// <summary>
        /// Szobák lekérdezése.
        /// </summary>
        /// <returns>Szobák listája.</returns>
        /// <response code="200">Sikeres lekérdezés.</response>
        [Route("list")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> List() 
        {
            List<Room> roomList = await RoomHandler.GetRooms();
            List<object> final = new List<object>();
            foreach (Room item in roomList)
            {
                RoomStatus? roomstatus = await RoomHandler.GetRoomStatusById(item.Status);
                RoomType? roomtype = await RoomHandler.GetRoomTypeById(item.Type);
                if (roomtype == null || roomstatus == null) { continue; }
                final.Add(new {
                    id = item.Id,
                    roomNumber = item.RoomNumber,
                    type = roomtype.Description,
                    pricePerNight = item.PricePerNight,
                    description = item.Description,
                    deleted = item.Deleted,
                    status = roomstatus.Description,
                    capacity = item.Capacity
                });
            }
            return Ok(new {
                message = "Szobák sikeresen lekérve.",
                rooms = final
            });
        }
        /// <summary>
        /// Új szoba létrehozása.
        /// </summary>
        /// <param name="request">Szoba paramétereinek formázása.</param>
        /// <returns>Szoba adatok.</returns>
        /// <response code="200">Sikeres szoba létrehozás.</response>
        /// <response code="400">Hibás kérés.</response>
        /// <response code="401">Már van ilyen szoba.</response>
        /// <response code="500">Sikertelen létrehozás.</response>
        [Route("create")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] RoomCreateRequest request)
        {
            if (request == null |
                string.IsNullOrEmpty(request.StatusDescription) ||
                string.IsNullOrEmpty(request.TypeDescription) ||
                string.IsNullOrEmpty(request.RoomNumber) ||
                request.Description == null)
            {
                return BadRequest(new
                {
                    message = "Érvénytelen kérés."
                });
            }
            RoomStatus? roomStatus = await RoomHandler.GetRoomStatusByDescription(request.StatusDescription);
            if (roomStatus == null)
            {
                return BadRequest(new
                {
                    message = "Érvénytelen szoba állapot."
                });
            }
            RoomType? roomType = await RoomHandler.GetRoomTypeByDescription(request.TypeDescription);
            if (roomType == null)
            {
                return BadRequest(new
                {
                    message = "Érvénytelen szoba típus."
                });
            }
            if (await RoomHandler.GetRoomByRoomNumber(request.RoomNumber) != null)
            {
                return Unauthorized(new
                {
                    message = "Már van ilyen számmal rendelkező szoba."
                });
            } 
            Room room = new Room()
            {
                Deleted = false,
                Description = request.Description,
                RoomNumber = request.RoomNumber,
                Status = roomStatus.Id,
                Type = roomType.Id,
                PricePerNight = request.PricePerNight,
                Capacity = request.Capacity
            };

            Room? createdRoom = await RoomHandler.CreateRoom(room);
            if (createdRoom == null) 
            {
                return StatusCode(500, new
                {
                    message = "Nem sikerült szobát létrehozni."
                });
            }
            return Ok(new
            {
                message = "Új szoba létrehozva.",
                roomData = new {
                    id = createdRoom.Id,
                    description = createdRoom.Description,
                    status = roomStatus.Description,
                    type = roomType.Description,
                    pricePerNight = createdRoom.PricePerNight,
                    deleted = createdRoom.Deleted,
                    roomNumber = createdRoom.RoomNumber,
                    capacity = createdRoom.Capacity
                }
            });
        }
        /// <summary>
        /// Szoba törlése.
        /// </summary>
        /// <param name="request">Szoba törlési paramétereinek formázása.</param>
        /// <returns>Szoba id, rendszerüzenet, státuskód.</returns>
        /// <response code="200">Sikeres törlés.</response>
        /// <response code="400">Hibás kérés.</response>
        /// <response code="401">Szoba törlés meghiúsult.</response>
        /// <response code="404">Szoba nem található.</response>
        /// <response code="500">Sikertelen törlés.</response>
        [Route("delete")]
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete([FromBody] RoomDeleteRequest request) 
        {
            if (request == null)
            {
                return BadRequest(new 
                { 
                    message = "Érvénytelen kérés."    
                });
            }
            Room? room = await RoomHandler.GetRoomById(request.RoomId);
            if (room == null) 
            { 
                return NotFound(new 
                {
                    message = "Ez a szoba nem létezik."
                });
            }
            if (room.Deleted)
            {
                return Unauthorized(new
                {
                    message = "Ez a szoba már ki van törölve."
                });
            }
            room.Deleted = true;
            if (await RoomHandler.ModifyRoom(room) == null) 
            {
                return StatusCode(500, new 
                {
                    message = "Nem sikerült törölni a szobát."
                });
            }
            return Ok(new
            {
                message = "Szoba eltörölve.",
                roomId = room.Id,
            });
        }
        /// <summary>
        /// Szoba módosítása.
        /// </summary>
        /// <param name="request">Szoba módosításának formázása.</param>
        /// <returns>Szoba adatok.</returns>
        /// <response code="200">Sikeres módosítás.</response>
        /// <response code="400">Hibás kérés.</response>
        /// <response code="404">Szoba nem található.</response>
        /// <response code="500">Sikertelen módosítás.</response>
        [Route("modify")]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Modify([FromBody] RoomModifyRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Érvénytelen kérés." });
            }

            Room? room = await RoomHandler.GetRoomById(request.RoomId);
            if (room == null) { return NotFound(new { message = "Ez a szoba nem létezik." }); }

            if (!string.IsNullOrWhiteSpace(request.StatusDescription))
            {
                RoomStatus? roomStatus = await RoomHandler.GetRoomStatusByDescription(request.StatusDescription);
                if (roomStatus == null) { return BadRequest(new { message = "Érvénytelen állapot típus." }); }
                room.Status = roomStatus.Id;
            }

            if (!string.IsNullOrWhiteSpace(request.TypeDescription))
            {
                RoomType? roomType = await RoomHandler.GetRoomTypeByDescription(request.TypeDescription);
                if (roomType == null) { return BadRequest(new { message = "Érvénytelen szoba típus." }); }
                room.Type = roomType.Id;
            }

            if (!string.IsNullOrWhiteSpace(request.RoomNumber)) room.RoomNumber = request.RoomNumber;
            if (request.PricePerNight != room.PricePerNight) room.PricePerNight = request.PricePerNight;
            if (!string.IsNullOrWhiteSpace(request.Description)) room.Description = request.Description;

            Room? updateSuccess = await RoomHandler.ModifyRoom(room);
            if (updateSuccess == null) { return StatusCode(500, new { message = "Nem sikerült frissíteni a szobát." }); }

            string roomStatusDesc = (await RoomHandler.GetRoomStatusById(room.Status))?.Description ?? "Unknown";
            string roomTypeDesc = (await RoomHandler.GetRoomTypeById(room.Type))?.Description ?? "Unknown";

            return Ok(new
            {
                message = "Szoba frissítve.",
                roomData = new
                {
                    id = room.Id,
                    roomNumber = room.RoomNumber,
                    type = roomTypeDesc,
                    pricePerNight = room.PricePerNight,
                    description = room.Description,
                    deleted = room.Deleted,
                    status = roomStatusDesc,
                    capacity = room.Capacity
                }
            });
        }

    }
}
