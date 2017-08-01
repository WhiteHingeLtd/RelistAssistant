using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using WHLClasses;

namespace RelistAssistant
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SkuCollection MainColl = new SkuCollection();
        public MainWindow()
        {
            InitializeComponent();
            var Loader = new GenericDataController();
            MainColl = Loader.SmartSkuCollLoad(true);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Misc.OperationDialog("Generating CSV", GenerateCsv); 
        }

        private void GenerateCsv(object sender, DoWorkEventArgs e)
        {
            var DataBag = new ConcurrentBag<WhlSKU>();
            var Datalist = new List<WhlSKU>();
            var newwatch = new Stopwatch();
            newwatch.Reset();
            newwatch.Start();
            Parallel.ForEach(MainColl, sku =>
                {
                    Console.WriteLine(DataBag.Count);
                    if (sku.Stock.Minimum < sku.Stock.Level) return;
                    if (!sku.NewItem.IsListed) return;
                    if (sku.NewItem.Status == "Dead") return;
                    try
                    {
                        if (sku.EnvelopeObject.Name.Contains("Select")) return;
                    }
                    catch (Exception){}

                    if (sku.Stock.Minimum < 3) return;
                    if (sku.SKU.Contains("xxxx")) return;
                    try
                    {
                        sku.RefreshLocations();
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                        WHLClasses.Reporting.ErrorReporting.ReportException(exception);
                    }
                    if(sku.Locations.Count != 0) DataBag.Add(sku);

                }
            );
            Console.WriteLine(newwatch.ElapsedMilliseconds.ToString());
            Console.WriteLine("Finished Parallel");
            Console.WriteLine(DataBag.Count);
            Datalist.AddRange(DataBag);
            Datalist.Sort((x,y) => x.GetLocation(SKULocation.SKULocationType.Pickable).LocationID.CompareTo(y.GetLocation(SKULocation.SKULocationType.Pickable).LocationID));
            var csvbuilder = "Sku,Location,Title,Level,Minimum,Stock,Check" + Environment.NewLine;
            foreach (var sku in Datalist)
            {
                csvbuilder += sku.SKU + "," + sku.GetLocation(SKULocation.SKULocationType.Pickable).LocationText +","+sku.Title.Label + "," + sku.Stock.Level.ToString() + "," + sku.Stock.Minimum.ToString() + "," + sku.Stock.Total.ToString() + "," + Environment.NewLine;
            }
            try
            {
               File.WriteAllText(@"X:\Relisting\NewList.csv", csvbuilder);
            }
            catch (IOException exception)
            {
                File.WriteAllText(@"X:\Relisting\NewList1.csv", csvbuilder);
                Console.WriteLine(exception);
            }
            newwatch.Stop();
            Console.WriteLine(newwatch.ElapsedMilliseconds.ToString());
            Console.WriteLine("Finished");
            //Console.Write(csvbuilder);
        }
    }
}
