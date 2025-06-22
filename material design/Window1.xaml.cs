
using material_design;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls; // Для TextBox, Button и других элементов управления


namespace material_design
{
    public partial class Window1 : Window
    {
        public Employees SelectedEmployee { get; set; }

        public bool IsNewEmployee { get; set; }

        public Window1(Employees employee = null)
        {
            InitializeComponent();
            tbId.Text = tbId.Tag.ToString();
            tbId.Foreground = Brushes.Gray;

            tbName.Text = tbName.Tag.ToString();
            tbName.Foreground = Brushes.Gray;

            tbPhone.Text = tbPhone.Tag.ToString();
            tbPhone.Foreground = Brushes.Gray;

            tbPost.Text = tbPost.Tag.ToString();
            tbPost.Foreground = Brushes.Gray;

            tbEmail.Text = tbEmail.Tag.ToString();
            tbEmail.Foreground = Brushes.Gray;

            if (employee != null)
            {
                SelectedEmployee = employee;
                SelectedEmployee.id_employee = Convert.ToInt32(tbId.Text);
                SelectedEmployee.name_employee = tbName.Text;
                SelectedEmployee.ph_number_emp = tbPhone.Text;
                SelectedEmployee.post_emp_fk = Convert.ToInt32(tbPost.Text);
                SelectedEmployee.email = tbEmail.Text;

                IsNewEmployee = false;
            }
            else
            {
                SelectedEmployee = new Employees();
                IsNewEmployee = true;
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            SelectedEmployee.id_employee = Convert.ToInt32(tbId.Text);
            SelectedEmployee.name_employee = tbName.Text;
            SelectedEmployee.ph_number_emp = tbPhone.Text;
            SelectedEmployee.post_emp_fk = Convert.ToInt32(tbPost.Text);
            SelectedEmployee.email = tbEmail.Text;


            DialogResult = true;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && textBox.Text == textBox.Tag.ToString())
            {
                textBox.Text = "";
                textBox.Foreground = Brushes.Black;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = textBox.Tag.ToString();
                textBox.Foreground = Brushes.Gray;
            }
        }

        // В конструкторе окна инициализируйте подсказки:
        
    }
}