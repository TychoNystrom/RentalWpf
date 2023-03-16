/// <summary> 
/// MainWindow.xaml.cs
/// Main window for the program.
/// Starts the ContactForm Window.
/// Project Assignment 7 (VG)
/// Created by: Tycho Nyström (AC9320) 2014-01-13
/// </summary>
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
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
using System.Collections.ObjectModel;
using RentalWpf.CustomerData;
using System.Collections.Specialized;

namespace RentalWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
     
        private string[] GenreStrings = Enum.GetNames(typeof(Genre));
        private string[] _GenreStrings;

        ObservableCollection<DvdList> lstClassObservable = new ObservableCollection<DvdList>();
        ObservableCollection<Customer> CustomerObservable = new ObservableCollection<Customer>();

        System.ComponentModel.BackgroundWorker aWorker = new System.ComponentModel.BackgroundWorker();

        List<DvdList> lstResultTitles = new List<DvdList>();
        List<Customer> lstResultNames = new List<Customer>();

        private string _SearchTitle = string.Empty;
        private string _SearchName = string.Empty;
        private ObservableCollection<DvdList> _lstClassObservable= new ObservableCollection<DvdList>();
        private ObservableCollection<Customer> _customerObservable = new ObservableCollection<Customer>();
        private string path = @"..\..\dvdlist5.xls";
        public event PropertyChangedEventHandler PropertyChanged;
        private bool ShowOnlyLeasedMovies = false;
        
        public MainWindow()
        {
            InitializeComponent();
            InitializeGUI();
            
            aWorker.WorkerSupportsCancellation = true;
            aWorker.DoWork += aWorker_DoWork;
            aWorker.RunWorkerCompleted += aWorker_RunWorkerCompleted;

            aWorker.RunWorkerAsync();            
        }


        private void aWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            {
                //Connection String
                string connstring = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties='Excel 8.0;HDR=YES;IMEX=1';";

                List<string> lst = new List<string>();
                Stopwatch st = new Stopwatch();
                st.Start();
                Console.WriteLine("Started");
                using (OleDbConnection con = new OleDbConnection(connstring))
                {

                    int RowsRead = 0;
                    con.Open();

                    //Get All Sheets Name
                    DataTable sheetsName = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "Table" });

                    for (int i = 0; i < sheetsName.Rows.Count; i++)
                    {
                        //Get the First Sheet Name
                        string SheetName = sheetsName.Rows[i][2].ToString();
                        string SelectString = string.Empty;
                        string WhereClause = string.Empty;

                        string sql = string.Format("SELECT * FROM [{0}]", SheetName);

                        OleDbCommand oconn = new OleDbCommand(sql, con);

                        using (OleDbDataReader reader = oconn.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int Year = 0, Copies_Available = 0, Rented_Out;
                                double Price = 0.0;
                                string stringToConvert = string.Empty;
                                DvdList dl = new DvdList();

                                //Add Data to List<T>
                                dl.Refnr = RowsRead+1;
                                dl.Title = reader["DVD_Title"].ToString();
                                
                                stringToConvert = reader["Price"].ToString();
                                bool Ok = double.TryParse(stringToConvert, out Price);
                                
                                stringToConvert = (reader["Year"]).ToString();
                                Ok = Int32.TryParse(stringToConvert, out Year);
                                dl.Year = (Ok ? Year : 0);

                                dl.Genre = reader["Genre"].ToString();
                                stringToConvert = (reader["Copies_Available"]).ToString();
                                Ok = Int32.TryParse(stringToConvert, out Copies_Available);
                                dl.Copies_Available = (Ok ? Copies_Available : 0);

                                stringToConvert = (reader["Rented_Out"]).ToString();
                                Ok = Int32.TryParse(stringToConvert, out Rented_Out);
                                dl.Rented_Out = (Ok ? Rented_Out : 0);
                                dl.Rented_By = 0;
                                
                                proplstClassObservable.Add(dl);
                                lstClassObservable.Add(dl);
                                
                                RowsRead++;
                                if (aWorker.CancellationPending)
                                {
                                    e.Cancel = true;
                                    reader.Close();
                                    return;
                                }
                                UpdateDelegate update = new UpdateDelegate(UpdateLabel);
                                lblStatus.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, update, RowsRead);
                            }
                            reader.Close();
                        }
                        st.Stop();
                        Console.WriteLine("Elapsed = {0}", st.Elapsed.ToString());
                        if (Stopwatch.IsHighResolution)
                        {
                            Console.WriteLine("Timed with Hi res");
                        }
                        else
                            Console.WriteLine("Not Timed with Hi res");
                    }
                }
            }
        }

        /// <summary>
        /// aWorker_RunWorkerCompleted fires when the BackgroundWorker has finished its task
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (!(e.Cancelled))
            {      
                dataGridViewTitles.ItemsSource = lstClassObservable;
                dataGridViewTitles.DataContext = lstClassObservable;

                dataGridViewCustomers.ItemsSource = CustomerObservable;
            }
        }

        /// <summary>
        /// Declares an Delegate and gets notifications for the the worker thread that the the worker
        /// is progressing its task. It also updates a label with number of rows read.
        /// </summary>
        /// <param name="RowsRead"></param>
        private delegate void UpdateDelegate(int RowsRead);
        private void UpdateLabel(int RowsRead) 
        {
            lblStatus.Content = "Rows read: " + RowsRead.ToString();
        }

        /// <summary>
        /// Initializes the GUI and sets some bindings.
        /// </summary>
        private void InitializeGUI()
        {
            InitializeGenreCombobox();
            cmbGenre.SelectedIndex = (int)Genre.All_Genres; //All genres as default

            Binding bindingST = new Binding();
            bindingST.Source = this;
            bindingST.Path = new PropertyPath("SearchTitle");
            tboxSearchTitle.SetBinding(TextBox.TextProperty, bindingST);

            Binding bindingSN = new Binding();
            bindingSN.Source = this;
            bindingSN.Path = new PropertyPath("SearchName");
            tboxSearchCustomer.SetBinding(TextBox.TextProperty, bindingSN);


            Binding bindingDGV= new Binding();
            bindingDGV.Source = this;
            bindingDGV.Path = new PropertyPath("proplstClassObservable");
            dataGridViewTitles.SetBinding(TextBox.TextProperty, bindingDGV);

            Binding bindingDGVC = new Binding();
            bindingDGVC.Source = this;
            bindingDGVC.Path = new PropertyPath("propCustomerObservable");
            dataGridViewCustomers.SetBinding(TextBox.TextProperty, bindingDGVC);


            lblStatus.Content = "Initalizing...";            
        }

        /// <summary>
        /// Linq-questions fort titles
        /// </summary>
        private void LinqQuestionsTitles ()
        {
            string SearchStringLowerCase = SearchTitle.ToLower();

            string SearchGenre = ((Genre)cmbGenre.SelectedIndex).ToString().Replace('1', '/').Replace('_', ' ');
            int LeasedOut = (ShowOnlyLeasedMovies ? 1 : 0);
            //If Genre is selected but no search title is entered then display all titles for that genre
            if ((cmbGenre.SelectedIndex != (int)Genre.All_Genres) && SearchTitle == string.Empty)
            {

                var lstResult2 =
                (from c in lstClassObservable
                where (c.Genre.Equals(SearchGenre) && c.Rented_Out >= LeasedOut)
                select new DvdList { Refnr=c.Refnr, Title = c.Title, Price = c.Price, Year = c.Year, Genre = c.Genre, Copies_Available=c.Copies_Available, Rented_Out=c.Rented_Out, Rented_By=c.Rented_By}).ToList();

                dataGridViewTitles.ItemsSource = null;
                dataGridViewTitles.Items.Clear();
                lstResultTitles = lstResult2.ToList<DvdList>();
                dataGridViewTitles.ItemsSource = lstResultTitles;
                dataGridViewTitles.DataContext = lstResultTitles;
                lblStatus.Content = "Rows shown : " + lstResultTitles.Count.ToString();
            }
            //If no specific Genre is selected but a search title then dislay all the titles that contains the search criteria 
            else if ((cmbGenre.SelectedIndex == (int)Genre.All_Genres) && SearchTitle != string.Empty)
            {
                var lstResult2 =
                (from c in lstClassObservable
                where (c.Title.ToLower().Contains(SearchStringLowerCase) && c.Rented_Out >= LeasedOut)
                 select new DvdList { Refnr = c.Refnr, Title = c.Title, Price = c.Price, Year = c.Year, Genre = c.Genre, Copies_Available = c.Copies_Available, Rented_Out = c.Rented_Out, Rented_By = c.Rented_By }).ToList();

                dataGridViewTitles.ItemsSource = null;
                dataGridViewTitles.Items.Clear();
                lstResultTitles = lstResult2.ToList<DvdList>();
                dataGridViewTitles.ItemsSource = lstResultTitles;
                dataGridViewTitles.DataContext = lstResultTitles;
                lblStatus.Content = "Rows shown : " + lstResultTitles.Count.ToString();
            }
            //If a specific Genre is selected and a search title is entered then dislay all the titles that contains both the selected genre and the search criteria for the title
            else if ((cmbGenre.SelectedIndex != (int)Genre.All_Genres) && SearchTitle != string.Empty)
            {
                var lstResult2 =
                (from c in lstClassObservable
                 where c.Title.ToLower().Contains(SearchStringLowerCase) && c.Genre.Equals(SearchGenre) && c.Rented_Out >= LeasedOut
                 select new DvdList { Refnr = c.Refnr, Title = c.Title, Price = c.Price, Year = c.Year, Genre = c.Genre, Copies_Available = c.Copies_Available, Rented_Out = c.Rented_Out, Rented_By = c.Rented_By }).ToList();

                dataGridViewTitles.ItemsSource = null;
                dataGridViewTitles.Items.Clear();
                lstResultTitles = lstResult2.ToList<DvdList>();
                dataGridViewTitles.ItemsSource = lstResultTitles;
                dataGridViewTitles.DataContext = lstResultTitles;
                lblStatus.Content = "Rows shown : " + lstResultTitles.Count.ToString();
            }
            else if ((cmbGenre.SelectedIndex == (int)Genre.All_Genres) && SearchTitle == string.Empty)
            {
                var lstResult2 =
                (from c in lstClassObservable
                 where c.Rented_Out >= LeasedOut
                 select new DvdList { Refnr = c.Refnr, Title = c.Title, Price = c.Price, Year = c.Year, Genre = c.Genre, Copies_Available = c.Copies_Available, Rented_Out = c.Rented_Out, Rented_By = c.Rented_By }).ToList();

                dataGridViewTitles.ItemsSource = null;
                dataGridViewTitles.Items.Clear();
                lstResultTitles = lstResult2.ToList<DvdList>();
                dataGridViewTitles.ItemsSource = lstResultTitles;
                dataGridViewTitles.DataContext = lstResultTitles;
                lblStatus.Content = "Rows shown : " + lstResultTitles.Count.ToString();
            }


            LeasedOut = 0;

            ShowOnlyLeasedMovies = false;
            return;
        }

        /// <summary>
        /// Linq-queries for Customers
        /// </summary>
        private void LinqQuestionsCustomers()
        {
            string SearchStringLowerCase = SearchName.ToLower();
            //If no specific Genre is selected but a search title then dislay all the titles that contains the search criteria 
            if (SearchName != string.Empty)
            {
                var lstResult2 =
                (from c in CustomerObservable
                 where c.LastName.ToLower().Contains(SearchStringLowerCase) || c.FirstName.ToLower().Contains(SearchStringLowerCase)
                 select new Customer { Refnr = c.Refnr,FirstName = c.FirstName, LastName = c.LastName, Street = c.Street, ZipCode=c.ZipCode, City=c.City,
                                       HomePhone=c.HomePhone,WorkPhone=c.WorkPhone,PersonalEmail=c.PersonalEmail,WorkEmail=c.WorkEmail,RentedTitles=c.RentedTitles}).ToList();
                dataGridViewCustomers.ItemsSource = null;
                dataGridViewCustomers.Items.Clear();
                lstResultNames = lstResult2.ToList<Customer>();
                dataGridViewCustomers.ItemsSource = lstResultNames;
                dataGridViewCustomers.DataContext = lstResultNames;
                //lblStatus.Content = "Antal träffar :" + lstResultNames.Count.ToString();
            }


            //Inform the user that either part of a title or a specific genre must be entered
            else
            {
                //Inform the user
                MessageBox.Show("You must either enter a part of a title or choose specific a genre.", "Error!", MessageBoxButton.OK);
            }
            return;
        }


        /// <summary>
        /// property for SearchTitle
        /// </summary>
        public string SearchTitle
        {
            get { return _SearchTitle; }
            set
            {
                if (value != _SearchTitle)
                {
                    _SearchTitle = value;
                    //OnPropertyChanged("SearchTitle");
                }
            }
        }

        /// <summary>
        /// property for SearchName
        /// </summary>
        public string SearchName
        {
            get { return _SearchName; }
            set
            {
                if (value != _SearchName)
                {
                    _SearchName = value;
                    //OnPropertyChanged("SearchTitle");
                }
            }
        }

        /// <summary>
        /// property for lstClassObservable
        /// </summary
        private ObservableCollection<DvdList> proplstClassObservable       
        {
            get { return _lstClassObservable; }
            set
            {
                if (value != _lstClassObservable)
                {
                    _lstClassObservable = value;
                    OnPropertyChanged("lstClassObservable");
                }
            }
        }

        /// <summary>
        /// property for CustomerObservable
        /// </summary
        private ObservableCollection<Customer> propCustomerObservable
        {
            get { return _customerObservable; }
            set
            {
                if (value != _customerObservable)
                {
                    _customerObservable = value;
                    OnPropertyChanged("customerObservable");
                }
            }

        }

        /// <summary>
        /// property for GenreStrings
        /// </summary>
        public string[] propGenreStrings 
        {
            get { return GenreStrings; }
            set
            {
            if (value != _GenreStrings)
                {
                    _GenreStrings = value;
                    OnPropertyChanged("GenreStrings");
                }
            }
        }



        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        /// <summary>
        /// This event is not interesting but the desinger will not let me delete it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        /// <summary>
        /// Initializes the combobox with genres
        /// </summary>
        private void InitializeGenreCombobox()
        {
            cmbGenre.Items.Clear();
            
            for (int index = 0; index < GenreStrings.Length; index++)
            {
                //1 will be converted to /
                //_ will be converted to space

                GenreStrings[index] = GenreStrings[index].Replace('1', '/');
                GenreStrings[index] = GenreStrings[index].Replace('_', ' ');
                cmbGenre.Items.Add(GenreStrings[index]);
            }
        }

        /// <summary>
        /// This event is not interesting but the desinger will not let me delete it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        /// <summary>
        /// Handles the btnSearchTitle_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearchTitle_Click(object sender, RoutedEventArgs e)
        {
            LinqQuestionsTitles();
            return;
        }


        /// <summary>
        /// Catches the event when a row has been edited (after the user has switched rows) 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewCustomers_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            var _dl = e.Row.Item as DvdList;
            if (e.Row.IsNewItem) //Is it a new row ?
            {
                lstClassObservable[lstClassObservable.Count - 1].Refnr = lstClassObservable.Count;
                //_dl.Refnr = 0;
            }
            else //Check if any fields have been changed
            {
                lstClassObservable[_dl.Refnr - 1].Refnr = _dl.Refnr;
                lstClassObservable[_dl.Refnr - 1].Title = _dl.Title;
                lstClassObservable[_dl.Refnr - 1].Price = _dl.Price;
                lstClassObservable[_dl.Refnr - 1].Year = _dl.Year;
                lstClassObservable[_dl.Refnr - 1].Genre = _dl.Genre;
                lstClassObservable[_dl.Refnr - 1].Copies_Available = _dl.Copies_Available;
                lstClassObservable[_dl.Refnr - 1].Rented_Out = _dl.Rented_Out;
            }            
        }

        /// <summary>
        /// Not an interesting event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewCustomers_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
        }

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    LinqQuestionsCustomers();
        //}

        //private void Button_Click_1(object sender, RoutedEventArgs e)
        //{
        //    LinqQuestionsCustomers();
        //}

        private void btnSearchCustomer_Click(object sender, RoutedEventArgs e)
        {
            LinqQuestionsCustomers();
        }

        /// <summary>
        /// Handles the Rent title button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRentMovie_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridViewTitles.SelectedItem == null || dataGridViewCustomers.SelectedItem == null)
                return;

            DvdList d = (DvdList)dataGridViewTitles.SelectedItem;
            Customer c = (Customer)dataGridViewCustomers.SelectedItem;
            if (d == null || c == null)
                return;
            else
            {
                
                if (d.Copies_Available > 0)
                {
                    if (c == null) //No row chosen in the Customer Datagrid
                    {
                        MessageBox.Show("You must select a customer to rent a title.", "Error!", MessageBoxButton.OK);
                        return;
                    }
                    if(c.LastName.Equals(string.Empty) || d.Title.Equals(string.Empty))
                    {
                        MessageBox.Show("The title and the renter must be a valid renter and both" + Environment.NewLine +
                                        "DVD-title and last name of the renter must not be empty", "Error!", MessageBoxButton.OK);
                        return;
                    }

                    lstClassObservable[d.Refnr - 1].Rented_By = c.Refnr;
                    lstClassObservable[d.Refnr - 1].Copies_Available--;
                    lstClassObservable[d.Refnr - 1].Rented_Out++;
                    CustomerObservable[c.Refnr - 1].RentedTitles++;
                    dataGridViewTitles.Items.Refresh();
                    dataGridViewCustomers.Items.Refresh();
                }
 
            }
        }

        /// <summary>
        /// Handles the Return title button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReturnMovie_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridViewTitles.SelectedItem == null || dataGridViewCustomers.SelectedItem == null)
                return;

            DvdList d = (DvdList)dataGridViewTitles.SelectedItem;
            Customer c = (Customer)dataGridViewCustomers.SelectedItem;
            if (d == null || c == null)
                return;
            else
            {
                if (d.Rented_Out > 0)
                {
                    if (c == null) //No row chosen in the Customer Datagrid
                    {
                        MessageBox.Show("You must select a customer to return a movie.", "Error!", MessageBoxButton.OK);
                        return;
                    }

                    if (c.RentedTitles == 0) //No row chosen in the Customer Datagrid
                    {
                        MessageBox.Show("The customer des not have any rented titles for the moment.", "Error!", MessageBoxButton.OK);
                        return;
                    }

                    lstClassObservable[d.Refnr - 1].Rented_By = 0;
                    lstClassObservable[d.Refnr - 1].Copies_Available++;
                    lstClassObservable[d.Refnr - 1].Rented_Out--;                    
                    CustomerObservable[c.Refnr - 1].RentedTitles--;
                    dataGridViewTitles.Items.Refresh();
                    dataGridViewCustomers.Items.Refresh();
                }
                else
                {
                    MessageBox.Show("This movie has not been rented out", "Error!", MessageBoxButton.OK);
                    return;
                }


            }
        }

        /// <summary>
        /// uninteresting event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewTitles_LoadingRow(object sender, DataGridRowEventArgs e)
        {

        }

        /// <summary>
        /// This event fires when the datagridview is loading where makes sure that on 1 instead of 0 (more user-friendly)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewTitles_LoadingRow_1(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString(); 
        }

        /// <summary>
        /// This event fires when the datagridview is loading where makes sure that on 1 instead of 0 (more user-friendly)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewCustomers_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString(); 
        }

        /// <summary>
        /// Handles the event when the user is editing the row in order to set an internal row number (not the same as the row number in the header!)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewCustomers_RowEditEnding_1(object sender, DataGridRowEditEndingEventArgs e)
        {
            var _dl = e.Row.Item as Customer;
            if (e.Row.IsNewItem) //Is it a new row ?
            {
                CustomerObservable[CustomerObservable.Count - 1].Refnr = CustomerObservable.Count;
            }

        }

        /// <summary>
        /// Handles the Search Leased Movies - show only leased movies
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearchLeasedMovies_Click(object sender, RoutedEventArgs e)
        {
            ShowOnlyLeasedMovies = true;
            LinqQuestionsTitles();
        }
    }
}
