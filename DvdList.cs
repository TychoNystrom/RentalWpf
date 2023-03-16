/// <summary> 
/// DvdList.cs
/// The DvdList class is used to contain data about the titles
/// Project Assignment 7 (VG)
/// Created by: Tycho Nyström (AC9320) 2014-01-13
/// </summary>
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentalWpf
{
    class DvdList : INotifyPropertyChanged
    {
        private int DVD_Refnr = 0;
        private string DVD_Title = string.Empty;
        private double DVD_Price = 0.0;
        private int DVD_Year = 0;
        private string DVD_Genre = string.Empty;
        private int DVD_Copies_Available = 0;
        private int DVD_Rented_Out = 0;
        private int DVD_Rented_By = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        public DvdList()
        {
            //this.PropertyChanged += this.DvdListPropertyChanged;
        }

        /// <summary>
        /// Property related to the field m_Refnr
        /// Both read and write access
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public int Refnr
        {
            get { return DVD_Refnr; }
            set { DVD_Refnr = value; OnPropertyChanged("Refnr"); }
        }

        /// <summary>
        /// Property related to the field m_Title
        /// Both read and write access
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Title
        {
            get { return DVD_Title; }
            set { DVD_Title = value; OnPropertyChanged("Title"); }
        }

        /// <summary>
        /// Property related to the field m_Price
        /// Both read and write access
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public double Price
        {
            get { return DVD_Price; }
            set { DVD_Price = value; OnPropertyChanged("Price"); }
        }

        /// <summary>
        /// Property related to the field m_year
        /// Both read and write access
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public int Year
        {
            get { return DVD_Year; }
            set { DVD_Year = value; OnPropertyChanged("Year"); }
        }

        /// <summary>
        /// Property related to the field m_Genre
        /// Both read and write access
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Genre
        {
            get { return DVD_Genre; }
            set { DVD_Genre = value; OnPropertyChanged("Genre"); }
        }

        /// <summary>
        /// Property related to the field m_Copies_Available
        /// Both read and write access
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public int Copies_Available
        {
            get { return DVD_Copies_Available; }
            set { DVD_Copies_Available = value; OnPropertyChanged("Copies_Available"); }
        }

        /// <summary>
        /// Property related to the field m_Rented_Out
        /// Both read and write access
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public int Rented_Out
        {
            get { return DVD_Rented_Out; }
            set { DVD_Rented_Out = value; OnPropertyChanged("Rented_Out"); }
        }

        /// <summary>
        /// Property related to the field m_Rented_By
        /// Both read and write access
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public int Rented_By
        {
            get { return DVD_Rented_By; }
            set { DVD_Rented_By = value; OnPropertyChanged("Rented_By"); }
        }

        /// <summary>
        /// This function is not interesting
        /// </summary>
        public void StartListening()
        {
            this.PropertyChanged += this.DvdListPropertyChanged;
        }


        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        /// <summary>
        /// This event is not interesting 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DvdListPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //string IsSelected = "IsSelected";

            // Make sure that the property name we're referencing is valid.
            // This is a debugging technique, and does not execute in a Release build.


            // When a customer is selected or unselected, we must let the
            // world know that the TotalSelectedSales property has changed,
            // so that it will be queried again for a new value.
            //if (e.PropertyName == IsSelected)
            //if()
            if (e.PropertyName.Equals("Title")            || 
                e.PropertyName.Equals("Price")            ||
                e.PropertyName.Equals("Year")             ||
                e.PropertyName.Equals("Genre")            ||
                e.PropertyName.Equals("Copies_Available") ||
                e.PropertyName.Equals("Rented_Out"))
                    this.OnPropertyChanged(e.PropertyName);
        }
    }
}
