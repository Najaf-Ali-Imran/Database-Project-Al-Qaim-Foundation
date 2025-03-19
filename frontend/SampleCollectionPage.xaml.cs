using System;
using System.Collections.Generic;
using System.Windows;

namespace AIQaimFoundation
{
    /// <summary>
    /// Interaction logic for SampleCollectionPage.xaml
    /// </summary>
    public partial class SampleCollectionPage : Window
    {
        public SampleCollectionPage()
        {
            InitializeComponent();
        }
    }

    public class SampleData
    {
        public required string Id { get; set; }
        public required string PatientName { get; set; }
        public required string SampleType { get; set; }
        public required string CollectionDate { get; set; }
        public required string Status { get; set; }
    }
}
