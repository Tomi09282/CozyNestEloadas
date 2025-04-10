import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import styles from "./Rooms.module.css";

const RoomType = {
  NORMAL: "Standard",
  DELUXE: "Deluxe",
  SUITE: "Suite",
  MAINTENANCE: "Maintenance",
  OCCUPIED: "Occupied",
  AVAILABLE: "Available",
};

// Define Room Data Type
type Room = {
  id: number;
  roomNumber: string;
  capacity: number;
  roomType: string;
  pricePerNight: number;
  description: string;
  deleted: boolean;
  status: string;
};

export const Rooms = () => {
  const [rooms, setRooms] = useState<Room[]>([]);
  const [filteredRooms, setFilteredRooms] = useState<Room[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [roomTypeFilter, setRoomTypeFilter] = useState<string>("");
  const [availabilityFilter, setAvailabilityFilter] = useState<string>("");
  const [searchText, setSearchText] = useState<string>("");
  const [minPrice, setMinPrice] = useState<number>(0);
  const [maxPrice, setMaxPrice] = useState<number>(300000);

  const [startDate, setStartDate] = useState<string>("");
  const [endDate, setEndDate] = useState<string>("");

  const today = new Date();
  const minStartDate = new Date(today);
  minStartDate.setDate(today.getDate() + 8);
  const minStartDateString = minStartDate.toISOString().split("T")[0];

  const minEndDate = new Date(minStartDate);
  minEndDate.setDate(minStartDate.getDate() + 1);
  const minEndDateString = minEndDate.toISOString().split("T")[0];

  useEffect(() => {
    if (!startDate) {
      setStartDate(minStartDateString);
    }
    if (!endDate || new Date(endDate) <= new Date(startDate)) {
      setEndDate(minEndDateString);
    }
  }, [startDate, endDate]);

  const navigate = useNavigate();

  useEffect(() => {
    const fetchRooms = async () => {
      try {
        let response;
        const requestOptions: RequestInit = {
          method: startDate && endDate ? "POST" : "GET",
          headers: {
            "Content-Type": "application/json",
          },
        };

        if (startDate && endDate) {
          requestOptions.body = JSON.stringify({
            start: startDate,
            end: endDate,
          });
        }

        response = await fetch(
          "http://localhost:5232/api/reservation/getrooms",
          requestOptions
        );

        const data = await response.json();
        if (data.rooms) {
          const mappedRooms = data.rooms.filter((room: any) => !room.deleted);
          setRooms(mappedRooms);
        }
      } catch (err) {
        setError("Failed to fetch rooms.");
      } finally {
        setLoading(false);
      }
    };

    fetchRooms();
  }, [startDate, endDate]);

  useEffect(() => {
    const applyFilters = () => {
      let filtered = rooms;

      if (roomTypeFilter) {
        filtered = filtered.filter((room) => room.roomType === roomTypeFilter);
      }

      if (availabilityFilter === "Show Available") {
        filtered = filtered.filter(
          (room) => room.status === RoomType.AVAILABLE
        );
      } else if (availabilityFilter === "Show Unavailable") {
        filtered = filtered.filter(
          (room) => room.status !== RoomType.AVAILABLE
        );
      }

      if (searchText) {
        filtered = filtered.filter(
          (room) =>
            room.roomNumber.toLowerCase().includes(searchText.toLowerCase()) ||
            room.description.toLowerCase().includes(searchText.toLowerCase())
        );
      }

      filtered = filtered.filter(
        (room) =>
          room.pricePerNight >= minPrice && room.pricePerNight <= maxPrice
      );

      setFilteredRooms(filtered);
    };

    applyFilters();
  }, [
    roomTypeFilter,
    availabilityFilter,
    searchText,
    minPrice,
    maxPrice,
    rooms,
    startDate,
    endDate,
  ]);

  const handleReservationClick = (room: Room) => {
    navigate("/reserve-room", { state: { room } });
  };

  return (
    <div className={styles.roomPage}>
      <div className={styles.Container}>
        <h1>ELÉRHETŐ SZÁLLÁSOK</h1>
        <div className={styles.SearchDiv}>
          <select
            name="roomType"
            id="roomType"
            className={styles.Select}
            onChange={(e) => setRoomTypeFilter(e.target.value)}
          >
            <option value="">Összes</option>
            <option value={RoomType.NORMAL}>{RoomType.NORMAL}</option>
            <option value={RoomType.DELUXE}>{RoomType.DELUXE}</option>
            <option value={RoomType.SUITE}>{RoomType.SUITE}</option>
          </select>

          <select
            name="roomAvailability"
            id="roomAvailability"
            className={styles.Select}
            onChange={(e) => setAvailabilityFilter(e.target.value)}
          >
            <option value="">Összes</option>
            <option value="Show Available">Elérhető</option>
            <option value="Show Unavailable">Nem Elérhető</option>
          </select>

          <input
            type="text"
            className={styles.SearchBar}
            placeholder="Keresés névre..."
            onChange={(e) => setSearchText(e.target.value)}
          />

          <div className={styles.PriceInputs}>
            <div className={styles.PriceInputsColumn}>
              <p>Ártartomány</p>

              <input
                type="number"
                className={styles.PriceInput}
                placeholder="Min Price"
                value={minPrice}
                onChange={(e) => setMinPrice(Number(e.target.value))}
              />
              <input
                type="number"
                className={styles.PriceInput}
                placeholder="Max Price"
                value={maxPrice}
                onChange={(e) => setMaxPrice(Number(e.target.value))}
              />
            </div>
          </div>

          {/* Date Pickers (Below price inputs now) */}
          <div className={styles.DatePickers}>
            <label htmlFor="startDate">Érkezés dátuma</label>
            <input
              type="date"
              id="startDate"
              value={startDate}
              onChange={(e) => setStartDate(e.target.value)}
            />
            <label htmlFor="endDate">Távozás dátuma</label>
            <input
              type="date"
              id="endDate"
              value={endDate}
              onChange={(e) => setEndDate(e.target.value)}
            />
          </div>
        </div>

        {loading ? (
          <h1>Szobák betöltése...</h1>
        ) : error ? (
          <h1 className={styles.Error}>{error}</h1>
        ) : filteredRooms.length === 0 ? (
          <h1 className={styles.Info}>
            {new Date(endDate) <= new Date(startDate)
              ? "A távozási dátumnak későbbinek kell lennie, mint az érkezési dátum. Kérjük, módosítsa a dátumokat."
              : "Nincs elérhető szoba a megadott szűrők alapján. Próbálja meg módosítani a keresési feltételeket."}
          </h1>
        ) : (
          <div className={styles.Wrapper}>
            {filteredRooms.map((room) => (
              <div key={room.id} className={styles.Card}>
                <div
                  className={`${styles.cardImage} ${
                    room.roomType === "Standard"
                      ? styles.cardImageBasic
                      : room.roomType === "Deluxe"
                      ? styles.cardImageDeluxe
                      : styles.cardImageSuite
                  }`}
                ></div>
                <div className={styles.CardPadding}>
                  <div className={styles.roomHeader}>
                    <h3 className={styles.RoomName}>#{room.roomNumber}</h3>
                    <h3>{room.roomType}</h3>
                  </div>
                  <div className={styles.RoomInfo}>
                    <p>{room.description}</p>
                    <p>{room.pricePerNight} HUF</p>
                    <p>{room.capacity} Ágy</p>
                    <p>Availability: {room.status}</p>
                  </div>
                  <button
                    className={styles.btn}
                    disabled={room.status !== RoomType.AVAILABLE}
                    onClick={() => handleReservationClick(room)}
                  >
                    {room.status === RoomType.AVAILABLE
                      ? "Foglalás"
                      : "Nem elérhető"}
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};
