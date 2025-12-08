using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace material_design
{
    public partial class Window2 : Window
    {
        private cafe_barEntities1 _db = new cafe_barEntities1();
        private string _currentTable;
        private object _currentEntity;
        private Dictionary<string, Type> _tableTypes = new Dictionary<string, Type>();
        private Dictionary<string, StackPanel> _fieldPanels = new Dictionary<string, StackPanel>();

        public Window2()
        {
            InitializeComponent();
            InitializeTableTypes();
        }

        private void InitializeTableTypes()
        {
            // Исключаем таблицу авторизации
            _tableTypes.Add("Должности", typeof(Post));
            _tableTypes.Add("Сотрудники", typeof(Employees));
            _tableTypes.Add("Клиенты", typeof(Clients));
            _tableTypes.Add("Постоянные клиенты", typeof(Regular_Clients));
            _tableTypes.Add("Бронирования", typeof(Reservation));
            _tableTypes.Add("Категории меню", typeof(CategoriesMenu));
            _tableTypes.Add("Меню", typeof(Menu));
            _tableTypes.Add("Заказы", typeof(Orders));
            _tableTypes.Add("Детали заказов", typeof(Order_details));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTables();
        }

        private void LoadTables()
        {
            lvTables.Items.Clear();
            foreach (var table in _tableTypes.Keys)
            {
                lvTables.Items.Add(table);
            }
        }

        private void lvTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvTables.SelectedItem == null) return;

            _currentTable = lvTables.SelectedItem.ToString();
            tbTableTitle.Text = _currentTable;
            spEditPanel.Visibility = Visibility.Visible;
            tbEditTitle.Text = $"Редактирование: {_currentTable}";

            LoadTableData();
            CreateEditFields();
        }

        private void LoadTableData()
        {
            try
            {
                Type entityType = _tableTypes[_currentTable];
                var dbSet = _db.Set(entityType);
                var query = ((IQueryable)dbSet).IncludeAll();

                dataGrid.ItemsSource = query.ToList(entityType);
                tbStatus.Text = $"Загружено записей: {((System.Collections.IList)dataGrid.ItemsSource).Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void CreateEditFields()
        {
            spFields.Children.Clear();
            _fieldPanels.Clear();

            Type entityType = _tableTypes[_currentTable];
            var properties = entityType.GetProperties()
                .Where(p => p.Name != "id" &&
                           !p.Name.EndsWith("_fk") &&
                           !p.Name.Contains("photo_data") &&
                           p.CanWrite);

            foreach (var prop in properties)
            {
                var stackPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 10) };

                string displayName = GetDisplayName(prop);
                var textBlock = new TextBlock
                {
                    Text = displayName + ":",
                    Margin = new Thickness(0, 0, 0, 5),
                    FontWeight = FontWeights.Normal
                };

                Control inputControl;

                if (prop.PropertyType == typeof(string))
                {
                    inputControl = new TextBox();
                    ((TextBox)inputControl).TextChanged += (s, e) => UpdateCurrentEntity(prop, ((TextBox)s).Text);
                }
                else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(decimal))
                {
                    inputControl = new TextBox();
                    ((TextBox)inputControl).TextChanged += (s, e) => UpdateCurrentEntity(prop, ((TextBox)s).Text);
                }
                else if (prop.PropertyType == typeof(DateTime))
                {
                    inputControl = new DatePicker();
                    ((DatePicker)inputControl).SelectedDateChanged += (s, e) =>
                        UpdateCurrentEntity(prop, ((DatePicker)s).SelectedDate);
                }
                else if (prop.PropertyType == typeof(byte) || prop.PropertyType == typeof(short))
                {
                    inputControl = new TextBox();
                    ((TextBox)inputControl).TextChanged += (s, e) => UpdateCurrentEntity(prop, ((TextBox)s).Text);
                }
                else
                {
                    inputControl = new TextBox();
                    ((TextBox)inputControl).TextChanged += (s, e) => UpdateCurrentEntity(prop, ((TextBox)s).Text);
                }

                inputControl.Tag = prop.Name;
                inputControl.Margin = new Thickness(0, 0, 0, 5);

                stackPanel.Children.Add(textBlock);
                stackPanel.Children.Add(inputControl);

                spFields.Children.Add(stackPanel);
                _fieldPanels[prop.Name] = stackPanel;
            }

            // Добавляем поля для внешних ключей
            AddForeignKeyFields(entityType);
        }

        private string GetDisplayName(PropertyInfo prop)
        {
            var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
            return displayAttr?.Name ?? RussianTranslator.GetFieldName(prop.Name) ?? prop.Name;
        }

        private void AddForeignKeyFields(Type entityType)
        {
            // Для таблиц с внешними ключами добавляем ComboBox
            if (_currentTable == "Сотрудники")
            {
                AddComboBoxField("post_emp_fk", "Должность", _db.Post.ToList(), "title_post", "id_post");
            }
            else if (_currentTable == "Постоянные клиенты")
            {
                AddComboBoxField("id_reg_client_fk", "Клиент", _db.Clients.ToList(), "name_client", "id_client");
            }
            else if (_currentTable == "Бронирования")
            {
                AddComboBoxField("id_client_fk", "Клиент", _db.Clients.ToList(), "name_client", "id_client");
                AddComboBoxField("id_employee_fk", "Сотрудник", _db.Employees.ToList(), "name_employee", "id_employee");
            }
            else if (_currentTable == "Меню")
            {
                AddComboBoxField("id_category_fk", "Категория", _db.CategoriesMenu.ToList(), "title_category", "id_category");
            }
            else if (_currentTable == "Заказы")
            {
                AddComboBoxField("id_cli_fk", "Клиент", _db.Clients.ToList(), "name_client", "id_client");
                AddComboBoxField("id_emp_fk", "Сотрудник", _db.Employees.ToList(), "name_employee", "id_employee");
            }
            else if (_currentTable == "Детали заказов")
            {
                AddComboBoxField("id_order_fk", "Заказ", _db.Orders.ToList(), "id_order", "id_order");
                AddComboBoxField("id_menu_item_fk", "Позиция меню", _db.Menu.ToList(), "item_name", "id_menu_item");
            }
        }

        private void AddComboBoxField(string propertyName, string displayName,
            System.Collections.IEnumerable items, string displayMember, string valueMember)
        {
            var stackPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 10) };
            var textBlock = new TextBlock
            {
                Text = displayName + ":",
                Margin = new Thickness(0, 0, 0, 5),
                FontWeight = FontWeights.Normal
            };

            var comboBox = new ComboBox
            {
                DisplayMemberPath = displayMember,
                SelectedValuePath = valueMember,
                ItemsSource = items,
                Margin = new Thickness(0, 0, 0, 5)
            };

            comboBox.SelectionChanged += (s, e) =>
            {
                if (comboBox.SelectedValue != null && _currentEntity != null)
                {
                    var prop = _currentEntity.GetType().GetProperty(propertyName);
                    if (prop != null)
                    {
                        prop.SetValue(_currentEntity, comboBox.SelectedValue);
                    }
                }
            };

            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(comboBox);

            spFields.Children.Add(stackPanel);
            _fieldPanels[propertyName] = stackPanel;
        }

        private void UpdateCurrentEntity(PropertyInfo prop, object value)
        {
            if (_currentEntity != null)
            {
                try
                {
                    if (value == null)
                    {
                        prop.SetValue(_currentEntity, null);
                    }
                    else if (prop.PropertyType == typeof(string))
                    {
                        prop.SetValue(_currentEntity, value.ToString());
                    }
                    else if (prop.PropertyType == typeof(int))
                    {
                        if (int.TryParse(value.ToString(), out int intValue))
                            prop.SetValue(_currentEntity, intValue);
                    }
                    else if (prop.PropertyType == typeof(decimal))
                    {
                        if (decimal.TryParse(value.ToString(), out decimal decimalValue))
                            prop.SetValue(_currentEntity, decimalValue);
                    }
                    else if (prop.PropertyType == typeof(DateTime))
                    {
                        prop.SetValue(_currentEntity, value);
                    }
                    else if (prop.PropertyType == typeof(byte))
                    {
                        if (byte.TryParse(value.ToString(), out byte byteValue))
                            prop.SetValue(_currentEntity, byteValue);
                    }
                    else if (prop.PropertyType == typeof(short))
                    {
                        if (short.TryParse(value.ToString(), out short shortValue))
                            prop.SetValue(_currentEntity, shortValue);
                    }
                }
                catch { }
            }
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentEntity = dataGrid.SelectedItem;
            if (_currentEntity == null) return;

            // Заполняем поля значениями из выбранной сущности
            foreach (var kvp in _fieldPanels)
            {
                var propertyName = kvp.Key;
                var stackPanel = kvp.Value;

                var inputControl = stackPanel.Children[1] as Control;
                if (inputControl == null) continue;

                var prop = _currentEntity.GetType().GetProperty(propertyName);
                if (prop == null) continue;

                var value = prop.GetValue(_currentEntity);

                if (inputControl is TextBox textBox)
                {
                    textBox.Text = value?.ToString() ?? "";
                }
                else if (inputControl is DatePicker datePicker && value is DateTime dateTime)
                {
                    datePicker.SelectedDate = dateTime;
                }
                else if (inputControl is ComboBox comboBox)
                {
                    comboBox.SelectedValue = value;
                }
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Type entityType = _tableTypes[_currentTable];
                _currentEntity = Activator.CreateInstance(entityType);

                // Очищаем поля
                foreach (var kvp in _fieldPanels)
                {
                    var stackPanel = kvp.Value;
                    var inputControl = stackPanel.Children[1] as Control;

                    if (inputControl is TextBox textBox)
                    {
                        textBox.Text = "";
                    }
                    else if (inputControl is DatePicker datePicker)
                    {
                        datePicker.SelectedDate = null;
                    }
                    else if (inputControl is ComboBox comboBox)
                    {
                        comboBox.SelectedItem = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания новой записи: {ex.Message}");
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_currentEntity == null)
            {
                MessageBox.Show("Выберите запись для редактирования или создайте новую");
                return;
            }

            try
            {
                Type entityType = _tableTypes[_currentTable];
                var dbSet = _db.Set(entityType);

                // Проверяем, новая ли это запись
                var idProperty = entityType.GetProperty(entityType.Name.ToLower().Replace("s", "") + "_id") ??
                               entityType.GetProperty("id") ??
                               entityType.GetProperties().FirstOrDefault(p => p.Name.EndsWith("_id"));

                if (idProperty == null) return;

                var idValue = idProperty.GetValue(_currentEntity);

                if (idValue == null || Convert.ToInt32(idValue) == 0)
                {
                    // Новая запись
                    dbSet.Add(_currentEntity);
                }

                _db.SaveChanges();
                LoadTableData();

                MessageBox.Show("Данные успешно сохранены", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_currentEntity == null || dataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите запись для удаления");
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить выбранную запись?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                Type entityType = _tableTypes[_currentTable];
                var dbSet = _db.Set(entityType);
                dbSet.Remove(_currentEntity);
                _db.SaveChanges();

                LoadTableData();
                _currentEntity = null;

                // Очищаем поля
                foreach (var kvp in _fieldPanels)
                {
                    var stackPanel = kvp.Value;
                    var inputControl = stackPanel.Children[1] as Control;

                    if (inputControl is TextBox textBox)
                    {
                        textBox.Text = "";
                    }
                    else if (inputControl is DatePicker datePicker)
                    {
                        datePicker.SelectedDate = null;
                    }
                    else if (inputControl is ComboBox comboBox)
                    {
                        comboBox.SelectedItem = null;
                    }
                }

                MessageBox.Show("Запись успешно удалена", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}");
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            var mainDashboard = new MainDashboard();
            mainDashboard.Show();
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _db.Dispose();
        }
    }

    // Вспомогательные extension методы
    public static class QueryableExtensions
    {
        public static IQueryable IncludeAll(this IQueryable query)
        {
            // Этот метод можно расширить для загрузки связанных данных
            return query;
        }

        public static System.Collections.IList ToList(this IQueryable query, Type elementType)
        {
            var method = typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(elementType);
            return (System.Collections.IList)method.Invoke(null, new object[] { query });
        }
    }
}