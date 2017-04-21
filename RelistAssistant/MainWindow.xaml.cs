using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
            GenericDataController Loader = new GenericDataController();
            MainColl = Loader.SmartSkuCollLoad(true);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Misc.OperationDialog("Generating CSV", GenerateCsv); 
        }

        private void GenerateCsv(object sender, DoWorkEventArgs e)
        {
            ConcurrentBag<WhlSKU> DataBag = new ConcurrentBag<WhlSKU>();
            List<WhlSKU> Datalist = new List<WhlSKU>();
            Stopwatch newwatch = new Stopwatch();
            newwatch.Reset();
            newwatch.Start();
            Parallel.ForEach(MainColl, sku =>
                {
                    Console.WriteLine(DataBag.Count);
                    if (sku.Stock.Minimum < sku.Stock.Level) return;
                    try
                    {
                        if (sku.EnvelopeObject.Name.Contains("No list")) return;
                    }
                    catch (Exception exception)
                    {
                        
                    }

                    if (sku.Stock.Minimum < 3) return;
                    if (sku.SKU.Contains("xxxx")) return;
                    DataBag.Add(sku);
                    //if (sku.PackSize == 1)
                    //{

                    //    if (sku.Stock.Minimum < 3) return;
                    //    if (sku.Stock.Level != 0) return;
                    //    DataBag.Add(sku);

                    //}
                    //else
                    //{
                    //    if (!(sku.Stock.Minimum > sku.Stock.Level)) return;
                    //    if (sku.SKU.Contains("xxxx")) return;
                    //    DataBag.Add(sku);
                    //}
                }
            );
            Console.WriteLine(newwatch.ElapsedMilliseconds.ToString());
            Console.WriteLine("Finished Parallel");
            Console.WriteLine(DataBag.Count);
            Datalist.AddRange(DataBag);
            Datalist.Sort((x,y) => x.GetLocation(SKULocation.SKULocationType.Pickable).LocationID.CompareTo(y.GetLocation(SKULocation.SKULocationType.Pickable).LocationID));
            var csvbuilder = "Sku,Location,Title,Level,Minimum,Stock,Check" + Environment.NewLine;
            foreach (WhlSKU sku in Datalist)
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
