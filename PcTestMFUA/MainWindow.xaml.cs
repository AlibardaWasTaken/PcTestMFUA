using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenHardwareMonitor.Hardware;
using System.Threading;
using System.Diagnostics;

namespace PcTestMFUA
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

       public UpdateVisitor updateVisitor = new UpdateVisitor();
        public Computer computer = new Computer();

        public int cpuUsage = 85;  // Процент использования процессора при стресс тесте
        public List<Thread> threads = new List<Thread>(); //  Лист потоков для стресс тестирования


        public MainWindow()
        {
            InitializeComponent();

            computer.Open();
            computer.GPUEnabled = true;
            computer.CPUEnabled = true;
            computer.RAMEnabled = true;
           
            
            GetSystemCpuInfo();
            GetSystemGpuInfo();
            GetSystemMemInfo();

            DispatcherTimer RefreshTimer = new DispatcherTimer(); // таймер для обновления информации
            RefreshTimer.Interval = TimeSpan.FromSeconds(1.0); // 1 секунда
            RefreshTimer.Tick += new EventHandler(timer_Tick);
            RefreshTimer.Start();

   


        }

        private void timer_Tick(object sender, EventArgs e) // обновление данных таймером
        {
           

           computer.Accept(updateVisitor); // Обновление информации
            GetSystemCpuInfo();
            GetSystemGpuInfo();
            GetSystemMemInfo();
            
        }


        public void GetSystemCpuInfo()// Температура Процессора
        {

            CpuInfo.Text = "";
            if (computer.Hardware.Length > 0) 
            {
                for (int i = 0; i < computer.Hardware.Length; i++)
                {
                    if (computer.Hardware[i].HardwareType == HardwareType.CPU)
                    {
                        for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                        {
                            if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature)
                                CpuInfo.Text += computer.Hardware[i].Sensors[j].Name + " : " + computer.Hardware[i].Sensors[j].Value.ToString() + " °C" + "\r";
                        }
                    }
                }
            }
              
           
        }


        public void GetSystemGpuInfo() // Температура Видеокарты
        {

            GpuInfo.Text = "";
            if (computer.Hardware.Length > 0)
            {
                for (int i = 0; i < computer.Hardware.Length; i++)
                {

                        if (computer.Hardware[i].HardwareType == HardwareType.GpuNvidia || computer.Hardware[i].HardwareType == HardwareType.GpuAti)
                        {
                        for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                        {
                            if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature)
                                GpuInfo.Text += computer.Hardware[i].Sensors[j].Name + " : " + computer.Hardware[i].Sensors[j].Value.ToString() + " °C" + "\r";

                        }
                        }
                    }



                }
            }
        


        public void GetSystemMemInfo()// Информация о памяти
        {
 
            MemInfo.Text = "";
            if (computer.Hardware.Length > 0)
            {
                for (int i = 0; i < computer.Hardware.Length; i++)
                {
                    if (computer.Hardware[i].HardwareType == HardwareType.RAM)
                    {
                        for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                        {
                           if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load)
                                MemInfo.Text += "% Загрузки памяти" + " : " + string.Format("{0:0.00}", computer.Hardware[i].Sensors[j].Value) + "%" + "\r";

                            if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Data)
                            {
                                if(computer.Hardware[i].Sensors[j].Name == "Used Memory")
                                { 
                                    MemInfo.Text += "Используемая память" + " : " + string.Format("{0:0.00}", computer.Hardware[i].Sensors[j].Value) + " ГБ" + "\r";
                                }
                                else
                                {
                                    MemInfo.Text += "Свободная память" + " : " + string.Format("{0:0.00}", computer.Hardware[i].Sensors[j].Value) + " ГБ" + "\r";
                                }
                            }                               
                                
                        }
                    }
                }
            }

          
        }




        public static void CPUKTesting(object cpuUsage) // цикл для нагрузки процессора
        {
            Parallel.For(0, 1, new Action<int>((int i) =>
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                while (true)
                {
                    if (watch.ElapsedMilliseconds > (int)cpuUsage)
                    {
                        Thread.Sleep(100 - (int)cpuUsage);
                        watch.Reset();
                        watch.Start();
                    }
                }
               
            }));

        }






        public class UpdateVisitor : IVisitor  //Обновление информации для работы методов
        {
            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }

            public void VisitHardware(IHardware hardware)// Обновление запчастей пк
            {
                hardware.Update(); 
                foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
            }

            public void VisitSensor(ISensor sensor) { }
            public void VisitParameter(IParameter parameter) { }
        }

        private void CPUcheckBox_Checked(object sender, RoutedEventArgs e)
        {


            threads.Clear(); // Очистка списка
            
            for (int i = 0; i < Environment.ProcessorCount + 1; i++)
            {
                Thread t = new Thread(new ParameterizedThreadStart(CPUKTesting));
                t.Start(cpuUsage);
                threads.Add(t);
            }
            
  
        }

        private void CPUcheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 1; i++) // Остановка циклов
            {
                foreach (Thread t in threads)
                {
                    t.Abort();
                }
            }



        }

   
    }



 
}
