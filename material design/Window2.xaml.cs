using material_design;
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
using System.Windows.Shapes;

namespace material_design
{
    /// <summary>
    /// Логика взаимодействия для Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {
        cafe_barEntities1 db;

        public Window2()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            db = new cafe_barEntities1();
            menu.ItemsSource = db.Menu.ToList();
            post.ItemsSource = db.Post.ToList();
            clients.ItemsSource = db.Clients.ToList();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Закрываем текущее окно
            this.Close();

           
        }
    }
}
