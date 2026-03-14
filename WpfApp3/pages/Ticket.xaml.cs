using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Airlines_Миронченков.Classes;

namespace Airlines_Миронченков.Pages
{
    /// <summary>
    /// Логика взаимодействия для Ticket.xaml
    /// </summary>
    public partial class Ticket : Page
    {
        public List<TicketContext> AllTickets;
        private DateTime departureDate;
        private DateTime returnDate;
        private string fromCity;
        private string toCity;

        // Существующий конструктор (для обратной совместимости)
        public Ticket(string from, string to)
        {
            InitializeComponent();
            AllTickets = TicketContext.AllTickets().FindAll(x =>
                (x.from == from && to == "") || (from == "" && x.to == to) || (x.from == from && x.to == to));
            CreateUI(false);
        }

        // Новый конструктор для туда-обратно
        public Ticket(string from, string to, DateTime departureDate, DateTime returnDate)
        {
            InitializeComponent();

            this.fromCity = from;
            this.toCity = to;
            this.departureDate = departureDate;
            this.returnDate = returnDate;

            // Получаем все билеты
            AllTickets = TicketContext.AllTickets();

            // Ищем пары билетов (туда и обратно)
            FindRoundTripTickets();

            CreateUI(true);
        }

        private void FindRoundTripTickets()
        {
            // Ищем все возможные пары билетов
            var ticketPairs = new List<(TicketContext forward, TicketContext backward)>();
            var allTicketsList = AllTickets.ToList();

            // Берем билеты на дату туда
            var forwardTickets = allTicketsList
                .Where(t => t.time_start.Date == departureDate.Date &&
                           (string.IsNullOrEmpty(fromCity) || t.from.ToLower().Contains(fromCity.ToLower())) &&
                           (string.IsNullOrEmpty(toCity) || t.to.ToLower().Contains(toCity.ToLower())))
                .ToList();

            // Для каждого билета туда ищем обратный билет
            foreach (var forwardTicket in forwardTickets)
            {
                // Ищем обратный билет (из города назначения в город отправления на дату возвращения)
                var backwardTicket = allTicketsList
                    .FirstOrDefault(t => t.time_start.Date == returnDate.Date &&
                                        t.from.ToLower() == forwardTicket.to.ToLower() &&
                                        t.to.ToLower() == forwardTicket.from.ToLower());

                if (backwardTicket != null)
                {
                    // Добавляем пару
                    ticketPairs.Add((forwardTicket, backwardTicket));
                }
            }

            // Формируем список для отображения (пары билетов)
            AllTickets = new List<TicketContext>();
            foreach (var pair in ticketPairs)
            {
                // Добавляем оба билета из пары
                AllTickets.Add(pair.forward);
                AllTickets.Add(pair.backward);
            }
        }

        public void CreateUI(bool isRoundTrip)
        {
            Label filterInfoLabel = new Label();

            if (isRoundTrip)
            {
                filterInfoLabel.Content = $"Туда: {departureDate.ToString("dd.MM.yyyy")} | Обратно: {returnDate.ToString("dd.MM.yyyy")}";
                filterInfoLabel.Foreground = Brushes.OrangeRed;
            }
            else
            {
                filterInfoLabel.Content = "Все доступные билеты";
                filterInfoLabel.Foreground = Brushes.DarkBlue;
            }

            filterInfoLabel.FontSize = 14;
            filterInfoLabel.FontWeight = FontWeights.Bold;
            filterInfoLabel.Margin = new Thickness(0, 0, 0, 10);
            filterInfoLabel.HorizontalAlignment = HorizontalAlignment.Center;
            filterInfoLabel.VerticalAlignment = VerticalAlignment.Top;
            parent.Children.Add(filterInfoLabel);

            if (AllTickets.Count == 0)
            {
                Label noTicketsLabel = new Label();
                if (isRoundTrip)
                {
                    noTicketsLabel.Content = "Билеты туда-обратно не найдены";
                }
                else
                {
                    noTicketsLabel.Content = "Билеты по вашему запросу не найдены";
                }
                noTicketsLabel.HorizontalAlignment = HorizontalAlignment.Center;
                noTicketsLabel.VerticalAlignment = VerticalAlignment.Center;
                noTicketsLabel.FontSize = 16;
                noTicketsLabel.Foreground = Brushes.Gray;
                parent.Children.Add(noTicketsLabel);
            }
            else
            {
                if (isRoundTrip)
                {
                    // Отображаем пары билетов для туда-обратно
                    DisplayRoundTripTickets();
                }
                else
                {
                    // Отображаем обычные билеты
                    foreach (TicketContext ticket in AllTickets)
                    {
                        parent.Children.Add(new Element.Item(ticket, false));
                    }
                }
            }
        }

