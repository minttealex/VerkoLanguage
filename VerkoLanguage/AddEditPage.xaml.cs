using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using System.IO;

namespace VerkoLanguage
{
    /// <summary>
    /// Логика взаимодействия для AddEditPage.xaml
    /// </summary>
    public partial class AddEditPage : Page
    {
        private Client _currentClient = new Client();

        public AddEditPage(Client SelectedClient)
        {
            InitializeComponent();

            if (SelectedClient != null)
            {
                _currentClient = SelectedClient;
                IDTextBlock.Visibility = Visibility.Visible;
                IDTextBox.Visibility = Visibility.Visible;

            }
            else
            {
                _currentClient.Birthday = DateTime.Now;
                IDTextBlock.Visibility = Visibility.Hidden;
                IDTextBox.Visibility = Visibility.Hidden;
            }

            DataContext = _currentClient;

            SetGenderRadioButtons();
        }

        private void SetGenderRadioButtons()
        {
            if (_currentClient.Gender != null)
            {
                if (_currentClient.Gender.Name == "женский")
                {
                    GenderFemale.IsChecked = true;
                }
                else if (_currentClient.Gender.Name == "мужской")
                {
                    GenderMale.IsChecked = true;
                }
            }
            else
            {
                GenderFemale.IsChecked = false;
                GenderMale.IsChecked = false;
            }
        }

        private void GenderFemale_Checked(object sender, RoutedEventArgs e)
        {
            _currentClient.Gender = VerkoLanguageEntities.GetContext().Gender.FirstOrDefault(g => g.Name == "женский");
        }

        private void GenderMale_Checked(object sender, RoutedEventArgs e)
        {
            _currentClient.Gender = VerkoLanguageEntities.GetContext().Gender.FirstOrDefault(g => g.Name == "мужской");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            // Проверка фамилии
            if (string.IsNullOrWhiteSpace(_currentClient.LastName))
                errors.AppendLine("Укажите фамилию клиента");
            else if (_currentClient.LastName.Length > 50)
                errors.AppendLine("Фамилия не может быть длиннее 50 символов");
            else if (!Regex.IsMatch(_currentClient.LastName, @"^[а-яА-ЯёЁa-zA-Zs-]+$"))
                errors.AppendLine("Фамилия может содержать только буквы, пробелы и дефисы");

            // Проверка имени
            if (string.IsNullOrWhiteSpace(_currentClient.FirstName))
                errors.AppendLine("Укажите имя клиента");
            else if (_currentClient.FirstName.Length > 50)
                errors.AppendLine("Имя не может быть длиннее 50 символов");
            else if (!Regex.IsMatch(_currentClient.FirstName, @"^[а-яА-ЯёЁa-zA-Zs-]+$"))
                errors.AppendLine("Имя может содержать только буквы, пробелы и дефисы");

            // Проверка отчества
            if (!string.IsNullOrWhiteSpace(_currentClient.Patronymic) && _currentClient.Patronymic.Length > 50)
                errors.AppendLine("Отчество не может быть длиннее 50 символов");
            else if (!string.IsNullOrWhiteSpace(_currentClient.Patronymic) && !Regex.IsMatch(_currentClient.Patronymic, @"^[а-яА-ЯёЁa-zA-Zs-]+$"))
                errors.AppendLine("Отчество может содержать только буквы, пробелы и дефисы");

            // Проверка email
            if (string.IsNullOrWhiteSpace(_currentClient.Email))
            {
                errors.AppendLine("Укажите email клиента");
            }
            else if (_currentClient.Email.Split('@')[0].Length == 0)
            {
                errors.AppendLine("Email должен содержать имя пользователя перед символом @");
            }
            else if (_currentClient.Email.Split('@').Length != 2)
            {
                errors.AppendLine("Email должен содержать один символ @");
            }
            else if (_currentClient.Email.Split('@')[1].Split('.').Length < 2)
            {
                errors.AppendLine("Email должен содержать домен и домен верхнего уровня (например, example.com)");
            }
            else if (_currentClient.Email.Split('@')[1].Split('.')[1].Length < 2)
            {
                errors.AppendLine("Домен верхнего уровня должен содержать не менее двух символов");
            }
            else if (!Regex.IsMatch(_currentClient.Email, @"^[a-zA-Z0-9]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                errors.AppendLine("Email может содержать только латинские буквы, цифры, точку, дефис и @");
            }


            // Проверка телефона
            if (string.IsNullOrWhiteSpace(_currentClient.Phone))
            {
                errors.AppendLine("Укажите номер телефона клиента");
            }
            else
            {
                // Проверка на допустимые символы
                if (!Regex.IsMatch(_currentClient.Phone, @"^[0-9+-s()]+$"))
                {
                    errors.AppendLine("Номер телефона может содержать только цифры, плюс, минус, пробелы и скобки");
                }
                else
                {
                    // Удаляем все символы, кроме цифр для дальнейшей проверки
                    string ph = _currentClient.Phone.Replace("(", "").Replace(")", "").Replace("-", "").Replace("+", "").Replace(" ", "").Replace(".", "");

                    // Проверка длины номера телефона
                    if (ph.Length >= 10 && ph.Length <= 15)
                    {
                        // Номер телефона корректен
                    }
                    else
                    {
                        errors.AppendLine("Укажите корректный телефон клиента");
                    }
                }
            }


            // Проверка даты рождения
            if (_currentClient.Birthday == null)
                errors.AppendLine("Укажите дату рождения клиента");

            // Проверка пола
            if (!GenderFemale.IsChecked.Value && !GenderMale.IsChecked.Value)
                errors.AppendLine("Укажите пол клиента");

            // Если есть ошибки, выводим их
            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            // Сохранение данных
            if (_currentClient.ID == 0)
            {
                _currentClient.RegistrationDate = DateTime.Now;
                VerkoLanguageEntities.GetContext().Client.Add(_currentClient);
            }

            try
            {
                VerkoLanguageEntities.GetContext().SaveChanges();
                MessageBox.Show("Информация сохранена");
                Manager.MainFrame.Navigate(new ClientPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void ChangePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog myOpenFileDialog = new Microsoft.Win32.OpenFileDialog();

            // Рабочий каталог уже установлен на нужный путь,
            // диалоговое окно должно открываться в папке "Клиенты"
            myOpenFileDialog.InitialDirectory = Directory.GetCurrentDirectory();

            // Открываем диалоговое окно для выбора файла
            if (myOpenFileDialog.ShowDialog() == true)
            {
                _currentClient.PhotoPath = myOpenFileDialog.FileName; // Сохраняем путь к выбранному файлу
                PhotoPathImage.Source = new BitmapImage(new Uri(myOpenFileDialog.FileName)); // Загружаем изображение
            }
        }


    }

}
