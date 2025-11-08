using System;
using System.Linq;
using System.Windows;

namespace material_design
{
    public partial class EditEmployeeWindow : Window
    {
        private cafe_barEntities1 _db;
        private Employees _employee;

        public EditEmployeeWindow(Employees employee = null)
        {
            InitializeComponent();
            _db = new cafe_barEntities1();
            _employee = employee ?? new Employees();

            // Загружаем должности в ComboBox
            cmbPost.ItemsSource = _db.Post.ToList();

            if (_employee.id_employee != 0)
            {
                txtName.Text = _employee.name_employee;
                txtPhone.Text = _employee.ph_number_emp;
                cmbPost.SelectedValue = _employee.post_emp_fk;
                txtEmail.Text = _employee.email;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введите ФИО сотрудника");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPhone.Text) || txtPhone.Text.Length != 11)
                {
                    MessageBox.Show("Введите корректный номер телефона (11 цифр)");
                    return;
                }

                if (cmbPost.SelectedValue == null)
                {
                    MessageBox.Show("Выберите должность");
                    return;
                }

                _employee.name_employee = txtName.Text;
                _employee.ph_number_emp = txtPhone.Text;
                _employee.post_emp_fk = (int)cmbPost.SelectedValue;
                _employee.email = txtEmail.Text;

                if (_employee.id_employee == 0)
                    _db.Employees.Add(_employee);

                _db.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}