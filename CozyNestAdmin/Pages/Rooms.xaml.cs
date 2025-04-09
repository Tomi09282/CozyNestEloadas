using System;
using static CozyNestAdmin.GlobalMethods;
using static CozyNestAdmin.GlobalEnums;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CozyNestAdmin
{
    public partial class Rooms : Page
    {
        private const string BaseUrl = "http://localhost:5232/api/room";
        private List<Room> roomList = new();
        private Room selectedRoom = null;

        public Rooms()
        {
            InitializeComponent();
            LoadRooms();
        }

        private async void LoadRooms()
        {

            try
            {
                using HttpClient client = CreateHTTPClient(TokenDeclaration.AccessToken);
                var response = await client.GetAsync(GetEndpoint(RoomEndpoints.List));

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();

                    var roomData = JsonSerializer.Deserialize<RoomResponse>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (roomData == null || roomData.Rooms == null)
                    {
                        MessageBox.Show("No rooms found in response.");
                        return;
                    }

                    roomList = roomData.Rooms;
                    Application.Current.Dispatcher.Invoke(DisplayRooms);
                }
                else
                {
                    MessageBox.Show($"Error loading rooms: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: {ex.Message}");
            }
        }

        private void DisplayRooms()
        {
            RoomsWrapPanel.Children.Clear();
            foreach (var room in roomList)
            {
                var card = CreateRoomCard(room);
                RoomsWrapPanel.Children.Add(card);
            }
        }

        private UIElement CreateRoomCard(Room room)
        {
            var card = new StackPanel { Margin = new Thickness(10), Width = 200, Height = 150, Background = System.Windows.Media.Brushes.LightGray };
            card.Children.Add(new TextBlock { Text = $"Room {room.RoomNumber}", FontWeight = FontWeights.Bold });
            card.Children.Add(new TextBlock { Text = $"Type: {room.Type}" });
            card.Children.Add(new TextBlock { Text = $"Price: {room.PricePerNight}$" });

            var editButton = new Button { Content = "Edit", Margin = new Thickness(5) };
            var deleteButton = new Button { Content = "Delete", Margin = new Thickness(5) };

            editButton.Click += (s, e) => EditRoom(room);
            deleteButton.Click += (s, e) => DeleteRoom(room.Id);

            card.Children.Add(editButton);
            card.Children.Add(deleteButton);

            return card;
        }

        private async void EditRoom(Room room)
        {
            selectedRoom = room;
            RoomNameTextBox.Text = room.RoomNumber;
            RoomTypeComboBox.Text = room.Type;
            PriceTextBox.Text = room.PricePerNight.ToString();
            CapacityTextBox.Text = room.Description;

            AddButton.IsEnabled = false;
            ConfirmButton.IsEnabled = true;
            CancelButton.IsEnabled = true;
        }

        private async void ConfirmEdit_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRoom == null) return;

            selectedRoom.RoomNumber = RoomNameTextBox.Text;
            selectedRoom.Type = RoomTypeComboBox.Text;
            selectedRoom.PricePerNight = double.Parse(PriceTextBox.Text);
            selectedRoom.Description = CapacityTextBox.Text;

            try
            {
                using HttpClient client = CreateHTTPClient(TokenDeclaration.AccessToken);
                var response = await client.PutAsync(GetEndpoint(RoomEndpoints.Modify),
                    new StringContent(JsonSerializer.Serialize(new
                    {
                        roomId = selectedRoom.Id,
                        roomNumber = selectedRoom.RoomNumber,
                        typeDescription = selectedRoom.Type,
                        pricePerNight = selectedRoom.PricePerNight,
                        description = selectedRoom.Description,
                        statusDescription = selectedRoom.Status
                    }), Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    LoadRooms();
                    RoomNameTextBox.Text = "";
                    RoomTypeComboBox.Text = "";
                    PriceTextBox.Text = RoomTypeComboBox.Text = "";
                    CapacityTextBox.Text = RoomTypeComboBox.Text = "";
                    AddButton.IsEnabled = true;
                    ConfirmButton.IsEnabled = false;
                    CancelButton.IsEnabled = false;
                }
                else
                {
                    MessageBox.Show($"Error updating room: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private async void CreateRoom()
        {
            try
            {
                // Ellenőrizzük, hogy minden szükséges mező ki van-e töltve
                if (string.IsNullOrWhiteSpace(RoomNameTextBox.Text) ||
                    string.IsNullOrWhiteSpace(PriceTextBox.Text) ||
                    string.IsNullOrWhiteSpace(CapacityTextBox.Text) ||
                    string.IsNullOrWhiteSpace(DescriptionTextBox.Text) ||
                    RoomTypeComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Minden mező kitöltése kötelező!");
                    return;
                }

                // Biztosítjuk, hogy a konverziók helyesek
                if (!double.TryParse(PriceTextBox.Text, out double pricePerNight))
                {
                    MessageBox.Show("Érvénytelen ár! Kérlek, számot adj meg.");
                    return;
                }

                if (!int.TryParse(CapacityTextBox.Text, out int capacity))
                {
                    MessageBox.Show("Érvénytelen kapacitás! Kérlek, egész számot adj meg.");
                    return;
                }

                var newRoom = new
                {
                    RoomNumber = RoomNameTextBox.Text.Trim(),
                    TypeDescription = (RoomTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    PricePerNight = pricePerNight,
                    StatusDescription = "Available",
                    Capacity = capacity,
                    Description = DescriptionTextBox.Text.Trim()
                };

                // JSON konvertálás és megjelenítés debug céljából
                string jsonContent = JsonSerializer.Serialize(newRoom, new JsonSerializerOptions { WriteIndented = true });
                MessageBox.Show($"Küldött JSON adatok:\n{jsonContent}", "Debug", MessageBoxButton.OK, MessageBoxImage.Information);

                using HttpClient client = CreateHTTPClient(TokenDeclaration.AccessToken);

                var response = await client.PostAsync(GetEndpoint(RoomEndpoints.Create), new StringContent(jsonContent, Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Szoba sikeresen létrehozva!");
                    LoadRooms();
                }
                else
                {
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Hiba a szoba létrehozásakor: {response.StatusCode}\n{errorMessage}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba: {ex.Message}");
            }
        }


        private async void DeleteRoom(int roomId)
        {
            try
            {
                using HttpClient client = CreateHTTPClient(TokenDeclaration.AccessToken);
                
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/delete")
                {
                    Content = new StringContent(JsonSerializer.Serialize(new { roomId }), Encoding.UTF8, "application/json")
                };

                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    LoadRooms();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting room: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            RoomNameTextBox.Text = "";
            RoomTypeComboBox.Text = "";
            PriceTextBox.Text = RoomTypeComboBox.Text = "";
            CapacityTextBox.Text = RoomTypeComboBox.Text = "";
            AddButton.IsEnabled = true;
            ConfirmButton.IsEnabled = false;
            CancelButton.IsEnabled = false;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            CreateRoom();
        }
    }
}

public class RoomResponse
    {
        public string Message { get; set; }
        public List<Room> Rooms { get; set; } = new();
    }

    public class Room
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; }
        public string Type { get; set; }
        public double PricePerNight { get; set; }
        public string Description { get; set; }
        public bool Deleted { get; set; }
        public string Status { get; set; }
    }

