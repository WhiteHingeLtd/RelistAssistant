using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        public SkuCollection MainColl;
        public MainWindow()
        {
            InitializeComponent();
            var loader = new GenericDataController();
            MainColl = loader.SmartSkuCollLoad(true);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Misc.OperationDialog("Generating CSV", GenerateCsv); 
        }

        private void GenerateCsv(object sender, DoWorkEventArgs e)
        {
            var dataBag = new ConcurrentBag<WhlSKU>();
            var datalist = new List<WhlSKU>();
            Parallel.ForEach(MainColl.MakeMixdown(), sku =>
                {
                    try
                    {
                        if (sku.SalesData.CombinedWeekly < 3) return;
                        if (sku.GetLocation(SKULocation.SKULocationType.Pickable).WarehouseID == 2) dataBag.Add(sku);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(sku.ShortSku);
                    }
                    
                }
            );
            Console.WriteLine("Finished Parallel");
            Console.WriteLine(dataBag.Count);
            datalist.AddRange(dataBag);
            datalist.Sort((x,y) => y.SalesData.CombinedWeekly.CompareTo(x.SalesData.CombinedWeekly));
            var csvbuilder = "Sku,Location,Title,Sales,Stock,Check" + Environment.NewLine;

            foreach (var sku in datalist)
            {
                csvbuilder += sku.ShortSku + "," + sku.GetLocation(SKULocation.SKULocationType.Pickable).LocationText +","+sku.Title.Label + "," + sku.SalesData.CombinedWeekly.ToString() + $",{sku.Stock.Total} ," + Environment.NewLine;
            }

            try
            {
               File.WriteAllText(@"X:\Reporting\Unit1BySales.csv", csvbuilder);
            }
            catch (IOException exception)
            {
                File.WriteAllText(@"X:\Reporting\Unit1BySales2.csv", csvbuilder);
                Console.WriteLine(exception);
            }
            Console.WriteLine("Finished");
            //Console.Write(csvbuilder);
        }
    }
}
