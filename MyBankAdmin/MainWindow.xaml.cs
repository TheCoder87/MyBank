﻿using System;
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

namespace MyBankAdmin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Entities Bank { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            Bank = new Entities();
            UsersListBox.ItemsSource = Bank.AspNetUsers.ToList();
        }

        private void UsersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UserItemsGrid.DataContext = (sender as ListBox).SelectedItem as AspNetUser;
            Bank.SaveChanges();
        }

        private void AccountsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Bank.SaveChanges();
        }
    }
}