import { useState } from "react";
import styles from "./CreateRoom.module.css";

const API_URL = "http://localhost:5232";

const CreateRoom = () => {
  const [roomNumber, setRoomNumber] = useState("");
  const [typeDescription, setTypeDescription] = useState("Standard");
  const [pricePerNight, setPricePerNight] = useState(0);
  const [statusDescription, setStatusDescription] = useState("Available");
  const [description, setDescription] = useState("");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setSuccess("");

    const accessToken = localStorage.getItem("accessToken");
    if (!accessToken) {
      setError("No access token found. Please log in.");
      return;
    }

    const roomData = {
      accessToken,
      roomNumber,
      typeDescription,
      pricePerNight,
      statusDescription,
      description,
    };

    try {
      const response = await fetch(`${API_URL}/api/room/create`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(roomData),
      });

      if (!response.ok) {
        throw new Error("Failed to create room");
      }

      setSuccess("Room created successfully!");
      setRoomNumber("");
      setTypeDescription("Standard");
      setPricePerNight(0);
      setStatusDescription("Available");
      setDescription("");
    } catch (error) {
      setError(error.message);
    }
  };

  return (
    <div className={styles.container}>
      <h2 className={styles.heading}>Create a New Room</h2>
      {error && <p className={styles.error}>{error}</p>}
      {success && <p className={styles.success}>{success}</p>}
      <form onSubmit={handleSubmit} className={styles.form}>
        <input
          type="text"
          placeholder="Room Number"
          value={roomNumber}
          onChange={(e) => setRoomNumber(e.target.value)}
          required
          className={styles.input}
        />
        <select
          value={typeDescription}
          onChange={(e) => setTypeDescription(e.target.value)}
          required
          className={styles.select}
        >
          <option value="Standard">Standard</option>
          <option value="Deluxe">Deluxe</option>
          <option value="Suite">Suite</option>
        </select>
        <input
          type="number"
          placeholder="Price Per Night"
          value={pricePerNight}
          onChange={(e) => setPricePerNight(Number(e.target.value))}
          required
          className={styles.input}
        />
        <select
          value={statusDescription}
          onChange={(e) => setStatusDescription(e.target.value)}
          required
          className={styles.select}
        >
          <option value="Available">Available</option>
          <option value="Maintenance">Maintenance</option>
          <option value="Occupied">Occupied</option>
        </select>
        <textarea
          placeholder="Description"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          required
          className={styles.textarea}
        />
        <button type="submit" className={styles.button}>Create Room</button>
      </form>
    </div>
  );
};

export default CreateRoom;
