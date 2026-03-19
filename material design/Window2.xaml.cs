using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace material_design
{
    public partial class Window2 : Window
    {
        private cafe_barEntities _db = new cafe_barEntities();
        private string _currentTable;
        private object _currentEntity;
        private Dictionary<string, Control> _fieldControls = new Dictionary<string, Control>();

        private readonly Dictionary<string, TableConfig> _tableConfigs = new Dictionary<string, TableConfig>();

        public Window2()
        {
            InitializeComponent();
            InitializeTableConfigs();
        }

        private void InitializeTableConfigs()
        {
            _tableConfigs.Add("Должности", new TableConfig
            {
                EntityType = typeof(Post),
                DisplayColumns = new List<ColumnConfig>
                {
                    new ColumnConfig { Field = "id_post", Header = "ID", Type = ColumnType.Id },
                    new ColumnConfig { Field = "title_post", Header = "Должность" },
                    new ColumnConfig { Field = "accessLevel", Header = "Уровень доступа", IsNumeric = true }
                },
                Editable = true,
                TableName = "Post"
            });

            _tableConfigs.Add("Сотрудники", new TableConfig
            {
                EntityType = typeof(Employees),
                DisplayColumns = new List<ColumnConfig>
                {
                    new ColumnConfig { Field = "id_employee", Header = "ID", Type = ColumnType.Id },
                    new ColumnConfig { Field = "name_employee", Header = "ФИО" },
                    new ColumnConfig { Field = "ph_number_emp", Header = "Телефон" },
                    new ColumnConfig { Field = "email", Header = "Email" },
                    new ColumnConfig
                    {
                        Field = "post_emp_fk",
                        Header = "Должность",
                        Type = ColumnType.ForeignKey,
                        LookupTable = typeof(Post),
                        DisplayField = "title_post",
                        ValueField = "id_post"
                    }
                },
                Editable = false,
                TableName = "Employees"
            });

            // Клиенты
            _tableConfigs.Add("Клиенты", new TableConfig
            {
                EntityType = typeof(Clients),
                DisplayColumns = new List<ColumnConfig>
                {
                    new ColumnConfig { Field = "id_client", Header = "ID", Type = ColumnType.Id },
                    new ColumnConfig { Field = "name_client", Header = "ФИО клиента" },
                    new ColumnConfig { Field = "ph_numb_client", Header = "Телефон" }
                },
                Editable = true,
                TableName = "Clients"
            });

            _tableConfigs.Add("Постоянные клиенты", new TableConfig
            {
                EntityType = typeof(Regular_Clients),
                DisplayColumns = new List<ColumnConfig>
                {
                    new ColumnConfig
                    {
                        Field = "id_reg_client_fk",
                        Header = "Клиент",
                        Type = ColumnType.ForeignKey,
                        LookupTable = typeof(Clients),
                        DisplayField = "name_client",
                        ValueField = "id_client"
                    },
                    new ColumnConfig { Field = "discount_rate", Header = "Скидка (%)", IsNumeric = true, Format = "F2" },
                    new ColumnConfig { Field = "total_spent", Header = "Всего потрачено", IsNumeric = true, Format = "F2" }
                },
                Editable = false,
                TableName = "Regular_Clients"
            });

            _tableConfigs.Add("Бронирования", new TableConfig
            {
                EntityType = typeof(Reservation),
                DisplayColumns = new List<ColumnConfig>
                {
                    new ColumnConfig { Field = "id_reservation", Header = "ID", Type = ColumnType.Id },
                    new ColumnConfig
                    {
                        Field = "id_client_fk",
                        Header = "Клиент",
                        Type = ColumnType.ForeignKey,
                        LookupTable = typeof(Clients),
                        DisplayField = "name_client",
                        ValueField = "id_client"
                    },
                    new ColumnConfig
                    {
                        Field = "id_employee_fk",
                        Header = "Сотрудник",
                        Type = ColumnType.ForeignKey,
                        LookupTable = typeof(Employees),
                        DisplayField = "name_employee",
                        ValueField = "id_employee"
                    },
                    new ColumnConfig { Field = "reservation_date", Header = "Дата бронирования", IsDateTime = true },
                    new ColumnConfig { Field = "guests_count", Header = "Количество гостей", IsNumeric = true }
                },
                Editable = false,
                TableName = "Reservation"
            });

            _tableConfigs.Add("Категории меню", new TableConfig
            {
                EntityType = typeof(CategoriesMenu),
                DisplayColumns = new List<ColumnConfig>
                {
                    new ColumnConfig { Field = "id_category", Header = "ID", Type = ColumnType.Id },
                    new ColumnConfig { Field = "title_category", Header = "Категория" }
                },
                Editable = true,
                TableName = "CategoriesMenu"
            });

            _tableConfigs.Add("Меню", new TableConfig
            {
                EntityType = typeof(Menu),
                DisplayColumns = new List<ColumnConfig>
                {
                    new ColumnConfig { Field = "id_menu_item", Header = "ID", Type = ColumnType.Id },
                    new ColumnConfig { Field = "item_name", Header = "Название" },
                    new ColumnConfig
                    {
                        Field = "id_category_fk",
                        Header = "Категория",
                        Type = ColumnType.ForeignKey,
                        LookupTable = typeof(CategoriesMenu),
                        DisplayField = "title_category",
                        ValueField = "id_category"
                    },
                    new ColumnConfig { Field = "cost_item", Header = "Цена", IsNumeric = true, Format = "F2" }
                },
                Editable = false,
                TableName = "Menu"
            });

            _tableConfigs.Add("Заказы", new TableConfig
            {
                EntityType = typeof(Orders),
                DisplayColumns = new List<ColumnConfig>
                {
                    new ColumnConfig { Field = "id_order", Header = "ID", Type = ColumnType.Id },
                    new ColumnConfig
                    {
                        Field = "id_cli_fk",
                        Header = "Клиент",
                        Type = ColumnType.ForeignKey,
                        LookupTable = typeof(Clients),
                        DisplayField = "name_client",
                        ValueField = "id_client"
                    },
                    new ColumnConfig
                    {
                        Field = "id_emp_fk",
                        Header = "Сотрудник",
                        Type = ColumnType.ForeignKey,
                        LookupTable = typeof(Employees),
                        DisplayField = "name_employee",
                        ValueField = "id_employee"
                    },
                    new ColumnConfig { Field = "order_date", Header = "Дата заказа", IsDateTime = true },
                    new ColumnConfig { Field = "totalAmount", Header = "Сумма", IsNumeric = true, Format = "F2" }
                },
                Editable = false,
                TableName = "Orders"
            });

            _tableConfigs.Add("Детали заказов", new TableConfig
            {
                EntityType = typeof(Order_details),
                DisplayColumns = new List<ColumnConfig>
                {
                    new ColumnConfig { Field = "id_order_details", Header = "ID", Type = ColumnType.Id },
                    new ColumnConfig
                    {
                        Field = "id_order_fk",
                        Header = "Заказ",
                        Type = ColumnType.ForeignKey,
                        LookupTable = typeof(Orders),
                        DisplayField = "id_order",
                        ValueField = "id_order"
                    },
                    new ColumnConfig
                    {
                        Field = "id_menu_item_fk",
                        Header = "Позиция меню",
                        Type = ColumnType.ForeignKey,
                        LookupTable = typeof(Menu),
                        DisplayField = "item_name",
                        ValueField = "id_menu_item"
                    },
                    new ColumnConfig { Field = "quantity", Header = "Количество", IsNumeric = true },
                    new ColumnConfig { Field = "unit_price", Header = "Цена за единицу", IsNumeric = true, Format = "F2" },
                    new ColumnConfig { Field = "subtotal", Header = "Сумма", IsNumeric = true, Format = "F2" }
                },
                Editable = false,
                TableName = "Order_details"
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTables();
        }

        private void LoadTables()
        {
            lvTables.Items.Clear();
            foreach (var tableName in _tableConfigs.Keys.OrderBy(k => k))
            {
                lvTables.Items.Add(tableName);
            }
        }

        private void lvTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvTables.SelectedItem == null) return;

            _currentTable = lvTables.SelectedItem.ToString();
            tbTableTitle.Text = _currentTable;

            var config = _tableConfigs[_currentTable];
            spEditPanel.Visibility = config.Editable ? Visibility.Visible : Visibility.Collapsed;
            tbEditTitle.Text = config.Editable ? $"Редактирование: {_currentTable}" : $"{_currentTable} (только просмотр)";

            LoadTableData(config);
            if (config.Editable)
            {
                CreateEditFields(config);
            }
        }

        private void LoadTableData(TableConfig config)
        {
            try
            {
                var data = LoadTableDataInternal(config);

                var displayData = TransformDataForDisplay(data, config);
                dataGrid.ItemsSource = displayData;

                CreateDataGridColumns(config);
                tbStatus.Text = $"Загружено записей: {displayData.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}\n{ex.InnerException?.Message}");
                tbStatus.Text = "Ошибка загрузки";
            }
        }

        private IEnumerable LoadTableDataInternal(TableConfig config)
        {
            switch (config.TableName)
            {
                case "Post":
                    return _db.Post.ToList();
                case "Employees":
                    return _db.Employees.Include(e => e.Post).ToList();
                case "Clients":
                    return _db.Clients.ToList();
                case "Regular_Clients":
                    return _db.Regular_Clients.Include(rc => rc.Clients).ToList();
                case "Reservation":
                    return _db.Reservation
                        .Include(r => r.Clients)
                        .Include(r => r.Employees)
                        .ToList();
                case "CategoriesMenu":
                    return _db.CategoriesMenu.ToList();
                case "Menu":
                    return _db.Menu.Include(m => m.CategoriesMenu).ToList();
                case "Orders":
                    return _db.Orders
                        .Include(o => o.Clients)
                        .Include(o => o.Employees)
                        .ToList();
                case "Order_details":
                    return _db.Order_details
                        .Include(od => od.Orders)
                        .Include(od => od.Menu)
                        .ToList();
                default:
                    return new List<object>();
            }
        }

        private List<Dictionary<string, object>> TransformDataForDisplay(IEnumerable sourceData, TableConfig config)
        {
            var result = new List<Dictionary<string, object>>();

            foreach (var item in sourceData)
            {
                var displayItem = new Dictionary<string, object>();

                foreach (var column in config.DisplayColumns)
                {
                    object value = null;

                    if (column.Type == ColumnType.ForeignKey)
                    {
                        value = GetForeignKeyDisplayValue(item, column);
                    }
                    else
                    {
                        value = GetPropertyValue(item, column.Field);
                    }

                    displayItem[column.Field] = value ?? string.Empty;
                }

                result.Add(displayItem);
            }

            return result;
        }

        private object GetPropertyValue(object obj, string propertyName)
        {
            try
            {
                var property = obj.GetType().GetProperty(propertyName);
                return property?.GetValue(obj) ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private object GetForeignKeyDisplayValue(object item, ColumnConfig column)
        {
            try
            {
                var fkValue = GetPropertyValue(item, column.Field);
                if (fkValue == null || (fkValue is int intValue && intValue == 0))
                    return string.Empty;

                var navPropertyName = column.Field.Replace("_fk", "");
                var navProperty = item.GetType().GetProperty(navPropertyName);

                if (navProperty != null)
                {
                    var navObject = navProperty.GetValue(item);
                    if (navObject != null)
                    {
                        var displayProperty = navObject.GetType().GetProperty(column.DisplayField);
                        return displayProperty?.GetValue(navObject) ?? fkValue;
                    }
                }

                return fkValue;
            }
            catch
            {
                return string.Empty;
            }
        }

        private void CreateDataGridColumns(TableConfig config)
        {
            dataGrid.Columns.Clear();
            dataGrid.AutoGenerateColumns = false;

            foreach (var column in config.DisplayColumns)
            {
                DataGridColumn gridColumn = CreateDataGridColumn(column);
                dataGrid.Columns.Add(gridColumn);
            }
        }

        private DataGridColumn CreateDataGridColumn(ColumnConfig column)
        {
            DataGridTextColumn textColumn = new DataGridTextColumn
            {
                Header = column.Header,
                Binding = new Binding($"[{column.Field}]")
            };

            // Форматирование
            if (!string.IsNullOrEmpty(column.Format))
            {
                textColumn.Binding.StringFormat = column.Format;
            }
            else if (column.IsDateTime)
            {
                textColumn.Binding.StringFormat = "dd.MM.yyyy HH:mm";
            }
            else if (column.IsNumeric && column.Format == null)
            {
                textColumn.Binding.StringFormat = column.Field.Contains("rate") ? "F2" : "N2";
            }

            return textColumn;
        }

        private void CreateEditFields(TableConfig config)
        {
            spFields.Children.Clear();
            _fieldControls.Clear();

            if (!config.Editable) return;

            foreach (var column in config.DisplayColumns.Where(c => c.Type != ColumnType.Id))
            {
                if (column.Type == ColumnType.ForeignKey)
                {
                    CreateForeignKeyComboBox(column);
                }
                else
                {
                    CreateTextBoxField(column);
                }
            }
        }

        private void CreateTextBoxField(ColumnConfig column)
        {
            var stackPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 10) };

            var textBlock = new TextBlock
            {
                Text = column.Header + ":",
                Margin = new Thickness(0, 0, 0, 5),
                FontWeight = FontWeights.Normal
            };

            var textBox = new TextBox { Tag = column.Field };

            if (column.IsNumeric)
            {
                textBox.PreviewTextInput += (s, e) =>
                    e.Handled = !char.IsDigit(e.Text, 0) && e.Text != "-" && e.Text != ".";
            }

            textBox.TextChanged += (s, e) =>
            {
                if (_currentEntity != null)
                {
                    UpdateEntityProperty(column.Field, textBox.Text);
                }
            };

            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(textBox);
            spFields.Children.Add(stackPanel);

            _fieldControls[column.Field] = textBox;
        }

        private void CreateForeignKeyComboBox(ColumnConfig column)
        {
            var stackPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 10) };

            var textBlock = new TextBlock
            {
                Text = column.Header + ":",
                Margin = new Thickness(0, 0, 0, 5),
                FontWeight = FontWeights.Normal
            };

            var comboBox = new ComboBox
            {
                DisplayMemberPath = column.DisplayField,
                SelectedValuePath = column.ValueField,
                Tag = column.Field
            };

            LoadComboBoxData(comboBox, column.LookupTable);

            comboBox.SelectionChanged += (s, e) =>
            {
                if (_currentEntity != null && comboBox.SelectedValue != null)
                {
                    UpdateEntityProperty(column.Field, comboBox.SelectedValue);
                }
            };

            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(comboBox);
            spFields.Children.Add(stackPanel);

            _fieldControls[column.Field] = comboBox;
        }

        private void LoadComboBoxData(ComboBox comboBox, Type entityType)
        {
            try
            {
                IEnumerable data = null;

                if (entityType == typeof(Post))
                    data = _db.Post.ToList();
                else if (entityType == typeof(Employees))
                    data = _db.Employees.ToList();
                else if (entityType == typeof(Clients))
                    data = _db.Clients.ToList();
                else if (entityType == typeof(CategoriesMenu))
                    data = _db.CategoriesMenu.ToList();
                else if (entityType == typeof(Menu))
                    data = _db.Menu.ToList();
                else if (entityType == typeof(Orders))
                    data = _db.Orders.ToList();

                comboBox.ItemsSource = data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки данных для ComboBox: {ex.Message}");
            }
        }

        private void UpdateEntityProperty(string propertyName, object value)
        {
            if (_currentEntity == null) return;

            var property = _currentEntity.GetType().GetProperty(propertyName);
            if (property == null) return;

            try
            {
                object convertedValue = ConvertValue(property.PropertyType, value);
                property.SetValue(_currentEntity, convertedValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления свойства {propertyName}: {ex.Message}");
            }
        }

        private object ConvertValue(Type targetType, object value)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return GetDefaultValue(targetType);

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
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGrid.SelectedItem is Dictionary<string, object> selectedItem)
            {
                LoadEntityForEditing(selectedItem);
                UpdateFormFields();
            }
        }

        private void LoadEntityForEditing(Dictionary<string, object> displayData)
        {
            var config = _tableConfigs[_currentTable];

            var idColumn = config.DisplayColumns.FirstOrDefault(c => c.Type == ColumnType.Id);
            if (idColumn == null) return;

            var idValue = displayData[idColumn.Field];
            if (idValue == null) return;

            _currentEntity = FindEntityById(config, idValue);
        }

        private object FindEntityById(TableConfig config, object idValue)
        {
            switch (config.TableName)
            {
                case "Post":
                    return _db.Post.Find(idValue);
                case "Employees":
                    return _db.Employees.Find(idValue);
                case "Clients":
                    return _db.Clients.Find(idValue);
                case "CategoriesMenu":
                    return _db.CategoriesMenu.Find(idValue);
                case "Regular_Clients":
                    return _db.Regular_Clients.Find(idValue);
                case "Reservation":
                    return _db.Reservation.Find(idValue);
                case "Menu":
                    return _db.Menu.Find(idValue);
                case "Orders":
                    return _db.Orders.Find(idValue);
                case "Order_details":
                    return _db.Order_details.Find(idValue);
                default:
                    return null;
            }
        }

        private void UpdateFormFields()
        {
            if (_currentEntity == null) return;

            foreach (var kvp in _fieldControls)
            {
                var propertyName = kvp.Key;
                var control = kvp.Value;

                var propertyValue = GetPropertyValue(_currentEntity, propertyName);

                if (control is TextBox textBox)
                {
                    textBox.Text = propertyValue?.ToString() ?? "";
                }
                else if (control is ComboBox comboBox)
                {
                    comboBox.SelectedValue = propertyValue;
                }
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var config = _tableConfigs[_currentTable];
                _currentEntity = Activator.CreateInstance(config.EntityType);

                ClearFormFields();
                MessageBox.Show("Создана новая запись. Заполните поля и нажмите 'Сохранить'",
                    "Новая запись", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания новой записи: {ex.Message}");
            }
        }

        private void ClearFormFields()
        {
            foreach (var control in _fieldControls.Values)
            {
                if (control is TextBox textBox)
                    textBox.Text = "";
                else if (control is ComboBox comboBox)
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
                var config = _tableConfigs[_currentTable];

                var idColumn = config.DisplayColumns.First(c => c.Type == ColumnType.Id);
                var idProperty = config.EntityType.GetProperty(idColumn.Field);
                var idValue = idProperty.GetValue(_currentEntity);

                if (idValue == null || Convert.ToInt32(idValue) == 0)
                {
                    AddEntityToDbSet(config, _currentEntity);
                }

                _db.SaveChanges();
                LoadTableData(config);
                ClearFormFields();
                MessageBox.Show("Данные сохранены", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}\n{ex.InnerException?.Message}");
            }
        }

        private void AddEntityToDbSet(TableConfig config, object entity)
        {
            switch (config.TableName)
            {
                case "Post":
                    _db.Post.Add((Post)entity);
                    break;
                case "Employees":
                    _db.Employees.Add((Employees)entity);
                    break;
                case "Clients":
                    _db.Clients.Add((Clients)entity);
                    break;
                case "CategoriesMenu":
                    _db.CategoriesMenu.Add((CategoriesMenu)entity);
                    break;
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_currentEntity == null)
            {
                MessageBox.Show("Выберите запись для удаления");
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить выбранную запись?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var config = _tableConfigs[_currentTable];

                RemoveEntityFromDbSet(config, _currentEntity);

                _db.SaveChanges();
                LoadTableData(config);
                _currentEntity = null;
                ClearFormFields();

                MessageBox.Show("Запись успешно удалена", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}");
            }
        }

        private void RemoveEntityFromDbSet(TableConfig config, object entity)
        {
            switch (config.TableName)
            {
                case "Post":
                    _db.Post.Remove((Post)entity);
                    break;
                case "Employees":
                    _db.Employees.Remove((Employees)entity);
                    break;
                case "Clients":
                    _db.Clients.Remove((Clients)entity);
                    break;
                case "CategoriesMenu":
                    _db.CategoriesMenu.Remove((CategoriesMenu)entity);
                    break;
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (App.UserContext.IsAuthenticated)
            {
                var mainDashboard = new MainDashboard(
                    App.UserContext.UserName,
                    App.UserContext.UserRole,
                    App.UserContext.AccessLevel);
                mainDashboard.Show();
            }
            else
            {
                var authWindow = new autorization();
                authWindow.Show();
            }

            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _db?.Dispose();
        }

        // Вспомогательные классы
        private class TableConfig
        {
            public Type EntityType { get; set; }
            public List<ColumnConfig> DisplayColumns { get; set; }
            public bool Editable { get; set; }
            public string TableName { get; set; }
        }

        private class ColumnConfig
        {
            public string Field { get; set; }
            public string Header { get; set; }
            public ColumnType Type { get; set; } = ColumnType.Regular;
            public bool IsNumeric { get; set; }
            public bool IsDateTime { get; set; }
            public string Format { get; set; }

            // Для внешних ключей
            public Type LookupTable { get; set; }
            public string DisplayField { get; set; }
            public string ValueField { get; set; }
        }

        private enum ColumnType
        {
            Id,
            Regular,
            ForeignKey
        }
    }
}