        private void DisplayRoundTripTickets()
        {
            // Группируем билеты по парам
            var ticketPairs = new Dictionary<TicketContext, TicketContext>();
            var processedTickets = new HashSet<TicketContext>();

            foreach (var ticket in AllTickets)
            {
                if (processedTickets.Contains(ticket)) continue;

                // Ищем пару для этого билета
                var pair = AllTickets.FirstOrDefault(t =>
                    t != ticket &&
                    t.from.ToLower() == ticket.to.ToLower() &&
                    t.to.ToLower() == ticket.from.ToLower() &&
                    ((t.time_start.Date == departureDate.Date && ticket.time_start.Date == returnDate.Date) ||
                     (t.time_start.Date == returnDate.Date && ticket.time_start.Date == departureDate.Date)));

                if (pair != null)
                {
                    ticketPairs[ticket] = pair;
                    processedTickets.Add(ticket);
                    processedTickets.Add(pair);
                }
            }

            // Отображаем пары
            foreach (var pair in ticketPairs)
            {
                var forwardTicket = pair.Key.time_start.Date == departureDate.Date ? pair.Key : pair.Value;
                var backwardTicket = pair.Key.time_start.Date == returnDate.Date ? pair.Key : pair.Value;

                // Создаем контейнер для пары билетов
                var pairContainer = CreatePairContainer(forwardTicket, backwardTicket);
                parent.Children.Add(pairContainer);
            }
        }

