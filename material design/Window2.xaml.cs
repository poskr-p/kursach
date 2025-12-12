using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace material_design
{
    public partial class Window2 : Window
    {
        private cafe_barEntities _db = new cafe_barEntities();
        private string _currentTable;
        private object _currentEntity;
        private Dictionary<string, Type> _tableTypes = new Dictionary<string, Type>();
        private Dictionary<string, Control> _fieldControls = new Dictionary<string, Control>();
        private Dictionary<string, ComboBox> _comboBoxControls = new Dictionary<string, ComboBox>();
        private List<string> _excludedProperties = new List<string> { "photo_data" };

        public Window2()
        {
            InitializeComponent();
            InitializeTableTypes();
        }

        private void InitializeTableTypes()
        {
            // Используем English имена из RussianTranslator
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

                // Получаем данные без навигационных свойств
                var data = ((IQueryable)dbSet).AsNoTracking().ToList(entityType);

                // Настраиваем DataGrid
                dataGrid.ItemsSource = data;
                dataGrid.AutoGenerateColumns = false;
                dataGrid.Columns.Clear();

                // Создаем колонки вручную для контроля
                CreateDataGridColumns(entityType);

                tbStatus.Text = $"Загружено записей: {((IList)data).Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}\n{ex.InnerException?.Message}");
            }
        }

        private void CreateDataGridColumns(Type entityType)
        {
            var properties = entityType.GetProperties()
                .Where(p => !IsNavigationProperty(p) &&
                           !_excludedProperties.Contains(p.Name) &&
                           IsSimpleType(p.PropertyType))
                .ToList();

            foreach (var prop in properties)
            {
                var column = new DataGridTextColumn
                {
                    Header = RussianTranslator.GetFieldName(prop.Name),
                    Binding = new System.Windows.Data.Binding(prop.Name)
                };

                // Форматирование для разных типов данных
                if (prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(decimal?))
                {
                    column.Binding.StringFormat = "F2";
                }
                else if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                {
                    column.Binding.StringFormat = "dd.MM.yyyy HH:mm";
                }

                dataGrid.Columns.Add(column);
            }
        }

        private void CreateEditFields()
        {
            spFields.Children.Clear();
            _fieldControls.Clear();
            _comboBoxControls.Clear();

            Type entityType = _tableTypes[_currentTable];

            // Получаем все простые свойства, исключая навигационные
            var properties = entityType.GetProperties()
                .Where(p => !IsNavigationProperty(p) &&
                           !_excludedProperties.Contains(p.Name) &&
                           !p.Name.EndsWith("_fk")) // Внешние ключи обрабатываем отдельно
                .Where(p => IsSimpleType(p.PropertyType))
                .OrderBy(p => p.Name) // Сортируем для красивого отображения
                .ToList();

            foreach (var prop in properties)
            {
                CreateField(prop);
            }

            // Добавляем поля для внешних ключей
            AddForeignKeyFields(entityType);
        }

        private void CreateField(PropertyInfo prop)
        {
            var stackPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 10) };

            string displayName = RussianTranslator.GetFieldName(prop.Name);
            var textBlock = new TextBlock
            {
                Text = displayName + ":",
                Margin = new Thickness(0, 0, 0, 5),
                FontWeight = FontWeights.Normal
            };

            Control inputControl = CreateInputControl(prop);

            if (inputControl != null)
            {
                stackPanel.Children.Add(textBlock);
                stackPanel.Children.Add(inputControl);
                spFields.Children.Add(stackPanel);

                _fieldControls[prop.Name] = inputControl;
            }
        }

        private Control CreateInputControl(PropertyInfo prop)
        {
            var controlType = GetControlTypeForProperty(prop.PropertyType);

            switch (controlType)
            {
                case ControlType.TextBox:
                    var textBox = new TextBox();
                    textBox.TextChanged += (s, e) => UpdateCurrentEntity(prop, ((TextBox)s).Text);
                    return textBox;

                case ControlType.NumericTextBox:
                    var numTextBox = new TextBox();
                    numTextBox.PreviewTextInput += (s, e) =>
                        e.Handled = !char.IsDigit(e.Text, 0) && e.Text != "-" && e.Text != ".";
                    numTextBox.TextChanged += (s, e) => UpdateCurrentEntity(prop, ((TextBox)s).Text);
                    return numTextBox;

                case ControlType.DatePicker:
                    var datePicker = new DatePicker();
                    datePicker.SelectedDateChanged += (s, e) =>
                        UpdateCurrentEntity(prop, ((DatePicker)s).SelectedDate);
                    return datePicker;

                case ControlType.ComboBox:
                    // Для простых enum или справочников
                    var comboBox = new ComboBox();
                    comboBox.SelectionChanged += (s, e) =>
                        UpdateCurrentEntity(prop, ((ComboBox)s).SelectedValue);
                    return comboBox;

                default:
                    return new TextBox();
            }
        }

        private ControlType GetControlTypeForProperty(Type propertyType)
        {
            var type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            if (type == typeof(string))
                return ControlType.TextBox;
            else if (type == typeof(int) || type == typeof(decimal) || type == typeof(double) ||
                     type == typeof(float) || type == typeof(byte) || type == typeof(short))
                return ControlType.NumericTextBox;
            else if (type == typeof(DateTime))
                return ControlType.DatePicker;
            else if (type.IsEnum)
                return ControlType.ComboBox;
            else
                return ControlType.TextBox;
        }

        private bool IsSimpleType(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;

            return type.IsPrimitive ||
                   type == typeof(string) ||
                   type == typeof(decimal) ||
                   type == typeof(DateTime) ||
                   type == typeof(Guid) ||
                   type.IsEnum;
        }

        private bool IsNavigationProperty(PropertyInfo prop)
        {
            var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            // Навигационные свойства обычно являются коллекциями или ссылками на другие сущности
            if (propType.IsClass && propType != typeof(string) && propType != typeof(byte[]))
            {
                // Проверяем, является ли тип коллекцией
                if (typeof(IEnumerable).IsAssignableFrom(propType) && propType != typeof(string))
                    return true;

                // Проверяем, есть ли в контексте DbSet для этого типа
                var dbSetProperties = typeof(cafe_barEntities).GetProperties()
                    .Where(p => p.PropertyType.IsGenericType &&
                               p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

                foreach (var dbSetProp in dbSetProperties)
                {
                    var entityType = dbSetProp.PropertyType.GetGenericArguments()[0];
                    if (entityType == propType || propType.IsAssignableFrom(entityType))
                        return true;
                }
            }

            return false;
        }

        private void AddForeignKeyFields(Type entityType)
        {
            // Определяем внешние ключи для текущей таблицы
            var fkProperties = entityType.GetProperties()
                .Where(p => p.Name.EndsWith("_fk") && !_excludedProperties.Contains(p.Name))
                .ToList();

            foreach (var fkProp in fkProperties)
            {
                AddForeignKeyComboBox(fkProp);
            }
        }

        private void AddForeignKeyComboBox(PropertyInfo fkProperty)
        {
            var stackPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 10) };

            string displayName = RussianTranslator.GetFieldName(fkProperty.Name);
            var textBlock = new TextBlock
            {
                Text = displayName + ":",
                Margin = new Thickness(0, 0, 0, 5),
                FontWeight = FontWeights.Normal
            };

            // Определяем связанную таблицу и поля для ComboBox
            var comboBoxInfo = GetComboBoxInfo(fkProperty.Name);
            if (comboBoxInfo == null) return;

            var comboBox = new ComboBox
            {
                DisplayMemberPath = comboBoxInfo.DisplayMember,
                SelectedValuePath = comboBoxInfo.ValueMember,
                ItemsSource = comboBoxInfo.ItemsSource,
                Margin = new Thickness(0, 0, 0, 5),
                Tag = fkProperty.Name
            };

            comboBox.SelectionChanged += (s, e) =>
            {
                if (comboBox.SelectedValue != null && _currentEntity != null)
                {
                    try
                    {
                        var value = Convert.ChangeType(comboBox.SelectedValue, fkProperty.PropertyType);
                        fkProperty.SetValue(_currentEntity, value);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка установки значения: {ex.Message}");
                    }
                }
            };

            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(comboBox);
            spFields.Children.Add(stackPanel);

            _comboBoxControls[fkProperty.Name] = comboBox;
        }

        private ComboBoxInfo GetComboBoxInfo(string fkPropertyName)
        {
            switch (_currentTable)
            {
                case "Сотрудники":
                    if (fkPropertyName == "post_emp_fk")
                        return new ComboBoxInfo(_db.Post.ToList(), "title_post", "id_post");
                    break;

                case "Постоянные клиенты":
                    if (fkPropertyName == "id_reg_client_fk")
                        return new ComboBoxInfo(_db.Clients.ToList(), "name_client", "id_client");
                    break;

                case "Бронирования":
                    if (fkPropertyName == "id_client_fk")
                        return new ComboBoxInfo(_db.Clients.ToList(), "name_client", "id_client");
                    if (fkPropertyName == "id_employee_fk")
                        return new ComboBoxInfo(_db.Employees.ToList(), "name_employee", "id_employee");
                    break;

                case "Меню":
                    if (fkPropertyName == "id_category_fk")
                        return new ComboBoxInfo(_db.CategoriesMenu.ToList(), "title_category", "id_category");
                    break;

                case "Заказы":
                    if (fkPropertyName == "id_cli_fk")
                        return new ComboBoxInfo(_db.Clients.ToList(), "name_client", "id_client");
                    if (fkPropertyName == "id_emp_fk")
                        return new ComboBoxInfo(_db.Employees.ToList(), "name_employee", "id_employee");
                    break;

                case "Детали заказов":
                    if (fkPropertyName == "id_order_fk")
                        return new ComboBoxInfo(_db.Orders.ToList(), "id_order", "id_order");
                    if (fkPropertyName == "id_menu_item_fk")
                        return new ComboBoxInfo(_db.Menu.ToList(), "item_name", "id_menu_item");
                    break;
            }

            return null;
        }

        private void UpdateCurrentEntity(PropertyInfo prop, object value)
        {
            if (_currentEntity != null && prop != null)
            {
                try
                {
                    object convertedValue = null;

                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        convertedValue = GetDefaultValue(prop.PropertyType);
                    }
                    else
                    {
                        convertedValue = ConvertValue(prop.PropertyType, value);
                    }

                    prop.SetValue(_currentEntity, convertedValue);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка обновления свойства {prop.Name}: {ex.Message}");
                }
            }
        }

        private object ConvertValue(Type targetType, object value)
        {
            if (value == null) return null;

            targetType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            var stringValue = value.ToString();

            try
            {
                if (targetType == typeof(string))
                    return stringValue;
                else if (targetType == typeof(int))
                    return int.Parse(stringValue);
                else if (targetType == typeof(decimal))
                    return decimal.Parse(stringValue);
                else if (targetType == typeof(DateTime))
                    return DateTime.Parse(stringValue);
                else if (targetType == typeof(byte))
                    return byte.Parse(stringValue);
                else if (targetType == typeof(short))
                    return short.Parse(stringValue);
                else if (targetType == typeof(bool))
                    return bool.Parse(stringValue);
                else if (targetType.IsEnum)
                    return Enum.Parse(targetType, stringValue);
                else
                    return Convert.ChangeType(value, targetType);
            }
            catch
            {
                return GetDefaultValue(targetType);
            }
        }

        private object GetDefaultValue(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentEntity = dataGrid.SelectedItem;
            if (_currentEntity == null) return;

            // Заполняем текстовые поля и DatePicker
            foreach (var kvp in _fieldControls)
            {
                var propertyName = kvp.Key;
                var control = kvp.Value;

                var prop = _currentEntity.GetType().GetProperty(propertyName);
                if (prop == null) continue;

                var value = prop.GetValue(_currentEntity);

                if (control is TextBox textBox)
                {
                    textBox.Text = value?.ToString() ?? "";
                }
                else if (control is DatePicker datePicker)
                {
                    datePicker.SelectedDate = value as DateTime?;
                }
            }

            // Заполняем ComboBox для внешних ключей
            foreach (var kvp in _comboBoxControls)
            {
                var propertyName = kvp.Key;
                var comboBox = kvp.Value;

                var prop = _currentEntity.GetType().GetProperty(propertyName);
                if (prop == null) continue;

                var value = prop.GetValue(_currentEntity);
                comboBox.SelectedValue = value;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Type entityType = _tableTypes[_currentTable];
                _currentEntity = Activator.CreateInstance(entityType);

                // Очищаем все поля
                ClearAllFields();

                MessageBox.Show("Создана новая запись. Заполните поля и нажмите 'Сохранить'",
                    "Новая запись", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания новой записи: {ex.Message}");
            }
        }

        private void ClearAllFields()
        {
            foreach (var control in _fieldControls.Values)
            {
                if (control is TextBox textBox)
                    textBox.Text = "";
                else if (control is DatePicker datePicker)
                    datePicker.SelectedDate = null;
            }

            foreach (var comboBox in _comboBoxControls.Values)
            {
                comboBox.SelectedItem = null;
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

                // Определяем свойство ID
                var idProperty = entityType.GetProperties()
                    .FirstOrDefault(p => p.Name.ToLower().Contains("id") &&
                                        !p.Name.ToLower().Contains("_fk"));

                if (idProperty != null)
                {
                    var idValue = idProperty.GetValue(_currentEntity);
                    var id = Convert.ToInt32(idValue);

                    if (id == 0) // Новая запись
                    {
                        dbSet.Add(_currentEntity);
                        MessageBox.Show("Новая запись добавлена", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else // Обновление существующей
                    {
                        var existing = dbSet.Find(idValue);
                        if (existing != null)
                        {
                            // Копируем значения из _currentEntity в existing
                            foreach (var prop in entityType.GetProperties()
                                .Where(p => !IsNavigationProperty(p) && p.CanWrite))
                            {
                                var value = prop.GetValue(_currentEntity);
                                prop.SetValue(existing, value);
                            }
                        }
                        else
                        {
                            dbSet.Add(_currentEntity);
                        }
                    }

                    _db.SaveChanges();
                    LoadTableData();

                    MessageBox.Show("Данные успешно сохранены", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}\n{ex.InnerException?.Message}");
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

                // Находим запись в базе данных
                var idProperty = entityType.GetProperties()
                    .FirstOrDefault(p => p.Name.ToLower().Contains("id") &&
                                        !p.Name.ToLower().Contains("_fk"));

                if (idProperty != null)
                {
                    var idValue = idProperty.GetValue(_currentEntity);
                    var entityToDelete = dbSet.Find(idValue);

                    if (entityToDelete != null)
                    {
                        dbSet.Remove(entityToDelete);
                        _db.SaveChanges();

                        LoadTableData();
                        _currentEntity = null;
                        ClearAllFields();

                        MessageBox.Show("Запись успешно удалена", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
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
            _db?.Dispose();
        }

        // Вспомогательные классы
        private class ComboBoxInfo
        {
            public IEnumerable ItemsSource { get; }
            public string DisplayMember { get; }
            public string ValueMember { get; }

            public ComboBoxInfo(IEnumerable itemsSource, string displayMember, string valueMember)
            {
                ItemsSource = itemsSource;
                DisplayMember = displayMember;
                ValueMember = valueMember;
            }
        }

        private enum ControlType
        {
            TextBox,
            NumericTextBox,
            DatePicker,
            ComboBox
        }
    }

    // Extension методы
    public static class QueryableExtensions
    {
        public static IQueryable AsNoTracking(this IQueryable query)
        {
            var method = typeof(System.Data.Entity.QueryableExtensions)
                .GetMethods()
                .FirstOrDefault(m => m.Name == "AsNoTracking" && m.IsGenericMethod);

            if (method != null)
            {
                var genericMethod = method.MakeGenericMethod(query.ElementType);
                return (IQueryable)genericMethod.Invoke(null, new object[] { query });
            }

            return query;
        }

        public static IList ToList(this IQueryable query, Type elementType)
        {
            var toListMethod = typeof(Enumerable).GetMethods()
                .First(m => m.Name == "ToList" && m.IsGenericMethod)
                .MakeGenericMethod(elementType);

            return (IList)toListMethod.Invoke(null, new object[] { query });
        }
    }
}