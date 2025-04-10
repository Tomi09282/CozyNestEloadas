import React, { useEffect, useState } from "react";
import styles from "./Reservations.module.css";

type Reservation = {
  id: number;
  roomId: number;
  roomDescription: string;
  roomNumber: string;
  roomType: string;
  checkInDate: string;
  checkOutDate: string;
  status: number;
  notes?: string;
};

type ReservationsResponse = {
  message: string;
  reservations: Reservation[];
};

const Reservations = () => {
  const [reservations, setReservations] = useState<Reservation[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchReservations = async () => {
      try {
        const token = localStorage.getItem("accessToken");
        if (!token) {
          throw new Error("No access token found");
        }

        const response = await fetch(
          "http://localhost:5232/api/reservation/getreservations",
          {
            method: "GET",
            headers: {
              Authorization: `Bearer ${token}`,
              "Content-Type": "application/json",
            },
          }
        );

        if (!response.ok) {
          throw new Error(`HTTP error! Status: ${response.status}`);
        }

        const data: ReservationsResponse = await response.json();
        setReservations(data.reservations || []);
      } catch (err) {
        setError((err as Error).message);
      } finally {
        setLoading(false);
      }
    };

    fetchReservations();
  }, []);

  const cancelReservation = async (reservationId: number) => {
    try {
      const token = localStorage.getItem("accessToken");
      if (!token) {
        throw new Error("No access token found");
      }

      const response = await fetch(
        "http://localhost:5232/api/reservation/cancel",
        {
          method: "POST",
          headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json",
          },
          body: JSON.stringify({ reservationId }),
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! Status: ${response.status}`);
      }

      // Remove canceled reservation from state
      setReservations((prev) => prev.filter((res) => res.id !== reservationId));
    } catch (err) {
      alert((err as Error).message);
    }
  };

  if (loading) {
    return <p className={styles.loading}>Foglalások betöltése...</p>;
  }
  if (error) {
    return <p className={styles.error}>Error: {error}</p>;
  }

  return (
    <div className={styles.container}>
      <h2 className={styles.heading}>Foglalásaid</h2>
      {reservations.length === 0 ? (
        <p className={styles.noReservations}>Nem található foglalás.</p>
      ) : (
        <div className={styles.cardWrapper}>
          {reservations.map((reservation) => (
            <div key={reservation.id} className={styles.card}>
              <div
                className={`${styles.cardImage} ${
                  reservation.roomType === "Standard"
                    ? styles.cardImageBasic
                    : reservation.roomType === "Deluxe"
                    ? styles.cardImageDeluxe
                    : styles.cardImageSuite
                }`}
              ></div>
              <div className={styles.cardBody}>
                <div className={styles.cardHeader}>
                  <span className={styles.roomId}>Szoba</span>
                  <span className={styles.roomNumber}>{reservation.roomNumber}</span>
                </div>
                <div className={styles.cardBody}>
                  <div className={styles.roomDescription}>
                    <span>{reservation.roomDescription}</span>
                  </div>
                  <div className={styles.dates}>
                    <span>
                      Érkezés: {new Date(reservation.checkInDate).toLocaleString()}
                    </span>
                    <span>
                      Távozás: {new Date(reservation.checkOutDate).toLocaleString()}
                    </span>
                  </div>
                  <div className={styles.status}>Státusz: {reservation.status}</div>
                  {reservation.notes && (
                    <p className={styles.notes}>Jegyzet: {reservation.notes}</p>
                  )}
                  <button
                    className={styles.cancelButton}
                    onClick={() => cancelReservation(reservation.id)}
                  >
                    Foglalás lemondása
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default Reservations;