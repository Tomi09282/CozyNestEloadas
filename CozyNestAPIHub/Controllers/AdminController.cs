using Microsoft.AspNetCore.Mvc;
using CozyNestAPIHub.RequestTypes;
using CozyNestAPIHub.Handlers;
using CozyNestAPIHub.Models;
using CozyNestAPIHub.Attributes;

namespace CozyNestAPIHub.Controllers
{
    /// <summary>
    /// API végpont gyűjtő adminisztrációs funkciókhoz.
    /// </summary>
    [Route("api/admin")]
    [ApiController]
    [RequireAccessToken]
    public class AdminController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AdminController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        /// <summary>
        /// Lekéri az összes felhasználót.
        /// </summary>
        /// <returns>Felhasználók listája.</returns>
        /// <response code="200">Sikeres kérés.</response>
        [Route("getusers")]
        [HttpGet]
        [Role("Manager", "Receptionist")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsers()
        {
            List<User> userList = await UserHandler.GetUsers();
            List<object> usersFinal = new List<object>();
            foreach (User loopedUser in userList)
            {
                usersFinal.Add(new
                {
                    id = loopedUser.Id,
                    email = loopedUser.Email,
                    username = loopedUser.Username,
                    firstName = loopedUser.FirstName,
                    lastName = loopedUser.LastName,
                    closed = loopedUser.Closed,
                    joinDate = loopedUser.JoinDate,
                    roleName = (await UserHandler.GetRoleById(loopedUser.RoleId)).Name
                });
            }
            return Ok(new { message = "Sikeres lekérés.", users = usersFinal });
        }
        /// <summary>
        /// Lekéri az összes szerepkört.
        /// </summary>
        /// <returns>Szerepkörök listája</returns>
        /// <response code="200">Sikeres kérés.</response>
        [Route("getroles")]
        [HttpGet]
        [Role("Manager", "Receptionist")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRoles()
        {
            List<Role> roleList = await UserHandler.GetRoles();
            List<object> rolesFinal = new List<object>();
            foreach (Role loopedRole in roleList)
            {
                rolesFinal.Add(loopedRole.Name);
            }
            return Ok(new { message = "Sikeresen lekérve az összes szerepkör.", roles = rolesFinal });
        }
        /// <summary>
        /// Módosítja a felhasználó adatait.
        /// </summary>
        /// <param name="request">A módosítási adatok formázása.</param>
        /// <returns>Rendszerüzenet, felhasználói adatok, státuszkód.</returns>
        /// <response code="200">Sikeres módosítás.</response>
        /// <response code="404">Nem található felhasználó vagy szerepkör.</response>
        /// <response code="500">Sikertelen módosítás.</response>
        [Route("modifyuser")]
        [HttpPut]
        [Role("Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ModifyUser([FromBody] UserUpdateRequest request)
        {
            User? user = await UserHandler.GetUserById(request.Id);
            if (user == null)
            {
                return NotFound(new { message = "Felhasználó nem található." });
            }
            Role? role = await UserHandler.GetRoleByName(request.RoleName);
            if (role == null)
            {
                return NotFound(new { message = "Szerepkör nem található." });
            }
            user.Username = request.Username;
            user.Closed = request.Closed;
            user.RoleId = role.Id;
            if (request.PasswordReset)
            {
                user.HashedPassword = HashPassword(GenerateRandomPassword());
            }
            user = await UserHandler.ModifyUser(user);
            if (user == null)
            {
                return StatusCode(500, new { message = "Hiba történt a felhasználó módosítása közben." });
            }
            return Ok(new
            {
                message = "Felhasználó módosítva.",
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    username = user.Username,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    closed = user.Closed,
                    joinDate = user.JoinDate,
                    roleName = (await UserHandler.GetRoleById(user.RoleId)).Name
                }
            });
        }
        /// <summary>
        /// Létrehoz egy felhasználót.
        /// </summary>
        /// <param name="request">Felhasználó létrehozásának formázása.</param>
        /// <returns>Felhasználói adatok, rendszerüzenetek, státuszkód.</returns>
        /// <response code="200">Sikeres felhasználó hozzáadás.</response>
        /// <response code="401">Már létezik ez a felhasználó.</response>
        /// <response code="404">Ilyen szerepkör nem létezik.</response>
        /// <response code="500">Hiba lépett fel a felhasználó hozzáadása során.</response>
        [Route("adduser")]
        [HttpPost]
        [Role("Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddUser([FromBody] AdminRegisterRequest request)
        {

            if (await UserHandler.UserExists(request.Username, request.Email))
            {
                return Unauthorized(new
                {
                    message = "Ez a felhasználónév vagy email már foglalt."
                });
            }
            Role? role = await UserHandler.GetRoleByName(request.Role);
            if (role == null)
            {
                return NotFound(new
                {
                    message = "A megadott szerepkör nem található."
                });
            }
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                HashedPassword = HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Address = request.Address,
                JoinDate = DateTime.UtcNow,
                RoleId = role.Id
            };
            var createdUser = await UserHandler.CreateUser(user);
            if (createdUser == null)
            {
                return StatusCode(500, new
                {
                    message = "Sikertelen regisztráció."
                });
            }

            return Ok(new
            {
                message = "Sikeres regisztráció.",
                userData = new
                {
                    id = createdUser.Id,
                    email = createdUser.Email,
                    username = createdUser.Username,
                    firstName = createdUser.FirstName,
                    lastName = createdUser.LastName,
                    closed = createdUser.Closed,
                    joinDate = createdUser.JoinDate,
                    roleName = role.Name,
                }
            });
        }
        /// <summary>
        /// Új foglalást vesz fel.
        /// </summary>
        /// <param name="request">Foglalási adatok szerkezete.</param>
        /// <returns>Foglalási adatok, rendszerüzenetek, státuszkód.</returns>
        /// <response code="200">Sikeres foglalás.</response>
        /// <response code="400">Hibás kérés.</response>
        /// <response code="404">Nem található a szoba.</response>
        /// <response code="500">Sikertelen foglalás.</response>
        [Route("addreservation")]
        [HttpPost]
        [Role("Manager", "Receptionist")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddReservation([FromBody] ReservationAdminRequest request)
        {
            User? user = await UserHandler.GetUserById(request.UserId);
            if (user == null)
            {
                return NotFound(new
                {
                    message = "Nincs ilyen felhasználó."
                });
            }
            Room? room = await RoomHandler.GetRoomByRoomNumber(request.RoomNumber);
            if (room == null)
            {
                return NotFound(new
                {
                    message = "Nincs ilyen szoba."
                });
            }
            if (request.Capacity < 1)
            {
                return BadRequest(new
                {
                    message = "Nem lehet a foglaláshoz tartozó személyek mennyisége kisebb, mint 1."
                });
            }
            if (room.Capacity < request.Capacity)
            {
                return BadRequest(new
                {
                    message = "Nem lehet nagyobb a foglaláshoz tartozó személyek száma, mint amit a szoba képes támogatni."
                });
            }
            ReservationStatus? rStatus = await ReservationHandler.GetReservationStatusByDescription("Incomplete");
            Reservation reservation = new Reservation
            {
                GuestId = user.Id,
                RoomId = room.Id,
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate,
                Status = rStatus.Id,
                Notes = request.Notes,
                Capacity = request.Capacity
            };
            if (!await ReservationHandler.IsReservationTimeValid(reservation))
            {
                return BadRequest(new
                {
                    message = "Ennek a foglalásnak az időpontja más foglalással ütközik."
                });
            }
            Reservation? createdReservation = await ReservationHandler.CreateReservation(reservation);
            if (createdReservation == null)
            {
                return StatusCode(500, new
                {
                    message = "Nem sikerült lefoglalni a szobát."
                });
            }
            foreach (var item in request.Services)
            {
                await ReservationHandler.CreateReservationService(new ReservationService() { Quantity = item.Quantity, ReservationId = createdReservation.Id, ServiceId = item.ServiceId });
            }
            return Ok(new
            {
                message = "Sikeres foglalás.",
                reservationData = new
                {
                    id = createdReservation.Id,
                    roomNumber = room.RoomNumber,
                    checkInDate = createdReservation.CheckInDate,
                    checkOutDate = createdReservation.CheckOutDate,
                    status = rStatus.Description,
                    notes = createdReservation.Notes,
                    capacity = createdReservation.Capacity
                },
            });
        }
        /// <summary>
        /// Foglalás lemondása.
        /// </summary>
        /// <param name="request">Foglalás lemondási kérés szerkezet.</param>
        /// <returns>Foglalás adatai, rendszerüzenet, státuszkód.</returns>
        /// <response code="200">Sikeres lemondás.</response>
        /// <response code="400">Hibás kérés.</response>
        /// <response code="404">Nem található a foglalás.</response>
        /// <response code="500">Sikertelen lemondás.</response>
        [Route("cancelreservation")]
        [HttpDelete]
        [Role("Manager", "Receptionist")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelReservation([FromBody] ReservationCancelRequest request)
        {
            Reservation? reservation = await ReservationHandler.GetReservationById(request.ReservationId);
            if (reservation == null)
            {
                return NotFound(new
                {
                    message = "Nincs ilyen foglalás."
                });
            }
            List<ReservationStatus> reservationStatuses = await ReservationHandler.GetReservationStatuses();
            string resDesc = reservationStatuses.First(x => x.Id == reservation.Status).Description;
            if (resDesc == "Cancelled")
            {
                return BadRequest(new
                {
                    message = "A foglalás már lemondottnak számít."
                });
            }
            if (resDesc == "Complete")
            {
                return BadRequest(new
                {
                    message = "Nem lehet befejezett foglalást lemondani."
                });
            }
            reservation.Status = reservationStatuses.First(x => x.Description == "Cancelled").Id;
            Room? room = await RoomHandler.GetRoomById(reservation.RoomId);
            Reservation? updatedReservation = await ReservationHandler.ModifyReservation(reservation);
            if (updatedReservation == null)
            {
                return StatusCode(500, new
                {
                    message = "Nem sikerült a foglalást lemondani."
                });
            }
            return Ok(new
            {
                message = "Foglalás sikeresen lemondva.",
                reservationData = new
                {
                    id = updatedReservation.Id,
                    roomNumber = room.RoomNumber,
                    checkInDate = updatedReservation.CheckInDate,
                    checkOutDate = updatedReservation.CheckOutDate,
                    status = "Cancelled",
                    notes = updatedReservation.Notes,
                    capacity = updatedReservation.Capacity
                }
            });
        }
        /// <summary>
        /// Lekéri az összes foglalást.
        /// </summary>
        /// <returns>Foglalások listája.</returns>
        /// <response code="200">Sikeres lekérés.</response>
        [Route("getreservations")]
        [HttpGet]
        [Role("Manager", "Receptionist")]
        public async Task<IActionResult> GetReservations()
        {
            List<Reservation> reservations = await ReservationHandler.GetReservations();
            List<object> finalList = new List<object>();
            foreach (Reservation item in reservations)
            {
                Room? room = await RoomHandler.GetRoomById(item.RoomId);
                if (room == null) continue;
                RoomType? rType = await RoomHandler.GetRoomTypeById(room.Type);
                if (rType == null) continue;
                RoomStatus? rStatus = await RoomHandler.GetRoomStatusById(room.Status);
                if (rStatus == null) continue;
                ReservationStatus? resStatus = await ReservationHandler.GetReservationStatusById(item.Status);
                if (resStatus == null || resStatus.Description == "Complete" || resStatus.Description == "Cancelled") continue;
                finalList.Add(new
                {
                    id = item.Id,
                    roomNumber = room.RoomNumber,
                    roomDescription = room.Description,
                    roomType = rType.Description,
                    checkInDate = item.CheckInDate,
                    checkOutDate = item.CheckOutDate,
                    status = rStatus.Description,
                    capacity = item.Capacity,
                    notes = item.Notes
                });
            }
            return Ok(new
            {
                message = "Foglalások sikeresen lekérve.",
                reservations = finalList
            });
        }
        /// <summary>
        /// Lekéri a felhasználóhoz tartozó foglalásokat.
        /// </summary>
        /// <returns>Foglalások listája.</returns>
        /// <response code="200">Sikeres lekérés.</response>
        [Route("getreservations")]
        [HttpPost]
        [Role("Manager", "Receptionist")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReservations([FromBody] GetReservationsByUserIdRequest request)
        {
            User? user = await UserHandler.GetUserById(request.Id);
            if (user == null)
            {
                return NotFound(new
                {
                    message = "Nincs ilyen felhasználó."
                });
            }
            List<Reservation> reservations = await ReservationHandler.GetUserReservations(user);
            List<object> finalList = new List<object>();
            foreach (var item in reservations)
            {
                Room? room = await RoomHandler.GetRoomById(item.RoomId);
                if (room == null) continue;
                RoomType? rType = await RoomHandler.GetRoomTypeById(room.Type);
                if (rType == null) continue;
                RoomStatus? rStatus = await RoomHandler.GetRoomStatusById(room.Status);
                if (rStatus == null) continue;
                ReservationStatus? resStatus = await ReservationHandler.GetReservationStatusById(item.Status);
                if (resStatus == null || resStatus.Description == "Complete" || resStatus.Description == "Cancelled") continue;
                finalList.Add(new
                {
                    id = item.Id,
                    roomNumber = room.RoomNumber,
                    roomDescription = room.Description,
                    roomType = rType.Description,
                    checkInDate = item.CheckInDate,
                    checkOutDate = item.CheckOutDate,
                    status = rStatus.Description,
                    capacity = item.Capacity,
                    notes = item.Notes
                });
            }
            return Ok(new
            {
                message = "Foglalások sikeresen lekérve.",
                reservations = finalList
            });
        }
        /// <summary>
        /// Hozzáad egy új szolgáltatást.
        /// </summary>
        /// <param name="request">Szolgáltatás adatainak szerkezete.</param>
        /// <returns>Szolgáltatás adatai, rendszerüzenet, státuszkód.</returns>
        /// <response code="200">Sikeres hozzáadás.</response>
        /// <response code="400">Hibás kérés.</response>
        /// <response code="500">Sikertelen hozzáadás.</response>
        [Route("addservice")]
        [HttpPost]
        [Role("Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddService([FromBody] AddServiceRequest request)
        {
            Service? service = await ReservationHandler.GetServiceByName(request.Name);
            if (service != null)
            {
                return BadRequest(new
                {
                    message = "Ilyen szolgáltatás már létezik."
                });
            }
            Service newService = new Service
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price
            };
            Service? createdService = await ReservationHandler.CreateService(newService);
            if (createdService == null)
            {
                return StatusCode(500, new
                {
                    message = "Nem sikerült a szolgáltatás hozzáadása."
                });
            }
            return Ok(new
            {
                message = "Szolgáltatás sikeresen hozzáadva.",
                serviceData = new
                {
                    id = createdService.Id,
                    name = createdService.Name,
                    description = createdService.Description,
                    price = createdService.Price,
                    is_active = true
                }
            });
        }
        /// <summary>
        /// Módosít egy szolgáltatást.
        /// </summary>
        /// <param name="request">Szolgáltatás módosításának szerkezete.</param>
        /// <returns>Frissített adatok, rendszerüzenet, státuszkód.</returns>
        /// <response code="200">Sikeres módosítás.</response>
        /// <response code="404">Nem található a szolgáltatás.</response>
        /// <response code="500">Sikertelen módosítás.</response>
        [Route("modifyservice")]
        [HttpPost]
        [Role("Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ModifyService([FromBody] ModifyServiceRequest request)
        {
            Service? service = await ReservationHandler.GetServiceById(request.Id);
            if (service == null)
            {
                return NotFound(new
                {
                    message = "Nincs ilyen szolgáltatás."
                });
            }
            service.Name = request.Name;
            service.Description = request.Description;
            service.Price = request.Price;
            service.IsActive = request.IsActive;
            Service? updatedService = await ReservationHandler.ModifyService(service);
            if (updatedService == null)
            {
                return StatusCode(500, new
                {
                    message = "Nem sikerült a szolgáltatás módosítása."
                });
            }
            return Ok(new
            {
                message = "Szolgáltatás sikeresen módosítva.",
                serviceData = new
                {
                    id = updatedService.Id,
                    name = updatedService.Name,
                    description = updatedService.Description,
                    price = updatedService.Price
                }
            });
        }
    }
}
