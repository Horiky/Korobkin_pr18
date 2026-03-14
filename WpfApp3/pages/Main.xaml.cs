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

namespace Airlines.Pages
{
    /// <summary>
    /// Логика взаимодействия для Main.xaml
    /// </summary>
    public partial class Main : Page
    {
        public Main()
        {
            InitializeComponent();
            // Устанавливаем начальные даты
            departureDate.SelectedDate = DateTime.Now.AddDays(1);
            returnDate.SelectedDate = DateTime.Now.AddDays(8);
        }

        private void Search(object sender, RoutedEventArgs e)
        {
            string fromText = from.Text.Trim();
            string toText = to.Text.Trim();

            // Проверяем, что выбраны обе даты
            if (departureDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату вылета туда!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (returnDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату вылета обратно!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime departure = departureDate.SelectedDate.Value;
            DateTime returnDateValue = returnDate.SelectedDate.Value;

            // Проверяем, что дата возвращения позже даты вылета
            if (returnDateValue <= departure)
            {
                MessageBox.Show("Дата обратного вылета должна быть позже даты вылета!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Переходим на страницу с билетами
            MainWindow.init.OpenPage(new Pages.Ticket(fromText, toText, departure, returnDateValue));
        }

        private void DepartureDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // При изменении даты вылета устанавливаем минимальную дату для возвращения
            if (departureDate.SelectedDate.HasValue)
            {
                returnDate.DisplayDateStart = departureDate.SelectedDate.Value.AddDays(1);

                // Если текущая дата возвращения раньше новой даты вылета, обновляем ее
                if (returnDate.SelectedDate.HasValue &&
                    returnDate.SelectedDate.Value <= departureDate.SelectedDate.Value)
                {
                    returnDate.SelectedDate = departureDate.SelectedDate.Value.AddDays(7);
                }
            }
        }
    }
}