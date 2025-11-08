using System;
using System.Linq;
using System.Windows;

namespace material_design
{
    public partial class EditOrderDetailWindow : Window
    {
        private cafe_barEntities1 _db;
        private Order_details _orderDetail;

        public EditOrderDetailWindow(Order_details orderDetail = null)
        {
            InitializeComponent();
            _db = new cafe_barEntities1();
            _orderDetail = orderDetail ?? new Order_details();

            // Загружаем заказы и позиции меню
            cmbOrder.ItemsSource = _db.Orders.ToList();
            cmbMenuItem.ItemsSource = _db.Menu.ToList();

            if (_orderDetail.id_order_details != 0)
            {
                cmbOrder.SelectedValue = _orderDetail.id_order_fk;
                cmbMenuItem.SelectedValue = _orderDetail.id_menu_item_fk;
                txtQuantity.Text = _orderDetail.quantity.ToString();
                txtUnitPrice.Text = _orderDetail.unit_price.ToString();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbOrder.SelectedValue == null)
                {
                    MessageBox.Show("Выберите заказ");
                    return;
                }

                if (cmbMenuItem.SelectedValue == null)
                {
                    MessageBox.Show("Выберите позицию меню");
                    return;
                }

                if (!short.TryParse(txtQuantity.Text, out short quantity) || quantity <= 0)
                {
                    MessageBox.Show("Введите корректное количество");
                    return;
                }

                if (!decimal.TryParse(txtUnitPrice.Text, out decimal unitPrice) || unitPrice <= 0)
                {
                    MessageBox.Show("Введите корректную цену");
                    return;
                }

                _orderDetail.id_order_fk = (int)cmbOrder.SelectedValue;
                _orderDetail.id_menu_item_fk = (int)cmbMenuItem.SelectedValue;
                _orderDetail.quantity = quantity;
                _orderDetail.unit_price = unitPrice;

                if (_orderDetail.id_order_details == 0)
                    _db.Order_details.Add(_orderDetail);

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