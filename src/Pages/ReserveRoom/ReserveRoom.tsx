import { useEffect, useState } from "react";
import { useLocation } from "react-router-dom";
import styles from "./ReserveRoom.module.css";

const BASEURL = "http://localhost:5232";

const ReserveRoom = () => {
  const location = useLocation();
  const room = location.state?.room;

  const [checkInDate, setCheckInDate] = useState<string>(new Date().toISOString().split("T")[0]);
  const [checkOutDate, setCheckOutDate] = useState<string>(new Date().toISOString().split("T")[0]);
  const [notes, setNotes] = useState<string>("");
  const [services, setServices] = useState<number[]>([]);
  const [serviceOptions, setServiceOptions] = useState<any[]>([]);
  const [guests, setGuests] = useState<number>(1);

  useEffect(() => {
    const fetchServices = async () => {
      try {
        const response = await fetch(`${BASEURL}/api/service/services`);
        const data = await response.json();
        if (data.services) {
          setServiceOptions(data.services);
        }
      } catch (error) {
        console.error("Failed to fetch services:", error);
      }
    };
    fetchServices();
  }, []);

  const calculateTotalPrice = () => {
    const checkIn = new Date(checkInDate);
    const checkOut = new Date(checkOutDate);
    const nights = Math.max((checkOut.getTime() - checkIn.getTime()) / (1000 * 60 * 60 * 24), 1);
    const roomPrice = room?.pricePerNight * nights;
    const servicePrice = services.reduce((total, serviceId) => {
      const option = serviceOptions.find((s) => s.id === serviceId);
      return total + (option ? option.price * nights * guests : 0);
    }, 0);
    return roomPrice + servicePrice;
  };

  const handleServiceChange = (serviceId: number) => {
    setServices((prev) =>
      prev.includes(serviceId) ? prev.filter((id) => id !== serviceId) : [...prev, serviceId]
    );
  };

  const handleReservation = async () => {
    if (!room || !checkInDate || !checkOutDate) return;

    const reservationData = {
      roomNumber: room.roomNumber,
      checkInDate,
      checkOutDate,
      services: services.map((id) => ({
        serviceId: id,
        quantity: 1,
      })),
      notes,
      capacity: guests,
    };
console.log(reservationData)
    try {
      const token = localStorage.getItem("accessToken");
      if (!token) {
        alert("You must be logged in to make a reservation.");
        return;
      }

      const response = await fetch(`${BASEURL}/api/reservation/reserve`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(reservationData),
      });

      const data = await response.json();
      if (data.success) {
        alert("Room reserved successfully!");
      } else {
        alert(data.message);
      }
    } catch (error) {
      console.error(error);
      alert("Error making reservation.");
    }
  };

  return (
    <div className={styles.reservePage}>
      <div className={styles.roomInfo}>
        <div className={styles.infoBorder}>
          <h1>Szoba foglalás</h1>
          <div className={styles.roomDetails}>
            <h3>Room #{room?.roomNumber}</h3>
            <p>{room?.description}</p>
            <p>Ár: {room?.pricePerNight} HUF per night</p>
            <p>Max-Férőhely: {room?.capacity} Ágy</p>
          </div>

          <div className={styles.datePicker}>
            <label>Érkezés</label>
            <input type="date" value={checkInDate} onChange={(e) => setCheckInDate(e.target.value)} />
            <label>Távozás</label>
            <input type="date" value={checkOutDate} onChange={(e) => setCheckOutDate(e.target.value)} />
            <label>Vendégek</label>
            <input type="number" min="1" max={room?.capacity} value={guests} onChange={(e) => setGuests(parseInt(e.target.value) || 1)} />
          </div>

          <div className={styles.services}>
            <h3>Szolgáltatások (Naponta)</h3>
            <div className={styles.serviceGrid}>
              {serviceOptions.map((service) => (
                <div key={service.id} className={styles.serviceItem}>
                  <label className={styles.checkboxS}>
                    <input
                      type="checkbox"
                      checked={services.includes(service.id)}
                      onChange={() => handleServiceChange(service.id)}
                    />
                    <p>{service.name} ({service.price} HUF)</p>
                    
                  </label>
                </div>
              ))}
            </div>
          </div>

          <div className={styles.notes}>
            <label>Notes</label>
            <textarea value={notes} onChange={(e) => setNotes(e.target.value)} />
          </div>

          <div className={styles.totalPrice}>
            <h3>Total Price: {calculateTotalPrice()} HUF</h3>
          </div>

          <button className={styles.reserveBtn} onClick={handleReservation}>Reserve</button>
        </div>
      </div>
    </div>
  );
};

export default ReserveRoom;