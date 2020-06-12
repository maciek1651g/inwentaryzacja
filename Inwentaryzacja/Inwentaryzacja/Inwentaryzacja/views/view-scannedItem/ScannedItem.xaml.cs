﻿using Inwentaryzacja.Controllers.Api;
using Inwentaryzacja.Models;
using System;
using System.Linq;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Inwentaryzacja.views.view_scannedItem
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    /// <summary>
    /// Klasa odpowiadajaca za widok okna zeskanowych srodkow trwalych i rzeczy z tym zwiazane
    /// </summary>
    public partial class ScannedItem : ContentPage
    {
        APIController api = new APIController();
        RoomEntity ScanningRoom;
        List<AllScaning> allScaning;
        ScanningUpdate scanningUpdate;

        /// <summary>
        /// Konstruktor klasy
        /// </summary>
        /// <param name="scannedItems">zeskanowane srodki trwale</param>
        /// <param name="scanningRoom">pokoj w ktorym odbylo sie skanowanie</param>
        public ScannedItem(List<AllScaning> scannedItems, RoomEntity scanningRoom, ScanningUpdate scanningUpdate)
        {
            InitializeComponent();
            this.scanningUpdate = scanningUpdate;
            allScaning = scannedItems;
            api.ErrorEventHandler += onApiError;
            ScanningRoom = scanningRoom;
            BindingContext = this;
            ScannedInRoomTopic.Text = "Zeskanowane z sali " + ScanningRoom.name;
            UnscannedInRoomTopic.Text = "Niezeskanowane z sali " + ScanningRoom.name;
            ShowInfo();
        }

        /// <summary>
        /// Klasa odpowiadajaca za skanowanie srodkow trwalych i rzeczy z tym zwiazane
        /// </summary>
        public class AllScaning
        {
            public ReportPositionPrototype reportPositionPrototype;

            public AssetEntity AssetEntity;
            private RoomEntity ScanningRoom;
            public bool Approved = false;
            public int? AssetRoomId;

            public int state = 0;

            /// <summary>
            /// Konstruktor klasy
            /// </summary>
            /// <param name="scanningRoom">pokoj w ktorym odbywa sie skanowanie</param>
            /// <param name="assetEntity">zeskanowany srodek trwaly</param>
            /// <param name="assetRoom">pokoj z ktorego pochodzi srodek trwaly</param>
            public AllScaning(AssetEntity assetEntity, RoomEntity assetRoom, RoomEntity scanningRoom)
            {
                if (assetRoom != null)
                {
                    AssetRoomId = assetRoom.id;
                    AssetRoomName = assetRoom.name;
                }
                else
                {
                    AssetRoomId = null;
                    AssetRoomName = "brak";
                }

                reportPositionPrototype = new ReportPositionPrototype(assetEntity, assetRoom, false);
                AssetEntity = assetEntity;
                ScannedId = assetEntity.id;
                ScanningRoom = scanningRoom;
            }

            /// <summary>
            /// Funkcja odpowiadajaca za przeniesienie srodka trwalego do aktualnie skanowanego pokoju
            /// </summary>
            public void ItemMoved()
            {
                state = 1;
                Approved = true;
                reportPositionPrototype.present = true;
                AssetRoomId = ScanningRoom.id;
            }

            /// <summary>
            /// Opcja nie rób nic
            /// </summary>
            public void ItemDontMove()
            {
                state = 2;
                Approved = true;
            }

            /// <summary>
            /// Funkcja odpowiadajaca za tekst wyswietlany po zeskanowaniu danego srodka trwalego
            /// </summary>
            public string ScaningText {
                get { return $"{AssetEntity.type.name} {AssetEntity.id}"; }
            }

            /// <summary>
            /// Funkcja odpowiadajaca za ustawienie/zwrocenie ID zeskanowanego srodka trwalego
            /// </summary>
            public int ScannedId { get; set; }

            /// <summary>
            /// Funkcja odpowiadajaca za ustawienie/zwrocenie nazwy pokoju pochodzenia zeskanowanego srodka trwalego
            /// </summary>
            public string AssetRoomName { get; set; }
        }

        public class ScanningUpdate {
            private APIController Api;
            private RoomEntity ThisRoom;
            private int Scanid;

            public ScanningUpdate(APIController api, RoomEntity room, int scanid)
            {
                ThisRoom = room;
                Scanid = scanid;
                Api = api;
            }

            public async void Update(List<AllScaning> allPositions)
            {
                var scannedItems = allPositions.FindAll((el) => el.Approved || el.AssetRoomId != ThisRoom.id);

                var positions = new List<ScanPositionPropotype>();

                foreach (var position in scannedItems)
                {
                    positions.Add(new ScanPositionPropotype(position.AssetEntity.id, position.state));
                }

                await Api.updateScan(new ScanUpdatePropotype(Scanid, positions.ToArray()));
            }

            public async void Delete()
            {
                await Api.deleteScan(Scanid);
            }
        }
        /// <summary>
        /// Funkcja odpowiadajaca za wyswietlenie informacji dotyczacych srodkow trwalych po zeskanowaniu
        /// </summary>
        private void ShowInfo()
        {
            int[] items = { 0, 0, 0, 0, 0, 0 };//c k m p s t
            string[] types = { "Komputer:", "Krzesło:", "Monitor:", "Projektor:", "Stół:", "Tablica:" };

            foreach (AllScaning item in allScaning)
            {
                if (item.reportPositionPrototype.present)
                {
                    UpdateAmount(items, item.AssetEntity.type.id);
                }
            }

            ScannedInRoomLabel.Text = "";
            ScannedInRoomAmount.Text = "";

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] > 0)
                {
                    ScannedInRoomLabel.Text += types[i] + "\n";
                    ScannedInRoomAmount.Text += items[i];

                    if (items[i] == 1)
                        ScannedInRoomAmount.Text += " sztuka\n";
                    else if (items[i] <= 4)
                        ScannedInRoomAmount.Text += " sztuki\n";
                    else
                        ScannedInRoomAmount.Text += " sztuk\n";
                }
            }

            if (ScannedInRoomLabel.Text == "")
            {
                ScannedInRoomLabel.Text = "Brak";
            }
            else
            {
                ScannedInRoomLabel.Text = ScannedInRoomLabel.Text.Substring(0, ScannedInRoomLabel.Text.Length - 1);
                ScannedInRoomAmount.Text = ScannedInRoomAmount.Text.Substring(0, ScannedInRoomAmount.Text.Length - 1);
            }

            items = new int[] { 0, 0, 0, 0, 0, 0 };//c k m p s t

            foreach (AllScaning item in allScaning)
            {
                if (!item.reportPositionPrototype.present && item.reportPositionPrototype.previous == ScanningRoom.id)
                {
                    UpdateAmount(items, item.AssetEntity.type.id);
                }
            }

            UnscannedInRoomLabel.Text = "";
            UnscannedInRoomAmount.Text = "";

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] > 0)
                {
                    UnscannedInRoomLabel.Text += types[i] + "\n";
                    UnscannedInRoomAmount.Text += items[i];
                    if (items[i] == 1)
                        UnscannedInRoomAmount.Text += " sztuka\n";
                    else if (items[i] <= 4)
                        UnscannedInRoomAmount.Text += " sztuki\n";
                    else
                        UnscannedInRoomAmount.Text += " sztuk\n";

                }
            }

            if (UnscannedInRoomLabel.Text == "")
            {
                UnscannedInRoomLabel.Text = "Brak";
            }
            else
            {
                UnscannedInRoomLabel.Text = UnscannedInRoomLabel.Text.Substring(0, UnscannedInRoomLabel.Text.Length - 1);
                UnscannedInRoomAmount.Text = UnscannedInRoomAmount.Text.Substring(0, UnscannedInRoomAmount.Text.Length - 1);
            }

            items = new int[] { 0, 0, 0, 0, 0, 0 };//c k m p s t

            foreach (AllScaning item in allScaning)
            {
                if (!item.reportPositionPrototype.present && item.reportPositionPrototype.previous != ScanningRoom.id)
                {
                    UpdateAmount(items, item.AssetEntity.type.id);
                }
            }

            OtherLabel.Text = "";
            OtherAmount.Text = "";

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] > 0)
                {
                    OtherLabel.Text += types[i] + "\n";
                    OtherAmount.Text += items[i];
                    if (items[i] == 1)
                        OtherAmount.Text += " sztuka\n";
                    else if (items[i] <= 4)
                        OtherAmount.Text += " sztuki\n";
                    else
                        OtherAmount.Text += " sztuk\n";

                }
            }

            if (OtherLabel.Text == "")
            {
                OtherLabel.Text = "Brak";
            }
            else
            {
                OtherLabel.Text = OtherLabel.Text.Substring(0, OtherLabel.Text.Length - 1);
                OtherAmount.Text = OtherAmount.Text.Substring(0, OtherAmount.Text.Length - 1);
            }

            List<AllScaning> scannedItems = new List<AllScaning>();

            foreach (AllScaning item in allScaning)
            {
                if (!item.Approved && item.reportPositionPrototype.previous != ScanningRoom.id)
                {
                    scannedItems.Add(item);
                }
            }

            ReportList.ItemsSource = scannedItems;

            if (scannedItems.Count == 0)
                ButtonMoveAll.IsVisible = false;
            else
                ButtonMoveAll.IsVisible = true;
        }
        /// <summary>
        /// Funkcja odpowiadajaca za wyswietlenie szczegolow srodkow trwalych ktore zostaly zeskanowane w pokoju
        /// </summary>
        async private void ScannedInRoomDetails(object sender, EventArgs e)
        {
            string text = "";

            foreach (AllScaning item in allScaning)
            {
                if (item.reportPositionPrototype.present)
                {
                    text += item.AssetEntity.type.name + " numer: " + item.AssetEntity.id + "\n";
                }
            }

            if (text == "") text = "brak";

            await DisplayAlert("Zeskanowane z sali " + ScanningRoom.name, text, "Ok");
        }
        /// <summary>
        /// Funkcja odpowiadajaca za wyswietlenie szczegolow srodkow trwalych ktore nie zostaly zeskanowane w pokoju
        /// </summary>
        async private void UnscannedInRoomDetails(object sender, EventArgs e)
        {
            string text = "";

            foreach (AllScaning item in allScaning)
            {
                if (!item.reportPositionPrototype.present && item.reportPositionPrototype.previous == ScanningRoom.id)
                {
                    text += item.AssetEntity.type.name + " numer: " + item.AssetEntity.id + "\n";
                }
            }

            if (text == "") text = "brak";

            await DisplayAlert("Niezeskanowane z sali " + ScanningRoom.name, text, "Ok");
        }
        /// <summary>
        /// Funkcja odpowiadajaca za wyswietlenie innych szczegolow
        /// </summary>
        async private void OtherDetails(object sender, EventArgs e)
        {
            string text = "";

            foreach (AllScaning item in allScaning)
            {
                if (!item.reportPositionPrototype.present && item.reportPositionPrototype.previous != ScanningRoom.id)
                {
                    text += $"{item.AssetEntity.type.name} numer: {item.AssetEntity.id}\n";
                }
            }

            if (text == "") text = "brak";

            await DisplayAlert("Nieprzeniesione z innych sal", text, "Ok");
        }

        /// <summary>
        /// Funkcja odpowiadajaca za sprawdzenie ilosci srodkow trwalych po skanowaniu
        /// </summary>
        private void UpdateAmount(int[] items, int typeId)
        {
            switch (typeId)
            {
                case 1: items[0]++; break;
                case 2: items[1]++; break;
                case 3: items[2]++; break;
                case 4: items[3]++; break;
                case 5: items[4]++; break;
                case 6: items[5]++; break;
            }
        }

        /// <summary>
        /// Funkcja odpowiadajaca za zakonczenie skanowania
        /// </summary>
        async private void EndScanning(object sender, EventArgs e)
        {
            bool message1 = false;
            bool message2 = false;

            foreach (AllScaning item in allScaning)
            {
                if (!item.Approved)
                {
                    if(item.AssetRoomId == ScanningRoom.id)
                        message2 = true;
                    else
                        message1 = true;
                }
            }

            if (message1 == true)
            {
                await DisplayAlert("Uwaga", "Istnieją niezatwierdzone przedmioty", "Wróć");

                return;
            }
            else if (message2 == true)
            {
                bool response = await DisplayAlert("Uwaga", "Jeśli kontynuujesz, wszystkie niezeskanowane przedmioty z sali zostaną z niej usunięte!", "Kontunuuj", "Anuluj");
                if(response) GenerateRaport();

                else return;
            }

            GenerateRaport();
        }

        /// <summary>
        /// Funkcja odpowiadajaca za probe zmiany lokalizacji/pokoju danego srodka trwalego
        /// </summary>
        async private void ChangeRoom(object sender, EventArgs e)
        {
            bool response = await DisplayAlert("Uwaga", "Czy na pewno chcesz przenieść tutaj ten przedmiot?", "Tak", "Nie");

            if (response)
            {
                Button button = sender as Button;
                int id = Convert.ToInt32(button.CommandParameter);

                foreach (AllScaning item in allScaning)
                {
                    if (item.ScannedId == id)
                    {
                        item.ItemMoved();
                        ShowInfo();
                        break;
                    }
                }

                scanningUpdate.Update(allScaning);
            }
        }

        /// <summary>
        /// Funkcja odpowiadajaca za odrzucenie proby zmiany lokalizacji/pokoju danego srodka trwalego
        /// </summary>
        async private void DontDoAnything(object sender, EventArgs e)
        {
            bool response = await DisplayAlert("Uwaga", "Czy na pewno nie chcesz zmieniać lokalizacji tego przedmiotu?", "Tak", "Nie");

            if (response)
            {
                Button button = sender as Button;
                int id = Convert.ToInt32(button.CommandParameter);

                foreach (AllScaning item in allScaning)
                {
                    if (item.ScannedId == id)
                    {
                        item.ItemDontMove();
                        ShowInfo();
                        break;
                    }
                }

                scanningUpdate.Update(allScaning);
            }
        }

        /// <summary>
        /// Funkcja odpowiadajaca za przeniesienie wszystkich zeskanowanych przedmiotow, do pokoju w ktorym sie odbylo skanowanie, jezeli z niego nie pochodza
        /// </summary>s
        async private void moveAllForeignAssetsToThisRoom(object sender, EventArgs e)
        {
            bool response = await DisplayAlert("Uwaga", "Czy na pewno chcesz przenieść wszystkie przedmioty?", "Tak", "Nie");

            if (response)
            {
                foreach (AllScaning item in allScaning)
                {
                    if (!item.Approved && item.AssetRoomId != ScanningRoom.id)
                    {
                        item.ItemMoved();
                    }
                }

                scanningUpdate.Update(allScaning);

                ShowInfo();
                scrollView.IsEnabled = false;
                await scrollView.ScrollToAsync(0, 0, true);
            }
        }

        /// <summary>
        /// Funkcja odpowiadajaca za przeniesienie wszystkich zeskanowanych przedmiotow, do pokoju w ktorym sie odbylo skanowanie, jezeli z niego nie pochodza
        /// </summary>
        async private void considerEverythingInRoomAsScanned(object sender, EventArgs e)
        {
            bool response = await DisplayAlert("Uwaga", "Czy na pewno chcesz przenieść wszystkie przedmioty?", "Tak", "Nie");

            if (response)
            {
                foreach (AllScaning item in allScaning)
                {
                    if (!item.Approved && item.AssetRoomId == ScanningRoom.id)
                    {
                        item.ItemMoved();
                    }
                }

                scanningUpdate.Update(allScaning);

                ShowInfo();
            }
        }
        
        /// <summary>
        /// Funkcja odpowiadajaca za wygenerowanie raportu po skanowaniu
        /// </summary>
        private async void GenerateRaport()
        {
            EnableView(false);
            ReportPositionPrototype[] reportPositionPrototype = new ReportPositionPrototype[allScaning.Count];
            
            for (int i = 0; i < allScaning.Count; i++) 
            {
                reportPositionPrototype[i] = allScaning[i].reportPositionPrototype;
            }
            
            int reportId = await api.createReport(new ReportPrototype($"Raport {ScanningRoom.building.name}", ScanningRoom, reportPositionPrototype));

            EnableView(true);
            
            if (reportId != -1)
            {
                scanningUpdate.Delete();
                App.Current.MainPage = new NavigationPage(new WelcomeViewPage());
            }
        }
        
        /// <summary>
        /// Funkcja odpowiadajaca za umozliwienie wyswietlenia widoku okna
        /// </summary>
        private void EnableView(bool state)
        {
            LoadingScreen.IsVisible = !state;
            ButtonPrevPage.IsEnabled = state;
            ButtonConfirm.IsEnabled = state;
        }
        
        /// <summary>
        /// Funkcja odpowiadajaca za powrot do poprzedniego okna
        /// </summary>
        private async void RetPrevPage(object sender, EventArgs e)
        {
            EnableView(false);
            await Navigation.PopModalAsync();
            EnableView(true);
        }
        
        /// <summary>
        /// Funkcja odpowiadajaca za wyswietlenie bledu
        /// </summary>
        private async void onApiError(object o, ErrorEventArgs error)
        {
            await DisplayAlert("Błąd", error.Message, "OK");

            if (error.Auth == false)
            {
                await Navigation.PushAsync(new LoginPage());
            }
        }
    }
}
