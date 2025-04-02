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

namespace VerkoLanguage
{
    /// <summary>
    /// Логика взаимодействия для ClientPage.xaml
    /// </summary>
    public partial class ClientPage : Page
    {
        int TotalRecords;
        int CountRecords;
        int CountPage;
        int CurrentPage = 0;

        List<Client> CurrentPagelist = new List<Client>();
        List<Client> TableList;

        public ClientPage()
        {
            InitializeComponent();

            TotalRecords = VerkoLanguageEntities.GetContext().Client.Count();

            var currentClients = VerkoLanguageEntities.GetContext().Client.ToList();

            ClientListView.ItemsSource = currentClients;

            ComboEntries.SelectedIndex = 0;
            ComboGender.SelectedIndex = 0;
            ComboSort.SelectedIndex = 0;

            UpdateClients();
            UpdateCounter();
        }

        private string NormalizePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;

            // Удаляем все символы, кроме цифр
            return new string(phoneNumber.Where(char.IsDigit).ToArray());
        }

        private void UpdateClients()
        {
            var currentClients = VerkoLanguageEntities.GetContext().Client.ToList();

            /*string search = TBoxSearch.Text.ToLower();
            currentClients = currentClients.Where(p =>
                (p.LastName + " " + p.FirstName + " " + p.Patronymic).ToLower().Contains(search) ||
                p.Email.ToLower().Contains(search) ||  // Поиск по email
                p.Phone.ToLower().Contains(search)    // Поиск по телефону
            ).ToList();*/

            var searchTerms = TBoxSearch.Text.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Нормализуем введенные поисковые термины
            var normalizedSearchTerms = searchTerms.Select(term => NormalizePhoneNumber(term)).ToArray();

            currentClients = currentClients.Where(client =>
                searchTerms.All(term =>
                    (client.LastName + " " + client.FirstName + " " + client.Patronymic).ToLower().Contains(term) ||
                    (client.Email ?? "").ToLower().Contains(term) ||
                    (NormalizePhoneNumber(client.Phone) ?? "").Contains(term)
                )
            ).ToList(); // строка разбивается на отдельные слова, все слова должны присутсвовать хотя бы в одном из полей

            if (ComboGender.SelectedIndex == 1)
            {
                currentClients = currentClients.Where(p => p.GenderName == "женский").ToList();
            }
            else if (ComboGender.SelectedIndex == 2)
            {
                currentClients = currentClients.Where(p => p.GenderName == "мужской").ToList();
            }


            switch (ComboSort.SelectedIndex)
            {
                case 1: // По фамилии
                    currentClients = currentClients.OrderBy(p => p.LastName).ToList();
                    break;
                case 2: // По дате последнего посещения
                    currentClients = currentClients.OrderByDescending(p => p.LastVisitDate).ToList();
                    break;
                case 3: // По количеству посещений
                    currentClients = currentClients.OrderByDescending(p => p.VisitCount).ToList();
                    break;
            }

            TableList = currentClients;
            CurrentPage = 0;
            CurrentPagelist.Clear();

            int pageSize = GetPageSize();
            CountRecords = TableList.Count;
            CountPage = (int)Math.Ceiling((double)CountRecords / pageSize);

            if (CountRecords > 0)
            {
                LoadPage(0);
            }
            else
            {
                ClientListView.ItemsSource = null;
            }
        }


        private void LoadPage(int page)
        {
            CurrentPagelist.Clear();
            int start = page * GetPageSize();
            int end = Math.Min(start + GetPageSize(), TableList.Count);

            for (int i = start; i < end; i++)
            {
                CurrentPagelist.Add(TableList[i]);
            }

            PageListBox.Items.Clear();
            for (int i = 1; i <= CountPage; i++)
            {
                PageListBox.Items.Add(i);
            }
            PageListBox.SelectedIndex = page;

            ClientListView.ItemsSource = CurrentPagelist;
            ClientListView.Items.Refresh();
            UpdateCounter();
        }


        private int GetPageSize()
        {
            switch (ComboEntries.SelectedIndex)
            {
                case 0: return 10;
                case 1: return 50;
                case 2: return 200;
                default: return TableList.Count;
            }
        }


        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var currentClient = (sender as Button).DataContext as Client;

            var currentClientService = VerkoLanguageEntities.GetContext().ClientService.ToList();
            currentClientService = currentClientService.Where(p => p.ClientID == currentClient.ID).ToList();

            if (currentClientService.Count != 0)
                MessageBox.Show("Невозможно выполнить удаление, так как у клиента есть информация о посещениях");
            else
            {
                if (MessageBox.Show("Вы точно хотите выполнить удаление?", "Внимание!", MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        VerkoLanguageEntities.GetContext().Client.Remove(currentClient);
                        VerkoLanguageEntities.GetContext().SaveChanges();

                        TotalRecords = VerkoLanguageEntities.GetContext().Client.Count();
                        ClientListView.ItemsSource = VerkoLanguageEntities.GetContext().Client.ToList();
                        UpdateClients();
                        UpdateCounter();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message.ToString());
                    }
                }
            }
        }

        private void ChangePage(int direction, int? selectedPage)
        {
            int pageSize = GetPageSize();
            CountPage = (int)Math.Ceiling((double)TableList.Count / pageSize);

            if (selectedPage.HasValue)
            {
                if (selectedPage >= 0 && selectedPage < CountPage)
                {
                    CurrentPage = (int)selectedPage;
                    LoadPage(CurrentPage);
                }
            }
            else
            {
                switch (direction)
                {
                    case 1:
                        if (CurrentPage > 0)
                        {
                            CurrentPage--;
                            LoadPage(CurrentPage);
                        }
                        break;
                    case 2:
                        if (CurrentPage < CountPage - 1)
                        {
                            CurrentPage++;
                            LoadPage(CurrentPage);
                        }
                        break;
                }
            }
        }

        private void UpdateCounter()
        {
            int currentTotal = TableList.Count;
            int currentPage = CurrentPagelist.Count;
            CounterText.Text = $"Показано {currentTotal} из {TotalRecords}";
        }



        private void LeftDirButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(1, null);
        }

        private void RighDirButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(2, null);
        }

        private void PageListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ChangePage(0, Convert.ToInt32(PageListBox.SelectedItem.ToString()) - 1);
        }

        private void ComboEntries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateClients();  
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateClients();
        }

        private void ComboGender_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateClients();
        }

        private void ComboSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateClients();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage((sender as Button).DataContext as Client));
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage(null));
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                VerkoLanguageEntities.GetContext().ChangeTracker.Entries().ToList().ForEach(p => p.Reload());
                ClientListView.ItemsSource = VerkoLanguageEntities.GetContext().Client.ToList(); 
            }
        }
    }
}