        private UIElement CreatePairContainer(TicketContext forward, TicketContext backward)
        {
            // Создаем Grid для пары билетов
            Grid pairGrid = new Grid();
            pairGrid.Margin = new Thickness(0, 0, 0, 15);
            pairGrid.Background = new SolidColorBrush(Color.FromArgb(30, 255, 64, 0)); // Полупрозрачный оранжевый

            // Определяем столбцы
            ColumnDefinition col1 = new ColumnDefinition();
            col1.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition col2 = new ColumnDefinition();
            col2.Width = new GridLength(1, GridUnitType.Star);

            pairGrid.ColumnDefinitions.Add(col1);
            pairGrid.ColumnDefinitions.Add(col2);

            // Добавляем заголовок "ТУДА-ОБРАТНО"
            Label headerLabel = new Label();
            headerLabel.Content = "ТУДА-ОБРАТНО";
            headerLabel.FontWeight = FontWeights.Bold;
            headerLabel.Foreground = Brushes.OrangeRed;
            headerLabel.HorizontalAlignment = HorizontalAlignment.Center;
            headerLabel.VerticalAlignment = VerticalAlignment.Top;
            headerLabel.FontSize = 14;
            headerLabel.Margin = new Thickness(0, 5, 0, 0);
            Grid.SetColumnSpan(headerLabel, 2);
            pairGrid.Children.Add(headerLabel);

            // Добавляем билет туда
            Border forwardBorder = new Border();
            forwardBorder.Background = Brushes.White;
            forwardBorder.Margin = new Thickness(5, 30, 2, 5);
            forwardBorder.CornerRadius = new CornerRadius(5);

            StackPanel forwardPanel = new StackPanel();
            forwardPanel.Margin = new Thickness(10);

            Label forwardLabel = new Label();
            forwardLabel.Content = "ТУДА";
            forwardLabel.FontWeight = FontWeights.Bold;
            forwardLabel.Foreground = Brushes.Green;
            forwardLabel.HorizontalAlignment = HorizontalAlignment.Center;

            Label forwardFrom = new Label();
            forwardFrom.Content = forward.from;
            forwardFrom.HorizontalAlignment = HorizontalAlignment.Center;

            Label forwardTo = new Label();
            forwardTo.Content = forward.to;
            forwardTo.HorizontalAlignment = HorizontalAlignment.Center;

            Label forwardDate = new Label();
            forwardDate.Content = forward.time_start.ToString("dd.MM.yyyy");
            forwardDate.HorizontalAlignment = HorizontalAlignment.Center;

            Label forwardTime = new Label();
            forwardTime.Content = $"{forward.time_start:HH:mm} - {forward.time_end:HH:mm}";
            forwardTime.HorizontalAlignment = HorizontalAlignment.Center;

            Label forwardPrice = new Label();
            forwardPrice.Content = $"{forward.price} Р.";
            forwardPrice.FontWeight = FontWeights.Bold;
            forwardPrice.HorizontalAlignment = HorizontalAlignment.Center;
            forwardPrice.Foreground = Brushes.DarkBlue;

            forwardPanel.Children.Add(forwardLabel);
            forwardPanel.Children.Add(forwardFrom);
            forwardPanel.Children.Add(forwardTo);
            forwardPanel.Children.Add(forwardDate);
            forwardPanel.Children.Add(forwardTime);
            forwardPanel.Children.Add(forwardPrice);

            forwardBorder.Child = forwardPanel;
            Grid.SetColumn(forwardBorder, 0);
            pairGrid.Children.Add(forwardBorder);

            // Добавляем билет обратно
            Border backwardBorder = new Border();
            backwardBorder.Background = Brushes.White;
            backwardBorder.Margin = new Thickness(2, 30, 5, 5);
            backwardBorder.CornerRadius = new CornerRadius(5);

            StackPanel backwardPanel = new StackPanel();
            backwardPanel.Margin = new Thickness(10);

            Label backwardLabel = new Label();
            backwardLabel.Content = "ОБРАТНО";
            backwardLabel.FontWeight = FontWeights.Bold;
            backwardLabel.Foreground = Brushes.Blue;
            backwardLabel.HorizontalAlignment = HorizontalAlignment.Center;

            Label backwardFrom = new Label();
            backwardFrom.Content = backward.from;
            backwardFrom.HorizontalAlignment = HorizontalAlignment.Center;

            Label backwardTo = new Label();
            backwardTo.Content = backward.to;
            backwardTo.HorizontalAlignment = HorizontalAlignment.Center;

            Label backwardDate = new Label();
            backwardDate.Content = backward.time_start.ToString("dd.MM.yyyy");
            backwardDate.HorizontalAlignment = HorizontalAlignment.Center;

            Label backwardTime = new Label();
            backwardTime.Content = $"{backward.time_start:HH:mm} - {backward.time_end:HH:mm}";
            backwardTime.HorizontalAlignment = HorizontalAlignment.Center;

            Label backwardPrice = new Label();
            backwardPrice.Content = $"{backward.price} Р.";
            backwardPrice.FontWeight = FontWeights.Bold;
            backwardPrice.HorizontalAlignment = HorizontalAlignment.Center;
            backwardPrice.Foreground = Brushes.DarkBlue;

            backwardPanel.Children.Add(backwardLabel);
            backwardPanel.Children.Add(backwardFrom);
            backwardPanel.Children.Add(backwardTo);
            backwardPanel.Children.Add(backwardDate);
            backwardPanel.Children.Add(backwardTime);
            backwardPanel.Children.Add(backwardPrice);

            backwardBorder.Child = backwardPanel;
            Grid.SetColumn(backwardBorder, 1);
            pairGrid.Children.Add(backwardBorder);

            // Добавляем общую цену
            Label totalPriceLabel = new Label();
            int totalPrice = forward.price + backward.price;
            totalPriceLabel.Content = $"Общая стоимость: {totalPrice} Р.";
            totalPriceLabel.FontWeight = FontWeights.Bold;
            totalPriceLabel.Foreground = Brushes.OrangeRed;
            totalPriceLabel.HorizontalAlignment = HorizontalAlignment.Center;
            totalPriceLabel.VerticalAlignment = VerticalAlignment.Bottom;
            totalPriceLabel.Margin = new Thickness(0, 0, 0, 5);
            Grid.SetColumnSpan(totalPriceLabel, 2);
            pairGrid.Children.Add(totalPriceLabel);

            // Добавляем кнопку
            Button buyButton = new Button();
            buyButton.Content = "Купить оба билета";
            buyButton.Width = 150;
            buyButton.Height = 25;
            buyButton.Background = Brushes.OrangeRed;
            buyButton.Foreground = Brushes.White;
            buyButton.BorderBrush = Brushes.OrangeRed;
            buyButton.HorizontalAlignment = HorizontalAlignment.Center;
            buyButton.VerticalAlignment = VerticalAlignment.Bottom;
            buyButton.Margin = new Thickness(0, 0, 0, 5);
            buyButton.Click += (s, e) =>
            {
                MessageBox.Show($"Покупка билетов:\nТуда: {forward.from} → {forward.to} за {forward.price} Р.\nОбратно: {backward.from} → {backward.to} за {backward.price} Р.\nИтого: {totalPrice} Р.",
                    "Покупка", MessageBoxButton.OK, MessageBoxImage.Information);
            };
            Grid.SetColumnSpan(buyButton, 2);
            pairGrid.Children.Add(buyButton);

            return pairGrid;
        }
    }
}